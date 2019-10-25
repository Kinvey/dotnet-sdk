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

using System.Threading.Tasks;
using System.Linq;

namespace Kinvey
{
    /// <summary>
    /// Request operation for pulling all records for a collection during a sync, and refreshing the cache with the
    /// updated data.
    /// </summary>
    /// <typeparam name="T">The type of an entity.</typeparam>
    public class PullRequest<T> : ReadRequest<T, PullDataStoreResponse<T>>
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequest{T}"/> class.
        /// </summary>
        /// <param name="client">Client that the user is logged in.</param>
        /// <param name="collection">Collection name.</param>
        /// <param name="cache">Cache.</param>
        /// <param name="deltaSetFetchingEnabled">If set to <c>true</c> delta set fetching enabled.</param>
        /// <param name="query">Query.</param>
        public PullRequest(AbstractClient client, string collection, ICache<T> cache, bool deltaSetFetchingEnabled, IQueryable<object> query)
			: base(client, collection, cache, query, ReadPolicy.FORCE_NETWORK, deltaSetFetchingEnabled)
		{
		}

        /// <summary>
        /// Executes the request asynchronously.
        /// </summary>
        /// <returns> The async task with the request result.</returns>
		public override async Task<PullDataStoreResponse<T>> ExecuteAsync()
		{
			var result = await PerformNetworkFind().ConfigureAwait(false);
			return new PullDataStoreResponse<T>(result.TotalCount, result.ResultSet.Count, result.ResultSet);
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
