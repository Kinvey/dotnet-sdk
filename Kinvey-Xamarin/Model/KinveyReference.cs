// Copyright (c) 2015, Kinvey, Inc. All rights reserved.
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
	[JsonObject]
	public class KinveyReference<T>
	{
	
		[JsonProperty]
		public string type {get; set;}

		[JsonProperty("_id")]
		public string id {get; set;}

		[JsonProperty("_collection")]
		public string collection {get; set;}

		[JsonProperty("_obj")]
		public T resolved {get; set;}

		public KinveyReference ()
		{
			this.type = "KinveyRef";
		}

		public KinveyReference(string collection, string id)
		{
			this.type = "KinveyRef";
			this.collection = collection;
			this.id = id;
		}

		public T getResolvedObject(){
			return resolved;


		}
	}
}

