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
    /// This is a client request to be sent to Kinvey
    /// </summary>
    /// <typeparam name="T">The type of Kinvey client request and response.</typeparam>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class AbstractKinveyClientRequest<T>
    {
        #region Properties and constructors
        /// <summary>
        /// The Kinvey Client which created this request.
        /// </summary>
        /// <value> The object of the class inherited from AbstractClient. </value>.
        protected readonly AbstractClient client;
        /// <summary>
        /// The request method.
        /// </summary>
        /// <value> The string value containing a request method. </value>.
        protected readonly string requestMethod;
		/// <summary>
		/// The request headers.
		/// </summary>
        private List<KeyValuePair<string, IEnumerable<string>>> requestHeaders = new List<KeyValuePair<string, IEnumerable<string>>>();
        /// <summary>
        /// The URI template.
        /// </summary>
        /// <value> The string value containing uri template. </value>.
        public string uriTemplate;
		/// <summary>
		/// The content of the request.
		/// </summary>
        private Object requestContent;
		/// <summary>
		/// The last response headers, in case the request is repeated.
		/// </summary>
        private List<KeyValuePair<string, IEnumerable<string>>> lastResponseHeaders = new List<KeyValuePair<string, IEnumerable<string>>>();
		/// <summary>
		/// The last response code.
		/// </summary>
        private HttpStatusCode lastResponseCode;
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
        /// <value> The dictionary type value containing resource parameters for uri. </value>.
        public Dictionary<string, string> uriResourceParameters;
		/// <summary>
		/// the authenticator.
		/// </summary>
        private IAuthenticator auth;

        //TODO: this needs to be removed and instead the client should be passed around
        //public String clientAppVersion { get; set;}

        /// <summary>
        /// Custom headers for the request.	        
        /// </summary>
        /// <value>The customRequestHeaders property gets/sets the value of the JObject field, _customRequestHeaders. </value>	        
		public JObject customRequestHeaders {get; set;}

		/// <summary>
		/// The base URL for this request
		/// </summary>
		/// <value>The base URL.</value>
		private string baseURL {get; set;}

		/// <summary>
		/// Used for MIC to indicate if a request has been repeated after getting a refresh token
		/// </summary>
		private bool hasRetried = false;

        /// <summary>
        /// Should the request intercept redirects and route them to an override
        /// </summary>
        /// <value>The OverrideRedirect property gets/sets the value of the bool type. If the value is <c>true</c> then the request overrides redirects.</value>
        public bool OverrideRedirect {get; set; }
	
		/// <summary>
		/// The type of payload
		/// </summary>
		/// <value>The type of the payload.</value>
		public RequestPayloadType PayloadType { get; set;}

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractKinveyClientRequest<T>"/> class.
        /// </summary>
        /// <param name="client">Client.</param>
        /// <param name="requestMethod">Request method.</param>
        /// <param name="uriTemplate">URI template.</param>
        /// <param name="httpContent">Http content.</param>
        /// <param name="uriParameters">URI parameters.</param>
        protected AbstractKinveyClientRequest (AbstractClient client, string requestMethod, string uriTemplate, Object httpContent, Dictionary<string, string> uriParameters) :
		this (client, client.BaseUrl, requestMethod, uriTemplate, httpContent, uriParameters)
		{}

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractKinveyClientRequest<T>"/> class.
        /// </summary>
        /// <param name="client">Client.</param>
        /// <param name="baseURL">Base URL.</param>
        /// <param name="requestMethod">Request method.</param>
        /// <param name="uriTemplate">URI template.</param>
        /// <param name="httpContent">Http content.</param>
        /// <param name="uriParameters">URI parameters.</param>
        protected AbstractKinveyClientRequest(AbstractClient client, string baseURL, string requestMethod, string uriTemplate, Object httpContent, Dictionary<string, string> uriParameters)
        {
            this.client = client;
            this.requestMethod = requestMethod;
            this.uriTemplate = uriTemplate;
            this.requestContent = httpContent;
            this.uriResourceParameters = uriParameters;
            this.RequireAppCredentials = false;
			this.customRequestHeaders = client.GetCustomRequestProperties();
			this.baseURL = baseURL;
			this.PayloadType = new JSONPayload();
			this.OverrideRedirect = false;
        }

		/// <summary>
		/// Gets the client.
		/// </summary>
		/// <value>The client.</value>
        public AbstractClient Client
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
        public List<KeyValuePair<string, IEnumerable<string>>> RequestHeaders
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
        public List<KeyValuePair<string, IEnumerable<string>>> LastResponseHeaders
        {
            get { return this.lastResponseHeaders; }
        }

		/// <summary>
		/// Gets the last response code.
		/// </summary>
		/// <value>The last response code.</value>
        public HttpStatusCode LastResponseCode
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
		/// Start time of the request.
		/// </summary>
		/// <value>The string value containing the start time of the request.</value>
        public string RequestStartTime { get; set; }

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

		#endregion

		#region Request building
		/// <summary>
		/// Builds the rest request.
		/// </summary>
		/// <returns>The rest request.</returns>
		public HttpRequestMessage BuildRestRequest() 
        {
            var uri = uriTemplate;
            foreach (var keyValuePair in uriResourceParameters)
            {
                uri = uri.Replace($"{{{keyValuePair.Key}}}", keyValuePair.Value);
            }
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(new Uri(this.baseURL), uri)
            };

            switch (requestMethod)
            {
                case "GET":
                    request.Method = HttpMethod.Get;
                    break;
				case "POST":
                    request.Method = HttpMethod.Post;
                    break;
				case "PUT":
                    request.Method = HttpMethod.Put;
					break;
				case "DELETE":
                    request.Method = HttpMethod.Delete;
					break;
            }


			if (this.HttpContent == null && requestMethod.Equals (HttpMethod.Post)) {
                request.Content = new StringContent(string.Empty);
			} else if (this.HttpContent == null ) {
				//don't add a request body
			}
            else
            {
                request.Content = new StringContent(PayloadType.getHttpContent(HttpContent), Encoding.UTF8, PayloadType.getContentType());
            }

            foreach (var header in requestHeaders)
            {
                if (header.Key.Equals(Constants.STR_REQUEST_HEADER_API_VERSION) && !string.IsNullOrEmpty(client.ApiVersion) && !header.Value.FirstOrDefault().Equals(client.ApiVersion))
                {
                    request.Headers.Add(header.Key, client.ApiVersion);
                    continue;
                }

                if (header.Value.Count() > 1)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                else
                {
                    request.Headers.Add(header.Key, header.Value.FirstOrDefault());
                }
            }

			// check to see if Kinvey content type needs to be added for GCS Upload
			if ((this.requestContent != null) &&
				(this.requestContent.GetType() == typeof(FileMetaData))) {
				string mimetype = ((FileMetaData)this.requestContent).mimetype;

				if (!string.IsNullOrEmpty(mimetype)) {
                    request.Headers.Add("X-Kinvey-Content-Type", mimetype);
				}
			}

			if (!string.IsNullOrEmpty(client.GetClientAppVersion())) {
                request.Headers.Add("X-Kinvey-Client-App-Version", client.GetClientAppVersion ());
			}
			if (client.GetCustomRequestProperties () != null && client.GetCustomRequestProperties ().Count > 0) {
				string jsonHeaders = JsonConvert.SerializeObject (this.customRequestHeaders);
				if (Encoding.UTF8.GetByteCount(jsonHeaders) < 2000){
					request.Headers.Add("X-Kinvey-Custom-Request-Properties", jsonHeaders);
				}else{
					throw new KinveyException(EnumErrorCategory.ERROR_REQUIREMENT, EnumErrorCode.ERROR_REQUIREMENT_CUSTOM_REQUEST_PROPERTY_LIMIT, "");
				}

			}
				
			auth.Authenticate (request);
            return request;           
        }

		/// <summary>
		/// Initializes the rest client.
		/// </summary>
		/// <returns>The rest client.</returns>
        private HttpClient InitializeRestClient()
        {
            HttpClient httpClient;
            if (OverrideRedirect)
            {
                var httpClientHandler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                };
                httpClient = new HttpClient(httpClientHandler)
                {
                    BaseAddress = new Uri(this.baseURL)
                };
            }
            else
            {
                httpClient = this.client.HttpClient;
            }

            return httpClient;
        }

		#endregion

		#region Request execution
		/// <summary>
		/// Executes the request without any parsing.
		/// </summary>
		/// <returns>The unparsed.</returns>
        public HttpResponseMessage ExecuteUnparsed()
        {
            var client = InitializeRestClient();
            var request = BuildRestRequest();

            RequestAuth.Authenticate(request);
            Logger.Log(request);
            var req = client.SendAsync(request);
			var response = req.Result;
            Logger.Log(response);
            var contentType = response.Headers
                                      .Where(x => x.Key.ToLower().Equals("content-type"))
                                      .Select(x => x.Value)
                                      .SingleOrDefault();
            if (contentType != null && contentType.Any() && !contentType.First().Contains( "application/json")) {
                var kinveyException = new KinveyException(
                    EnumErrorCategory.ERROR_REQUIREMENT,
                    EnumErrorCode.ERROR_REQUIREMENT_CONTENT_TYPE_HEADER,
                    contentType.FirstOrDefault()
                )
                {
                    RequestID = HelperMethods.getRequestID(response)
                };
                throw kinveyException;
			}

            lastResponseCode = response.StatusCode;
            lastResponseMessage = response.StatusCode.ToString();
            lastResponseHeaders = new List<KeyValuePair<string, IEnumerable<string>>>();
            foreach (var header in response.Headers)
            {
                lastResponseHeaders.Add(header);
            }

			try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new KinveyException(
                    EnumErrorCategory.ERROR_BACKEND,
                    EnumErrorCode.ERROR_JSON_RESPONSE,
                    ex.Message,
                    ex
                );
            }


            return response;
        }

        /// <summary>
        /// Executes the request asynchronously without any parsing.
        /// </summary>
        /// <returns> The async task with Http response. </returns>
        public async Task<HttpResponseMessage> ExecuteUnparsedAsync()
		{
			var httClient = InitializeRestClient();
			var request = BuildRestRequest();

            RequestAuth.Authenticate(request);
            Logger.Log(request);
            var response = await httClient.SendAsync(request);
            Logger.Log(response);
            var contentType = response.Headers
                                      .Where(x => x.Key.ToLower().Equals("content-type"))
                                      .Select(x => x.Value)
                                      .FirstOrDefault()?
                                      .FirstOrDefault();

            if (contentType != null && !contentType.Contains( "application/json")) {
                var kinveyException = new KinveyException(
                    EnumErrorCategory.ERROR_REQUIREMENT,
                    EnumErrorCode.ERROR_REQUIREMENT_CONTENT_TYPE_HEADER,
                    contentType
                ) {
                    RequestID = HelperMethods.getRequestID(response)
                };
                throw kinveyException;
			}

			lastResponseCode = response.StatusCode;
            lastResponseMessage = response.StatusCode.ToString();
            lastResponseHeaders = new List<KeyValuePair<string, IEnumerable<string>>>();

			foreach (var header in response.Headers)
			{
                lastResponseHeaders.Add(header);
			}

            if ((int)response.StatusCode == 401)
            {
                ServerError error = null;
                try
                {
                    var json = await response.Content.ReadAsStringAsync();
                    error = JsonConvert.DeserializeObject<ServerError>(json);
                }
                catch (Exception)
                {

                }

                if (error != null && !error.Error.Equals(Constants.STR_ERROR_BACKEND_INSUFFICIENT_CREDENTIALS))
                {                    
                    // Attempt to get the refresh token
                    Credential cred = Client.Store.Load(Client.ActiveUser.Id, Client.SSOGroupKey);
                    string refreshToken = null;
                    string redirectUri = null;
                    string micClientId = null;

                    if (cred != null)
                    {
                        refreshToken = cred.RefreshToken;
                        redirectUri = cred.RedirectUri;
                        micClientId = cred.MICClientID;

                        if (!string.IsNullOrEmpty(refreshToken) && !refreshToken.ToLower().Equals("null"))
                        {
                            if (!hasRetried)
                            {
                                // Attempting retry - set flag to prevent additional attempts
                                hasRetried = true;

                                //use the refresh token for a new access token
                                JObject result = await Client.ActiveUser.UseRefreshToken(refreshToken, redirectUri, micClientId).ExecuteAsync();

                                // log out the current user without removing the user record from the credential store
                                Client.ActiveUser.LogoutSoft();

                                //login with the access token
                                Provider provider = new Provider();
                                provider.kinveyAuth = new MICCredential(result["access_token"].ToString());
                                User u = await User.LoginAsync(new ThirdPartyIdentity(provider), Client);

                                //store the new refresh token
                                Credential currentCred = Client.Store.Load(Client.ActiveUser.Id, Client.SSOGroupKey);
                                currentCred.AccessToken = result["access_token"].ToString();
                                currentCred.RefreshToken = result.GetValidValue("refresh_token");
                                currentCred.RedirectUri = redirectUri;
                                Client.Store.Store(Client.ActiveUser.Id, Client.SSOGroupKey, currentCred);

                                // Retry the original request
                                RequestAuth = new KinveyAuthenticator(currentCred.AuthToken);
                                var retryResponse = await ExecuteUnparsedAsync();
                                return retryResponse;
                            }
                            else
                            {
                                Client.ActiveUser.Logout();
                            }
                        }
                        else
                        {
                            //logout the current user
                            Client.ActiveUser.Logout(); 
                        }
                    }
                    else
                    {
                        Client.ActiveUser.Logout();
                    }
                }
            }

            try
            {
                response.EnsureSuccessStatusCode();
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
        /// Executes this request.
        /// </summary>
        /// <returns> The type of response. </returns>
        public virtual T Execute()
        {
            var response = ExecuteUnparsed();

			// TODO this method must be deleted once everything is async
//			if (OverrideRedirect){
//				var locList = response.Headers.FirstOrDefault(HeaderToCheck => HeaderToCheck.Name.Equals("Location")).Value;
//				return onRedirect((locList as List<string>)[0]);
//			}

            // special case to handle void or empty responses
			if (response.Content == null) 
            {
                return default(T);
            }

            string json = null;
            try
            {
                var task = response.Content.ReadAsStringAsync();
                task.Wait();
                json = task.Result;
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (JsonException ex)
            {
                throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_NETWORK, EnumErrorCode.ERROR_JSON_PARSE,
                    HelperMethods.GetCustomParsingJsonErrorMessage(json, response.RequestMessage.RequestUri.ToString(), typeof(T).FullName),
                    null,
                    ex)
                {
                    RequestID = HelperMethods.getRequestID(response)
                };
            }
            catch (ArgumentException ex)
            {
                Logger.Log(ex.Message);
                return default(T);
            }
            catch (NullReferenceException ex)
            {
                Logger.Log(ex.Message);
                return default(T);
            }

        }

        /// <summary>
        /// Executes this request asynchronously.
        /// </summary>
        /// <returns> The async task with the type of the response. </returns>
		public virtual async Task<T> ExecuteAsync(){
			var response = await ExecuteUnparsedAsync();

			if (OverrideRedirect)
			{
				string newLoc = string.Empty;
                foreach (var header in response.Headers)
				{
                    if (header.Key.ToLower().Equals("location"))
                    {
                        newLoc = header.Value.FirstOrDefault();
						break;
					}
				}
				//string newlocation = response.Headers.FirstOrDefault(stringToCheck => stringToCheck.ToString().Equals("Location")).ToString();
				return await onRedirectAsync(newLoc);
			}
			// special case to handle void or empty responses
			if (response.Content == null)
			{
				return default(T);
			}

			string path = response.RequestMessage.RequestUri.AbsolutePath;

			if (path != null &&
			    path.Contains(Constants.STR_PATH_CUSTOM_ENDPOINT) &&
			    (((int)response.StatusCode) < 200 || ((int)response.StatusCode) > 302))
			{
				// Seems like only Custom Endpoint/BL would result in having a successful response
				// without having a successful status code.  The BL executed successfully, but did
				// produce a successsful outcome.
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    var ke = new KinveyException(
                        EnumErrorCategory.ERROR_CUSTOM_ENDPOINT,
                        EnumErrorCode.ERROR_CUSTOM_ENDPOINT_ERROR,
                        response,
                        ex
                    );
                    throw ke;
                }
			}

			if (path != null &&
			    path.Contains(Constants.STR_PATH_REALTIME_STREAM) &&
				(((int)response.StatusCode) < 200 || ((int)response.StatusCode) > 302))
			{
				// Appears as though there is a stream error.  A stream error could result in having a successful response
				// without having a successful status code, such as a 401.  The request was successful, but the response
				// indicates that there is an issue with what was being requested
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    var ke = new KinveyException(
                        EnumErrorCategory.ERROR_REALTIME,
                        EnumErrorCode.ERROR_REALTIME_ERROR,
                        response,
                        ex
                    );
                    throw ke;
                }
      }

      if (((int)response.StatusCode) < 200 || ((int)response.StatusCode) > 302)
			{
				try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    var kinveyException = new KinveyException(
                        EnumErrorCategory.ERROR_BACKEND,
                        EnumErrorCode.ERROR_JSON_RESPONSE,
                        response,
                        ex
                    ) {
                        RequestID = HelperMethods.getRequestID(response)
                    };
                    throw kinveyException;
                }
			}

            string json = null;
			try
			{
                json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(json);

                RequestStartTime = HelperMethods.GetRequestStartTime(response);

                return result;
			}
            catch(JsonException ex)
            {
                KinveyException kinveyException = new KinveyException(
                    EnumErrorCategory.ERROR_DATASTORE_NETWORK,
                    EnumErrorCode.ERROR_JSON_PARSE,
                    HelperMethods.GetCustomParsingJsonErrorMessage(json, response.RequestMessage.RequestUri.ToString(), typeof(T).FullName),
                    null,
                    ex
                ) {
                    RequestID = HelperMethods.getRequestID(response)
                };
                throw kinveyException;
			}
			catch(ArgumentException ex)
			{
				Logger.Log (ex.Message);  
				return default(T);
			}
			catch (NullReferenceException ex)
			{
				Logger.Log (ex.Message);
				return default(T);
			}
		}

        /// <summary>
        /// Redirects the request to a new location.
        /// </summary>
        /// <param name="newLocation">New location</param>
        /// <returns> The async task with the type of the response. </returns>
        public virtual async Task<T> onRedirectAsync(String newLocation)
		{
			Logger.Log ("Override Redirect in response is expected, but not implemented!");  
			return default(T);
		}

        #endregion

        /// <summary>
        /// Base class of a request payload type.
        /// </summary>
        public abstract class RequestPayloadType{
            /// <summary>
            /// Gets the content type of the request payload type.
            /// </summary>
            /// <returns> The string value of the content type. </returns>
            public abstract string getContentType ();

            /// <summary>
            /// Gets the Http content as string.
            /// </summary>
            /// <param name="HttpContent">Http content.</param>
            /// <returns> The string value of the Http content. </returns>
            public abstract string getHttpContent(object HttpContent);
		}

        /// <summary>
        /// The class for the JSON payload.
        /// </summary>
        public class JSONPayload : RequestPayloadType{
            /// <summary>
            /// Gets the content type of the JSON payload.
            /// </summary>
            /// <returns> The string value of the content type for the current payload. </returns>
			public override string getContentType (){
				return "application/json";

			}

            /// <summary>
            /// Gets the Http content as string.
            /// </summary>
            /// <param name="HttpContent">Http content.</param>
            /// <returns> The string value of the Http content for the current payload. </returns>
			public override string getHttpContent(object HttpContent){
				return JsonConvert.SerializeObject(HttpContent);
			}
		}

        /// <summary>
        /// The class for the URL encoded payload.
        /// </summary>
        public class URLEncodedPayload : RequestPayloadType{
            /// <summary>
            /// Gets the content type of the URL encoded payload.
            /// </summary>
            /// <returns> The string value of the content type for the current payload. </returns>
            public override string getContentType (){
				return "application/x-www-form-urlencoded";
			}

            /// <summary>
            /// Gets the Http content as string.
            /// </summary>
            /// <param name="HttpContent">Http content.</param>
            /// <returns> The string value of the Http content for the current payload. </returns>
            public override string getHttpContent(object HttpContent){
				var dict = HttpContent as Dictionary<string, string>;

				return String.Join("&", dict.Select(kvp => String.Concat(Uri.EscapeDataString(kvp.Key), "=", Uri.EscapeDataString(kvp.Value.ToString()))));
			}
		}
    }
}
