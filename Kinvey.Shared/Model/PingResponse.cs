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

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Kinvey
{
	/// <summary>
	/// This class represents the response of a ping request.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class PingResponse
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PingResponse"/> class.
		/// </summary>
		[Preserve]
		public PingResponse ()
		{}

        /// <summary>
        /// The version of Kinvey SDK.
        /// </summary>
        /// <value>The <see cref="System.String"/> value containing the version.</value>
        [Preserve]
		[JsonProperty]
		public string version;

        /// <summary>
        /// Kinvey message.
        /// </summary>
        /// <value>The <see cref="System.String"/> value containing Kinvey message.</value>
		[JsonProperty]
		public string kinvey;
	}
}

