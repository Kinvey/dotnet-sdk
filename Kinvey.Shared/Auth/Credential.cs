﻿// Copyright (c) 2019, Kinvey, Inc. All rights reserved.
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

using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kinvey
{
	/// <summary>
	/// This is a credential object, storing authentication information
	/// </summary>
	[DataContract]
	public class Credential : IKinveyRequestInitializer
	{
		/// <summary>
		/// The user _id.
		/// </summary>
		[DataMember]
		private string userId;

		/// <summary>
		/// The auth token.
		/// </summary>
		[DataMember]
		private string authToken;

		/// <summary>
		/// The user name.
		/// </summary>
		[DataMember]
		private string userName;

        /// <summary>
        /// The _socialIdentity object.
        /// </summary>
        /// <value>The AuthSocialID  property gets/sets the value of the KinveyAuthSocialID field, _authSocialID .</value>
        [DataMember]
		public KinveyAuthSocialID AuthSocialID { get; set; }

        /// <summary>
        /// The access token.
        /// </summary>
        /// <value>The SecAuthToken property gets/sets the value of the byte[] field, _secAuthToken. </value>
        [DataMember]
		public byte[] SecAuthToken { get; set; }

        /// <summary>
        /// The access token.
        /// </summary>
        /// <value>The AccessToken property gets/sets the value of the string field, _accessToken.</value>
        [DataMember]
		public string AccessToken { get; set; }

        /// <summary>
        /// The refresh token.
        /// </summary>
        /// <value>The RefreshToken property gets/sets the value of the string field, _refreshToken.</value>
        [DataMember]
		public string RefreshToken { get; set; }

        /// <summary>
        /// The redirect uri.
        /// </summary>
        /// <value>The RedirectUri property gets/sets the value of the string field, _redirectUri.</value>
        [DataMember]
		public string RedirectUri { get; set; }

		/// <summary>
		/// The custom attributes for a user.
		/// </summary>
		[DataMember]
		private Dictionary<string, JToken> attributes;

		[DataMember]
		private KinveyUserMetaData userKMD;

        /// <summary>
        /// The device ID associated with this user.
        /// </summary>
        /// <value>The DeviceID property gets/sets the value of the string field, _deviceID.</value>
        [DataMember]
		public string DeviceID { get; set; }

		/// <summary>
		/// Gets or sets the MIC Client identifier.
		/// </summary>
		/// <value>The MIC client identifier.</value>
		[DataMember]
		public string MICClientID { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Credential"/> class.
		/// </summary>
		[Preserve]
		public Credential()
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="KinveyXamarin.Credential"/> class.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="accessToken">Access token</param>
        /// <param name="socialIdentity">Kinvey user metadata</param>
        /// <param name="authToken">Auth token</param>
        /// <param name="userName">User name</param>
        /// <param name="attributes">User attributes</param>
        /// <param name="kmd">Kinvey metadata</param>
        /// <param name="refreshToken">Refresh token</param>
        /// <param name="redirectURI">Redirect URI</param>
        /// <param name="deviceID">Device ID</param>
        /// <param name="micClientID">MIC Client ID</param>
        public Credential(string userId,
		                  string accessToken,
		                  KinveyAuthSocialID socialIdentity,
		                  string authToken,
		                  string userName,
		                  Dictionary<string, JToken> attributes,
		                  KinveyUserMetaData kmd,
		                  string refreshToken,
		                  string redirectURI,
		                  string deviceID,
		                  string micClientID)
		{
			this.userId = userId;
			this.AccessToken = accessToken;
			this.AuthSocialID = socialIdentity;
			this.authToken = authToken;
			this.userName = userName;
			this.attributes = attributes;
			this.userKMD = kmd;
			this.RefreshToken = refreshToken;
			this.RedirectUri = redirectURI;
			this.DeviceID = deviceID;
			this.MICClientID = micClientID;
		}
		/// <summary>
		/// Gets the user _id.
		/// </summary>
		/// <value>The user identifier.</value>
		public string UserId
		{
			get { return this.userId; }
			[Preserve]
			internal set { this.userId = value; }
		}

		/// <summary>
		/// Gets the auth token.
		/// </summary>
		/// <value>The auth token.</value>
		public string AuthToken
		{
			get { return this.authToken; }
			[Preserve]
			internal set { this.authToken = value; }
		}

        /// <summary>
        /// The UserName property represents the user name.
        /// </summary>
        /// <value>The UserName property gets the value of the string field, userName.</value>
        public string UserName
		{
			get { return this.userName; }
			[Preserve]
			internal set { this.userName = value; }
		}

        /// <summary>
		/// Gets the attributes.
		/// </summary>
		/// <value>The Attributes property gets the value of the Dictionary field, attributes.</value>
		public Dictionary<string, JToken> Attributes
		{
			get { return this.attributes; }
			[Preserve]
			internal set { this.attributes = value; }
		}

        /// <summary>The UserKMD property represents Kinvey user metadata.</summary>
        /// <value>The UserKMD property gets the value of the KinveyUserMetaData field, userKMD.</value>
        public KinveyUserMetaData UserKMD
		{
			get { return this.userKMD; }
			[Preserve]
			internal set { this.userKMD = value; }
		}

        /// <summary>
        /// Initialize the specified clientRequest with this credential.
        /// </summary>
        /// <param name="clientRequest">Client Request.</param>
        /// <param name="clientId">[optional] Client Id.</param>
        /// <typeparam name="T">The type of the Client Request</typeparam>
        public void Initialize<T>(AbstractKinveyClientRequest<T> clientRequest, string clientId = null)
		{
			if (authToken != null)
			{
				clientRequest.RequestAuth = new KinveyAuthenticator(authToken);
			}
		}

        /// <summary>
        /// Create a new Credential from a KinveyAuthResponse.
        /// </summary>
        /// <param name="response">The response of a Kinvey login/create request.</param>
        /// <returns>Credential object, storing authentication information.</returns>
        public static Credential From(KinveyAuthResponse response)
		{
			return new Credential(response.UserId, response.AccessToken, response.AuthSocialIdentity, response.AuthToken, response.username, response.Attributes, response.UserMetaData, null, null, null, null);
		}

        /// <summary>
        /// Create a new Credential from a Kinvey User object.
        /// </summary>
        /// <param name="user">User.</param>
        /// <returns>Credential object, storing authentication information.</returns>
        public static Credential From(User user)
		{
			return new Credential(user.Id, user.AccessToken, user.AuthSocialID, user.AuthToken, user.UserName, user.Attributes, user.Metadata, null, null, user.KinveyClient.DeviceID, null);
		}

        /// <summary>
        /// Creates new Credential from NativeCredential object.
        /// </summary>
        /// <param name="nc">Native credential.</param>
        /// <returns>Credential object, storing authentication information.</returns>
		public static Credential From(NativeCredential nc)
		{
			return new Credential(nc.UserID,
			                      nc.Properties[Constants.STR_ACCESS_TOKEN],
			                      null, // TODO add _socialIdentity object here
			                      nc.Properties[Constants.STR_AUTH_TOKEN],
			                      nc.Properties[Constants.STR_USERNAME],
			                      JsonConvert.DeserializeObject<Dictionary<string, JToken>>(nc.Properties[Constants.STR_ATTRIBUTES]),
			                      JsonConvert.DeserializeObject<KinveyUserMetaData>(nc.Properties[Constants.STR_USER_KMD]),
			                      nc.Properties[Constants.STR_REFRESH_TOKEN],
			                      nc.Properties[Constants.STR_REDIRECT_URI],
			                      null,
			                      null);
		}

		internal static Credential From(SQLCredential sqlcred)
		{
			Dictionary<string, JToken> attributes = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(sqlcred.Attributes);
			KinveyUserMetaData userKMD = JsonConvert.DeserializeObject<KinveyUserMetaData>(sqlcred.UserKMD);
			KinveyAuthSocialID socialIdentity = JsonConvert.DeserializeObject<KinveyAuthSocialID>(sqlcred.AuthSocialID);

			var credential = new Credential(sqlcred.UserID, sqlcred.AccessToken, socialIdentity, sqlcred.AuthToken, sqlcred.UserName, attributes, userKMD, sqlcred.RefreshToken, sqlcred.RedirectUri, sqlcred.DeviceID, sqlcred.MICClientID);

			credential.SecAuthToken = sqlcred.SecAuthToken;

			return credential;
		}

        /// <summary>
		/// Creates new Credential from Credential object and encrypted auth token.
		/// </summary>
		/// <param name="cred">Credential object, storing authentication information.</param>
        /// <param name="encryptedAuthToken">Encrypted auth token.</param>
        /// <returns>Credential object, storing authentication information</returns>
        public static Credential From(Credential cred, byte[] encryptedAuthToken)
		{
            var secCredential = new Credential(cred.userId, cred.AccessToken, cred.AuthSocialID, null, cred.userName, cred.attributes, cred.userKMD, cred.RefreshToken, cred.RedirectUri, cred.DeviceID, cred.MICClientID);
			secCredential.SecAuthToken = encryptedAuthToken;
			return secCredential;
		}

        /// <summary>
		/// Creates new Credential from Credential object and original auth token.
		/// </summary>
		/// <param name="cred">Credential object, storing authentication information.</param>
        /// <param name="originalAuthToken">Original auth token.</param>
        /// <returns>Credential object, storing authentication information</returns>
        public static Credential From(Credential cred, string originalAuthToken)
		{
            return new Credential(cred.userId, cred.AccessToken, cred.AuthSocialID, originalAuthToken, cred.userName, cred.attributes, cred.userKMD, cred.RefreshToken, cred.RedirectUri, cred.DeviceID, cred.MICClientID);
		}
	}
}
