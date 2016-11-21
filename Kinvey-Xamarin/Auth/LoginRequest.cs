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
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Kinvey
{
	internal class LoginRequest
	{
		Credential credential;
		EnumLoginType type;
		protected KinveyAuthRequest.Builder builder;
		protected KinveyAuthRequest request;

		protected AbstractClient abstractClient;

		internal LoginRequest(AbstractClient client, KinveyAuthRequest.Builder builder, EnumLoginType loginType)
		{
			this.abstractClient = client;
			this.builder = builder;
			this.builder.Create = true;
			this.type = loginType;
		}

		internal LoginRequest(AbstractClient client, KinveyAuthRequest.Builder builder, EnumLoginType loginType, string username, string password, bool setCreate)
		{
			this.abstractClient = client;
			this.builder = builder;
			this.builder.Username = username;
			this.builder.Password = password;
			this.builder.Create = setCreate;
			this.type = loginType;
		}

		internal LoginRequest(AbstractClient client, KinveyAuthRequest.Builder builder, EnumLoginType loginType, ThirdPartyIdentity identity)
		{
			this.abstractClient = client;
			this.builder = builder;
			this.builder.Identity = identity;
			this.builder.Create = false;
			this.type = loginType;
		}

		internal LoginRequest(AbstractClient client, KinveyAuthRequest.Builder builder, EnumLoginType loginType, Credential credential)
		{
			this.abstractClient = client;
			this.builder = builder;
			this.credential = credential;
			this.type = loginType;
		}

		internal LoginRequest BuildAuthRequest(Dictionary<string, JToken> customFieldsAndValues = null)
		{
			this.builder.CustomFieldsAndValues = customFieldsAndValues;
			this.request = this.builder.build();
			return this;
		}

		internal async Task<User> ExecuteAsync()
		{
			if (this.abstractClient.ActiveUser != null &&
				this.type != EnumLoginType.CREDENTIALSTORE)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_USER, EnumErrorCode.ERROR_USER_ALREADY_LOGGED_IN, "");
			}

			string userType = "";
			if (this.type == EnumLoginType.CREDENTIALSTORE)
			{
				return this.InitUser(credential);
			}
			else
			{
				userType = this.type.ToString();
			}

			KinveyAuthResponse response = await this.request.ExecuteAsync();

			return InitUser(response, userType);
		}

		private User InitUser(Credential cred) // TODO move to UserFactory?
		{
			((KinveyClientRequestInitializer)this.abstractClient.RequestInitializer).KinveyCredential = cred;

			abstractClient.ActiveUser = User.From(cred, abstractClient);

			return abstractClient.ActiveUser;
		}

		private User InitUser(KinveyAuthResponse response, string userType) // TODO move to UserFactory?
		{
			CredentialManager credentialManager = new CredentialManager(this.abstractClient.Store);

			Credential activeUserCredential = credentialManager.CreateAndStoreCredential(response, response.UserId, abstractClient.SSOGroupKey);

			((KinveyClientRequestInitializer)abstractClient.RequestInitializer).KinveyCredential = activeUserCredential;

			abstractClient.ActiveUser = User.From(activeUserCredential, abstractClient);

			return abstractClient.ActiveUser;
		}
	}
}
