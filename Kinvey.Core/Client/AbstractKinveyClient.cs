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

using Newtonsoft.Json.Linq;
using KinveyUtils;
using System.Net.Http;

namespace Kinvey
{
	/// <summary>
	/// Base class of the Kinvey Client.  Handles setup for making requests to Kinvey backend.
	/// </summary>
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
		private HttpClient httpClient;

		private string clientAppVersion = null;

		private JObject customRequestProperties = new JObject();

		/// <summary>
		/// Sets the client app version. We strongly recommend (but do not require) using version strings that conform to the pattern:
		/// "major.minor.patch", where all values are integers and minor and patch are optional.
		/// Here are some examples for version strings specified in this format - “1.1.5”, “2.6”, “3”
		/// </summary>
		/// <returns>The client app version.</returns>
		/// <param name="appVersion">App version.</param>
		public void SetClientAppVersion(string appVersion)
		{
			this.clientAppVersion = appVersion;	
		}

		/// <summary>
		/// Sets the client app version, in terms major, minor and revision numbers.
		/// </summary>
		/// <param name="major">Major version number</param>
		/// <param name="minor">Minor version number</param>
		/// <param name="revision">Revision number</param>
		public void SetClientAppVersion(int major, int minor, int revision)
		{
			SetClientAppVersion(major + "." + minor + "." + revision);
		}

		/// <summary>
		/// Gets the client app version.
		/// </summary>
		/// <returns>The client app version.</returns>
		public string GetClientAppVersion()
		{
			return this.clientAppVersion;
		}

		/// <summary>
		/// Sets the custom request properties.
		/// </summary>
		/// <returns>The custom request properties.</returns>
		/// <param name="customheaders">Customheaders.</param>
		public void SetCustomRequestProperties(JObject customheaders)
		{
			this.customRequestProperties = customheaders;
		}

		/// <summary>
		/// Sets a specific custom request property from a Json object.
		/// </summary>
		/// <param name="key">Custom request property key</param>
		/// <param name="value">Custom request property value as a JObject</param>
		public void SetCustomRequestProperty(string key, JObject value)
		{
			if (this.customRequestProperties == null)
			{
				this.customRequestProperties = new JObject();
			}

			this.customRequestProperties.Add (key, value);
		}

		/// <summary>
		/// Sets a specific custom request property from a string.
		/// </summary>
		/// <param name="key">Custom request property key</param>
		/// <param name="value">Custom request property value as a string</param>
		public void SetCustomRequestProperty(string key, string value)
		{
			if (this.customRequestProperties == null)
			{
				this.customRequestProperties = new JObject();
			}

			this.customRequestProperties.Add (key, value);
		}

		/// <summary>
		/// Clears the currently saved custom request properties.
		/// </summary>
		public void ClearCustomRequestProperties()
		{
			this.customRequestProperties = new JObject();
		}

		/// <summary>
		/// Gets the custom request properties.
		/// </summary>
		/// <returns>The custom request properties.</returns>
		public JObject GetCustomRequestProperties()
		{
			return this.customRequestProperties;
		}
       
		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.AbstractKinveyClient"/> class.
		/// </summary>
		/// <param name="restClient">The REST client to be used for network requests.</param>
		/// <param name="rootUrl">The root URL of the backend service</param>
		/// <param name="servicePath">The service path</param>
        protected AbstractKinveyClient(HttpClient httpClient, string rootUrl, string servicePath)
            : this(httpClient, rootUrl, servicePath, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.AbstractKinveyClient"/> class.
		/// </summary>
		/// <param name="restClient">The REST client to be used for network requests.</param>
		/// <param name="rootUrl">The root URL of the backend service</param>
		/// <param name="servicePath">The service path</param>
		/// <param name="initializer">Kinvey request initializer</param>
		protected AbstractKinveyClient(HttpClient httpClient, string rootUrl, string servicePath, IKinveyRequestInitializer initializer)
        {
            this.httpClient = httpClient;
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
        public HttpClient HttpClient
        {
            get { return this.httpClient; }
        }

		/// <summary>
		/// Initializes the request.
		/// </summary>
		/// <param name="request">Request.</param>
		/// <typeparam name="T">The Type of the response</typeparam>
        public void InitializeRequest<T>(AbstractKinveyClientRequest<T> request, string clientID = null)
        {
            if (RequestInitializer != null) 
            {
                RequestInitializer.Initialize(request, clientID);
            }
        }

		/// <summary>
		/// Normalizes the root URL.
		/// </summary>
		/// <returns>The normalized root URL.</returns>
		/// <param name="rootUrl">Root URL.</param>
        private static string NormalizeRootUrl(string rootUrl)
        {
			if (!rootUrl.ToUpper().StartsWith("HTTPS", System.StringComparison.OrdinalIgnoreCase))
			{
				if (!rootUrl.ToLower().StartsWith("http://localhost:", System.StringComparison.OrdinalIgnoreCase) &&
				    !rootUrl.ToLower().StartsWith("http://127.0.0.1:", System.StringComparison.OrdinalIgnoreCase))
				{
					throw new KinveyException(EnumErrorCategory.ERROR_REQUIREMENT, EnumErrorCode.ERROR_REQUIREMENT_HTTPS, "");
				}
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

		/// <summary>
		/// Class which sets up the building of the <see cref="KinveyXamarin.AbstractKinveyClient"/> class.
		/// </summary>
		public abstract class Builder
        {
			/// <summary>
			/// The rest client.
			/// </summary>
            private HttpClient httpClient;

			/// <summary>
			/// The base URL.
			/// </summary>
            private string baseUrl;

            private string micHostName;

            /// <summary>
            /// The service path.
            /// </summary>
            private string servicePath;

			/// <summary>
			/// The kinvey request initializer.
			/// </summary>
            private KinveyClientRequestInitializer kinveyRequestInitializer;

			/// <summary>
			/// Initializes a new instance of the <see cref="KinveyXamarin.AbstractKinveyClient"/> Builder class.
			/// </summary>
			/// <param name="transport">Transport.</param>
			/// <param name="defaultRootUrl">Default root URL.</param>
			/// <param name="defaultServicePath">Default service path.</param>
			/// <param name="kinveyRequestInitializer">[optional] Kinvey request initializer.</param>
			public Builder(HttpClient transport, string defaultRootUrl, string defaultServicePath, KinveyClientRequestInitializer kinveyRequestInitializer = null)
            {
                this.httpClient = transport;
                BaseUrl = defaultRootUrl;
                ServicePath = defaultServicePath;
                this.kinveyRequestInitializer = kinveyRequestInitializer;
            }
				
			/// <summary>
			/// Gets the http rest client.
			/// </summary>
			/// <value>The http rest client.</value>
            public HttpClient HttpClient
            {
                get { return this.httpClient; }
				set { this.httpClient = value; }
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

            public string MICHostName
            {
                get { return this.micHostName; }
                set { this.micHostName = NormalizeRootUrl(value); }
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
