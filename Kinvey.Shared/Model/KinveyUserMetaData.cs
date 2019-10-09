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

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kinvey
{
	/// <summary>
	/// Kinvey user metadata.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class KinveyUserMetaData : JObject
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="KinveyUserMetaData"/> class.
        /// </summary>
        public KinveyUserMetaData()
		{
			this.EmailVerification = new KMDEmailVerification();
			this.PasswordReset = new KMDPasswordReset();
			this.UserStatus = new KMDStatus();
		}

        /// <summary>
        /// Authentication token.
        /// </summary>
        /// <value>The AuthToken property gets/sets the value of the string field, _authToken.</value>
        [Preserve]
		[JsonProperty("authtoken")]
		public string AuthToken { get; set; }

        /// <summary>
        /// The last time of modification.
        /// </summary>
        /// <value>The LastModifiedTime property gets/sets the value of the string field, _lastModifiedTime.</value>
		[Preserve]
		[JsonProperty("lmt")]
		public string LastModifiedTime { get; set; }

        /// <summary>
        /// The time of entity creation.
        /// </summary>
        /// <value>The EntityCreationTime property gets/sets the value of the string field, _entityCreationTime.</value>
        [Preserve]
		[JsonProperty("ect")]
		public String EntityCreationTime { get; set; }

        /// <summary>
        /// The email verification information for a user.
        /// </summary>
        /// <value>The EmailVerification property gets/sets the value of the KMDEmailVerification field, _emailVerification.</value>
        [Preserve]
		[JsonProperty("emailVerification")]
		public KMDEmailVerification EmailVerification { get; set; }

        /// <summary>
        /// The password reset information for a user.
        /// </summary>
        /// <value>The PasswordReset property gets/sets the value of the KMDPasswordReset field, _passwordReset.</value>
        [Preserve]
		[JsonProperty("passwordReset")]
		public KMDPasswordReset PasswordReset { get; set; }

        /// <summary>
        /// The status of the user, including whether or not the user is diabled.
        /// </summary>
        /// <value>The UserStatus property gets/sets the value of the KMDStatus field, _userStatus.</value>
        [Preserve]
		[JsonProperty("status")]
		public KMDStatus UserStatus { get; set; }
	}
}
