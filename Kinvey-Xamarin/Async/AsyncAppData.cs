using System;
using System.Threading.Tasks;

namespace KinveyXamarin
{

	/// <summary>
	/// Async app data.  This class allows access to a Kinvey datastore asynchronously.  CRUD operations are supported, as well as LINQ querying.  
	/// </summary>
	public class AsyncAppData<T> : AppData<T>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.AsyncAppData`1"/> class.
		/// </summary>
		/// <param name="collectionName">Collection name.</param>
		/// <param name="myClass">The class json data will be serialized into.</param>
		/// <param name="client">A configured instance of a Kinvey client.</param>
		public AsyncAppData (string collectionName, Type myClass, AbstractClient client): base(collectionName, myClass, client)
		{
		}
			
		/// <summary>
		/// Get a single entity stored in a Kinvey collection.
		/// </summary>
		/// <param name="entityId">Entity identifier.</param>
		/// <param name="delegates">Delegates for success or failure.</param>
		public void GetEntity(string entityId, KinveyDelegate<T> delegates)
		{
			Task.Run (() => {
				try {
					T entity = base.GetEntityBlocking (entityId).Execute ();
					delegates.onSuccess (entity);
				} catch (Exception e) {
					delegates.onError (e);
				}
			});

		}

		/// <summary>
		/// Get a single entity stored in a Kinvey collection.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="entityId">Entity identifier.</param>
		public async Task<T> GetEntityAsync(string entityId){
			return await GetEntityBlocking (entityId).ExecuteAsync ();
		}
			
		/// <summary>
		/// Get all entities from a Kinvey collection.
		/// </summary>
		/// <param name="delegates">Delegates for success or failure.</param>
		public void Get(KinveyDelegate<T[]> delegates)
		{
			Task.Run (() => {
				try {
					T[] entity = base.GetBlocking ().Execute ();
					delegates.onSuccess (entity);
				} catch (Exception e) {
					delegates.onError (e);
				}
			});
		}

		/// <summary>
		/// Get all entities from a Kinvey collection.
		/// </summary>
		/// <returns>The async task.</returns>
		public async Task<T[]> GetAsync(){
			return await GetBlocking ().ExecuteAsync ();
		}

		/// <summary>
		/// Save the specified entity to a Kinvey collection.
		/// </summary>
		/// <param name="entity">the entity to save.</param>
		/// <param name="delegates">Delegates for success or failure.</param>
		public void Save(T entity, KinveyDelegate<T> delegates)
		{
			Task.Run (() => {
				try {
					T saved = base.SaveBlocking (entity).Execute ();
					delegates.onSuccess (saved);
				} catch (Exception e) {
					delegates.onError (e);
				}
			});
		}

		/// <summary>
		/// Save the specified entity to a Kinvey collection.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="entity">the entity to save.</param>
		public async Task<T> SaveAsync(T entity){
			return await SaveBlocking (entity).ExecuteAsync ();
		}

		/// <summary>
		/// Returns the results of a kinvey-style mongodb raw query.  Note this class also supports LINQ for querying.
		/// </summary>
		/// <param name="query">The raw query string to execute.</param>
		/// <param name="delegates">Delegates for success or failure.</param>
		public void Get(string query, KinveyDelegate<T[]> delegates){
			Task.Run (() => {
				try {
					T[] results = base.getQueryBlocking (query).Execute ();
					delegates.onSuccess (results);
				} catch (Exception e) {
					delegates.onError (e);
				}
			});

		}

		/// <summary>
		/// Returns the results of a kinvey-style mongodb raw query.  Note this class also supports LINQ for querying.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="query">The raw query string to execute.</param>
		public async Task<T[]> getAsync(string query){
			return await getQueryBlocking (query).ExecuteAsync ();
		}

		/// <summary>
		/// Deletes the entity associated with the provided id
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="entityId">the _id of the entity to delete.</param>
		public async Task<KinveyDeleteResponse> DeleteAsync(string entityId){
			return await DeleteBlocking (entityId).ExecuteAsync ();
		}
	}
}

