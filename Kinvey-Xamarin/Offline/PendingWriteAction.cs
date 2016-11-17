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
using SQLite.Net.Attributes;
using Newtonsoft.Json;

namespace Kinvey
{
	public class PendingWriteAction
	{
		[PrimaryKey, AutoIncrement]
		public int key { get; set; }

		public string entityId { get; set; }

		public string state { get; set; }
			
		public string collection { get; set; }

		public string action { get; set; }

		public PendingWriteAction(){}

		public static PendingWriteAction buildFromRequest <T> (NetworkRequest<T> request) {
			PendingWriteAction newAction = new PendingWriteAction ();
			//newAction.collection = request.CollectionName;
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
			//T entity = cache.GetByIdAsync (entityId);
			//NetworkRequest<T> request = new NetworkRequest<T>(client, this.action) 
			return null;
		}
	}
}
