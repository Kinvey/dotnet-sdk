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

namespace Kinvey
{
	/// <summary>
	/// JSON representation of the fields needed to do delta set comparisons.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class DeltaSetFetchInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DeltaSetFetchInfo"/> class.
		/// </summary>
		public DeltaSetFetchInfo()
		{
		}

        /// <summary>
        /// The ID of this entity used for delta set comparisons.
        /// </summary>
        ///  <value>The ID property gets/sets the value of the string field.</value>
        [JsonProperty("_id")]
		public String ID { get; set; }

        /// <summary>
        /// The <see cref="KinveyMetaData"/> of this entity, which includes the
        /// last modified time("_lmt") that is used for delta set comparisons.
        /// </summary>
        /// <value>The KMD property gets/sets the value of the KinveyMetaData field.</value>
        [JsonProperty("_kmd")]
		public KinveyMetaData KMD { get; set; }
	}
}
