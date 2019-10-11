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

namespace Kinvey
{
	/// <summary>
	/// This interface defines the behaivor of building a query.
	/// </summary>
	public interface IQueryBuilder
	{
		/// <summary>
		/// Reset this query builder instance.
		/// </summary>
		void Reset();

		/// <summary>
		/// Write the specified value to the mongo-style query
		/// </summary>
		/// <param name="value">Value to be appended to the query.</param>
		void Write(object value);

		/// <summary>
		/// Write the specified value as a query modifier.
		/// </summary>
		/// <param name="value">Value to be appended to the modifier portion of the mongo-style query string.</param>
		void AddModifier(object value);

		/// <summary>
		/// Gets the full mongo-style query string.
		/// </summary>
		/// <returns>The full mongo query, complete with modifiers.</returns>
		string BuildQueryString();
	}
}
