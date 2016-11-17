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
using Android.Accounts;
using Android.Content;
using Newtonsoft.Json;

namespace Kinvey
{
	/// <summary>
	/// Android native credential store.
	/// </summary>
	public class AndroidNativeCredentialStore : NativeCredentialStore
	{
		private Context appContext;
		private AccountManager accountManager;

		#region NativeStoreCredential implementation

		/// <summary>
		/// Initializes a new instance of the <see cref="T:KinveyXamarin.AndroidNativeCredentialStore"/> class.
		/// </summary>
		/// <param name="context">App context.</param>
		public AndroidNativeCredentialStore(Context context)
		{
			appContext = context;
			accountManager = AccountManager.Get(appContext);
		}

		/// <summary>
		/// Load the specified userID.
		/// </summary>
		/// <param name="userID">User identifier.</param>
		/// <param name="ssoGroupKey">SSO Group Key.</param>
		override public Credential Load(string userID, string ssoGroupKey)
		{
			Credential credential = null;

			try
			{
				NativeCredential nc = null;

				var credentials = FindCredentialsForOrg(ssoGroupKey);

				foreach (var c in credentials)
				{
					if (userID.Equals(string.Empty) ||
						userID.Equals(c.UserID))
					{
						nc = c;
						break;
					}
				}

				if (nc != null)
				{
					credential = Credential.From(nc);
				}
			}
			catch (System.Exception e)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_USER, EnumErrorCode.ERROR_USER_LOAD_CREDENTIAL, "", e);
			}

