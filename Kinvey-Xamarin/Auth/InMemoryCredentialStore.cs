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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// In memory credential store.
/// </summary>
namespace KinveyXamarin
{
	/// <summary>
	/// In memory credential store.
	/// </summary>
    public class InMemoryCredentialStore : ICredentialStore
    {
		/// <summary>
		/// The store.
		/// </summary>
        private Dictionary<string, Credential> store = new Dictionary<string, Credential>();

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.InMemoryCredentialStore"/> class.
		/// </summary>
		public InMemoryCredentialStore(){
		}

		/// <summary>
		/// Load the specified userId.
		/// </summary>
		/// <param name="userId">User._id.</param>
        public Credential Load(string userId)
        {
            return store[userId];
        }

		/// <summary>
		/// Store the specified userId and credential.
		/// </summary>
		/// <param name="userId">User identifier.</param>
		/// <param name="credential">Credential.</param>
        public void Store(string userId, Credential credential)
        {
            if (userId != null)
            {
				Credential cred = new Credential(userId, credential.AuthToken, credential.UserName, credential.Attributes, credential.UserKMD, credential.RefreshToken, credential.RedirectUri);
                store.Add(userId, cred);
            }
        }

		/// <summary>
		/// Delete the specified userId.
		/// </summary>
		/// <param name="userId">User identifier.</param>
        public void Delete(string userId)
        {
            if (userId != null)
            {
                store.Remove(userId);
            }
        }

		public Credential getActiveUser (){
			return store.FirstOrDefault ().Value;
		}

    }
}
