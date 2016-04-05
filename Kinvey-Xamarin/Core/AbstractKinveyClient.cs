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
using Newtonsoft.Json.Linq;
using KinveyUtils;

namespace KinveyXamarin
{
	public abstract class AbstractKinveyClient
	{
		/// <summary>
		/// The kinvey request initializer.
		/// </summary>
        private readonly IKinveyRequestInitializer kinveyRequestInitializer;

		/// <summary>
		/// The root URL.
		/// </summary>
		private readonly string rootUrl;

		/// <summary>
		/// The service path.
		/// </summary>
		private readonly string servicePath;

		/// <summary>
		/// The rest client.
		/// </summary>
		private RestClient restClient;

		private string clientAppVersion = null;

		private JObject customRequestProperties = new JObject();

		public void SetClientAppVersion(string appVersion){
			this.clientAppVersion = appVersion;	
		}

		public void SetClientAppVersion(int major, int minor, int revision){
			SetClientAppVersion(major + "." + minor + "." + revision);
		}

		public string GetClientAppVersion(){
			return this.clientAppVersion;
		}

		public void SetCustomRequestProperties(JObject customheaders){
			this.customRequestProperties = customheaders;
		}

		public void SetCustomRequestProperty(string key, JObject value){
			if (this.customRequestProperties == null){
				this.customRequestProperties = new JObject();
			}
			this.customRequestProperties.Add (key, value);
		}

		public void SetCustomRequestProperty(string key, string value){
			if (this.customRequestProperties == null){
				this.customRequestProperties = new JObject();
			}
			this.customRequestProperties.Add (key, value);
		}

		public void ClearCustomRequestProperties(){
			this.customRequestProperties = new JObject();
		}

		public JObject GetCustomRequestProperties(){
			return this.customRequestProperties;
		}
       
		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.AbstractKinveyClient"/> class.
		/// </summary>
		/// <param name="restClient">Rest client.</param>
		/// <param name="rootUrl">Root URL.</param>
		/// <param name="servicePath">Service path.</param>
        protected AbstractKinveyClient(RestClient restClient, string rootUrl, string servicePath)
            : this(restClient, rootUrl, servicePath, null) {}

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.AbstractKinveyClient"/> class.
		/// </summary>
		/// <param name="restClient">Rest client.</param>
		/// <param name="rootUrl">Root URL.</param>
		/// <param name="servicePath">Service path.</param>
		/// <param name="initializer">Initializer.</param>
        protected AbstractKinveyClient(RestClient restClient, string rootUrl, string servicePath, IKinveyRequestInitializer initializer)
        {
            this.restClient = restClient;
            this.kinveyRequestInitializer = initializer;
            this.rootUrl = NormalizeRootUrl(rootUrl);
            this.servicePath = NormalizeServicePath(servicePath);
			Logger.Log ("Kinvey Client created, running version: " + KinveyHeaders.VERSION);
        }

		//public abstract User User ();

		/// <summary>
		/// Gets the root URL.
		/// </summary>
		/// <value>The root URL.</value>
        public string RootUrl
        {
            get { return this.rootUrl; }
        }

		/// <summary>
		/// Gets the service path.
		/// </summary>
		/// <value>The service path.</value>
        public string ServicePath
        {
            get { return this.servicePath; }
        }

		/// <summary>
		/// Gets the base URL.
		/// </summary>
		/// <value>The base URL.</value>
        public string BaseUrl
        {
            get { return RootUrl + servicePath; }
        }

		/// <summary>
		/// Gets the request initializer.
		/// </summary>
		/// <value>The request initializer.</value>
        public IKinveyRequestInitializer RequestInitializer
        {
            get { return this.kinveyRequestInitializer; }
        }

		/// <summary>
		/// Gets the rest client.
		/// </summary>
		/// <value>The rest client.</value>
        public RestClient RestClient
        {
            get { return this.restClient; }
        }

		/// <summary>
		/// Initializes the request.
		/// </summary>
		/// <param name="request">Request.</param>
		/// <typeparam name="T">The Type of the response</typeparam>
        public void InitializeRequest<T>(AbstractKinveyClientRequest<T> request)
        {
            if (RequestInitializer != null) 
            {
                RequestInitializer.Initialize(request);
            }
        }

		/// <summary>
		/// Normalizes the root URL.
		/// </summary>
		/// <returns>The normalized root URL.</returns>
		/// <param name="rootUrl">Root URL.</param>
        private static string NormalizeRootUrl(string rootUrl)
        {
			if (!rootUrl.ToUpper ().StartsWith ("HTTPS")) {
				throw new KinveyException ("Kinvey requires the usage of SSL over http.  Use `https` as the protocol when setting a base URL");
			}

            if (!rootUrl.EndsWith("/"))
            {
                rootUrl += "/";
            }
            return rootUrl;
        }

		/// <summary>
		/// Normalizes the service path.
		/// </summary>
		/// <returns>The normalized service path.</returns>
		/// <param name="servicePath">Service path.</param>
        private static string NormalizeServicePath(string servicePath)
        {
            if (servicePath.Length == 1)
            {
                servicePath = "";
            }
            else if (servicePath.Length > 0)
            {
                if (!servicePath.EndsWith("/")) 
                {
                    servicePath += "/";

                }
                if (servicePath.StartsWith("/"))
                {
                    servicePath = servicePath.Substring(1);
                }
            }
            return servicePath;
        }

        public abstract class Builder
        {

			/// <summary>
			/// The rest client.
			/// </summary>
            private readonly RestClient restClient;
			/// <summary>
			/// The base URL.
			/// </summary>
            private string baseUrl;
			/// <summary>
			/// The service path.
			/// </summary>
            private string servicePath;
			/// <summary>
			/// The kinvey request initializer.
			/// </summary>
            private KinveyClientRequestInitializer kinveyRequestInitializer;

			/// <summary>
			/// Initializes a new instance of the <see cref="KinveyXamarin.AbstractKinveyClient+Builder"/> class.
			/// </summary>
			/// <param name="transport">Transport.</param>
			/// <param name="defaultRootUrl">Default root URL.</param>
			/// <param name="defaultServicePath">Default service path.</param>
			/// <param name="kinveyRequestInitializer">[optional] Kinvey request initializer.</param>
			public Builder(RestClient transport, string defaultRootUrl, string defaultServicePath, KinveyClientRequestInitializer kinveyRequestInitializer = null)
            {
                this.restClient = transport;
                BaseUrl = defaultRootUrl;
                ServicePath = defaultServicePath;
                this.kinveyRequestInitializer = kinveyRequestInitializer;
            }
				
			/// <summary>
			/// Gets the http rest client.
			/// </summary>
			/// <value>The http rest client.</value>
            public RestClient HttpRestClient
            {
                get { return this.restClient; }
            }

			/// <summary>
			/// Gets or sets the base URL.
			/// </summary>
			/// <value>The base URL.</value>
            public string BaseUrl
            {
                get { return this.baseUrl; }
                set { this.baseUrl = NormalizeRootUrl(value); }
            }

			/// <summary>
			/// Gets or sets the service path.
			/// </summary>
			/// <value>The service path.</value>
            public string ServicePath
            {
                get { return this.servicePath; }
                set { this.servicePath = NormalizeServicePath(value); }
            }

			/// <summary>
			/// Gets or sets the request initializer.
			/// </summary>
			/// <value>The request initializer.</value>
            public KinveyClientRequestInitializer RequestInitializer
            {
                get { return kinveyRequestInitializer; }
                set { this.kinveyRequestInitializer = value; }
            }
        }
    }
}
