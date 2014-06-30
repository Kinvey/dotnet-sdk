using System;
using Kinvey.DotNet.Framework;
using RestSharp;
using Kinvey.DotNet.Framework.Core;
using Kinvey.DotNet.Framework.Auth;

namespace KinveyXamarin
{
	public class Client : AbstractClient
	{


		private AsyncUser user;
//		private AsyncAppData appData;

		protected Client(RestClient client, string rootUrl, string servicePath, KinveyClientRequestInitializer initializer, ICredentialStore store)
			: base(client, rootUrl, servicePath, initializer, store) {}


		public new AsyncUser User()
		{
			lock (Lock)
			{
				if (user == null) {
					var appKey = ((KinveyClientRequestInitializer)this.RequestInitializer).AppKey;
					var appSecret = ((KinveyClientRequestInitializer)this.RequestInitializer).AppSecret;
					this.user = new AsyncUser(this, new KinveyAuthRequest.Builder(this.RestClient, this.BaseUrl, appKey, appSecret, null));
				}

				return user;
			}
		}

		public new AsyncAppData<T> AppData<T>(String collectionName, Type myClass)
		{
			return new AsyncAppData<T>(collectionName, myClass, this);
		}

	
		public new class Builder : AbstractClient.Builder
		{
		
			public Builder(string appKey, string appSecret) 
				: base(new RestClient (), new Kinvey.DotNet.Framework.Core.KinveyClientRequestInitializer (appKey, appSecret, new KinveyHeaders ())) {}


			public Client build() {
				return new Client(this.HttpRestClient, this.BaseUrl, this.ServicePath, this.RequestInitializer, this.Store);
			}

	
		}
	}
}

