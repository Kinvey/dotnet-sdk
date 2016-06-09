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

		public List<PendingWriteAction> GetAll()
		{
			List<PendingWriteAction> listPWA = new List<PendingWriteAction>();

			var filter = dbConnection.Table<PendingWriteAction>().Where(blah => blah.collection == this.Collection);
			foreach (PendingWriteAction pwa in filter)
			{
				listPWA.Add(pwa);
			}

			return listPWA;
		}

		public PendingWriteAction GetByID(string entityId) {
			return  dbConnection.Table<PendingWriteAction> ()
				.Where (t => t.collection == this.Collection && t.entityId == entityId)
				.FirstOrDefault();
		}

		public  PendingWriteAction Peek () {
			return  dbConnection.Table<PendingWriteAction> ()
				.Where (t => t.collection == this.Collection)
				.OrderByDescending(u => u.key)
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

		public int Remove(string entityId)
		{
			PendingWriteAction item = GetByID(entityId);
			return dbConnection.Delete(item);
		}

		public int RemoveAll () {
			return  dbConnection.DeleteAll <PendingWriteAction> ();
		}


	}
}

