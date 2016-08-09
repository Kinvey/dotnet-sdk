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

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace KinveyXamarin
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
			return BuildLoginRequest(new Credential(userID, authToken, null, null, null, null, null));
		}
	}
}
