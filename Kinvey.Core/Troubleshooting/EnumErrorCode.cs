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
		/// Error removing all entities in the local cache table.
		/// </summary>
		ERROR_DATASTORE_CACHE_CLEAR,

		/// <summary>
		/// Error removing entities in the local cache table based on query.
		/// </summary>
		ERROR_DATASTORE_CACHE_CLEAR_QUERY,

		/// <summary>
		/// Error removing an entity by ID in the local cache.
		/// </summary>
		ERROR_DATASTORE_CACHE_REMOVE_ENTITY,

        /// <summary>
		/// Error removing entities according to query in the local cache.
		/// </summary>
		ERROR_DATASTORE_CACHE_REMOVING_ENTITIES_ACCORDING_TO_QUERY,

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
		/// Error due to attempting to pull when the sync queue has not been cleared.  Try to perform a <see cref="KinveyXamarin.DataStore{T}.PushAsync"/> operation first.
		/// </summary>
		ERROR_DATASTORE_PULL_ONLY_ON_CLEAN_SYNC_QUEUE,

        /// <summary>
		/// Error due to attempting to remove entities with null query.
		/// </summary>
		ERROR_DATASTORE_NULL_QUERY,

        /// <summary>
        /// Error due to attempting a file metadata operation without a file ID.
        /// </summary>
        ERROR_FILE_MISSING_FILE_ID,

		/// <summary>
		/// Error due to attempting a file upload operation with either null or missing metadata information.  Verify that the <see cref="KinveyXamarin.FileMetaData"/> is valid.
		/// </summary>
		ERROR_FILE_UPLOAD_MISSING_METADATA_INFORMATION,

		/// <summary>
		/// Error due to attempting a file download operation with either null or missing metadata information.
		/// Verify that the <see cref="KinveyXamarin.FileMetaData"/> is valid, or download by query if FileID is unknown.
		/// </summary>
		ERROR_FILE_DOWNLOAD_MISSING_METADATA_INFORMATION,

		/// <summary>
		/// JSON Error - Invalid format.
		/// </summary>
		ERROR_JSON_INVALID,

		/// <summary>
		/// JSON Error - Parsing issue.
		/// </summary>
		ERROR_JSON_PARSE,

		/// <summary>
		/// JSON Error - issue with response.
		/// </summary>
		ERROR_JSON_RESPONSE,

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
		/// User Error - Loading credential from credential store.
		/// </summary>
		ERROR_USER_LOAD_CREDENTIAL,

		/// <summary>
		/// User Error - Storing credential in credential store.
		/// </summary>
		ERROR_USER_STORE_CREDENTIAL,

		/// <summary>
		/// User Error - Retrieving credential for given account.
		/// </summary>
		ERROR_USER_GET_CREDENTIAL_FOR_ACCOUNT,

		/// <summary>
		/// MIC Error - Missing redirect code.
		/// </summary>
		ERROR_MIC_MISSING_REDIRECT_CODE,

        /// <summary>
        /// MIC Error - Error in Redirect URI.
        /// </summary>
        ERROR_MIC_REDIRECT_ERROR,

        /// <summary>
        /// MIC Error - Hostname URL missing 'HTTPS' protocol.
        /// </summary>
        ERROR_MIC_HOSTNAME_REQUIREMENT_HTTPS,

		/// <summary>
		/// MIC Error - Credential could not be saved from the credential store.
		/// </summary>
		ERROR_MIC_CREDENTIAL_SAVE,

		/// <summary>
		/// MIC Error - Credential could not be deleted from the credential store.
		/// </summary>
		ERROR_MIC_CREDENTIAL_DELETE,

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
		/// Requirement Error - Username and/or password missing from login attempt.
		/// </summary>
		ERROR_REQUIREMENT_MISSING_USERNAME_PASSWORD,

		/// <summary>
		/// Error condition for a method not being implemented
		/// </summary>
		ERROR_REQUIREMENT_INVALID_PARAMETER,

		/// <summary>
		/// Error condition for a method not being implemented
		/// </summary>
		ERROR_METHOD_NOT_IMPLEMENTED,

		/// <summary>
		/// Error condition for a LINQ where clause not supported.
		/// </summary>
		ERROR_LINQ_WHERE_CLAUSE_NOT_SUPPORTED,

		/// <summary>
		/// Error code for a backend custom endpoint problem.
		/// </summary>
		ERROR_CUSTOM_ENDPOINT_ERROR,

		/// <summary>
		/// Error code for a backend realtime request error.
		/// </summary>
		ERROR_REALTIME_ERROR,

		ERROR_REALTIME_CRITICAL_VERIFY_CIPHER_KEY,
		ERROR_REALTIME_CRITICAL_INCORRECT_SUBSBRIBE_KEY,
		ERROR_REALTIME_CRITICAL_NOT_AUTHORIZED_ON_CHANNEL,
		ERROR_REALTIME_CRITICAL_INTERNAL_SERVER_ERROR,
		ERROR_REALTIME_CRITICAL_BAD_GATEWAY,
		ERROR_REALTIME_CRITICAL_GATEWAY_TIMEOUT,
		ERROR_REALTIME_CRITICAL_UNKNOWN,

		ERROR_REALTIME_WARNING_VERIFY_HOSTNAME,
		ERROR_REALTIME_WARNING_CHECK_NETWORK_CONNECTION,
		ERROR_REALTIME_WARNING_NO_NETWORK_CONNECTION,
		ERROR_REALTIME_WARNING_VERIFY_CIPHER_KEY,
		ERROR_REALTIME_WARNING_PROTOCOL_ERROR,
		ERROR_REALTIME_WARNING_SERVER_PROTOCOL_VIOLATION,
		ERROR_REALTIME_WARNING_MESSAGE_TOO_LARGE,
		ERROR_REALTIME_WARNING_BAD_REQUEST,
		ERROR_REALTIME_WARNING_INVALID_PUBLISH_KEY,
		ERROR_REALTIME_WARNING_PAM_NOT_ENABLED,
		ERROR_REALTIME_WARNING_INCORRECT_PUBLIC_OR_SECRET_KEY,
		ERROR_REALTIME_WARNING_URL_LENGTH_TOO_LONG,
		ERROR_REALTIME_WARNING_UNDOCUMENTED_ERROR,
		ERROR_REALTIME_WARNING_UNKNOWN,

		ERROR_REALTIME_INFORMATIONAL_NO_NETWORK_CONNECTION,
		//ERROR_REALTIME_INFORMATIONAL_NETWORK_CONNECTION_BACK,
		ERROR_REALTIME_INFORMATIONAL_DUPLICATE_CHANNEL_SUBSCRIPTION,
		ERROR_REALTIME_INFORMATIONAL_INVALID_CHANNEL_NAME,
		ERROR_REALTIME_INFORMATIONAL_CHANNEL_NOT_SUBSCRIBED,
		ERROR_REALTIME_INFORMATIONAL_UNSUBSCRIBE_INCOMPLETE,
		ERROR_REALTIME_INFORMATIONAL_NETWORK_NOT_AVAILABLE,
		ERROR_REALTIME_INFORMATIONAL_NETWORK_MAX_RETRIES_REACHED,
		ERROR_REALTIME_INFORMATIONAL_PUBLISH_OPERATION_TIMEOUT,
		ERROR_REALTIME_INFORMATIONAL_UNKNOWN,

		/// <summary>
		/// Error code for the realtime service attempting to be used without first registering for realtime access.
		/// </summary>
		ERROR_REALTIME_NOT_REGISTERED,

		/// <summary>
		/// General error condition
		/// </summary>
		ERROR_GENERAL
	}
}
