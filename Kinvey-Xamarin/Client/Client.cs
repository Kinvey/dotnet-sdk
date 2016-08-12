﻿// Copyright (c) 2015, Kinvey, Inc. All rights reserved.
//
// This software is licensed to you under the Kinvey terms of service located at
// http://www.kinvey.com/terms-of-use. By downloading, accessing and/or using this
// software, you hereby accept such terms of service  (and any agreement referenced
// therein) and agree that you have read, understand and agree to be bound by such
// terms of service and are of legal age to agree to such terms with Kinvey.
//
// This software contains valuable confidential and proprietary information of
// KINVEY, INC and is subject to applicable licensing agreements.
// Unauthorized reproduction, transmission or distribution of this file and its
// contents is a violation of applicable laws.

using System;
using System.Collections.Generic;
using RestSharp;
using SQLite.Net.Interop;
using System.Threading.Tasks;
using KinveyUtils;
using Newtonsoft.Json;

namespace KinveyXamarin
{
	/// <summary>
	/// This class is the entry point for access to all of the Kinvey's library features.  Use a Client.Builder to create one.
	/// Once the Client is created and a user is logged in, all of Kinvey's features can be accessed through their respective accessors.
	/// This implementation is thread safe.
	/// </summary>
	public class Client : AbstractClient
	{
		/// <summary>
		/// Gets or sets the logger, this action is performed when writing to the logs.
		/// </summary>
		/// <value>The logger.</value>
		public Action<string> logger {get; set;}

		/// <summary>
		/// The Sender ID for GCM Push
		/// </summary>
		/// <value>The sender ID.</value>
		public string senderID { get; set;}

		private static Client _sharedClient;

		/// <summary>
		/// The Shared Client instance.
		/// Whenever a new Client is built with Client.Builder(...).build(), it is set as the SharedClient. 
		/// SharedClient must be built before it is accessed. Attempting to access a null SharedClient will result in a KinveyException thrown from the getter.
		/// </summary>
		/// <value>The shared client.</value>
		public static Client SharedClient
		{
			get
			{
				if (_sharedClient == null)
				{
					throw new KinveyException(EnumErrorCategory.ERROR_CLIENT, EnumErrorCode.ERROR_CLIENT_SHARED_CLIENT_NULL, "");
				}
				return _sharedClient;
			}
			set
			{
				_sharedClient = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.Client"/> class.  Use a Client.Builder to create one.
		/// </summary>
		/// <param name="client">The RestClient.</param>
		/// <param name="rootUrl">The Root URL of the Kinvey instance this is associated with.</param>
		/// <param name="servicePath">The service path, appended to the rootURL.</param>
		/// <param name="initializer">The request initializer, maintaining headers and authentication.</param>
		/// <param name="store">The credential store, where the current user's credentials will be stored.</param>
		protected Client(IRestClient client, string rootUrl, string servicePath, KinveyClientRequestInitializer initializer, ICredentialStore store)
			: base(client, rootUrl, servicePath, initializer, store) {}


		/// <summary>
		/// Execute custom endpoints
		/// </summary>
		/// <returns>A configured instance of the Custom Endpoint object.</returns>
		/// <typeparam name="I">The Type of the input.</typeparam>
		/// <typeparam name="O">The Type of the output.</typeparam>
		public new AsyncCustomEndpoint<I, O> CustomEndpoint<I, O>(){
			return new AsyncCustomEndpoint<I, O> (this);
		}

		/// <summary>
		/// Pings the backend service in order to ensure that a connection can be established to Kinvey from this client.
		/// </summary>
		/// <returns>The <see cref="KinveyXamarin.PingResponse"/> object, from which the version can be accessed. </returns>
		public async Task<PingResponse> PingAsync()
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer) RequestInitializer).AppKey);

			PingRequest ping = new PingRequest(this, urlParameters);
			ping.RequireAppCredentials = true;
			InitializeRequest(ping);

