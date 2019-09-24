// Copyright (c) 2019, Kinvey, Inc. All rights reserved.
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

using System.Net.Http;
using System.Net.Http.Headers;

namespace Kinvey
{
    /// <summary>
    /// This interface defines the ability to authenticate.
    /// </summary>
    public interface IAuthenticator
    {
        /// <summary>
        /// Authenticates a request.
        /// </summary>
        /// <param name="request">Http request message to be authenticated.</param>
        void Authenticate(HttpRequestMessage request);
    }

    /// <summary>
    /// Authenticator for Kinvey style authentication.
    /// </summary>
    public class KinveyAuthenticator : IAuthenticator
    {
		/// <summary>
		/// The auth header scheme.
		/// </summary>
        private static readonly string AuthHeaderScheme = "Kinvey";
		/// <summary>
		/// The auth token.
		/// </summary>
        private readonly string authToken;

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyAuthenticator"/> class.
		/// </summary>
		/// <param name="authToken">Auth token.</param>
		public KinveyAuthenticator(string authToken) {
			this.authToken = authToken;
		}

		/// <summary>
		/// Authenticates the specified request.
		/// </summary>
		/// <param name="request">Request.</param>
		public void Authenticate(HttpRequestMessage request)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(AuthHeaderScheme, authToken);
        }
    }
}
