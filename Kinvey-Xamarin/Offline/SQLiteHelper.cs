using System;
using System.Collections.Generic;

namespace KinveyXamarin
{
	public class SQLiteHelper<T> : DatabaseHelper<T>
	{
		private static SQLiteHelper<T> _instance;

		public static SQLiteHelper<T> getInstance(){
			if (_instance == null) {
				_instance = new SQLiteHelper<T> ();
			}
			return _instance;
		}

		#region DatabaseHelper implementation

		public OfflineTable<T> getTable (string collectionName)
		{
			return new OfflineTable<T> ();
		}


		public List<string> getCollectionTables ()
		{
			return new List<string> (); //TODO
		}

		public void deleteContentsOfTable (string str)
		{
			throw new NotImplementedException (); //TODO
		}
		#endregion
	}
}

