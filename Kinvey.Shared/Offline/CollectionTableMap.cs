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

namespace Kinvey
{
    /// <summary>
	/// Represents SQLite table for collection mapping. 
	/// </summary>
	public class CollectionTableMap
	{
        /// <summary>
        /// Collection name.
        /// </summary>
        /// <value>The CollectionName property gets/sets the value of the sting field, _collectionName.</value>
        [PrimaryKey]
		public string CollectionName { get; set; }

        /// <summary>
        /// Table name.
        /// </summary>
        /// <value>The TableName property gets/sets the value of the sting field, _tableName.</value>
        public string TableName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionTableMap"/> class.
        /// </summary>
        public CollectionTableMap () { }
	}
}
