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
    /// Class used to capture information about data store operations.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    abstract public class DataStoreResponse<T>
	{
        /// <summary>
        /// The count of results from the data store operation
        /// </summary>
        /// <value><see cref="System.Int32"/> value.</value>
        protected int count;

        /// <summary>
        /// The entities resulting from a data store operation.
        /// </summary>
        /// <value><see cref="List{T}"/> of entities.</value>
        protected List<T> entities;

		List<KinveyException> kinveyExceptions;

		/// <summary>
		/// Gets the datastore exceptions.
		/// </summary>
		/// <value>The kinvey exceptions.</value>
		public List<KinveyException> KinveyExceptions
		{
			get
			{
				return kinveyExceptions;
			}

			private set
			{
				kinveyExceptions = value;
			}
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="DataStoreResponse{T}"/> class.
        /// </summary>
        protected DataStoreResponse()
		{
			count = 0;
			entities = new List<T>();
			kinveyExceptions = new List<KinveyException>();
		}

		internal void AddKinveyException(KinveyException e)
		{
			kinveyExceptions.Add(e);
		}

		internal void AddEntities(List<T> newEntities)
		{
			entities.AddRange(newEntities);
		}

        internal void AddExceptions(ICollection<KinveyException> exceptions)
        {
            kinveyExceptions.AddRange(exceptions);
        }
    }
}
