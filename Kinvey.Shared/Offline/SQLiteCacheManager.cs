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
using System.IO;
using System.Threading.Tasks;
using SQLite;

namespace Kinvey
{
    /// <summary>
    /// SQLite cache manager.
    /// </summary>
    public class SQLiteCacheManager : ICacheManager
    {

        private class DebugTraceListener
        {
            public void Receive(string message)
            {
                Logger.Log(message);
            }
        }

        //The version of the internal structure of the database.
        private readonly int databaseSchemaVersion = 1;

        // The asynchronous db connection.
        private SQLiteAsyncConnection dbConnectionAsync;

        private static readonly Dictionary<String, List<SQLiteConnection>> SQLiteFiles = new Dictionary<String, List<SQLiteConnection>>();

        // The asynchronous db connection.
        private SQLiteConnection _dbConnectionSync;
        private SQLiteConnection DBConnectionSync
        {
            get
            {
                lock (this)
                {
                    if (_dbConnectionSync == null)
                    {
                        lock (SQLiteFiles)
                        {
                            _dbConnectionSync = new SQLiteConnection(
                                databasePath: dbpath,
                                openFlags: SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex | SQLiteOpenFlags.PrivateCache,
                                storeDateTimeAsTicks: false
                            );
                            List<SQLiteConnection> connections;
                            if (SQLiteFiles.ContainsKey(dbpath))
                            {
                                connections = SQLiteFiles[dbpath];
                            }
                            else
                            {
                                connections = new List<SQLiteConnection>();
                                SQLiteFiles[dbpath] = connections;
                            }
                            connections.Add(_dbConnectionSync);
                        }
                    }

                    return _dbConnectionSync;
                }
            }
        }

