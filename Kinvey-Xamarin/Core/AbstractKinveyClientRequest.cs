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

namespace KinveyXamarin
{

	/// <summary>
	/// This is a client request to be sent to Kinvey
	/// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class AbstractKinveyClientRequest<T>
    {
		/// <summary>
		/// the Kinvey Client which created this request.
		/// </summary>
		protected readonly AbstractKinveyClient client;
		/// <summary>
		/// The request method.
		/// </summary>
		protected readonly string requestMethod;
		/// <summary>
		/// The request headers.
		/// </summary>
        private List<RestSharp.HttpHeader> requestHeaders = new List<HttpHeader>();
		/// <summary>
		/// The URI template.
		/// </summary>
		public string uriTemplate;
		/// <summary>
		/// The content of the request.
		/// </summary>
        private Object requestContent;
		/// <summary>
		/// The last response headers, in case the request is repeated.
		/// </summary>
        private List<Parameter> lastResponseHeaders = new List<Parameter>();
		/// <summary>
		/// The last response code.
		/// </summary>
        private int lastResponseCode = -1;
		/// <summary>
		/// The last response message.
		/// </summary>
        private string lastResponseMessage;
		/// <summary>
		/// The app key.
		/// </summary>
        private string appKey;
		/// <summary>
		/// The URI resource parameters.
		/// </summary>
		public Dictionary<string, string> uriResourceParameters;
		/// <summary>
		/// the authenticator.
		/// </summary>
        private IAuthenticator auth;

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.AbstractKinveyClientRequest`1"/> class.
		/// </summary>
		/// <param name="client">Client.</param>
		/// <param name="requestMethod">Request method.</param>
		/// <param name="uriTemplate">URI template.</param>
		/// <param name="httpContent">Http content.</param>
		/// <param name="uriParameters">URI parameters.</param>
		protected AbstractKinveyClientRequest(AbstractKinveyClient client, string requestMethod, string uriTemplate, Object httpContent, Dictionary<string, string> uriParameters)
        {
            this.client = client;
            this.requestMethod = requestMethod;
            this.uriTemplate = uriTemplate;
            this.requestContent = httpContent;
            this.uriResourceParameters = uriParameters;
            this.RequireAppCredentials = false;
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
		/// Gets the request method.
		/// </summary>
		/// <value>The request method.</value>
        public string RequestMethod
        {
            get { return this.requestMethod; }     
        }

		/// <summary>
		/// Gets or sets the request headers.
		/// </summary>
		/// <value>The request headers.</value>
        public List<HttpHeader> RequestHeaders
        {
            get { return this.requestHeaders; }
            set { this.requestHeaders = value; }
        }

		/// <summary>
		/// Gets or sets the content of the http.
		/// </summary>
		/// <value>The content of the http.</value>
        public Object HttpContent
        {
            get { return this.requestContent; }
			set { this.requestContent = value;}
        }

		/// <summary>
		/// Gets the last response headers.
		/// </summary>
		/// <value>The last response headers.</value>
        public List<Parameter> LastResponseHeaders
        {
            get { return this.lastResponseHeaders; }
        }

		/// <summary>
		/// Gets the last response code.
		/// </summary>
		/// <value>The last response code.</value>
        public int LastResponseCode
        {
            get { return this.lastResponseCode; }
        }

		/// <summary>
		/// Gets the last response message.
		/// </summary>
		/// <value>The last response message.</value>
        public string LastResponseMessage
        {
            get { return this.lastResponseMessage; }
    
        }

		/// <summary>
		/// Gets or sets the request authenticator.
		/// </summary>
		/// <value>The request auth.</value>
        public IAuthenticator RequestAuth
        {
            get { return auth; }
            set { auth = value; }
        }

		/// <summary>
		/// Gets or sets the app key.
		/// </summary>
		/// <value>The app key.</value>
        [JsonProperty("appKey")]
        public string AppKey
        {
            get { return this.appKey; }
            set { this.appKey = value; }
        }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="KinveyXamarin.AbstractKinveyClientRequest`1"/> requires app credentials or user credentials.
		/// </summary>
		/// <value><c>true</c> if require app level credentials; otherwise, <c>false</c>.</value>
        public bool RequireAppCredentials
        {
            get;
            set;
        }
			

		/// <summary>
		/// Builds the rest request.
		/// </summary>
		/// <returns>The rest request.</returns>
		public RestRequest BuildRestRequest() 
        {
			RestRequest restRequest = new RestRequest (uriTemplate);

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
				case "DELETE":
					restRequest.Method = Method.DELETE;
					break;
            }
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
			foreach (var parameter in uriResourceParameters)
			{
				restRequest.AddParameter(parameter.Key, parameter.Value, ParameterType.UrlSegment);
			}
				
			auth.Authenticate (restRequest);
            return restRequest;           
        }

		/// <summary>
		/// Initializes the rest client.
		/// </summary>
		/// <returns>The rest client.</returns>
        private RestClient InitializeRestClient()
        {
            RestClient restClient = this.client.RestClient;
            restClient.BaseUrl = client.BaseUrl;
            return restClient;
        }

		/// <summary>
		/// returns an Exception if Kinvey returns an error.
		/// </summary>
		/// <returns>The exception on error.</returns>
		/// <param name="response">Response.</param>
        protected KinveyJsonResponseException NewExceptionOnError(IRestResponse response)
        {
            return KinveyJsonResponseException.From(response);
        }

		/// <summary>
		/// Executes the request without any parsing.
		/// </summary>
		/// <returns>The unparsed.</returns>
        public RestResponse ExecuteUnparsed()
        {
            RestClient client = InitializeRestClient();
            RestRequest request = BuildRestRequest();

			ClientLogger.Log (request);

            client.Authenticator = RequestAuth;

			var req = client.ExecuteAsync(request);
			var response = req.Result;

			ClientLogger.Log (response);

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


		/// <summary>
		/// Execute this request.
		/// </summary>
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
