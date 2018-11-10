// Copyright (c) 2015, Kinvey, Inc. All rights reserved.
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
using SQLite;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;

namespace Kinvey
{
	/// <summary>
	/// This class is the entry point for access to all of the Kinvey's library features.  Use a Client.Builder to create one.
	/// Once the Client is created and a user is logged in, all of Kinvey's features can be accessed through their respective accessors.
	/// This implementation is thread safe.
	/// </summary>
	public partial class Client : AbstractClient
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

		internal static Client _sharedClient;

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

        public Constants.DevicePlatform DevicePlatform { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.Client"/> class.  Use a Client.Builder to create one.
		/// </summary>
		/// <param name="client">The RestClient.</param>
		/// <param name="rootUrl">The Root URL of the Kinvey instance this is associated with.</param>
		/// <param name="servicePath">The service path, appended to the rootURL.</param>
		/// <param name="initializer">The request initializer, maintaining headers and authentication.</param>
		/// <param name="store">The credential store, where the current user's credentials will be stored.</param>
		protected Client(HttpClient client, string rootUrl, string servicePath, KinveyClientRequestInitializer initializer, ICredentialStore store)
			: base(client, rootUrl, servicePath, initializer, store) {}


		/// <summary>
		/// Execute custom endpoints
		/// </summary>
		/// <returns>A configured instance of the Custom Endpoint object.</returns>
		/// <typeparam name="I">The Type of the input.</typeparam>
		/// <typeparam name="O">The Type of the output.</typeparam>
		public new CustomEndpoint<I, O> CustomEndpoint<I, O>()
		{
			return new CustomEndpoint<I, O>(this);
		}

		/// <summary>
		/// Pings the backend service in order to ensure that a connection can be established to Kinvey from this client.
		/// </summary>
		/// <returns>The <see cref="KinveyXamarin.PingResponse"/> object, from which the version can be accessed. </returns>
		public async Task<PingResponse> PingAsync()
		{
            var urlParameters = new Dictionary<string, string>
            {
                { "appKey", ((KinveyClientRequestInitializer)RequestInitializer).AppKey }
            };

            var ping = new PingRequest(this, urlParameters)
            {
                RequireAppCredentials = true
            };
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
		public new partial class Builder : AbstractClient.Builder
		{

#pragma warning disable IDE1006 // Naming Styles
            /// <summary>
            /// A reference to the local file system -- going to be platform dependent
            /// </summary>
            /// <value>The file path.</value>
            [Obsolete("This property has been deprecated. Please use FilePath instead.")]
            private string filePath
            {
                get
                {
                    return FilePath;
                }
                set
                {
                    FilePath = value;
                }
            }
#pragma warning restore IDE1006 // Naming Styles

            /// <summary>
            /// A reference to the local file system -- going to be platform dependent
            /// </summary>
            /// <value>The file path.</value>
            public string FilePath { get; set; }

            private ICacheManager CacheManager {get; set; }

			/// <summary>
			/// Gets or sets the log Action -- going to be platform dependent
			/// </summary>
			/// <value>The log.</value>
			private Action<string> log{ get ; set;}

			private string senderID { get ; set;}

			private string ssoGroupKey;

            private string instanceID;

            private Constants.DevicePlatform devicePlatform;

			/// <summary>
			/// Initializes a new instance of the <see cref="T:KinveyXamarin.Client.Builder"/> class.
			/// </summary>
			/// <param name="appKey">App key from Kinvey</param>
			/// <param name="appSecret">App secret from Kinvey</param>
            protected Builder(string appKey, string appSecret, string filePath, Constants.DevicePlatform devicePlatform = Constants.DevicePlatform.PCL)
                : base(new HttpClient(), new KinveyClientRequestInitializer(appKey, appSecret, new KinveyHeaders(devicePlatform)))
			{
                FilePath = filePath;
				ssoGroupKey = appKey;
                instanceID = string.Empty;
                this.devicePlatform = devicePlatform;
			}

			/// <summary>
			/// This method creates and initializes a client for use with Kinvey.
			/// </summary>
			public virtual Client Build()
			{
				if (this.FilePath != null)
				{
					if (this.Store == null)
					{
						this.Store = new SQLiteCredentialStore (FilePath);
					}

					if (this.CacheManager == null)
					{
						this.CacheManager = new SQLiteCacheManager (FilePath);
					}
				}

				if (this.Store == null)
				{
					this.Store = new InMemoryCredentialStore();
				}

				Client c =  new Client(this.HttpClient, this.BaseUrl, this.ServicePath, this.RequestInitializer, this.Store);
//				c.offline_platform = this.offlinePlatform;
//				c.filePath = this.filePath;
                c.CacheManager = this.CacheManager;
				c.logger = this.log;
				c.senderID = this.senderID;
				c.SSOGroupKey = this.ssoGroupKey;
                if (!string.IsNullOrEmpty(this.MICHostName)) c.MICHostName = this.MICHostName;
                if (!string.IsNullOrEmpty(instanceID))
                {
                    c.MICHostName = $"{Constants.STR_PROTOCOL_HTTPS + instanceID + Constants.STR_HYPHEN + Constants.STR_HOSTNAME_AUTH}";
                }
                c.DevicePlatform = this.devicePlatform;

                Logger.initialize (c.logger);

				SharedClient = c;

				Credential currentCredential = this.Store.GetStoredCredential(this.ssoGroupKey);
				if (currentCredential != null)
				{
					c.DeviceID = currentCredential.DeviceID;
					User.LoginAsync(currentCredential, c);
				}

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
            public Builder setBaseURL(string url)
            {
                this.BaseUrl = url;
                return this;
            }

            public Builder setMICHostName(string url)
            {
                this.MICHostName = url;
                return this;
            }

            /// <summary>
            /// Set any appended service url to the base url, if necessary.
            /// </summary>
            /// <returns>The service path.</returns>
            /// <param name="servicePath">Service path.</param>
            public Builder setServicePath(string servicePath)
            {
                this.ServicePath = servicePath;
                return this;
            }

#pragma warning disable IDE1006 // Naming Styles
            /// <summary>
            /// Set the directory to use for offline.
            /// </summary>
            /// <returns>The file path.</returns>
            /// <param name="filePath">Path.</param>
            [Obsolete("This method has been deprecated. Please use SetFilePath() instead.")]
            public Builder setFilePath(string filePath)
            {
                return SetFilePath(filePath);
            }
#pragma warning restore IDE1006 // Naming Styles

            /// <summary>
            /// Set the directory to use for offline.
            /// </summary>
            /// <returns>The file path.</returns>
            /// <param name="filePath">Path.</param>
            public Builder SetFilePath(string filePath)
            {
                FilePath = filePath;
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

            public Builder SetRestClient(HttpClient client)
            {
                this.HttpClient = client;
                return this;
            }

			public Builder SetSSOGroupKey(string ssoGroupKey)
			{
				this.ssoGroupKey = ssoGroupKey;
				return this;
			}

            public Builder SetInstanceID(string instanceID)
            {
                this.instanceID = instanceID;
                string url = $"{Constants.STR_PROTOCOL_HTTPS + instanceID + Constants.STR_HYPHEN + Constants.STR_HOSTNAME_API}";
                this.setBaseURL(url);
                return this;
            }
        }
	}
}