			return await ping.ExecuteAsync();
		}

		[JsonObject(MemberSerialization.OptIn)]
		private class PingRequest : AbstractKinveyClientRequest<PingResponse>
		{
			private const string REST_PATH = "appdata/{appKey}";

			internal PingRequest(AbstractClient client, Dictionary<string, string> urlProperties)
				: base(client, "GET", REST_PATH, default(PingResponse), urlProperties)
			{
			}
		}

		/// <summary>
		/// Builder for creating a new instance of a client.  Use this class to easily create a new client, as it uses the builder pattern so methods can be chained together.
		/// Once the builder is configured, call `.build()` to return an instance of a client.
		/// </summary>
		public new class Builder : AbstractClient.Builder
		{

			/// <summary>
			/// A reference to the local file system -- going to be platform dependent
			/// </summary>
			/// <value>The file path.</value>
			private string filePath {get; set;}

			/// <summary>
			///a reference to the sqlite implementation -- going to be platform dependent
			/// </summary>
			/// <value>The offline platform.</value>
			private ISQLitePlatform offlinePlatform {get; set;}

			private ICacheManager CacheManager {get; set; }

			/// <summary>
			/// Gets or sets the log Action -- going to be platform dependent
			/// </summary>
			/// <value>The log.</value>
			private Action<string> log{ get ; set;}

			private string senderID { get ; set;}

			/// <summary>
			/// Initializes a new instance of the <see cref="T:KinveyXamarin.Client.Builder"/> class.
			/// </summary>
			/// <param name="appKey">App key from Kinvey</param>
			/// <param name="appSecret">App secret from Kinvey</param>
			public Builder(string appKey, string appSecret)
				: base(new RestClient (), new KinveyClientRequestInitializer(appKey, appSecret, new KinveyHeaders()))
			{
			}

			/// <summary>
			/// This method creates and initializes a client for use with Kinvey.
			/// </summary>
			public virtual Client build(){
				if (this.filePath != null && offlinePlatform != null){
					if (this.Store == null) {
						this.Store = new SQLiteCredentialStore (offlinePlatform, filePath);
					}
					if (this.CacheManager == null) {
						this.CacheManager = new SQLiteCacheManager (offlinePlatform, filePath);
					}
				}

				if (this.Store == null){
					this.Store = new InMemoryCredentialStore();
				}


				Client c =  new Client(this.HttpRestClient, this.BaseUrl, this.ServicePath, this.RequestInitializer, this.Store);
//				c.offline_platform = this.offlinePlatform;
//				c.filePath = this.filePath;
				c.CacheManager = this.CacheManager;
				c.logger = this.log;
				c.senderID = this.senderID;

				Logger.initialize (c.logger);
				Credential currentCredential = this.Store.getActiveUser ();
				if (currentCredential != null)
				{
					User.LoginAsync(currentCredential, c);
				}

				SharedClient = c;
				return c;
			}

			/// <summary>
			///Set the credential store to use for the client.
			/// </summary>
			/// <returns>This builder.</returns>
			/// <param name="store">Store.</param>
			public Builder setCredentialStore(ICredentialStore store){
				this.Store = store;
				return this;
			}
				

			/// <summary>
			///Set the base url to use for this client, if it is a custom one.
			/// </summary>
			/// <returns>This builder..</returns>
			/// <param name="url">URL.</param>
			public Builder setBaseURL(string url){
				this.BaseUrl = url;
				return this;
			}


			/// <summary>
			/// Set any appended service url to the base url, if necessary.
			/// </summary>
			/// <returns>The service path.</returns>
			/// <param name="servicePath">Service path.</param>
			public Builder setServicePath(string servicePath){
				this.ServicePath = servicePath;
				return this;
			}
				
			/// <summary>
			/// Set the directory to use for offline.
			/// </summary>
			/// <returns>The file path.</returns>
			/// <param name="path">Path.</param>
			public Builder setFilePath(string path){
				this.filePath = path;
				return this;
			}
				
			/// <summary>
			/// Set the sqlite implementation to use for offline.
			/// </summary>
			/// <returns>The offline platform.</returns>
			/// <param name="platform">Platform.</param>
			public Builder setOfflinePlatform(ISQLitePlatform platform){
				this.offlinePlatform = platform;
				return this;
			}

			/// <summary>
			/// Sets the logger action -- the ClientLogger class uses this to write to logs.
			/// </summary>
			/// <returns>The logger.</returns>
			/// <param name="log">Log.</param>
			public Builder setLogger(Action<string> log){
				this.log = log;
				return this;
			}

			/// <summary>
			/// Sets the project identifier of the <see cref="KinveyXamarin.Client"/>.
			/// </summary>
			/// <returns>The <see cref="KinveyXamarin.Client.Builder"/> object</returns>
			/// <param name="senderid">Sender ID.</param>
			public Builder SetProjectId(string senderid)
			{
				this.senderID = senderid;
				return this;
			}

			public Builder SetRestClient(IRestClient client)
			{
				this.HttpRestClient = client;
				return this;
			}
		}
	}
}