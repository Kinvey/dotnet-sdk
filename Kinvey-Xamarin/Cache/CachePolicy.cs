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

namespace KinveyXamarin
{
	/// <summary>
	/// Cache policies, used for Caching.
	/// </summary>
	public enum CachePolicy
	{
		/// <summary>
		/// Executes the request online every single time.
		/// </summary>
		NO_CACHE,
		/// <summary>
		/// Executes the request from the cache every single time.
		/// </summary>
		CACHE_ONLY,
		/// <summary>
		/// Attempts to get the response from the cache, if it's not present attempts to execute online.  If online is successful, the response will cached.
		/// </summary>
		CACHE_FIRST,
		/// <summary>
		/// Attempts to get the response from the cache, if it's not present attempts to execute online.  If online is successful, the response will not be cached.
		/// </summary>
		CACHE_FIRST_NOREFRESH,
		/// <summary>
		/// Attempts to get the response from the network, if that fails attempts to get the response from the cache.
		/// </summary>
		NETWORK_FIRST
	}
}

