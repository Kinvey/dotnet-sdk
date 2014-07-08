using System;
using Kinvey.DotNet.Framework.Core;

namespace KinveyXamarin
{
	/// <summary>
	/// This class performs the actual logic against the database.  It is created by the offline store, and uses a provided DatabaseHelper to perform the operations
	/// 
	/// </summary>
	public class OfflineTable<T>
	{


		private string collectionName{ get; set;}



		public OfflineTable (string collectionName)
		{
			this.collectionName = collectionName;

		}



		public T insertEntity (DatabaseHelper<T> handler, AbstractKinveyClient client, string jsonContent)
		{

//			string jsonContent = JsonConvert.SerializeObject (entity);

			return default(T);
		}


		public T getQuery (DatabaseHelper<T> handler, AbstractKinveyClient client, string query, object par)
		{


			return default(T);
		}

		public void enqueueRequest (DatabaseHelper<T> handler, string verb, string targetURL)
		{

		}

		public T getAll (DatabaseHelper<T> handler, AbstractKinveyClient client, object par)
		{
			return default(T);
		}

		public T getEntity (DatabaseHelper<T> handler, AbstractKinveyClient client, string targetID, Type currentType)
		{
			return default(T);
		}



		public KinveyDeleteResponse delete (DatabaseHelper<T> handler, AbstractKinveyClient client, string targetID)
		{
			return new KinveyDeleteResponse ();
		}
	}
}

