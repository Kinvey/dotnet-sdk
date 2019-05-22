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
using System.Collections.Generic;

namespace Kinvey
{
    /// <summary>
	/// This class represents a response sent from Kinvey backend after a multi insert operation has been executed.
	/// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class KinveyDataStoreResponse<T>
    {
        /// <summary>
		/// Represents a collection of entities which were inserted to Kinvey data source.
		/// </summary>
		/// <value>List of entities.</value>
        [JsonProperty]
        public List<T> Entities { get; set; }

        /// <summary>
		/// Represents a collection of errors which were occurring during  inserting operation.
		/// </summary>
		/// <value>List of errors.</value>
        [JsonProperty]
        public List<Error> Errors { get; set; }
    }
}