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

namespace Kinvey
{
	/// <summary>
	/// This class represents the response sent from Kinvey after a delete has been executed.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class KinveyDeleteResponse
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KinveyDeleteResponse"/> class.
		/// </summary>
		public KinveyDeleteResponse ()
		{
		}
		/// <summary>
		/// Gets or sets the count of entities deleted.
		/// </summary>
		/// <value>The count.</value>
		[JsonProperty]
		public int count{get; set;}

		/// <summary>
		/// Represents the IDs of the entities deleted, when available.
		/// For internal use only.
		/// </summary>
		/// <value>List of IDs for deleted entities</value>
		internal List<string> IDs { get; set;}
	}
}

