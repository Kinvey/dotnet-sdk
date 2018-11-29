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
using Foundation;
using Security;
using Newtonsoft.Json;
using System;

namespace Kinvey
{
	/// <summary>
	/// iOS native credential store.
	/// </summary>
	public class IOSNativeCredentialStore : NativeCredentialStore, IDisposable
    {
		#region NativeStoreCredential implementation

		/// <summary>
		/// Load the credential associted with the specified user ID.
		/// </summary>
		/// <param name="userID">User identifier used to access appropriate credential.</param>
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
				string msg = e.Message;
			}

			return credential;
		}

		/// <summary>
		/// Store the credential specified by the user ID.
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
				string msg = e.Message;
			}
		}

		/// <summary>
		/// Delete the specified credential based on user ID and SSO group key.
		/// </summary>
		/// <param name="userID">User identifier.</param>
		/// <param name="ssoGroupKey">SSO Group Key.</param>
		override public void Delete(string userID, string ssoGroupKey)
		{
			var nativeCredEnumeration = FindCredentialsForOrg(ssoGroupKey);

			foreach (var nc in nativeCredEnumeration)
			{
				//if (nc.UserID.Equals(userID))
				//{
					//query.Account = nc.UserID;
					//break;
				//}

				SecRecord query = new SecRecord(SecKind.GenericPassword);
				query.Service = ssoGroupKey;
				query.Account = nc.UserID;

				var statusCode = SecKeyChain.Remove(query);

				if (statusCode != SecStatusCode.Success &&
					statusCode != SecStatusCode.ItemNotFound)
				{
					throw new KinveyException(EnumErrorCategory.ERROR_USER, EnumErrorCode.ERROR_MIC_CREDENTIAL_DELETE, statusCode.ToString());
				}
			}
		}

		/// <summary>
		/// Gets the stored credential based on the SSO group key given.
		/// If found, this credential represents the active user.
		/// </summary>
		/// <returns>The stored credential for this SSO group key, if it exists.</returns>
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

			var query = new SecRecord(SecKind.GenericPassword);
			query.Service = ssoGroupKey;

			SecStatusCode result;
			var records = SecKeyChain.QueryAsRecord(query, 10, out result);

			if (records != null)
			{
				foreach (var record in records)
				{
					credentials.Add(GetCredentialFromRecord(record));
				}
			}

			return credentials;
		}

		private void SaveNativeCredential(NativeCredential nativeCredential, string ssoGroupKey)
		{
			var statusCode = SecStatusCode.Success;
			var serializedCredential = nativeCredential.Serialize();
			var data = NSData.FromString(serializedCredential, NSStringEncoding.UTF8);

			// If there exists a credential, delete before writing new credential
			var existingCredential = FindCredential(nativeCredential.UserID, ssoGroupKey);
			if (existingCredential != null)
			{
				var query = new SecRecord(SecKind.GenericPassword);
				query.Service = ssoGroupKey;
				query.Account = nativeCredential.UserID;

				statusCode = SecKeyChain.Remove(query);
				if (statusCode != SecStatusCode.Success)
				{
					throw new KinveyException(EnumErrorCategory.ERROR_USER, EnumErrorCode.ERROR_MIC_CREDENTIAL_SAVE, statusCode.ToString());
				}
			}

			// Add new credential
			var record = new SecRecord(SecKind.GenericPassword);
			record.Service = ssoGroupKey;
			record.Account = nativeCredential.UserID;
			record.Generic = data;
			record.Accessible = SecAccessible.WhenUnlocked;

			statusCode = SecKeyChain.Add(record);

			if (statusCode != SecStatusCode.Success)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_USER, EnumErrorCode.ERROR_MIC_CREDENTIAL_SAVE, statusCode.ToString());
			}
		}

		private NativeCredential FindCredential(string username, string ssoGroupKey)
		{
			NativeCredential nc = null;

			var query = new SecRecord(SecKind.GenericPassword);
			query.Service = ssoGroupKey;
			query.Account = username;

			SecStatusCode result;
			var record = SecKeyChain.QueryAsRecord(query, out result);

			if (record != null)
			{
				nc = GetCredentialFromRecord(record);
			}

			return nc;
		}

		private NativeCredential GetCredentialFromRecord(SecRecord record)
		{
			var serializedNativeCredential = NSString.FromData(record.Generic, NSStringEncoding.UTF8);
			return NativeCredential.Deserialize(serializedNativeCredential);
		}

        #endregion

        #region IDisposable Support

        public override void Dispose()
        {
        }

        #endregion

    }
}
