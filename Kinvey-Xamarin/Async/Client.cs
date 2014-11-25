using System;
using RestSharp;
using SQLite.Net.Interop;

namespace KinveyXamarin
{
	/// <summary>
	/// This class is the entry point for access to all of the Kinvey's library features.  Use a Client.Builder to create one, and then login through:  myClient.User().Login*.
	/// Once the Client is created and a user is logged in, all of Kinvey's features can be accessed through their respective accessors.
	/// This implementation is thread safe.
	/// </summary>
	public class Client : AbstractClient
	{

		/// <summary>
		/// This maintains the current logged in user
		/// </summary>
		private AsyncUser user;

		/// <summary>
		/// The file path for writing to disk is platform specific, so this is maintained in the client.
		/// </summary>
		/// <value>The file path.</value>
		public string filePath { get; set; }

		/// <summary>
		/// the SQLite platform is platform specific, so this is maintained in the client.
		/// </summary>
		/// <value>The offline platform.</value>
		public ISQLitePlatform offline_platform { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.Client"/> class.  Use a Client.Builder to create one.
		/// </summary>
		/// <param name="client">The RestClient.</param>
		/// <param name="rootUrl">The Root URL of the Kinvey instance this is associated with.</param>
		/// <param name="servicePath">The service path, appended to the rootURL.</param>
		/// <param name="initializer">The request initializer, maintaining headers and authentication.</param>
		/// <param name="store">The credential store, where the current user's credentials will be stored.</param>
		protected Client(RestClient client, string rootUrl, string servicePath, KinveyClientRequestInitializer initializer, ICredentialStore store)
			: base(client, rootUrl, servicePath, initializer, store) {}


		/// <summary>
		/// Use this accessor to get a configurared instance of the <see cref="KinveyXamarin.AsyncUser"/> class. 
		/// </summary>
		/// <returns>the current authenticated User object.</returns>
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

		/// <summary>
		/// Use this accessor to get a configured instance of the <see cref="KinveyXamarin.AsyncAppData"/> class.
		/// </summary>
		/// <returns>A configured instance of AppData.</returns>
		/// <param name="collectionName">The collection name associated with this instance of AppData.</param>
		/// <param name="myClass">The Class associated with entites in the collection.</param>
		/// <typeparam name="T">The Type of the entity in the collection.</typeparam>
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

