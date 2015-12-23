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
using Newtonsoft.Json.Serialization;

namespace KinveyXamarin
{
	/// <summary>
	/// This class represents the response of a ping request.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class PingResponse
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.PingResponse"/> class.
		/// </summary>
		[Preserve]
		public PingResponse ()
		{}

		[Preserve]
		[JsonProperty]
		public string version;

		[JsonProperty]
		public string kinvey;

	}
}

