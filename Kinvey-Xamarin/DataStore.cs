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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinveyXamarin;
using System.IO;
using System.Linq.Expressions;
using System.Collections;
using Newtonsoft.Json.Linq;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;
using KinveyUtils;

namespace KinveyXamarin
{
	/// <summary>
	/// Class for managing appData access to the Kinvey backend.
	/// </summary>
	public class DataStore<T> : KinveyQueryable<T>, IObservable<T>  where T:class
	{
		#region Member variables

		private String collectionName;

		//private Type typeof(T);

		private AbstractClient client;

		private ICache<T> cache = null;

		private ISyncQueue syncQueue = null;

		private DataStoreType storeType = DataStoreType.SYNC;

		private JObject customRequestProperties = new JObject();

		private NetworkFactory networkFactory;
		/// <summary>
		/// Sets the custom request properties.
		/// </summary>
		/// <param name="customheaders">Customheaders.</param>
		public void SetCustomRequestProperties(JObject customheaders){
			this.customRequestProperties = customheaders;
		}

		/// <summary>
		/// Sets the custom request property.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public void SetCustomRequestProperty(string key, JObject value){
			if (this.customRequestProperties == null){
				this.customRequestProperties = new JObject();
			}
			this.customRequestProperties.Add (key, value);
		}

		/// <summary>
		/// Gets or sets the name of the collection.
		/// </summary>
		/// <value>The name of the collection.</value>
		public string CollectionName {
			get { return this.collectionName; }
			set { this.collectionName = value; }
		}

		/// <summary>
		/// Gets or sets the type of the current.
		/// </summary>
		/// <value>The type of the current.</value>
		//		public Type CurrentType {
		//			get { return this.typeof(T); }
		//			set { this.typeof(T) = value; }
		//		}

		/// <summary>
		/// Gets or sets the kinvey client.
		/// </summary>
		/// <value>The kinvey client.</value>
		public AbstractClient KinveyClient {
			get { return this.client; }
			set { this.client = value; }
		}


		public void setOffline (ICache<T> cache)
		{

			this.cache = cache;
			//			this.store.dbpath = Path.Combine (((Client)KinveyClient).filePath, "kinveyOffline.sqlite");
			//			this.store.platform = ((Client)KinveyClient).offline_platform;
		}

		/// <summary>
		/// Gets the custom request properties.
		/// </summary>
		/// <returns>The custom request properties.</returns>
		public JObject GetCustomRequestProperties(){
			return this.customRequestProperties;
		}

		#endregion

		private DataStore (DataStoreType type, string collectionName, AbstractClient client = null)
			: base (new KinveyQueryProvider(typeof(KinveyQueryable<T>), QueryParser.CreateDefault(), new KinveyQueryExecutor<T>()), typeof(T))
		{
		//	this.collectionName = typeof(T).FullName;
			this.collectionName = collectionName;

			if (client != null)
			{
				this.client = client;
			}
			else
			{
				this.client = Client.SharedClient;
			}

			this.cache = this.client.CacheManager.GetCache<T> (collectionName);
			this.syncQueue = this.client.CacheManager.GetSyncQueue (collectionName);
			this.storeType = type;
			this.customRequestProperties = this.client.GetCustomRequestProperties();
			this.networkFactory = new NetworkFactory(this.client);
		}

		#region Public interface

		public static DataStore<T> GetInstance(DataStoreType type, string collectionName, AbstractClient client = null)
		{
			// TODO do we need to make this a singleton based on collection, store type and store ID?
			return new DataStore<T> (type, collectionName, client);
		}

		/// <summary>
		/// Get a single entity stored in a Kinvey collection.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="entityId">Entity identifier.</param>
//		public async Task<T> FindByIDAsync(string entityID)
//		{
//			List<string> entityIDs = new List<string>();
//			entityIDs.Add(entityID);
//			FindRequest<T> findByIDsRequest = new FindRequest<T>(client, collectionName, cache, storeType.ReadPolicy, entityIDs, null);
//			List<T> listEntities = await findByIDsRequest.ExecuteAsync();
//			return listEntities.FirstOrDefault();
//		}

		/// <summary>
		/// Get a single entity stored in a Kinvey collection.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="entityId">Entity identifier.</param>
//		internal async Task<List<T>> FindByIDsAsync(List<string> entityIDs)
//		{
//			FindRequest<T> findByIDsRequest = new FindRequest<T>(client, collectionName, cache, storeType.ReadPolicy, entityIDs, null);
//			return await findByIDsRequest.ExecuteAsync();
//		}

		public async Task<List<T>> FindAsync(string queryString){
			return await networkFactory.buildGetRequest <T> (this.CollectionName, queryString).ExecuteAsync ();
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			return new Unsubscriber();
		}

		private class Unsubscriber : IDisposable
		{
			public void Dispose()
			{
			}
		}

