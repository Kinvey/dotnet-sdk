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
using RestSharp;
using Newtonsoft.Json;

namespace Kinvey
{
	/// <summary>
	/// Default Kinvey specific headers added to every request.
	/// </summary>
    public class KinveyHeaders : List<HttpHeader>
    {
		/// <summary>
		/// The version of the SDK.
		/// </summary>
		public static string VERSION = "3.1.2";

		// The kinvey API version.
        static string kinveyApiVersionKey = "X-Kinvey-API-Version";
        static string kinveyApiVersion = "4";

		// The user agent.
        static string userAgentKey = "user-agent";
		string userAgent = "xamarin-kinvey-http/" + VERSION;

		//// The device info, which includes the OS and OS version, as well as the device model.
        //static string kinveyDeviceInfoKey = "X-Kinvey-Device-Info";
        static string KinveyDeviceInfo { get; set; }

		//static string _kinveyDeviceInfo = null;
		//static string KinveyDeviceInfo
		//{
		//	get
		//	{
		//		if (_kinveyDeviceInfo == null)
		//		{
		//			try
		//			{
		//				_kinveyDeviceInfo = Plugin.DeviceInfo.CrossDeviceInfo.Current?.Platform + " " +
		//									Plugin.DeviceInfo.CrossDeviceInfo.Current?.Version + " " +
		//									Plugin.DeviceInfo.CrossDeviceInfo.Current?.Model;
		//			}
		//			catch (Exception e)
		//			{
		//				_kinveyDeviceInfo = String.Empty;
		//			}
		//		}

		//		return _kinveyDeviceInfo;
		//	}
		//}

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyHeaders"/> class.
		/// </summary>
        public KinveyHeaders(Constants.DevicePlatform devicePlatform)
        {
			Add(new HttpHeader { Name = userAgentKey, Value = new List<string> { userAgent } });
			Add(new HttpHeader { Name = kinveyApiVersionKey, Value = new List<string> { kinveyApiVersion } });

            JsonObject deviceInfo = new JsonObject();

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
            KinveyDeviceInfo = deviceInfo.ToString();

            Add(new HttpHeader { Name = Constants.STR_REQUEST_HEADER_DEVICE_INFO, Value = new List<string> { KinveyDeviceInfo } });
        }
    }
}
