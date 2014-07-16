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
using Kinvey.DotNet.Framework.Core;
using System.Net.Http;
using RestSharp;

namespace Kinvey.DotNet.Framework.Auth
{
    [JsonObject(MemberSerialization.OptIn)]
	public class KinveyAuthRequest
    {
        public enum LoginType
        {
            IMPLICIT,
            KINVEY,
            THIRDPARTY
        }

        private class AuthRequestPayload
        {
            [JsonProperty("username")]
            private string Username { get; set; }
            [JsonProperty("password")]
            private string Password { get; set; }
        }

        private bool create;
        private RestClient client;
        private string BaseUrl;
        private Dictionary<string, string> uriTemplateParameters;

        private HttpBasicAuthenticator appKeyAuthentication;

        private IAuthenticator requestPayload;
	
        private static KinveyHeaders kinveyHeaders = new KinveyHeaders();

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

        private RestRequest BuildRestRequest() 
        {
		
			RestRequest restRequest = new RestRequest();
            if (this.requestPayload != null)
            {
                restRequest.AddBody(JsonConvert.SerializeObject(this.requestPayload));
            }

            restRequest.Resource = BaseUrl + "user/{appKey}/" + (this.create ? "" : "login");
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

        private RestClient InitializeRestClient()
        {
            RestClient restClient = this.client;
            restClient.BaseUrl = client.BaseUrl;
            return restClient;
        }


		public async Task<RestResponse> ExecuteUnparsedAsync()
		{
			RestClient client = InitializeRestClient();
			RestRequest request = BuildRestRequest();

			var response = await client.ExecuteAsync(request);
//			var response = req.Result;

			if (response.ErrorException != null || (int)response.StatusCode < 200 || (int) response.StatusCode > 300 )
			{
				throw NewExceptionOnError(response);
			}

			return (RestResponse)response;
		}



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

		public KinveyAuthResponse Execute()
        {
			return JsonConvert.DeserializeObject<KinveyAuthResponse>( ExecuteUnparsed().Content);
        }


		public async Task<KinveyAuthResponse> ExecuteAsync()
		{
			return JsonConvert.DeserializeObject<KinveyAuthResponse>((await ExecuteUnparsedAsync()).Content);
		}

        protected KinveyJsonResponseException NewExceptionOnError(IRestResponse response)
        {
            return KinveyJsonResponseException.From(response);
        }

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

            public Builder(RestClient transport, string baseUrl, string appKey, string appSecret, User user)
            {
                this.client = transport;
                this.baseUrl = baseUrl;
                this.appKeyAuthentication = new HttpBasicAuthenticator(appKey, appSecret);
                this.appKey = appKey;
                this.user = user;
            }

            public Builder(RestClient transport, string baseUrl, string appKey, string appSecret, string username, string password, User user)
                : this(transport, baseUrl, appKey, appSecret, user)
            {
                this.username = username;
                this.password = password;
            }

            public KinveyAuthRequest build()
            {
                return new KinveyAuthRequest(Client, BaseUrl, AppKeyAuthentication, AppKey, Username, Password, KinveyUser, this.create);
            }

            public string Username
            {
                get { return this.username; }
                set { this.username = value; }
            }

            public string Password
            {
                get { return this.password; }
                set { this.password = value; }
            }

            public bool Create
            {
                get { return this.create; }
                set { this.create = value; }
            }

            public User KinveyUser
            {
                get { return this.user; }
                set { this.user = value; }
            }

            public RestClient Client
            {
                get { return this.client; }
            }

            public HttpBasicAuthenticator AppKeyAuthentication
            {
                get { return appKeyAuthentication; }
            }

            public string BaseUrl
            {
                get { return baseUrl; }
            }

            public string AppKey
            {
                get { return appKey; }
                set { this.appKey = value; } 
            }
        }

    }
}