		/// <summary>
		/// Find based on query.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="queryObj">Query object, which includes the query to run and the delegates to call back.</param>
		public async Task FindAsync(KinveyObserver<T> queryObj, IQueryable<T> query = null)
		{
			IDisposable u = this.Subscribe (queryObj);
			FindRequest<T> findByQueryRequest = new FindRequest<T> (client, collectionName, cache, storeType.ReadPolicy, queryObj, query, null);
			await findByQueryRequest.ExecuteAsync ();
			u.Dispose ();
		}

		/// <summary>
		/// Find based on query.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="queryObj">Query object, which includes the query to run and the delegates to call back.</param>
		public async Task FindAsync(KinveyObserver<T> queryObj, string entityID)
		{
			IDisposable u = this.Subscribe(queryObj);
			List<string> listIDs = new List<string>();
			if (entityID != null)
			{
				listIDs.Add(entityID);
			}
			FindRequest<T> findByQueryRequest = new FindRequest<T>(client, collectionName, cache, storeType.ReadPolicy, queryObj, null, listIDs);
			await findByQueryRequest.ExecuteAsync();
			u.Dispose();
		}

		/// <summary>
		/// Gets a count of all the entities in a collection
		/// </summary>
		/// <returns>The async task which returns the count.</returns>
//		public async Task<uint> GetCountAsync()
//		{
//			uint count = 0;
//			JObject countObj = await buildGetCountRequest().ExecuteAsync ();
//
//			if (countObj != null)
//			{
//				JToken value = countObj.GetValue("count");
//				count = value.ToObject<uint>();
//			}
//
//			return count;
//		}

		/// <summary>
		/// Save the specified entity to a Kinvey collection.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="entity">the entity to save.</param>
		public async Task<T> SaveAsync(T entity)
		{
			SaveRequest<T> request = new SaveRequest<T>(entity, this.client, this.CollectionName, this.cache, this.syncQueue, this.storeType.WritePolicy);
			return await request.ExecuteAsync();
		}


		/// <summary>
		/// Deletes the entity associated with the provided id
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="entityId">the _id of the entity to delete.</param>
		public async Task<KinveyDeleteResponse> RemoveAsync(string entityID)
		{
			RemoveRequest<T> request = new RemoveRequest<T>(entityID, client, CollectionName, cache, syncQueue, storeType.WritePolicy);
			return await request.ExecuteAsync();
		}

		/// <summary>
		/// Sync the data in this data store
		/// </summary>
		/// <returns>The async.</returns>
		public async Task<DataStoreResponse> SyncAsync()
		{
			DataStoreResponse dsr = default(DataStoreResponse);

			// push
			PushRequest<T> pushRequest = new PushRequest<T>(client, CollectionName, cache, syncQueue, storeType.WritePolicy);
			dsr = await pushRequest.ExecuteAsync();

			// pull
			PullRequest<T> pullRequest = new PullRequest<T>(client, CollectionName, cache, storeType.ReadPolicy);
			await pullRequest.ExecuteAsync();

			return dsr;
		}

		/// <summary>
		/// Gets the count of the number of items in the sync queue.
		/// </summary>
		/// <returns>The sync queue item count.</returns>
		/// <param name="allCollections">[optional] Flag to determine if count should be for all collections.  Default to false.</param>
		public int GetSyncCount(bool allCollections = false)
		{
			return syncQueue.Count(allCollections);
		}

		#endregion

