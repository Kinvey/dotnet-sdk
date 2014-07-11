using System;
using Kinvey.DotNet.Framework.Auth;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace KinveyXamarin
{
	public class OnDiskCredentialStore : ICredentialStore
	{

		private string path { get; set;}

		private Dictionary<string, Credential> credentials;

		public OnDiskCredentialStore (string path)
		{
			this.path = Path.Combine(path, "kinvey_cred");
			credentials = loadCredentials();
		}


		#region ICredentialStore implementation
		Credential ICredentialStore.Load (string userId)
		{
			Credential ret;
			credentials.TryGetValue (userId, out ret);
			return ret;
		}
			
		void ICredentialStore.Store (string userId, Credential credential)
		{
			credentials.Add (userId, credential);
			writeCredentials ();
		}

		void ICredentialStore.Delete (string userId)
		{
			credentials.Remove (userId);
			writeCredentials ();

		}
		#endregion

		private Dictionary<string, Credential> loadCredentials(){
			credentials = new Dictionary<string, Credential> ();

			DataContractSerializer serializer = new DataContractSerializer (typeof(Dictionary<string, Credential>));

//			using (FileStream fs = File.Open (path, FileMode.Create)) 
//			{
//
//			}


			return credentials;
		}

		private void writeCredentials(){

		}


	}
}

