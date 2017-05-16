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
		/// The access token.
		/// </summary>
		[DataMember]
		public byte[] SecAuthToken { get; set; }

		/// <summary>
		/// The access token.
		/// </summary>
		[DataMember]
		public string AccessToken { get; set; }

		/// <summary>
		/// The refresh token.
		/// </summary>
		[DataMember]
		public string RefreshToken { get; set; }

		/// <summary>
		/// The redirect uri.
		/// </summary>
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
		[DataMember]
		public string DeviceID { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.Credential"/> class.
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
		/// <param name="authToken">Auth token</param>
		/// <param name="userName">User name</param>
		/// <param name="attributes">User attributes</param>
		/// <param name="kmd">Kinvey metadata</param>
		/// <param name="refreshToken">Refresh token</param>
		/// <param name="redirectURI">Redirect URI</param>
		public Credential(string userId,
						  string accessToken,
						  string authToken,
						  string userName,
						  Dictionary<string, JToken> attributes,
						  KinveyUserMetaData kmd,
						  string refreshToken,
						  string redirectURI,
						  string deviceID)
		{
			this.userId = userId;
			this.AccessToken = accessToken;
			this.authToken = authToken;
			this.userName = userName;
			this.attributes = attributes;
			this.userKMD = kmd;
			this.RefreshToken = refreshToken;
			this.RedirectUri = redirectURI;
			this.DeviceID = deviceID;
		}
		/// <summary>
		/// Gets or sets the user _id.
		/// </summary>
		/// <value>The user identifier.</value>
		public string UserId
		{
			get { return this.userId; }
			[Preserve]
			internal set { this.userId = value; }
		}

		/// <summary>
		/// Gets or sets the auth token.
		/// </summary>
		/// <value>The auth token.</value>
		public string AuthToken
		{
			get { return this.authToken; }
			[Preserve]
			internal set { this.authToken = value; }
		}

		/// <summary>
		/// Gets or sets the auth token.
		/// </summary>
		/// <value>The auth token.</value>
		public string UserName
		{
			get { return this.userName; }
			[Preserve]
			internal set { this.userName = value; }
		}

		public Dictionary<string, JToken> Attributes
		{
			get { return this.attributes; }
			[Preserve]
			internal set { this.attributes = value; }
		}

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
		/// <typeparam name="T">The type of the Client Request</typeparam>
		public void Initialize<T>(AbstractKinveyClientRequest<T> clientRequest)
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
		public static Credential From(KinveyAuthResponse response)
		{
			return new Credential(response.UserId, response.AccessToken, response.AuthToken, response.username, response.Attributes, response.UserMetaData, null, null, null);
		}

		/// <summary>
		/// Create a new Credential from a Kinvey User object.
		/// </summary>
		/// <param name="user">User.</param>
		public static Credential From(User user)
		{
			return new Credential(user.Id, user.AccessToken, user.AuthToken, user.UserName, user.Attributes, user.Metadata, null, null, user.KinveyClient.DeviceID);
		}

		public static Credential From(NativeCredential nc)
		{
			return new Credential(nc.UserID,
								  nc.Properties[Constants.STR_ACCESS_TOKEN],
								  nc.Properties[Constants.STR_AUTH_TOKEN],
								  nc.Properties[Constants.STR_USERNAME],
								  JsonConvert.DeserializeObject<Dictionary<string, JToken>>(nc.Properties[Constants.STR_ATTRIBUTES]),
								  JsonConvert.DeserializeObject<KinveyUserMetaData>(nc.Properties[Constants.STR_USER_KMD]),
								  nc.Properties[Constants.STR_REFRESH_TOKEN],
								  nc.Properties[Constants.STR_REDIRECT_URI],
								  null);
		}

		internal static Credential From(SQLCredential sqlcred)
		{
			Dictionary<string, JToken> attributes = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(sqlcred.Attributes);
			KinveyUserMetaData userKMD = JsonConvert.DeserializeObject<KinveyUserMetaData>(sqlcred.UserKMD);
			var credential = new Credential(sqlcred.UserID, sqlcred.AccessToken, sqlcred.AuthToken, sqlcred.UserName, attributes, userKMD, sqlcred.RefreshToken, sqlcred.RedirectUri, sqlcred.DeviceID);
			credential.SecAuthToken = sqlcred.SecAuthToken;
			return credential;
		}

		public static Credential From(Credential cred, byte[] encryptedAuthToken)
		{
			var secCredential = new Credential(cred.userId, cred.AccessToken, null, cred.userName, cred.attributes, cred.userKMD, cred.RefreshToken, cred.RedirectUri, cred.DeviceID);
			secCredential.SecAuthToken = encryptedAuthToken;
			return secCredential;
		}

		public static Credential From(Credential cred, string originalAuthToken)
		{
			return new Credential(cred.userId, cred.AccessToken, originalAuthToken, cred.userName, cred.attributes, cred.userKMD, cred.RefreshToken, cred.RedirectUri, cred.DeviceID);
		}
	}
}
