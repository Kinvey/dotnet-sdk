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

using System;
using System.IO;
using SQLite.Net.Interop;
using SQLite.Net;
using SQLite.Net.Attributes;
using System.Collections.Generic;
using System.Linq;

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
			if (_dbConnection == null) {
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
			SQLCredential sqlcred = _dbConnection.Table<SQLCredential> ().Where (t => t.userID == userId).FirstOrDefault ();
			Credential cred = null;
			if (sqlcred != null) {
				cred =  new Credential (sqlcred.userID, sqlcred.AuthToken, sqlcred.RefreshToken);
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
			SQLCredential cred = new SQLCredential();
			cred.userID = credential.UserId;
			cred.AuthToken = credential.AuthToken;
			cred.RefreshToken = credential.RefreshToken;
			_dbConnection.Insert(cred);
		}

		/// <summary>
		/// Delete the specified userId.
		/// </summary>
		/// <param name="userId">User identifier.</param>
		public void Delete (string userId)
		{
			_dbConnection.Delete<SQLCredential> (userId);
		}

		#endregion
	}

	/// <summary>
	/// SQL credential.
	/// </summary>
	public class SQLCredential{
		/// <summary>
		/// Gets or sets the auth token.
		/// </summary>
		/// <value>The auth token.</value>
		public string AuthToken { get; set;}

		/// <summary>
		/// Gets or sets the user I.
		/// </summary>
		/// <value>The user Id.</value>
		[PrimaryKey]
		public string userID {get; set;}

		/// <summary>
		/// Gets or sets the refresh token.
		/// </summary>
		/// <value>The refresh token.</value>
		public string RefreshToken {get; set;}

	}
}

