using System;
using System.Collections.Generic;

namespace KinveyXamarin
{

	/// <summary>
	/// This interface defines the behaivor necessary to interface with a database implementation
	/// 
	/// </summary>
	public interface DatabaseHelper<T>
	{

		OfflineTable<T> getTable(string collectionName);

		List<string> getCollectionTables ();

		void deleteContentsOfTable (string str);

		void RunCommand (string createCommand);


	}
}

