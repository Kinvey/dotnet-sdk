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

using SQLite;
using Newtonsoft.Json;

namespace Kinvey
{
    /// <summary>
	/// Represents SQLite table for pending write action items. 
	/// </summary>
	public class PendingWriteAction
	{
        /// <summary>
        /// Primary key.
        /// </summary>
        /// <value>The key property gets/sets the value of the int field, _key.</value>
        [PrimaryKey, AutoIncrement]
		public int key { get; set; }

        /// <summary>
        /// Entity identifier.
        /// </summary>
        /// <value>The entityId property gets/sets the value of the string field, _entityId.</value>
        public string entityId { get; set; }

        /// <summary>
        /// State of a pending write action item.
        /// </summary>
        /// <value>The state property gets/sets the value of the string field, _state.</value>
		public string state { get; set; }

        /// <summary>
        /// Collection name.
        /// </summary>
        /// <value>The collection property gets/sets the value of the string field, _collection.</value>
        public string collection { get; set; }

        /// <summary>
        /// Action of a pending write action item.
        /// </summary>
        /// <value>The action property gets/sets the value of the string field, _action.</value>
		public string action { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PendingWriteAction"/> class.
        /// </summary>
		public PendingWriteAction(){}

        /// <summary>
		/// Builds a pending write action item from a network request.
		/// </summary>
		/// <returns> The pending write action item. </returns>
		/// <param name="request"> Network request. </param>
        /// <typeparam name="T">The type of an item.</typeparam>
		public static PendingWriteAction buildFromRequest <T> (NetworkRequest<T> request) {
			PendingWriteAction newAction = new PendingWriteAction ();
			newAction.action = request.RequestMethod;

			if (request.uriResourceParameters.ContainsKey("entityID"))
			{
				newAction.entityId = request.uriResourceParameters["entityID"];
			}

			if (request.uriResourceParameters.ContainsKey("collectionName"))
			{
				newAction.collection = request.uriResourceParameters["collectionName"];
			}

			newAction.state = JsonConvert.SerializeObject (request.customRequestHeaders);

			return newAction;
		}

		public NetworkRequest<T> toNetworkRequest <T>(AbstractClient client){
			return null;
		}
	}
}
