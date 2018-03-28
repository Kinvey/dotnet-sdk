// Copyright (c) 2018, Kinvey, Inc. All rights reserved.
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
    /// JSON representation of the response from server-side delta set fetch.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class DeltaSetResponse<T> : IRequestStartTime
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeltaSetResponse{T}"/> class.
        /// </summary>
        public DeltaSetResponse()
        {
        }

        /// <summary>
        /// The array of JSON objects that have changed since the last delta
        /// set fetch.
        /// </summary>
        [JsonProperty("changed")]
        public List<T> Changed { get; set; }

        /// <summary>
        /// The array of entity ID strings that have been deleted since the last
        /// delta set fetch.
        /// </summary>
        [JsonProperty("deleted")]
        public List<Entity> Deleted { get; set; }

        public string LastRequestTime { get; set; }
    }
}
