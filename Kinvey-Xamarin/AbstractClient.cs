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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using KinveyUtils;

namespace KinveyXamarin
{

	/// <summary>
	/// This class adds the concept of a user to the Client, and couples it with Kinvey.
	/// </summary>
    public abstract class AbstractClient : AbstractKinveyClient
    {
		public ICacheManager CacheManager { get; set; }

		public NetworkFactory NetworkFactory { get ; set ;}
		/// <summary>
		/// The default base URL.
		/// </summary>
        public const string DefaultBaseUrl = "https://baas.kinvey.com/";

		private string micHostName;
		private string micApiVersion;
		/// <summary>
		/// Gets or sets the host URL for MIC.
		/// The host name for your MIC API. This is relevant if you are using a dedicated instance of Kinvey, with an auth base URL that differs from https://auth.kinvey.com
		/// </summary>
		/// <value>The MIC host.  Your hostname must use "https".</value>
		public string MICHostName {
			get { return micHostName;}
			set
			{
				if (!value.StartsWith("https"))
				{
					throw new KinveyException(EnumErrorCode.ERROR_MIC_HOSTNAME_REQUIREMENT_HTTPS, value);
				}

				if (!value.EndsWith ("/"))
				{
					value += "/";
				}

				micHostName = value;
			}
		}

		/// <summary>
		/// Sets the MIC API version. This is relevant in case you need to use a specific version of MIC such as an Early Adopter release.
		/// </summary>
		public string MICApiVersion
		{
			get { return this.micApiVersion; }
			set
			{
				if (!value.StartsWith("v"))
				{
					value = "v" + value;
				}

				micApiVersion = value;
			}
		}

		/// <summary>
		/// The redirect URI for MIC login requests
		/// </summary>
		public string MICRedirectURI { get; set;}

		/// <summary>
		/// The callback for the MIC login, this is used after the redirect
		/// </summary>
		public KinveyDelegate<User> MICDelegate;

		/// <summary>
		/// The default service path.
		/// </summary>
        public const string DefaultServicePath = "";

		/// <summary>
		/// The current user.
		/// </summary>
        protected User currentUser;

		/// <summary>
		/// The current credential store.
		/// </summary>
        private ICredentialStore store;

		/// <summary>
		/// The access lock
		/// </summary>
        protected object Lock = new object();

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.AbstractClient"/> class.
		/// </summary>
		/// <param name="client">Client.</param>
		/// <param name="rootUrl">Root URL.</param>
		/// <param name="servicePath">Service path.</param>
		/// <param name="initializer">Initializer.</param>
		/// <param name="store">Store.</param>
        protected AbstractClient(RestClient client, string rootUrl, string servicePath, KinveyClientRequestInitializer initializer, ICredentialStore store)
            : base(client, rootUrl, servicePath, initializer)
        {
            this.store = store;
			this.MICHostName = "https://auth.kinvey.com/";
			this.NetworkFactory = new NetworkFactory (this);
        }

		/// <summary>
		/// Access AppData operations through this.
		/// </summary>
		/// <returns>The data.</returns>
		/// <param name="collectionName">Collection name.</param>
		/// <param name="myClass">The class definition for entities in this collection.</param>
		/// <typeparam name="T">The Type associated with the Class</typeparam>
		public DataStore<T> AppData<T>(String collection, DataStoreType storeType) where T:class
        {
			//return new AppData<T>(collectionName, myClass, this);
			return KinveyXamarin.DataStore<T>.GetInstance(storeType, collection, this);
//            lock(Lock) 
//            {
//                if (appData == null) 
//                {
//                    appData = new AppData<T>(collectionName, myClass, this);
//                }
//            return (AppData<T>) appData;
//            }
        }

		/// <summary>
		/// Access file operations through this.
		/// </summary>
		public File File()
		{
			return new File (this);
	
		}

		/// <summary>
		/// Execute custom endpoints
		/// </summary>
		/// <returns>A configured instance of the Custom Endpoint object.</returns>
		/// <typeparam name="I">The Type of the input.</typeparam>
		/// <typeparam name="O">The Type of the output.</typeparam>
		public CustomEndpoint<I, O> CustomEndpoint<I, O>(){
			return new CustomEndpoint<I, O> (this);
		}
			
		/// <summary>
		/// Gets or sets the current user.
		/// </summary>
		/// <value>The current user.</value>
        public virtual User CurrentUser
        {
            get
            {
                lock (Lock)
                {
					if (currentUser == null)
					{
						var appKey = ((KinveyClientRequestInitializer)this.RequestInitializer).AppKey;
						var appSecret = ((KinveyClientRequestInitializer)this.RequestInitializer).AppSecret;
						this.CurrentUser = new User(this, new KinveyAuthRequest.Builder(this, appKey, appSecret, null));
					}
                    return currentUser;
                }
            }
            set
            {
                lock (Lock)
                {
                    currentUser = value;
                }
            }
        }

		/// <summary>
		/// Gets the credential store.
		/// </summary>
		/// <value>The store.</value>
        public ICredentialStore Store
        {
            get { return store; }
        }

		/// <summary>
		/// Builder for this AbstractClient implementation.
		/// </summary>
		public new abstract class Builder : AbstractKinveyClient.Builder
        {
            private ICredentialStore store;
            //private Properties props = new Properties();

            public Builder(RestClient transport)
                : base(transport, DefaultBaseUrl, DefaultServicePath) { }

            public Builder(RestClient transport, KinveyClientRequestInitializer clientRequestInitializer)
                : base(transport, DefaultBaseUrl, DefaultServicePath, clientRequestInitializer) { }

            public Builder(RestClient transport, string baseUrl, KinveyClientRequestInitializer clientRequestInitializer)
                : base(transport, baseUrl, DefaultServicePath, clientRequestInitializer) { }

            public ICredentialStore Store
            {
                get { return this.store; }
                set { this.store = value; }
            }

			/// <summary>
			/// Gets the credential for a specified user._id
			/// </summary>
			/// <returns><c>true</c>, if credential was loaded, <c>false</c> otherwise.</returns>
			/// <param name="userId">User identifier.</param>
			protected bool GetCredential(String userId) 
			{

				CredentialManager credentialManager = new CredentialManager(store);
				Credential storedCredential = credentialManager.LoadCredential(userId);
				if (storedCredential != null) 
				{
					var kinveyRequestInitializer = ((KinveyClientRequestInitializer) this.RequestInitializer);
					kinveyRequestInitializer.KinveyCredential = new Credential(userId, storedCredential.AuthToken, storedCredential.UserName, storedCredential.Attributes, null, null);
					return true;
				}
				else
				{
					return false;
				}
			}
		}
	}
}
