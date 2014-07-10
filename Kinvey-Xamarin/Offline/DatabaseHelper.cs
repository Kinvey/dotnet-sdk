using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

		int deleteContentsOfTable (string str);

		void onCreate(string collectionName);

		void upsertEntity(string id, string collection, string json);

		List<T> getQuery (string queryString, string collection);

		void enqueRequest (string action, string collection, string id);
		List<T> getAll (string collection);
		T getEntity (string collection, string id);
		KinveyDeleteResponse delete(string collection, string id);

		SQLTemplates.QueueItem popQueue ();


		void removeFromQueue (int primaryKey);
	}
}

