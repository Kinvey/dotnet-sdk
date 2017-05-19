// Copyright (c) 2016, Kinvey, Inc. All rights reserved.
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
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Kinvey
{
	/// <summary>
	/// User request factory.
	/// </summary>
	public class UserRequestFactory
	{
		/// <summary>
		/// Gets the client associated with this user request factory.
		/// </summary>
		/// <value>The client.</value>
		public AbstractClient Client { get; }

		/// <summary>
		/// Gets the auth request builder associated with this user request factory.
		/// </summary>
		/// <value>The auth request builder.</value>
		public KinveyAuthRequest.Builder AuthRequestBuilder { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:KinveyXamarin.UserRequestFactory"/> class.
		/// </summary>
		/// <param name="client">The Kinvey client object associated with this user request factory.</param>
		public UserRequestFactory(AbstractClient client)
		{
			Client = client;

			var appKey = ((KinveyClientRequestInitializer)Client.RequestInitializer).AppKey;
			var appSecret = ((KinveyClientRequestInitializer)Client.RequestInitializer).AppSecret;

			AuthRequestBuilder = new KinveyAuthRequest.Builder(Client, appKey, appSecret);
		}

		#region LoginRequest factory methods

		internal LoginRequest BuildCreateRequest(string username, string password, Dictionary<string, JToken> customFieldsAndValues = null)
		{
			return new LoginRequest(Client, AuthRequestBuilder, EnumLoginType.KINVEY, username, password, true).BuildAuthRequest(customFieldsAndValues);
		}

		internal LoginRequest BuildLoginRequest()
		{
			return new LoginRequest(Client, AuthRequestBuilder, EnumLoginType.IMPLICIT).BuildAuthRequest();
		}

		internal LoginRequest BuildLoginRequest(string username, string password)
		{
			return new LoginRequest(Client, AuthRequestBuilder, EnumLoginType.KINVEY, username, password, false).BuildAuthRequest();
		}

		internal LoginRequest BuildLoginRequest(ThirdPartyIdentity identity)
		{
			return new LoginRequest(Client, AuthRequestBuilder, EnumLoginType.THIRDPARTY, identity).BuildAuthRequest();
		}

		internal MICLoginRequest BuildMICLoginRequest(ThirdPartyIdentity identity)
		{
			return new MICLoginRequest(Client, AuthRequestBuilder, EnumLoginType.THIRDPARTY, identity).buildAuthRequest();
		}

		internal LoginRequest BuildLoginRequest(Credential cred)
		{
			return new LoginRequest(Client, AuthRequestBuilder, EnumLoginType.CREDENTIALSTORE, cred).BuildAuthRequest();
		}

		internal LoginRequest BuildLoginRequestWithKinveyAuthToken(string userID, string authToken)
		{
			return BuildLoginRequest(new Credential(userID, null, null, authToken, null, null, null, null, null, Client.DeviceID));
		}

		#endregion

		#region User CRUD request factory methods

		internal UserExistenceRequest BuildUserExistenceRequest(string username)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)Client.RequestInitializer).AppKey);

			UserExistenceRequest existenceRequest = new UserExistenceRequest(username, Client, urlParameters);

			Client.InitializeRequest(existenceRequest);
			return existenceRequest;
		}

		internal ForgotUsernameRequest BuildForgotUsernameRequest(string email)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)Client.RequestInitializer).AppKey);

			ForgotUsernameRequest forgotUsernameRequest = new ForgotUsernameRequest(email, Client, urlParameters);

			Client.InitializeRequest(forgotUsernameRequest);
			return forgotUsernameRequest;
		}

		#endregion
	}

	#region User request classes

	// Build request to determine if a username already exists
	internal class UserExistenceRequest : AbstractKinveyClientRequest<JObject>
	{
		private const string REST_PATH = "rpc/{appKey}/check-username-exists";

		internal UserExistenceRequest(string username, AbstractClient client, Dictionary<string, string> urlProperties) :
			base(client, "POST", REST_PATH, default(JObject), urlProperties)
		{
			this.RequireAppCredentials = true;

			JObject requestPayload = new JObject();
			requestPayload.Add("username", username);
			base.HttpContent = requestPayload;
		}
	}

	// Build request to send an email with forgotten username information
	internal class ForgotUsernameRequest : AbstractKinveyClientRequest<JObject>
	{
		private const string REST_PATH = "rpc/{appKey}/user-forgot-username";

		internal ForgotUsernameRequest(string email, AbstractClient client, Dictionary<string, string> urlProperties) :
			base(client, "POST", REST_PATH, default(JObject), urlProperties)
		{
			this.RequireAppCredentials = true;

			JObject requestPayload = new JObject();
			requestPayload.Add("email", email);
			base.HttpContent = requestPayload;
		}
	}

	#endregion
}