			return credential;
		}

		/// <summary>
		/// Store the specified userID and credential.
		/// </summary>
		/// <param name="userID">User identifier.</param>
		/// <param name="ssoGroupKey">SSO Group Key.</param>
		/// <param name="credential">Credential.</param>
		override public void Store(string userID, string ssoGroupKey, Credential credential)
		{
			Dictionary<string, string> properties = new Dictionary<string, string>();
			properties.Add(Constants.STR_ACCESS_TOKEN, (credential.AccessToken ?? string.Empty));
			properties.Add(Constants.STR_AUTH_TOKEN, (credential.AuthToken ?? string.Empty));
			properties.Add(Constants.STR_REFRESH_TOKEN, (credential.RefreshToken ?? string.Empty));
			properties.Add(Constants.STR_REDIRECT_URI, (credential.RedirectUri ?? string.Empty));
			properties.Add(Constants.STR_USERNAME, (credential.UserName ?? string.Empty));

			properties.Add(Constants.STR_ATTRIBUTES, (credential.Attributes != null ?
										  JsonConvert.SerializeObject(credential.Attributes) :
										  string.Empty));

			properties.Add(Constants.STR_USER_KMD, (credential.UserKMD != null ?
									   JsonConvert.SerializeObject(credential.UserKMD) :
									   string.Empty));


			NativeCredential nc = new NativeCredential(userID, properties);

			try
			{
				SaveNativeCredential(nc, ssoGroupKey);
			}
			catch (System.Exception e)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_USER, EnumErrorCode.ERROR_USER_STORE_CREDENTIAL, "", e);
			}
		}

		/// <summary>
		/// Delete the specified userID.
		/// </summary>
		/// <param name="userID">User identifier.</param>
		/// <param name="ssoGroupKey">SSO Group Key.</param>
		override public void Delete(string userID, string ssoGroupKey)
		{
			var nativeCredEnumeration = FindCredentialsForOrg(ssoGroupKey);
			foreach (var nc in nativeCredEnumeration)
			{
				if (nc.UserID.Equals(userID))
				{
					Account[] accounts = accountManager.GetAccountsByType(ssoGroupKey);

					foreach (var account in accounts)
					{
						if (account.Type.Equals(ssoGroupKey))
						{
							accountManager.RemoveAccountExplicitly(account);
						}
					}
					break;
				}
			}
		}

		/// <summary>
		/// Gets the active user.
		/// </summary>
		/// <returns>The active user.</returns>
		/// <param name="ssoGroupKey">SSO Group Key.</param>
		override public Credential GetStoredCredential(string ssoGroupKey)
		{
			return Load(string.Empty, ssoGroupKey);
		}

		#endregion

		#region Helper methods

		private IEnumerable<NativeCredential> FindCredentialsForOrg(string ssoGroupKey)
		{
			List<NativeCredential> credentials = new List<NativeCredential>();

			Account[] accounts = accountManager.GetAccountsByType(ssoGroupKey);

			foreach (var account in accounts)
			{
				if (account.Type.Equals(ssoGroupKey))
				{
					credentials.Add(GetCredentialFromAccount(account));
				}
			}

			return credentials;
		}

		private void SaveNativeCredential(NativeCredential nativeCredential, string ssoGroupKey)
		{
			var serializedCredential = nativeCredential.Serialize();

			// If there exists a credential, delete before writing new credential
			var existingCredential = FindCredential(nativeCredential.UserID, ssoGroupKey);
			if (existingCredential != null)
			{
				accountManager.RemoveAccountExplicitly(new Account(existingCredential.UserID, ssoGroupKey));
			}

			// Add new credential
			Account account = new Account(nativeCredential.UserID, ssoGroupKey);
			Android.OS.Bundle bundle = new Android.OS.Bundle();
			bundle.PutString(Constants.STR_CREDENTIAL, serializedCredential);
			accountManager.AddAccountExplicitly(account, "", bundle);
		}

		private NativeCredential FindCredential(string username, string ssoGroupKey)
		{
			NativeCredential nc = null;

			Account[] accounts = accountManager.GetAccountsByType(ssoGroupKey);

			foreach (var account in accounts)
			{
				if (account.Type.Equals(ssoGroupKey))
				{
					nc = GetCredentialFromAccount(account);
				}
			}

			return nc;
		}

		private NativeCredential GetCredentialFromAccount(Account account)
		{
			NativeCredential nc = null;
			try
			{
				var serializedNativeCredential = accountManager.GetUserData(account, Constants.STR_CREDENTIAL);
				nc = NativeCredential.Deserialize(serializedNativeCredential);
			}
			catch (Exception e)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_USER, EnumErrorCode.ERROR_USER_GET_CREDENTIAL_FOR_ACCOUNT, "", e);
			}

			return nc;
		}

		#endregion
	}

	/// <summary>
	/// Kinvey account authenticator.
	/// </summary>
	public class KinveyAccountAuthenticator : AbstractAccountAuthenticator
	{
		Context ctx;

		public KinveyAccountAuthenticator(Context context)
			: base(context)
		{
			ctx = context;
		}

		/// <summary>
		/// Adds the account.
		/// </summary>
		/// <returns>The account.</returns>
		/// <param name="response">Response.</param>
		/// <param name="accountType">Account type.</param>
		/// <param name="authTokenType">Auth token type.</param>
		/// <param name="requiredFeatures">Required features.</param>
		/// <param name="options">Options.</param>
		public override Android.OS.Bundle AddAccount(AccountAuthenticatorResponse response, string accountType, string authTokenType, string[] requiredFeatures, Android.OS.Bundle options)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Gets the auth token.
		/// </summary>
		/// <returns>The auth token.</returns>
		/// <param name="response">Response.</param>
		/// <param name="account">Account.</param>
		/// <param name="authTokenType">Auth token type.</param>
		/// <param name="options">Options.</param>
		public override Android.OS.Bundle GetAuthToken(AccountAuthenticatorResponse response, Account account, string authTokenType, Android.OS.Bundle options)
		{
			throw new System.NotImplementedException();
		}






		public override Android.OS.Bundle HasFeatures(AccountAuthenticatorResponse response, Account account, string[] features)
		{
			throw new System.NotImplementedException();
		}
		public override Android.OS.Bundle EditProperties(AccountAuthenticatorResponse response, string accountType)
		{
			throw new System.NotImplementedException();
		}
		public override Android.OS.Bundle ConfirmCredentials(AccountAuthenticatorResponse response, Account account, Android.OS.Bundle options)
		{
			throw new System.NotImplementedException();
		}
		public override Android.OS.Bundle UpdateCredentials(AccountAuthenticatorResponse response, Account account, string authTokenType, Android.OS.Bundle options)
		{
			throw new System.NotImplementedException();
		}
		public override string GetAuthTokenLabel(string authTokenType)
		{
			throw new System.NotImplementedException();
		}
	}
}
