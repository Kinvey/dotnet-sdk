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
using Kinvey.DotNet.Framework.Core;
using Kinvey.DotNet.Framework.Auth;
using RestSharp;

namespace Kinvey.DotNet.Framework
{
    public class AbstractClient : AbstractKinveyClient
    {
        public const string DefaultBaseUrl = "https://baas.kinvey.com/";
        public const string DefaultServicePath = "";

        private User currentUser;
        private ICredentialStore store;
        private AppData appData;
        protected object Lock = new object();
        private IClientUsers clientUsers;

        protected AbstractClient(RestClient client, string rootUrl, string servicePath, KinveyClientRequestInitializer initializer, ICredentialStore store)
            : base(client, rootUrl, servicePath, initializer)
        {
            this.store = store;
        }

        public User KinveyUser()
        {
            lock (Lock)
            {
                if (currentUser == null)
                {
                    var appKey = ((KinveyClientRequestInitializer)this.RequestInitializer).AppKey;
                    var appSecret = ((KinveyClientRequestInitializer)this.RequestInitializer).AppSecret;
                    this.currentUser = new User(this, new KinveyAuthRequest.Builder(this.RestClient, this.BaseUrl, appKey, appSecret, null));
                }
                return currentUser;
            }
        }

        public AppData<T> AppData<T>(String collectionName, Type myClass)
        {
            lock(Lock) 
            {
                if (appData == null) 
                {
                    appData = new AppData<T>(collectionName, myClass, this);
                }
            return (AppData<T>) appData;
            }
        }

        //public abstract IClientUsers GetClientUsers();

        public User CurrentUser
        {
            get
            {
                lock (Lock)
                {
                    return currentUser;
                }
            }
            set
            {
                lock (Lock)
                {
                    currentUser = value;
                }
            }
        }

        public IClientUsers ClientUsers
        {
            get 
            { 
                if (this.clientUsers == null) 
                { 
                    this.clientUsers = InMemoryClientUsers.GetClientUsers();
                } 
                return this.clientUsers; 
            }
            set { this.clientUsers = value; }
        }

        public ICredentialStore Store
        {
            get { return store; }
        }

        private bool GetCredential(String userId) 
        {

            CredentialManager credentialManager = new CredentialManager(store);
            Credential storedCredential = credentialManager.LoadCredential(userId);
            if (storedCredential != null) 
            {
                var kinveyRequestInitializer = ((KinveyClientRequestInitializer) this.RequestInitializer);
                kinveyRequestInitializer.KinveyCredential = new Credential(userId, storedCredential.AuthToken);
                return true;
            }
            else
            {
                return false;
            }
        }

		public new class Builder : AbstractKinveyClient.Builder
        {
            private ICredentialStore store;
            //private Properties props = new Properties();

            public Builder(RestClient transport)
                : base(transport, DefaultBaseUrl, DefaultServicePath) { }

            public Builder(RestClient transport, KinveyClientRequestInitializer clientRequestInitializer)
                : base(transport, DefaultBaseUrl, DefaultServicePath, clientRequestInitializer) { }

            public Builder(RestClient transport, string baseUrl, KinveyClientRequestInitializer clientRequestInitializer)
                : base(transport, baseUrl, DefaultServicePath, clientRequestInitializer) { }

            public ICredentialStore Store
            {
                get { return this.store; }
                set { this.store = value; }
            }

            public override AbstractKinveyClient build()
            {
                return new AbstractClient(this.HttpRestClient, this.BaseUrl, this.ServicePath, this.RequestInitializer, this.Store);
            }
        }
    }
}
