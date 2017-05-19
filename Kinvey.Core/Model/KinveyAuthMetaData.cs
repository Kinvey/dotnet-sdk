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
using Newtonsoft.Json.Linq;

namespace Kinvey
{
	/// <summary>
	/// Kinvey auth metadata.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class KinveyAuthMetaData : JObject
	{
		public KinveyAuthMetaData()
		{
		}

		[Preserve]
		[JsonProperty("access_token")]
		public string AccessToken { get; set; }

		[Preserve]
		[JsonProperty("id")]
		public string AuthID { get; set; }

		/// <summary>
		/// Gets or sets the entity creation time.
		/// </summary>
		[Preserve]
		[JsonProperty("audience")]
		public string AuthAudience { get; set; }

		/// <summary>
		/// Serialize this instance of <see cref="KinveyAuthMetaData"/> in the local cache.
		/// </summary>
		public string Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
