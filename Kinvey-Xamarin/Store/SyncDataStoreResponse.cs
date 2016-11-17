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

using System.Collections.Generic;

namespace Kinvey
{
	/// <summary>
	/// Class used to capture information about push data store operations
	/// </summary>
	public class SyncDataStoreResponse<T>
	{
		/// <summary>
		/// Gets the data store response for the push part of the sync operation.
		/// </summary>
		public PushDataStoreResponse<T> PushResponse { get; set; }

		/// <summary>
		/// Gets the data store response for the pull part of the sync operation.
		/// </summary>
		public PullDataStoreResponse<T> PullResponse { get; set; }
	}
}
