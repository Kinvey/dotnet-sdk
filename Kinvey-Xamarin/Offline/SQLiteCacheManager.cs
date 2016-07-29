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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite.Net;
using SQLite.Net.Async;
using SQLite.Net.Interop;

namespace KinveyXamarin
{
	/// <summary>
	/// SQLite cache manager.
	/// </summary>
	public class SQLiteCacheManager : ICacheManager
	{
		//The version of the internal structure of the database.
		private int databaseSchemaVersion = 1;

		// The asynchronous db connection.
		private SQLiteAsyncConnection dbConnectionAsync;

		// The asynchronous db connection.
		private SQLiteConnection dbConnectionSync;

		/// <summary>
		/// Gets or sets the platform.
		/// </summary>
		/// <value>The platform.</value>
		public ISQLitePlatform platform { get; set; }

		/// <summary>
		/// Gets or sets the database file path.
		/// </summary>
		/// <value>The dbpath.</value>
		public string dbpath { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.SQLiteCacheManager"/> class.
		/// </summary>
		public SQLiteCacheManager(ISQLitePlatform platform, string filePath)
		{
			this.platform = platform;
			this.dbpath = Path.Combine (filePath, "kinveyOffline.sqlite");

			new Task( () =>kickOffUpgrade() ).Start();
			//			Task.Run (kickOffUpgrade ());
		}

		private Dictionary<string, object> mapCollectionToCache = new Dictionary<string, object>();

		private int kickOffUpgrade()
		{
			// Get stored version number.  If null, set to the current dbscheme version and save.
			int newVersion = 0;
			SQLTemplates.OfflineVersion ver = getDBSchemaVersion();

			if (ver == null)
			{
				newVersion = databaseSchemaVersion;
			}
			else
			{
				newVersion = ver.currentVersion + 1;
			}

			updateDBSchemaVersion(newVersion);

			return newVersion;
		}

//		private async Task<int> kickOffUpgradeAsync(){
//			//get stored version number, if it's null set it to the current dbscheme version and save it it
//			//call onupgrade with current version number and dbsv.
//			//DatabaseHelper<JObject> handler = getDatabaseHelper<JObject> ();
//			SQLTemplates.OfflineVersion ver = await getConnectionAsync ().Table<SQLTemplates.OfflineVersion> ().FirstOrDefaultAsync ();
//			if (ver == null) {
//				ver = new SQLTemplates.OfflineVersion ();
//				ver.currentVersion = databaseSchemaVersion;
//				await updateDBSchemaVersion (ver.currentVersion);
//			}
//			int newVersion = onUpgrade (ver.currentVersion, databaseSchemaVersion);
//			return newVersion;
//		}

		private SQLiteConnection getConnection()
		{
			//ContractResolver myResolver = new ContractResolver (t => true, Deserialize);
			if (dbConnectionSync == null)
			{
				//var connectionFactory = new Func<SQLiteConnectionWithLock>(()=>new SQLiteConnectionWithLock(platform, new SQLiteConnectionString(this.dbpath, false, null, new KinveyContractResolver())));
				//dbConnection = new SQLiteAsyncConnection (connectionFactory);			
				dbConnectionSync = new SQLiteConnection(platform, dbpath, false, null, null, null, new KinveyContractResolver());
			}

			return dbConnectionSync;
		}

//		private SQLiteAsyncConnection getConnectionAsync(){
//			//ContractResolver myResolver = new ContractResolver (t => true, Deserialize);
//			if (dbConnectionAsync == null) {
//				//var connectionFactory = new Func<SQLiteConnectionWithLock>(()=>new SQLiteConnectionWithLock(platform, new SQLiteConnectionString(this.dbpath, false, null, new KinveyContractResolver())));
//				//dbConnection = new SQLiteAsyncConnection (connectionFactory);			
//				dbConnectionSync = new SQLiteConnection(platform, dbpath, false, null, null, null, new KinveyContractResolver());
//
//			}
//			return dbConnectionAsync;
//		}


		// Gets the DB schema version.
		private SQLTemplates.OfflineVersion getDBSchemaVersion()
		{
			SQLTemplates.OfflineVersion ver = getConnection().Table<SQLTemplates.OfflineVersion>().FirstOrDefault();
			return ver;
		}

//		// Gets the DB schema version asynchronously.
//		private async Task<SQLTemplates.OfflineVersion> getDBSchemaVersionAsync (){
//			SQLTemplates.OfflineVersion ver =  await getConnection ().Table<SQLTemplates.OfflineVersion> ().FirstOrDefaultAsync ();
//			return ver;
//		}

		/// <summary>
		/// Updates the DB schema version.
		/// </summary>
		/// <returns>The DB schema version.</returns>
		/// <param name="newVersion">New version.</param>
		public int updateDBSchemaVersion(int newVersion)
		{
			SQLTemplates.OfflineVersion ver = new SQLTemplates.OfflineVersion();
			ver.currentVersion = newVersion;

			getConnection().Insert(ver);

			return ver.currentVersion;
		}

		/// <summary>
		/// Clears the storage.
		/// </summary>
		public void clearStorage()
		{
			if (TableExists<CollectionTableMap>(dbConnectionSync))
			{
				List<CollectionTableMap> collections = dbConnectionSync.Table<CollectionTableMap>().ToList();
				if (collections != null)
				{
					foreach (var collection in collections)
					{
						string dropQuery = $"DROP TABLE {collection.TableName}";
						dbConnectionSync.Execute(dropQuery);
						GetSyncQueue(collection.CollectionName).RemoveAll();
					}

					dbConnectionSync.DeleteAll<CollectionTableMap>();
				}
			}
		}

		/// <summary>
		/// Gets the database helper.
		/// </summary>
		/// <returns>The database helper.</returns>
		/// <typeparam name="T">The type of entities stored in this collection.</typeparam>
//		public static DatabaseHelper<T> getDatabaseHelper<T>(){
//			return SQLiteHelper<T>.getInstance (platform, dbpath);
//		}

		public ICache<T> GetCache<T>(string collectionName) where T : class
		{
			if (!TableExists<CollectionTableMap>(dbConnectionSync))
			{
				dbConnectionSync.CreateTable<CollectionTableMap>();
			}

			CollectionTableMap ctm = new CollectionTableMap();
			ctm.CollectionName = collectionName;
			ctm.TableName = typeof(T).Name;

			dbConnectionSync.InsertOrReplace(ctm);

			if (mapCollectionToCache.ContainsKey(collectionName))
			{
				return mapCollectionToCache[collectionName] as ICache<T>;
			}

			mapCollectionToCache[collectionName] = new SQLiteCache<T> (collectionName, dbConnectionAsync, dbConnectionSync, platform);
			return mapCollectionToCache[collectionName] as ICache<T>;
		}

		/// <summary>
		/// Gets the collection tables.
		/// </summary>
		/// <returns>The collection tables.</returns>
		public List<string> getCollectionTables()
		{
			List<SQLTemplates.TableItem> result = dbConnectionSync.Table<SQLTemplates.TableItem>().OrderByDescending(t => t.name).ToList();
			List<string> collections = new List<string>();

			foreach (SQLTemplates.TableItem item in result)
			{
				collections.Add(item.name);
			}

			return collections;
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

		public ISyncQueue GetSyncQueue(string collectionName) {
			if (!TableExists<PendingWriteAction>(dbConnectionSync)){
				dbConnectionSync.CreateTable<PendingWriteAction> ();
			}

			return new SQLiteSyncQueue(collectionName, dbConnectionSync);
		}

		public static bool TableExists<T> (SQLiteConnection connection)
		{    
			const string cmdText = "SELECT name FROM sqlite_master WHERE type='table' AND name=?";
			var cmd = connection.CreateCommand (cmdText, typeof(T).Name);
			return cmd.ExecuteScalar<string> () != null;
		}


		/// <summary>
		/// Kinvey contract resolver - this resolver is used to replace the default SQLite resolver,
		/// so that any class that can be serialized / deserialized as a JSON string can be stored in SQL
		/// </summary>
		class KinveyContractResolver:ContractResolver{

			public KinveyContractResolver () : base(t =>  true, Deserialize){
				
			}

			public static object Deserialize(Type t, object [] obj){
				//if (t == typeof(ISerializable<string>)) {
				if (t.GetTypeInfo().ImplementedInterfaces.Contains(typeof (ISerializable<string>)) &&
				    obj != null &&
				    obj.Count() > 0)
				{
					return JsonConvert.DeserializeObject (obj[0].ToString(), t);
				}

				return Activator.CreateInstance(t, obj);
			} 
		}
	}
}
