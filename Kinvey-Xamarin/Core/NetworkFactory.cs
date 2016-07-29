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
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;

namespace KinveyXamarin
{
	public class NetworkFactory
	{
		public AbstractClient client  { get;}

		#region Global request configs such as timeouts and hostname go here

		#endregion

		public NetworkFactory (AbstractClient client)
		{
			this.client = client;
		}

		#region Request Builders

		public NetworkRequest<T> buildGetByIDRequest <T> (string collectionName, string entityID)
		{
			const string REST_PATH = "appdata/{appKey}/{collectionName}/{entityID}";

			var urlParameters = new Dictionary<string, string> ();
			urlParameters.Add ("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("collectionName", collectionName);
			urlParameters.Add ("entityID", entityID);

			NetworkRequest<T> getEntity = new NetworkRequest<T> (client, "GET", REST_PATH, null, urlParameters);
			client.InitializeRequest (getEntity);
			//getEntity.Cache = this.cache;
			//getEntity.clientAppVersion = this.GetClientAppVersion ();
			//getEntity.customRequestHeaders = this.GetCustomRequestProperties ();
			return getEntity;
		}

		public NetworkRequest<List<T>> buildGetRequest <T> (string collectionName, string queryString = null)
		{
			var urlParameters = new Dictionary<string, string> ();
			urlParameters.Add ("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("collectionName", collectionName);

			string REST_PATH = "appdata/{appKey}/{collectionName}";

			if (!string.IsNullOrEmpty (queryString)) {
				REST_PATH = "appdata/{appKey}/{collectionName}?query={querystring}";
				urlParameters.Add ("querystring", queryString);
			}

			NetworkRequest<List<T>> getQuery = new NetworkRequest<List<T>> (client, "GET", REST_PATH, null, urlParameters);
			client.InitializeRequest(getQuery);
			//getQuery.SetCache(this.store, storeType.ReadPolicy);
			//getQuery.clientAppVersion = this.GetClientAppVersion();
			//getQuery.customRequestHeaders = this.GetCustomRequestProperties();
			return getQuery;
		}

		public NetworkRequest<T> buildGetCountRequest <T> (string collectionName, string queryString = null)
		{
			string REST_PATH = "appdata/{appKey}/{collectionName}/_count";

			var urlParameters = new Dictionary<string, string> ();
			urlParameters.Add ("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("collectionName", collectionName);

			if (!string.IsNullOrEmpty (queryString)) { 
				REST_PATH = "appdata/{appKey}/{collectionName}/_count?query={querystring}";
				urlParameters.Add ("querystring", queryString);
			}

			NetworkRequest<T> getCountQuery = new NetworkRequest<T>(client, "GET", REST_PATH, null, urlParameters);
			client.InitializeRequest(getCountQuery);
			//getCountQuery.customRequestHeaders = this.GetCustomRequestProperties ();
			return getCountQuery;
		}

//		public NetworkRequest<T> buildGetCountQueryRequest <T> (string collectionName, string queryString){
//			const string REST_PATH = "appdata/{appKey}/{collectionName}/_count?query={querystring}";
//		
//		
//		}


		public NetworkRequest<T> buildCreateRequest <T> (string collectionName, T entity)
		{
			const string REST_PATH = "appdata/{appKey}/{collectionName}";

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("collectionName", collectionName);

			NetworkRequest<T> create = new NetworkRequest<T> (client, "POST", REST_PATH, entity, urlParameters);
			client.InitializeRequest(create);
			return create;
		}

		public NetworkRequest<T> buildUpdateRequest <T> (string collectionName, T entity, string entityID)
		{
			const string REST_PATH = "appdata/{appKey}/{collectionName}/{entityID}";

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("collectionName", collectionName);
			urlParameters.Add("entityID", entityID);

			NetworkRequest<T> update = new NetworkRequest<T> (client, "PUT", REST_PATH, entity, urlParameters);
			client.InitializeRequest(update);
			return update;
		}

		public NetworkRequest<T> buildDeleteRequest <T>(string collectionName, string entityID)
		{	
			const string REST_PATH = "appdata/{appKey}/{collectionName}/{entityID}";

			var urlParameters = new Dictionary<string, string> ();
			urlParameters.Add ("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("collectionName", collectionName);
			urlParameters.Add ("entityID", entityID);

			NetworkRequest<T> delete = new NetworkRequest<T> (client, "DELETE", REST_PATH, null, urlParameters);
			//delete.SetCache (this.cache, storeType.ReadPolicy);
			//delete.Cache = this.cache;
			client.InitializeRequest (delete);
			//delete.clientAppVersion = this.GetClientAppVersion ();
			//delete.customRequestHeaders = this.GetCustomRequestProperties ();
			return delete;
		}

		#endregion

		#region Query processing
		private Dictionary<string, string> decodeQuery(string queryString){
			string queryBuilder = "query=" + queryString;
			string uriTemplate = "";
			Dictionary<string, string> uriResourceParameters = new Dictionary<string, string>();

			Dictionary<string, string> decodedQueryMap = queryBuilder.Split('&')
				.ToDictionary(c => c.Split('=')[0],
					c => Uri.UnescapeDataString(c.Split('=')[1]));

			if (decodedQueryMap.ContainsKey("skip")){
				uriTemplate += "&skip={skip}";
				uriResourceParameters.Add("skip", decodedQueryMap["skip"]);
			}
			if (decodedQueryMap.ContainsKey("limit")){
				uriTemplate += "&limit={limit}";
				uriResourceParameters.Add("limit", decodedQueryMap["limit"]);
			}

			if (decodedQueryMap.ContainsKey("sort")) {
				uriTemplate += "&sort={sort}";
				uriResourceParameters.Add("sort", decodedQueryMap["sort"]);
			}

			uriResourceParameters["querystring"] = decodedQueryMap["query"];

			return uriResourceParameters;

		}
	
		#endregion
	}
}
