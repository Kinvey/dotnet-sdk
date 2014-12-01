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
			SQLiteConnection _dbConnection = new SQLiteConnection (platform, databasePath);
			List<SQLTemplates.TableItem> result = _dbConnection.Table<SQLTemplates.TableItem> ().OrderByDescending (t => t.name).ToList ();
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
		public int deleteContentsOfTable (string collection)
		{
			SQLiteConnection _dbConnection = new SQLiteConnection (platform, databasePath);
			int result = _dbConnection.DropTable<SQLTemplates.TableItem> ();
			return result;
		
		}

		/// <summary>
		/// Creates all the tables associated with a collection
		/// </summary>
		/// <param name="collection">Collection.</param>
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

			if (query == null) {
				return null;
			}

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

		public void saveQueryResults (string queryString, string collection, List<string> ids)
		{
			SQLTemplates.QueryItem query = new SQLTemplates.QueryItem ();
			query.query = queryString;
			query.collection = collection;
			query.commaDelimitedIds = String.Join (",", ids); 

			SQLiteConnection _dbConnection = new SQLiteConnection (platform, databasePath);

			int count = _dbConnection.Update (query);
			if (count == 0) {
				_dbConnection.Insert (query);
			}
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

