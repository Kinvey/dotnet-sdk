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

using SQLite;

namespace Kinvey
{
    /// <summary>
    /// Represents SQLite table for query cache items. 
    /// </summary>
    public class QueryCacheItem
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        /// <value>The key property gets/sets the value of the int field, _key.</value>
        [PrimaryKey]
        public int? key { get; set; }

        /// <summary>
        /// Collection name.
        /// </summary>
        /// <value>The collectionName property gets/sets the value of the string field, _collectionName.</value>
        public string collectionName { get; set; }

        /// <summary>
        /// Query.
        /// </summary>
        /// <value>The query property gets/sets the value of the string field, _query.</value>
        public string query { get; set; }

        /// <summary>
        /// The last request.
        /// </summary>
        /// <value>The lastRequest property gets/sets the value of the string field, _lastRequest.</value>
        public string lastRequest { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryCacheItem"/> class.
        /// </summary>
        public QueryCacheItem() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryCacheItem"/> class.
        /// </summary>
        /// <param name="collectionName"> Collection name. </param>
        /// <param name="query"> Query. </param>
        /// <param name="lastRequest"> The last request. </param>
        public QueryCacheItem(string collectionName, string query, string lastRequest)
        {
            this.collectionName = collectionName;
            this.query = query;
            this.lastRequest = lastRequest;
        }
    }
}
