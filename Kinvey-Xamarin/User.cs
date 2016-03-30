// Copyright (c) 2015, Kinvey, Inc. All rights reserved.
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
using Newtonsoft.Json.Linq;
using KinveyUtils;

namespace KinveyXamarin
{
	/// <summary>
	/// This class manages the state of a Kinvey user.  User methods can be accessed through this class, and this class represents the currently logged in user.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
    public class User : JObject
    {
		////////////////////////////////////////
		// INSTANCE VARIABLES AND GET/SET
		////////////////////////////////////////

		/// <summary>
		/// The name of the user collection.
		/// </summary>
        public const string UserCollectionName = "user";

		/// <summary>
		/// the available login types
		/// </summary>
        public enum LoginType 
        {
            IMPLICIT,
            KINVEY,
            CREDENTIALSTORE,
			THIRDPARTY
        }

		/// <summary>
		/// The _id.
		/// </summary>
        [JsonProperty("_id")]
        private String id;

		/// <summary>
		/// The auth token.
		/// </summary>
        private String authToken;

		/// <summary>
		/// The username.
		/// </summary>
        [JsonProperty("username")]
        private String username;

		/// <summary>
		/// A name-value dictionary of custom attributes of the user
		/// </summary>
		[JsonExtensionData]
		public Dictionary<string, JToken> Attributes;

		/// <summary>
		/// The client.
		/// </summary>
		[JsonIgnore]
        private AbstractClient client;

		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
        public string Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

		/// <summary>
		/// Gets or sets the auth token.
		/// </summary>
		/// <value>The auth token.</value>
        public string AuthToken
        {
            get { return this.authToken; }
            set { this.authToken = value; }
        }

		/// <summary>
		/// Gets or sets the name of the user.
		/// </summary>
		/// <value>The name of the user.</value>
        public string UserName
        {
            get { return this.username; }
            set { this.username = value; }
        }

		/// <summary>
		/// Gets the kinvey client.
		/// </summary>
		/// <value>The kinvey client.</value>
		[JsonIgnore]
		public AbstractClient KinveyClient
        {
            get { return this.client; }
        }

		/// <summary>
		/// the auth request builder.
		/// </summary>
		[JsonIgnore]
        private KinveyAuthRequest.Builder builder;

		/// <summary>
		/// The type of user
		/// </summary>
		[JsonIgnore]
		private LoginType type {get; set;}









		////////////////////////////////////////
		// CONSTRUCTORS AND INITIALIZERS
		////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.User"/> class.
		/// </summary>
		/// <param name="client">Client.</param>
		/// <param name="builder">Builder.</param>
        public User(AbstractClient client, KinveyAuthRequest.Builder builder) 
        {
            this.client = client;
            this.builder = builder;
            builder.KinveyUser = this;
			this.Attributes = new Dictionary<string, JToken>();
        }
		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.User"/> class.
		/// </summary>
        public User() {
			this.Attributes = new Dictionary<string, JToken>();
		}

		/// <summary>
		/// checks if there is currently a logged in user.
		/// </summary>
		/// <returns><c>true</c>, if user logged in was ised, <c>false</c> otherwise.</returns>
        public bool isUserLoggedIn()
        {
             return (this.id != null || this.AuthToken != null || this.UserName != null);
        }

		/// <summary>
		/// Inits the user from a kinvey auth response.
		/// </summary>
		/// <returns>The user.</returns>
		/// <param name="response">Response.</param>
		/// <param name="userType">User type.</param>
        private User InitUser(KinveyAuthResponse response, string userType) 
        {
            this.id = response.UserId;
            // TODO process Unknown keys
            // this.put("_kmd", response.getMetadata());
            // this.putAll(response.getUnknownKeys());

            //this.username = response
            this.AuthToken = response.AuthToken;
			this.Attributes = response.Attributes;
            CredentialManager credentialManager = new CredentialManager(KinveyClient.Store);
            ((KinveyClientRequestInitializer) KinveyClient.RequestInitializer).KinveyCredential = credentialManager.CreateAndStoreCredential(response, this.id);
            KinveyClient.ClientUsers.AddUser(this.id, userType);
            KinveyClient.ClientUsers.CurrentUser = this.id;
            return this;
        }

		/// <summary>
		/// Inits the user from a credential
		/// </summary>
		/// <returns>The user.</returns>
		/// <param name="credential">Credential.</param>
        private User InitUser(Credential credential)
        {
            this.id = credential.UserId;
            this.AuthToken = credential.AuthToken;
			CredentialManager credentialManager = new CredentialManager(KinveyClient.Store);
			((KinveyClientRequestInitializer)KinveyClient.RequestInitializer).KinveyCredential = credential;
			KinveyClient.ClientUsers.AddUser(this.id, "Credential");
			KinveyClient.ClientUsers.CurrentUser = this.id;
            return this;
        }

		/// <summary>
		/// Removes the user from the store..
		/// </summary>
		/// <param name="userID">User _id.</param>
        private void removeFromStore(string userID)
        {
            CredentialManager credentialManager = new CredentialManager(KinveyClient.Store);
            credentialManager.RemoveCredential(userID);
        }









