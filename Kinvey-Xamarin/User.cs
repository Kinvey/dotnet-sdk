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
using Kinvey.DotNet.Framework.Auth;
using Kinvey.DotNet.Framework.Core;

namespace Kinvey.DotNet.Framework
{
    [JsonObject(MemberSerialization.OptIn)]
    public class User
    {
        public const string UserCollectionName = "user";

        protected enum LoginType 
        {
            IMPLICIT,
            KINVEY,
            CREDENTIALSTORE
        }

        [JsonProperty("_id")]
        private String id;

        private String authToken;

        [JsonProperty("username")]
        private String username;

        private AbstractClient client;

        public string Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public string AuthToken
        {
            get { return this.authToken; }
            set { this.authToken = value; }
        }

        public string UserName
        {
            get { return this.username; }
            set { this.username = value; }
        }

        public AbstractClient KinveyClient
        {
            get { return this.client; }
        }

        private KinveyAuthRequest.Builder builder;

        public User(AbstractClient client, KinveyAuthRequest.Builder builder) 
        {
            this.client = client;
            this.builder = builder;
            builder.KinveyUser = this;
        }

        public User() { }

        public bool isUserLoggedIn()
        {
            return (this.Id != null || this.AuthToken != null || this.UserName != null);
        }

        private User InitUser(KinveyAuthResponse response, string userType) 
        {
            this.Id = response.UserId;
            // TODO process Unknown keys
            // this.put("_kmd", response.getMetadata());
            // this.putAll(response.getUnknownKeys());

            //this.username = response
            this.AuthToken = response.AuthToken;
            CredentialManager credentialManager = new CredentialManager(KinveyClient.Store);
            ((KinveyClientRequestInitializer) KinveyClient.RequestInitializer).KinveyCredential = credentialManager.CreateAndStoreCredential(response, this.Id);
            KinveyClient.ClientUsers.AddUser(this.Id, userType);
            KinveyClient.ClientUsers.CurrentUser = this.Id;
            return this;
        }

        private User initUser(Credential credential)
        {
            this.Id = credential.UserId;
            this.AuthToken = credential.AuthToken;
            return this;
        }

        private void removeFromStore(string userID)
        {
            CredentialManager credentialManager = new CredentialManager(KinveyClient.Store);
            credentialManager.RemoveCredential(userID);
        }

		public LoginRequest LoginBlocking()
        {
            return new LoginRequest(this).buildAuthRequest();
        }

		public LoginRequest LoginBlocking(string username, string password)
        {
            return new LoginRequest(username, password, false, this);
        }

		public LoginRequest LoginBlocking(Credential cred) 
        {
            return new LoginRequest(cred, this);
        }

		public LoginRequest LoginKinveyAuthTokenBlocking(string userId, string authToken) 
        {
            this.AuthToken = authToken;
            this.Id = userId;
            Credential c = Credential.From(this);
			return LoginBlocking(c);
        }

		public LogoutRequest logoutBlocking() 
        {
            return new LogoutRequest(this.KinveyClient.Store, this);
        }

		/// <summary>
		/// Creates the User with a blocking implementation.
		/// </summary>
		/// <returns>The created User.</returns>
		/// <param name="userid">the username of the user.</param>
		/// <param name="password">the password for the user.</param>
		public LoginRequest CreateBlocking(string username, string password) 
        {
			return new LoginRequest(username, password, true, this).buildAuthRequest();
        }

		public class LoginRequest 
        {
            Credential credential;
            LoginType type;
            KinveyAuthRequest request;
            User memberUser;


			/// <summary>
			/// Initializes a new instance of the <see cref="Kinvey.DotNet.Framework.User+LoginRequest"/> class.
			/// </summary>
			/// <param name="user">User.</param>
            public LoginRequest(User user) 
            {
                memberUser = user;
                user.builder.Create = true;
                this.type=LoginType.IMPLICIT;
            }

