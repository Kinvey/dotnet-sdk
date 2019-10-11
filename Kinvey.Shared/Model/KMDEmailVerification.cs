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
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Kinvey
{
	/// <summary>
	/// JSON representation of the emailVerification field present on user
	/// entities stored in Kinvey that have verified through email
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
    [DataContract]
	public class KMDEmailVerification
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="KMDEmailVerification"/> class.
		/// </summary>
		[Preserve]
		public KMDEmailVerification()
		{
		}

        /// <summary>
        /// The status of email verification for the user.
        /// </summary>
        /// <value>The Status property gets/sets the value of the string field, _status.</value>
        [Preserve]
		[JsonProperty("status")]
        [DataMember(Name = "status")]
		public String Status { get; set; }

        /// <summary>
        /// The last time when the state of email verification changed.
        /// </summary>
        /// <value>The LastStateChangeAt  property gets/sets the value of the string field, _lastStateChangeAt.</value>
        [Preserve]
		[JsonProperty("lastStateChangeAt")]
        [DataMember(Name = "lastStateChangeAt")]
        public String LastStateChangeAt { get; set; }

        /// <summary>
        /// The last time when email verification was confirmed.
        /// </summary>
        /// <value>The LastConfirmedAt  property gets/sets the value of the string field, _lastConfirmedAt.</value>
        [Preserve]
		[JsonProperty("lastConfirmedAt")]
        [DataMember(Name = "lastConfirmedAt")]
        public String LastConfirmedAt { get; set; }

        /// <summary>
        /// The email address of the user used for email verification.
        /// </summary>
        /// <value>The EmailAddress  property gets/sets the value of the string field, _emailAddress.</value>
        [Preserve]
		[JsonProperty("emailAddress")]
        [DataMember(Name = "emailAddress")]
        public String EmailAddress { get; set; }
	}
}
