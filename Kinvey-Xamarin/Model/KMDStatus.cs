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

namespace KinveyXamarin
{
	/// <summary>
	/// JSON representation of the emailVerification field present on user
	/// entities stored in Kinvey that have verified through email
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class KMDStatus : ISerializable<string>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KMDPasswordReset"/> class.
		/// </summary>
		[Preserve]
		public KMDStatus()
		{
		}

		/// <summary>
		/// Gets or sets the status of the password reset request for the user.  This field is set 
		/// to "InProgress" during the fulfillment of the request, and is empty when the request is complete.
		/// </summary>
		[Preserve]
		[JsonProperty("val")]
		public String Value { get; set; }

		/// <summary>
		/// Gets or sets the last time when the state of the password reset request changed.  If the status field 
		/// is set to "InProgress", this field reflects when the password reset request was issued.  If the status 
		/// field is empty, this field reflects when the password reset request was fulfilled.
		/// </summary>
		[Preserve]
		[JsonProperty("lastChange")]
		public String LastChange { get; set; }

		/// <summary>
		/// Serialize this instance of <see cref="KinveyXamarin.KMDPasswordReset"/> in the local cache.
		/// </summary>
		public string Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
