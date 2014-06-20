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
using Kinvey.DotNet.Framework.Auth;
using RestSharp;

namespace Kinvey.DotNet.Framework.Core
{
    public class KinveyClientRequestInitializer : IKinveyRequestInitializer
    {
        private readonly string appKey;
        private readonly string appSecret;

        private Credential credential;

        private readonly KinveyHeaders headers;

        public KinveyClientRequestInitializer(string appKey, string appSecret, KinveyHeaders headers) : this(appKey, appSecret, headers, default(Credential)) {}

        public KinveyClientRequestInitializer(string appKey, string appSecret, KinveyHeaders headers, Credential credential)
        {
            this.appKey = appKey;
            this.appSecret = appSecret;
            this.headers = headers;
            this.credential = credential;
        }

        public string AppKey
        {
            get { return appKey; }
        }

        public string AppSecret
        {
            get { return appSecret; }
        }

        public KinveyHeaders Headers
        {
            get { return headers;}
        }

        public Credential KinveyCredential
        {
            set { this.credential = value; }
        }

        public void Initialize<T>(AbstractKinveyClientRequest<T> request)
        {
            if (credential != null && !request.RequireCredentials)
            {
                credential.Initialize(request);
			}else if (!request.RequireCredentials)
            {
                request.RequestAuth = new HttpBasicAuthenticator(AppKey, AppSecret);
            }
            request.AppKey = appKey;

            foreach (var header in Headers)
            {
                request.RequestHeaders.Add(new HttpHeader() { Name = header.Name, Value = header.Value });
            }

        }

    }
}
