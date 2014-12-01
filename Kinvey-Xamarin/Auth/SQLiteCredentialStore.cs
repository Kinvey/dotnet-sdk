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
				cred =  new Credential (sqlcred.userID, sqlcred.AuthToken);
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
		/// <value>The user I.</value>
		[PrimaryKey]
		public string userID {get; set;}

	}
}

