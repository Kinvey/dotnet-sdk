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

		public async Task<bool> Enqueue (PendingWriteAction pending){
			return false;
		}
		public async Task<List<PendingWriteAction>> GetAll () {
			return default(List<PendingWriteAction>);
		}
		public async Task<List<PendingWriteAction>> GetByCollection (string collection) {
			return default(List<PendingWriteAction>);
		}
		public async Task<PendingWriteAction> GetByID(string entityID) {
			return default(PendingWriteAction);
		}

		public async Task<PendingWriteAction> Peek () {
			return default(PendingWriteAction);

		}

		public async Task<PendingWriteAction> Pop () {
			return default(PendingWriteAction);

		}

		public async Task<bool> Remove (string entityID) {
			return false;
		}

		public async Task<bool> RemoveAll () {
			return false;
		}


	}
}

