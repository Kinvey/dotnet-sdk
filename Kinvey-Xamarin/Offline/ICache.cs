// Copyright (c) 2016, Kinvey, Inc. All rights reserved.
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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Kinvey
{
	/// <summary>
	/// Interface for the data store cache.
	/// </summary>
	public interface ICache <T>
	{
		/// <summary>
		/// Checks if the cache is empty.
		/// </summary>
		/// <returns><c>true</c>, if cache is empty, <c>false</c> otherwise.</returns>
		bool IsCacheEmpty();

		/// <summary>
		/// Finds all items in the cache.
		/// </summary>
		/// <returns>All cache items.</returns>
		List<T> FindAll();

		/// <summary>
		/// Finds the by entity ID.
		/// </summary>
		/// <returns>The entity with the given ID.</returns>
		/// <param name="ID">The ID of the entity to find.</param>
		T FindByID(string ID);

		/// <summary>
		/// Finds a list of entities by the given IDs.
		/// </summary>
		/// <returns>A list of the entities matching the given IDs.</returns>
		/// <param name="ids">The IDs to find.</param>
		List<T> FindByIDs(List<string> ids);

		// TODO do this via LINQ
		//Task<List<T>> GetAsync (string query);

		/// <summary>
		/// Save the specified item.
		/// </summary>
		/// <returns>The saved item.</returns>
		/// <param name="item">The item to save in the cache.</param>
		T Save(T item);

		/// <summary>
		/// Save the specified list of item.
		/// </summary>
		/// <returns>The list of saved items.</returns>
		/// <param name="items">The list of items to save in the cache.</param>
		List<T> Save(List<T> items);

		/// <summary>
		/// Updates the cached item with the final ID.
		/// </summary>
		/// <returns>The cached item.</returns>
		/// <param name="item">The item to update.</param>
		/// <param name="tempID">The temporary ID used in the cached, which will be replaced with the permanent ID.</param>
		T UpdateCacheSave(T item, string tempID);

		/// <summary>
		/// Update the specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		T Update(T item);

		/// <summary>
		/// Refreshs the cache with the latest items from the backend.
		/// </summary>
		/// <returns>The list of cached items.</returns>
		/// <param name="items">List of items used to refresh cache.</param>
		List<T> RefreshCache(List<T> items);

		/// <summary>
		/// Finds entities by query.
		/// </summary>
		/// <returns>List of entities matching the given query</returns>
		/// <param name="expr">Expression derived from QueryModel object.</param>
		List<T> FindByQuery(Expression expr);

		/// <summary>
		/// Deletes all the cached items.
		/// </summary>
		/// <returns>A KinveyDeleteResponse object.</returns>
		/// <param name="expr">Expression derived from QueryModel object.</param>
		KinveyDeleteResponse Clear(Expression expr);

		/// <summary>
		/// Deletes the cached item by ID.
		/// </summary>
		/// <returns>A KinveyDeleteResponse object.</returns>
		/// <param name="id">The ID of the entity to delete from the cache.</param>
		KinveyDeleteResponse DeleteByID(string id);

		/// <summary>
		/// Deletes a list of entities by the given IDs.
		/// </summary>
		/// <returns>A KinveyDeleteResponse object.</returns>
		/// <param name="ids">The IDs of the entities to delete from the cache.</param>
		KinveyDeleteResponse DeleteByIDs(List<string> IDs);

		Task<KinveyDeleteResponse> DeleteAsync (string query);

		List<GroupAggregationResults> GetAggregateResult(EnumReduceFunction reduceFunction, string groupField, string aggregateField, Expression query);

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
