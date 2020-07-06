// Copyright (c) 2019, Kinvey, Inc. All rights reserved.
//
// This software is licensed to you under the Kinvey terms of service located at
// http://www.kinvey.com/terms-of-use. By downloading, accessing and/or using this
// software, you hereby accept such terms of service  (and any agreement referenced
// therein) and agree that you have read, understand and agree to be bound by such
// terms of service and are of legal age to agree to such terms with Kinvey.
//
// This software contains valuable confidential and proprietary information of
// KINVEY, INC and is subject to applicable licensing agreements.
// Unauthorized reproduction, transmission or distribution of this file and its
// contents is a violation of applicable laws.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Kinvey
{
    /// <summary>
    /// Request built for use by a <see cref="DataStore{T}"/> to get some count of entities from collection.
    /// </summary>
    /// <typeparam name="T">The type of an entity.</typeparam>
	public class PagedPullRequest<T> : ReadRequest<T, PullDataStoreResponse<T>>
	{
		BlockingCollection<List<T>> workQueue = new BlockingCollection<List<T>>(10);

		int count;
		bool isInitial;
		bool isConsumerWorking = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedPullRequest{T}"/> class.
        /// </summary>
        /// <param name="client">Client that the user is logged in.</param>
        /// <param name="collection">Collection name.</param>
        /// <param name="cache">Cache.</param>
        /// <param name="deltaSetFetchingEnabled">If set to <c>true</c> delta set fetching enabled.</param>
        /// <param name="query">Query.</param>
        /// <param name="count">Limit of entities.</param>
        /// <param name="isInitial">If <c>true</c> then entities received from backend are expected to be not existing in Cache, otherwise <c>false</c>.</param>
        public PagedPullRequest(AbstractClient client, string collection, ICache<T> cache, bool deltaSetFetchingEnabled, IQueryable<object> query, int count, bool isInitial)
			: base(client, collection, cache, query, ReadPolicy.FORCE_NETWORK, deltaSetFetchingEnabled)
		{
			this.count = count;
			this.isInitial = isInitial;
		}

        /// <summary>
        /// Executes the request asynchronously.
        /// </summary>
        /// <returns> The async task with the request result.</returns>
		public override async Task<PullDataStoreResponse<T>> ExecuteAsync()
		{
			int skipCount = 0, pageSize = 10000;

			if (count < 0) {
				count = (int) await new GetCountRequest<T>(this.Client, this.Collection, this.Cache, ReadPolicy.FORCE_NETWORK, false, this.Query).ExecuteAsync().ConfigureAwait(false);
			}

			Task consumer = null;
			var pageQueue = new List<Task<List<T>>>();
            IQueryable<object> query = Query ?? Enumerable.Empty<object>().AsQueryable();

            do
			{
				var skipTakeQuery = query.Skip(skipCount).Take(pageSize);
				pageQueue.Add(new FindRequest<T>(Client, Collection, Cache, ReadPolicy.FORCE_NETWORK, false, skipTakeQuery, null).ExecuteAsync());
				skipCount += pageSize;
			} while (skipCount < count);


			while (pageQueue.Count > 0) {
				Debug.WriteLine("Pagequeue size: " + pageQueue.Count);
				var page = await Task.WhenAny(pageQueue).ConfigureAwait(false);
				pageQueue.Remove(page);
				//maxThread.Release();
				workQueue.Add(await page.ConfigureAwait(false));
				if (!isConsumerWorking) {
					consumer = Task.Run(() => ConsumeWorkQueue());
				}
			}
			workQueue.CompleteAdding();
			await consumer.ConfigureAwait(false);
			return new PullDataStoreResponse<T>();
		}

		private void ConsumeWorkQueue()
		{
			isConsumerWorking = true;
			while (true)
			{
				try
				{
					List<T> items = workQueue.Take();
					if (this.isInitial)
					{
						Cache.Save(items);
					}
					else {
						Cache.RefreshCache(items);
					}

					Debug.WriteLine(string.Format("Processing {0} items, workQueue size = {1}", items.Count, workQueue.Count));
				}
				catch (InvalidOperationException)
				{
					Debug.WriteLine(string.Format("Work queue has been closed."));
					break;
				}
			}
			isConsumerWorking = false;
		}

        /// <summary>
        /// Communicates the request for cancellation.
        /// </summary>
        /// <returns>The async task with the boolean result. If the result is <c>true</c> then the request was canceled, otherwise <c>false</c>.</returns>
        public override Task<bool> Cancel()
		{
			throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on PullRequest not implemented.");
		}
	}
}
