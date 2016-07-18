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

namespace KinveyXamarin
{
	/// <summary>
	/// Kinvey utility for assertions
	/// </summary>
	public class KAssert
	{

		/// <summary>
		/// Asserts that a string is not null
		/// </summary>
		/// <returns>the string, if it's not null.  If it is, an exception will be thrown.</returns>
		/// <param name="toAssert">The string to check for a null value.</param>
		/// <param name="message">the message to use in the exception if the string is null.</param>
		public static String notNull(string toAssert, string message){
			if (toAssert == null) {
				throw new ArgumentNullException (message);
			}
			return toAssert;
		}

		public static Object notNull(Object toAssert, string message){
			if (toAssert == null) {
				throw new ArgumentNullException (message);
			}
			return toAssert;
		}
	}
}

