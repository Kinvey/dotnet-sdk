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
		void createTable(string collectionName);

		/// <summary>
		/// Returns a list of all collections currently stored offline
		/// </summary>
		/// <returns>The collection tables.</returns>
		List<string> getCollectionTables ();

		/// <summary>
		/// Deletes the contents of table.
		/// </summary>
		/// <returns>The contents of table.</returns>
		/// <param name="str">String.</param>
		int deleteContentsOfTable (string str);

		/// <summary>
		/// Creates all the defaults for a new collection
		/// </summary>
		/// <param name="collectionName">Collection name.</param>
		void onCreate(string collectionName);

		/// <summary>
		/// Upsertsa specific entity
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="collection">Collection.</param>
		/// <param name="json">Json.</param>
		void upsertEntity(string id, string collection, string json);

		/// <summary>
		/// Gets the results of a query
		/// </summary>
		/// <returns>The query.</returns>
		/// <param name="queryString">Query string.</param>
		/// <param name="collection">Collection.</param>
		List<T> getQuery (string queryString, string collection);

		/// <summary>
		/// Saves the query and the _ids associated with it's results
		/// </summary>
		/// <param name="queryString">Query string.</param>
		/// <param name="collection">Collection.</param>
		/// <param name="ids">Identifiers.</param>
		void saveQueryResults (string queryString, string collection, List<string> ids);

		/// <summary>
		/// Enqueues the request.
		/// </summary>
		/// <param name="action">Action.</param>
		/// <param name="collection">Collection.</param>
		/// <param name="id">Identifier.</param>
		void enqueueRequest (string action, string collection, string id);

		/// <summary>
		/// Gets all entites in a collection
		/// </summary>
		/// <returns>The entities.</returns>
		/// <param name="collection">Collection.</param>
		List<T> getAll (string collection);

		/// <summary>
		/// Gets a specific entity.
		/// </summary>
		/// <returns>The entity.</returns>
		/// <param name="collection">Collection.</param>
		/// <param name="id">_id.</param>
		T getEntity (string collection, string id);

		/// <summary>
		/// Delete the specified _id in the collection.
		/// </summary>
		/// <param name="collection">Collection.</param>
		/// <param name="id">_id to delete.</param>
		KinveyDeleteResponse delete(string collection, string id);

		/// <summary>
		/// Pops the queue.
		/// </summary>
		/// <returns>the next request ot execute.</returns>
		SQLTemplates.QueueItem popQueue ();

		/// <summary>
		/// Removes from queue.
		/// </summary>
		/// <param name="primaryKey">Primary key of the queue item to remove.</param>
		void removeFromQueue (int primaryKey);
	}
}

