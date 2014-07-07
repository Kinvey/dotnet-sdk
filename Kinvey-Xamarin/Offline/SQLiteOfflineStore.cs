using System;

namespace KinveyXamarin
{
	public class SQLiteOfflineStore<T> : OfflineStore<T>
	{
		public SQLiteOfflineStore ()
		{
		}


		public T executeGet(){
			return default(T);
		}

		public T executeSave(){
			return default(T);
		}

		public void executeDelete(){

		}

		public void insertEntity(){

		}

		public void clearStorage(){

		}

		public void kickOffSync(){

		}

	}
}

