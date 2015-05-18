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

		/// <summary>
		/// Creates a new table
		/// </summary>
		/// <param name="collectionName">Collection name.</param>
		Task<int> createTableAsync(string collectionName);

		/// <summary>
		/// Returns a list of all collections currently stored offline
		/// </summary>
		/// <returns>The collection tables.</returns>
		Task<List<string>> getCollectionTablesAsync ();

		/// <summary>
		/// Deletes the contents of table.
		/// </summary>
		/// <returns>The contents of table.</returns>
		/// <param name="str">String.</param>
		Task<int> deleteContentsOfTableAsync (string str);

		/// <summary>
		/// Creates all the defaults for a new collection
		/// </summary>
		/// <param name="collectionName">Collection name.</param>
		Task<int> onCreateAsync(string collectionName);

		/// <summary>
		/// Upsertsa specific entity
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="collection">Collection.</param>
		/// <param name="json">Json.</param>
		Task<T> upsertEntityAsync(string id, string collection, string json);

		/// <summary>
		/// Gets the results of a query
		/// </summary>
		/// <returns>The query.</returns>
		/// <param name="queryString">Query string.</param>
		/// <param name="collection">Collection.</param>
		Task<T[]> getQueryAsync (string queryString, string collection);

		/// <summary>
		/// Saves the query and the _ids associated with it's results
		/// </summary>
		/// <param name="queryString">Query string.</param>
		/// <param name="collection">Collection.</param>
		/// <param name="ids">Identifiers.</param>
		Task<int> saveQueryResultsAsync (string queryString, string collection, List<string> ids);

		/// <summary>
		/// Enqueues the request.
		/// </summary>
		/// <param name="action">Action.</param>
		/// <param name="collection">Collection.</param>
		/// <param name="id">Identifier.</param>
		Task<int> enqueueRequestAsync (string action, string collection, string id, AbstractKinveyOfflineClientRequest<T> req);

		/// <summary>
		///Enqueues the request from metadata.
		/// </summary>
		/// <returns>The request async.</returns>
		/// <param name="action">Action.</param>
		/// <param name="collection">Collection.</param>
		/// <param name="metadata">Metadata.</param>
		Task<int> enqueueRequestAsync (string action, string collection,SQLTemplates.OfflineMetaData metadata);

		/// <summary>
		/// Gets all entites in a collection
		/// </summary>
		/// <returns>The entities.</returns>
		/// <param name="collection">Collection.</param>
		Task<List<T>> getAllAsync (string collection);

		/// <summary>
		/// Gets a specific entity.
		/// </summary>
		/// <returns>The entity.</returns>
		/// <param name="collection">Collection.</param>
		/// <param name="id">_id.</param>
		Task<T> getEntityAsync (string collection, string id);

		/// <summary>
		/// Delete the specified _id in the collection.
		/// </summary>
		/// <param name="collection">Collection.</param>
		/// <param name="id">_id to delete.</param>
		Task<KinveyDeleteResponse> deleteAsync(string collection, string id);

		/// <summary>
		/// Pops the queue.
		/// </summary>
		/// <returns>the next request ot execute.</returns>
		Task<SQLTemplates.QueueItem> popQueueAsync ();

		/// <summary>
		/// Removes from queue.
		/// </summary>
		/// <param name="primaryKey">Primary key of the queue item to remove.</param>
		Task<int> removeFromQueueAsync (int primaryKey);
	}
}

