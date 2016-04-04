// Copyright (c) 2015, Kinvey, Inc. All rights reserved.
//
// This software is licensed to you under the Kinvey terms of service located at
// http://www.kinvey.com/terms-of-use. By downloading, accessing and/or using this
// software, you hereby accept such terms of service  (and any agreement referenced
// therein) and agree that you have read, understand and agree to be bound by such
// terms of service and are of legal age to agree to such terms with Kinvey.
//
// This software contains valuable confidential and proprietary information of
// KINVEY, INC and is subject to applicable licensing agreements.
// Unauthorized reproduction, transmission or distribution of this file and its
// contents is a violation of applicable laws.

using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace KinveyXamarin
{

	/// <summary>
	/// This class defines the behaivor of an offline store, which mimics CRUD operations
	/// </summary>
	public interface IOfflineStore <T> {
		
		Task<List<T>> GetAsync (string query);

		Task<T> GetByIdAsync (string id);

		Task<List<T>> GetAsync (List<string> ids);

		Task<List<T>> GetAsync ();
	
		Task<List<T>> SaveAsync (List<T> items);

		Task<T> SaveAsync (T item);

		Task<KinveyDeleteResponse> DeleteAsync (string query);

		Task<KinveyDeleteResponse> DeleteByIdAsync (string id);

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

