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
	public static partial class Constants
	{
		// Const Characters
		public const char CHAR_AMPERSAND = '&';
		public const char CHAR_EQUAL = '=';
		internal const char CHAR_COMMA = ',';
		internal const char CHAR_PERIOD = '.';
		internal const char CHAR_QUOTATION_MARK = '"';
		internal const char MIC_ID_SEPARATOR = CHAR_PERIOD;

		// Const Strings
		public const string STR_AMPERSAND = "&";
		public const string STR_EQUAL = "=";
		internal const string STR_SQUARE_BRACKET_OPEN = "[";
		internal const string STR_SQUARE_BRACKET_CLOSE = "]";
        internal const string STR_HYPHEN = "-";

		// Core Product Strings
		internal const string STR_APP_KEY = "appKey";
		internal const string STR_PATH_CUSTOM_ENDPOINT = "/custom/";
		internal const string STR_PATH_REALTIME_STREAM = "/stream/";

		// Authentication Strings
		public const string STR_ACCESS_TOKEN = "AccessToken";
		public const string STR_AUTH_TOKEN = "AuthToken";
		public const string STR_REFRESH_TOKEN = "RefreshToken";
		public const string STR_REDIRECT_URI = "RedirectUri";
		public const string STR_USERNAME = "UserName";
		public const string STR_ATTRIBUTES = "Attributes";
		public const string STR_USER_KMD = "UserKMD";
		public const string STR_CREDENTIAL = "credential";
        public const string STR_MIC_DEFAULT_VERSION = "v3";
        public const string STR_MIC_REDIRECT_CODE = "code=";
        public const string STR_MIC_REDIRECT_ERROR = "error=";
        public const string STR_MIC_REDIRECT_ERROR_DESCRIPTION = "error_description=";

        // Realtime Strings
		internal const string STR_REALTIME_COLLECTION_CHANNEL_PREPEND = "c-";
		internal const string STR_REALTIME_STREAM_CHANNEL_PREPEND = "s-";
        internal const string STR_REALTIME_USER_CHANNEL_PREPEND = "u-";
		internal const string STR_REALTIME_DEVICEID = "deviceId";
		internal const string STR_REALTIME_SUBSCRIBE_KEY = "subscribeKey";
		internal const string STR_REALTIME_PUBLISH_KEY = "publishKey";
		internal const string STR_REALTIME_CHANNEL_GROUP = "userChannelGroup";
		internal const string STR_REALTIME_STREAM_NAME = "streamName";
		internal const string STR_REALTIME_PUBLISH_SUBSTREAM_CHANNEL_NAME = "substreamChannelName";

        // REST Method Strings
        internal const string STR_REST_METHOD_POST = "POST";
        internal const string STR_REST_METHOD_PUT = "PUT";
        internal const string STR_REST_METHOD_DELETE = "DELETE";

        // Hostname Strings
        internal const string STR_PROTOCOL_HTTPS = "https://";
        internal const string STR_HOSTNAME_API = "baas.kinvey.com";
        internal const string STR_HOSTNAME_AUTH = "auth.kinvey.com";

        // Request Headers
        internal const string STR_REQUEST_HEADER_DEVICE_INFO = "X-Kinvey-Device-Info";
        internal const string STR_REQUEST_HEADER_API_VERSION = "X-Kinvey-API-Version";

        // X-Kinvey-Device-Info Strings
        internal const string STR_DEVICE_INFO_HEADER_KEY = "hv";
        internal const int STR_DEVICE_INFO_HEADER_VALUE = 1;

        internal const string STR_DEVICE_INFO_MODEL_KEY = "md";

        internal const string STR_DEVICE_INFO_OS_KEY = "os";
        internal const string STR_DEVICE_INFO_OS_VALUE_IOS = "iOS";
        internal const string STR_DEVICE_INFO_OS_VALUE_ANDROID = "Android";
        internal const string STR_DEVICE_INFO_OS_VALUE_WINDOWS = "Windows";
        internal const string STR_DEVICE_INFO_OS_VALUE_UNKNOWN = "Unknown";

        internal const string STR_DEVICE_INFO_OSVERSION_KEY = "ov";

        internal const string STR_DEVICE_INFO_PLATFORM_KEY = "sdk";
        internal const string STR_DEVICE_INFO_PLATFORM_VALUE_IOS = "Xamarin";
        internal const string STR_DEVICE_INFO_PLATFORM_VALUE_ANDROID = "Xamarin";
        internal const string STR_DEVICE_INFO_PLATFORM_VALUE_NET = "Dotnet";
        internal const string STR_DEVICE_INFO_PLATFORM_VALUE_PCL = "PCL";
        public enum DevicePlatform { PCL, iOS, Android, NET };

        internal const string STR_DEVICE_INFO_PLATFORMVERSION_KEY = "pv";

        // Request/Response Header Keys
        internal const string STR_HEADER_REQUEST_START_TIME = "x-kinvey-request-start";

        // Query strings
        internal const string STR_QUERY_MODIFIER_SKIP = "&skip=";
        internal const string STR_QUERY_MODIFIER_LIMIT = "&limit=";

        #region Backend Error Strings

        // Server-side Delta Sync Error Strings
        internal const string STR_ERROR_BACKEND_MISSING_CONFIGURATION = "MissingConfiguration";
        internal const string STR_ERROR_BACKEND_RESULT_SET_SIZE_EXCEEDED = "ResultSetSizeExceeded";
        internal const string STR_ERROR_BACKEND_PARAMETER_VALUE_OUT_OF_RANGE = "ParameterValueOutOfRange";

        #endregion

        //Limits
        internal const int NUMBER_LIMIT_OF_ENTITIES = 100;
    }
}
