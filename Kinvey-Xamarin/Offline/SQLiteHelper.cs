// Copyright (c) 2015, Kinvey, Inc. All rights reserved.
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
using SQLite.Net.Interop;
using SQLite.Net;
using SQLite.Net.Async;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;

namespace KinveyXamarin
{


	/// <summary>
	/// This class wraps and manages all sqlite access needed by offline.
	/// </summary>
	public class SQLiteHelper<T> : DatabaseHelper<T>
	{

		/// <summary>
		/// This class is a singleton, this is the instance.
		/// </summary>
		private static SQLiteHelper<T> _instance;

		/// <summary>
		/// The platform.
		/// </summary>
		private ISQLitePlatform platform;
		/// <summary>
		/// The database path.
		/// </summary>
		private string databasePath;

		/// <summary>
		/// The db connection.
		/// </summary>
		private SQLiteAsyncConnection dbConnection;

		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <returns>The instance.</returns>
		/// <param name="platform">The sqlite platform to use.</param>
		/// <param name="dbpath">Where to save the databse file.</param>
		public static SQLiteHelper<T> getInstance(ISQLitePlatform platform, string dbpath){
			if (_instance == null) {
				_instance = new SQLiteHelper<T> (platform, dbpath);
			}
			return _instance;
		}

		private SQLiteAsyncConnection getConnection(){
			if (dbConnection == null) {
				var connectionFactory = new Func<SQLiteConnectionWithLock>(()=>new SQLiteConnectionWithLock(platform, new SQLiteConnectionString(databasePath, storeDateTimeAsTicks: false)));
				dbConnection = new SQLiteAsyncConnection (connectionFactory);
			}
			return dbConnection;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.SQLiteHelper`1"/> class.
		/// </summary>
		/// <param name="platform">Platform.</param>
		/// <param name="databasePath">Database path.</param>
		internal SQLiteHelper(ISQLitePlatform platform, string databasePath) 
		{ 
			this.platform = platform;
			this.databasePath = databasePath;
		} 

		#region DatabaseHelper implementation

		/// <summary>
		/// Creates an Offline Table, which manages all offline collection features.
		/// </summary>
		/// <param name="collectionName">Collection name.</param>
		public async Task<int> createTableAsync (string collectionName)
		{
			await onCreateAsync (collectionName);
			return 0;
		}


		/// <summary>
		/// Gets the collection tables.
		/// </summary>
		/// <returns>The collection tables.</returns>
		public async Task<List<string>> getCollectionTablesAsync ()
		{
			List<SQLTemplates.TableItem> result = await getConnection().Table<SQLTemplates.TableItem> ().OrderByDescending (t => t.name).ToListAsync ();
			List<string> collections = new List<string> ();


			foreach (SQLTemplates.TableItem item in result) {
				collections.Add (item.name);
			}

			return collections;
		}

		/// <summary>
		/// Deletes the contents of table.
		/// </summary>
		/// <returns>The contents of table.</returns>
		/// <param name="collection">Collection.</param>
		public async Task<int> deleteContentsOfTableAsync (string collection)
		{
			int result = await getConnection().DropTableAsync<SQLTemplates.TableItem> ();
			return result;
		
		}

		/// <summary>
		/// Creates all the tables associated with a collection
		/// </summary>
		/// <param name="collection">Collection.</param>
		public async Task<int> onCreateAsync(string collection){
			await getConnection().CreateTableAsync<SQLTemplates.TableItem> ();
			await getConnection().CreateTableAsync<SQLTemplates.QueueItem> ();
			await getConnection().CreateTableAsync<SQLTemplates.QueryItem> ();
			await getConnection().CreateTableAsync<SQLTemplates.OfflineEntity> ();


			//create the collection item and store it in the collection list
			SQLTemplates.TableItem table = new SQLTemplates.TableItem ();
			table.name = collection;
			await getConnection().InsertAsync(table);

			return 0;
		}


		/// <summary>
		/// Upserts a specific entity, adding it directly to to the offline table.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="collection">Collection.</param>
		/// <param name="json">Json.</param>
		public async Task<T> upsertEntityAsync(string id, string collection, string json){
			SQLTemplates.OfflineEntity entity = new SQLTemplates.OfflineEntity ();
			entity.id = id;
			entity.json = json;
			entity.collection = collection;


			int count = await getConnection().UpdateAsync (entity);
			if (count == 0) {
				await getConnection().InsertAsync (entity);
			}

			return JsonConvert.DeserializeObject<T> (json);
		
		}


		public async Task<T[]> getQueryAsync (string queryString, string collection)
		{

		
			SQLTemplates.QueryItem query = await getConnection().Table<SQLTemplates.QueryItem>().Where(t => t.query == queryString && t.collection == collection).FirstOrDefaultAsync();

			if (query == null) {
				return null;
			}

			List<SQLTemplates.OfflineEntity> entities = new List<SQLTemplates.OfflineEntity>();

			string[] ids = query.commaDelimitedIds.Split (',');

			foreach (string id in ids){
				entities.Add(getConnection().Table<SQLTemplates.OfflineEntity>().Where(t => t.id == id && t.collection == collection).FirstOrDefaultAsync().Result);
			}

			T[] results = new T[ids.Length];

			for (int i = 0; i < results.Length; i++){
				results[i] = JsonConvert.DeserializeObject<T>(entities[i].json);
			}
				
			return results;

		}

		public async Task<int> saveQueryResultsAsync (string queryString, string collection, List<string> ids)
		{
			SQLTemplates.QueryItem query = new SQLTemplates.QueryItem ();
			query.query = queryString;
			query.collection = collection;
			query.commaDelimitedIds = String.Join (",", ids); 


			int count = await getConnection().UpdateAsync (query);
			if (count == 0) {
				await getConnection().InsertAsync (query);
			}

			return 0;
		}


		public async Task<int> enqueueRequestAsync (string action, string collection, string id, AbstractKinveyOfflineClientRequest<T> req)
		{
			SQLTemplates.QueueItem queue = new SQLTemplates.QueueItem ();
			queue.action = action;
			queue.collection = collection;

			SQLTemplates.OfflineMetaData metadata = new SQLTemplates.OfflineMetaData ();
			metadata.id = id;
			metadata.clientVersion = req.clientAppVersion;
			metadata.customHeaders = req.customRequestHeaders;

			queue.OfflineMetaDataAsJson = JsonConvert.SerializeObject (metadata);
			//queue.id = metadata;

			await getConnection().InsertAsync (queue);

			return 0;
		}

		public async Task<int> enqueueRequestAsync (string action, string collection,SQLTemplates.OfflineMetaData metadata)
		{
			SQLTemplates.QueueItem queue = new SQLTemplates.QueueItem ();
			queue.action = action;
			queue.collection = collection;

			queue.OfflineMetaDataAsJson = JsonConvert.SerializeObject (metadata);

			await getConnection().InsertAsync (queue);

			return 0;
		}

		public async Task<List<T>> getAllAsync (string collection)
		{

			List<SQLTemplates.OfflineEntity> entities = await getConnection().Table<SQLTemplates.OfflineEntity> ().Where (t => t.collection == collection).ToListAsync ();

			List<T> results = new List<T>();

			foreach (SQLTemplates.OfflineEntity ent in entities) {
				results.Add(JsonConvert.DeserializeObject<T>(ent.json));
			}

			return results;
		}
			

		public async Task<T> getEntityAsync (string collection, string id)
		{

			SQLTemplates.OfflineEntity entity = await getConnection().Table<SQLTemplates.OfflineEntity> ().Where (t => t.collection == collection && t.id == id).FirstOrDefaultAsync ();

			if (entity == default(SQLTemplates.OfflineEntity)) {
				return default(T);
			}
			return JsonConvert.DeserializeObject<T> (entity.json);

		}

		public async Task<KinveyDeleteResponse> deleteAsync(string collection, string id)
		{

			SQLTemplates.OfflineEntity entity = await getConnection().Table<SQLTemplates.OfflineEntity> ().Where (t => t.collection == collection && t.id == id).FirstOrDefaultAsync ();

			int count = await getConnection().DeleteAsync (entity.id);


			KinveyDeleteResponse resp = new KinveyDeleteResponse ();
			resp.count = count;

			return resp;
		}

		public async Task<SQLTemplates.QueueItem> popQueueAsync (){
			SQLTemplates.QueueItem item = await getConnection().Table<SQLTemplates.QueueItem> ().FirstOrDefaultAsync ();
			await removeFromQueueAsync (item.key);
			return item;

		}

		public async Task<int> removeFromQueueAsync (int primaryKey)
		{

			await getConnection().DeleteAsync<SQLTemplates.QueueItem> (primaryKey);
			return 1;

		}
		#endregion
	}
}

