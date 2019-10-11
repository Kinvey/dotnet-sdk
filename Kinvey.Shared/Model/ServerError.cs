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
	/// Represents JSON object with information about a server error. 
	/// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ServerError
    {
        /// <summary>
        /// Error message.
        /// </summary>
        /// <value>The Error property gets/sets the value of the string field, _error.</value>
        [JsonProperty]
        internal string Error { get; set; }

        /// <summary>
        /// Error description.
        /// </summary>
        /// <value>The Description property gets/sets the value of the string field, _description.</value>
        [JsonProperty]
        internal string Description { get; set; }

        /// <summary>
        /// Some additional information regarding the error.
        /// </summary>
        /// <value>The Debug property gets/sets the value of the string field, _debug.</value>
        [JsonProperty]
        internal string Debug { get; set; }
    }
}
