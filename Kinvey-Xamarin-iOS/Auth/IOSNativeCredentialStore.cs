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

namespace KinveyXamarin
{
	/// <summary>
	/// iOS native credential store.
	/// </summary>
	public class IOSNativeCredentialStore : NativeCredentialStore
	{
		private const string SSO_ORG_TEST = "SSO_ORG_TEST";

		#region NativeStoreCredential implementation

		/// <summary>
		/// Load the specified userID.
		/// </summary>
		/// <param name="userID">User identifier.</param>
		override public Credential Load(string userID)
		{
			Credential credential = null;

			try
			{
				NativeCredential nc = null;

				var credentials = FindCredentialsForOrg(SSO_ORG_TEST);

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
		/// Store the specified userID and credential.
		/// </summary>
		/// <param name="userID">User identifier.</param>
		/// <param name="credential">Credential.</param>
		override public void Store(string userID, Credential credential)
		{
			Dictionary<string, string> properties = new Dictionary<string, string>();
			properties.Add("AccessToken", (credential.AccessToken ?? string.Empty));
			properties.Add("AuthToken", (credential.AuthToken ?? string.Empty));
			properties.Add("RefreshToken", (credential.RefreshToken ?? string.Empty));
			properties.Add("RedirectUri", (credential.RedirectUri ?? string.Empty));
			properties.Add("UserName", (credential.UserName ?? string.Empty));

			properties.Add("Attributes", (credential.Attributes != null ?
			                              JsonConvert.SerializeObject(credential.Attributes) :
			                              string.Empty));

			properties.Add("UserKMD", (credential.UserKMD != null ?
			                           JsonConvert.SerializeObject(credential.UserKMD) :
			                           string.Empty));

			NativeCredential nc = new NativeCredential(userID, properties);

			try
			{
				SaveNativeCredential(nc, SSO_ORG_TEST);
			}
			catch (System.Exception e)
			{
				string msg = e.Message;
			}
		}

		/// <summary>
		/// Delete the specified userID and orgID.
		/// </summary>
		/// <param name="userID">User identifier.</param>
		override public void Delete(string userID)
		{
			SecRecord query = new SecRecord(SecKind.GenericPassword);
			query.Service = SSO_ORG_TEST;

			var nativeCredEnumeration = FindCredentialsForOrg(SSO_ORG_TEST);
			foreach (var nc in nativeCredEnumeration)
			{
				if (nc.UserID.Equals(userID))
				{
					query.Account = nc.UserID;
					break;
				}
			}

			var statusCode = SecKeyChain.Remove(query);

			if (statusCode != SecStatusCode.Success &&
			    statusCode != SecStatusCode.ItemNotFound)
			{
				throw new System.Exception("Could not delete account from KeyChain: " + statusCode);
			}
		}

		/// <summary>
		/// Gets the active user.
		/// </summary>
		/// <returns>The active user.</returns>
		override public Credential GetActiveUser()
		{
			return Load(string.Empty);
		}

		#endregion

		#region Helper methods

		private IEnumerable<NativeCredential> FindCredentialsForOrg(string orgID)
		{
			List<NativeCredential> credentials = new List<NativeCredential>();

			var query = new SecRecord(SecKind.GenericPassword);
			query.Service = orgID;

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

		private void SaveNativeCredential(NativeCredential nativeCredential, string orgID)
		{
			var statusCode = SecStatusCode.Success;
			var serializedCredential = nativeCredential.Serialize();
			var data = NSData.FromString(serializedCredential, NSStringEncoding.UTF8);

			// If there exists a credential, delete before writing new credential
			var existingCredential = FindCredential(nativeCredential.UserID, orgID);
			if (existingCredential != null)
			{
				var query = new SecRecord(SecKind.GenericPassword);
				query.Service = orgID;
				query.Account = nativeCredential.UserID;

				statusCode = SecKeyChain.Remove(query);
				if (statusCode != SecStatusCode.Success)
				{
					throw new System.Exception("Could not save account from KeyChain: " + statusCode);
				}
			}

			// Add new credential
			var record = new SecRecord(SecKind.GenericPassword);
			record.Service = orgID;
			record.Account = nativeCredential.UserID;
			record.Generic = data;
			record.Accessible = SecAccessible.WhenUnlocked;

			statusCode = SecKeyChain.Add(record);

			if (statusCode != SecStatusCode.Success)
			{
				throw new System.Exception("Could not save account from KeyChain: " + statusCode);
			}
		}

		private NativeCredential FindCredential(string username, string orgID)
		{
			NativeCredential nc = null;

			var query = new SecRecord(SecKind.GenericPassword);
			query.Service = orgID;
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
	}
}
