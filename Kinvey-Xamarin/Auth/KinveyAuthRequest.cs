// Copyright (c) 2014, Kinvey, Inc. All rights reserved.
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

namespace KinveyXamarin
{

	/// <summary>
	/// Kinvey auth request, used for creation/login and setting the session on the client.
	/// </summary>
    [JsonObject(MemberSerialization.OptIn)]
	public class KinveyAuthRequest
    {
		/// <summary>
		/// Login type.
		/// </summary>
        public enum LoginType
        {
            IMPLICIT,
            KINVEY,
            THIRDPARTY
        }

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
        private RestClient client;
		/// <summary>
		/// The base URL of the request.
		/// </summary>
        private string BaseUrl;
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
        private IAuthenticator requestPayload;

		/// <summary>
		/// The third party identity, if there is one.
		/// </summary>
		private ThirdPartyIdentity identity;
	
		/// <summary>
		/// The kinvey headers.
		/// </summary>
        private static KinveyHeaders kinveyHeaders = new KinveyHeaders();

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KinveyAuthRequest"/> class.
		/// </summary>
		/// <param name="client">Client.</param>
		/// <param name="baseUrl">Base URL.</param>
		/// <param name="auth">authenticator to use.</param>
		/// <param name="appKey">App key.</param>
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		/// <param name="user">User.</param>
		/// <param name="create">If set to <c>true</c> create.</param>
        public KinveyAuthRequest(RestClient client, string baseUrl, HttpBasicAuthenticator auth, string appKey, string username, string password, User user, bool create)
			
		{
            this.client = client;
            this.BaseUrl = baseUrl;
            this.appKeyAuthentication = auth;
            this.requestPayload = (username == null || password == null) ? null : new HttpBasicAuthenticator(username, password);
            if (user != null)
            {
                // TODO Add properties of user
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
		public KinveyAuthRequest(RestClient client, string baseUrl, HttpBasicAuthenticator auth, string appKey, ThirdPartyIdentity identity, User user, bool create)

		{
			this.client = client;
			this.BaseUrl = baseUrl;
			this.appKeyAuthentication = auth;
			this.identity = identity;
			if (user != null)
			{
				// TODO Add properties of user
			}
			this.create = create;
			this.uriTemplateParameters = new Dictionary<string,string>();
			this.uriTemplateParameters.Add("appKey", appKey);
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
                restRequest.AddBody(JsonConvert.SerializeObject(this.requestPayload));
            }else if (this.identity != null) {
				restRequest.AddBody (JsonConvert.SerializeObject(this.requestPayload));
			}

            restRequest.Resource = "user/{appKey}/" + (this.create ? "" : "login");
			restRequest.Method = this.create ? Method.POST : Method.PUT;

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
        private RestClient InitializeRestClient()
        {
            RestClient restClient = this.client;
            restClient.BaseUrl = client.BaseUrl;
            return restClient;
        }

		/// <summary>
		/// Executes the request async without parsing it.
		/// </summary>
		/// <returns>The unparsed async.</returns>
		public async Task<RestResponse> ExecuteUnparsedAsync()
		{
			RestClient client = InitializeRestClient();
			RestRequest request = BuildRestRequest();

			var response = await client.ExecuteAsync(request);

			if (response.ErrorException != null || (int)response.StatusCode < 200 || (int) response.StatusCode > 300 )
			{
				throw NewExceptionOnError(response);
			}

			return (RestResponse)response;
		}


		/// <summary>
		/// Executes the request without parsing it.
		/// </summary>
		/// <returns>The unparsed.</returns>
        public RestResponse ExecuteUnparsed()
        {
            RestClient client = InitializeRestClient();
            RestRequest request = BuildRestRequest();

			var req = client.ExecuteAsync(request);
			var response = req.Result;

			if (response.ErrorException != null || (int)response.StatusCode < 200 || (int) response.StatusCode > 300 )
            {
                throw NewExceptionOnError(response);
            }

            return (RestResponse)response;
        }

		/// <summary>
		/// Executes this request and parses the result.
		/// </summary>
		public KinveyAuthResponse Execute()
        {
			return JsonConvert.DeserializeObject<KinveyAuthResponse>( ExecuteUnparsed().Content);
        }

		/// <summary>
		/// Executes this request async and parses the result.
		/// </summary>
		/// <returns>The async request.</returns>
		public async Task<KinveyAuthResponse> ExecuteAsync()
		{
			return JsonConvert.DeserializeObject<KinveyAuthResponse>((await ExecuteUnparsedAsync()).Content);
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

            private readonly RestClient client;

            private readonly HttpBasicAuthenticator appKeyAuthentication;

            private bool create = false;

            private string username;

            private User user;

            private string password;

            private string baseUrl;

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
            public Builder(RestClient transport, string BaseUrl, string appKey, string appSecret, User user)
            {
                this.client = transport;
                this.baseUrl = BaseUrl;
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
            public Builder(RestClient transport, string BaseUrl, string appKey, string appSecret, string username, string password, User user)
                : this(transport, BaseUrl, appKey, appSecret, user)
            {
                this.username = username;
                this.password = password;
            }


			public Builder(RestClient transport, string BaseUrl, string appKey, string appSecret, ThirdPartyIdentity identity, User user)
				: this(transport, BaseUrl, appKey, appSecret, user)
			{
				this.identity = identity;

			}


			/// <summary>
			/// Build the Auth Request.
			/// </summary>
            public KinveyAuthRequest build()
            {
				if (identity == null) {
					return new KinveyAuthRequest (Client, BaseUrl, AppKeyAuthentication, AppKey, Username, Password, KinveyUser, this.create);
				} else {
					return new KinveyAuthRequest (Client, BaseUrl, AppKeyAuthentication, AppKey, identity, KinveyUser, this.create);
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
            public RestClient Client
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
			/// Gets the base URL.
			/// </summary>
			/// <value>The base URL.</value>
            public string BaseUrl
            {
                get { return baseUrl; }
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
