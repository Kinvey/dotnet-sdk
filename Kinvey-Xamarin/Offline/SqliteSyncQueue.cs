using System;
using System.Collections.Generic;
using SQLite.Net.Async;
using System.Threading.Tasks;


namespace KinveyXamarin
{
	public class SqliteSyncQueue : ISyncQueue
	{
		public string Collection { get;}
		private SQLiteAsyncConnection dbConnection;

		public SqliteSyncQueue (string collection, SQLiteAsyncConnection connection)
		{
			this.dbConnection = connection;
			this.Collection = collection;
		}

		public async Task<int> Enqueue (PendingWriteAction pending){
			return await dbConnection.InsertAsync (pending);
		}

		public async Task<List<PendingWriteAction>> GetAll () {
			return await dbConnection.Table <PendingWriteAction> ()
				.Where(t => t.collection == this.Collection).ToListAsync();
		}

		public async Task<PendingWriteAction> GetByID(string entityId) {
			return await dbConnection.Table<PendingWriteAction> ()
				.Where (t => t.collection == this.Collection && t.entityId == entityId)
				.FirstOrDefaultAsync();
		}

		public async Task<PendingWriteAction> Peek () {
			return await dbConnection.Table<PendingWriteAction> ()
				.Where (t => t.collection == this.Collection)
				.FirstOrDefaultAsync();
		}

		public async Task<PendingWriteAction> Pop () {
			try{
				PendingWriteAction item = await Peek ();
				await dbConnection.DeleteAsync <PendingWriteAction> (item.key);
				return item;
			} catch (Exception e){
				return null;
			}
		}

		public async Task<int> Remove (string entityId) {
			PendingWriteAction item = await GetByID (entityId);
			return await dbConnection.DeleteAsync (item.key);
		}

		public async Task<int> RemoveAll () {
			return await dbConnection.DeleteAllAsync <PendingWriteAction> ();
		}


	}
}

