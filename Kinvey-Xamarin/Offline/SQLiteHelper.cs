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
	public class SQLiteHelper<T> : DatabaseHelper<T>
	{
		private static SQLiteHelper<T> _instance;

//		private SQLiteConnection _dbConnection;

		private ISQLitePlatform platform;
		private string databasePath;


		public static SQLiteHelper<T> getInstance(ISQLitePlatform platform, string dbpath){
			if (_instance == null) {
				_instance = new SQLiteHelper<T> (platform, dbpath);
			}
			return _instance;
		}

		public SQLiteHelper(ISQLitePlatform platform, string databasePath) 
		{ 
			//_dbConnection = new SQLiteConnection (platform, databasePath);
			this.platform = platform;
			this.databasePath = databasePath;

//			SQLiteConnectionString _connectionParameters = new SQLiteConnectionString(databasePath, false); 
//			SQLiteConnectionPool _sqliteConnectionPool = new SQLiteConnectionPool(platform); 
//			_dbConnection = new SQLiteConnection(() =>
//				_sqliteConnectionPool.GetConnection(_connectionParameters));

		} 

		#region DatabaseHelper implementation

		public void createTable (string collectionName)
		{
			//return new OfflineTable<T> (this, collectionName);
			onCreate (collectionName);
		}


		public List<string> getCollectionTables ()
		{
			SQLiteConnection _dbConnection = new SQLiteConnection (platform, databasePath);
			List<SQLTemplates.TableItem> result = _dbConnection.Table<SQLTemplates.TableItem> ().OrderByDescending (t => t.name).ToList ();
			List<string> collections = new List<string> ();

			foreach (SQLTemplates.TableItem item in result) {
				collections.Add (item.name);
			}

			return collections;
		}

		public int deleteContentsOfTable (string collection)
		{
			SQLiteConnection _dbConnection = new SQLiteConnection (platform, databasePath);
			int result = _dbConnection.DropTable<SQLTemplates.TableItem> ();
			return result;
		
		}

		public void onCreate(string collection){
			SQLiteConnection _dbConnection = new SQLiteConnection (platform, databasePath);
			_dbConnection.CreateTable<SQLTemplates.TableItem> ();
			_dbConnection.CreateTable<SQLTemplates.QueueItem> ();
			_dbConnection.CreateTable<SQLTemplates.QueryItem> ();
			_dbConnection.CreateTable<SQLTemplates.OfflineEntity> ();


			//create the collection item and store it in the collection list
			SQLTemplates.TableItem table = new SQLTemplates.TableItem ();
			table.name = collection;
			_dbConnection.Insert(table);

			int count = _dbConnection.Table<SQLTemplates.TableItem> ().Count ();
			int x = 0;
		}

		public void upsertEntity(string id, string collection, string json){
			SQLTemplates.OfflineEntity entity = new SQLTemplates.OfflineEntity ();
			entity.id = id;
			entity.json = json;
			entity.collection = collection;

			SQLiteConnection _dbConnection = new SQLiteConnection (platform, databasePath);

			int count = _dbConnection.Update (entity);
			if (count == 0) {
				_dbConnection.Insert (entity);
			}
		
		}


		public List<T> getQuery (string queryString, string collection)
		{

			SQLiteConnection _dbConnection = new SQLiteConnection (platform, databasePath);
		
			SQLTemplates.QueryItem query = _dbConnection.Table<SQLTemplates.QueryItem>().Where(t => t.query == queryString && t.collection == collection).FirstOrDefault();

			List<SQLTemplates.OfflineEntity> entities = new List<SQLTemplates.OfflineEntity>();

			string[] ids = query.commaDelimitedIds.Split (',');

			foreach (string id in ids){
				entities.Add(_dbConnection.Table<SQLTemplates.OfflineEntity>().Where(t => t.id == id && t.collection == collection).FirstOrDefault());
			}

			List<T> results = new List<T> ();
			foreach (SQLTemplates.OfflineEntity ent in entities) {
				results.Add (JsonConvert.DeserializeObject<T> (ent.json));
			}
			return results;

		}

		public void enqueueRequest (string action, string collection, string id)
		{
			SQLTemplates.QueueItem queue = new SQLTemplates.QueueItem ();
			queue.action = action;
			queue.collection = collection;
			queue.id = id;

			SQLiteConnection _dbConnection = new SQLiteConnection (platform, databasePath);

			_dbConnection.Insert (queue);

		}

		public List<T> getAll (string collection)
		{

			SQLiteConnection _dbConnection = new SQLiteConnection (platform, databasePath);
			List<SQLTemplates.OfflineEntity> entities = _dbConnection.Table<SQLTemplates.OfflineEntity> ().Where (t => t.collection == collection).ToList ();

			List<T> results = new List<T>();

			foreach (SQLTemplates.OfflineEntity ent in entities) {
				results.Add(JsonConvert.DeserializeObject<T>(ent.json));
			}

			return results;
		}
			

		public T getEntity (string collection, string id)
		{
			SQLiteConnection _dbConnection = new SQLiteConnection (platform, databasePath);

			SQLTemplates.OfflineEntity entity = _dbConnection.Table<SQLTemplates.OfflineEntity> ().Where (t => t.collection == collection && t.id == id).FirstOrDefault ();

			return JsonConvert.DeserializeObject<T> (entity.json);

		}

		public KinveyDeleteResponse delete(string collection, string id)
		{

			SQLiteConnection _dbConnection = new SQLiteConnection (platform, databasePath);
			SQLTemplates.OfflineEntity entity = _dbConnection.Table<SQLTemplates.OfflineEntity> ().Where (t => t.collection == collection && t.id == id).FirstOrDefault ();

			int count = _dbConnection.Delete (entity.id);


			KinveyDeleteResponse resp = new KinveyDeleteResponse ();
			resp.count = count;

			return resp;
		}

		public SQLTemplates.QueueItem popQueue (){
			SQLiteConnection _dbConnection = new SQLiteConnection (platform, databasePath);
			SQLTemplates.QueueItem item = _dbConnection.Table<SQLTemplates.QueueItem> ().FirstOrDefault ();
			return item;

		}

		public void removeFromQueue (int primaryKey)
		{
			SQLiteConnection _dbConnection = new SQLiteConnection (platform, databasePath);

			_dbConnection.Delete<SQLTemplates.QueueItem> (primaryKey);

		}
		#endregion
	}
}

