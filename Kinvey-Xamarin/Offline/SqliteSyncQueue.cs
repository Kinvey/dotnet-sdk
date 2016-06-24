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

		public int Enqueue (PendingWriteAction pending)
		{
			PendingWriteAction existingSyncItem = GetByID(pending.entityId);
			if (existingSyncItem != null)
			{
				// an entry for this entity already exists
				if (existingSyncItem.action == "POST" && pending.action == "PUT")
				{
					// do not put in the sync queue, since a POST will already capture all the latest changes
					return 0;
				}
				else if (existingSyncItem.action == "PUT" && pending.action == "POST")
				{
					// highly unlikely, but favor the POST
					this.Remove(existingSyncItem.entityId);
				}
				else if (existingSyncItem.action == "DELETE" && (pending.action == "PUT" || pending.action == "POST"))
				{
					// odd case where an object has somehow been created/updated after a delete call, but favor the create/update
					this.Remove(existingSyncItem.entityId);
				}
				else if (pending.action == "DELETE")
				{
					// no matter what, favor the current deletion
					this.Remove(existingSyncItem.entityId);
				}
			}

			return dbConnection.Insert(pending);
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

		public List<PendingWriteAction> GetFirstN(int limit, int offset)
		{
			string query = $"SELECT * FROM PendingWriteAction WHERE collection == \"{this.Collection}\" LIMIT {limit} OFFSET {offset}";
			return dbConnection.Query<PendingWriteAction>(query);
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

		public int Count(bool allCollections)
		{
			if (allCollections)
			{
				return dbConnection.Table<PendingWriteAction>()
								   .Count();
			}

			return dbConnection.Table<PendingWriteAction>()
				               .Where (t => t.collection == this.Collection)
				               .Count();
		}

		public int Remove(string entityId)
		{
			PendingWriteAction item = GetByID(entityId);
			if (item == null)
			{
				return 0;
			}

			return dbConnection.Delete(item);
		}

		public int RemoveAll () {
			return  dbConnection.DeleteAll <PendingWriteAction> ();
		}
	}
}

