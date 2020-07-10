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

namespace Kinvey
{
    /// <summary>
	/// Represents JSON object with information about an error. 
	/// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Error
    {
        /// <summary>
		/// An index of an entity in the collection <see cref="KinveyMultiInsertResponse{T}.Entities"/>.
		/// </summary>
        /// <value>The Index property gets/sets the value of the int field, _index.</value>
        [JsonProperty]
        public int Index { get; set; }

        /// <summary>
        /// Error code.
        /// </summary>
        /// <value>The Code property gets/sets the value of the int field, _code.</value>
        [JsonProperty]
        public int Code { get; set; }

        /// <summary>
        /// Error message.
        /// </summary>
        /// <value>The Errmsg property gets/sets the value of the string field, _error.</value>
        [JsonProperty("error")]
        public string Errmsg { get; set; }

        /// <summary>
        /// Error description.
        /// </summary>
        /// <value>The Description property gets/sets the value of the string field, _description.</value>
        [JsonProperty]
        public string Description { get; set; }

        /// <summary>
        /// Debug information.
        /// </summary>
        /// <value>The Debug property gets/sets the value of the string field, _debug.</value>
        [JsonProperty]
        public string Debug { get; set; }
    }
}
