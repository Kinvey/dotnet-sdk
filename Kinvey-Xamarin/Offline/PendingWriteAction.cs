using System;
using SQLite.Net.Attributes;
using Newtonsoft.Json;

namespace KinveyXamarin
{
	public class PendingWriteAction
	{
		[PrimaryKey, AutoIncrement]
		public int key { get; set; }

		public string entityId { get; set; }

		public string state { get; set; }
			
		public string collection { get; set; }

		public string action { get; set; }

		public PendingWriteAction(){}

		public static PendingWriteAction buildFromRequest <T> (NetworkRequest<T> request) {
			PendingWriteAction newAction = new PendingWriteAction ();
			//newAction.collection = request.CollectionName;
			newAction.action = request.RequestMethod;

			if (request.uriResourceParameters.ContainsKey("entityID"))
			{
				newAction.entityId = request.uriResourceParameters["entityID"];
			}

			if (request.uriResourceParameters.ContainsKey("collectionName"))
			{
				newAction.collection = request.uriResourceParameters["collectionName"];
			}

			newAction.state = JsonConvert.SerializeObject (request.customRequestHeaders);

			return newAction;
		}

		public NetworkRequest<T> toNetworkRequest <T>(AbstractClient client){
			//T entity = cache.GetByIdAsync (entityId);
			//NetworkRequest<T> request = new NetworkRequest<T>(client, this.action) 
			return null;
		}
	}
}