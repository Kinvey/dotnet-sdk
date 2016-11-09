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

namespace KinveyXamarin
{
	/// <summary>
	/// Class used to capture information about data store operations
	/// </summary>
	public class DataStoreResponse<T>
	{
		/// <summary>
		/// Gets the datastore exceptions.
		/// </summary>
		/// <value>The kinvey exceptions.</value>
		public List<KinveyException> KinveyExceptions { get; private set; }

		/// <summary>
		/// Gets or sets the count of datastore objects returned.
		/// </summary>
		/// <value>The count.</value>
		public int Count { get; internal set; }

		public List<T> EntitiesFromPull { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:KinveyXamarin.DataStoreResponse"/> class.
		/// </summary>
		public DataStoreResponse()
		{
			KinveyExceptions = new List<KinveyException>();

			EntitiesFromPull = new List<T>();
		}

		internal void AddKinveyException(KinveyException e)
		{
			KinveyExceptions.Add(e);
		}

		internal void AddEntitiesFromPull(List<T> pulledEntities)
		{
			EntitiesFromPull.AddRange(pulledEntities);
		}
	}
}
