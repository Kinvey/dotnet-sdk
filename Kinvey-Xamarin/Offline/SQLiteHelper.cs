using System;

namespace KinveyXamarin
{
	public class SQLiteHelper
	{
		private static SQLiteHelper _instance;

		public static SQLiteHelper getInstance(){
			if (_instance == null) {
				_instance = new SQLiteHelper ();
			}
			return _instance;
		}





	}
}

