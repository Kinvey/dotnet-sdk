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
using SQLite;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Kinvey
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
		/// Initializes a new instance of the <see cref="SQLiteCredentialStore"/> class.
		/// </summary>
		/// <param name="platform">Platform.</param>
		/// <param name="filepath">Filepath.</param>
		public SQLiteCredentialStore (string filepath)
		{
			if (_dbConnection == null)
			{
                var dbPath = Path.Combine(filepath, "kinvey_tokens.sqlite");
                _dbConnection = new SQLiteConnection (
                    dbPath,
                    SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex
                );
				_dbConnection.CreateTable<SQLCredential>();
			}
		}

		#region ICredentialStore implementation

		/// <summary>
		/// Load the specified userId.
		/// </summary>
		/// <param name="userId">User identifier.</param>
		/// <param name="ssoGroupKey">SSO Group Key.</param>
		virtual public Credential Load (string userId, string ssoGroupKey)
		{
			SQLCredential sqlcred = _dbConnection.Table<SQLCredential> ().Where (t => t.UserID == userId).FirstOrDefault ();
			Credential cred = null;
			if (sqlcred != null)
			{
				cred = Credential.From(sqlcred);
			}

			return cred;
		}

		/// <summary>
		/// Store the specified userId and credential.
		/// </summary>
		/// <param name="userId">User identifier.</param>
		/// <param name="ssoGroupKey">SSO Group Key.</param>
		/// <param name="credential">Credential.</param>
		virtual public void Store (string userId, string ssoGroupKey, Credential credential)
		{
			Delete (userId, ssoGroupKey);
			SQLCredential cred = new SQLCredential();
			cred.UserID = credential.UserId;
			cred.AuthSocialID = JsonConvert.SerializeObject(credential.AuthSocialID);
			cred.AuthToken = credential.AuthToken;
			cred.SecAuthToken = credential.SecAuthToken;
			cred.UserName = credential.UserName;
			cred.Attributes = JsonConvert.SerializeObject(credential.Attributes);
			cred.UserKMD = JsonConvert.SerializeObject(credential.UserKMD);
			cred.AccessToken = credential.AccessToken;
			cred.RefreshToken = credential.RefreshToken;
			cred.RedirectUri = credential.RedirectUri;
			cred.DeviceID = credential.DeviceID;
            cred.MICClientID = credential.MICClientID;
			_dbConnection.Insert(cred);
		}

		/// <summary>
		/// Delete the specified userId.
		/// </summary>
		/// <param name="userId">User identifier.</param>
		/// <param name="ssoGroupKey">SSO Group Key.</param>
		public void Delete (string userId, string ssoGroupKey)
		{
			_dbConnection.Delete<SQLCredential> (userId);
		}

		virtual public Credential GetStoredCredential(string ssoGroupKey)
		{
			Credential cred = null;

			SQLCredential sqlcred = _dbConnection.Table<SQLCredential> ().FirstOrDefault ();

			if (sqlcred != null)
			{
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

				KinveyAuthSocialID socialIdentity = null;
				if (sqlcred.AuthSocialID != null)
				{
					socialIdentity = JsonConvert.DeserializeObject<KinveyAuthSocialID>(sqlcred.AuthSocialID);
				}

				cred =  new Credential (sqlcred.UserID, sqlcred.AccessToken, socialIdentity, sqlcred.AuthToken, sqlcred.UserName, attributes, kmd, sqlcred.RefreshToken, sqlcred.RedirectUri, sqlcred.DeviceID, sqlcred.MICClientID);
				cred.SecAuthToken = sqlcred.SecAuthToken;
			}

			return cred;
		}

		#endregion

	}
}
