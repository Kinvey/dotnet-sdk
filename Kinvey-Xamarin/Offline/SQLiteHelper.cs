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
		public void createTable (string collectionName)
		{
			onCreate (collectionName);
		}


		/// <summary>
		/// Gets the collection tables.
		/// </summary>
		/// <returns>The collection tables.</returns>
		public List<string> getCollectionTables ()
		{


			Task<List<SQLTemplates.TableItem>> result = getConnection().Table<SQLTemplates.TableItem> ().OrderByDescending (t => t.name).ToListAsync ();
			List<string> collections = new List<string> ();

			var res = result.Result;

			foreach (SQLTemplates.TableItem item in res) {
				collections.Add (item.name);
			}

			return collections;
		}

		/// <summary>
		/// Deletes the contents of table.
		/// </summary>
		/// <returns>The contents of table.</returns>
		/// <param name="collection">Collection.</param>
		public int deleteContentsOfTable (string collection)
		{
			Task<int> result = getConnection().DropTableAsync<SQLTemplates.TableItem> ();
			return result.Result;
		
		}

		/// <summary>
		/// Creates all the tables associated with a collection
		/// </summary>
		/// <param name="collection">Collection.</param>
		public void onCreate(string collection){
			getConnection().CreateTableAsync<SQLTemplates.TableItem> ();
			getConnection().CreateTableAsync<SQLTemplates.QueueItem> ();
			getConnection().CreateTableAsync<SQLTemplates.QueryItem> ();
			getConnection().CreateTableAsync<SQLTemplates.OfflineEntity> ();


			//create the collection item and store it in the collection list
			SQLTemplates.TableItem table = new SQLTemplates.TableItem ();
			table.name = collection;
			getConnection().InsertAsync(table);
		}


		/// <summary>
		/// Upserts a specific entity, adding it directly to to the offline table.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="collection">Collection.</param>
		/// <param name="json">Json.</param>
		public void upsertEntity(string id, string collection, string json){
			SQLTemplates.OfflineEntity entity = new SQLTemplates.OfflineEntity ();
			entity.id = id;
			entity.json = json;
			entity.collection = collection;


			Task<int> count = getConnection().UpdateAsync (entity);
			if (count.Result == 0) {
				getConnection().InsertAsync (entity);
			}
		
		}


		public T[] getQuery (string queryString, string collection)
		{

		
			Task<SQLTemplates.QueryItem> query = getConnection().Table<SQLTemplates.QueryItem>().Where(t => t.query == queryString && t.collection == collection).FirstOrDefaultAsync();

			if (query == null || query.Result == null) {
				return null;
			}

			List<SQLTemplates.OfflineEntity> entities = new List<SQLTemplates.OfflineEntity>();

			string[] ids = query.Result.commaDelimitedIds.Split (',');

			foreach (string id in ids){
				entities.Add(getConnection().Table<SQLTemplates.OfflineEntity>().Where(t => t.id == id && t.collection == collection).FirstOrDefaultAsync().Result);
			}

			T[] results = new T[ids.Length];

			for (int i = 0; i < results.Length; i++){
				results[i] = JsonConvert.DeserializeObject<T>(entities[i].json);
			}
				
			return results;

		}

		public void saveQueryResults (string queryString, string collection, List<string> ids)
		{
			SQLTemplates.QueryItem query = new SQLTemplates.QueryItem ();
			query.query = queryString;
			query.collection = collection;
			query.commaDelimitedIds = String.Join (",", ids); 


			int count = getConnection().UpdateAsync (query).Result;
			if (count == 0) {
				getConnection().InsertAsync (query);
			}
		}


		public void enqueueRequest (string action, string collection, string id)
		{
			SQLTemplates.QueueItem queue = new SQLTemplates.QueueItem ();
			queue.action = action;
			queue.collection = collection;
			queue.id = id;


			getConnection().InsertAsync (queue);

		}

		public List<T> getAll (string collection)
		{

			List<SQLTemplates.OfflineEntity> entities = getConnection().Table<SQLTemplates.OfflineEntity> ().Where (t => t.collection == collection).ToListAsync ().Result;

			List<T> results = new List<T>();

			foreach (SQLTemplates.OfflineEntity ent in entities) {
				results.Add(JsonConvert.DeserializeObject<T>(ent.json));
			}

			return results;
		}
			

		public T getEntity (string collection, string id)
		{

			SQLTemplates.OfflineEntity entity = getConnection().Table<SQLTemplates.OfflineEntity> ().Where (t => t.collection == collection && t.id == id).FirstOrDefaultAsync ().Result;

			if (entity == default(SQLTemplates.OfflineEntity)) {
				return default(T);
			}
			return JsonConvert.DeserializeObject<T> (entity.json);

		}

		public KinveyDeleteResponse delete(string collection, string id)
		{

			SQLTemplates.OfflineEntity entity = getConnection().Table<SQLTemplates.OfflineEntity> ().Where (t => t.collection == collection && t.id == id).FirstOrDefaultAsync ().Result;

			int count = getConnection().DeleteAsync (entity.id).Result;


			KinveyDeleteResponse resp = new KinveyDeleteResponse ();
			resp.count = count;

			return resp;
		}

		public SQLTemplates.QueueItem popQueue (){
			SQLTemplates.QueueItem item = getConnection().Table<SQLTemplates.QueueItem> ().FirstOrDefaultAsync ().Result;
			return item;

		}

		public void removeFromQueue (int primaryKey)
		{

			getConnection().DeleteAsync<SQLTemplates.QueueItem> (primaryKey);

		}
		#endregion
	}
}

