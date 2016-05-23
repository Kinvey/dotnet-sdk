using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace KinveyXamarin
{
	public abstract class AbstractDataRequest<T> : AbstractKinveyClientRequest<T>{
		[JsonProperty]
		public string CollectionName { get; set; }

		public AbstractDataRequest (AbstractClient client, string method, string template, Object httpContent, string collection): base(client, method, template, httpContent, new Dictionary<string, string>()){
			this.CollectionName = collection;
			uriResourceParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			uriResourceParameters.Add("collectionName", collection);
		}
	}}

