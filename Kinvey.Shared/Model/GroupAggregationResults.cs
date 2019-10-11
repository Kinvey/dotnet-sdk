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
	/// JSON representation of the fields return from group aggreation functions
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class GroupAggregationResults
	{
        /// <summary>
        /// The field used to group the entities prior to aggregation.
        /// </summary>
        /// <value>The GroupField property gets/sets the value of the string field, _groupField.</value>
        public string GroupField { get; set; }

        /// <summary>
        /// The result of the aggregation for this group
        /// </summary>
        /// <value>The Result property gets/sets the value of the int field, _result.</value>
        public int Result { get; set; }
	}
}
