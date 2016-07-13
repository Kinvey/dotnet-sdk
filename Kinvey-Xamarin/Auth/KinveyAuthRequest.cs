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
using Newtonsoft.Json;
using System.Net.Http;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace KinveyXamarin
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
        private HttpBasicAuthenticator appKeyAuthentication;
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
        private static KinveyHeaders kinveyHeaders = new KinveyHeaders();

		public KinveyAuthRequest(){}

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KinveyAuthRequest"/> class.
		/// </summary>
		/// <param name="client">Client.</param>
		/// <param name="auth">authenticator to use.</param>
		/// <param name="appKey">App key.</param>
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		/// <param name="user">User.</param>
		/// <param name="create">If set to <c>true</c> create.</param>
		public KinveyAuthRequest(AbstractKinveyClient client, HttpBasicAuthenticator auth, string appKey, string username, string password, User user, bool create)
			
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
            this.create = create;
            this.uriTemplateParameters = new Dictionary<string,string>();
            this.uriTemplateParameters.Add("appKey", appKey);
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KinveyAuthRequest"/> class.
		/// </summary>
		/// <param name="client">Client.</param>
		/// <param name="baseUrl">Base URL.</param>
		/// <param name="auth">authenticator to use.</param>
		/// <param name="appKey">App key.</param>
		/// <param name="identity">The third party identity.</param>
		/// <param name="user">User.</param>
		/// <param name="create">If set to <c>true</c> create.</param>
		public KinveyAuthRequest(AbstractKinveyClient client, HttpBasicAuthenticator auth, string appKey, ThirdPartyIdentity identity, User user, bool create)

		{
			this.client = client;
			this.appKeyAuthentication = auth;
			this.identity = identity;
			this.create = create;
			this.uriTemplateParameters = new Dictionary<string,string>();
			this.uriTemplateParameters.Add("appKey", appKey);
		}

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
        private RestRequest BuildRestRequest() 
        {
		
			RestRequest restRequest = new RestRequest();
            if (this.requestPayload != null)
            {
				restRequest.AddParameter("application/json", JsonConvert.SerializeObject(this.requestPayload), ParameterType.RequestBody);
            }else if (this.identity != null) {
				restRequest.AddParameter("application/json", JsonConvert.SerializeObject(this.identity, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore}), ParameterType.RequestBody);
			}

            restRequest.Resource = "user/{appKey}/" + (this.create ? "" : "login");

			restRequest.Method = Method.POST;

            foreach (var parameter in uriTemplateParameters)
            {
                restRequest.AddParameter(parameter.Key, parameter.Value, ParameterType.UrlSegment);
            }

            foreach (var header in kinveyHeaders)
            {
				restRequest.AddHeader(header.Name, header.Value.FirstOrDefault());
            }

			appKeyAuthentication.Authenticate (restRequest);


            return restRequest;    
        }

		/// <summary>
		/// Initializes the rest client.
		/// </summary>
		/// <returns>The rest client.</returns>
        private IRestClient InitializeRestClient()
        {
			IRestClient restClient = this.client.RestClient;
			restClient.BaseUrl = client.BaseUrl;
			return restClient;
        }

		/// <summary>
		/// Executes the request async without parsing it.
		/// </summary>
		/// <returns>The unparsed async.</returns>
		public async Task<RestResponse> ExecuteUnparsedAsync()
		{
			IRestClient client = InitializeRestClient();
			RestRequest request = BuildRestRequest();

			var response = await client.ExecuteAsync(request);

			if ((int) response.StatusCode == 404 && this.create == false) { //if user is not found, create a new user
				this.create = true;
				return await ExecuteUnparsedAsync ();
			} else if (response.ErrorException != null || (int)response.StatusCode < 200 || (int) response.StatusCode >= 300 )
			{
				throw NewExceptionOnError(response);
			}

			return (RestResponse)response;
		}

		/// <summary>
		/// Executes this request async and parses the result.
		/// </summary>
		/// <returns>The async request.</returns>
		public async Task<KinveyAuthResponse> ExecuteAsync()
		{
			try
			{
				return JsonConvert.DeserializeObject<KinveyAuthResponse>((await ExecuteUnparsedAsync()).Content);
			}
			catch (KinveyJsonResponseException JSONException)
			{
				throw JSONException;
			}
			catch (Exception e)
			{
				throw new KinveyException(EnumErrorCode.ERROR_USER_LOGIN_ATTEMPT, "Error deserializing response content.");
			}
		}

		/// <summary>
		/// Throw an expection when an error occurs.
		/// </summary>
		/// <returns>The exception.</returns>
		/// <param name="response">Response.</param>
        protected KinveyJsonResponseException NewExceptionOnError(IRestResponse response)
        {
            return KinveyJsonResponseException.From(response);
        }

		/// <summary>
		/// Builder for an auth request.
		/// </summary>
        public class Builder
        {

			private readonly AbstractKinveyClient client;

            private readonly HttpBasicAuthenticator appKeyAuthentication;

            private bool create = false;

            private string username;

            private User user;

            private string password;


            private string appKey;

			private ThirdPartyIdentity identity;

			/// <summary>
			/// Initializes a new instance of the <see cref="KinveyXamarin.KinveyAuthRequest+Builder"/> class.
			/// </summary>
			/// <param name="transport">Transport.</param>
			/// <param name="baseUrl">Base URL.</param>
			/// <param name="appKey">App key.</param>
			/// <param name="appSecret">App secret.</param>
			/// <param name="user">User.</param>
			public Builder(AbstractKinveyClient transport, string appKey, string appSecret, User user)
            {
                this.client = transport;
                this.appKeyAuthentication = new HttpBasicAuthenticator(appKey, appSecret);
                this.appKey = appKey;
                this.user = user;
            }

			/// <summary>
			/// Initializes a new instance of the <see cref="KinveyXamarin.KinveyAuthRequest+Builder"/> class.
			/// </summary>
			/// <param name="transport">Transport.</param>
			/// <param name="baseUrl">Base URL.</param>
			/// <param name="appKey">App key.</param>
			/// <param name="appSecret">App secret.</param>
			/// <param name="username">Username.</param>
			/// <param name="password">Password.</param>
			/// <param name="user">User.</param>
			public Builder(AbstractKinveyClient transport, string appKey, string appSecret, string username, string password, User user)
                : this(transport, appKey, appSecret, user)
            {
				this.client = transport;
				this.appKeyAuthentication = new HttpBasicAuthenticator(appKey, appSecret);
				this.appKey = appKey;
                this.username = username;
                this.password = password;
				this.user = user;
            }


			public Builder(AbstractKinveyClient transport, string appKey, string appSecret, ThirdPartyIdentity identity, User user)
				: this(transport, appKey, appSecret, user)
			{
				this.identity = identity;

			}


			/// <summary>
			/// Build the Auth Request.
			/// </summary>
            public KinveyAuthRequest build()
            {
				if (identity == null) {
					return new KinveyAuthRequest (Client, AppKeyAuthentication, AppKey, Username, Password, KinveyUser, this.create);
				} else {
					return new KinveyAuthRequest (Client, AppKeyAuthentication, AppKey, identity, KinveyUser, this.create);
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

			public ThirdPartyIdentity Identity
			{
				get { return this.identity; }
				set { this.identity = value;}
			}

			/// <summary>
			/// Gets the app key authentication.
			/// </summary>
			/// <value>The app key authentication.</value>
            public HttpBasicAuthenticator AppKeyAuthentication
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
