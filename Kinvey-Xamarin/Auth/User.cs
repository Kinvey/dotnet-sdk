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
using System.Threading;
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

		#region User class member variables
		/// <summary>
		/// The name of the user collection.
		/// </summary>
        public const string UserCollectionName = "user";

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
		/// Used to get information regarding email verification.  The metadata object contains 
		/// an instance of <see cref="KinveyXamarin.KMDEmailVerification"/> as well as an 
		/// instance of <see cref="KMDPasswordReset"/> 
		/// </summary>
		[JsonProperty("_kmd")]
		private KinveyUserMetaData metadata;

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
		/// Gets or sets the name of the user.
		/// </summary>
		/// <value>The name of the user.</value>
		public KinveyUserMetaData Metadata
		{
			get { return this.metadata; }
			set { this.metadata = value; }
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
        internal KinveyAuthRequest.Builder builder; // TODO change back to private, or remove altogether.

		/// <summary>
		/// The login type of the user
		/// </summary>
		[JsonIgnore]
		internal EnumLoginType type { get; set; } // TODO change back to private, or remove altogether.

		#endregion

		#region User class Constructors and Initializers

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.User"/> class.
		/// </summary>
		internal User()
		{
			this.Attributes = new Dictionary<string, JToken>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.User"/> class.
		/// </summary>
		/// <param name="builder">Builder.</param>
		/// <param name="client">[optional] Client (default is SharedClient).</param>
		internal User(KinveyAuthRequest.Builder builder, AbstractClient client = null)
		{
			if (client != null)
			{
				this.client = client;
			}
			else
			{
				this.client = Client.SharedClient;
			}

			this.builder = builder;
			builder.KinveyUser = this;
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
        internal User InitUser(KinveyAuthResponse response, string userType) // TODO change back to private?
        {
            this.id = response.UserId;
            // TODO process Unknown keys
            // this.put("_kmd", response.getMetadata());
            // this.putAll(response.getUnknownKeys());

            //this.username = response
            this.AuthToken = response.AuthToken;
			this.Attributes = response.Attributes;
			this.Metadata = response.UserMetaData;
            CredentialManager credentialManager = new CredentialManager(KinveyClient.Store);
            ((KinveyClientRequestInitializer) KinveyClient.RequestInitializer).KinveyCredential = credentialManager.CreateAndStoreCredential(response, this.id);
            return this;
        }

		/// <summary>
		/// Inits the user from a credential
		/// </summary>
		/// <returns>The user.</returns>
		/// <param name="credential">Credential.</param>
        internal User InitUser(Credential credential) // TODO change back to private?
		{
            this.id = credential.UserId;
            this.AuthToken = credential.AuthToken;
			//CredentialManager credentialManager = new CredentialManager(KinveyClient.Store);
			((KinveyClientRequestInitializer)KinveyClient.RequestInitializer).KinveyCredential = credential;
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
		#endregion

		#region User class Public APIs

		#region User class Login APIs
		/// <summary>
		/// Login (and create) an new kinvey user without any specified details.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User> LoginAsync(CancellationToken ct = default(CancellationToken))
		{
			LoginRequest loginRequest = buildLoginRequest();
			ct.ThrowIfCancellationRequested();
			return await loginRequest.ExecuteAsync();
		}

		/// <summary>
		/// Login with a specified username and password.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User> LoginAsync(string username, string password, CancellationToken ct = default(CancellationToken))
		{
			LoginRequest loginRequest = buildLoginRequest(username, password);
			ct.ThrowIfCancellationRequested();
			return await loginRequest.ExecuteAsync();
		}

		/// <summary>
		/// Login with a Kinvey Auth Token directly.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="userID">The _id of the current user.</param>
		/// <param name="authToken">The user's Kinvey Auth Token..</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User> LoginKinveyAuthTokenAsync(string userID, string authToken, CancellationToken ct = default(CancellationToken))
		{
			LoginRequest loginRequest = buildLoginRequestWithKinveyAuthToken(userID, authToken);
			ct.ThrowIfCancellationRequested();
			return await loginRequest.ExecuteAsync();
		}

		/// <summary>
		/// Login with a third party identity
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="identity">The Third party identity.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User> LoginAsync(ThirdPartyIdentity identity, CancellationToken ct = default(CancellationToken))
		{
			LoginRequest loginRequest = buildLoginRequestWithThirdParty(identity);
			ct.ThrowIfCancellationRequested();
			return await loginRequest.ExecuteAsync();
		}

		// Social Login Convenence APIs
		//

		/// <summary>
		/// Login with Facebook Credentials
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="accessToken">Facebook Access token.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User> LoginFacebookAsync(string accessToken, CancellationToken ct = default(CancellationToken))
		{
			Provider provider = new Provider ();
			ct.ThrowIfCancellationRequested();
			provider.facebook = new FacebookCredential (accessToken);
			ct.ThrowIfCancellationRequested();
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
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User> LoginTwitterAsync(string accesstoken, string accesstokensecret, string consumerkey, string consumersecret, CancellationToken ct = default(CancellationToken))
		{
			Provider provider = new Provider ();
			ct.ThrowIfCancellationRequested();
			provider.twitter = new TwitterCredential (accesstoken, accesstokensecret, consumerkey, consumersecret);
			ct.ThrowIfCancellationRequested();
			return await LoginAsync(new ThirdPartyIdentity(provider));
		}

		/// <summary>
		/// Login with Google Credentials
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="accessToken">Google Access token.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User> LoginGoogleAsync(string accessToken, CancellationToken ct = default(CancellationToken))
		{
			Provider provider = new Provider ();
			ct.ThrowIfCancellationRequested();
			provider.google = new GoogleCredential (accessToken);
			ct.ThrowIfCancellationRequested();
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
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User> LoginLinkedinAsync(string accesstoken, string accesstokensecret, string consumerkey, string consumersecret, CancellationToken ct = default(CancellationToken))
		{
			Provider provider = new Provider ();
			ct.ThrowIfCancellationRequested();
			provider.linkedin = new LinkedInCredential (accesstoken, accesstokensecret, consumerkey, consumersecret);
			ct.ThrowIfCancellationRequested();
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
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User> LoginSalesforceAsync(string access, string reauth, string clientid, string id, CancellationToken ct = default(CancellationToken))
		{
			Provider provider = new Provider ();
			ct.ThrowIfCancellationRequested();
			provider.salesforce = new SalesforceCredential (access, reauth, clientid, id);
			ct.ThrowIfCancellationRequested();
			return await LoginAsync(new ThirdPartyIdentity(provider));
		}

		/// <summary>
		/// Sends a verification email
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="userID">Userid.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User> EmailVerificationAsync(string userID, CancellationToken ct = default(CancellationToken))
		{
			EmailVerificationRequest emailVerificationRequest = buildEmailVerificationRequest(userID);
			ct.ThrowIfCancellationRequested();
			return await emailVerificationRequest.ExecuteAsync();
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
				LogoutRequest logoutRequest = buildLogoutRequest();
				logoutRequest.Execute();
			}
		}

		/// <summary>
		/// Login with Auth Link Credentials
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="accesstoken">Auth Link Accesstoken.</param>
		/// <param name="refreshtoken">Auth Link Refreshtoken.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User> LoginAuthlinkAsync(string accesstoken, string refreshtoken, CancellationToken ct = default(CancellationToken))
		{
			Provider provider = new Provider();
			ct.ThrowIfCancellationRequested();
			provider.authlink = new AuthLinkCredential (accesstoken, refreshtoken);
			ct.ThrowIfCancellationRequested();
			return await LoginAsync(new ThirdPartyIdentity(provider));
		}

		/// <summary>
		/// Login with MIC Credentials
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="accessToken">MIC Access token.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User> LoginMICWithAccessTokenAsync(string accessToken, CancellationToken ct = default(CancellationToken))
		{
			Provider provider = new Provider();
			ct.ThrowIfCancellationRequested();
			provider.kinveyAuth = new MICCredential(accessToken);
			ct.ThrowIfCancellationRequested();
			return await LoginMICAsync(new ThirdPartyIdentity(provider));
		}

		/// <summary>
		/// Login with a third party identity
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="identity">The Third party identity.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User> LoginMICAsync(ThirdPartyIdentity identity, CancellationToken ct = default(CancellationToken))
		{
			MICLoginRequest loginRequestMIC = buildLoginRequestWithMIC(identity);
			ct.ThrowIfCancellationRequested();
			return await loginRequestMIC.ExecuteAsync();
		}

		/// <summary>
		/// Performs MIC Login authorization through a login page.
		/// </summary>
		/// <param name="redirectURI">The redirect URI to be used for parsing the grant code</param>
		/// <param name="MICDelegate">MIC Delegate, which has a callback to pass back the URL to render for login, as well as success and error callbacks.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public void LoginWithAuthorizationCodeLoginPage(string redirectURI, KinveyMICDelegate<User> MICDelegate, CancellationToken ct = default(CancellationToken))
		{
			//return URL for login page
			//https://auth.kinvey.com/oauth/auth?client_id=<your_app_id>&redirect_uri=<redirect_uri>&response_type=code

			string appkey = ((KinveyClientRequestInitializer)KinveyClient.RequestInitializer).AppKey;
			string hostname = KinveyClient.MICHostName;
			if (KinveyClient.MICApiVersion != null && KinveyClient.MICApiVersion.Length > 0)
			{
				hostname += KinveyClient.MICApiVersion + "/";
			}

			ct.ThrowIfCancellationRequested();

			string myURLToRender = hostname + "oauth/auth?client_id=" + appkey + "&redirect_uri=" + redirectURI + "&response_type=code";

			//keep a reference to the redirect uri for later
			this.KinveyClient.MICRedirectURI = redirectURI;
			this.KinveyClient.MICDelegate = MICDelegate;

			if (MICDelegate != null)
			{
				ct.ThrowIfCancellationRequested();
				MICDelegate.onReadyToRender(myURLToRender);
			}
		}

		/// <summary>
		/// Performs MIC Login authorization through an API.
		/// </summary>
		/// <param name="username">Username for authentication</param>
		/// <param name="password">Password for authentication</param>
		/// <param name="redirectURI">The redirect URI to be used for parsing the grant code</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task LoginWithAuthorizationCodeAPIAsync(string username, string password, string redirectURI, CancellationToken ct = default(CancellationToken))
		{
			this.KinveyClient.MICRedirectURI = redirectURI;

			try
			{
				ct.ThrowIfCancellationRequested();

				GetMICTempURLRequest  MICTempURLRequest = buildMICTempURLRequest();
				ct.ThrowIfCancellationRequested();
				JObject tempResult = await MICTempURLRequest.ExecuteAsync();

				string tempURL = tempResult["temp_login_uri"].ToString();

				LoginToTempURLRequest MICLoginToTempURL = buildMICLoginToTempURL(username, password, tempURL);
				ct.ThrowIfCancellationRequested();
				JObject accessResult = await MICLoginToTempURL.ExecuteAsync();

				ct.ThrowIfCancellationRequested();

				string accessToken = accessResult["access_token"].ToString();

				ct.ThrowIfCancellationRequested();
				User u = await LoginMICWithAccessTokenAsync(accessToken);

				ct.ThrowIfCancellationRequested();

				//store the new refresh token
				Credential currentCred = KinveyClient.Store.Load(u.Id);
				currentCred.RefreshToken = accessResult["refresh_token"].ToString();
				currentCred.RedirectUri = this.KinveyClient.MICRedirectURI;
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
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task GetMICAccessTokenAsync(String token, CancellationToken ct = default(CancellationToken))
		{
			try
			{
				ct.ThrowIfCancellationRequested();
				JObject result = await getMICToken(token).ExecuteAsync();
				string accessToken = result["access_token"].ToString();

				ct.ThrowIfCancellationRequested();
				User u = await LoginMICWithAccessTokenAsync(accessToken);

				ct.ThrowIfCancellationRequested();

				//store the new refresh token
				Credential currentCred = KinveyClient.Store.Load(u.Id);
				currentCred.RefreshToken = result["refresh_token"].ToString();
				currentCred.RedirectUri = this.KinveyClient.MICRedirectURI;
				KinveyClient.Store.Store(u.Id, currentCred);

				if (KinveyClient.MICDelegate != null)
				{
					ct.ThrowIfCancellationRequested();
					KinveyClient.MICDelegate.onSuccess(u);
				}
				else
				{
					Logger.Log("MIC Delegate is null in Async User");
				}
			}
			catch(Exception e)
			{
				if (KinveyClient.MICDelegate != null)
				{
					ct.ThrowIfCancellationRequested();
					KinveyClient.MICDelegate.onError(e);
				}
				else
				{
					Logger.Log("MIC Delegate is null in Async User");
				}
			}
		}
		#endregion

		#region User CRUD APIs

		// User Create APIs
		//

		/// <summary>
		/// Create a new Kinvey user, with the specified username and password.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="username">the username.</param>
		/// <param name="password">the password.</param>
		/// <param name="customFieldsAndValues">[optional] Custom key/value pairs to be added to user at creation.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User> CreateAsync(string username, string password, Dictionary<string, JToken> customFieldsAndValues = null, CancellationToken ct = default(CancellationToken))
		{
			LoginRequest loginRequest = buildCreateRequest(username, password, customFieldsAndValues);
			ct.ThrowIfCancellationRequested();
			return await loginRequest.ExecuteAsync();
		}


		// User Read APIs
		//

		/// <summary>
		/// Retrieve the specified User
		/// </summary>
		/// <returns>Task which returns the requested user</returns>
		/// <param name="userID">Userid.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User> RetrieveAsync(string userID, CancellationToken ct = default(CancellationToken))
		{
			RetrieveRequest retrieveRequest = buildRetrieveRequest(userID);
			ct.ThrowIfCancellationRequested();
			return await retrieveRequest.ExecuteAsync();
		}

		/// <summary>
		/// Retrieves the async.
		/// </summary>
		/// <returns>Task which returns the requested user</returns>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User> RetrieveAsync(CancellationToken ct = default(CancellationToken))
		{
			RetrieveRequest retrieveRequest = buildRetrieveRequest(this.Id);
			ct.ThrowIfCancellationRequested();
			return await retrieveRequest.ExecuteAsync();
		}

		/// <summary>
		/// Resolve the specified query, resolves, resolve_depth, retain to get a set of users
		/// </summary>
		/// <returns>Task which returns an array of the requested users</returns>
		/// <param name="query">Query used to filter the results</param>
		/// <param name="resolves">Resolves</param>
		/// <param name="resolveDepth">Resolve depth</param>
		/// <param name="retain">If set to <c>true</c> retain references.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User[]> RetrieveAsync(string query, string[] resolves, int resolveDepth, bool retain, CancellationToken ct = default(CancellationToken))
		{
			RetrieveUsersRequest retrieveUsersRequest = buildRetrieveUsersRequest(query, resolves, resolveDepth, retain);
			ct.ThrowIfCancellationRequested();
			return await retrieveUsersRequest.ExecuteAsync();
		}

		/// <summary>
		/// User Discovery Lookup of users, based on supplied criteria.
		/// </summary>
		/// <returns>The async task which will return an array of User objects.</returns>
		/// <param name="criteria">UserDiscovery object which contains the lookup criteria.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User[]> LookupAsync(UserDiscovery criteria, CancellationToken ct = default(CancellationToken))
		{
			User[] users = default(User[]);

			if ((criteria != null) &&
				(criteria.getCriteria() != null) &&
				(criteria.getCriteria().Count > 0))
			{
				LookupRequest lookupRequest = buildLookupRequest(criteria);
				ct.ThrowIfCancellationRequested();
				users = await lookupRequest.ExecuteAsync();
			}

			return users;
		}


		// User Update APIs
		//

		/// <summary>
		/// Updates the current user.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User> UpdateAsync(CancellationToken ct = default(CancellationToken))
		{
			UpdateRequest updateRequest = buildUpdateRequest(this);
			ct.ThrowIfCancellationRequested();
			return await updateRequest.ExecuteAsync();
		}

		/// <summary>
		/// Updates the specified user.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="user">User.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User> UpdateAsync(User user, CancellationToken ct = default(CancellationToken))
		{
			UpdateRequest updateRequest = buildUpdateRequest(user);
			ct.ThrowIfCancellationRequested();
			return await updateRequest.ExecuteAsync();
		}

		/// <summary>
		/// Resets the password for the specified user ID.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="userID">The user ID of the user whose password is reset.  This can either be the 
		/// ID of the user, or the email address of the user.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User> ResetPasswordAsync(string userID, CancellationToken ct = default(CancellationToken))
		{
			ResetPasswordRequest resetPasswordRequest = buildResetPasswordRequest(userID);
			ct.ThrowIfCancellationRequested();
			return await resetPasswordRequest.ExecuteAsync();
		}


		// User Delete APIs
		//

		/// <summary>
		/// Remove the user with the specified user ID, with a flag for hard delete
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="userID">The user ID of user to delete.</param>
		/// <param name="hard">If set to <c>true</c> the user will be permanently deleted.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<KinveyDeleteResponse> DeleteAsync(string userID, bool hard, CancellationToken ct = default(CancellationToken))
		{
			DeleteRequest deleteRequest = buildDeleteRequest(userID, hard);
			ct.ThrowIfCancellationRequested();
			return await deleteRequest.ExecuteAsync();
		}

		#endregion

		#endregion

		#region User class blocking private classes - used to build up requests
		// Logs a user in asynchronously with a credential object.  Internal use only.
		internal async Task LoginAsync(Credential cred)
		{
			this.Id = cred.UserId;
			this.AuthToken = cred.AuthToken;

			LoginRequest loginRequest = buildLoginRequest(cred);
			await loginRequest.ExecuteAsync();
		}

		private LoginRequest buildLoginRequest()
		{
			this.type = EnumLoginType.IMPLICIT;
			return new LoginRequest(this).buildAuthRequest();
		}

		private LoginRequest buildLoginRequest(string username, string password)
		{
			this.type = EnumLoginType.KINVEY;
			return new LoginRequest(username, password, false, this).buildAuthRequest();
		}

		private LoginRequest buildLoginRequest(Credential cred) 
		{
			this.type = EnumLoginType.CREDENTIALSTORE;
			return new LoginRequest(cred, this).buildAuthRequest();
		}

		private LoginRequest buildLoginRequestWithThirdParty(ThirdPartyIdentity identity)
		{
			this.type = EnumLoginType.THIRDPARTY;
			return new LoginRequest(identity, this).buildAuthRequest();
		}

		private LoginRequest buildLoginRequestWithKinveyAuthToken(string userID, string authToken) 
		{
			this.AuthToken = authToken;
			this.id = userID;

			return buildLoginRequest(Credential.From(this));
		}

		private MICLoginRequest buildLoginRequestWithMIC(ThirdPartyIdentity identity)
		{
			this.type = EnumLoginType.THIRDPARTY;
			return new MICLoginRequest(identity, this).buildAuthRequest();
		}

		// Generates a request to exchange the OAuth2.0 authorization code for a MIC user token
		private RetrieveMICAccessTokenRequest getMICToken(String code)
		{
			//        grant_type: "authorization_code" - this is always set to this value
			//        code: use the ‘code’ returned in the callback 
			//        redirect_uri: The same redirect uri used when obtaining the auth grant.
			//        client_id:  The appKey (kid) of the app

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("grant_type", "authorization_code");
			data.Add("code", code);
			data.Add("redirect_uri",this.KinveyClient.MICRedirectURI);
			data.Add("client_id", ((KinveyClientRequestInitializer) client.RequestInitializer).AppKey);

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);

			RetrieveMICAccessTokenRequest getToken = new RetrieveMICAccessTokenRequest(client, client.MICHostName, data, urlParameters);
			getToken.RequireAppCredentials =  true;
			client.InitializeRequest(getToken);
			return getToken;
		}

		// Generates a request that uses the refresh token to retrieve a new MIC user token
		internal RetrieveMICAccessTokenRequest UseRefreshToken(String refreshToken, string redirectUri)
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

			RetrieveMICAccessTokenRequest getToken = new RetrieveMICAccessTokenRequest(client, client.MICHostName, data, urlParameters);
			getToken.RequireAppCredentials = true;
			client.InitializeRequest(getToken);
			return getToken;
		}

		// Generates a request to get a temporary MIC URL (automated authorization grant flow)
		private GetMICTempURLRequest buildMICTempURLRequest()
		{
			//    	client_id:  this is the app’s appKey (the KID)
			//    	redirect_uri:  the uri that the grant will redirect to on authentication, as set in the console. Note, this must exactly match one of the redirect URIs configured in the console.
			//    	response_type:  this is always set to “code”

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("response_type", "code");
			data.Add("redirect_uri", this.KinveyClient.MICRedirectURI);
			data.Add("client_id", ((KinveyClientRequestInitializer) client.RequestInitializer).AppKey);

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			if (client.MICApiVersion != null && client.MICApiVersion.Length > 0)
			{
				urlParameters.Add ("MICApiVersion", client.MICApiVersion);
			}

			GetMICTempURLRequest getTemp = new GetMICTempURLRequest(client, client.MICHostName, data, urlParameters);
			getTemp.RequireAppCredentials = true;
			client.InitializeRequest(getTemp);

			return getTemp;
		}

		// Generates a request to login a user to the temporary MIC URL (automated authorization grant flow)
		private LoginToTempURLRequest buildMICLoginToTempURL(String username, String password, String tempURL)
		{
			//    	client_id:  this is the app’s appKey (the KID)
			//    	redirect_uri:  the uri that the grant will redirect to on authentication, as set in the console. Note, this much exactly match one of the redirect URIs configured in the console.
			//    	response_type:  this is always set to “code”
			//    	username
			//    	password

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("client_id", ((KinveyClientRequestInitializer) client.RequestInitializer).AppKey);
			data.Add("redirect_uri", this.KinveyClient.MICRedirectURI);
			data.Add("response_type", "code");
			data.Add("username", username);
			data.Add("password", password);

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);

			LoginToTempURLRequest loginTemp = new LoginToTempURLRequest(client, this, tempURL, data, urlParameters);
			loginTemp.RequireAppCredentials = true;
			client.InitializeRequest(loginTemp);
			return loginTemp;
		}

		private LogoutRequest buildLogoutRequest()
		{
			return new LogoutRequest(this.KinveyClient.Store, this);
		}

		private RetrieveRequest buildRetrieveRequest(string userid)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("userID", userid);
			RetrieveRequest retrieve = new RetrieveRequest(client, userid, urlParameters);
			client.InitializeRequest(retrieve);

			return retrieve;
		}

		private RetrieveUsersRequest buildRetrieveUsersRequest(string query, string[] resolves, int resolveDepth, bool retain)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);

			urlParameters.Add("query", query);

			urlParameters.Add("resolve", string.Join(",", resolves));
			urlParameters.Add("resolve_depth", resolveDepth > 0 ? resolveDepth.ToString() : "1");
			urlParameters.Add("retainReferences",  retain.ToString());

			RetrieveUsersRequest retrieve = new RetrieveUsersRequest (client, query, urlParameters);

			client.InitializeRequest(retrieve);

			return retrieve;
		}

		private LookupRequest buildLookupRequest(UserDiscovery criteria)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);

			LookupRequest lookup = new LookupRequest(client, urlParameters, criteria);
			client.InitializeRequest(lookup);

			return lookup;
		}

		private UpdateRequest buildUpdateRequest(User u)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("userID", u.id);

			UpdateRequest update = new UpdateRequest (client, u, urlParameters);

			client.InitializeRequest(update);

			return update;
		}

		private ResetPasswordRequest buildResetPasswordRequest(string userID)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("userID", userID);

			ResetPasswordRequest reset = new ResetPasswordRequest (client, userID, urlParameters);

			client.InitializeRequest(reset);

			return reset;
		}

		private DeleteRequest buildDeleteRequest(string userid, bool hard)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("userID", userid);
			urlParameters.Add ("hard", hard.ToString());

			DeleteRequest delete = new DeleteRequest (client, userid, hard, urlParameters);

			client.InitializeRequest(delete);

			return delete;		
		}

		private EmailVerificationRequest buildEmailVerificationRequest(string userID)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("userID", userID);

			EmailVerificationRequest email = new EmailVerificationRequest (client, userID, urlParameters);

			client.InitializeRequest(email);

			return email;
		}

		private LoginRequest buildCreateRequest(string username, string password, Dictionary<string, JToken> customFieldsAndValues = null) 
        {
			this.type = EnumLoginType.KINVEY;
			if (customFieldsAndValues != null)
			{
				foreach (KeyValuePair<string, JToken> entry in customFieldsAndValues)
				{
					this.Attributes.Add (entry.Key, entry.Value);
				}
			}

			return new LoginRequest(username, password, true, this).buildAuthRequest();
        }
		#endregion

		#region User class Request inner classes

		// A login request

		// A login request to MIC
		private class MICLoginRequest : LoginRequest
		{
			internal MICLoginRequest(ThirdPartyIdentity identity, User user) :
				base(identity, user)
			{
				memberUser.builder.Create = false;
			}

			internal MICLoginRequest buildAuthRequest()
			{
				base.buildAuthRequest ();
				request.buildRequestPayload ();
				return this;
			}
		}

		// Request to retrieve MIC access token
		internal class RetrieveMICAccessTokenRequest : AbstractKinveyClientRequest<JObject>
		{
			private const string REST_PATH = "oauth/token";

			internal RetrieveMICAccessTokenRequest(AbstractClient client, string baseURL, Object content, Dictionary<string, string> urlProperties) : 
				base(client, baseURL, "POST", REST_PATH, content, urlProperties)
			{
				this.PayloadType = new URLEncodedPayload();
			}
		}

		// Request to get MIC temp URL (automated authorization grant flow)
		private class GetMICTempURLRequest : AbstractKinveyClientRequest<JObject>
		{
			private const string REST_PATH = "oauth/auth";

			internal GetMICTempURLRequest(AbstractClient client, string baseURL, Object content, Dictionary<string, string> urlProperties) :
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

		// Request to login to temp URL (automated autorization grant flow)
		private class LoginToTempURLRequest : AbstractKinveyClientRequest<JObject>
		{
			private User user;

			internal LoginToTempURLRequest(AbstractClient client, User user, string tempURL, Object httpContent, Dictionary<string, string> urlProperties):
				base(client, tempURL, "POST", "", httpContent, urlProperties)
			{
				this.PayloadType = new URLEncodedPayload();
				this.OverrideRedirect = true;
				this.user = user;
			}

			public override async Task<JObject> onRedirectAsync (string newLocation)
			{
				// TODO clean up this code - a lot of assumptions made here
				int codeIndex = newLocation.IndexOf("code=");
				if (codeIndex == -1){
					throw new KinveyException(EnumErrorCategory.ERROR_USER, EnumErrorCode.ERROR_MIC_MISSING_REDIRECT_CODE, newLocation);
				}

				String accesstoken = newLocation.Substring (codeIndex + 5); // TODO change "String" to "string" - use alias everywhere
				return await user.getMICToken (accesstoken).ExecuteAsync();
			}
		}

		// A logout request
        private class LogoutRequest
        {
			private ICredentialStore store;
			private User memberUser;

			internal LogoutRequest(ICredentialStore store, User user)
			{
				this.memberUser = user;
				this.store = store;
			}

			internal void Execute()
			{
				// delete cache and sync queue
				ICacheManager cm = ((Client)memberUser.KinveyClient).CacheManager;
				cm?.clearStorage();

				CredentialManager manager = new CredentialManager(this.store);
				var userId = memberUser.id;
				if (userId != null)
				{
					manager.RemoveCredential (userId);
				}

				((KinveyClientRequestInitializer)memberUser.KinveyClient.RequestInitializer).KinveyCredential = null;
				memberUser.KinveyClient.ActiveUser = null;
			}
		}

		// Build request to delete the user with the specified ID
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

			public override async Task<User> ExecuteAsync()
			{
				User u = await base.ExecuteAsync();

				if (u.id == (user.id))
				{
					KinveyAuthResponse auth = new KinveyAuthResponse();

					auth.UserId =  u["_id"].ToString();

					KinveyUserMetaData kmd = new KinveyUserMetaData();

					kmd.Add("lmt", u["_kmd.lmt"]) ;
					kmd.Add("authtoken", u["_kmd.authtoken"]);
					kmd.Add("_kmd", u["_kmd"]);
					auth.UserMetaData = kmd;
					auth.username =  u["username"].ToString();
					auth.Attributes = u.Attributes;

					string utype = user.type.ToString();
				
					return this.user.InitUser(auth, utype);
				}
				else
				{
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

			internal ResetPasswordRequest(AbstractClient client, string userID, Dictionary<string, string> urlProperties) :
				base(client, "POST", REST_PATH, default(User), urlProperties)
			{
				this.userID = userID;
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

		#endregion
    }
}
