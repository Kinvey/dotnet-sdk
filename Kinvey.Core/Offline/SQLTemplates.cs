// Copyright (c) 2015, Kinvey, Inc. All rights reserved.
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
using System.Collections.Generic;
using SQLite;
using Newtonsoft.Json.Linq; 

namespace Kinvey
{
	/// <summary>
	/// These are the structures stored in the database, one for each table
	/// </summary>
	public class SQLTemplates{

		/// <summary>
		/// This maintains the collection names
		/// </summary>
		public class TableItem{
			public string name { get; set;}
		}

		/// <summary>
		/// This maintains the entities themselves.
		/// </summary>
		public class OfflineEntity{
			[PrimaryKey]
			public string id { get; set;}
			public string json { get; set;}
			public string collection { get; set; }
		}

		/// <summary>
		/// This maintains a queue of pending requests.
		/// </summary>
		public class QueueItem{
			[PrimaryKey, AutoIncrement] 
			public int key { get; set; } 
			public String OfflineMetaDataAsJson { get; set; }
			public string collection { get; set; }
			public string action { get; set; }

		}

		/// <summary>
		/// This maintains a query and it's responses.
		/// </summary>
		public class QueryItem{
			[PrimaryKey, AutoIncrement] 	
			public int key { get; set; } 
			public string query { get; set; }
			public string commaDelimitedIds { get; set; }
			public string collection { get; set; }
				
		}

		/// <summary>
		/// This maintains the custom request parameters, client app verison, and _id of a queued request
		/// </summary>
		public class OfflineMetaData{
			public string id;
			public JObject customHeaders;
			//public string clientVersion;

			//public OfflineMetaData(string id, JObject customHeaders, string clientVersion){
			public OfflineMetaData(string id, JObject customHeaders)
			{
				this.id = id;
				this.customHeaders = customHeaders;
				//this.clientVersion = clientVersion;
			}

			public OfflineMetaData(){}
		}
	}
}

