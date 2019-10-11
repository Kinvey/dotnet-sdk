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

namespace Kinvey
{
    /// <summary>
    /// Interface for cache manager.
    /// </summary>
    public interface ICacheManager : IDisposable
	{
        /// <summary>
        /// Gets or sets the database file path.
        /// </summary>
        /// <value>The database file path.</value>
        string dbpath { get; set;}

        /// <summary>
        /// Gets the cache.
        /// </summary>
        /// <returns>The cache.</returns>
        /// <param name="collectionName">Collection name.</param>
        /// <typeparam name="T">The type of an item.</typeparam>
        ICache<T> GetCache <T>(string collectionName) where T : class, new();

        /// <summary>
        /// Gets the synchronization queue.
        /// </summary>
        /// <returns>The synchronization queue.</returns>
        /// <param name="collectionName">Collection name.</param>
        /// <typeparam name="T">The type of an item.</typeparam>
        ISyncQueue GetSyncQueue (string collectionName);

        /// <summary>
        /// Gets query cache item.
        /// </summary>
        /// <returns>Query cache item.</returns>
        /// <param name="collectionName">Collection name.</param>
        /// <param name="query">Query.</param>
        /// <param name="lastRequestTime">The last request time.</param>
        QueryCacheItem GetQueryCacheItem(string collectionName, string query, string lastRequestTime);

        /// <summary>
        /// Sets query cache item.
        /// </summary>
        /// <returns><c>True</c> if the query cache item was inserted; otherwise, <c>false</c>.</returns>
        /// <param name="item">Query cache item.</param>
        bool SetQueryCacheItem(QueryCacheItem item);

        /// <summary>
        /// Deletes query cache item.
        /// </summary>
        /// <returns><c>True</c> if the query cache item was deleted; otherwise, <c>false</c>.</returns>
        /// <param name="item">Query cache item.</param>
        bool DeleteQueryCacheItem(QueryCacheItem item);

		/// <summary>
		/// Clears the storage.
		/// </summary>
		void clearStorage();
	}
}
