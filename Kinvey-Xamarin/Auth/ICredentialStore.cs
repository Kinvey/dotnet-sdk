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

namespace Kinvey
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
		/// <param name="ssoGroupKey">SSO Group Key.</param>
		Credential Load(string userId, string ssoGroupKey);

		/// <summary>
		/// Store the specified userId and credential.
		/// </summary>
		/// <param name="userId">User identifier.</param>
		/// <param name="ssoGroupKey">SSO Group Key.</param>
		/// <param name="credential">Credential.</param>
		void Store(string userId, string ssoGroupKey, Credential credential);

		/// <summary>
		/// Delete the specified userId.
		/// </summary>
		/// <param name="userId">User identifier.</param>
		/// <param name="ssoGroupKey">SSO Group Key.</param>
		void Delete(string userId, string ssoGroupKey);

		/// <summary>
		/// Gets the stored credential from the credential store, based on the given SSO group key.
		/// If found, this credential represents the active user.
		/// </summary>
		/// <returns>The <see cref="Credential"/> object if it exists, otherwise null.</returns>
		/// <param name="ssoGroupKey">SSO Group Key.</param>
		Credential GetStoredCredential(string ssoGroupKey);
    }
}
