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
using System.IO;
using RestSharp;
using System.Net.Http;
using KinveyXamarin;

namespace Kinvey.DotNet.Framework.Core
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AbstractKinveyClientRequest<T>
    {
		protected readonly AbstractKinveyClient client;
		protected readonly string requestMethod;
        private List<RestSharp.HttpHeader> requestHeaders = new List<HttpHeader>();
		public string uriTemplate;
        private T requestContent;
        private List<Parameter> lastResponseHeaders = new List<Parameter>();
        private int lastResponseCode = -1;
        private string lastResponseMessage;
        private string appKey;
		public Dictionary<string, string> uriResourceParameters;
        private IAuthenticator auth;



        protected AbstractKinveyClientRequest(AbstractKinveyClient client, string requestMethod, string uriTemplate, T httpContent, Dictionary<string, string> uriParameters)
        {
            this.client = client;
            this.requestMethod = requestMethod;
            this.uriTemplate = uriTemplate;
            this.requestContent = httpContent;
            this.uriResourceParameters = uriParameters;
            this.RequireCredentials = false;
        }

        public AbstractKinveyClient Client
        {
            get { return this.client; }
        }

        public string RequestMethod
        {
            get { return this.requestMethod; }     
        }

        public List<HttpHeader> RequestHeaders
        {
            get { return this.requestHeaders; }
            set { this.requestHeaders = value; }
        }

        public T HttpContent
        {
            get { return this.requestContent; }
			set { this.requestContent = value;}
        }

        public List<Parameter> LastResponseHeaders
        {
            get { return this.lastResponseHeaders; }
        }

        public int LastResponseCode
        {
            get { return this.lastResponseCode; }
        }

        public string LastResponseMessage
        {
            get { return this.lastResponseMessage; }
    
        }

        public IAuthenticator RequestAuth
        {
            get { return auth; }
            set { auth = value; }
        }

        [JsonProperty("appKey")]
        public string AppKey
        {
            get { return this.appKey; }
            set { this.appKey = value; }
        }

        public bool RequireCredentials
        {
            get;
            set;
        }

		public RestRequest BuildRestRequest() 
        {
            RestRequest restRequest = new RestRequest();

            switch (requestMethod)
            {
                case "GET":
					restRequest.Method = Method.GET;
                    break;
				case "POST":
					restRequest.Method = Method.POST;
                    break;
				case "PUT":
					restRequest.Method = Method.PUT;
					break;
            }
			//TODO WTF
			if (this.HttpContent == null && requestMethod.Equals(HttpMethod.Post) )
            {
                restRequest.AddBody(new object());
            }
            else
            {

                restRequest.AddParameter("application/json", JsonConvert.SerializeObject(HttpContent), ParameterType.RequestBody);

            }
            foreach (var header in requestHeaders)
            {
				restRequest.AddHeader(header.Name, header.Value.FirstOrDefault());
            }
            restRequest.Resource = uriTemplate;
            
            foreach (var parameter in uriResourceParameters)
            {
                restRequest.AddParameter(parameter.Key, parameter.Value, ParameterType.UrlSegment);
            }

//			restRequest.

			auth.Authenticate (restRequest);
            return restRequest;           
        }

        private RestClient InitializeRestClient()
        {
            RestClient restClient = this.client.RestClient;
            restClient.BaseUrl = client.BaseUrl;
            return restClient;
        }

        protected KinveyJsonResponseException NewExceptionOnError(IRestResponse response)
        {
            return KinveyJsonResponseException.From(response);
        }

        public RestResponse ExecuteUnparsed()
        {
            RestClient client = InitializeRestClient();
            RestRequest request = BuildRestRequest();

            client.Authenticator = RequestAuth;

			var req = client.ExecuteAsync(request);
			var response = req.Result;

            lastResponseCode = (int)response.StatusCode;
            lastResponseMessage = response.StatusDescription;
            lastResponseHeaders = new List<Parameter>();
            foreach (var header in response.Headers)
            {
                lastResponseHeaders.Add(header);
            }


            if (response.ErrorException != null)
            {
                throw NewExceptionOnError(response);
            }


            return (RestResponse) response;
        }


        public virtual T Execute()
        {
            var response = ExecuteUnparsed();

            // special case to handle void or empty responses
			if (response.Content == null) 
            {
                return default(T);
            }
            try
            {
				return JsonConvert.DeserializeObject<T>(response.Content);
            }
//			catch(Exception isArray){
//				try{
//					return JsonConvert.DeserializeObject<T[]>(response.Content);
//
//				}catch (Exception nope){
//					ClientLogger.Log (nope.Message);
//					return default(T);
//				}
//
//
//			}

            catch(ArgumentException ex)
            {
				ClientLogger.Log (ex.Message);
                return default(T);
            }
            catch (NullReferenceException ex)
            {
				ClientLogger.Log (ex.Message);
                return default(T);
            }

        }
    }
}
