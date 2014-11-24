using System;
using RestSharp;
using SQLite.Net.Interop;

namespace KinveyXamarin
{
	public class Client : AbstractClient
	{


		private AsyncUser user;


		public string filePath { get; set; }

		public ISQLitePlatform offline_platform { get; set; }

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

//			private string offlinePath{ get; set;}
			private string filePath {get; set;}
			private ISQLitePlatform offlinePlatform {get; set;}
		
			public Builder(string appKey, string appSecret) 
				: base(new RestClient (), new KinveyClientRequestInitializer (appKey, appSecret, new KinveyHeaders ())) {}


			public Client build() {
				if (this.filePath != null && offlinePlatform != null && this.Store == null) {
					this.Store = new SQLiteCredentialStore (offlinePlatform, filePath);
				} else if (this.Store == null){
					this.Store = new InMemoryCredentialStore();
				}


				Client c =  new Client(this.HttpRestClient, this.BaseUrl, this.ServicePath, this.RequestInitializer, this.Store);
//				c.offline_dbpath = this.offlinePath;
				c.offline_platform = this.offlinePlatform;
				c.filePath = this.filePath;
	
				return c;
			}

			public Builder setCredentialStore(ICredentialStore store){
				this.Store = store;
				return this;
			}

			public Builder setBaseURL(string url){
				this.BaseUrl = url;
				return this;
			}

			public Builder setServicePath(string servicePath){
				this.ServicePath = servicePath;
				return this;
			}

			public Builder setFilePath(string path){
				this.filePath = path;
				return this;
			}


			public Builder setOfflinePlatform(ISQLitePlatform platform){
				this.offlinePlatform = platform;
				return this;
			}


	
		}
	}
}

