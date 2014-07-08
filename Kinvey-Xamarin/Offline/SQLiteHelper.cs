using System;
using System.Collections.Generic;
using SQLite.Net.Interop;
using SQLite.Net;
using SQLite.Net.Async;

namespace KinveyXamarin
{
	public class SQLiteHelper<T> : DatabaseHelper<T>
	{
		private static SQLiteHelper<T> _instance;

		public static SQLiteHelper<T> getInstance(){
			if (_instance == null) {
				_instance = new SQLiteHelper<T> (null, null);
			}
			return _instance;
		}

		public SQLiteHelper(ISQLitePlatform platform, string databasePath) 
		{ 
			SQLiteConnectionString _connectionParameters = new SQLiteConnectionString(databasePath, false); 
			SQLiteConnectionPool _sqliteConnectionPool = new SQLiteConnectionPool(platform); 
			SQLiteAsyncConnection _dbConnection = new SQLiteAsyncConnection(() =>
				_sqliteConnectionPool.GetConnection(_connectionParameters)); 
		} 

		#region DatabaseHelper implementation

		public OfflineTable<T> getTable (string collectionName)
		{
			return new OfflineTable<T> (collectionName);
		}


		public List<string> getCollectionTables ()
		{
			return new List<string> (); //TODO
		}

		public void deleteContentsOfTable (string str)
		{
			throw new NotImplementedException (); //TODO
		}

		public void RunCommand (string createCommand)
		{
			throw new NotImplementedException (); //TODO
		}
		#endregion
	}
}

