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
using Newtonsoft.Json;
using SQLite.Net;

namespace Kinvey
{
	/// <summary>
	/// JSON representation of the emailVerification field present on user
	/// entities stored in Kinvey that have verified through email
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class KMDEmailVerification : ISerializable<string>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KMDEmailVerification"/> class.
		/// </summary>
		[Preserve]
		public KMDEmailVerification()
		{
		}

		/// <summary>
		/// Gets or sets the status of email verification for the user.
		/// </summary>
		[Preserve]
		[JsonProperty("status")]
		public String Status { get; set; }

		/// <summary>
		/// Gets or sets the last time when the state of email verification changed.
		/// </summary>
		[Preserve]
		[JsonProperty("lastStateChangeAt")]
		public String LastStateChangeAt { get; set; }

		/// <summary>
		/// Gets or sets the last time when email verification was confirmed.
		/// </summary>
		[Preserve]
		[JsonProperty("lastConfirmedAt")]
		public String LastConfirmedAt { get; set; }

		/// <summary>
		/// Gets or sets the email address of the user used for email verification.
		/// </summary>
		[Preserve]
		[JsonProperty("emailAddress")]
		public String EmailAddress { get; set; }

		/// <summary>
		/// Serialize this instance of <see cref="KinveyXamarin.KMDEmailVerification"/> in the local cache.
		/// </summary>
		public string Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
