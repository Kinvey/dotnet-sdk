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
	/// Enumeration for error codes that are client-side errors
	/// </summary>
	public enum EnumErrorCategory
	{
		/// <summary>
		/// The error code associated with this category has to do with Kinvey client.
		/// </summary>
		ERROR_CLIENT,

		/// <summary>
		/// This error category has to do with a missing requirement
		/// </summary>
		ERROR_REQUIREMENT,

		/// <summary>
		/// The error code associated with this category has to do with the local part of the datastore.
		/// </summary>
		ERROR_DATASTORE_CACHE,

		/// <summary>
		/// The error code associated with this category has to do with the network part of the datastore.
		/// </summary>
		ERROR_DATASTORE_NETWORK,

		/// <summary>
		/// The error code associated with this category has to do with the use of the Kinvey File API.
		/// </summary>
		ERROR_FILE,

		/// <summary>
		/// The error code associated with this category has to do with the user of the app.
		/// </summary>
		ERROR_USER,

		/// <summary>
		/// The error code associated with this category has to do with an error received from the backend.
		/// </summary>
		ERROR_BACKEND,

		/// <summary>
		/// The error code associated with this category has to do with an error
		/// received from thebackend because of an issue with a custom endpoint.
		/// </summary>
		ERROR_CUSTOM_ENDPOINT,

		/// <summary>
		/// The error code associated with this category has to do with an error
		/// received from the reaaltime service because of an issue.
		/// </summary>
		ERROR_REALTIME,

		/// <summary>
		/// General error category
		/// </summary>
		ERROR_GENERAL
	}
}
