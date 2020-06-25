// Copyright (c) 2019, Kinvey, Inc. All rights reserved.
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
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Kinvey
{

	/// <summary>
	/// Kinvey auth request, used for creation/login and setting the session on the client.
	/// </summary>
    [JsonObject(MemberSerialization.OptIn)]
	public class KinveyAuthRequest
    {
		/// <summary>
		/// Auth request payload
		/// </summary>
        private class AuthRequestPayload
        {
            [JsonProperty("username")]
            private string Username { get; set; }
            [JsonProperty("password")]
            private string Password { get; set; }
        }

		/// <summary>
		/// Is this a create request?
		/// </summary>
        private bool create;
		/// <summary>
		/// The RestSharp client
		/// </summary>
		private AbstractKinveyClient client;

		/// <summary>
		/// The URI template parameters.
		/// </summary>
        private Dictionary<string, string> uriTemplateParameters;

		/// <summary>
		/// The authenticator of the request, create/login use appkey authentication.
		/// </summary>
        private IAuthenticator appKeyAuthentication;
		/// <summary>
		/// The payload of the request.
		/// </summary>
        private JObject requestPayload;

		/// <summary>
		/// The third party identity, if there is one.
		/// </summary>
		private ThirdPartyIdentity identity;
	
		/// <summary>
		/// The kinvey headers.
		/// </summary>
        private static KinveyHeaders kinveyHeaders = new KinveyHeaders(Client.SharedClient.DevicePlatform);

        /// <summary>
        /// Initializes a new instance of the <see cref="KinveyAuthRequest"/> class.
        /// </summary>
        public KinveyAuthRequest(){}

        /// <summary>
        /// Initializes a new instance of the <see cref="KinveyAuthRequest"/> class.
        /// </summary>
        /// <param name="client">Client.</param>
        /// <param name="auth">authenticator to use.</param>
        /// <param name="appKey">App key.</param>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="customFieldsAndValues">Custom fields and values.</param>
        /// <param name="user">User.</param>
        /// <param name="create">If set to <c>true</c> create.</param>
        public KinveyAuthRequest(AbstractKinveyClient client, IAuthenticator auth, string appKey, string username, string password, Dictionary<string, JToken> customFieldsAndValues, User user, bool create)
			
		{
            this.client = client;
            this.appKeyAuthentication = auth;
			if (username != null && password != null) {
				this.requestPayload = new JObject ();
				this.requestPayload ["username"] = username;
				this.requestPayload ["password"] = password;
			}
            if (user != null)
            {
				if (this.requestPayload == null) {
					this.requestPayload = new JObject ();
				}
				var keys = user.Properties().Select(p => p.Name).ToList();
				foreach (string key in keys) {
					this.requestPayload.Add (key, user [key]);	
				}

				foreach (KeyValuePair<string, JToken> entry in user.Attributes) {
					this.requestPayload.Add(entry.Key, entry.Value);
				}

			}

			if (customFieldsAndValues != null)
			{
				foreach (KeyValuePair<string, JToken> entry in customFieldsAndValues)
				{
					this.requestPayload.Add(entry.Key, entry.Value);
				}
			}

			this.create = create;
            this.uriTemplateParameters = new Dictionary<string,string>();
            this.uriTemplateParameters.Add("appKey", appKey);
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyAuthRequest"/> class.
		/// </summary>
		/// <param name="client">Client.</param>
		/// <param name="baseUrl">Base URL.</param>
		/// <param name="auth">authenticator to use.</param>
		/// <param name="appKey">App key.</param>
		/// <param name="identity">The third party identity.</param>
		/// <param name="user">User.</param>
		/// <param name="create">If set to <c>true</c> create.</param>
        public KinveyAuthRequest(AbstractKinveyClient client, IAuthenticator auth, string appKey, ThirdPartyIdentity identity, User user, bool create)

		{
			this.client = client;
			this.appKeyAuthentication = auth;
			this.identity = identity;
			this.create = create;
			this.uriTemplateParameters = new Dictionary<string,string>();
			this.uriTemplateParameters.Add("appKey", appKey);
		}

        /// <summary>
		/// Builds payload for request.
		/// </summary>
		public void buildRequestPayload()
		{
			if (identity.provider.kinveyAuth.accessToken != null) {
				var kinveyAuth = new JObject ();
				kinveyAuth ["access_token"] = identity.provider.kinveyAuth.accessToken;

				var socialIdentity = new JObject ();
				socialIdentity ["kinveyAuth"] = kinveyAuth;

				this.requestPayload = new JObject ();
				this.requestPayload ["_socialIdentity"] = socialIdentity;
			}
		}

		/// <summary>
		/// Builds the rest request.
		/// </summary>
		/// <returns>The rest request.</returns>
        private HttpRequestMessage BuildRestRequest() 
        {

            HttpRequestMessage request = new HttpRequestMessage();
            string body = null;
            if (this.requestPayload != null)
            {
                body = JsonConvert.SerializeObject(this.requestPayload);
            }
            else if (this.identity != null)
            {
                body = JsonConvert.SerializeObject(this.identity, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			}
            if (body != null)
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }

            var userEndpoint = this.create ? "" : "login";
            request.RequestUri = new Uri(new Uri(client.BaseUrl), $"user/{uriTemplateParameters["appKey"]}/{userEndpoint}");

            request.Method = HttpMethod.Post;

            foreach (var header in kinveyHeaders)
            {
                var key = header.Key;
                var value = header.Value.FirstOrDefault();

                if (key.Equals(Constants.STR_REQUEST_HEADER_API_VERSION) && !string.IsNullOrEmpty(client.ApiVersion) && !value.Equals(client.ApiVersion))
                {
                    value = client.ApiVersion;
                }
                request.Headers.Add(key, value);
            }

            appKeyAuthentication.Authenticate(request);

            return request;    
        }

		/// <summary>
		/// Initializes the rest client.
		/// </summary>
		/// <returns>The rest client.</returns>
        private HttpClient InitializeHttpClient()
        {
            var httpClient = this.client.HttpClient;
            return httpClient;
        }

		/// <summary>
		/// Executes the request async without parsing it.
		/// </summary>
		/// <returns>The unparsed async.</returns>
		public async Task<HttpResponseMessage> ExecuteUnparsedAsync()
		{
			var client = InitializeHttpClient();
			var request = BuildRestRequest();
            Logger.Log(request);
            var response = await client.SendAsync(request).ConfigureAwait(false);
            Logger.Log(response);
            if (response.StatusCode == HttpStatusCode.NotFound && this.create == false)
            { //if user is not found, create a new user
				this.create = true;
				return await ExecuteUnparsedAsync ().ConfigureAwait(false);
			}
            try
            {
                response.EnsureSuccessStatusCodeWithoutDispose();
            }
            catch (Exception ex)
            {
                throw new KinveyException(
                    EnumErrorCategory.ERROR_BACKEND,
                    EnumErrorCode.ERROR_JSON_RESPONSE,
                    response,
                    ex
                );
            }

            return response;
		}
		/// <summary>
		/// Executes this request async and parses the result.
		/// </summary>
		/// <returns>The async request.</returns>
		public async Task<KinveyAuthResponse> ExecuteAsync()
		{
            string json = null;
            HttpResponseMessage response = null;
			try
			{
                response = await ExecuteUnparsedAsync().ConfigureAwait(false);
                json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				return JsonConvert.DeserializeObject<KinveyAuthResponse>(json);
			}
            catch (JsonException ex)
            {
                KinveyException kinveyException = new KinveyException(
                    EnumErrorCategory.ERROR_DATASTORE_NETWORK,
                    EnumErrorCode.ERROR_JSON_PARSE,
                    HelperMethods.GetCustomParsingJsonErrorMessage(json, response?.RequestMessage.RequestUri.ToString(), typeof(KinveyAuthResponse).FullName),
                    null,
                    ex
                )
                {
                    RequestID = HelperMethods.getRequestID(response)
                };
                throw kinveyException;
            }
            catch (KinveyException)
			{
				throw;
			}           
            catch (Exception ex)
			{
				throw new KinveyException(
                    EnumErrorCategory.ERROR_USER,
                    EnumErrorCode.ERROR_USER_LOGIN_ATTEMPT,
                    "Error deserializing response content.",
                    ex
                );
			}
		}

		/// <summary>
		/// Builder for an auth request.
		/// </summary>
        public class Builder
        {

			private readonly AbstractKinveyClient client;

            private readonly IAuthenticator appKeyAuthentication;

            private bool create = false;

            private string username;

            private User user;

            private string password;

			private Dictionary<string, JToken> customFieldsAndValues;

            private string appKey;

			private ThirdPartyIdentity identity;

            /// <summary>
            /// Initializes a new instance of the class.
            /// </summary>
            /// <param name="transport">Transport.</param>
            /// <param name="baseUrl">Base URL.</param>
            /// <param name="appKey">App key.</param>
            /// <param name="appSecret">App secret.</param>
            /// <param name="user">[optional] User.</param>
            public Builder(AbstractKinveyClient transport, string appKey, string appSecret, User user = null)
            {
                this.client = transport;
                this.appKeyAuthentication = new HttpBasicAuthenticator(appKey, appSecret);
                this.appKey = appKey;
                this.user = user;
            }

            /// <summary>
            /// Initializes a new instance of the class.
            /// </summary>
            /// <param name="transport">Transport.</param>
            /// <param name="appKey">App key.</param>
            /// <param name="appSecret">App secret.</param>
            /// <param name="username">Username.</param>
            /// <param name="password">Password.</param>
            /// <param name="customFieldsAndValues">[optional] Custom fields and values.</param>
            /// <param name="user">[optional] User.</param>
            public Builder(AbstractKinveyClient transport, string appKey, string appSecret, string username, string password, Dictionary<string, JToken> customFieldsAndValues = null, User user = null)
                : this(transport, appKey, appSecret, user)
            {
				this.client = transport;
				this.appKeyAuthentication = new HttpBasicAuthenticator(appKey, appSecret);
				this.appKey = appKey;
                this.username = username;
                this.password = password;
				this.customFieldsAndValues = customFieldsAndValues;
				this.user = user;
            }

            /// <summary>
            /// Initializes a new instance of the class.
            /// </summary>
            /// <param name="transport">Transport.</param>
            /// <param name="appKey">App key.</param>
            /// <param name="appSecret">App secret.</param>
            /// <param name="identity">Third party identity.</param>
            /// <param name="user">[optional] Kinvey user.</param>
			public Builder(AbstractKinveyClient transport, string appKey, string appSecret, ThirdPartyIdentity identity, User user = null)
				: this(transport, appKey, appSecret, user)
			{
				this.identity = identity;
			}

            /// <summary>
            /// Build the Auth Request.
            /// </summary>
            /// <returns>Built Kinvey auth request.</returns>
            public KinveyAuthRequest build()
            {
				if (identity == null)
				{
					return new KinveyAuthRequest(
                        Client,
                        AppKeyAuthentication,
                        AppKey,
                        Username,
                        Password,
                        CustomFieldsAndValues,
                        KinveyUser,
                        this.create
                    );
				}
				else
				{
					return new KinveyAuthRequest(Client, AppKeyAuthentication, AppKey, identity, KinveyUser, this.create);
				}
            }

			/// <summary>
			/// Gets or sets the username.
			/// </summary>
			/// <value>The username.</value>
            public string Username
            {
                get { return this.username; }
                set { this.username = value; }
            }

			/// <summary>
			/// Gets or sets the password.
			/// </summary>
			/// <value>The password.</value>
            public string Password
            {
                get { return this.password; }
                set { this.password = value; }
            }

			/// <summary>
			/// Gets or sets a value indicating whether this <see cref="KinveyXamarin.KinveyAuthRequest+Builder"/> is create.
			/// </summary>
			/// <value><c>true</c> if create; otherwise, <c>false</c>.</value>
            public bool Create
            {
                get { return this.create; }
                set { this.create = value; }
            }

            /// <summary>
			/// The CustomFieldsAndValues property represents custom fields and values for Kinvey auth request.
			/// </summary>
			/// <value>The CustomFieldsAndValues property gets/sets the value of the Dictionary field, customFieldsAndValues.</value>
			public Dictionary<string, JToken> CustomFieldsAndValues
			{
				get { return customFieldsAndValues; }
				set { this.customFieldsAndValues = value; }
			}

			/// <summary>
			/// Gets or sets the kinvey user.
			/// </summary>
			/// <value>The kinvey user.</value>
            public User KinveyUser
            {
                get { return this.user; }
                set { this.user = value; }
            }

			/// <summary>
			/// Gets the client.
			/// </summary>
			/// <value>The client.</value>
			public AbstractKinveyClient Client
            {
                get { return this.client; }
            }

            /// <summary>
            /// The Identity property represents the third party identity.
            /// </summary>
            /// <value>The Identity property gets/sets the value of the ThirdPartyIdentity field, identity.</value>
            public ThirdPartyIdentity Identity
			{
				get { return this.identity; }
				set { this.identity = value;}
			}

			/// <summary>
			/// Gets the app key authentication.
			/// </summary>
			/// <value>The app key authentication.</value>
            public IAuthenticator AppKeyAuthentication
            {
                get { return appKeyAuthentication; }
            }

			/// <summary>
			/// Gets or sets the app key.
			/// </summary>
			/// <value>The app key.</value>
            public string AppKey
            {
                get { return appKey; }
                set { this.appKey = value; } 
            }
        }

    }
}