		////////////////////////////////////////
		// PUBLIC API - ALL ASYNC CALLS
		////////////////////////////////////////

		// Login/Logout APIs
		//

		/// <summary>
		/// Login (and create) an new kinvey user without any specified details.
		/// </summary>
		/// <returns>The async task.</returns>
		public async Task<User> LoginAsync()
		{
			return await LoginBlocking ().ExecuteAsync ();
		}

		/// <summary>
		/// Login with a specified username and password.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		public async Task<User> LoginAsync(string username, string password)
		{
			return await LoginBlocking (username, password).ExecuteAsync ();
		}

		/// <summary>
		/// Login with a Kinvey Auth Token directly.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="userId">The _id of the current user.</param>
		/// <param name="authToken">The user's Kinvey Auth Token..</param>
		public async Task<User> LoginKinveyAuthTokenAsync(string userid, string authtoken)
		{
			return await LoginKinveyAuthTokenBlocking (userid, authtoken).ExecuteAsync();
		}

		/// <summary>
		/// Login with a third party identity
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="identity">The Third party identity.</param>
		public async Task<User> LoginAsync(ThirdPartyIdentity identity)
		{
			return await LoginBlocking (identity).ExecuteAsync();
		}

		// Social Login Convenence APIs
		//

		/// <summary>
		/// Login with Facebook Credentials
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="accessToken">Facebook Access token.</param>
		public async Task<User> LoginFacebookAsync(string accessToken)
		{
			Provider provider = new Provider ();
			provider.facebook = new FacebookCredential (accessToken);
			return await LoginAsync(new ThirdPartyIdentity(provider));
		}

		/// <summary>
		/// Login with Twitter Credentials
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="accesstoken">Twitter Accesstoken.</param>
		/// <param name="accesstokensecret">Twitter Accesstokensecret.</param>
		/// <param name="consumerkey">Twitter Consumerkey.</param>
		/// <param name="consumersecret">Twitter Consumersecret.</param>
		public async Task<User> LoginTwitterAsync(string accesstoken, string accesstokensecret, string consumerkey, string consumersecret)
		{
			Provider provider = new Provider ();
			provider.twitter = new TwitterCredential (accesstoken, accesstokensecret, consumerkey, consumersecret);
			return await LoginAsync(new ThirdPartyIdentity(provider));
		}

		/// <summary>
		/// Login with Google Credentials
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="accessToken">Google Access token.</param>
		public async Task<User> LoginGoogleAsync(string accessToken)
		{
			Provider provider = new Provider ();
			provider.google = new GoogleCredential (accessToken);
			return await LoginAsync(new ThirdPartyIdentity(provider));
		}

		/// <summary>
		/// Login with LinkedIn Credentials
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="accesstoken">Linkedin Accesstoken.</param>
		/// <param name="accesstokensecret">Linkedin Accesstokensecret.</param>
		/// <param name="consumerkey">Linkedin Consumerkey.</param>
		/// <param name="consumersecret">Linkedin Consumersecret.</param>
		public async Task<User> LoginLinkedinAsync(string accesstoken, string accesstokensecret, string consumerkey, string consumersecret)
		{
			Provider provider = new Provider ();
			provider.linkedin = new LinkedInCredential (accesstoken, accesstokensecret, consumerkey, consumersecret);
			return await LoginAsync(new ThirdPartyIdentity(provider));
		}

		/// <summary>
		/// Login with Salesforce Credentials
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="access">Salesforce Access.</param>
		/// <param name="reauth">Salesforce Reauth.</param>
		/// <param name="clientid">Salesforce Clientid.</param>
		/// <param name="id">Salesforce Identifier.</param>
		public async Task<User> LoginSalesforceAsync(string access, string reauth, string clientid, string id)
		{
			Provider provider = new Provider ();
			provider.salesforce = new SalesforceCredential (access, reauth, clientid, id);
			return await LoginAsync(new ThirdPartyIdentity(provider));
		}

		/// <summary>
		/// Sends a verification email
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="userid">Userid.</param>
		public async Task<User> EmailVerificationAsync(string userid)
		{
			return await EmailVerificationBlocking(userid).ExecuteAsync ();
		}

		private static readonly object classLock = new object();

		/// <summary>
		/// Logout the current user.
		/// </summary>
		public void Logout()
		{
			// TODO rethink locking
			lock (classLock)
			{
				logoutBlocking ().Execute ();
			}
		}

		/// <summary>
		/// Login with Auth Link Credentials
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="accesstoken">Auth Link Accesstoken.</param>
		/// <param name="refreshtoken">Auth Link Refreshtoken.</param>
		public async Task<User> LoginAuthlinkAsync(string accesstoken, string refreshtoken)
		{
			Provider provider = new Provider ();
			provider.authlink = new AuthLinkCredential (accesstoken, refreshtoken);
			return await LoginAsync(new ThirdPartyIdentity(provider));
		}





		// User CRUD APIs
		//

		// User Create APIs
		//

