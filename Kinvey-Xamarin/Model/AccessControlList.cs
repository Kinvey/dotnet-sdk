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
using System.Collections.Generic;
using Newtonsoft.Json;
using SQLite.Net;

namespace KinveyXamarin
{

	/// <summary>
	/// JSON represention of the _acl fied present on every stored in Kinvey
	/// </summary>
	[JsonObject]
	public class AccessControlList : ISerializable<string>
	{

		/// <summary>
		/// the field name within every JSON object.
		/// </summary>
		public const string JSON_FIELD_NAME = "_acl";


		[JsonProperty]
		public string creator {get; set;}

		[JsonProperty("gr")]
		public bool globallyReadable {get; set;}

		[JsonProperty("gw")]
		public bool globallyWriteable {get; set;}

		[JsonProperty("r")]
		public List<string> read {get; set;}

		[JsonProperty("w")]
		public List<string> write {get; set;}

		[JsonProperty("groups")]
		public List<AclGroups> groups { get; set;}

		public AccessControlList(){}

		[JsonObject]
		public class AclGroups  {

			[JsonProperty("r")]
			public string read {get; set;}

			[JsonProperty("w")]
			public string write {get; set;}

			public AclGroups(){}

		}

		public string Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}
	}

}