			/// <summary>
			/// Initializes a new instance of the <see cref="Kinvey.DotNet.Framework.User+LoginRequest"/> class.
			/// </summary>
			/// <param name="username">Username.</param>
			/// <param name="password">Password.</param>
			/// <param name="setCreate">If set to <c>true</c> set create.</param>
			/// <param name="user">User.</param>
            public LoginRequest(string username, string password, bool setCreate, User user) 
            {
                this.memberUser = user;
                user.builder.Username = username;
                user.builder.Password = password;
                user.builder.Create = true;
                user.builder.KinveyUser = user;
                this.type = LoginType.KINVEY;
            }

			/// <summary>
			/// Initializes a new instance of the <see cref="Kinvey.DotNet.Framework.User+LoginRequest"/> class.
			/// </summary>
			/// <param name="credential">Credential.</param>
			/// <param name="user">User.</param>
            public LoginRequest(Credential credential, User user) 
            {
                this.memberUser = user;
                this.credential = credential;
                this.type = LoginType.CREDENTIALSTORE;
            }

			/// <summary>
			/// Builds the auth request.
			/// </summary>
			/// <returns>The auth request.</returns>
            public LoginRequest buildAuthRequest() {
                this.request = memberUser.builder.build();
                return this;
            }

//			public async Task<User> ExecuteAsync() 
//            {
//                if (memberUser.isUserLoggedIn())
//                {
//                    throw new KinveyException("Attempting to login when a user is already logged in",
//                            "call `myClient.user().logout().execute() first -or- check `myClient.user().isUserLoggedIn()` before attempting to login again",
//                            "Only one user can be active at a time, and logging in a new user will replace the current user which might not be intended");
//                }
//                string userType = "";
//                if (this.type == LoginType.CREDENTIALSTORE) 
//                {
//                    return memberUser.initUser(credential);
//                }
//                else 
//                {
//                    switch (this.type)
//                    {
//                        case LoginType.IMPLICIT:
//                            userType = "Implicit";
//                            break;
//                        case LoginType.KINVEY:
//                            userType = "Kinvey";
//                            break;
//                        default:
//                            throw new ArgumentException("Invalid LoginType operation.");
//                    }
//                }
////				this.request.RequestAuth = new HttpBasicAuthenticator(AppKey, AppSecret);
//
////				client.InitializeRequest(this.request);
////				this.request.
//
//				KinveyAuthResponse response = await this.request.ExecuteAsync();
//                return memberUser.InitUser(response, userType);
//            }

			/// <summary>
			/// Execute this instance.
			/// </summary>
			public User Execute() 
			{
				if (memberUser.isUserLoggedIn())
				{
					throw new KinveyException("Attempting to login when a user is already logged in",
						"call `myClient.user().logout().execute() first -or- check `myClient.user().isUserLoggedIn()` before attempting to login again",
						"Only one user can be active at a time, and logging in a new user will replace the current user which might not be intended");
				}
				string userType = "";
				if (this.type == LoginType.CREDENTIALSTORE) 
				{
					return memberUser.initUser(credential);
				}
				else 
				{
					switch (this.type)
					{
					case LoginType.IMPLICIT:
						userType = "Implicit";
						break;
					case LoginType.KINVEY:
						userType = "Kinvey";
						break;
					default:
						throw new ArgumentException("Invalid LoginType operation.");
					}
				}
				KinveyAuthResponse response = this.request.Execute();
				return memberUser.InitUser(response, userType);
			}
        }


        public class LogoutRequest
        {

            private ICredentialStore store;
            private User memberUser;

            public LogoutRequest(ICredentialStore store, User user)
            {
                this.memberUser = user;
                this.store = store;
            }

            public void Execute()
            {
                CredentialManager manager = new CredentialManager(this.store);
                manager.RemoveCredential(memberUser.Id);
                memberUser.KinveyClient.CurrentUser = null;
                ((KinveyClientRequestInitializer)memberUser.KinveyClient.RequestInitializer).KinveyCredential = null;
            }
        }

    }
}


            