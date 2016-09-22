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

using System.IO;
using SQLite.Net.Interop;
using SQLite.Net;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace KinveyXamarin
{
	/// <summary>
	/// SQ lite credential store.
	/// </summary>
	public class SQLiteCredentialStore : ICredentialStore
	{
		/// <summary>
		/// The db connection.
		/// </summary>
		private SQLiteConnection _dbConnection;

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.SQLiteCredentialStore"/> class.
		/// </summary>
		/// <param name="platform">Platform.</param>
		/// <param name="filepath">Filepath.</param>
		public SQLiteCredentialStore (ISQLitePlatform platform, string filepath)
		{
			string dbPath = Path.Combine (filepath, "kinvey_tokens.sqlite");
			if (_dbConnection == null)
			{
				_dbConnection = new SQLiteConnection (platform, dbPath);
				_dbConnection.CreateTable<SQLCredential>();
			}
		}

		#region ICredentialStore implementation

		/// <summary>
		/// Load the specified userId.
		/// </summary>
		/// <param name="userId">User identifier.</param>
		public Credential Load (string userId)
		{
			SQLCredential sqlcred = _dbConnection.Table<SQLCredential> ().Where (t => t.UserID == userId).FirstOrDefault ();
			Credential cred = null;
			if (sqlcred != null)
			{
				Dictionary<string, JToken> attributes = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(sqlcred.Attributes);
				KinveyUserMetaData userKMD = JsonConvert.DeserializeObject<KinveyUserMetaData>(sqlcred.UserKMD);
				cred =  new Credential (sqlcred.UserID, sqlcred.AccessToken, sqlcred.AuthToken, sqlcred.UserName, attributes, userKMD, sqlcred.RefreshToken, sqlcred.RedirectUri);
			}

			// VRG try Xamarin.Auth
			Credential ssocred = LoadAccount(userId);
			if (cred != null)
			{
				if (ssocred != null && !string.IsNullOrEmpty(ssocred.AccessToken))
				{
					cred.AccessToken = ssocred.AccessToken;
				}
			}

			return cred;
		}

		/// <summary>
		/// Store the specified userId and credential.
		/// </summary>
		/// <param name="userId">User identifier.</param>
		/// <param name="credential">Credential.</param>
		public void Store (string userId, Credential credential)
		{
			Delete (userId);
			SQLCredential cred = new SQLCredential();
			cred.UserID = credential.UserId;
			cred.AuthToken = credential.AuthToken;
			cred.UserName = credential.UserName;
			cred.Attributes = JsonConvert.SerializeObject(credential.Attributes);
			cred.UserKMD = JsonConvert.SerializeObject(credential.UserKMD);
			cred.RefreshToken = credential.RefreshToken;
			cred.RedirectUri = credential.RedirectUri;
			_dbConnection.Insert(cred);

			// VRG try Xamarin.Auth
			StoreAccount(credential);
		}

		/// <summary>
		/// Delete the specified userId.
		/// </summary>
		/// <param name="userId">User identifier.</param>
		public void Delete (string userId)
		{
			_dbConnection.Delete<SQLCredential> (userId);
		}

		public Credential GetActiveUser()
		{
			Credential cred = null;

			// VRG try Xamarin.Auth
			Credential ssocred = GetActiveUserAccount();

			SQLCredential sqlcred = _dbConnection.Table<SQLCredential> ().FirstOrDefault ();

			if (sqlcred != null)
			{
				// VRG try Xamarin.Auth
				LoadAccount(sqlcred.UserID);

				Dictionary<string, JToken> attributes = null;
				if (sqlcred.Attributes != null)
				{
					attributes = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(sqlcred.Attributes);
				}

				KinveyUserMetaData kmd = null;
				if (sqlcred.UserKMD != null)
				{
					kmd = JsonConvert.DeserializeObject<KinveyUserMetaData>(sqlcred.UserKMD);
				}
				cred =  new Credential (sqlcred.UserID, sqlcred.AccessToken, sqlcred.AuthToken, sqlcred.UserName, attributes, kmd, sqlcred.RefreshToken, sqlcred.RedirectUri);
			}

			if (cred != null)
			{
				if (ssocred != null && !string.IsNullOrEmpty(ssocred.AccessToken))
				{
					cred.AccessToken = ssocred.AccessToken;
				}
			}
			else
			{
				cred = ssocred;
			}

			return cred;
		}

		#endregion

		public void StoreAccount(Credential cred)
		{
			Dictionary<string, string> properties = new Dictionary<string, string>();
			//foreach (KeyValuePair<string, JToken> kvp in cred.Attributes)
			//{
			//	JTokenType mytype = kvp.Value.Type;
			//	if (mytype.Equals(JTokenType.Object))
			//	{
			//		properties.Add(kvp.Key, JsonConvert.SerializeObject(kvp.Value));
			//	}
			//	else
			//	{
			//		properties.Add(kvp.Key, kvp.Value.ToString());
			//	}
			//}
			properties.Add("AccessToken", cred.AccessToken);
			//properties.Add("RedirectUri", cred.RedirectUri);
			//properties.Add("RefreshToken", cred.RefreshToken);
			//properties.Add("UserName", cred.UserName);
			//properties.Add("UserKMD", JsonConvert.SerializeObject(cred.UserKMD));
			Xamarin.Auth.Account account = new Xamarin.Auth.Account(cred.UserId, properties);
			string app_key = ((KinveyClientRequestInitializer)Client.SharedClient.RequestInitializer).AppKey;
			try
			{
				//Settings.GeneralSettings = cred.UserId;
				Xamarin.Auth.AccountStore store = Xamarin.Auth.AccountStore.Create();
				store.Save(account, "SSOtest");
			}
			catch (System.Exception e)
			{
				string msg = e.Message;
			}
		}

		public virtual Credential LoadAccount(string userID)
		{
			Credential cred = null;

			try
			{
				Xamarin.Auth.Account account = null;
				Xamarin.Auth.AccountStore store = Xamarin.Auth.AccountStore.Create();
				IEnumerable<Xamarin.Auth.Account> allAccounts = store.FindAccountsForService("SSOtest");
				foreach (var acc in allAccounts)
				{
					// should only be one
					if (userID.Equals(string.Empty) || acc.Username.Equals(userID))
					{
						account = acc;
						break;
					}
				}

				if (account != null)
				{
					cred = new Credential();
					cred.AccessToken = account.Properties["AccessToken"];
					cred.UserId = account.Username;
				}
			}
			catch (System.Exception e)
			{
				string msg = e.Message;
			}

			return cred;
		}

		public Credential GetActiveUserAccount()
		{
			return LoadAccount(string.Empty);
		}
	}
}
