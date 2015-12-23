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

namespace KinveyXamarin
{
	/// <summary>
	/// JSON representation of the _kmd field present on every entity stored in Kinvey
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class KinveyMetaData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KinveyMetaData"/> class.
		/// </summary>
		[Preserve]
		public KinveyMetaData ()
		{
		}

		/// <summary>
		/// The field name within every JSON object.
		/// </summary>
		public const string JSON_FIELD_NAME = "_kmd";

		/// <summary>
		/// Gets or sets the last modified time.
		/// </summary>
		/// <value>The last modified time.</value>
		[Preserve]
		[JsonProperty("lmt")]
		public String lastModifiedTime{get; set;}

		/// <summary>
		/// Gets or sets the entity creation time.
		/// </summary>
		/// <value>The entity creation time.</value>
		[Preserve]
		[JsonProperty("ect")]
		public String entityCreationTime{get; set;}
	}
}

