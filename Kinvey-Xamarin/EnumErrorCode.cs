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

namespace KinveyXamarin
{
	/// <summary>
	/// Enumeration for error codes that are client-side errors
	/// </summary>
	public enum EnumErrorCode
	{
		/// <summary>
		/// The shared client is null.
		/// </summary>
		ERROR_CLIENT_SHARED_CLIENT_NULL,

		/// <summary>
		/// Error saving a new entity in the local cache.
		/// </summary>
		ERROR_DATASTORE_CACHE_SAVE_INSERT_ENTITY,

		/// <summary>
		/// Error saving the permanent entity ID in the local cache.
		/// </summary>
		ERROR_DATASTORE_CACHE_SAVE_UPDATE_ID,

		/// <summary>
		/// Error saving an updated entity in the local cache.
		/// </summary>
		ERROR_DATASTORE_CACHE_SAVE_UPDATE_ENTITY,

		/// <summary>
		/// Error finding entities by query in the local cache.
		/// </summary>
		ERROR_DATASTORE_CACHE_FIND_QUERY,

		/// <summary>
		/// Error refreshing the local cache with a list of new and updated entities.
		/// </summary>
		ERROR_DATASTORE_CACHE_REFRESH,

		/// <summary>
		/// Error removing an entity by ID in the local cache.
		/// </summary>
		ERROR_DATASTORE_CACHE_REMOVE_ENTITY,

		/// <summary>
		/// Error due to an invalid pull operation being performed on this type of datastore.
		/// </summary>
		ERROR_DATASTORE_INVALID_PULL_OPERATION,

		/// <summary>
		/// Error due to an invalid push operation being performed on this type of datastore.
		/// </summary>
		ERROR_DATASTORE_INVALID_PUSH_OPERATION,

		/// <summary>
		/// Error due to an invalid sync operation being performed on this type of datastore.
		/// </summary>
		ERROR_DATASTORE_INVALID_SYNC_OPERATION,

		/// <summary>
		/// Error due to attempting to pull when the sync queue has not been cleared.  Try to perform a <code>push</code> operation first.
		/// </summary>
		ERROR_DATASTORE_PULL_ONLY_ON_CLEAN_SYNC_QUEUE,

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
		/// User Error - Problem attempting to log in.
		/// </summary>
		ERROR_USER_LOGIN_ATTEMPT,

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