		/// <summary>
		/// Create a new Kinvey user, with the specified username and password.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="userid">the username.</param>
		/// <param name="password">the password.</param>
		/// <param name="customFieldsAndValues">[optional] Custom key/value pairs to be added to user at creation.</param>
		public async Task<User> CreateAsync(string username, string password, Dictionary<string, JToken> customFieldsAndValues = null)
		{
			return await CreateBlocking (username, password, customFieldsAndValues).ExecuteAsync ();
		}


		// User Read APIs
		//

		/// <summary>
		/// Retrieve the specified User
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="userid">Userid.</param>
		public async Task<User> RetrieveAsync(string userid)
		{
			return await RetrieveBlocking (userid).ExecuteAsync ();
		}

		public async Task<User> RetrieveAsync()
		{
			return await RetrieveAsync (this.Id);
		}

		/// <summary>
		/// Resolve the specified query, resolves, resolve_depth, retain to get a set of users
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="query">Query.</param>
		/// <param name="resolves">Resolves.</param>
		/// <param name="resolve_depth">Resolve depth.</param>
		/// <param name="retain">If set to <c>true</c> retain references.</param>
		public async Task<User[]> RetrieveAsync(string query, string[] resolves, int resolve_depth, bool retain)
		{
			return await RetrieveBlocking(query, resolves, resolve_depth, retain).ExecuteAsync ();
		}

		/// <summary>
		/// User Discovery Lookup of users, based on supplied criteria.
		/// </summary>
		/// <returns>The async task which will return an array of User objects.</returns>
		/// <param name="criteria">UserDiscovery object which contains the lookup criteria.</param>
		public async Task<User[]> LookupAsync(UserDiscovery criteria)
		{
			User[] users = default(User[]);

			if ((criteria != null) &&
				(criteria.getCriteria() != null) &&
				(criteria.getCriteria().Count > 0))
			{
				users = await LookupBlocking(criteria).ExecuteAsync();
			}

			return users;
		}

		// User Update APIs
		//

		/// <summary>
		/// Updates the current user.
		/// </summary>
		/// <returns>The async task.</returns>
		public async Task<User> UpdateAsync()
		{
			return await UpdateBlocking(this).ExecuteAsync ();
		}

		/// <summary>
		/// Updates the specified user.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="user">User.</param>
		public async Task<User> UpdateAsync(User user)
		{
			return await UpdateBlocking(user).ExecuteAsync ();
		}

		/// <summary>
		/// Resets the password for the specified user id
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="userid">Userid.</param>
		public async Task<User> ResetPasswordAsync(string userid)
		{
			return await ResetPasswordBlocking(userid).ExecuteAsync ();
		}

		// User Delete APIs
		//

		/// <summary>
		/// Delete the specified userid, with a flag for hard delete
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="userid">Userid.</param>
		/// <param name="hard">If set to <c>true</c> the user will be permanently deleted.</param>
		public async Task<KinveyDeleteResponse> DeleteAsync(string userid, bool hard)
		{
			return await DeleteBlocking(userid, hard).ExecuteAsync ();
		}










		////////////////////////////////////////
		// BLOCKING CALLS - TURN TO PRIVATE ACCESS
		////////////////////////////////////////

		internal async Task LoginAsync(Credential cred)
		{
			this.Id = cred.UserId;
			this.AuthToken = cred.AuthToken;

			await LoginBlocking(cred).ExecuteAsync();
		}

		/// <summary>
		/// Logins an anonymous user synchronously.
		/// </summary>
		/// <returns>The blocking.</returns>
		private LoginRequest LoginBlocking()
        {
			this.type = LoginType.IMPLICIT;
			return new LoginRequest(this).buildAuthRequest();
        }

		/// <summary>
		/// Logins a user with a username and password synchronously.
		/// </summary>
		/// <returns>The blocking.</returns>
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		private LoginRequest LoginBlocking(string username, string password)
        {
			this.type = LoginType.KINVEY;
			return new LoginRequest(username, password, false, this).buildAuthRequest();
        }

		/// <summary>
		/// Logins a user with a credential synchronously
		/// </summary>
		/// <returns>The blocking.</returns>
		/// <param name="cred">Cred.</param>
		private LoginRequest LoginBlocking(Credential cred) 
        {
			this.type = LoginType.CREDENTIALSTORE;
			return new LoginRequest (cred, this).buildAuthRequest ();
        }

		// Logs a user in synchronously with a third party identity.
		internal LoginRequest LoginBlocking(ThirdPartyIdentity identity)
		{
			// TODO change from internal to private once synchronous Execute() method
			// is removed from AbstractKinveyClientRequest.cs
			this.type = LoginType.THIRDPARTY;
			return new LoginRequest (identity, this).buildAuthRequest ();
		}

		/// <summary>
		/// Logins a user with a Kinvey Auth token synchronously.
		/// </summary>
		/// <returns>The kinvey auth token blocking.</returns>
		/// <param name="userId">User identifier.</param>
		/// <param name="authToken">Auth token.</param>
		private LoginRequest LoginKinveyAuthTokenBlocking(string userId, string authToken) 
        {
            this.AuthToken = authToken;
            this.id = userId;
			Credential c = Credential.From (this);
			return LoginBlocking(c);
        }

