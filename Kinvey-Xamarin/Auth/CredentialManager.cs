// Copyright (c) 2014, Kinvey, Inc. All rights reserved.
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
    public class CredentialManager
    {
        private ICredentialStore credentialStore;
        private CredentialManager() { }

        public CredentialManager(ICredentialStore store)
        {
            if (credentialStore == null)
            {
                this.credentialStore = new InMemoryCredentialStore();
            }
            else
            {
                this.credentialStore = store;
            }

        }

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

        public Credential CreateAndStoreCredential(KinveyAuthResponse response, string userId)
        {
            Credential newCredential = Credential.From(response);
            if (userId != null && credentialStore != null)
            {
                credentialStore.Store(userId, newCredential);
            }
            return newCredential;
        }

        public void RemoveCredential(string userId)
        {
            if (credentialStore != null)
            {
                credentialStore.Delete(userId);
            }
        }
    }
}
