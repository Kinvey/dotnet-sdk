// Copyright (c) 2016, Kinvey, Inc. All rights reserved.
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

namespace Kinvey
{
	/// <summary>
	/// Enumeration for all possible reduce functions used for grouping/aggregation.
	/// </summary>
	public enum EnumReduceFunction
	{
		/// <summary>
		/// Sum reduce function.
		/// </summary>
		REDUCE_FUNCTION_SUM,

		/// <summary>
		/// Count reduce function.
		/// </summary>
		REDUCE_FUNCTION_COUNT,

		/// <summary>
		/// Min reduce function.
		/// </summary>
		REDUCE_FUNCTION_MIN,

		/// <summary>
		/// Max reduce function.
		/// </summary>
		REDUCE_FUNCTION_MAX,

		/// <summary>
		/// Average reduce function.
		/// </summary>
		REDUCE_FUNCTION_AVERAGE
	}
}
