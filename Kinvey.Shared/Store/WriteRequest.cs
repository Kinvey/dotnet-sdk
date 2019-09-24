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

using Newtonsoft.Json.Linq;

namespace Kinvey
{
    /// <summary>
    /// Base class for creating requests to write data.
    /// </summary>
    /// <typeparam name="T">The type of the network request.</typeparam>
    /// <typeparam name="U">The type of the network response.</typeparam>
	public abstract class WriteRequest <T, U> : Request <T, U>
	{
        /// <summary>
        /// Gets the interface for operating with sync queue.
        /// </summary>
        /// <value>The instance implementing <see cref="ISyncQueue{T}" /> interface.</value>
        public ISyncQueue SyncQueue { get;}

        /// <summary>
        /// Gets the interface for operating with data store cache.
        /// </summary>
        /// <value>The instance implementing <see cref="ICache{T}" /> interface.</value>
        public ICache<T> Cache { get; }

        /// <summary>
        /// Gets collection name for the request.
        /// </summary>
        /// <value>String value with collection name.</value>
        public string Collection { get; }

        /// <summary>
        /// Gets write policy for the request.
        /// </summary>
        /// <value><see cref="WritePolicy" /> enum value containing write policy for the request.</value>
        public WritePolicy Policy { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WriteRequest{T,U}"/> class.
        /// </summary>
        /// <param name="client">Client that the user is logged in.</param>
        /// <param name="collection">Collection name.</param>
        /// <param name="cache">Cache.</param>
        /// <param name="queue">Synchronization queue.</param>
        /// <param name="policy">Write policy.</param>
        public WriteRequest(AbstractClient client, string collection, ICache<T> cache, ISyncQueue queue, WritePolicy policy) 
			: base(client)
		{
			this.Collection = collection;
			this.Cache = cache;
			this.SyncQueue = queue;
			this.Policy = policy;
		}

        /// <summary>
        /// Populates the entity by temporary id.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <value>Temporary id.</value>
        protected string PrepareCacheSave(ref T entity)
        {
            string guid = System.Guid.NewGuid().ToString();
            string tempID = "temp_" + guid;

            JObject obj = JObject.FromObject(entity);
            obj["_id"] = tempID;
            entity = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(obj.ToString());

            return tempID;
        }
    }
}
