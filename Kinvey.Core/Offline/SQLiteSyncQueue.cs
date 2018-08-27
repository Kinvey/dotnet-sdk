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
using SQLite.Net;
using System.Threading.Tasks;


namespace Kinvey
{
	public class SQLiteSyncQueue : ISyncQueue
	{
		public string Collection { get;}
		private SQLiteConnection dbConnection;

		public SQLiteSyncQueue (string collection, SQLiteConnection connection)
		{
			this.dbConnection = connection;
			this.Collection = collection;
		}

        public int Enqueue(PendingWriteAction pending)
        {
            lock (dbConnection)
            {
                // Check if a sync queue entry for this entity already exists
                PendingWriteAction existingSyncItem = GetByID(pending.entityId);
                if (existingSyncItem != null)
                {
                    if (existingSyncItem.action == Constants.STR_REST_METHOD_PUT &&
                        pending.action == Constants.STR_REST_METHOD_PUT)
                    {
                        // If both the existing and pending actions are PUT, this means either this is an already created
                        // item, or the item has been created with a custom ID on the client.  In either case, the existing
                        // entry will capture the state of the entity.
                        return 0;
                    }
                    else if (existingSyncItem.action == Constants.STR_REST_METHOD_POST &&
                             pending.action == Constants.STR_REST_METHOD_PUT)
                    {
                        // Do not enqueue in the case of an existing POST, since the POST
                        // entry will already capture the current state of the entity.
                        return 0;
                    }
                    else if (existingSyncItem.action == Constants.STR_REST_METHOD_PUT &&
                             pending.action == Constants.STR_REST_METHOD_POST)
                    {
                        // highly unlikely, but favor the POST
                        this.Remove(existingSyncItem);
                    }
                    else if (existingSyncItem.action == Constants.STR_REST_METHOD_POST &&
                             pending.action == Constants.STR_REST_METHOD_POST)
                    {
                        // Should be imposssible to have this situation, favor the
                        // existing POST by not enqueueing
                        return 0;
                    }
                    else if (existingSyncItem.action == Constants.STR_REST_METHOD_DELETE &&
                             (pending.action == Constants.STR_REST_METHOD_PUT || pending.action == Constants.STR_REST_METHOD_POST))
                    {
                        // odd case where an object has somehow been created/updated
                        // after a delete call, but favor the create/update
                        this.Remove(existingSyncItem);
                    }
                    else if (pending.action == Constants.STR_REST_METHOD_DELETE)
                    {
                        // no matter what, favor the current deletion
                        this.Remove(existingSyncItem);

                        // If the existing item that is being deleted is something that only existed locally,
                        // do not insert the DELETE action into the queue, since it is local-only.
                        // Note that this cannot be optimized for the case when a custom ID has been set on
                        // the entity.
                        if (existingSyncItem.entityId.StartsWith("temp_", StringComparison.OrdinalIgnoreCase))
                        {
                            return 0;
                        }
                    }
                }

                return dbConnection.Insert(pending);
            }
        }

		public List<PendingWriteAction> GetAll()
		{
            lock (dbConnection)
            {
                List<PendingWriteAction> listPWA = new List<PendingWriteAction>();

                var filter = dbConnection.Table<PendingWriteAction>().Where(blah => blah.collection == this.Collection);
                foreach (PendingWriteAction pwa in filter)
                {
                    listPWA.Add(pwa);
                }

                return listPWA;
            }
		}

		public List<PendingWriteAction> GetFirstN(int limit, int offset)
		{
            lock (dbConnection)
            {
                string query = $"SELECT * FROM PendingWriteAction WHERE collection == \"{this.Collection}\" LIMIT {limit} OFFSET {offset}";
                return dbConnection.Query<PendingWriteAction>(query);
            }
		}

		public PendingWriteAction GetByID(string entityId) {
            lock (dbConnection)
            {
                return dbConnection.Table<PendingWriteAction>()
                    .Where(t => t.collection == this.Collection && t.entityId == entityId)
                    .FirstOrDefault();
            }
		}

		public  PendingWriteAction Peek () {
            lock (dbConnection)
            {
                return dbConnection.Table<PendingWriteAction>()
                    .Where(t => t.collection == this.Collection)
                    .OrderByDescending(u => u.key)
                    .FirstOrDefault();
            }
		}

		public PendingWriteAction Pop () {
            lock (dbConnection)
            {
                try
                {
                    PendingWriteAction item = Peek();
                    dbConnection.Delete<PendingWriteAction>(item.key);
                    return item;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
		}

		public int Count(bool allCollections)
		{
            lock (dbConnection)
            {
                if (allCollections)
                {
                    return dbConnection.Table<PendingWriteAction>()
                                       .Count();
                }

                return dbConnection.Table<PendingWriteAction>()
                                   .Where(t => t.collection == this.Collection)
                                   .Count();
            }
		}

		public int Remove(PendingWriteAction pending)
		{
            lock (dbConnection)
            {
                return dbConnection.Delete(pending);
            }
		}

		public int Remove(IEnumerable<PendingWriteAction> pendings) {
            lock (dbConnection)
            {
                if (pendings == null)
                {
                    return RemoveAll();
                }

                int ret = 0;
                foreach (var pending in pendings)
                {
                    ret += this.Remove(pending);
                }
                return ret;
            }
		}

		public int RemoveAll () {
            lock (dbConnection)
            {
                return dbConnection.DeleteAll<PendingWriteAction>();
            }
		}
	}
}