using System;
using System.IO;
using SQLite.Net.Interop;
using SQLite.Net;
using SQLite.Net.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace KinveyXamarin
{
	public class SQLiteCredentialStore : ICredentialStore
	{
		private SQLiteConnection _dbConnection;

		public SQLiteCredentialStore (ISQLitePlatform platform, string filepath)
		{
			string dbPath = Path.Combine (filepath, "kinvey_tokens.sqlite");
			if (_dbConnection == null) {
				_dbConnection = new SQLiteConnection (platform, dbPath);
				_dbConnection.CreateTable<SQLCredential>();
			}
		}

		#region ICredentialStore implementation

		public Credential Load (string userId)
		{
			SQLCredential sqlcred = _dbConnection.Table<SQLCredential> ().Where (t => t.userID == userId).FirstOrDefault ();
			Credential cred = null;
			if (sqlcred != null) {
				cred =  new Credential (sqlcred.userID, sqlcred.AuthToken);
			}
			return cred;
		}

		public void Store (string userId, Credential credential)
		{
			SQLCredential cred = new SQLCredential();
			cred.userID = credential.UserId;
			cred.AuthToken = credential.AuthToken;
			_dbConnection.Insert(cred);
		}

		public void Delete (string userId)
		{
			_dbConnection.Delete<SQLCredential> (userId);
		}

		#endregion
	}

	public class SQLCredential{

		public string AuthToken { get; set;}

		[PrimaryKey]
		public string userID {get; set;}

	}
}

