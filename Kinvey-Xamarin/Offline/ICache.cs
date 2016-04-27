using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace KinveyXamarin
{
	public interface ICache <T> {

		Task<List<T>> GetAsync (string query);

		Task<T> GetByIdAsync (string id);

		Task<List<T>> GetAsync (List<string> ids);

		Task<List<T>> GetAsync ();

		Task<List<T>> SaveAsync (List<T> items);

		T Save(T item);

		T UpdateCacheSave(T item, string tempID);

		Task<KinveyDeleteResponse> DeleteAsync (string query);

		KinveyDeleteResponse DeleteByIdAsync (string id);

		Task<KinveyDeleteResponse> DeleteAsync (List<string> ids);


		//Task<int> InsertEntityAsync (T entity);

		//Task<T> UpsertEntityAsync(string id, string json);

		//Task<int> CreateTableAsync(string collectionName);

		//Task<int> DeleteContentsOfTableAsync (string str);


		//TODO typed methods

		//Task<List<T>> getAllAsync (string collection);
		//Task<T> getEntityAsync (string collection, string id);
		//Task<KinveyDeleteResponse> deleteAsync(string collection, string id);



		//Task<int> onCreateAsync(string collectionName);


		//TODO Sync Methods

		//		Task<T[]> getQueryAsync (string queryString);
		//		Task<int> saveQueryResultsAsync (string queryString, string collection, List<string> ids);
		//		Task<int> enqueueRequestAsync (string action, string collection, string id, AbstractKinveyOfflineClientRequest<T> req);
		//		Task<int> enqueueRequestAsync (string action, string collection,SQLTemplates.OfflineMetaData metadata);
		//		Task<SQLTemplates.QueueItem> popQueueAsync ();
		//		Task<int> removeFromQueueAsync (int primaryKey);
		//
		//
	}
}

