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


		public static string PREFIX_OFFLINE = "offline_";
		public static string PREFIX_QUEUE = "queue_";
		public static string PREFIX_QUERY = "query_";
		public static string PREFIX_RESULTS = "results_";


		public OfflineTable ()
		{
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

		public T insertEntity (DatabaseHelper<T> handler, AbstractKinveyClient client, string jsonContent)
		{
			return default(T);
		}

		public KinveyDeleteResponse delete (DatabaseHelper<T> handler, AbstractKinveyClient client, string targetID)
		{
			return new KinveyDeleteResponse ();
		}
	}
}

