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
using System;

namespace Kinvey
{
	/// <summary>
	/// SQ lite credential store.
	/// </summary>
	public class SQLiteCredentialStore : ICredentialStore
	{

        private static readonly Dictionary<String, List<SQLiteConnection>> SQLiteFiles = new Dictionary<String, List<SQLiteConnection>>();

        /// <summary>
        /// The db connection.
        /// </summary>
        private readonly SQLiteConnection _dbConnection;
        private readonly String dbPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteCredentialStore"/> class.
        /// </summary>
        /// <param name="filepath">Filepath.</param>
        public SQLiteCredentialStore (string filepath)
		{
            dbPath = Path.Combine(filepath, "kinvey_tokens.sqlite");
            _dbConnection = new SQLiteConnection(
                dbPath,
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex | SQLiteOpenFlags.PrivateCache
            );
            List<SQLiteConnection> connections;
            if (SQLiteFiles.ContainsKey(dbPath))
            {
                connections = SQLiteFiles[dbPath];
            }
            else
            {
                connections = new List<SQLiteConnection>();
                SQLiteFiles[dbPath] = connections;
            }
            connections.Add(_dbConnection);
            _dbConnection.CreateTable<SQLCredential>();
        }

        #region ICredentialStore implementation

        /// <summary>
        /// Load the specified userId.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <param name="ssoGroupKey">SSO Group Key.</param>
        virtual public Credential Load (string userId, string ssoGroupKey)
		{
            SQLCredential sqlcred;
            lock (SQLiteFiles[dbPath])
            {
                sqlcred = _dbConnection.Table<SQLCredential>().Where(t => t.UserID == userId).FirstOrDefault();
            }
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
            SQLCredential cred = new SQLCredential
            {
                UserID = credential.UserId,
                AuthSocialID = JsonConvert.SerializeObject(credential.AuthSocialID),
                AuthToken = credential.AuthToken,
                SecAuthToken = credential.SecAuthToken,
                UserName = credential.UserName,
                Attributes = JsonConvert.SerializeObject(credential.Attributes),
                UserKMD = JsonConvert.SerializeObject(credential.UserKMD),
                AccessToken = credential.AccessToken,
                RefreshToken = credential.RefreshToken,
                RedirectUri = credential.RedirectUri,
                DeviceID = credential.DeviceID,
                MICClientID = credential.MICClientID
            };
            lock (SQLiteFiles[dbPath])
            {
                try
                {
                    _dbConnection.BeginTransaction();
                    _dbConnection.Insert(cred);
                    _dbConnection.Commit();
                }
                catch (Exception)
                {
                    _dbConnection.Rollback();
                    throw;
                }
            }
		}

		/// <summary>
		/// Delete the specified userId.
		/// </summary>
		/// <param name="userId">User identifier.</param>
		/// <param name="ssoGroupKey">SSO Group Key.</param>
		public void Delete (string userId, string ssoGroupKey)
		{
            lock (SQLiteFiles[dbPath])
            {
                try
                {
                    _dbConnection.BeginTransaction();
                    _dbConnection.Delete<SQLCredential>(userId);
                    _dbConnection.Commit();
                }
                catch (Exception)
                {
                    _dbConnection.Rollback();
                    throw;
                }
            }
		}

		virtual public Credential GetStoredCredential(string ssoGroupKey)
		{
			Credential cred = null;

            SQLCredential sqlcred;
            lock (SQLiteFiles[dbPath])
            {
                sqlcred = _dbConnection.Table<SQLCredential>().FirstOrDefault();
            }

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

                cred = new Credential(sqlcred.UserID, sqlcred.AccessToken, socialIdentity, sqlcred.AuthToken, sqlcred.UserName, attributes, kmd, sqlcred.RefreshToken, sqlcred.RedirectUri, sqlcred.DeviceID, sqlcred.MICClientID)
                {
                    SecAuthToken = sqlcred.SecAuthToken
                };
            }

			return cred;
		}

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            lock (this)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // dispose managed state (managed objects).
                    }

                    // free unmanaged resources (unmanaged objects) and override a finalizer below.
                    if (_dbConnection != null)
                    {
                        _dbConnection.Close();
                        lock (SQLiteFiles)
                        {
                            if (SQLiteFiles.TryGetValue(dbPath, out List<SQLiteConnection> connections))
                            {
                                connections.Remove(_dbConnection);
                                if (connections.Count == 0) SQLiteFiles.Remove(dbPath);
                            }
                        }
                        _dbConnection.Dispose();
                    }

                    // set large fields to null.


                    disposedValue = true;
                }
            }
        }

        // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~SQLiteCredentialStore() {
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #endregion

    }
}