        /// <summary>
        /// Gets or sets the database file path.
        /// </summary>
        /// <value>The database file path.</value>
        public string dbpath { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteCacheManager"/> class.
        /// </summary>
        /// <param name="filePath">File path.</param>
        public SQLiteCacheManager(string filePath)
		{
			this.dbpath = Path.Combine (filePath, "kinveyOffline.sqlite");
		}

		private Dictionary<string, object> mapCollectionToCache = new Dictionary<string, object>();

		/// <summary>
		/// Clears the storage.
		/// </summary>
		public void clearStorage()
		{
            lock (DBConnectionSync)
            {
                if (TableExists<CollectionTableMap>(DBConnectionSync))
                {
                    List<CollectionTableMap> collections = DBConnectionSync.Table<CollectionTableMap>().ToList();
                    if (collections != null)
                    {
                        foreach (var collection in collections)
                        {
                            string dropQuery = $"DROP TABLE IF EXISTS {collection.TableName}";
                            DBConnectionSync.Execute(dropQuery);
                            GetSyncQueue(collection.CollectionName).RemoveAll();
                            mapCollectionToCache.Remove(collection.CollectionName);
                        }

                        DBConnectionSync.DeleteAll<CollectionTableMap>();

                        // Remove _QueryCache table
                        if (TableExists<QueryCacheItem>(DBConnectionSync))
                        {
                            DBConnectionSync.DeleteAll<QueryCacheItem>();
                        }
                    }
                }
            }
		}

        /// <summary>
        /// Gets the cache.
        /// </summary>
        /// <returns>The cache.</returns>
        /// <param name="collectionName">Collection name.</param>
        /// <typeparam name="T">The type of an item.</typeparam>
        public ICache<T> GetCache<T>(string collectionName) where T : class, new()
		{
            lock (DBConnectionSync)
            {
                if (!TableExists<CollectionTableMap>(DBConnectionSync))
                {
                    DBConnectionSync.CreateTable<CollectionTableMap>();
                }

                CollectionTableMap ctm = new CollectionTableMap
                {
                    CollectionName = collectionName,
                    TableName = typeof(T).Name
                };

                DBConnectionSync.InsertOrReplace(ctm);

                if (mapCollectionToCache.ContainsKey(collectionName))
                {
                    return mapCollectionToCache[collectionName] as ICache<T>;
                }

                mapCollectionToCache[collectionName] = new SQLiteCache<T>(collectionName, dbConnectionAsync, DBConnectionSync);
                return mapCollectionToCache[collectionName] as ICache<T>;
            }
		}

		/// <summary>
		/// Gets the collection tables.
		/// </summary>
		/// <returns>The collection tables.</returns>
		public List<string> getCollectionTables()
		{
            lock (DBConnectionSync)
            {
                List<SQLTemplates.TableItem> result = DBConnectionSync.Table<SQLTemplates.TableItem>().OrderByDescending(t => t.name).ToList();
                List<string> collections = new List<string>();

                foreach (SQLTemplates.TableItem item in result)
                {
                    collections.Add(item.name);
                }

                return collections;
            }
		}

		/// <summary>
		/// Gets the collection tables asynchronously.
		/// </summary>
		/// <returns>The collection tables.</returns>
		public async Task<List<string>> getCollectionTablesAsync ()
		{
			List<SQLTemplates.TableItem> result = await dbConnectionAsync.Table<SQLTemplates.TableItem> ().OrderByDescending (t => t.name).ToListAsync ();
			List<string> collections = new List<string> ();


			foreach (SQLTemplates.TableItem item in result) {
				collections.Add (item.name);
			}

			return collections;
		}

        /// <summary>
        /// Gets query cache item.
        /// </summary>
        /// <returns>Query cache item.</returns>
        /// <param name="collectionName">Collection name.</param>
        /// <param name="query">Query.</param>
        /// <param name="lastRequest">The last request time.</param>
        public QueryCacheItem GetQueryCacheItem(string collectionName, string query, string lastRequest)
        {
            lock (DBConnectionSync)
            {
                QueryCacheItem result = null;

                if (!TableExists<QueryCacheItem>(DBConnectionSync))
                {
                    DBConnectionSync.CreateTable<QueryCacheItem>();
                }
                else
                {
                    var items = DBConnectionSync.Table<QueryCacheItem>().Where(item => item.collectionName == collectionName && item.query == query);
                    if (items.Count() == 1)
                    {
                        foreach (QueryCacheItem item in items)
                        {
                            result = item;
                        }
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Sets query cache item.
        /// </summary>
        /// <returns><c>True</c> if the query cache item was inserted; otherwise, <c>false</c>.</returns>
        /// <param name="item">Query cache item.</param>
        public bool SetQueryCacheItem(QueryCacheItem item)
        {
            lock (DBConnectionSync)
            {
                bool success = false;

                if (!TableExists<QueryCacheItem>(DBConnectionSync))
                {
                    DBConnectionSync.CreateTable<QueryCacheItem>();
                }

                int result = DBConnectionSync.InsertOrReplace(item);
                if (result != 0)
                {
                    success = true;
                }

                return success;
            }
        }

        /// <summary>
        /// Deletes query cache item.
        /// </summary>
        /// <returns><c>True</c> if the query cache item was deleted; otherwise, <c>false</c>.</returns>
        /// <param name="item">Query cache item.</param>
        public bool DeleteQueryCacheItem(QueryCacheItem item)
        {
            lock (DBConnectionSync)
            {
                bool success = false;

                if (TableExists<QueryCacheItem>(DBConnectionSync))
                {
                    int result = DBConnectionSync.Delete(item);

                    if (result != 0)
                    {
                        success = true;
                    }
                }

                return success;
            }
        }

        /// <summary>
        /// Gets the synchronization queue.
        /// </summary>
        /// <returns>The synchronization queue.</returns>
        /// <param name="collectionName">Collection name.</param>
        /// <typeparam name="T">The type of an item.</typeparam>
        public ISyncQueue GetSyncQueue(string collectionName) {
            lock (DBConnectionSync)
            {
                if (!TableExists<PendingWriteAction>(DBConnectionSync))
                {
                    DBConnectionSync.CreateTable<PendingWriteAction>();
                }

                return new SQLiteSyncQueue(collectionName, DBConnectionSync);
            }
		}

        /// <summary>
        /// Checks existing of a table.
        /// </summary>
        /// <returns><c>True</c> if the table exists; otherwise, <c>false</c>.</returns>
        /// <param name="connection">SQLite connection.</param>
        /// <typeparam name="T">The type of an item.</typeparam>
		public static bool TableExists<T> (SQLiteConnection connection)
		{    
			const string cmdText = "SELECT name FROM sqlite_master WHERE type='table' AND name=?";
			var cmd = connection.CreateCommand (cmdText, typeof(T).Name);
			return cmd.ExecuteScalar<string> () != null;
		}

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        /// <summary>
        /// Performs all object cleanup.
        /// </summary>
        /// <param name="disposing">Indicates whether the method call comes from a Dispose method (its value is true) or from a finalizer (its value is false).</param>
        protected virtual void Dispose(bool disposing)
        {
            lock (this)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // dispose managed state (managed objects).
                    }

                    // free unmanaged resources (unmanaged objects) and override a finalizer below.
                    if (_dbConnectionSync != null)
                    {
                        _dbConnectionSync.Close();
                        lock (SQLiteFiles)
                        {
                            if (SQLiteFiles.TryGetValue(dbpath, out List<SQLiteConnection> connections))
                            {
                                connections.Remove(_dbConnectionSync);
                                if (connections.Count == 0) SQLiteFiles.Remove(dbpath);
                            }
                        }
                        _dbConnectionSync.Dispose();
                    }

                    // set large fields to null.
                    _dbConnectionSync = null;

                    disposedValue = true;
                }
            }
        }

        /// <summary>
        /// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
        /// </summary>
        ~SQLiteCacheManager() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        /// <summary>
        /// Performs all object cleanup, so the garbage collector no longer needs to call the objects Object.Finalize override.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