		#region Requests

//		[JsonObject (MemberSerialization.OptIn)]
//		public abstract class GetListRequest<T>:AbstractDataRequest<List<T>>{
//			public ICache<T> Cache { get; set; }
//
//			public GetListRequest (AbstractClient client, string REST_PATH, string collection)
//				: base (client, "GET", REST_PATH, default(T[]), collection){
//
//			}
////			public async override Task<List<T>> ExecuteAsync(){
////				List<T> ret = await this.Cache.GetAsync ();
////				if (ret != null && ret.Count > 0) {
////					//cached data found
////					//return ret;
////					//Cool! 
////				} else {
////				}
////				ret = await base.ExecuteAsync ();
////				this.Cache.SaveAsync (ret);
////
////				return ret;
////			}
//
//		}
//		/// <summary>
//		/// A Get request, which is implemented synchronously
//		/// </summary>
//		[JsonObject (MemberSerialization.OptIn)]
//		public class GetRequest <T> : GetListRequest<T>
//		{
//			private const string REST_PATH = "appdata/{appKey}/{collectionName}/";
//
//			public GetRequest (AbstractClient client, string collection)
//				: base (client, REST_PATH, collection)
//			{
//			}
//
//		}
//
//		/// <summary>
//		/// Get entity request, which is implemented synchronously
//		/// </summary>
//		[JsonObject (MemberSerialization.OptIn)]
//		public class GetEntityRequest <T> : AbstractDataRequest<T>
//		{
//			public ICache<T> Cache { get; set; }
//
//			private const string REST_PATH = "appdata/{appKey}/{collectionName}/{entityId}";
//
//			[JsonProperty]
//			public string EntityId { get; set; }
//
//			public GetEntityRequest (string entityId, AbstractClient client, string collection)
//				: base (client, "GET", REST_PATH, default(T), collection)
//			{
//				this.EntityId = entityId;
//				uriResourceParameters.Add ("entityId", entityId);
//
//			}
//
//		}
//
//		/// <summary>
//		/// Get query request, which is implemented synchronously
//		/// </summary>
//		[JsonObject (MemberSerialization.OptIn)]
//		public class GetQueryRequest <T> : GetListRequest<T>
//		{
//			private const string REST_PATH = "appdata/{appKey}/{collectionName}/?query={querystring}";
//
//			[JsonProperty]
//			public string QueryString { get; set; }
//
//			public GetQueryRequest (string queryString, AbstractClient client, string collection)
//				: base (client, REST_PATH, collection)
//			{
//			
//				string queryBuilder = "query=" + queryString;
//			
//				var decodedQueryMap = queryBuilder.Split('&')
//						.ToDictionary(c => c.Split('=')[0],
//						c => Uri.UnescapeDataString(c.Split('=')[1]));
//			
//				if (decodedQueryMap.ContainsKey("skip")){
//					this.uriTemplate += "&skip={skip}";
//					this.uriResourceParameters.Add("skip", decodedQueryMap["skip"]);
//				}
//				if (decodedQueryMap.ContainsKey("limit")){
//					this.uriTemplate += "&limit={limit}";
//					this.uriResourceParameters.Add("limit", decodedQueryMap["limit"]);		
//				}
//
//				if (decodedQueryMap.ContainsKey("sort")) {
//					this.uriTemplate += "&sort={sort}";
//					this.uriResourceParameters.Add("sort", decodedQueryMap["sort"]);
//				}
//
//				this.QueryString = decodedQueryMap["query"];
//				this.uriResourceParameters["querystring"] = this.QueryString;
//
//			}
//
//
//		}
//
//		/// <summary>
//		/// Get the count request, which is implemented synchronously.
//		/// </summary>
//		[JsonObject (MemberSerialization.OptIn)]
//		public class GetCountRequest : AbstractDataRequest<JObject>
//		{
//			private const string REST_PATH = "appdata/{appKey}/{collectionName}/_count";
//
//			public GetCountRequest(AbstractClient client, string collection)
//				: base(client, "GET", REST_PATH, default(JObject), collection)
//			{
//			}
//		}
//
//		/// <summary>
//		/// Get the count request, which is implemented synchronously.
//		/// </summary>
//		[JsonObject (MemberSerialization.OptIn)]
//		public class GetCountQueryRequest : AbstractDataRequest<JObject>
//		{
//			private const string REST_PATH = "appdata/{appKey}/{collectionName}/_count?query={querystring}";
//
//			[JsonProperty]
//			public string QueryString { get; set; }
//
//			public GetCountQueryRequest(string queryString, AbstractClient client, string collection)
//				: base(client, "GET", REST_PATH, default(JObject), collection)
//			{
//				string queryBuilder = "query=" + queryString;
//
//				var decodedQueryMap = queryBuilder.Split('&')
//					.ToDictionary(c => c.Split('=')[0],
//						c => Uri.UnescapeDataString(c.Split('=')[1]));
//
//				if (decodedQueryMap.ContainsKey("skip")){
//					this.uriTemplate += "&skip={skip}";
//					this.uriResourceParameters.Add("skip", decodedQueryMap["skip"]);
//				}
//				if (decodedQueryMap.ContainsKey("limit")){
//					this.uriTemplate += "&limit={limit}";
//					this.uriResourceParameters.Add("limit", decodedQueryMap["limit"]);
//				}
//
//				if (decodedQueryMap.ContainsKey("sort")) {
//					this.uriTemplate += "&sort={sort}";
//					this.uriResourceParameters.Add("sort", decodedQueryMap["sort"]);
//				}
//
//				this.QueryString = decodedQueryMap["query"];
//				this.uriResourceParameters["querystring"] = this.QueryString;
//			}
//		}
//			
//		/// <summary>
//		/// Delete request, which is implemented synchronously.
//		/// </summary>
//		[JsonObject (MemberSerialization.OptIn)]
//		public class DeleteRequest : AbstractDataRequest<KinveyDeleteResponse>
//		{
//
//			private const string REST_PATH = "appdata/{appKey}/{collectionName}/{entityId}";
//
//			[JsonProperty]
//			public string EntityId { get; set; }
//
//			public DeleteRequest (string entityId, AbstractClient client, string collectionName)
//				: base (client, "DELETE", REST_PATH, default(KinveyDeleteResponse), collectionName)
//			{
//				this.EntityId = entityId;
//				uriResourceParameters.Add ("entityId", entityId);
//			}
//
//		}			

		#endregion
	}
}
