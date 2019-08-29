// Copyright (c) 2016, Kinvey, Inc. All rights reserved.
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

namespace Kinvey
{
	/// <summary>
	/// Native credential store.
	/// </summary>
	abstract public class NativeCredentialStore : ICredentialStore, IDisposable
    {
		/// <summary>
		/// Load the credential object from the native credential store based on the specified user ID.
		/// </summary>
		/// <param name="userID">User identifier.</param>
		/// <param name="ssoGroupKey">SSO Group Key.</param>
		abstract public Credential Load(string userID, string ssoGroupKey);

		/// <summary>
		/// Store the specified userID and credential.
		/// </summary>
		/// <param name="userID">User identifier.</param>
		/// <param name="ssoGroupKey">SSO Group Key.</param>
		/// <param name="credential">Credential.</param>
		abstract public void Store(string userID, string ssoGroupKey, Credential credential);

		/// <summary>
		/// Delete the specified user ID into the native credential store.
		/// </summary>
		/// <param name="ssoGroupKey">SSO Group Key.</param>
		/// <param name="ssoGroupKey">SSO Group Key.</param>
		abstract public void Delete(string userID, string ssoGroupKey);

		/// <summary>
		/// Gets the active user from the native credential store.
		/// </summary>
		/// <returns>The credential object representing the active user.</returns>
		/// <param name="ssoGroupKey">SSO Group Key.</param>
		abstract public Credential GetStoredCredential(string ssoGroupKey);

        public abstract void Dispose();
    }
}
