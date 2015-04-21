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

namespace KinveyXamarin
{
	/// <summary>
	/// This class is used to manage credential objects.
	/// </summary>
    public class CredentialManager
    {
		/// <summary>
		/// Defines where the Credential is stored-- in memory, on disk, etc.
		/// </summary>
        private ICredentialStore credentialStore;

		/// <summary>
		/// Create a new Credential Manager.
		/// </summary>
        private CredentialManager() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.CredentialManager"/> class with a CredentialStore implementation
		/// </summary>
		/// <param name="store">Store.</param>
        public CredentialManager(ICredentialStore store)
        {
            if (store == null)
            {
                this.credentialStore = new InMemoryCredentialStore();
            }
            else
            {
                this.credentialStore = store;
            }

        }

		/// <summary>
		/// Loads the credential of a provided user._id from the previously configured Credential Store.
		/// </summary>
		/// <returns>The credential.</returns>
		/// <param name="userId">User _id.</param>
        public Credential LoadCredential(string userId)
        {
            if (credentialStore == null)
            {
                return null;
            }
            else
            {
                return credentialStore.Load(userId);
            }
        }

		/// <summary>
		/// Saves the credential associated with the user._id.
		/// </summary>
		/// <param name="userId">User _id.</param>
		/// <param name="credential">Credential.</param>
        public void MakePersistant(string userId, Credential credential)
        {
            if (credentialStore == null)
            {
                return;
            }
            else
            {
                credentialStore.Store(userId, credential);
            }
        }

		/// <summary>
		/// Creates a new Credential object from a Kinvey user login/create request, and saves the it in the Credential Store.
		/// </summary>
		/// <returns>The and store credential.</returns>
		/// <param name="response">Response.</param>
		/// <param name="userId">User _id.</param>
        public Credential CreateAndStoreCredential(KinveyAuthResponse response, string userId)
        {
            Credential newCredential = Credential.From(response);
            if (userId != null && credentialStore != null)
            {
                credentialStore.Store(userId, newCredential);
            }
            return newCredential;
        }

		/// <summary>
		/// Removes the user._id's credential from the Credential Store.
		/// </summary>
		/// <param name="userId">User identifier.</param>
        public void RemoveCredential(string userId)
        {
            if (credentialStore != null)
            {
                credentialStore.Delete(userId);
            }
        }
    }
}
