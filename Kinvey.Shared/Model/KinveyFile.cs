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
    /// JSON representation of the response with Kinvey file data from server-side.
    /// </summary>
    [JsonObject]
	public class KinveyFile
	{
        /// <summary>
        /// The type of a file.
        /// </summary>
        /// <value>The type property gets/sets the value of the string field, _type.</value>
        [JsonProperty("_type")]
		public string type {get; set;}

        /// <summary>
        /// The type id a file.
        /// </summary>
        /// <value>The id property gets/sets the value of the string field, _id.</value>
		[JsonProperty("_id")]
		public string id {get; set;}

        /// <summary>
        /// Initializes a new instance of the <see cref="KinveyFile"/> class.
        /// </summary>
        /// <param name="id">The identifier of a file. </param>
        public KinveyFile(String id){
	        this.type = "KinveyRef";
			this.id = id;
		}
	}
}

