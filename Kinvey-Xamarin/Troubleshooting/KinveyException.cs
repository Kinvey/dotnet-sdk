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

		private string reason;

		private string fix;

		private string explanation;

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
		/// Gets or sets the reason.
		/// </summary>
		/// <value>The reason for this exception.</value>
		public string Reason
		{
			get { return reason; }
			set { this.reason = value; }
		}

		/// <summary>
		/// Gets or sets the fix.
		/// </summary>
		/// <value>The potential fix for this exception.</value>
		public string Fix
		{
			get { return fix; }
			set { this.fix = value; }
		}

		/// <summary>
		/// Gets or sets the explanation.
		/// </summary>
		/// <value>The explanation of this exception.</value>
		public string Explanation
		{
			get { return explanation; }
			set { this.explanation = value; }
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
			this.reason = errorInfo.Item1;
			this.fix = errorInfo.Item2;
			this.explanation = errorInfo.Item3;
		}

		public KinveyException(EnumErrorCategory errorCategory, EnumErrorCode errorCode, IRestResponse responseJSON)
			: base(responseJSON.ErrorMessage, responseJSON.ErrorException)
		{
			this.errorCategory = errorCategory;
			this.errorCode = errorCode;

			KinveyJsonError errorJSON = KinveyJsonError.parse(responseJSON);
			this.reason = errorJSON.Error;
			this.fix = errorJSON.Debug;
			this.explanation = errorJSON.Description;
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
			string reason = "";
			string fix = "";
			string explanation = "";

			switch (code)
			{
				case EnumErrorCode.ERROR_JSON_INVALID:
					reason = "";
					fix = "";
					explanation = "";
				break;

				case EnumErrorCode.ERROR_JSON_PARSE:
					reason = "Unable to parse the json in the repsonse";
					fix = "examine BL or DLC to ensure data format is correct.";
					explanation = "If the exception is caused by `Path <somekey>`, then <somekey> might be a different type than is expected (int instead of of string)";
				break;

				case EnumErrorCode.ERROR_MIC_MISSING_REDIRECT_CODE:
					reason = "MIC Hostname must use the https protocol, trying to set: ";
					fix = "";
					explanation = "";
					break;

				case EnumErrorCode.ERROR_MIC_HOSTNAME_REQUIREMENT_HTTPS:
					reason = "Redirect does not contain `code=`, was: ";
					fix = "";
					explanation = "";
					break;

				case EnumErrorCode.ERROR_REQUIREMENT_CONTENT_TYPE_HEADER:
					reason = "The response expects `Content-Type` header to be \"application/json\", but was instead: ";
					fix = "";
					explanation = "";
					break;

				case EnumErrorCode.ERROR_REQUIREMENT_CUSTOM_REQUEST_PROPERTY_LIMIT:
					reason = "Cannot attach more than 2k of Custom Request Properties";
					fix = "";
					explanation = "";
					break;

				case EnumErrorCode.ERROR_REQUIREMENT_HTTPS:
					reason = "Kinvey requires the usage of SSL over http.  Use `https` as the protocol when setting a base URL";
					fix = "";
					explanation = "";
					break;

				case EnumErrorCode.ERROR_USER_ALREADY_LOGGED_IN:
					reason = "Attempting to login when a user is already logged in";
					fix = "call `myClient.user().logout().execute() first -or- check `myClient.user().isUserLoggedIn()` before attempting to login again";
					explanation = "Only one user can be active at a time, and logging in a new user will replace the current user which might not be intended\")";
					break;

				case EnumErrorCode.ERROR_USER_NO_ACTIVE:
					reason = "";
					fix = "";
					explanation = "";
					break;

				case EnumErrorCode.ERROR_USER_LOGIN_ATTEMPT:
					reason = "Error attempting to log in.";
					fix = "";
					explanation = "";
					break;

				case EnumErrorCode.ERROR_CLIENT_SHARED_CLIENT_NULL:
					reason = "SharedClient is null.";
					fix = "Call Client.Builder(...).build() to build a new Kinvey shared client.";
					explanation = "A Client must be initialized in the app before using other Kinvey SDK methods. This error indicates that a SharedClient is being accessed by the app before it has been built.";
					break;

				case EnumErrorCode.ERROR_DATASTORE_INVALID_PULL_OPERATION:
					reason = "Invalid operation for this data store";
					fix = "Calling pull() on a Network store is not allowed. Use a different type of data store if you need data to be stored locally and pushed to the backend.";
					explanation = "Refer to the documentation on DataStore types for proper usage of the DataStore caching and syncing APIs.";
					break;

				case EnumErrorCode.ERROR_DATASTORE_PULL_ONLY_ON_CLEAN_SYNC_QUEUE:
					reason = "Cannot pull until all local changes are pushed to the backend.";
					fix = "Call store.push() to push pending local changes, or store.purge() to clean local changes.";
					explanation = "Refer to the documentation on DataStore types for proper usage of the DataStore caching and syncing APIs.";
					break;

				case EnumErrorCode.ERROR_DATASTORE_INVALID_PUSH_OPERATION:
					reason = "Invalid operation for this data store";
					fix = "Calling push() on a Network store is not allowed. Use a different type of data store if you need data to be stored locally and pushed to the backend.";
					explanation = "Refer to the documentation on DataStore types for proper usage of the DataStore caching and syncing APIs.";
					break;

				case EnumErrorCode.ERROR_DATASTORE_INVALID_SYNC_OPERATION:
					reason = "Invalid operation for this data store";
					fix = "Calling sync() on a Network store is not allowed. Use a different type of data store if you need data to be stored locally and pushed to the backend.";
					explanation = "Refer to the documentation on DataStore types for proper usage of the DataStore caching and syncing APIs.";
					break;

				case EnumErrorCode.ERROR_DATASTORE_CACHE_SAVE_INSERT_ENTITY:
					reason = "An exception was thrown while trying to save an entity in the cache.";
					fix = "";
					explanation = "Error in inserting new entity cache with temporary ID.";
					break;

				case EnumErrorCode.ERROR_DATASTORE_CACHE_SAVE_UPDATE_ENTITY:
					reason = "An exception was thrown while trying to update an entity in the cache.";
					fix = "";
					explanation = "Error in updating an existing entity in the cache.";
					break;

				case EnumErrorCode.ERROR_DATASTORE_CACHE_FIND_QUERY:
					reason = "An exception was thrown while trying to find entities in the cache.";
					fix = "";
					explanation = "Error in the query expression used to find entities in the cache.";
					break;

				case EnumErrorCode.ERROR_DATASTORE_CACHE_SAVE_UPDATE_ID:
					reason = "An exception was thrown while trying to save an entity in the cache.";
					fix = "";
					explanation = "Error in updating cache with permanent entity ID.";
					break;

				case EnumErrorCode.ERROR_DATASTORE_CACHE_REFRESH:
					reason = "An exception was thrown while trying to refresh entities in the cache.";
					fix = "";
					explanation = "Error in trying to insert or update entities in the cache based on teh list of given entities.";
					break;

				case EnumErrorCode.ERROR_DATASTORE_CACHE_REMOVE_ENTITY:
					reason = "An exception was thrown while trying to remove an entity from the cache.";
					fix = "";
					explanation = "Error in trying to delete an entity from the cache based on the given entity ID.";
					break;

				default:
					reason = "Unknown error";
					fix = "Unknown error";
					explanation = "Unknown error";
					break;
			}

			return new Tuple<string, string, string>(reason, fix, explanation);
		}

		#endregion
    }
}
