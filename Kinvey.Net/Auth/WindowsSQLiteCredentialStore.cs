using System.Security.Cryptography;
using Newtonsoft.Json;
using SQLite.Net.Interop;

namespace Kinvey
{
	public class WindowsSQLiteCredentialStore : SQLiteCredentialStore
	{
		public WindowsSQLiteCredentialStore(ISQLitePlatform platform, string filepath)
			: base(platform, filepath)
		{
		}

		#region SQLiteCredentialStore implementation

		/// <summary>
		/// Load the credential associted with the specified user ID.
		/// </summary>
		/// <param name="userID">User identifier used to access appropriate credential.</param>
		/// <param name="ssoGroupKey">SSO Group Key.</param>
		override public Credential Load(string userID, string ssoGroupKey)
		{
			Credential decryptedCredential = null;

			try
			{
				Credential credential = base.Load(userID, ssoGroupKey);

				if (credential?.SecAuthToken != null)
				{
					byte[] origAuthToken = ProtectedData.Unprotect(credential.SecAuthToken, null, DataProtectionScope.CurrentUser);

					decryptedCredential = Credential.From(credential, System.Text.Encoding.Unicode.GetString(origAuthToken));
				}
				else
				{
					decryptedCredential = credential;
				}
			}
			catch (System.Exception e)
			{
				string msg = e.Message;
			}

			return decryptedCredential;
		}

		/// <summary>
		/// Store the credential specified by the user ID.
		/// </summary>
		/// <param name="userId">User identifier.</param>
		/// <param name="ssoGroupKey">SSO Group Key.</param>
		/// <param name="credential">Credential.</param>
		override public void Store(string userId, string ssoGroupKey, Credential credential)
		{
			try
			{
				byte[] origAuthToken = System.Text.Encoding.Unicode.GetBytes(credential.AuthToken);
				byte[] encAuthToken = ProtectedData.Protect(origAuthToken, null, DataProtectionScope.CurrentUser);

				Credential encryptedCredential = Credential.From(credential, encAuthToken);
				base.Store(userId, ssoGroupKey, encryptedCredential);
			}
			catch (System.Exception e)
			{
				string msg = e.Message;
			}
		}

		/// <summary>
		/// Load the credential associted with the specified user ID.
		/// </summary>
		/// <param name="ssoGroupKey">SSO Group Key.</param>
		override public Credential GetStoredCredential(string ssoGroupKey)
		{
			Credential decryptedCredential = null;

			try
			{
				Credential credential = base.GetStoredCredential(ssoGroupKey);

				if (credential?.SecAuthToken != null)
				{
					byte[] origAuthToken = ProtectedData.Unprotect(credential.SecAuthToken, null, DataProtectionScope.CurrentUser);

					decryptedCredential = Credential.From(credential, System.Text.Encoding.Unicode.GetString(origAuthToken));
				}
				else
				{
					decryptedCredential = credential;
				}
			}
			catch (System.Exception e)
			{
				string msg = e.Message;
			}

			return decryptedCredential;
		}

		#endregion
	}
}
