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

using System.Collections.Generic;
using Newtonsoft.Json;
using SQLite.Net;

namespace Kinvey
{
	/// <summary>
	/// JSON represention of the _acl present on every entity stored in Kinvey.
	/// This object allows the app to modify with more granularity the
	/// access permissions on the entity, which will override the settings at
	/// the collection level.
	/// </summary>
	[JsonObject]
	public class AccessControlList : ISerializable<string>
	{
		/// <summary>
		/// the field name within every JSON object.
		/// </summary>
		public const string JSON_FIELD_NAME = "_acl";

		/// <summary>
		/// Gets or sets the creator value.
		/// </summary>
		/// <value>The creator</value>
		[JsonProperty("creator")]
		public string Creator { get; set; }

		/// <summary>
		/// Gets or sets whether this entity is globally readable.
		/// </summary>
		/// <value><c>true</c> if globally readable; otherwise, <c>false</c>.</value>
		[JsonProperty("gr")]
		public bool GloballyReadable { get; set; }

		/// <summary>
		/// Gets or sets whether this entity is globally writeable.
		/// </summary>
		/// <value><c>true</c> if globally writeable; otherwise, <c>false</c>.</value>
		[JsonProperty("gw")]
		public bool GloballyWriteable { get; set; }

		/// <summary>
		/// Gets or sets a list of user IDs that are specifically allowed to read this entity.
		/// </summary>
		/// <value>The list of user IDs allowed to read this entity.</value>
		[JsonProperty("r")]
		public List<string> Read { get; set; }

		/// <summary>
		/// Gets or sets a list of user IDs that are specifically allowed to modify this entity.
		/// </summary>
		/// <value>The list of user IDs allowed to modify this entity.</value>
		[JsonProperty("w")]
		public List<string> Write { get; set; }

		/// <summary>
		/// Gets or sets the ACL group that contains lists of user groups which are authorized on the
		/// entity for reading and writing.
		/// Within the ACLGroups object, there are 2 lists: one list for the user groups allowed to
		/// read the entity, and one list for the user groups that are allowed to modify the entity.
		/// </summary>
		/// <value>The group object which contains the list of user groups allowed to read and/or modify the entity.</value>
		[JsonProperty("groups")]
		public ACLGroups Groups { get; set; }

		/// <summary>
		/// Class that holds the list of user groups that can read the entity and the list of user groups
		/// that can modify the entity.
		/// </summary>
		[JsonObject]
		public class ACLGroups
		{
			/// <summary>
			/// Gets or sets the list of user groups that can read the entity.
			/// </summary>
			/// <value>The list of user groups with read access to the entity.</value>
			[JsonProperty("r")]
			public List<string> Read { get; set; }

			/// <summary>
			/// Gets or sets the list of user groups that can modify the entity.
			/// </summary>
			/// <value>The list of user groups with write access to the entity.</value>
			[JsonProperty("w")]
			public List<string> Write { get; set; }
		}

		/// <summary>
		/// Implementation of ISerializeable interface, used to serialize this <see cref="AccessControlList"/> instance.
		/// </summary>
		public string Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
