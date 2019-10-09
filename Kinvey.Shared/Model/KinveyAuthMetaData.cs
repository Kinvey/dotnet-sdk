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

using System.Collections.Generic;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="KinveyAuthMetaData"/> class.
        /// </summary>
        public KinveyAuthMetaData()
		{
		}

        /// <summary>
		/// The access token received from backend.
		/// </summary>
        /// <value>The AccessToken property gets/sets the value of the string field, _accessToken.</value>
		[Preserve]
		[JsonProperty("access_token")]        
		public string AccessToken { get; set; }

        /// <summary>
		/// The Auth ID received from backend.
		/// </summary>
        /// <value>The AuthID property gets/sets the value of the string field, _authID.</value>
		[Preserve]
		[JsonProperty("id")]
		public string AuthID { get; set; }

        /// <summary>
        /// The entity creation time.
        /// </summary>
        /// <value>The AuthAudience property gets/sets the value of the string field, _authAudience.</value>
        [Preserve]
		[JsonProperty("audience")]
		public string AuthAudience { get; set; }

        /// <summary>
        /// A name-value dictionary of custom attributes of the kinveyAuth object.
        /// </summary>
        /// <value>The dictionary with custom attributes.</value>
        [Preserve]
		[JsonExtensionData]
		public Dictionary<string, JToken> Attributes;

        /// <summary>
        /// Serialize this instance of <see cref="KinveyAuthMetaData"/> in the local cache.
        /// </summary>
        /// <value>The string containing serialized <see cref="KinveyAuthMetaData"/> object to JSON.</value>
        public string Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
