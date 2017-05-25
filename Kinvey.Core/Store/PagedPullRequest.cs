// Copyright (c) 2016, Kinvey, Inc. All rights reserved.
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
	public class PagedPullRequest<T> : ReadRequest<T, PullDataStoreResponse<T>>
	{
		BlockingCollection<List<T>> workQueue = new BlockingCollection<List<T>>(10);

		public PagedPullRequest(AbstractClient client, string collection, ICache<T> cache, bool deltaSetFetchingEnabled, IQueryable<object> query)
			: base(client, collection, cache, query, ReadPolicy.FORCE_NETWORK, deltaSetFetchingEnabled)
		{

		}

		public override async Task<PullDataStoreResponse<T>> ExecuteAsync()
		{
			int skipCount = 0, pageSize = 10000;

			var count = await new GetCountRequest<T>(this.Client, this.Collection, this.Cache, ReadPolicy.FORCE_NETWORK, false, null, this.Query).ExecuteAsync();

			Task consumer = Task.Run(() => FlushWorkQueue());

			var pageQueue = new List<Task<List<T>>>();
			do
			{
				var skipTakeQuery = this.Query.Skip(skipCount).Take(pageSize);
				pageQueue.Add(new FindRequest<T>(Client, Collection, Cache, ReadPolicy.FORCE_NETWORK, false, null, skipTakeQuery, null).ExecuteAsync());
				skipCount += pageSize;
			} while (skipCount < count);


			while (pageQueue.Count > 0) {
				Debug.WriteLine("Pagequeue size: " + pageQueue.Count);
				var page = await Task.WhenAny(pageQueue);
				pageQueue.Remove(page);
				try
				{
					workQueue.Add(await page);
				}
				catch (OperationCanceledException e)
				{
					//log 
				}
				catch (Exception e) 
				{ 
					//return all exceptions once done
				}
			}
			workQueue.CompleteAdding();
			await consumer;
			return new PullDataStoreResponse<T>();
		}

		private void FlushWorkQueue()
		{

			while (true)
			{
				try
				{
					List<T> items = workQueue.Take();
					Cache.RefreshCache(items);
					Debug.WriteLine(string.Format("Processing {0} items, workQueue size = {1}", items.Count, workQueue.Count));
				}
				catch (InvalidOperationException e)
				{
					Debug.WriteLine(string.Format("Work queue has been closed."));
					break;
				}
			}
		}

		public override Task<bool> Cancel()
		{
			throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on PullRequest not implemented.");
		}
	}
}
