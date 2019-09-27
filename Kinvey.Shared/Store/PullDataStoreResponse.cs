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
    /// Class used to capture information about pull data store operations.
    /// </summary>
    /// <typeparam name="T">The type of an entity.</typeparam>
    public class PullDataStoreResponse<T> : DataStoreResponse<T>
	{
		internal PullDataStoreResponse() { }

		internal PullDataStoreResponse(int total, int pulled, List<T> pullEntities) {
			this.TotalCount = total;
			this.PullCount = pulled;
			this.PullEntities = pullEntities;
		}
        /// <summary>
        /// Gets or sets the total count of entities that match the request.
        /// This number will be equal to the PullCount, unless delta set caching is in use.
        /// When delta set caching is used, the number of entities retrieved from the backend (PullCount) is typically less than the total number of entities that match the request (TotalCount).
        /// </summary>
        /// <value>The total count of entities that match the request.</value>
        public int TotalCount;

		/// <summary>
		/// Gets the count of datastore objects returned.
		/// </summary>
		/// <value>The count of entities retrieved from the backend.</value>
		public int PullCount
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
		/// Gets the pulled entities.
		/// </summary>
		/// <value>The pull entities.</value>
		public List<T> PullEntities
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
	}
}
