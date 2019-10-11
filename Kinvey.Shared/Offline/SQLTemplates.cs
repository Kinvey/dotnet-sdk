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

using System;
using SQLite;
using Newtonsoft.Json.Linq; 

namespace Kinvey
{
    /// <summary>
    /// These are the structures stored in the database, one for each table
    /// </summary>
    [Obsolete("This class has been deprecated.")]
    public class SQLTemplates{

        /// <summary>
        /// This maintains the collection names
        /// </summary>
        [Obsolete("This class has been deprecated.")]
        public class TableItem{
            /// <summary>
            /// Name.
            /// </summary>
            /// <value>The name property gets/sets the value of the string field, _name.</value>
            public string name { get; set;}
		}

        /// <summary>
        /// This maintains the entities themselves.
        /// </summary>
        [Obsolete("This class has been deprecated.")]
        public class OfflineEntity{
            /// <summary>
            /// Identifier.
            /// </summary>
            /// <value>The id property gets/sets the value of the string field, _id.</value>
            [PrimaryKey]
			public string id { get; set;}

            /// <summary>
            /// JSON.
            /// </summary>
            /// <value>The json property gets/sets the value of the string field, _json.</value>
			public string json { get; set;}

            /// <summary>
            /// Collection name.
            /// </summary>
            /// <value>The collection property gets/sets the value of the string field, _collection.</value>
			public string collection { get; set; }
		}

        /// <summary>
        /// This maintains a queue of pending requests.
        /// </summary>
        [Obsolete("This class has been deprecated.")]
        public class QueueItem{
            /// <summary>
            /// Key.
            /// </summary>
            /// <value>The key property gets/sets the value of the int field, _key.</value>
            [PrimaryKey, AutoIncrement] 
			public int key { get; set; }

            /// <summary>
            /// Meta data as json.
            /// </summary>
            /// <value>The OfflineMetaDataAsJson property gets/sets the value of the string field, _offlineMetaDataAsJson.</value>
            public String OfflineMetaDataAsJson { get; set; }

            /// <summary>
            /// Collection name.
            /// </summary>
            /// <value>The collection property gets/sets the value of the string field, _collection.</value>
            public string collection { get; set; }

            /// <summary>
            /// Action.
            /// </summary>
            /// <value>The action property gets/sets the value of the string field, _action.</value>
            public string action { get; set; }
		}

        /// <summary>
        /// This maintains a query and it's responses.
        /// </summary>
        [Obsolete("This class has been deprecated.")]
        public class QueryItem{
            /// <summary>
            /// Key.
            /// </summary>
            /// <value>The key property gets/sets the value of the int field, _key.</value>
			[PrimaryKey, AutoIncrement] 	
			public int key { get; set; }

            /// <summary>
            /// Query.
            /// </summary>
            /// <value>The query property gets/sets the value of the string field, _query.</value>
            public string query { get; set; }

            /// <summary>
            /// Identifiers delimited by comma.
            /// </summary>
            /// <value>The commaDelimitedIds property gets/sets the value of the string field, _commaDelimitedIds.</value>
            public string commaDelimitedIds { get; set; }

            /// <summary>
            /// Collection name.
            /// </summary>
            /// <value>The collection property gets/sets the value of the string field, _collection.</value>
            public string collection { get; set; }
				
		}

        /// <summary>
        /// This maintains the custom request parameters, client app verison, and _id of a queued request
        /// </summary>
        [Obsolete("This class has been deprecated.")]
        public class OfflineMetaData{
            /// <summary>
            /// Identifier.
            /// </summary>
            /// <value>The string value with identifier.</value>
			public string id;

            /// <summary>
            /// Custom headers
            /// </summary>
            /// <value>The JSON object with custom headers.</value>
            public JObject customHeaders;

            /// <summary>
            /// Initializes a new instance of the <see cref="OfflineMetaData"/> class.
            /// </summary>
            /// <param name="id">Identifier.</param>
            /// <param name="customHeaders">Custom headers.</param>
            public OfflineMetaData(string id, JObject customHeaders)
			{
				this.id = id;
				this.customHeaders = customHeaders;
			}

            /// <summary>
            /// Initializes a new instance of the <see cref="OfflineMetaData"/> class.
            /// </summary>
            public OfflineMetaData(){}
		}
	}
}

