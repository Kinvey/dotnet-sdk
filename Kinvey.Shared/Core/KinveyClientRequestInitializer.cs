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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Kinvey
{
    /// <summary>
    /// The class is for initializing of Kinvey client request.
    /// </summary>
    public class KinveyClientRequestInitializer : IKinveyRequestInitializer
    {
        /// <summary>
        /// The app key.
        /// </summary>
        private readonly string appKey;
        /// <summary>
        /// The app secret.
        /// </summary>
        private readonly string appSecret;

        /// <summary>
        /// The credential to use to authenticate the request
        /// </summary>
        private Credential credential;

        /// <summary>
        /// the kinvey headers
        /// </summary>
        private readonly KinveyHeaders headers;

        /// <summary>
        /// Initializes a new instance of the <see cref="KinveyXamarin.KinveyClientRequestInitializer"/> class.
        /// </summary>
        /// <param name="appKey">App key.</param>
        /// <param name="appSecret">App secret.</param>
        /// <param name="headers">Headers.</param>
        public KinveyClientRequestInitializer(string appKey, string appSecret, KinveyHeaders headers) : this(appKey, appSecret, headers, default(Credential)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="KinveyXamarin.KinveyClientRequestInitializer"/> class.
        /// </summary>
        /// <param name="appKey">App key.</param>
        /// <param name="appSecret">App secret.</param>
        /// <param name="headers">Headers.</param>
        /// <param name="credential">Credential.</param>
        public KinveyClientRequestInitializer(string appKey, string appSecret, KinveyHeaders headers, Credential credential)
        {
            this.appKey = appKey;
            this.appSecret = appSecret;
            this.headers = headers;
            this.credential = credential;
        }

        /// <summary>
        /// Gets the app key.
        /// </summary>
        /// <value>The app key.</value>
        public string AppKey
        {
            get { return appKey; }
        }

        /// <summary>
        /// Gets the app secret.
        /// </summary>
        /// <value>The app secret.</value>
        public string AppSecret
        {
            get { return appSecret; }
        }

        /// <summary>
        /// Gets the auth service ID.
        /// </summary>
        /// <value>The AuthServiceID property gets the value of the string field, _authServiceID. </value>
        public string AuthServiceID
        {
            get; private set;
        }

        /// <summary>
        /// Gets the headers.
        /// </summary>
        /// <value>The headers.</value>
        public KinveyHeaders Headers
        {
            get { return headers; }
        }

        /// <summary>
        /// Sets the Kinvey credential.
        /// </summary>
        /// <value>The Kinvey credential.</value>
        public Credential KinveyCredential
        {
            set { this.credential = value; }
        }

        /// <summary>
        /// Initialize the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <param name="clientId">[optional] Client ID.</param>
        /// <typeparam name="T">The response type of the request.</typeparam>

        public void Initialize<T>(AbstractKinveyClientRequest<T> request, string clientId = null)
        {
            AuthServiceID = clientId ?? AppKey;

            if (!request.RequireAppCredentials)
            {
                if (credential == null ||
                    credential.UserId == null ||
                    credential.AuthToken == null)
                {
                    throw new KinveyException(EnumErrorCategory.ERROR_USER, EnumErrorCode.ERROR_USER_NO_ACTIVE, "");
                }
            }

            if (credential != null && !request.RequireAppCredentials)
            {
                credential.Initialize(request);
            }

            if (request.RequireAppCredentials)
            {
                request.RequestAuth = new HttpBasicAuthenticator(AuthServiceID, AppSecret);
            }

            request.AppKey = appKey;

            foreach (var header in Headers)
            {
                request.RequestHeaders.Add(header);
            }

        }

    }

    /// <summary>
    /// Authenticator for Http basic authentication.
    /// </summary>
    public class HttpBasicAuthenticator : IAuthenticator
    {

        private readonly string username;
        private readonly string password;
        private readonly string base64;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="username">User name.</param>
        /// <param name="password">Password.</param>
        public HttpBasicAuthenticator(string username, string password)
        {
            this.username = username;
            this.password = password;
            var bytes = Encoding.UTF8.GetBytes($"{username}:{password}");
            base64 = Convert.ToBase64String(bytes);
        }

        /// <summary>
		/// Authenticates the specified request.
		/// </summary>
		/// <param name="request">Request.</param>
        public void Authenticate(HttpRequestMessage request)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64);
        }
    }
}
