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
using System.Net.Http;

namespace Kinvey
{
	/// <summary>
	/// Wrapper for a Kinvey-specific exception, which contains information about how to resolve the issue.
	/// </summary>
    public class KinveyException : Exception 
    {
		#region Class Variables and Properties

		private EnumErrorCategory errorCategory;

		private EnumErrorCode errorCode;

		private string error;

		private string description;

		private string debug;

		private string requestID;

        private string info;

        /// <summary>
        /// Gets the error category.
        /// </summary>
        /// <value>The <see cref="KinveyXamarin.EnumErrorCategory"/>  enumeration for this exception.</value>
        public EnumErrorCategory ErrorCategory
		{
			get { return this.errorCategory; }
		}

		/// <summary>
		/// Gets the error code.
		/// </summary>
		/// <value>The <see cref="KinveyXamarin.EnumErrorCode"/> enumeration for this exception.</value>
		public EnumErrorCode ErrorCode
		{
			get { return this.errorCode; }
		}

		/// <summary>
		/// Gets or sets the error.
		/// </summary>
		/// <value>The error of this exception.</value>
		public string Error
		{
			get { return error; }
			set { this.error = value; }
		}

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description of this exception.</value>
		public string Description
		{
			get { return description; }
			set { this.description = value; }
		}

		/// <summary>
		/// Gets or sets the debug string.
		/// </summary>
		/// <value>The debug string for this exception, to help with resolution.</value>
		public string Debug
		{
			get { return debug; }
			set { this.debug = value; }
		}


        /// <summary>
        /// Gets or sets the info string.
        /// </summary>
        /// <value>The info string is additional information about the exception.</value>
        public string Info
        {
            get { return info; }
            set { this.info = value; }
        }

        /// <summary>
        /// Gets or sets the request ID associated with this exception.
        /// This field may be empty if there is no associated request ID with
        /// this exception (e.g. a client-side validation exception).
        /// </summary>
        /// <value>The request ID associated with this exception.</value>
        public string RequestID
		{
			get { return this.requestID == null ? String.Empty : this.requestID; }
			set { this.requestID = value; }
		}

