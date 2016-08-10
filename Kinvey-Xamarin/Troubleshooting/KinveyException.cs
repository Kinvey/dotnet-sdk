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
using RestSharp;

namespace KinveyXamarin
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

			Tuple<string, string, string> errorInfo = InfoFromErrorCode(errorCategory, errorCode);
			this.error = errorInfo.Item1;
			this.debug = errorInfo.Item2;
			this.description = errorInfo.Item3;
		}

		public KinveyException(EnumErrorCategory errorCategory, EnumErrorCode errorCode, IRestResponse responseJSON)
			: base(responseJSON.ErrorMessage, responseJSON.ErrorException)
		{
			this.errorCategory = errorCategory;
			this.errorCode = errorCode;

			KinveyJsonError errorJSON = KinveyJsonError.parse(responseJSON);
			this.error = errorJSON.Error;
			this.debug = errorJSON.Debug;
			this.description = errorJSON.Description;
			this.requestID = HelperMethods.getRequestID(responseJSON);
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

				case EnumErrorCode.ERROR_MIC_MISSING_REDIRECT_CODE:
					error = "MIC Hostname must use the https protocol, trying to set: ";
					debug = "";
					description = "";
					break;

				case EnumErrorCode.ERROR_MIC_HOSTNAME_REQUIREMENT_HTTPS:
					error = "Redirect does not contain `code=`, was: ";
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

				case EnumErrorCode.ERROR_DATASTORE_CACHE_SAVE_INSERT_ENTITY:
					error = "An exception was thrown while trying to save an entity in the cache.";
					debug = "";
					description = "Error in inserting new entity cache with temporary ID.";
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

				case EnumErrorCode.ERROR_DATASTORE_CACHE_SAVE_UPDATE_ID:
					error = "An exception was thrown while trying to save an entity in the cache.";
					debug = "";
					description = "Error in updating cache with permanent entity ID.";
					break;

				case EnumErrorCode.ERROR_DATASTORE_CACHE_REFRESH:
					error = "An exception was thrown while trying to refresh entities in the cache.";
					debug = "";
					description = "Error in trying to insert or update entities in the cache based on teh list of given entities.";
					break;

				case EnumErrorCode.ERROR_DATASTORE_CACHE_REMOVE_ENTITY:
					error = "An exception was thrown while trying to remove an entity from the cache.";
					debug = "";
					description = "Error in trying to delete an entity from the cache based on the given entity ID.";
					break;

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
