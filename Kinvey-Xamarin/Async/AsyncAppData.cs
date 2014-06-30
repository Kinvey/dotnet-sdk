using System;
using Kinvey.DotNet.Framework;
using Kinvey.DotNet.Framework.Core;

namespace KinveyXamarin
{
	public class AsyncAppData<T> : AppData<T>
	{
		public AsyncAppData (string collectionName, Type myClass, AbstractClient client): base(collectionName, myClass, client)
		{
		}


//		public GetEntityRequest GetEntityBlocking(string entityId)
//		{
//			var urlParameters = new Dictionary<string, string>();
//			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
//			urlParameters.Add("collectionName", CollectionName);
//			urlParameters.Add("entityId", entityId);
//			GetEntityRequest getEntity = new GetEntityRequest(entityId, myClass, client, urlParameters);
//			client.InitializeRequest(getEntity);
//			getEntity.setCache (this.cache, this.policy);
//			return getEntity;
//		}
//
//		public GetRequest GetBlocking()
//		{
//			var urlParameters = new Dictionary<string, string>();
//			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
//			urlParameters.Add("collectionName", CollectionName);
//			GetRequest get = new GetRequest(myClass, client, urlParameters);
//			client.InitializeRequest(get);
//			get.setCache (this.cache, this.policy);
//			return get;
//		}
//
//		public SaveRequest SaveBlocking(T entity)
//		{
//			SaveRequest save;
//			var urlParameters = new Dictionary<string, string>();
//			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
//			urlParameters.Add("collectionName", CollectionName);
//			save = new SaveRequest(entity, myClass, null, SaveMode.POST, client, urlParameters);
//			client.InitializeRequest(save);
//			return save;
//		}



	}
}

