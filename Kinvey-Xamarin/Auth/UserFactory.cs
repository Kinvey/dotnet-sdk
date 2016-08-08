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
	public class UserFactory
	{
		public AbstractClient Client { get; }

		public KinveyAuthRequest.Builder AuthRequestBuilder { get; }

		public UserFactory(AbstractClient client)
		{
			this.Client = client;

			var appKey = ((KinveyClientRequestInitializer)Client.RequestInitializer).AppKey;
			var appSecret = ((KinveyClientRequestInitializer)Client.RequestInitializer).AppSecret;

			this.AuthRequestBuilder = new KinveyAuthRequest.Builder(Client, appKey, appSecret);
		}

		internal LoginRequest BuildCreateRequest(string username, string password, Dictionary<string, JToken> customFieldsAndValues = null)
		{
			//this.type = EnumLoginType.KINVEY;
			if (customFieldsAndValues != null)
			{
				foreach (KeyValuePair<string, JToken> entry in customFieldsAndValues)
				{
					// TODO add back in
					//this.Attributes.Add(entry.Key, entry.Value);
				}
			}

			//LoginRequest loginRequest = uc.UserFactory.BuildLoginRequest(username, password);
			return new LoginRequest(Client, AuthRequestBuilder, EnumLoginType.KINVEY, username, password, true).BuildAuthRequest();
		}

		internal LoginRequest BuildLoginRequest()
		{
			//this.type = EnumLoginType.IMPLICIT;
			return new LoginRequest(Client, AuthRequestBuilder, EnumLoginType.IMPLICIT).BuildAuthRequest();
		}

		internal LoginRequest BuildLoginRequest(string username, string password)
		{
			//this.type = EnumLoginType.KINVEY;
			return new LoginRequest(Client, AuthRequestBuilder, EnumLoginType.KINVEY, username, password, false).BuildAuthRequest();
		}

		internal LoginRequest BuildLoginRequest(Credential cred)
		{
			//this.type = EnumLoginType.CREDENTIALSTORE;
			return new LoginRequest(Client, AuthRequestBuilder, EnumLoginType.CREDENTIALSTORE, cred).BuildAuthRequest();
		}

		internal LoginRequest BuildLoginRequest(ThirdPartyIdentity identity)
		{
			//this.type = EnumLoginType.THIRDPARTY;
			return new LoginRequest(Client, AuthRequestBuilder, EnumLoginType.THIRDPARTY, identity).BuildAuthRequest();
		}

		internal MICLoginRequest BuildMICLoginRequest(ThirdPartyIdentity identity)
		{
			//this.type = EnumLoginType.THIRDPARTY;
			return new MICLoginRequest(Client, AuthRequestBuilder, EnumLoginType.THIRDPARTY, identity).buildAuthRequest();
		}
	}
}
