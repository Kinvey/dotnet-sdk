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

using System;

namespace Kinvey
{
	/// <summary>
	/// Store type.
	/// </summary>
	public class DataStoreType
	{
		/// <summary>
		/// Gets the read policy.
		/// </summary>
		/// <value>The read policy.</value>
		public ReadPolicy ReadPolicy { get; }

		/// <summary>
		/// Gets the write policy.
		/// </summary>
		/// <value>The write policy.</value>
		public WritePolicy WritePolicy { get; }

		/// <summary>
		/// The SYNC store.
		/// </summary>
		public static readonly DataStoreType SYNC = new DataStoreType(ReadPolicy.FORCE_LOCAL, WritePolicy.FORCE_LOCAL);

		/// <summary>
		/// The CACHE store.
		/// </summary>
		public static readonly DataStoreType CACHE = new DataStoreType (ReadPolicy.BOTH, WritePolicy.NETWORK_THEN_LOCAL);

		/// <summary>
		/// The NETWORK store.
		/// </summary>
		public static readonly DataStoreType NETWORK = new DataStoreType (ReadPolicy.FORCE_NETWORK, WritePolicy.FORCE_NETWORK);

		DataStoreType (ReadPolicy readPolicy, WritePolicy writePolicy){
			this.ReadPolicy = readPolicy;
			this.WritePolicy = writePolicy;
		}
	}
}
