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

		private SQLiteConnection _dbConnection;

		public static SQLiteHelper<T> getInstance(ISQLitePlatform platform, string dbpath){
			if (_instance == null) {
				_instance = new SQLiteHelper<T> (platform, dbpath);
			}
			return _instance;
		}

		public SQLiteHelper(ISQLitePlatform platform, string databasePath) 
		{ 
			_dbConnection = new SQLiteConnection (platform, databasePath);

//			SQLiteConnectionString _connectionParameters = new SQLiteConnectionString(databasePath, false); 
//			SQLiteConnectionPool _sqliteConnectionPool = new SQLiteConnectionPool(platform); 
//			_dbConnection = new SQLiteConnection(() =>
//				_sqliteConnectionPool.GetConnection(_connectionParameters));

		} 

		#region DatabaseHelper implementation

		public OfflineTable<T> getTable (string collectionName)
		{
			return new OfflineTable<T> (this, collectionName);
		}


		public List<string> getCollectionTables ()
		{
			List<SQLTemplates.TableItem> result = _dbConnection.Table<SQLTemplates.TableItem> ().OrderByDescending (t => t.name).ToList ();
			List<string> collections = new List<string> ();

			foreach (SQLTemplates.TableItem item in result) {
				collections.Add (item.name);
			}

			return collections;
		}

		public int deleteContentsOfTable (string collection)
		{
			int result = _dbConnection.DropTable<SQLTemplates.TableItem> ();
			return result;
		
		}

		public void onCreate(string collection){
			_dbConnection.CreateTable<SQLTemplates.TableItem> ();
			_dbConnection.CreateTable<SQLTemplates.QueueItem> ();
			_dbConnection.CreateTable<SQLTemplates.QueryItem> ();
			_dbConnection.CreateTable<SQLTemplates.OfflineEntity> ();


			//create the collection item and store it in the collection list
			SQLTemplates.TableItem table = new SQLTemplates.TableItem ();
			table.name = collection;
			_dbConnection.Insert(table);
		}

		public void upsertEntity(string id, string collection, string json){
			SQLTemplates.OfflineEntity entity = new SQLTemplates.OfflineEntity ();
			entity.id = id;
			entity.json = json;
			entity.collection = collection;

			int count = _dbConnection.Update (entity);
			if (count == 0) {
				_dbConnection.Insert (entity);
			}
		}


		public List<T> getQuery (string queryString, string collection)
		{
		
			SQLTemplates.QueryItem query = _dbConnection.Table<SQLTemplates.QueryItem>().Where(t => t.query == queryString && t.collection == collection).FirstOrDefault();

			List<SQLTemplates.OfflineEntity> entities = new List<SQLTemplates.OfflineEntity>();
			foreach (string id in query.ids){
				entities.Add(_dbConnection.Table<SQLTemplates.OfflineEntity>().Where(t => t.id == id && t.collection == collection).FirstOrDefault());
			}

			List<T> results = new List<T> ();
			foreach (SQLTemplates.OfflineEntity ent in entities) {
				results.Add (JsonConvert.DeserializeObject<T> (ent.json));
			}
			return results;

		}

		public void enqueRequest (string action, string collection, string id)
		{
			SQLTemplates.QueueItem queue = new SQLTemplates.QueueItem ();
			queue.action = action;
			queue.collection = collection;
			queue.id = id;

			_dbConnection.Insert (queue);

		}

		public List<T> getAll (string collection)
		{
			List<SQLTemplates.OfflineEntity> entities = _dbConnection.Table<SQLTemplates.OfflineEntity> ().Where (t => t.collection == collection).ToList ();

			List<T> results = new List<T>();

			foreach (SQLTemplates.OfflineEntity ent in entities) {
				results.Add(JsonConvert.DeserializeObject<T>(ent.json));
			}

			return results;
		}
			

		public T getEntity (string collection, string id)
		{

			SQLTemplates.OfflineEntity entity = _dbConnection.Table<SQLTemplates.OfflineEntity> ().Where (t => t.collection == collection && t.id == id).FirstOrDefault ();

			return JsonConvert.DeserializeObject<T> (entity.json);

		}

		public KinveyDeleteResponse delete(string collection, string id)
		{
			SQLTemplates.OfflineEntity entity = _dbConnection.Table<SQLTemplates.OfflineEntity> ().Where (t => t.collection == collection && t.id == id).FirstOrDefault ();

			int count = _dbConnection.Delete (entity.key);

			KinveyDeleteResponse resp = new KinveyDeleteResponse ();
			resp.count = count;

			return resp;
		}

		public SQLTemplates.QueueItem popQueue (){
			SQLTemplates.QueueItem item = _dbConnection.Table<SQLTemplates.QueueItem> ().FirstOrDefault ();
			return item;

		}

		public void removeFromQueue (string primaryKey)
		{

			_dbConnection.Delete<SQLTemplates.QueueItem> (primaryKey);

		}
		#endregion
	}
}

