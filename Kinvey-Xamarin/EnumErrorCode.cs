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
	/// Enumeration for error codes that are client-side errors
	/// </summary>
	public enum EnumErrorCode
	{
		/// <summary>
		/// JSON Error - Invalid format.
		/// </summary>
		ERROR_JSON_INVALID,

		/// <summary>
		/// JSON Error - Parsing issue.
		/// </summary>
		ERROR_JSON_PARSE,

		/// <summary>
		/// User Error - A user is already logged into the app.
		/// </summary>
		ERROR_USER_ALREADY_LOGGED_IN,

		/// <summary>
		/// User Error - There is no active user.
		/// </summary>
		ERROR_USER_NO_ACTIVE,

		/// <summary>
		/// MIC Error - Missing redirect code.
		/// </summary>
		ERROR_MIC_MISSING_REDIRECT_CODE,

		/// <summary>
		/// MIC Error - Hostname URL missing 'HTTPS' protocol.
		/// </summary>
		ERROR_MIC_HOSTNAME_REQUIREMENT_HTTPS,

		/// <summary>
		/// Requirement Error - Base URL missing 'HTTPS' protocol.
		/// </summary>
		ERROR_REQUIREMENT_HTTPS,

		/// <summary>
		/// Requirement Error - Content-Type header is incorrect.
		/// </summary>
		ERROR_REQUIREMENT_CONTENT_TYPE_HEADER,

		/// <summary>
		/// Requirement Error - Limit on custom request property headers has been exceeded.
		/// </summary>
		ERROR_REQUIREMENT_CUSTOM_REQUEST_PROPERTY_LIMIT,

		/// <summary>
		/// Error condition for a method not being implemented
		/// </summary>
		ERROR_METHOD_NOT_IMPLEMENTED,

		/// <summary>
		/// General error condition
		/// </summary>
		ERROR_GENERAL
	}
}
