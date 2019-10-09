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
using System;

namespace Kinvey
{
    /// <summary>
	/// Represents JSON object with information about Kinvey reference. 
	/// </summary>
    /// <typeparam name="T">The type of a response returned from the backend.</typeparam>
    [Obsolete("This class has been deprecated.")]
	[JsonObject]
	public class KinveyReference<T>
	{
        /// <summary>
        /// The type of a reference.
        /// </summary>
        ///  <value>The type property gets/sets the value of the string field, _type.</value>
        [JsonProperty]
		public string type {get; set;}

        /// <summary>
        /// The identifier of a reference.
        /// </summary>
        ///  <value>The id property gets/sets the value of the string field, _id.</value>
		[JsonProperty("_id")]
		public string id {get; set;}

        /// <summary>
        /// The collection of a reference.
        /// </summary>
        ///  <value>The collection property gets/sets the value of the string field, _collection.</value>
		[JsonProperty("_collection")]
		public string collection {get; set;}

        /// <summary>
        /// Resolved object.
        /// </summary>
        ///  <value>The resolved property gets/sets the value of the T field, _resolved.</value>
		[JsonProperty("_obj")]
		public T resolved {get; set;}

        /// <summary>
        /// Initializes a new instance of the <see cref="KinveyReference{T}"/> class.
        /// </summary>
        public KinveyReference ()
		{
			this.type = "KinveyRef";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KinveyReference{T}"/> class.
        /// </summary>
        ///<param name ="collection" > Collection name. </param>
        ///<param name ="id" > Identifier. </param>
        public KinveyReference(string collection, string id)
		{
			this.type = "KinveyRef";
			this.collection = collection;
			this.id = id;
		}

        /// <summary>
        /// Gets resolved object.
        /// </summary>
        /// <returns>Resolved object.</returns>
        public T getResolvedObject(){
			return resolved;
		}
	}
}

