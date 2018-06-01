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

namespace Kinvey
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

		public NetworkRequest<T> BuildSubscribeRequest<T>(string collectionName, string deviceID)
		{
			const string REST_PATH = "appdata/{appKey}/{collectionName}/_subscribe";

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("collectionName", collectionName);

			NetworkRequest<T> subscribeCollection = new NetworkRequest<T>(client, "POST", REST_PATH, null, urlParameters);

			JObject requestPayload = new JObject();
			requestPayload.Add(Constants.STR_REALTIME_DEVICEID, deviceID);
			subscribeCollection.HttpContent = requestPayload;

			client.InitializeRequest(subscribeCollection);
			return subscribeCollection;
		}

		public NetworkRequest<T> BuildUnsubscribeRequest<T>(string collectionName)
		{
			const string REST_PATH = "appdata/{appKey}/{collectionName}/_unsubscribe";

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("collectionName", collectionName);

			NetworkRequest<T> unsubscribeCollection = new NetworkRequest<T>(client, "POST", REST_PATH, null, urlParameters);
			client.InitializeRequest(unsubscribeCollection);
			return unsubscribeCollection;
		}

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
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("collectionName", collectionName);

			string REST_PATH = "appdata/{appKey}/{collectionName}";

			if (!String.IsNullOrEmpty(queryString))
			{
				REST_PATH = "appdata/{appKey}/{collectionName}?query={querystring}";

				Dictionary<string, string> modifiers = ParseQueryForModifiers(queryString, ref REST_PATH, ref urlParameters);
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
				Dictionary<string, string> modifiers = ParseQueryForModifiers(queryString, ref REST_PATH, ref urlParameters);

			} 

			NetworkRequest<T> getCountQuery = new NetworkRequest<T>(client, "GET", REST_PATH, null, urlParameters);
			client.InitializeRequest(getCountQuery);
			//getCountQuery.customRequestHeaders = this.GetCustomRequestProperties ();
			return getCountQuery;
		}

		public NetworkRequest<T> BuildGetAggregateRequest<T>(string collectionName, EnumReduceFunction reduceFunction, string query, string groupField, string aggregateField)
		{
			string REST_PATH = "appdata/{appKey}/{collectionName}/_group";

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("collectionName", collectionName);

			JObject keyval = new JObject();
			if (!String.IsNullOrEmpty(groupField))
			{
				keyval.Add(groupField, true);
			}

			JObject initialval = new JObject();

			JObject httpBodyContent = new JObject();
			httpBodyContent.Add("key", keyval);

			string reduce = String.Empty;

			switch (reduceFunction)
			{
				case EnumReduceFunction.REDUCE_FUNCTION_SUM:
					initialval.Add("result", 0);
					reduce = $"function(doc,out){{ out.result += doc.{aggregateField}; }}";
					break;

				case EnumReduceFunction.REDUCE_FUNCTION_MIN:
					initialval.Add("result", Int32.MaxValue);
					reduce = $"function(doc,out){{ out.result = Math.min(out.result, doc.{aggregateField}); }}";
					break;

				case EnumReduceFunction.REDUCE_FUNCTION_MAX:
					initialval.Add("result", Int32.MinValue);
					reduce = $"function(doc,out){{ out.result = Math.max(out.result, doc.{aggregateField}); }}";
					break;

				case EnumReduceFunction.REDUCE_FUNCTION_AVERAGE:
					initialval.Add("result", 0);
					initialval.Add("count", 0);
					reduce = $"function(doc,out){{ out.result = (((out.result * out.count) + doc.{aggregateField}) / (out.count += 1)); }}";
					break;

				//case EnumReduceFunction.REDUCE_FUNCTION_COUNT:
				//	initialval.Add("result", 0);
				//	reduce = "";
				//	break;

				default:
					// TODO throw new KinveyException()
					break;
			}

			httpBodyContent.Add("initial", initialval);
			httpBodyContent.Add("reduce", reduce);

			if (!String.IsNullOrEmpty(query))
			{
				const char CHAR_CURLY_BRACE_OPENING = '{';
				const char CHAR_CURLY_BRACE_CLOSING = '}';
				const char CHAR_COLON = ':';
				const char CHAR_DOUBLE_QUOTATION_MARK = '"';

				JObject condition = new JObject();
				query = query.TrimStart(CHAR_CURLY_BRACE_OPENING).TrimEnd(CHAR_CURLY_BRACE_CLOSING);
				string[] cond = query.Split(CHAR_COLON);
				cond[0] = cond[0].TrimStart(CHAR_DOUBLE_QUOTATION_MARK).TrimEnd(CHAR_DOUBLE_QUOTATION_MARK);
				cond[1] = cond[1].TrimStart(CHAR_DOUBLE_QUOTATION_MARK).TrimEnd(CHAR_DOUBLE_QUOTATION_MARK);
				condition.Add(cond[0], cond[1]);

				httpBodyContent.Add("condition", condition);
			}

			NetworkRequest<T> findAggregateQuery = new NetworkRequest<T>(client, "POST", REST_PATH, httpBodyContent, urlParameters);

			client.InitializeRequest(findAggregateQuery);

			return findAggregateQuery;
		}

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

        public NetworkRequest<T> BuildDeltaSetRequest<T>(string collectionName, string lastRequestTime, string query = null)
        {
            string REST_PATH = "appdata/{appKey}/{collectionName}/_deltaset";
            string restPathParams = string.Empty;

            var urlParameters = new Dictionary<string, string>();
            urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
            urlParameters.Add("collectionName", collectionName);

            if (!string.IsNullOrEmpty(lastRequestTime))
            {
                restPathParams += string.IsNullOrEmpty(restPathParams) ? "?" : "&";
                restPathParams += "since={lastRequestTime}";
                urlParameters.Add("lastRequestTime", lastRequestTime);
            }

            if (!string.IsNullOrEmpty(query))
            {
                restPathParams += string.IsNullOrEmpty(restPathParams) ? "?" : "&";
                restPathParams += "query={query}";
                urlParameters.Add("query", query);
            }

            REST_PATH += restPathParams;
            NetworkRequest<T> deltaSet = new NetworkRequest<T>(client, "GET", REST_PATH, null, urlParameters);
            client.InitializeRequest(deltaSet);
            return deltaSet;
        }

        #endregion

		#region Query processing

		private Dictionary<string, string> ParseQueryForModifiers(string queryString, ref string uriTemplate, ref Dictionary<string, string> uriResourceParameters)
		{
			string queryBuilder = "query=" + queryString;

			Dictionary<string, string> decodedQueryMap =
				queryBuilder.Split('&').ToDictionary(c => c.Split('=')[0], c => Uri.UnescapeDataString(c.Split('=')[1]));

			if (decodedQueryMap.ContainsKey("skip"))
			{
				uriTemplate += "&skip={skip}";
				uriResourceParameters.Add("skip", decodedQueryMap["skip"]);
			}

			if (decodedQueryMap.ContainsKey("limit"))
			{
				uriTemplate += "&limit={limit}";
				uriResourceParameters.Add("limit", decodedQueryMap["limit"]);
			}

			if (decodedQueryMap.ContainsKey("sort"))
			{
				uriTemplate += "&sort={sort}";
				uriResourceParameters.Add("sort", decodedQueryMap["sort"]);
			}

			if (decodedQueryMap.ContainsKey("fields"))
			{
				uriTemplate += "&fields={fields}";
				uriResourceParameters.Add("fields", decodedQueryMap["fields"]);
			}

			uriResourceParameters["querystring"] = decodedQueryMap["query"];

			return uriResourceParameters;
		}
	
		#endregion
	}
}
