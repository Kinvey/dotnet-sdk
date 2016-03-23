using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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
		public AsyncAppData (string collectionName, Type myClass, AbstractClient client): base(DataStoreType.SYNC, client)
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
		/// Gets a count of all the entities in a collection
		/// </summary>
		/// <param name="delegates">Delegates for success or failure.</param>
		public void GetCount(KinveyDelegate<uint> delegates)
		{
			Task.Run( () => {
				try
				{
					uint count = 0;
					T countObj = base.getCountBlocking().Execute();
					if (countObj is JObject) {
						JToken value = (countObj as JObject).GetValue("count");
						count = value.ToObject<uint>();
					}
					delegates.onSuccess(count);
				}
				catch(Exception e)
				{
					delegates.onError(e);
				}
			});
		}

		/// <summary>
		/// Gets a count of all the entities in a collection that match a particular query
		/// </summary>
		/// <param name="queryString">The query to process.</param>
		/// <param name="delegates">Delegates for success or failure.</param>
		public void GetCount(string queryString, KinveyDelegate<uint> delegates)
		{
			Task.Run( () => {
				try
				{
					uint count = 0;
					T countObj = base.getCountBlocking(queryString).Execute();
					if (countObj is JObject) {
						JToken value = (countObj as JObject).GetValue("count");
						count = value.ToObject<uint>();
					}
					delegates.onSuccess(count);
				}
				catch(Exception e)
				{
					delegates.onError(e);
				}
			});
		}

		/// <summary>
		/// Gets a count of all the entities in a collection that match a particular query
		/// </summary>
		/// <returns>The async task which returns the count.</returns>
		/// <param name="queryString">The query to process.</param>
		public async Task<uint> GetCountAsync(string queryString)
		{
			uint count = 0;
			T countObj = await base.getCountBlocking(queryString).ExecuteAsync ();
			if (countObj is JObject) {
				JToken value = (countObj as JObject).GetValue("count");
				count = value.ToObject<uint>();
			}
			return count;
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

	}
}

