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
	public static class Constants
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

        // Request/Response Header Keys
        internal const string STR_HEADER_REQUEST_START_TIME = "x-kinvey-request-start";
	}
}
