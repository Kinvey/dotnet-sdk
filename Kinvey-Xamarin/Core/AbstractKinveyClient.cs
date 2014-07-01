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
using RestSharp;

namespace Kinvey.DotNet.Framework.Core
{
    public class AbstractKinveyClient
    {
        private readonly IKinveyRequestInitializer kinveyRequestInitializer;
        private readonly string rootUrl;
        private readonly string servicePath;
        private RestClient restClient;
       
        protected AbstractKinveyClient(RestClient restClient, string rootUrl, string servicePath)
            : this(restClient, rootUrl, servicePath, null) {}

        protected AbstractKinveyClient(RestClient restClient, string rootUrl, string servicePath, IKinveyRequestInitializer initializer)
        {
            this.restClient = restClient;
            this.kinveyRequestInitializer = initializer;
            this.rootUrl = NormalizeRootUrl(rootUrl);
            this.servicePath = NormalizeServicePath(servicePath);
        }

        public string RootUrl
        {
            get { return this.rootUrl; }
        }

        public string ServicePath
        {
            get { return this.servicePath; }
        }

        public string BaseUrl
        {
            get { return RootUrl + servicePath; }
        }

        public IKinveyRequestInitializer RequestInitializer
        {
            get { return this.kinveyRequestInitializer; }
        }

        public RestClient RestClient
        {
            get { return this.restClient; }
        }

        public void InitializeRequest<T>(AbstractKinveyClientRequest<T> request)
        {
            if (RequestInitializer != null) 
            {
                RequestInitializer.Initialize(request);
            }
        }

        private static string NormalizeRootUrl(string rootUrl)
        {
            if (!rootUrl.EndsWith("/"))
            {
                rootUrl += "/";
            }
            return rootUrl;
        }

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

        public abstract class Builder
        {

            private readonly RestClient restClient;
            private string baseUrl;
            private string servicePath;
            private KinveyClientRequestInitializer kinveyRequestInitializer;

            public Builder(RestClient transport, string defaultRootUrl, string defaultServicePath)
                : this(transport, defaultRootUrl, defaultServicePath, null) { }

            public Builder(RestClient transport, string defaultRootUrl, string defaultServicePath, KinveyClientRequestInitializer kinveyRequestInitializer)
            {
                this.restClient = transport;
                BaseUrl = defaultRootUrl;
                ServicePath = defaultServicePath;
                this.kinveyRequestInitializer = kinveyRequestInitializer;
            }
				
            public RestClient HttpRestClient
            {
                get { return this.restClient; }
            }

            public string BaseUrl
            {
                get { return this.baseUrl; }
                set { this.baseUrl = NormalizeRootUrl(value); }
            }

            public string ServicePath
            {
                get { return this.servicePath; }
                set { this.servicePath = NormalizeServicePath(value); }
            }

            public KinveyClientRequestInitializer RequestInitializer
            {
                get { return kinveyRequestInitializer; }
                set { this.kinveyRequestInitializer = value; }
            }
        }
    }
}