		/// <summary>
		/// Logouts the user synchronously.
		/// </summary>
		/// <returns>The blocking.</returns>
		private LogoutRequest logoutBlocking()
        {
            return new LogoutRequest(this.KinveyClient.Store, this);
        }

		/// <summary>
		/// Retrieves a user synchronously.
		/// </summary>
		/// <returns>The request instance used to retrieve the user.</returns>
		/// <param name="userid">The ID of the user.</param>
		private RetrieveRequest RetrieveBlocking(string userid)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("userID", userid);
			RetrieveRequest retrieve = new RetrieveRequest(client, userid, urlParameters);
			client.InitializeRequest(retrieve);

			return retrieve;
		}

		/// <summary>
		/// Retrieves a set of users synchronously, based on a query.
		/// </summary>
		/// <returns>The request instance used to retrieve the users.</returns>
		/// <param name="query">Query string to filter users.</param>
		/// <param name="resolves">The "resolve" query parameter that is used to resolve named references.</param>
		/// <param name="resolve_depth">The "resolve_depth" query parameter that resolves all references included in the enclosing entity up to a depth X.</param>
		/// <param name="retain">If set to <c>true</c>, retain references.</param>
		private RetrieveUsersRequest RetrieveBlocking(string query, string[] resolves, int resolve_depth, bool retain)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);

			urlParameters.Add("query", query);
		
			urlParameters.Add("resolve", string.Join(",", resolves));
			urlParameters.Add("resolve_depth", resolve_depth > 0 ? resolve_depth.ToString() : "1");
			urlParameters.Add("retainReferences",  retain.ToString());



			RetrieveUsersRequest retrieve = new RetrieveUsersRequest (client, query, urlParameters);

			client.InitializeRequest(retrieve);

			return retrieve;
		}

		/// <summary>
		/// Synchronously looks up users in the user collection based on a criteria.
		/// </summary>
		/// <returns>The request that looks up the user collection.</returns>
		/// <param name="criteria">The criteria used for the lookup.</param>
		private LookupRequest LookupBlocking(UserDiscovery criteria)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);

			LookupRequest lookup = new LookupRequest(client, urlParameters, criteria);
			client.InitializeRequest(lookup);

			return lookup;
		}

		/// <summary>
		/// Updates a user synchronously.
		/// </summary>
		/// <returns>The request that updates the user on the backend.</returns>
		/// <param name="u">The user to update</param>
		private UpdateRequest UpdateBlocking(User u)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("userID", u.id);

			UpdateRequest update = new UpdateRequest (client, u, urlParameters);

			client.InitializeRequest(update);

			return update;
		}

		/// <summary>
		/// Resets the user's password synchronously.
		/// </summary>
		/// <returns>The request that resets the password.</returns>
		/// <param name="userid">User ID</param>
		private ResetPasswordRequest ResetPasswordBlocking(string userid)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("userID", userid);

			ResetPasswordRequest reset = new ResetPasswordRequest (client, userid, urlParameters);

			client.InitializeRequest(reset);

			return reset;
		}

		/// <summary>
		/// Deletes a user synchronously.
		/// </summary>
		/// <returns>The request that deletes the user.</returns>
		/// <param name="userid">User ID</param>
		/// <param name="hard">If set to <c>true</c>  perform a hard delete.</param>
		private DeleteRequest DeleteBlocking(string userid, bool hard)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("userID", userid);
			urlParameters.Add ("hard", hard.ToString());

			DeleteRequest delete = new DeleteRequest (client, userid, hard, urlParameters);

			client.InitializeRequest(delete);

			return delete;		
		}

		/// <summary>
		/// Sends the user an email for verification.
		/// </summary>
		/// <returns>The client request that sends out the verification email.</returns>
		/// <param name="userid">User ID</param>
		private EmailVerificationRequest EmailVerificationBlocking(string userid)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("userID", userid);

			EmailVerificationRequest email = new EmailVerificationRequest (client, userid, urlParameters);

			client.InitializeRequest(email);

			return email;
		}
			

		/// <summary>
		/// Creates the User with a blocking implementation.
		/// </summary>
		/// <returns>The created User.</returns>
		/// <param name="userid">the username of the user.</param>
		/// <param name="password">the password for the user.</param>
		/// <param name="customFieldsAndValues">[optional] Custom key/value pairs to be added to user at creation.</param>
		private LoginRequest CreateBlocking(string username, string password, Dictionary<string, JToken> customFieldsAndValues = null) 
        {
			this.type = LoginType.KINVEY;
			if (customFieldsAndValues != null) {
				foreach (KeyValuePair<string, JToken> entry in customFieldsAndValues) {
					this.Attributes.Add (entry.Key, entry.Value);
				}
			}

			return new LoginRequest(username, password, true, this).buildAuthRequest();
        }










		////////////////////////////////////////
		// INNER REQUEST CLASSES
		////////////////////////////////////////

		/// <summary>
		/// A synchronous login request.
		/// </summary>
		public class LoginRequest 
        {
            Credential credential;
            LoginType type;
            protected KinveyAuthRequest request;
			protected User memberUser;


			/// <summary>
			/// Initializes a new instance of the <see cref="Kinvey.DotNet.Framework.User+LoginRequest"/> class.
			/// </summary>
			/// <param name="user">User.</param>
			public LoginRequest(User user) 
            {
                memberUser = user;
				memberUser.builder.Create = true;
				this.type = user.type;
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
				memberUser.builder.Username = username;
				memberUser.builder.Password = password;
				memberUser.builder.Create = setCreate;
				memberUser.builder.KinveyUser = user;
				this.type = user.type;
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
				this.type = user.type;
            }

			public LoginRequest(ThirdPartyIdentity identity, User user){
				this.memberUser = user;
				this.memberUser.builder.Identity = identity;
				this.type = user.type;
				this.memberUser.builder.Create = false;

			}

			/// <summary>
			/// Builds the auth request.
			/// </summary>
			/// <returns>The auth request.</returns>
            public LoginRequest buildAuthRequest() {
                this.request = memberUser.builder.build();
                return this;
            }
				
			/// <summary>
			/// Execute this instance.
			/// </summary>
			public User Execute() 
			{
				if (memberUser.isUserLoggedIn() && memberUser.type != LoginType.CREDENTIALSTORE)
				{
					throw new KinveyException("Attempting to login when a user is already logged in",
						"call `myClient.user().logout().execute() first -or- check `myClient.user().isUserLoggedIn()` before attempting to login again",
						"Only one user can be active at a time, and logging in a new user will replace the current user which might not be intended");
				}
				string userType = "";
				if (this.type == LoginType.CREDENTIALSTORE) 
				{
					return memberUser.InitUser(credential);
				}
				else 
				{
					userType = this.type.ToString ();
				}
				KinveyAuthResponse response = this.request.Execute();
				return memberUser.InitUser(response, userType);
			}


			/// <summary>
			/// Executes this auth request async
			/// </summary>
			/// <returns>The async task.</returns>
			public async Task<User> ExecuteAsync(){
				if (memberUser.isUserLoggedIn() && memberUser.type != LoginType.CREDENTIALSTORE)
				{
					throw new KinveyException("Attempting to login when a user is already logged in",
						"call `myClient.user().logout().execute() first -or- check `myClient.user().isUserLoggedIn()` before attempting to login again",
						"Only one user can be active at a time, and logging in a new user will replace the current user which might not be intended");
				}
				string userType = "";
				if (this.type == LoginType.CREDENTIALSTORE) 
				{
					return memberUser.InitUser(credential);
				}
				else 
				{
					userType = this.type.ToString ();
				}
				KinveyAuthResponse response = await this.request.ExecuteAsync();
				return memberUser.InitUser(response, userType);


			}
        }

		/// <summary>
		/// A synchronous logout request.
		/// </summary>
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
				var userId = memberUser.id;
				if (userId != null) {
					manager.RemoveCredential (userId);
				}
				((KinveyClientRequestInitializer)memberUser.KinveyClient.RequestInitializer).KinveyCredential = null;
                memberUser.KinveyClient.CurrentUser = null;
				if (userId != null) {
					memberUser.KinveyClient.ClientUsers.RemoveUser (userId);
				}
            }

        }
			
		// Build request to delete the user with the specified ID.
		[JsonObject(MemberSerialization.OptIn)]
		private class DeleteRequest : AbstractKinveyClientRequest<KinveyDeleteResponse>
		{
			private const string REST_PATH = "user/{appKey}/{userID}?hard={hard}";

			[JsonProperty]
			private bool hard = false;

			[JsonProperty]
			private string userID;

			internal DeleteRequest(AbstractClient client, string userID, bool hard, Dictionary<string, string> urlProperties) :
				base(client, "DELETE", REST_PATH, default(KinveyDeleteResponse), urlProperties)
			{
				this.userID = userID;
				this.hard = hard;
			}
		}

		// Build request to retrieve a user
		private class RetrieveRequest : AbstractKinveyClientRequest<User>
		{
			private const string REST_PATH = "user/{appKey}/{userID}";

			[JsonProperty]
			private string userID;

			internal RetrieveRequest(AbstractClient client, string userID, Dictionary<string, string> urlProperties) :
				base(client, "GET", REST_PATH, default(User), urlProperties)
			{
				this.userID = userID;
			}				
		}

		// Build request to retrieve users based on parameters
		private class RetrieveUsersRequest : AbstractKinveyClientRequest<User[]>
		{
			private const string REST_PATH = "user/{appKey}/{?query,resolve,resolve_depth,retainReference}";

			[JsonProperty("query")]
			private string queryFilter;

			[JsonProperty("resolve")]
			private string resolve;

			[JsonProperty("resolve_depth")]
			private string resolve_depth;

			[JsonProperty("retainReferences")]
			private string retainReferences;

			internal RetrieveUsersRequest(AbstractClient client, string query, Dictionary<string, string> urlProperties):
				base(client, "GET", REST_PATH, default(User[]), urlProperties)
			{
				this.queryFilter = query;
			}
		}

		// Build request to look up users
		private class LookupRequest : AbstractKinveyClientRequest<User[]>
		{
			private const string REST_PATH = "user/{appKey}/_lookup";

			internal LookupRequest(AbstractClient client, Dictionary<string, string> urlProperties, UserDiscovery criteria) :
				base(client, "POST", REST_PATH, null, urlProperties)
			{
				JObject requestPayload = new JObject();

				if ((criteria != null) &&
					(criteria.getCriteria() != null))
				{
					foreach (KeyValuePair<string, string> criterion in criteria.getCriteria())
					{
						requestPayload.Add(criterion.Key, criterion.Value);
					}
				}

				base.HttpContent = requestPayload;
			}
		}

		// Build request to update a user
		private class UpdateRequest : AbstractKinveyClientRequest<User>
		{
			private const string REST_PATH = "user/{appKey}/{userID}";

			[JsonProperty]
			private string userID;

			private User user;

			internal UpdateRequest(AbstractClient client, User user, Dictionary<string, string> urlProperties) :
				base(client, "PUT", REST_PATH, user, urlProperties)
			{
				this.userID = user.id;
				this.user = user;
			}

			public override User Execute(){
				// TODO can this method be removed?
				User u = base.Execute();

				if (u.id == (user.id)){
					KinveyAuthResponse auth = new KinveyAuthResponse();

					auth.UserId =  u["_id"].ToString();
					KinveyAuthResponse.KinveyUserMetadata kmd = new KinveyAuthResponse.KinveyUserMetadata();
					kmd.Add("lmt", u["_kmd.lmt"]) ;
					kmd.Add("authtoken", u["_kmd.authtoken"]);
					kmd.Add("_kmd", u["_kmd"]);
					auth.UserMetadata = kmd;
					auth.username =  u["username"].ToString();
					auth.Attributes = u.Attributes;

					string utype = user.type.ToString();
				
					return this.user.InitUser(auth, utype);
				}else{
					return u;
				}
			}


		}

		// Build request to reset password
		private class ResetPasswordRequest : AbstractKinveyClientRequest<User>
		{
			private const string REST_PATH = "/rpc/{appKey}/{userID}/user-password-reset-initiate";

			[JsonProperty]
			private string userID;

			internal ResetPasswordRequest(AbstractClient client, string userid, Dictionary<string, string> urlProperties) :
				base(client, "POST", REST_PATH, default(User), urlProperties)
			{
				this.userID = userid;
				this.RequireAppCredentials = true;
			}
		}

		// Build request to initiate email verification
		private class EmailVerificationRequest : AbstractKinveyClientRequest<User>
		{
			private const string REST_PATH = "rpc/{appKey}/{userID}/user-email-verification-initiate";

			[JsonProperty]
			private string userID;

			internal EmailVerificationRequest(AbstractClient client, string userID, Dictionary<string, string> urlProperties) :
				base(client, "POST", REST_PATH, default(User), urlProperties)
			{
				this.userID = userID;
				this.RequireAppCredentials = true;
			}
		}




















		// MIC-related APIs
		//

		/// <summary>
		/// The redirect URI for MIC login requests
		/// </summary>
		public string MICRedirectURI {get; set;}

		/// <summary>
		/// The callback for the MIC login, this is used after the redirect
		/// </summary>
		protected KinveyDelegate<User> MICDelegate;

		protected string MICHostName
		{
			get
			{
				if (this.client != null)
				{
					return this.client.MICHostName;
				}
				return null;
			}

			set
			{
				setMICHostName(value);
			}
		}

		protected string MICApiVersion { get; set;}

		/// <summary>
		/// The host name for your MIC API. This is relevant if you are using a dedicated instance of Kinvey, with an auth base URL that differs from https://auth.kinvey.com
		/// </summary>
		/// <param name="host">Your MIC host. Your hostname must use "https".</param>
		public void setMICHostName(string host){
			if (!host.StartsWith("https")){
				throw new KinveyException("MIC Hostname must use the https protocol, trying to set: " + MICHostName);
			}	
			if (!host.EndsWith ("/")) {
				host += "/";
			}

			this.client.MICHostName = host;
		}

		/// <summary>
		/// Sets the MIC API version. This is relevant in case you need to use a specific version of MIC such as an Early Adopter release.
		/// </summary>
		/// <param name="version">MIC version. eg: "v2". </param>
		public void setMICApiVersion(string version){
			if (!version.StartsWith("v")){
				version = "v" + version;
			}	
			MICApiVersion = version;
		}

		/// <summary>
		/// Login with MIC Credentials
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="accessToken">MIC Access token.</param>
		public async Task<User> LoginMICWithAccessTokenAsync(string accessToken)
		{
			Provider provider = new Provider ();
			provider.kinveyAuth = new MICCredential (accessToken);
			return await LoginMICAsync(new ThirdPartyIdentity(provider));
		}

		/// <summary>
		/// Login with a third party identity
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="identity">The Third party identity.</param>
		public async Task<User> LoginMICAsync(ThirdPartyIdentity identity)
		{
			return await MICLoginBlocking (identity).ExecuteAsync();
		}

		/// <summary>
		/// Builds login URL to be rendered by client.
		/// </summary>
		/// <param name="redirectURI">Redirect URI which will contain grant code.</param>
		/// <returns> Task<string> returning the login URL.</returns>
		public async Task<string> LoginWithAuthorizationCodeLoginPage(string redirectURI)
		{
			//return URL for login page
			//https://auth.kinvey.com/oauth/auth?client_id=<your_app_id>&redirect_uri=<redirect_uri>&response_type=code

			string appkey = ((KinveyClientRequestInitializer) KinveyClient.RequestInitializer).AppKey;
			string hostname = MICHostName;
			if (MICApiVersion != null && MICApiVersion.Length > 0)
			{
				hostname += MICApiVersion + "/";
			}

			string myURLToRender = hostname + "oauth/auth?client_id=" + appkey + "&redirect_uri=" + redirectURI + "&response_type=code";

			//keep a reference to the redirect uri for later
			this.MICRedirectURI = redirectURI;

			return myURLToRender;
		}

		/// <summary>
		/// Login the with authorization code API.
		/// </summary>
		/// <param name="username">Username for authentication.</param>
		/// <param name="password">Password for authentication.</param>
		/// <param name="redirectURI">Redirect URI.</param>
		public async Task LoginWithAuthorizationCodeAPI(string username, string password, string redirectURI)
		{
			this.MICRedirectURI = redirectURI;

			try
			{
				JObject tempResult = await getMICTempURL().ExecuteAsync();
				string tempURL = tempResult["temp_login_uri"].ToString();

				JObject accessResult = await MICLoginToTempURL(username, password, tempURL).ExecuteAsync();
				string accessToken = accessResult["access_token"].ToString();

				User u = await LoginMICWithAccessTokenAsync(accessToken);

				//store the new refresh token
				Credential currentCred = KinveyClient.Store.Load(u.Id);
				currentCred.RefreshToken = accessResult["refresh_token"].ToString();
				currentCred.RedirectUri = this.MICRedirectURI;
				KinveyClient.Store.Store(u.Id, currentCred);

			}
			catch(Exception e)
			{
				Logger.Log("Error in LoginWithAuthorizationCodeAPI: " + e.StackTrace);
			}
		}

		/// <summary>
		/// Gets the MIC access token, given the grant code passed in.
		/// </summary>
		/// <param name="token">Grant token passed back from MIC grant request</param>
		public async Task GetMICAccessTokenAsync(String token)
		{
			try
			{
				JObject result = await getMICToken(token).ExecuteAsync();
				string accessToken = result["access_token"].ToString();

				User u = await LoginMICWithAccessTokenAsync(accessToken);

				//store the new refresh token
				Credential currentCred = KinveyClient.Store.Load(u.Id);
				currentCred.RefreshToken = result["refresh_token"].ToString();
				currentCred.RedirectUri = this.MICRedirectURI;
				KinveyClient.Store.Store(u.Id, currentCred);

				if (MICDelegate != null)
				{
					MICDelegate.onSuccess(u);
				}
				else
				{
					Logger.Log("MIC Delegate is null in Async User");
				}
			}
			catch(Exception e)
			{
				if (MICDelegate != null)
				{
					MICDelegate.onError(e);
				}
				else
				{
					Logger.Log("MIC Delegate is null in Async User");
				}
			}
		}

		/// <summary>
		/// Logs a user in synchronously with Mobile Identity Connect.
		/// </summary>
		/// <returns>The request instance used to login the user.</returns>
		/// <param name="identity">The user's third party identity, represented on the backend as "_socialIdentity"</param>
		private MICLoginRequest MICLoginBlocking(ThirdPartyIdentity identity)
		{
			this.type = LoginType.THIRDPARTY;
			return new MICLoginRequest (identity, this).buildAuthRequest ();
		}

		// Generates a request to exchange the OAuth2.0 authorization code for a MIC user token
		private RetrieveMICAccessToken getMICToken(String code)
		{
			//        grant_type: "authorization_code" - this is always set to this value
			//        code: use the ‘code’ returned in the callback 
			//        redirect_uri: The same redirect uri used when obtaining the auth grant.
			//        client_id:  The appKey (kid) of the app

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("grant_type", "authorization_code");
			data.Add("code", code);
			data.Add("redirect_uri",this.MICRedirectURI);
			data.Add("client_id", ((KinveyClientRequestInitializer) client.RequestInitializer).AppKey);

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);

			RetrieveMICAccessToken getToken = new RetrieveMICAccessToken(client, MICHostName, data, urlParameters);
			getToken.RequireAppCredentials =  true;
			client.InitializeRequest(getToken);
			return getToken;
		}

		// Generates a request that uses the refresh token to retrieve a new MIC user token
		internal RetrieveMICAccessToken UseRefreshToken(String refreshToken, string redirectUri)
		{
			//        grant_type: "refresh_token" - this is always set to this value  - note the difference
			//        refresh_token: use the refresh token 
			//        redirect_uri: The same redirect uri used when obtaining the auth grant.
			//        client_id:  The appKey (kid) of the app

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("grant_type", "refresh_token");
			data.Add("refresh_token", refreshToken);
			data.Add("redirect_uri", redirectUri);
			data.Add("client_id", ((KinveyClientRequestInitializer) client.RequestInitializer).AppKey);

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);

			RetrieveMICAccessToken getToken = new RetrieveMICAccessToken(client, MICHostName, data, urlParameters);
			getToken.RequireAppCredentials = true;
			client.InitializeRequest(getToken);
			return getToken;
		}

		// Generates a request to get a temporary MIC URL (automated authorization grant flow)
		private GetMICTempURL getMICTempURL()
		{

			//    	client_id:  this is the app’s appKey (the KID)
			//    	redirect_uri:  the uri that the grant will redirect to on authentication, as set in the console. Note, this must exactly match one of the redirect URIs configured in the console.
			//    	response_type:  this is always set to “code”

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("response_type", "code");
			data.Add("redirect_uri", this.MICRedirectURI);
			data.Add("client_id", ((KinveyClientRequestInitializer) client.RequestInitializer).AppKey);

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			if (this.MICApiVersion != null && this.MICApiVersion.Length > 0) {
				urlParameters.Add ("MICApiVersion", this.MICApiVersion);
			}

			GetMICTempURL getTemp = new GetMICTempURL(client, MICHostName, data, urlParameters);
			getTemp.RequireAppCredentials = true;
			client.InitializeRequest(getTemp);
			return getTemp;  	

		}

		// Generates a request to login a user to the temporary MIC URL (automated authorization grant flow)
		private LoginToTempURL MICLoginToTempURL(String username, String password, String tempURL)
		{

			//    	client_id:  this is the app’s appKey (the KID)
			//    	redirect_uri:  the uri that the grant will redirect to on authentication, as set in the console. Note, this much exactly match one of the redirect URIs configured in the console.
			//    	response_type:  this is always set to “code”
			//    	username
			//    	password


			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("client_id", ((KinveyClientRequestInitializer) client.RequestInitializer).AppKey);
			data.Add("redirect_uri", this.MICRedirectURI);
			data.Add("response_type", "code");
			data.Add("username", username);
			data.Add("password", password);

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);

			LoginToTempURL loginTemp = new LoginToTempURL(client, this, tempURL, data, urlParameters);
			loginTemp.RequireAppCredentials = true;
			client.InitializeRequest(loginTemp);
			return loginTemp;  	

		}






		/// <summary>
		/// A synchronous MIC login request.
		/// </summary>
		public class MICLoginRequest : LoginRequest
		{

			public MICLoginRequest(ThirdPartyIdentity identity, User user) : base(identity, user)
			{
				memberUser.builder.Create = false;
			}

			/// <summary>
			/// Builds the auth request.
			/// </summary>
			/// <returns>The auth request.</returns>
			public MICLoginRequest buildAuthRequest() {
				base.buildAuthRequest ();
				request.buildRequestPayload ();
				return this;
			}

		}

		// Request to retrieve MIC access token
		internal class RetrieveMICAccessToken : AbstractKinveyClientRequest<JObject>
		{
			private const string REST_PATH = "oauth/token";

			internal RetrieveMICAccessToken(AbstractClient client, string baseURL, Object content, Dictionary<string, string> urlProperties) : 
				base(client, baseURL, "POST", REST_PATH, content, urlProperties)
			{
				this.PayloadType = new URLEncodedPayload();
			}
		}

		// Request to get MIC temp URL (automated authorization grant flow).
		private class GetMICTempURL : AbstractKinveyClientRequest<JObject>
		{
			private const string REST_PATH = "oauth/auth";

			internal GetMICTempURL(AbstractClient client, string baseURL, Object content, Dictionary<string, string> urlProperties) :
				base(client, baseURL, "POST", REST_PATH, content, urlProperties )
			{
				if (urlProperties.ContainsKey("MICApiVersion"))
				{
					string micVersion = urlProperties["MICApiVersion"];
					this.uriTemplate = micVersion + "/" + REST_PATH;
					urlProperties.Remove("MICApiVersion");
				}

				this.PayloadType = new URLEncodedPayload();
			}
		}

		// Request to login to temp URL (automated autorization grant flow).
		private class LoginToTempURL : AbstractKinveyClientRequest<JObject>
		{
			private User user;

			internal LoginToTempURL(AbstractClient client, User user, string tempURL, Object httpContent, Dictionary<string, string> urlProperties):
				base(client, tempURL, "POST", "", httpContent, urlProperties)
			{
				this.PayloadType = new URLEncodedPayload();
				this.OverrideRedirect = true;
				this.user = user;
			}

			public override JObject onRedirect (string newLocation)
			{
				// TODO clean up this code - a lot of assumptions made here
				int codeIndex = newLocation.IndexOf("code=");
				if (codeIndex == -1){
					throw new KinveyException("Redirect does not contain `code=`, was: " + newLocation);
				}

				String accesstoken = newLocation.Substring (codeIndex + 5); // TODO change "String" to "string" - use alias everywhere
				return user.getMICToken (accesstoken).Execute(); // TODO change method to be async, and await this call
			}
		}










    }
}
