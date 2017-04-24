// Copyright (c) 2017, Kinvey, Inc. All rights reserved.
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

namespace Kinvey
{
	/// <summary>
	/// JSON represention of the stream ACL, which is used to grant publish and subscribe
	/// access for users or groups of users on a particular stream.
	/// </summary>
	[JsonObject]
	public class StreamAccessControlList
	{
		/// <summary>
		/// Gets or sets a list of user IDs that are specifically allowed to subscribe to this stream.
		/// </summary>
		/// <value>The list of user IDs allowed to subscribe to this stream.</value>
		[JsonProperty("subscribe")]
		public List<string> Subscribers { get; set; }

		/// <summary>
		/// Gets or sets a list of user IDs that are specifically allowed to publish to this stream.
		/// </summary>
		/// <value>The list of user IDs allowed to publish to this stream.</value>
		[JsonProperty("publish")]
		public List<string> Publishers { get; set; }

		/// <summary>
		/// Gets or sets the StreamACL group that contains lists of user groups which are authorized on the
		/// stream for subscribing and/or publishing.
		/// Within the StreamACLGroups object, there are 2 lists: one list for the user groups allowed to
		/// subscribe to the stream, and one list for the user groups that are allowed to publish to the stream.
		/// </summary>
		/// <value>The group object which contains the list of user groups allowed to subscribe and/or publish to the stream.</value>
		[JsonProperty("groups")]
		public StreamACLGroups StreamGroups { get; set; }

		/// <summary>
		/// Class that holds the list of user groups that can subscribe to the stream and the list of user groups
		/// that can publish to the stream.
		/// </summary>
		[JsonObject]
		public class StreamACLGroups
		{
			/// <summary>
			/// Gets or sets the list of user groups that can subscribe to the stream.
			/// </summary>
			/// <value>The list of user groups with subscribe access to the stream.</value>
			[JsonProperty("subscribe")]
			public List<string> Subscribers { get; set; }

			/// <summary>
			/// Gets or sets the list of user groups that can publish to the stream.
			/// </summary>
			/// <value>The list of user groups with publish access to the stream.</value>
			[JsonProperty("publish")]
			public List<string> Publishers { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="T:Kinvey.StreamAccessControlList.StreamACLGroups"/> class.
			/// </summary>
			public StreamACLGroups()
			{
				Subscribers = new List<string>();

				Publishers = new List<string>();
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Kinvey.StreamAccessControlList"/> class.
		/// </summary>
		public StreamAccessControlList()
		{
			Subscribers = new List<string>();

			Publishers = new List<string>();

			StreamGroups = new StreamACLGroups();
		}
	}
}
