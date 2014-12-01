// Copyright (c) 2014, Kinvey, Inc. All rights reserved.
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
using System.Text;

namespace KinveyXamarin
{
	/// <summary>
	/// Builds a Kinvey-style query.
	/// </summary>
	public class StringQueryBuilder : IQueryBuilder
	{

		/// <summary>
		/// The StringBuilder used for building up the query string.
		/// </summary>
		private StringBuilder builder;
		/// <summary>
		/// The StringBuilder used for building up the modifiers on the query string (such as skip, limit
		/// </summary>
		private StringBuilder dangler;

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.StringQueryBuilder"/> class.
		/// </summary>
		public StringQueryBuilder()
		{
			Reset ();
		}

		/// <summary>
		/// Reset this instance by creating new StringBuilders.
		/// </summary>
		public void Reset(){
			this.builder = new StringBuilder ();
			this.dangler = new StringBuilder ();
		}

		/// <summary>
		/// Writes the specified value to the query Builder.
		/// </summary>
		/// <param name="value">Value.</param>
		public void Write(object value)
		{
			builder.Append(value);
		}

		/// <summary>
		/// Gets the full string by combining the two StringBuilders.
		/// </summary>
		/// <returns>The full string.</returns>
		public String GetFullString(){
			return builder.ToString () + dangler.ToString ();
		}

		/// <summary>
		/// Writes the specified value as a query modifier.
		/// </summary>
		/// <param name="value">Value.</param>
		public void Dangle(object value){
			dangler.Append (value);

		}
	}

}