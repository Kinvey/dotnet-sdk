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

namespace KinveyXamarin
{
	/// <summary>
	/// This interface defines the ability to store Credentials.
	/// </summary>
    public interface ICredentialStore
    {
		/// <summary>
		/// Load the specified userId.
		/// </summary>
		/// <param name="userId">User identifier.</param>
		/// <param name="orgID">Organization identifier.</param>
		Credential Load(string userId, string orgID);

		/// <summary>
		/// Store the specified userId and credential.
		/// </summary>
		/// <param name="userId">User identifier.</param>
		/// <param name="orgID">Organization identifier.</param>
		/// <param name="credential">Credential.</param>
        void Store(string userId, string orgID, Credential credential);

		/// <summary>
		/// Delete the specified userId.
		/// </summary>
		/// <param name="userId">User identifier.</param>
		/// <param name="orgID">Organization identifier.</param>
		void Delete(string userId, string orgID);

		/// <summary>
		/// Gets the active user from the credential store.
		/// </summary>
		/// <returns>The active user represented as a <see cref="Credential"/> object.</returns>
		/// <param name="orgID">Organization identifier.</param>
		Credential GetActiveUser(string orgID);
    }
}
