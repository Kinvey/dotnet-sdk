using System;
using Kinvey.DotNet.Framework.Core;
using Newtonsoft.Json;

namespace KinveyXamarin
{
	/// <summary>
	/// This class performs the actual logic against the database.  It is created by the offline store, and uses a provided DatabaseHelper to perform the operations
	/// 
	/// </summary>
	public class OfflineTable<T>
	{


		private string collectionName{ get; set;}



		public OfflineTable (DatabaseHelper<T> handler, string collectionName)
		{
			this.collectionName = collectionName;
			//TODO check if already created first althogh from documentation this won't overwrite if already exists
			handler.onCreate (collectionName);
		}
			
		public void insertEntity (DatabaseHelper<T> handler, AbstractKinveyClient client, string id, string collection, string jsonContent)
		{
			handler.upsertEntity (id, collection, jsonContent);
		}


		public T getQuery (DatabaseHelper<T> handler, AbstractKinveyClient client, string collection, string query)
		{
			handler.getQuery(query, collection);

			return default(T);
		}

		public void enqueueRequest (DatabaseHelper<T> handler, string verb, string collection, string id)
		{
			handler.enqueRequest (verb, collection, id);
		}

		public T getAll (DatabaseHelper<T> handler, AbstractKinveyClient client, string collection)
		{

			 handler.getAll(collection);
			throw new NotSupportedException ();
			return default(T);

		}

		public T getEntity (DatabaseHelper<T> handler, AbstractKinveyClient client, string collection, string id)
		{
			return handler.getEntity(collection, id);
		}



		public KinveyDeleteResponse delete (DatabaseHelper<T> handler, AbstractKinveyClient client, string collection, string id)
		{
			handler.delete (collection, id);
			return new KinveyDeleteResponse ();
		}
	}
}

