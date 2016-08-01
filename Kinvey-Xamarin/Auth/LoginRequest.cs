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

using System.Threading.Tasks;

namespace KinveyXamarin
{
	internal class LoginRequest
	{
		Credential credential;
		EnumLoginType type;
		protected KinveyAuthRequest request;
		protected User memberUser;

		internal LoginRequest(User user)
		{
			memberUser = user;
			memberUser.builder.Create = true;
			this.type = user.type;
		}

		internal LoginRequest(string username, string password, bool setCreate, User user)
		{
			this.memberUser = user;
			memberUser.builder.Username = username;
			memberUser.builder.Password = password;
			memberUser.builder.Create = setCreate;
			memberUser.builder.KinveyUser = user;
			this.type = user.type;
		}

		internal LoginRequest(Credential credential, User user)
		{
			this.memberUser = user;
			this.credential = credential;
			this.type = user.type;
		}

		internal LoginRequest(ThirdPartyIdentity identity, User user)
		{
			this.memberUser = user;
			this.memberUser.builder.Identity = identity;
			this.type = user.type;
			this.memberUser.builder.Create = false;
		}

		internal LoginRequest buildAuthRequest()
		{
			this.request = memberUser.builder.build();
			return this;
		}

		internal async Task<User> ExecuteAsync()
		{
			if (memberUser.isUserLoggedIn() &&
				memberUser.type != EnumLoginType.CREDENTIALSTORE)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_USER, EnumErrorCode.ERROR_USER_ALREADY_LOGGED_IN, "");
			}

			string userType = "";
			if (this.type == EnumLoginType.CREDENTIALSTORE)
			{
				return memberUser.InitUser(credential);
			}
			else
			{
				userType = this.type.ToString();
			}

			KinveyAuthResponse response = await this.request.ExecuteAsync();

			return memberUser.InitUser(response, userType);
		}
	}
}
