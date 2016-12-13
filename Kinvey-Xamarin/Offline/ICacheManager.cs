﻿// Copyright (c) 2016, Kinvey, Inc. All rights reserved.
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
using SQLite.Net.Interop;

namespace Kinvey
{
	public interface ICacheManager
	{
		/// <summary>
		/// Gets or sets the platform.
		/// </summary>
		/// <value>The platform.</value>
		ISQLitePlatform platform {get; set;}

		/// <summary>
		/// Gets or sets the dbpath.
		/// </summary>
		/// <value>The dbpath.</value>
		string dbpath{ get; set;}

		/// <summary>
		/// Gets the cache.
		/// </summary>
		/// <returns>The cache.</returns>
		/// <param name="collectionName">Collection name.</param>
		ICache<T> GetCache <T>(string collectionName) where T:class;

		ISyncQueue GetSyncQueue (string collectionName);

		/// <summary>
		/// Clears the storage.
		/// </summary>
		void clearStorage();

	}
}