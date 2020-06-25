// Copyright (c) 2019, Kinvey, Inc. All rights reserved.
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kinvey
{
	/// <summary>
	/// Default Kinvey specific headers added to every request.
	/// </summary>
    public class KinveyHeaders : List<KeyValuePair<string, IEnumerable<string>>>
    {
        /// <summary>
        /// The version of the SDK.
        /// </summary>
        /// <value>The string value containing the current version of SDK.</value>
        public static string VERSION = "4.4.0";

		// The kinvey API version.
        internal static readonly string kinveyApiVersion = "4";

		// The user agent.
        static string userAgentKey = "user-agent";
		string userAgent = "xamarin-kinvey-http/" + VERSION;

        static string KinveyDeviceInfo { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KinveyHeaders"/> class.
        /// </summary>
        /// <param name="devicePlatform">Device platform.</param>
        public KinveyHeaders(Constants.DevicePlatform devicePlatform)
        {
            Add(new KeyValuePair<string, IEnumerable<string>>(
                userAgentKey, new List<string> { userAgent }
            ));
            Add(new KeyValuePair<string, IEnumerable<string>>(
                Constants.STR_REQUEST_HEADER_API_VERSION, new List<string> { kinveyApiVersion }
            ));

            JObject deviceInfo = new JObject();

            // Set the X-Kinvey-Device-Info header version
            deviceInfo.Add(Constants.STR_DEVICE_INFO_HEADER_KEY, Constants.STR_DEVICE_INFO_HEADER_VALUE);

            // TODO
            // Set the device model
            deviceInfo.Add(Constants.STR_DEVICE_INFO_MODEL_KEY, string.Empty);

            // Set the device OS and platform
            switch (devicePlatform)
            {
                case Constants.DevicePlatform.Android:
                    deviceInfo.Add(Constants.STR_DEVICE_INFO_OS_KEY, Constants.STR_DEVICE_INFO_OS_VALUE_ANDROID);
                    deviceInfo.Add(Constants.STR_DEVICE_INFO_PLATFORM_KEY, Constants.STR_DEVICE_INFO_PLATFORM_VALUE_ANDROID);
                    break;

                case Constants.DevicePlatform.iOS:
                    deviceInfo.Add(Constants.STR_DEVICE_INFO_OS_KEY, Constants.STR_DEVICE_INFO_OS_VALUE_IOS);
                    deviceInfo.Add(Constants.STR_DEVICE_INFO_PLATFORM_KEY, Constants.STR_DEVICE_INFO_PLATFORM_VALUE_IOS);
                    break;

                case Constants.DevicePlatform.NET:
                    deviceInfo.Add(Constants.STR_DEVICE_INFO_OS_KEY, Constants.STR_DEVICE_INFO_OS_VALUE_WINDOWS);
                    deviceInfo.Add(Constants.STR_DEVICE_INFO_PLATFORM_KEY, Constants.STR_DEVICE_INFO_PLATFORM_VALUE_NET);
                    break;

                case Constants.DevicePlatform.PCL:
                    deviceInfo.Add(Constants.STR_DEVICE_INFO_OS_KEY, Constants.STR_DEVICE_INFO_OS_VALUE_UNKNOWN);
                    deviceInfo.Add(Constants.STR_DEVICE_INFO_PLATFORM_KEY, Constants.STR_DEVICE_INFO_PLATFORM_VALUE_PCL);
                    break;
            }

            // TODO
            // Set the device os version
            deviceInfo.Add(Constants.STR_DEVICE_INFO_OSVERSION_KEY, string.Empty);

            // Set the device platform version (SDK version)
            deviceInfo.Add(Constants.STR_DEVICE_INFO_PLATFORMVERSION_KEY, VERSION);

            // Set the device info header
            KinveyDeviceInfo = JsonConvert.SerializeObject(deviceInfo);

            Add(new KeyValuePair<string, IEnumerable<string>>(
                Constants.STR_REQUEST_HEADER_DEVICE_INFO, new List<string> { KinveyDeviceInfo }
            ));
        }
    }
}
