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
		public static string VERSION = "3.0.8";

		// The kinvey API version.
        static string kinveyApiVersionKey = "X-Kinvey-API-Version";
        static string kinveyApiVersion = "3";

		// The user agent.
        static string userAgentKey = "user-agent";
		string userAgent = "xamarin-kinvey-http/" + VERSION;

		//// The device info, which includes the OS and OS version, as well as the device model.
		//static string kinveyDeviceInfoKey = "X-Kinvey-Device-Info";
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
        public KinveyHeaders()
        {
			Add(new HttpHeader { Name = userAgentKey, Value = new List<string> { userAgent } });
			Add(new HttpHeader { Name = kinveyApiVersionKey, Value = new List<string> { kinveyApiVersion } });
			//Add(new HttpHeader { Name = kinveyDeviceInfoKey, Value = new List<string> { KinveyDeviceInfo } });
        }
    }
}
