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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinveyXamarin
{
	/// <summary>
	/// Wrapper for a kinvey specific exception containing information about how to resolve the issue. 
	/// </summary>
    public class KinveyException : Exception 
    {
		/// <summary>
		/// The reason.
		/// </summary>
        private string reason;
		/// <summary>
		/// The fix.
		/// </summary>
        private string fix;
		/// <summary>
		/// The explanation.
		/// </summary>
        private string explanation;

		/// <summary>
		/// [optional] The request ID associated with this exception.
		/// This field may be empty if there is no associated request ID with
		/// this exception (e.g. a client-side validation exception)
		/// </summary>
		private string requestID;

		private EnumErrorCode errorCode;

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KinveyException"/> class.
		/// </summary>
		/// <param name="reason">Reason.</param>
		/// <param name="fix">Fix.</param>
		/// <param name="explanation">Explanation.</param>
		public KinveyException(EnumErrorCode errorCode, string reason, string fix, string explanation)
			: base(FormatMessage(reason, fix, explanation))
		{
			this.errorCode = errorCode;
			this.reason = reason;
			this.fix = fix;
			this.explanation = explanation;
		}

		public KinveyException(EnumErrorCode errorCode, string reason, string fix, string explanation, Exception innerException)
			: base(FormatMessage(reason, fix, explanation), innerException)
		{
			this.errorCode = errorCode;
			this.reason = reason;
			this.fix = fix;
			this.explanation = explanation;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KinveyException"/> class.
		/// </summary>
		/// <param name="reason">Reason.</param>
		/// <param name="fix">Fix.</param>
		/// <param name="explanation">Explanation.</param>
		public KinveyException(EnumErrorCode errorCode, string info = null)
			: base(MessageFromErrorCode(errorCode, info))
		{
			this.errorCode = errorCode;
			this.reason = MessageFromErrorCode(errorCode, info);
		}
			
		/// <summary>
		/// Gets or sets the reason.
		/// </summary>
		/// <value>The reason.</value>
        public string Reason
        {
            get { return reason; }
            set { this.reason = value; }
        }

      
		/// <summary>
		/// Gets or sets the fix.
		/// </summary>
		/// <value>The fix.</value>
        public string Fix
        {
            get { return fix; }
            set { this.fix = value; }
        }

		/// <summary>
		/// Gets or sets the explanation.
		/// </summary>
		/// <value>The explanation.</value>
        public string Explanation
        {
            get { return explanation; }
            set { this.explanation = value; }
        }

		/// <summary>
		/// Gets or sets the request ID.  Can be empty if there is no associated request ID.
		/// </summary>
		/// <value>The request ID associated with this exception.</value>
		public string RequestID
		{
			get { return this.requestID == null ? "" : this.requestID; }
			set { this.requestID = value; }
		}

		public EnumErrorCode ErrorCode
		{
			get { return this.errorCode; }
		}

		/// <summary>
		/// Formats the message.
		/// </summary>
		/// <returns>The message.</returns>
		/// <param name="reason">Reason.</param>
		/// <param name="fix">Fix.</param>
		/// <param name="explanation">Explanation.</param>
        private static String FormatMessage(string reason, string fix, string explanation)
        {
            return "\nREASON: " + reason + "\n" + "FIX: " + fix + "\n" + "EXPLANATION: " + explanation + "\n";
        }

		/// <summary>
		/// Formats the message.
		/// </summary>
		/// <returns>The message.</returns>
		/// <param name="reason">Reason.</param>
		private static String FormatMessage(string reason)
		{
			return "\nREASON: " + reason;
		}

		private static string MessageFromErrorCode(EnumErrorCode code, string info = null)
		{
			StringBuilder sb = new StringBuilder();

			switch (code)
			{
				case EnumErrorCode.ERROR_JSON_INVALID:
					break;

				case EnumErrorCode.ERROR_JSON_PARSE:
					sb.Append(FormatMessage("Unable to parse the json in the repsonse",
											"examine BL or DLC to ensure data format is correct.",
											"If the exception is caused by `Path <somekey>`, then <somekey> might be a different type than is expected (int instead of of string)"));
					break;

				case EnumErrorCode.ERROR_MIC_MISSING_REDIRECT_CODE:
					sb.Append("Redirect does not contain `code=`, was: ");
					break;

				case EnumErrorCode.ERROR_MIC_HOSTNAME_REQUIREMENT_HTTPS:
					sb.Append("MIC Hostname must use the https protocol, trying to set: ");
					break;

				case EnumErrorCode.ERROR_REQUIREMENT_CONTENT_TYPE_HEADER:
					sb.Append("The response expects `Content-Type` header to be \"application/json\", but was instead: ");
					break;

				case EnumErrorCode.ERROR_REQUIREMENT_CUSTOM_REQUEST_PROPERTY_LIMIT:
					sb.Append("Cannot attach more than 2k of Custom Request Properties");
					break;

				case EnumErrorCode.ERROR_REQUIREMENT_HTTPS:
					sb.Append("Kinvey requires the usage of SSL over http.  Use `https` as the protocol when setting a base URL");
					break;

				case EnumErrorCode.ERROR_USER_ALREADY_LOGGED_IN:
					sb.Append(FormatMessage("Attempting to login when a user is already logged in",
											"call `myClient.user().logout().execute() first -or- check `myClient.user().isUserLoggedIn()` before attempting to login again",
											"Only one user can be active at a time, and logging in a new user will replace the current user which might not be intended"));
					break;

				case EnumErrorCode.ERROR_USER_NO_ACTIVE:
					break;

				case EnumErrorCode.ERROR_USER_LOGIN_ATTEMPT:
					sb.Append("Error attempting to log in.");
					break;

				default:
					sb.Append("Unknown error: ");
					break;
			}

			if (!String.IsNullOrEmpty(info))
			{
				sb.Append(info);
			}

			return sb.ToString();
		}
    }
}
