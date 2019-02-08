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

namespace Kinvey
{
	/// <summary>
	/// This class manages the state of a Kinvey user.  User methods can be accessed through this
	/// class, and the active user of an app is an instance of this class.  Login methods are
	/// implemented as static methods on this class, and will set the active user.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
    public class User : JObject
    {
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
		/// The access token.
		/// </summary>
		private String accessToken;

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
		/// Holds the social identity information for the user
		/// </summary>
		[JsonProperty("_socialIdentity")]
		private KinveyAuthSocialID socialIdentity;

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
		/// Gets or sets the access token.
		/// </summary>
		/// <value>The access token.</value>
		public string AccessToken
        {
			get { return this.accessToken; }
			set { this.accessToken = value; }
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
		/// The _socialIdentity object.
		/// </summary>
		public KinveyAuthSocialID AuthSocialID
		{
			get { return this.socialIdentity; }
			set { this.socialIdentity = value; }
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

		internal string MICClientID { get; set; }

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

		[JsonIgnore]
		public bool Disabled
		{
			get
			{
				return (Metadata?.UserStatus?.UserEnabledState == EnumUserStatus.USER_STATUS_DISABLED);
			}
		}

		#endregion

		#region User class Constructors and Initializers

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.User"/> class.
		/// </summary>
		internal User()
		{
			// This ctor is necessary for deserailzation of the JSON representation of the User.
			this.Attributes = new Dictionary<string, JToken>();
		}

		internal User(AbstractClient client)
		{
			this.client = client;
			this.Attributes = new Dictionary<string, JToken>();
		}

		public bool IsActive()
		{
			if (KinveyClient.IsUserLoggedIn() &&
			    KinveyClient.ActiveUser.Id == this.Id)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Inits the user from a kinvey auth response.
		/// </summary>
		/// <returns>The user.</returns>
		/// <param name="response">Response.</param>
		/// <param name="userType">User type.</param>
		private User UpdateUser(KinveyAuthResponse response, string userType) // TODO refactor into LoginRequest.InitUser()
		{
            this.id = response.UserId;
            // TODO process Unknown keys
            // this.put("_kmd", response.getMetadata());
            // this.putAll(response.getUnknownKeys());

            //this.username = response
            this.AuthToken = response.AuthToken;
			this.AccessToken = response.AccessToken;
			this.Attributes = response.Attributes;
			this.Metadata = response.UserMetaData;
            CredentialManager credentialManager = new CredentialManager(KinveyClient.Store);
			((KinveyClientRequestInitializer) KinveyClient.RequestInitializer).KinveyCredential = credentialManager.CreateAndStoreCredential(response, this.id, KinveyClient.SSOGroupKey, KinveyClient.DeviceID);
            return this;
        }

		static internal User From(Credential credential, AbstractClient userClient = null)
		{
			// NOTE: do not set the RedirectURI or the Refresh Token from the credential in
			// the User object, because the user object does not hold these properties

			AbstractClient uc = userClient ?? Client.SharedClient;
			User u = new User(uc);

			u.AuthSocialID = credential.AuthSocialID;
			u.AccessToken = credential.AccessToken;
			u.Attributes = credential.Attributes;
			u.AuthToken = credential.AuthToken;
			u.Id = credential.UserId;
			u.metadata = credential.UserKMD;
			u.UserName = credential.UserName;
			u.MICClientID = credential.MICClientID;
			return u;
		}

		///// <summary>
		///// Removes the user from the store..
		///// </summary>
		///// <param name="userID">User _id.</param>
  //      private void removeFromStore(string userID)
  //      {
  //          CredentialManager credentialManager = new CredentialManager(KinveyClient.Store);
  //          credentialManager.RemoveCredential(userID);
  //      }
		#endregion

		#region User class Public APIs

		#region User class Login APIs

		/// <summary>
		/// Login a new, anonymous Kinvey user, without any specified details.
		/// </summary>
		/// <returns>The user that is logged into the app.</returns>
		/// <param name="userClient">[optional] Client that the user is logged in for, defaulted to SharedClient.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		static public async Task<User> LoginAsync(AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
		{
			AbstractClient uc = userClient ?? Client.SharedClient;
			LoginRequest loginRequest = uc.UserFactory.BuildLoginRequest();
			ct.ThrowIfCancellationRequested();
			return await loginRequest.ExecuteAsync();
		}

		/// <summary>
		/// Login with a specified username and password.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		/// <param name="userClient">[optional] Client that the user is logged in for, defaulted to SharedClient.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		static public async Task<User> LoginAsync(string username, string password, AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
		{
			if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
			{
				throw new KinveyException(EnumErrorCategory.ERROR_USER,
				                          EnumErrorCode.ERROR_REQUIREMENT_MISSING_USERNAME_PASSWORD,
				                          "Invalid username and/or password");
			}

			AbstractClient uc = userClient ?? Client.SharedClient;
			LoginRequest loginRequest = uc.UserFactory.BuildLoginRequest(username, password);
			ct.ThrowIfCancellationRequested();
			return await loginRequest.ExecuteAsync();
		}

		/// <summary>
		/// Login with a third party identity
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="identity">The Third party identity.</param>
		/// <param name="userClient">[optional] Client that the user is logged in for, defaulted to SharedClient.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		static public async Task<User> LoginAsync(ThirdPartyIdentity identity, AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
		{
			AbstractClient uc = userClient ?? Client.SharedClient;
			LoginRequest loginRequest = uc.UserFactory.BuildLoginRequest(identity);
			ct.ThrowIfCancellationRequested();
			return await loginRequest.ExecuteAsync();
		}

        #region User class login methods - Social Login Convenience APIs

        /// <summary>
        /// Login with Facebook Credentials
        /// </summary>
        /// <returns>The async task.</returns>
        /// <param name="accessToken">Facebook Access token.</param>
        /// <param name="userClient">[optional] Client that the user is logged in for, defaulted to SharedClient.</param>
        /// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
        [Obsolete("This method has been deprecated. Please use LoginWithMIC() instead.")]
        static public async Task<User> LoginFacebookAsync(string accessToken, AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
		{
			AbstractClient uc = userClient ?? Client.SharedClient;
			Provider provider = new Provider();
			ct.ThrowIfCancellationRequested();
			provider.facebook = new FacebookCredential(accessToken);
			ct.ThrowIfCancellationRequested();
			return await LoginAsync(new ThirdPartyIdentity(provider), uc, ct);
		}

        /// <summary>
        /// Login with Google Credentials
        /// </summary>
        /// <returns>The async task.</returns>
        /// <param name="accessToken">Google Access token.</param>
        /// <param name="userClient">[optional] Client that the user is logged in for, defaulted to SharedClient.</param>
        /// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
        [Obsolete("This method has been deprecated. Please use LoginWithMIC() instead.")]
        static public async Task<User> LoginGoogleAsync(string accessToken, AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
		{
			AbstractClient uc = userClient ?? Client.SharedClient;
			Provider provider = new Provider();
			ct.ThrowIfCancellationRequested();
			provider.google = new GoogleCredential(accessToken);
			ct.ThrowIfCancellationRequested();
			return await LoginAsync(new ThirdPartyIdentity(provider), uc, ct);
		}

        /// <summary>
        /// Login with Twitter Credentials
        /// </summary>
        /// <returns>The async task.</returns>
        /// <param name="accesstoken">Twitter Accesstoken.</param>
        /// <param name="accesstokensecret">Twitter Accesstokensecret.</param>
        /// <param name="consumerkey">Twitter Consumerkey.</param>
        /// <param name="consumersecret">Twitter Consumersecret.</param>
        /// <param name="userClient">[optional] Client that the user is logged in for, defaulted to SharedClient.</param>
        /// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
        [Obsolete("This method has been deprecated. Please use LoginWithMIC() instead.")]
        static public async Task<User> LoginTwitterAsync(string accesstoken, string accesstokensecret, string consumerkey, string consumersecret, AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
		{
			AbstractClient uc = userClient ?? Client.SharedClient;
			Provider provider = new Provider();
			ct.ThrowIfCancellationRequested();
			provider.twitter = new TwitterCredential(accesstoken, accesstokensecret, consumerkey, consumersecret);
			ct.ThrowIfCancellationRequested();
			return await LoginAsync(new ThirdPartyIdentity(provider), uc, ct);
		}

        /// <summary>
        /// Login with LinkedIn Credentials
        /// </summary>
        /// <returns>The async task.</returns>
        /// <param name="accesstoken">Linkedin Accesstoken.</param>
        /// <param name="accesstokensecret">Linkedin Accesstokensecret.</param>
        /// <param name="consumerkey">Linkedin Consumerkey.</param>
        /// <param name="consumersecret">Linkedin Consumersecret.</param>
        /// <param name="userClient">[optional] Client that the user is logged in for, defaulted to SharedClient.</param>
        /// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
        [Obsolete("This method has been deprecated. Please use LoginWithMIC() instead.")]
        static public async Task<User> LoginLinkedinAsync(string accesstoken, string accesstokensecret, string consumerkey, string consumersecret, AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
		{
			AbstractClient uc = userClient ?? Client.SharedClient;
			Provider provider = new Provider();
			ct.ThrowIfCancellationRequested();
			provider.linkedin = new LinkedInCredential(accesstoken, accesstokensecret, consumerkey, consumersecret);
			ct.ThrowIfCancellationRequested();
			return await LoginAsync(new ThirdPartyIdentity(provider), uc, ct);
		}

        /// <summary>
        /// Login with Salesforce Credentials
        /// </summary>
        /// <returns>The async task.</returns>
        /// <param name="access">Salesforce Access.</param>
        /// <param name="reauth">Salesforce Reauth.</param>
        /// <param name="clientid">Salesforce Clientid.</param>
        /// <param name="id">Salesforce Identifier.</param>
        /// <param name="userClient">[optional] Client that the user is logged in for, defaulted to SharedClient.</param>
        /// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
        [Obsolete("This method has been deprecated. Please use LoginWithMIC() instead.")]
        static public async Task<User> LoginSalesforceAsync(string access, string reauth, string clientid, string id, AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
		{
			AbstractClient uc = userClient ?? Client.SharedClient;
			Provider provider = new Provider();
			ct.ThrowIfCancellationRequested();
			provider.salesforce = new SalesforceCredential(access, reauth, clientid, id);
			ct.ThrowIfCancellationRequested();
			return await LoginAsync(new ThirdPartyIdentity(provider), uc, ct);
		}

		#endregion
		
		private static readonly object classLock = new object();

		/// <summary>
		/// Logout the current user.
		/// </summary>
		public void Logout()
		{
			// TODO rethink locking
			lock (classLock)
			{
				RealtimeRouter.Uninitialize();
				this.KinveyClient.DeviceID = null;

				LogoutRequest logoutRequest = buildLogoutRequest();
				logoutRequest.Execute();
			}
		}

		internal void LogoutSoft()
		{
			// TODO rethink locking
			lock (classLock)
			{
				LogoutRequest logoutRequest = BuildLogoutSoftRequest();
				logoutRequest.Execute();
			}
		}

		#region User class login methods - MIC methods

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
		static public async Task<User> LoginMICWithAccessTokenAsync(string accessToken, CancellationToken ct = default(CancellationToken))
		{
			Provider provider = new Provider();
			ct.ThrowIfCancellationRequested();
			provider.kinveyAuth = new MICCredential(accessToken);
			ct.ThrowIfCancellationRequested();
			User u = await LoginMICAsync(new ThirdPartyIdentity(provider));
			return u;
		}

		/// <summary>
		/// Login with a third party identity
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="identity">The Third party identity.</param>
		/// <param name="userClient">[optional] Client that the user is logged in for, defaulted to SharedClient.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		static public async Task<User> LoginMICAsync(ThirdPartyIdentity identity, AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
		{
			AbstractClient uc = userClient ?? Client.SharedClient;
			MICLoginRequest loginRequestMIC = uc.UserFactory.BuildMICLoginRequest(identity);
			ct.ThrowIfCancellationRequested();
			return await loginRequestMIC.ExecuteAsync();
		}

		/// <summary>
		/// Performs MIC Login authorization through a login page.
		/// </summary>
		/// <param name="redirectURI">The redirect URI to be used for parsing the grant code</param>
		/// <param name="MICDelegate">MIC Delegate, which has a callback to pass back the URL to render for login, as well as success and error callbacks.</param>
		/// <param name="userClient">[optional] Client that the user is logged in for, defaulted to SharedClient.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		[Obsolete("This method has been deprecated.  Please use LoginWithMIC() instead.")]
		static public void LoginWithAuthorizationCodeLoginPage(string redirectURI, KinveyMICDelegate<User> MICDelegate, AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
		{
			LoginWithMIC(redirectURI, MICDelegate, null, userClient, ct);
		}

		/// <summary>
		/// Performs MIC Login authorization through a login page.
		/// </summary>
		/// <param name="redirectURI">The redirect URI to be used for parsing the grant code</param>
		/// <param name="MICDelegate">MIC Delegate, which has a callback to pass back the URL to render for login, as well as success and error callbacks.</param>
		/// <param name="clientID">[optional] Client ID configured during auth provider configuration, which is to be used during authentication.  Defaults to app key.</param>
		/// <param name="userClient">[optional] Client that the user is logged in for, defaulted to SharedClient.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		static public void LoginWithMIC(string redirectURI, KinveyMICDelegate<User> MICDelegate, string micID = null, AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
		{
			//return URL for login page
			//https://auth.kinvey.com/oauth/auth?client_id=<your_app_id>&redirect_uri=<redirect_uri>&response_type=code

			AbstractClient uc = userClient ?? Client.SharedClient;

			string appKey = ((KinveyClientRequestInitializer)uc.RequestInitializer).AppKey;
			string hostname = uc.MICHostName;
			if (uc.MICApiVersion != null && uc.MICApiVersion.Length > 0)
			{
				hostname += uc.MICApiVersion + "/";
			}

			string clientID = appKey;

			// Check if a client ID for MIC authentication has been provided
			//
			if (!string.IsNullOrEmpty(micID))
			{
				clientID += Constants.MIC_ID_SEPARATOR + micID;
			}

			ct.ThrowIfCancellationRequested();

			string myURLToRender = hostname + "oauth/auth?client_id=" + clientID + "&redirect_uri=" + redirectURI + "&response_type=code" + "&scope=openid";

			//keep a reference to the redirect uri for later
			uc.MICRedirectURI = redirectURI;
			uc.MICDelegate = MICDelegate;

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
		[Obsolete("This method has been deprecated.  Please use LoginWithMIC(username:, password:, micID:, userClient:, ct: ) instead.")]
		static public async Task LoginWithAuthorizationCodeAPIAsync(string username, string password, string redirectURI, AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
		{
			await LoginWithMICInternal(username, password, redirectURI, null, userClient, ct);
		}

        /// <summary>
        /// Performs MIC Login authorization through an API.
        /// </summary>
        /// <param name="username">Username for authentication</param>
        /// <param name="password">Password for authentication</param>
        /// <param name="redirectURI">The redirect URI to be used for parsing the grant code</param>
        /// <param name="micID">[optional] Auth Service ID</param>
        /// <param name="userClient">[optional] Client that the user is logged in, defaulted to SharedClient.</param>
        /// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
        [Obsolete("This method has been deprecated.  Please use LoginWithMIC(username:, password:, micID:, userClient:, ct: ) instead.")]
        static public async Task LoginWithMIC(string username, string password, string redirectURI, string micID = null, AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
		{
            await LoginWithMICInternal(username, password, redirectURI, micID, userClient, ct);
        }

        /// <summary>
        /// Performs MIC Login authorization through an API.
        /// </summary>
        /// <param name="username">Username for authentication</param>
        /// <param name="password">Password for authentication</param>
        /// <param name="micID">[optional] Auth Service ID</param>
        /// <param name="userClient">[optional] Client that the user is logged in, defaulted to SharedClient.</param>
        /// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
        static public async Task LoginWithMIC(string username, string password, string micID = null, AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
        {
            await LoginWithMICInternal(username, password, string.Empty, micID, userClient, ct);
        }

        /// <summary>
        /// Performs MIC Login authorization through an API.
        /// </summary>
        /// <param name="username">Username for authentication</param>
        /// <param name="password">Password for authentication</param>
        /// <param name="redirectURI">The redirect URI to be used for parsing the grant code</param>
        /// <param name="micID">[optional] Auth Service ID</param>
        /// <param name="userClient">[optional] Client that the user is logged in, defaulted to SharedClient.</param>
        /// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
        static private async Task LoginWithMICInternal(string username, string password, string redirectURI, string micID = null, AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
        {
            var uc = userClient ?? Client.SharedClient;
            uc.MICRedirectURI = redirectURI;

            try
            {
                ct.ThrowIfCancellationRequested();

                var clientID = ((KinveyClientRequestInitializer)uc.RequestInitializer).AppKey;

                // Check if a client ID for MIC authentication has been provided
                //
                if (!string.IsNullOrEmpty(micID))
                {
                    clientID += Constants.MIC_ID_SEPARATOR + micID;
                }

                // Initiate token request
                //
                var micLoginRequest = BuildMICLogin(uc, username, password, clientID);
                ct.ThrowIfCancellationRequested();
                var accessResult = await micLoginRequest.ExecuteAsync();

                ct.ThrowIfCancellationRequested();

                var accessToken = accessResult["access_token"].ToString();

                ct.ThrowIfCancellationRequested();

                // Log into Kinvey using the token
                //
                var user = await LoginMICWithAccessTokenAsync(accessToken);

                ct.ThrowIfCancellationRequested();

                // Store the new access token and refresh token
                //
                var currentCred = uc.Store.Load(user.Id, uc.SSOGroupKey);
                currentCred.AccessToken = accessResult["access_token"].ToString();

                if (accessResult["refresh_token"] != null)
                {
                    var refreshToken = accessResult["refresh_token"].ToString();
                    if (!string.IsNullOrEmpty(refreshToken) && !refreshToken.ToLower().Equals("null"))
                    {
                        currentCred.RefreshToken = refreshToken;
                    }
                }

                currentCred.RedirectUri = uc.MICRedirectURI;
                currentCred.MICClientID = clientID;
                uc.Store.Store(user.Id, uc.SSOGroupKey, currentCred);
            }
            catch (KinveyException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.Log("Error in LoginWithMIC: " + e.StackTrace);
                var kinveyException = new KinveyException(EnumErrorCategory.ERROR_USER, EnumErrorCode.ERROR_GENERAL, "Unexpected error in MIC Resource Owner Credentials Grant flow", e);
                throw kinveyException;
            }
        }

        /// <summary>
        /// Gets the MIC access token, given the grant code passed in.
        /// </summary>
        /// <param name="token">Grant token passed back from MIC grant request</param>
        /// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
        static public async Task GetMICAccessTokenAsync(String token, string micID = null, AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
		{
			AbstractClient uc = userClient ?? Client.SharedClient;

			try
			{
				string appKey = ((KinveyClientRequestInitializer)uc.RequestInitializer).AppKey;

				string clientID = appKey;

				// Check if a client ID for MIC authentication has been provided
				//
				if (!string.IsNullOrEmpty(micID))
				{
					clientID += Constants.MIC_ID_SEPARATOR + micID;
				}

				ct.ThrowIfCancellationRequested();
				JObject result = await User.GetMICToken(uc, token, clientID).ExecuteAsync();
				string accessToken = result["access_token"].ToString();

				ct.ThrowIfCancellationRequested();
				User u = await User.LoginMICWithAccessTokenAsync(accessToken);

				ct.ThrowIfCancellationRequested();

				//store the new refresh token
				Credential currentCred = uc.Store.Load(u.Id, uc.SSOGroupKey);
				currentCred.AccessToken = result["access_token"].ToString();

                if (result["refresh_token"] != null)
                {
                    var refreshToken = result["refresh_token"].ToString();
                    if (!string.IsNullOrEmpty(refreshToken) && !refreshToken.ToLower().Equals("null"))
                    {
                        currentCred.RefreshToken = refreshToken;
                    }
                }

                currentCred.RedirectUri = uc.MICRedirectURI;
                currentCred.MICClientID = clientID;
				uc.Store.Store(u.Id, uc.SSOGroupKey, currentCred);

				if (uc.MICDelegate != null)
				{
					ct.ThrowIfCancellationRequested();
					uc.MICDelegate.onSuccess(u);
				}
				else
				{
					Logger.Log("MIC Delegate is null in Async User");
				}
			}
			catch(Exception e)
			{
				if (uc.MICDelegate != null)
				{
					ct.ThrowIfCancellationRequested();
					uc.MICDelegate.onError(e);
				}
				else
				{
					Logger.Log("MIC Delegate is null in Async User");
				}
			}
		}

        #endregion

        #region Realtime

        /// <summary>
        /// Registers the active user for realtime messaging.
        /// </summary>
        /// <returns>The realtime.</returns>
        /// <param name="userClient">User client.</param>
        /// <param name="ct">Ct.</param>
        /// <param name="realtimeReconnectionPolicy"> Realtime reconnection policy </param>
        public async Task RegisterRealtimeAsync(AbstractClient userClient = null, CancellationToken ct = default(CancellationToken), RealtimeReconnectionPolicy realtimeReconnectionPolicy = RealtimeReconnectionPolicy.EXPONENTIAL)
		{
			if (!IsActive())
			{
				// throw an error stating that user object has to be the active user in order to register for realtime messages
			}

			try
			{
				AbstractClient uc = userClient ?? Client.SharedClient;

				// TODO make "Realtime Register" request to KCS, and throw any error received.
				// Only proceed with RealtimeRouter init if call is successful.
				RealtimeRegisterRequest realtimeRequest = BuildRealtimeRegisterRequest(Id, KinveyClient.DeviceID);
				ct.ThrowIfCancellationRequested();
				JObject responseRegister = await realtimeRequest.ExecuteAsync();

				string channelGroupName = responseRegister[Constants.STR_REALTIME_CHANNEL_GROUP].ToString();
				string publishKey = responseRegister[Constants.STR_REALTIME_PUBLISH_KEY].ToString();
				string subscribeKey = responseRegister[Constants.STR_REALTIME_SUBSCRIBE_KEY].ToString();

				RealtimeRouter.Initialize(channelGroupName, publishKey, subscribeKey, AuthToken, uc, realtimeReconnectionPolicy);
			}
			catch (KinveyException)
			{
				throw;
			}
		}

		/// <summary>
		/// Unregisters the active user from realtime messaging.
		/// </summary>
		/// <returns>The realtime.</returns>
		/// <param name="userClient">User client.</param>
		/// <param name="ct">Ct.</param>
		public async Task UnregisterRealtimeAsync(AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
		{
			try
			{
				RealtimeRouter.Uninitialize();

				// TODO make "Realtime Unregister" request to KCS, and throw any error received.
				RealtimeUnregisterRequest realtimeRequest = BuildRealtimeUnregisterRequest(Id, KinveyClient.DeviceID);
				ct.ThrowIfCancellationRequested();
				JObject responseUnregister = await realtimeRequest.ExecuteAsync();
			}
			catch (KinveyException)
			{
				throw;
			}
		}

		#endregion

		#endregion

		#region User CRUD APIs

		/// <summary>
		/// Create a new Kinvey user, with the specified username and password.
		/// </summary>
		/// <returns>The newly created user.</returns>
		/// <param name="username">The username of the new user.</param>
		/// <param name="password">The password of the new user.</param>
		/// <param name="customFieldsAndValues">[optional] Custom key/value pairs to be added to user at creation.</param>
		/// <param name="userClient">[optional] Client that the user is logged in for, defaulted to SharedClient.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		static public async Task<User> SignupAsync(string username, string password, Dictionary<string, JToken> customFieldsAndValues = null, AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
		{
			AbstractClient uc = userClient ?? Client.SharedClient;
			LoginRequest loginRequest = uc.UserFactory.BuildCreateRequest(username, password, customFieldsAndValues);
			ct.ThrowIfCancellationRequested();
			return await loginRequest.ExecuteAsync();
		}

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
		/// Refresh this user object with latest data.
		/// </summary>
		/// <returns>The up-to-date user object.</returns>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<User> RefreshAsync(CancellationToken ct = default(CancellationToken))
		{
			RetrieveRequest retrieveRequest = buildRetrieveRequest(this.Id);
			ct.ThrowIfCancellationRequested();
			User u = await retrieveRequest.ExecuteAsync();

			if (this.IsActive())
			{
				UpdateActiveUser(u);
			}

			return u;
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

		/// <summary>
		/// Checks to see if the given username already exists.
		/// </summary>
		/// <returns>True if the username currently exists, otherwise false.</returns>
		/// <param name="username">Username to check</param>
		/// <param name="userClient">[optional] Client that the user is logged in for, defaulted to SharedClient.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		static public async Task<bool> HasUser(string username, AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
		{
			AbstractClient uc = userClient ?? Client.SharedClient;
			ct.ThrowIfCancellationRequested();

			UserExistenceRequest existenceCheckRequest = uc.UserFactory.BuildUserExistenceRequest(username);

			ct.ThrowIfCancellationRequested();

			bool exists = false;

			try
			{
				JObject existenceResult = await existenceCheckRequest.ExecuteAsync();
				if (existenceResult != null)
				{
					JToken existenceCheck = existenceResult.GetValue("usernameExists");

					if (existenceCheck != null)
					{
						exists = existenceCheck.ToObject<bool>();
					}
					else
					{
						throw new KinveyException(EnumErrorCategory.ERROR_USER,
												  EnumErrorCode.ERROR_GENERAL,
												  "Error in DoesUsernameExist().");
					}
				}
			}
			catch (KinveyException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_USER,
										  EnumErrorCode.ERROR_GENERAL,
										  "Error in DoesUsernameExist().",
										  e);
			}

			return exists;
		}

		/// <summary>
		/// Sends an email to the provided email address if a user forgot their username.
		/// </summary>
		/// <param name="email">Email address used to send username info.</param>
		/// <param name="userClient">[optional] Client that the user is logged in for, defaulted to SharedClient.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		static public async Task ForgotUsername(string email, AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
		{
			if (String.IsNullOrEmpty(email))
			{
				throw new KinveyException(EnumErrorCategory.ERROR_USER,
										  EnumErrorCode.ERROR_REQUIREMENT_INVALID_PARAMETER,
										  "Invalid email");
			}

			AbstractClient uc = userClient ?? Client.SharedClient;
			ct.ThrowIfCancellationRequested();

			ForgotUsernameRequest forgotUsernameRequest = uc.UserFactory.BuildForgotUsernameRequest(email);

			ct.ThrowIfCancellationRequested();
			await forgotUsernameRequest.ExecuteAsync();
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
		static public async Task<User> ResetPasswordAsync(string userID, CancellationToken ct = default(CancellationToken))
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

        #endregion

        #endregion

        #region User class internal login methods - internal use only.

        // Logs a user in asynchronously with a credential object.  Internal use only.
        static internal async Task LoginAsync(Credential cred, AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
		{
			AbstractClient uc = userClient ?? Client.SharedClient;
			if (!String.IsNullOrEmpty(cred.AccessToken) &&
			    (0 != string.Compare(uc.SSOGroupKey, ((KinveyClientRequestInitializer)uc.RequestInitializer).AppKey, StringComparison.Ordinal)) )
			{
				User u = await User.LoginMICWithAccessTokenAsync(cred.AccessToken);
				uc.ActiveUser = u;
			}
			else
			{
				LoginRequest loginRequest = uc.UserFactory.BuildLoginRequest(cred);
				ct.ThrowIfCancellationRequested();
				await loginRequest.ExecuteAsync();
			}
		}

		// Logs a user in asynchronously  with a Kinvey Auth Token directly.  Internal use only.
		static internal async Task<User> LoginKinveyAuthTokenAsync(string userID, string authToken, AbstractClient userClient = null, CancellationToken ct = default(CancellationToken))
		{
			AbstractClient uc = userClient ?? Client.SharedClient;
			LoginRequest loginRequest = uc.UserFactory.BuildLoginRequestWithKinveyAuthToken(userID, authToken);
			ct.ThrowIfCancellationRequested();
			return await loginRequest.ExecuteAsync();
		}

		#endregion

		#region User class blocking private classes - used to build up requests

		// Generates a request to exchange the OAuth2.0 authorization code for a MIC user token
		static internal RetrieveMICAccessTokenRequest GetMICToken(AbstractClient cli, String code, string clientID)
		{
			//        grant_type: "authorization_code" - this is always set to this value
			//        code: use the ‘code’ returned in the callback 
			//        redirect_uri: The same redirect uri used when obtaining the auth grant.
			//        client_id:  The appKey (kid) of the app

			string app_id = ((KinveyClientRequestInitializer)cli.RequestInitializer).AppKey;

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("grant_type", "authorization_code");
			data.Add("code", code);
			data.Add("redirect_uri", cli.MICRedirectURI);
			data.Add("client_id", clientID);

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", app_id);

			RetrieveMICAccessTokenRequest getToken = new RetrieveMICAccessTokenRequest(cli, cli.MICHostName, data, urlParameters);
			getToken.RequireAppCredentials =  true;
            cli.InitializeRequest(getToken, clientID);
			return getToken;
		}

		// Generates a request that uses the refresh token to retrieve a new MIC user token
		internal RetrieveMICAccessTokenRequest UseRefreshToken(String refreshToken, string redirectUri, string clientID)
		{
			//        grant_type: "refresh_token" - this is always set to this value  - note the difference
			//        refresh_token: use the refresh token 
			//        redirect_uri: The same redirect uri used when obtaining the auth grant.
			//        client_id:  The appKey (kid) of the app

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("grant_type", "refresh_token");
			data.Add("refresh_token", refreshToken);
			data.Add("redirect_uri", redirectUri);
			data.Add("client_id", clientID);

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);

			RetrieveMICAccessTokenRequest getToken = new RetrieveMICAccessTokenRequest(client, client.MICHostName, data, urlParameters);
			getToken.RequireAppCredentials = true;
            client.InitializeRequest(getToken, clientID);
			return getToken;
		}

		// Generates a request to get a temporary MIC URL (automated authorization grant flow)
		static internal GetMICTempURLRequest BuildMICTempURLRequest(AbstractClient cli, string clientID)
		{
			//    	client_id:  this is the app’s appKey (the KID)
			//    	redirect_uri:  the uri that the grant will redirect to on authentication, as set in the console. Note, this must exactly match one of the redirect URIs configured in the console.
			//    	response_type:  this is always set to “code”

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("response_type", "code");
			data.Add("redirect_uri", Client.SharedClient.MICRedirectURI);//this.KinveyClient.MICRedirectURI);
			data.Add("scope", "openid");

			string appKey = ((KinveyClientRequestInitializer)cli.RequestInitializer).AppKey;

			data.Add("client_id", clientID);

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", appKey);

			if (cli.MICApiVersion != null && cli.MICApiVersion.Length > 0)
			{
				urlParameters.Add ("MICApiVersion", cli.MICApiVersion);
			}

			GetMICTempURLRequest getTemp = new GetMICTempURLRequest(cli, cli.MICHostName, data, urlParameters);
			getTemp.RequireAppCredentials = true;
			cli.InitializeRequest(getTemp);

			return getTemp;
		}

		// Generates a request to login a user to the temporary MIC URL (automated authorization grant flow)
		static private LoginToTempURLRequest BuildMICLoginToTempURL(AbstractClient cli, String username, String password, String tempURL, string clientID)
		{
			//    	client_id:  provided by the developer when configuring authlink (defaults to the app’s appKey (the KID))
			//    	redirect_uri:  the uri that the grant will redirect to on authentication, as set in the console. Note, this much exactly match one of the redirect URIs configured in the console.
			//    	response_type:  this is always set to “code”
			//    	username
			//    	password

			string appKey = ((KinveyClientRequestInitializer)cli.RequestInitializer).AppKey;

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("client_id", clientID);
			data.Add("redirect_uri", cli.MICRedirectURI);
			data.Add("response_type", "code");
			data.Add("username", username);
			data.Add("password", password);

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", appKey);

			LoginToTempURLRequest loginTemp = new LoginToTempURLRequest(cli, tempURL, data, urlParameters);
			loginTemp.RequireAppCredentials = true;
			cli.InitializeRequest(loginTemp);
			return loginTemp;
		}

        // Generates a request to login a user to the MIC URL (Resource Owner Credentials Grant (Username and Password))
        static private MicLoginRequest BuildMICLogin(AbstractClient cli, string username, string password, string clientID)
        {
            //    	client_id:  provided by the developer when configuring authlink (defaults to the app’s appKey (the KID))
            //    	client_secret: App Secret
            //      grant_type: "password" - this is always set to this value
            //    	username
            //    	password

            var appSecret = ((KinveyClientRequestInitializer)cli.RequestInitializer).AppSecret;

            var data = new Dictionary<string, string>
            {
                { "client_id", clientID },
                { "grant_type", "password" },
                { "username", username },
                { "password", password }
            };

            // Creation MIC Login request
            //
            var micLoginRequest = new MicLoginRequest(cli, data);          
            cli.InitializeRequest(micLoginRequest);
            return micLoginRequest;
        }

        private LogoutRequest buildLogoutRequest()
		{
			return new LogoutRequest(this.KinveyClient.Store, this);
		}

		private LogoutRequest BuildLogoutSoftRequest()
		{
			return new LogoutRequest(this.KinveyClient.Store, this, false);
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

		static private ResetPasswordRequest buildResetPasswordRequest(string userID, AbstractClient userClient = null)
		{
			AbstractClient uc = userClient ?? Client.SharedClient;
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)uc.RequestInitializer).AppKey);
			urlParameters.Add("userID", userID);

			ResetPasswordRequest reset = new ResetPasswordRequest(uc, userID, urlParameters);

			uc.InitializeRequest(reset);

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

		private RealtimeRegisterRequest BuildRealtimeRegisterRequest(string userID, string deviceID)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("userID", userID);

			var realtimeRegister = new RealtimeRegisterRequest(client, deviceID, urlParameters);
			client.InitializeRequest(realtimeRegister);

			return realtimeRegister;
		}

		private RealtimeUnregisterRequest BuildRealtimeUnregisterRequest(string userID, string deviceID)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("userID", userID);

			var realtimeUnregister = new RealtimeUnregisterRequest(client, deviceID, urlParameters);
			client.InitializeRequest(realtimeUnregister);

			return realtimeUnregister;
		}

		#endregion

		#region User class Request inner classes

		// A login request

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
		internal class GetMICTempURLRequest : AbstractKinveyClientRequest<JObject>
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
		internal class LoginToTempURLRequest : AbstractKinveyClientRequest<JObject>
		{
			AbstractClient cli;
			string clientID = null;

			internal LoginToTempURLRequest(AbstractClient client, string tempURL, Object httpContent, Dictionary<string, string> urlProperties):
				base(client, tempURL, "POST", "", httpContent, urlProperties)
			{
				this.PayloadType = new URLEncodedPayload();
				this.OverrideRedirect = true;
				this.cli = client;
				clientID = (httpContent as Dictionary<string, string>)["client_id"];
			}

			public override async Task<JObject> onRedirectAsync (string newLocation)
			{
				// TODO clean up this code - a lot of assumptions made here
				int codeIndex = newLocation.IndexOf(Constants.STR_MIC_REDIRECT_CODE);
				if (codeIndex == -1)
                {
                    int errorIndex = newLocation.IndexOf(Constants.STR_MIC_REDIRECT_ERROR);

                    if (errorIndex == -1)
                    {
                        throw new KinveyException(EnumErrorCategory.ERROR_USER, EnumErrorCode.ERROR_MIC_MISSING_REDIRECT_CODE, newLocation);
                    }
                    else
                    {
                        // Error response looks like:
                        // <redirect_uri>?error=<error code>&error_description=<error description text>
                        var ex = new KinveyException(EnumErrorCategory.ERROR_USER, EnumErrorCode.ERROR_MIC_REDIRECT_ERROR, newLocation);
                        ex.Description = newLocation.Substring(newLocation.IndexOf(Constants.STR_MIC_REDIRECT_ERROR_DESCRIPTION) + Constants.STR_MIC_REDIRECT_ERROR_DESCRIPTION.Length);
                        var errorstring = newLocation.Substring((newLocation.IndexOf(Constants.STR_MIC_REDIRECT_ERROR) + Constants.STR_MIC_REDIRECT_ERROR.Length));
                        ex.Error += errorstring.Substring(0, errorstring.IndexOf(Constants.CHAR_AMPERSAND));
                        throw ex;
                    }
                }

				String accesstoken = newLocation.Substring (codeIndex + 5); // TODO change "String" to "string" - use alias everywhere
				return await User.GetMICToken(cli, accesstoken, clientID).ExecuteAsync();
			}
		}

        // Request to get access token (Resource Owner Credentials Grant (Username and Password))
        internal class MicLoginRequest : AbstractKinveyClientRequest<JObject>
        {
            private const string REST_PATH = "oauth/token";

            internal MicLoginRequest(AbstractClient client, object httpContent) :
                base(client, client.MICHostName, "POST", REST_PATH, httpContent, new Dictionary<string, string>())
            {
                this.PayloadType = new URLEncodedPayload();
                RequireAppCredentials = true;
            }          
        }

        // A logout request
        private class LogoutRequest
        {
			private ICredentialStore store;
			private User memberUser;
			private bool hardLogout;

			internal LogoutRequest(ICredentialStore store, User user, bool hardLogout = true)
			{
				this.memberUser = user;
				this.store = store;
				this.hardLogout = hardLogout;
			}

			internal void Execute()
			{
				if (hardLogout)
				{
					// delete cache and sync queue
					ICacheManager cm = ((Client)memberUser.KinveyClient).CacheManager;
					cm?.clearStorage();

					CredentialManager manager = new CredentialManager(this.store);
					var userId = memberUser.id;

					if (userId != null)
					{
						manager.RemoveCredential(userId, memberUser.KinveyClient.SSOGroupKey);
					}
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

                    auth.UserId = u.Id;
                    auth.UserMetaData = u.Metadata;
                    auth.username = u.UserName;
                    auth.Attributes = u.Attributes;

					string utype = user.type.ToString();
				
					return this.user.UpdateUser(auth, utype);
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

		// Build request to register for realtime
		private class RealtimeRegisterRequest : AbstractKinveyClientRequest<JObject>
		{
			private const string REST_PATH = "user/{appKey}/{userID}/register-realtime";

			internal RealtimeRegisterRequest(AbstractClient client, string deviceID, Dictionary<string, string> urlProperties) :
				base(client, "POST", REST_PATH, default(JObject), urlProperties)
			{
				JObject requestPayload = new JObject();
				requestPayload.Add(Constants.STR_REALTIME_DEVICEID, deviceID);
				base.HttpContent = requestPayload;
			}
		}

		// Build request to unregister for realtime
		private class RealtimeUnregisterRequest : AbstractKinveyClientRequest<JObject>
		{
			private const string REST_PATH = "user/{appKey}/{userID}/unregister-realtime";

			internal RealtimeUnregisterRequest(AbstractClient client, string deviceID, Dictionary<string, string> urlProperties) :
				base(client, "POST", REST_PATH, default(JObject), urlProperties)
			{
				JObject requestPayload = new JObject();
				requestPayload.Add(Constants.STR_REALTIME_DEVICEID, deviceID);
				base.HttpContent = requestPayload;
			}
		}

		#endregion

		#region User class - Private helper methods

		// Update necessary fields of the active user object
		// Do not update ID or user client.
		private void UpdateActiveUser(User u)
		{
			this.KinveyClient.ActiveUser.Attributes = u.Attributes;
			this.KinveyClient.ActiveUser.UserName = u.UserName;
			this.KinveyClient.ActiveUser.Metadata = u.Metadata;
		}

		#endregion
	}
}
