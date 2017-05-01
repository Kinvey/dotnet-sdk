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
		public KinveyUserMetaData()
		{
			this.EmailVerification = new KMDEmailVerification();
			this.PasswordReset = new KMDPasswordReset();
			this.UserStatus = new KMDStatus();
		}

		[Preserve]
		[JsonProperty("authtoken")]
		public string AuthToken { get; set; }

		[Preserve]
		[JsonProperty("lmt")]
		public string LastModifiedTime { get; set; }

		/// <summary>
		/// Gets or sets the entity creation time.
		/// </summary>
		[Preserve]
		[JsonProperty("ect")]
		public String EntityCreationTime { get; set; }

		/// <summary>
		/// Gets or sets the email verification information for a user.
		/// </summary>
		[Preserve]
		[JsonProperty("emailVerification")]
		public KMDEmailVerification EmailVerification { get; set; }

		/// <summary>
		/// Gets or sets the password reset information for a user.
		/// </summary>
		[Preserve]
		[JsonProperty("passwordReset")]
		public KMDPasswordReset PasswordReset { get; set; }

		/// <summary>
		/// Gets or sets the status of the user, including whether or
		/// not the user is diabled.
		/// </summary>
		[Preserve]
		[JsonProperty("status")]
		public KMDStatus UserStatus { get; set; }
	}
}
