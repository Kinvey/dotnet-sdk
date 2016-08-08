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

namespace KinveyXamarin
{
	internal class LoginRequest
	{
		Credential credential;
		EnumLoginType type;
		protected KinveyAuthRequest.Builder builder;
		protected KinveyAuthRequest request;
		protected User memberUser;

		protected AbstractClient abstractClient;

		internal LoginRequest(AbstractClient client, KinveyAuthRequest.Builder builder, EnumLoginType loginType, User user = null)
		{
			this.abstractClient = client;
			//memberUser = user;
			this.builder = builder;
			this.builder.Create = true;

			if (user != null)
			{
				this.abstractClient.ActiveUser.builder.KinveyUser = user;
			}

			this.type = loginType;
		}

		internal LoginRequest(AbstractClient client, KinveyAuthRequest.Builder builder, EnumLoginType loginType, string username, string password, bool setCreate, User user = null)
		{
			this.abstractClient = client;
			//this.memberUser = user;
			this.builder = builder;
			this.builder.Username = username;
			this.builder.Password = password;
			this.builder.Create = setCreate;

			if (user != null)
			{
				this.abstractClient.ActiveUser.builder.KinveyUser = user;
			}

			this.type = loginType;
		}

		internal LoginRequest(AbstractClient client, KinveyAuthRequest.Builder builder, EnumLoginType loginType, ThirdPartyIdentity identity, User user = null)
		{
			this.abstractClient = client;
			//this.memberUser = user;
			this.builder = builder;
			this.builder.Identity = identity;
			this.builder.Create = false;

			if (user != null)
			{
				this.abstractClient.ActiveUser.builder.KinveyUser = user;
			}

			this.type = loginType;
		}

		internal LoginRequest(AbstractClient client, KinveyAuthRequest.Builder builder, EnumLoginType loginType, Credential credential, User user = null)
		{
			this.abstractClient = client;
			//this.memberUser = user;
			this.builder = builder;
			this.credential = credential;

			if (user != null)
			{
				this.abstractClient.ActiveUser.builder.KinveyUser = user;
			}

			this.type = loginType;
		}

		internal LoginRequest BuildAuthRequest(Dictionary<string, JToken> customFieldsAndValues = null)
		{
			this.builder.CustomFieldsAndValues = customFieldsAndValues;
			this.request = this.builder.build();
			return this;
		}

		internal LoginRequest buildAuthRequest()
		{
			this.request = memberUser.builder.build();
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
			User u = new User(this.abstractClient);
			u.Id = cred.UserId;
			u.AuthToken = cred.AuthToken;

			//CredentialManager credentialManager = new CredentialManager(KinveyClient.Store);
			((KinveyClientRequestInitializer)this.abstractClient.RequestInitializer).KinveyCredential = cred;

			this.abstractClient.ActiveUser = u;
			return u;
		}

		private User InitUser(KinveyAuthResponse response, string userType) // TODO move to UserFactory?
		{
			User u = new User(this.abstractClient);

			u.Id = response.UserId;
			// TODO process Unknown keys
			// this.put("_kmd", response.getMetadata());
			// this.putAll(response.getUnknownKeys());

			//this.username = response
			u.AuthToken = response.AuthToken;
			u.Attributes = response.Attributes;
			u.Metadata = response.UserMetaData;

			CredentialManager credentialManager = new CredentialManager(this.abstractClient.Store);
			((KinveyClientRequestInitializer)this.abstractClient.RequestInitializer).KinveyCredential = credentialManager.CreateAndStoreCredential(response, u.Id);

			this.abstractClient.ActiveUser = u;
			return u;
		}
	}

	// A login request to MIC
	internal class MICLoginRequest : LoginRequest
	{
		internal MICLoginRequest(AbstractClient client, KinveyAuthRequest.Builder builder, EnumLoginType loginType, ThirdPartyIdentity identity, User user = null) :
			base(client, builder, loginType, identity, user)
		{
			memberUser.builder.Create = false;
		}

		internal MICLoginRequest buildAuthRequest()
		{
			base.buildAuthRequest();
			request.buildRequestPayload();
			return this;
		}
	}
}
