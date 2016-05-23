using System;
using System.Collections.Generic;
using SQLite.Net;
using System.Threading.Tasks;


namespace KinveyXamarin
{
	public class SqliteSyncQueue : ISyncQueue
	{
		public string Collection { get;}
		private SQLiteConnection dbConnection;

		public SqliteSyncQueue (string collection, SQLiteConnection connection)
		{
			this.dbConnection = connection;
			this.Collection = collection;
		}

		public int Enqueue (PendingWriteAction pending){
			return dbConnection.Insert (pending);
		}

		public List<PendingWriteAction> GetAll () {
			//return dbConnection.Table <PendingWriteAction> ()
			//	.Where(t => t.collection == this.Collection);

			//TODO pending implementation
			return null; 

		}

		public PendingWriteAction GetByID(string entityId) {
			return  dbConnection.Table<PendingWriteAction> ()
				.Where (t => t.collection == this.Collection && t.entityId == entityId)
				.FirstOrDefault();
		}

		public  PendingWriteAction Peek () {
			return  dbConnection.Table<PendingWriteAction> ()
				.Where (t => t.collection == this.Collection)
				.FirstOrDefault();
		}

		public PendingWriteAction Pop () {
			try{
				PendingWriteAction item = Peek ();
				dbConnection.Delete <PendingWriteAction> (item.key);
				return item;
			} catch (Exception e){
				return null;
			}
		}

		public int Remove (string entityId) {
			PendingWriteAction item = GetByID (entityId);
			return  dbConnection.Delete (item.key);
		}

		public int RemoveAll () {
			return  dbConnection.DeleteAll <PendingWriteAction> ();
		}


	}
}