		/// <summary>
		/// Gets or sets the status code for this error.
		/// </summary>
		/// <value>The status code.</value>
		public int StatusCode { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KinveyException"/> class.
		/// </summary>
		/// <param name="errorCategory">The <see cref="KinveyXamarin.EnumErrorCategory"/>  of the exception.</param>
		/// <param name="errorCode">The <see cref="KinveyXamarin.EnumErrorCode"/>  of the exception.</param>
		/// <param name="info">Additional information about the exception, if available.</param>
		/// <param name="innerException">[optional] Inner exception thrown, if available.</param>
		public KinveyException(EnumErrorCategory errorCategory, EnumErrorCode errorCode, string info, Exception innerException = null)
			: base(MessageFromErrorCode(errorCategory, errorCode), innerException)
		{
			this.errorCategory = errorCategory;
			this.errorCode = errorCode;
            this.info = info;

			Tuple<string, string, string> errorInfo = InfoFromErrorCode(errorCategory, errorCode);
			this.error = errorInfo.Item1;
			this.debug = errorInfo.Item2;
			this.description = errorInfo.Item3;
		}

        public KinveyException(EnumErrorCategory errorCategory, EnumErrorCode errorCode, HttpResponseMessage response, Exception innerException)
            : base(innerException.Message, innerException)
		{
			this.errorCategory = errorCategory;
			this.errorCode = errorCode;

			Tuple<string, string, string> errorInfo = InfoFromErrorCode(errorCategory, errorCode);

			StatusCode = (int) response.StatusCode;

			try
			{
				KinveyJsonError errorJSON = KinveyJsonError.parse(response);
				this.error = errorJSON.Error ?? errorInfo.Item1;
				this.debug = errorJSON.Debug ?? errorInfo.Item2;
				this.description = errorJSON.Description ?? errorInfo.Item3;
				this.requestID = HelperMethods.getRequestID(response);

			}
			catch (Exception) { 
				//Catch any exceptions thrown while parsing an unknown responseJSON
			}
		}

		#endregion

		#region Message Formatters

		private static string MessageFromErrorCode(EnumErrorCategory category, EnumErrorCode code)
		{
			Tuple<string, string, string> errorInfo = InfoFromErrorCode(category, code);
			return errorInfo.Item1 + errorInfo.Item2 + errorInfo.Item3;
		}

		private static Tuple<string, string, string> InfoFromErrorCode(EnumErrorCategory category, EnumErrorCode code)
		{
			string error = "";
			string debug = "";
			string description = "";

			switch (code)
			{
				case EnumErrorCode.ERROR_JSON_INVALID:
					error = "";
					debug = "";
					description = "";
				break;

				case EnumErrorCode.ERROR_JSON_PARSE:
					error = "Unable to parse the json in the repsonse";
					debug = "examine BL or DLC to ensure data format is correct.";
					description = "If the exception is caused by `Path <somekey>`, then <somekey> might be a different type than is expected (int instead of of string)";
				break;

				case EnumErrorCode.ERROR_MIC_HOSTNAME_REQUIREMENT_HTTPS:
					error = "MIC Hostname must use the https protocol, trying to set: ";
					debug = "";
					description = "";
					break;

				case EnumErrorCode.ERROR_MIC_MISSING_REDIRECT_CODE:
					error = "Redirect does not contain `code=`, was: ";
					debug = "";
					description = "";
					break;

                case EnumErrorCode.ERROR_MIC_REDIRECT_ERROR:
                    error = "Redirect contains an `error=`, was: ";
                    debug = "";
                    description = "";
                    break;

                case EnumErrorCode.ERROR_MIC_CREDENTIAL_SAVE:
					error = "Could not save account to KeyChain.";
					debug = "";
					description = "";
					break;

				case EnumErrorCode.ERROR_MIC_CREDENTIAL_DELETE:
					error = "Could not delete account from KeyChain.";
					debug = "";
					description = "";
					break;

				case EnumErrorCode.ERROR_REQUIREMENT_CONTENT_TYPE_HEADER:
					error = "The response expects `Content-Type` header to be \"application/json\", but was instead: ";
					debug = "";
					description = "";
					break;

				case EnumErrorCode.ERROR_REQUIREMENT_CUSTOM_REQUEST_PROPERTY_LIMIT:
					error = "Cannot attach more than 2k of Custom Request Properties";
					debug = "";
					description = "";
					break;

				case EnumErrorCode.ERROR_REQUIREMENT_HTTPS:
					error = "Kinvey requires the usage of SSL over http.  Use `https` as the protocol when setting a base URL";
					debug = "";
					description = "";
					break;

				case EnumErrorCode.ERROR_REQUIREMENT_MISSING_USERNAME_PASSWORD:
					error = "Kinvey requires a valid username and password for this login attempt.";
					debug = "Please enter in a valid username and password.";
					description = "A valid username and password is required for login.";
					break;

                case EnumErrorCode.ERROR_REQUIREMENT_MISSING_PUSH_CONFIGURATION_RECEIVERS:
                    error = "FCM receivers are absent for push configuration.";
                    debug = "";
                    description = "To use FCM for push notifications, add com.google.firebase.iid.FirebaseInstanceIdInternalReceiver and com.google.firebase.iid.FirebaseInstanceIdReceiver to your project.";
                    break;

                case EnumErrorCode.ERROR_REQUIREMENT_MISSING_PUSH_CONFIGURATION_CLASS_OVERRIDE:
                    error = "KinveyFCMService class override is absent for push configuration.";
                    debug = "";
                    description = "To use FCM for push notifications, add KinveyFCMService class override to your project.";
                    break;

                case EnumErrorCode.ERROR_USER_ALREADY_LOGGED_IN:
					error = "Attempting to login when a user is already logged in";
					debug = "call `myClient.user().logout().execute() first -or- check `myClient.user().isUserLoggedIn()` before attempting to login again";
					description = "Only one user can be active at a time, and logging in a new user will replace the current user which might not be intended\")";
					break;

				case EnumErrorCode.ERROR_USER_NO_ACTIVE:
					error = "No Active User.";
					debug = "Please log in a user before retrying this request.";
					description = "There is currently no active user.";
					break;

				case EnumErrorCode.ERROR_USER_LOGIN_ATTEMPT:
					error = "Error attempting to log in.";
					debug = "";
					description = "";
					break;

				case EnumErrorCode.ERROR_CLIENT_SHARED_CLIENT_NULL:
					error = "SharedClient is null.";
					debug = "Call Client.Builder(...).build() to build a new Kinvey shared client.";
					description = "A Client must be initialized in the app before using other Kinvey SDK methods. This error indicates that a SharedClient is being accessed by the app before it has been built.";
					break;

				case EnumErrorCode.ERROR_DATASTORE_INVALID_PULL_OPERATION:
					error = "Invalid operation for this data store";
					debug = "Calling pull() on a Network store is not allowed. Use a different type of data store if you need data to be stored locally and pushed to the backend.";
					description = "Refer to the documentation on DataStore types for proper usage of the DataStore caching and syncing APIs.";
					break;

				case EnumErrorCode.ERROR_DATASTORE_PULL_ONLY_ON_CLEAN_SYNC_QUEUE:
					error = "Cannot pull until all local changes are pushed to the backend.";
					debug = "Call store.push() to push pending local changes, or store.purge() to clean local changes.";
					description = "Refer to the documentation on DataStore types for proper usage of the DataStore caching and syncing APIs.";
					break;

				case EnumErrorCode.ERROR_DATASTORE_INVALID_PUSH_OPERATION:
					error = "Invalid operation for this data store";
					debug = "Calling push() on a Network store is not allowed. Use a different type of data store if you need data to be stored locally and pushed to the backend.";
					description = "Refer to the documentation on DataStore types for proper usage of the DataStore caching and syncing APIs.";
					break;

				case EnumErrorCode.ERROR_DATASTORE_INVALID_SYNC_OPERATION:
					error = "Invalid operation for this data store";
					debug = "Calling sync() on a Network store is not allowed. Use a different type of data store if you need data to be stored locally and pushed to the backend.";
					description = "Refer to the documentation on DataStore types for proper usage of the DataStore caching and syncing APIs.";
					break;

                case EnumErrorCode.ERROR_DATASTORE_INVALID_CLEAR_CACHE_OPERATION:
                    error = "Invalid operation for this data store";
                    debug = "Calling ClearCache() on this type of data store is not allowed. Use a different type of data store if you need to remove data from local storage.";
                    description = "Refer to the documentation on DataStore types for proper usage of removing data from local storage.";
                    break;

                case EnumErrorCode.ERROR_DATASTORE_INVALID_PURGE_OPERATION:
                    error = "Invalid operation for this data store";
                    debug = "Calling Purge() on this type of data store is not allowed. Use a different type of data store if you need to remove pending write operations from local storage.";
                    description = "Refer to the documentation on DataStore types for proper usage of removing pending write operations from local storage.";
                    break;

                case EnumErrorCode.ERROR_DATASTORE_INVALID_SYNC_COUNT_OPERATION:
                    error = "Invalid operation for this data store";
                    debug = "Calling GetSyncCount() on this type of data store is not allowed. Use a different type of data store if you need to get the current count of items in the sync queue.";
                    description = "Refer to the documentation on DataStore types for proper usage of getting the current count of items in the sync queue.";
                    break;

                case EnumErrorCode.ERROR_DATASTORE_CACHE_SAVE_INSERT_ENTITY:
					error = "An exception was thrown while trying to save an entity in the cache.";
					debug = "";
					description = "Error in inserting new entity cache with temporary ID.";
					break;

                case EnumErrorCode.ERROR_DATASTORE_CACHE_MULTIPLE_SAVE:
                    error = "An exception was thrown while trying to save multiple entities to the cache.";
                    debug = "";
                    description = "An error in multiple inserting new entities or updating existing ones.";
                    break;

                case EnumErrorCode.ERROR_DATASTORE_CACHE_SAVE_UPDATE_ENTITY:
					error = "An exception was thrown while trying to update an entity in the cache.";
					debug = "";
					description = "Error in updating an existing entity in the cache.";
					break;

				case EnumErrorCode.ERROR_DATASTORE_CACHE_FIND_QUERY:
					error = "An exception was thrown while trying to find entities in the cache.";
					debug = "";
					description = "Error in the query expression used to find entities in the cache.";
					break;

                case EnumErrorCode.ERROR_DATASTORE_CACHE_FIND_BY_ID_GENERAL:
                    error = "An exception was thrown while trying to find entity by id in the cache.";
                    debug = "";
                    description = "Error in finding an entity by id in the cache";
                    break;

                case EnumErrorCode.ERROR_DATASTORE_CACHE_FIND_BY_ID_NOT_FOUND:
                    error = "";
                    debug = "";
                    description = "Response status code does not indicate success: 404 (Not Found).";
                    break;

                case EnumErrorCode.ERROR_DATASTORE_CACHE_SAVE_UPDATE_ID:
					error = "An exception was thrown while trying to save an entity in the cache.";
					debug = "";
					description = "Error in updating cache with permanent entity ID.";
					break;

				case EnumErrorCode.ERROR_DATASTORE_CACHE_REFRESH:
					error = "An exception was thrown while trying to refresh entities in the cache.";
					debug = "";
					description = "Error in trying to insert or update entities in the cache based on the list of given entities.";
					break;

				case EnumErrorCode.ERROR_DATASTORE_CACHE_REMOVE_ENTITY:
					error = "An exception was thrown while trying to remove an entity from the cache.";
					debug = "";
					description = "Error in trying to delete an entity from the cache based on the given entity ID.";
					break;

				case EnumErrorCode.ERROR_DATASTORE_CACHE_CLEAR:
					error = "An exception was thrown while trying clear all the entities from the cache.";
					debug = "";
					description = "Error while trying to clear all the data in the cache.  No data was deleted from the cache.";
					break;

				case EnumErrorCode.ERROR_DATASTORE_CACHE_CLEAR_QUERY:
					error = "An exception was thrown while trying clear entities from the cache based on a query.";
					debug = "Call Clear() to clear the entire cache";
					description = "Error while trying to clear data in the cache based on a query.  No data was deleted from the cache.";
					break;

                case EnumErrorCode.ERROR_DATASTORE_NOT_COMPATIBLE_KINVEY_API_VERSION:
                    error = "Not compatible Kinvey api version.";
                    debug = "";
                    description = "The current functionality is not compatible with the existing Kinvey api version.";
                    break;

                case EnumErrorCode.ERROR_DATASTORE_EMPTY_ARRAY_OF_ENTITIES:
                    error = "An empty array of entities.";
                    debug = "";
                    description = "An array of entities to be saved must not be empty.";
                    break;

                case EnumErrorCode.ERROR_DATASTORE_LIMIT_OF_ENTITIES_TO_BE_SAVED:
                    error = "Entities count limit was exceeded.";
                    debug = "";
                    description = string.Concat("The current limit of entities count is ", Constants.NUMBER_LIMIT_OF_ENTITIES);
                    break;

                case EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED:
					error = "An exception was thrown while trying to call a method that is not implemented.";
					debug = "Consult the reference guides for information on supported methods for the given class.";
					description = "The method being called on this class/object is not currently implemnted by the SDK.";
					break;

				case EnumErrorCode.ERROR_LINQ_WHERE_CLAUSE_NOT_SUPPORTED:
					error = "An exception was thrown while trying to use a LINQ `Where` clause that is not supported.";
					debug = "Consult the reference guides for information on supported LINQ clauses.";
					description = "The LINQ `Where` clause being called is not supported by the SDK.";
					break;

				case EnumErrorCode.ERROR_CUSTOM_ENDPOINT_ERROR:
					error = "An exception was thrown while trying to execute a custom endpoint.";
					debug = "Inspect the StatusCode property to determine the cause of the exception.";
					description = "A 4xx/5xx status code was set in the response by the custom endpoint.";
					break;

					#region Realtime errors

				case EnumErrorCode.ERROR_REALTIME_ERROR:
					error = "An exception was thrown while trying to execute a realtime request.";
					debug = "Inspect the StatusCode property to determine the cause of the exception.";
					description = "A 4xx/5xx status code was set in the response by the realtime service.";
					break;

				case EnumErrorCode.ERROR_REALTIME_CRITICAL_VERIFY_CIPHER_KEY:
					error = "Invalid cipher key.";
					debug = "Verify your cipher key.";
					description = "";
					break;

				case EnumErrorCode.ERROR_REALTIME_CRITICAL_INCORRECT_SUBSBRIBE_KEY:
					error = "Incorrect subscribe key.";
					debug = "Please provide correct subscribe key.";
					description = "This corresponds to a 401 on the server due to a bad sub key.";
					break;

				case EnumErrorCode.ERROR_REALTIME_CRITICAL_NOT_AUTHORIZED_ON_CHANNEL:
					error = "Not authorized.";
					debug = "Check the permissions on the channel.  Also verify authentication key, to check access.";
					description = "This corresponds to the user not being authorized to publish and/or subscribe on this channel.";
					break;

				case EnumErrorCode.ERROR_REALTIME_CRITICAL_INTERNAL_SERVER_ERROR:
					error = "Internal Server Error.";
					debug = "Please try again. If same problem persists, please contact PubNub support.";
					description = "Unexpected error occured at PubNub Server";
					break;

				case EnumErrorCode.ERROR_REALTIME_CRITICAL_BAD_GATEWAY:
					error = "Bad Gateway.";
					debug = "Please try again. If same problem persists, please contact PubNub support.";
					description = "Unexpected error occured at PubNub Server.";
					break;

				case EnumErrorCode.ERROR_REALTIME_CRITICAL_GATEWAY_TIMEOUT:
					error = "Gateway Timeout.";
					debug = "Please try again. If same problem persists, please contact PubNub support.";
					description = "No response from server due to PubNub server timeout.";
					break;

				case EnumErrorCode.ERROR_REALTIME_CRITICAL_UNKNOWN:
					error = "Unknown critical PubNub error.";
					debug = "";
					description = "";
					break;

					#endregion

				default:
					error = "Unknown error";
					debug = "Unknown error";
					description = "Unknown error";
					break;
			}

			return new Tuple<string, string, string>(error, debug, description);
		}

        #endregion
    }
}
