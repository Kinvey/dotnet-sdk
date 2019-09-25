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

using System.Collections.Generic;

namespace Kinvey
{
    /// <summary>
    /// Class used to capture information about push data store operations.
    /// </summary>
    /// <typeparam name="T">The type of an entity.</typeparam>
    public class PushDataStoreResponse<T> : DataStoreResponse<T>
	{
		/// <summary>
		/// Gets the count of datastore objects returned.
		/// </summary>
		/// <value>The count.</value>
		public int PushCount
		{
			get
			{
				return count;
			}

			internal set
			{
				count = value;
			}
		}

		/// <summary>
		/// Gets the pushed entities.
		/// </summary>
		/// <value>The push entities.</value>
		public List<T> PushEntities
		{
			get
			{
				return entities;
			}

			private set
			{
				entities = value;
			}
		}

        /// <summary>
		/// Sets the response.
		/// </summary>
        /// <param name="response">The instance of <see cref="PushDataStoreResponse{T}"/> class.</param>
        public void SetResponse(PushDataStoreResponse<T> response)
        {
            AddEntities(response.PushEntities);
            AddExceptions(response.KinveyExceptions);
            PushCount += response.PushCount;
        }
	}
}
