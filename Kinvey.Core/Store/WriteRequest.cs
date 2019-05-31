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

using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Kinvey
{
	public abstract class WriteRequest <T, U> : Request <T, U>
	{
		public ISyncQueue SyncQueue { get;}
		public ICache<T> Cache { get; } 
		public string Collection { get; }
		public WritePolicy Policy { get; }

		public WriteRequest(AbstractClient client, string collection, ICache<T> cache, ISyncQueue queue, WritePolicy policy) 
			: base(client)
		{
			this.Collection = collection;
			this.Cache = cache;
			this.SyncQueue = queue;
			this.Policy = policy;
		}

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
