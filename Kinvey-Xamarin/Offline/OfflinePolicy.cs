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
// contents is a violation of applicable laws.using System;

namespace KinveyXamarin
{
	/// <summary>
	/// Defines the policies available for offline behaivor.
	/// </summary>
	public enum OfflinePolicy
	{
		/// <summary>
		/// The device is always online, and will never attempt to use offline.
		/// </summary>
		ALWAYS_ONLINE,
		/// <summary>
		/// The device will first attempt to use the local storage, and if that fails attempt to go online.
		/// </summary>
		LOCAL_FIRST,
		/// <summary>
		/// The device will first attempt to use a network connection, and if that fails will use local storage.
		/// </summary>
		ONLINE_FIRST
	}
}

