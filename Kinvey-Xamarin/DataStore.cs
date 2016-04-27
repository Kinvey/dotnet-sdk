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
using Remotion.Linq.Parsing.Structure;
using KinveyUtils;

namespace KinveyXamarin
{
	/// <summary>
	/// Class for managing appData access to the Kinvey backend.
	/// </summary>
	public class DataStore<T> : KinveyQueryable<T>  where T:class
	{
		#region Member variables
		/// <summary>
		/// The name of the collection.
		/// </summary>
		private String collectionName;
		/// <summary>
		/// The Type of the class.
		/// </summary>
		//private Type typeof(T);

		private AbstractClient client;

		private ICache<T> cache = null;

		private DataStoreType storeType = DataStoreType.SYNC;

		private JObject customRequestProperties = new JObject();

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

		private DataStore (DataStoreType type, string collectionName, AbstractClient client) : base (QueryParser.CreateDefault(), new KinveyQueryExecutor<T>(), typeof(T))
		{
		//	this.collectionName = typeof(T).FullName;
			this.collectionName = collectionName;
			this.cache = client.CacheManager.GetCache<T> (collectionName);
			this.client = client;
			this.storeType = type;
			this.customRequestProperties = client.GetCustomRequestProperties ();
		}

		#region Public interface
		public static DataStore<T> GetInstance(DataStoreType type, string collectionName, AbstractClient client)
		{
			return new DataStore<T> (type, collectionName, client);
		}

		/// <summary>
		/// Get a single entity stored in a Kinvey collection.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="entityId">Entity identifier.</param>
		public async Task<T> GetEntityAsync(string entityId){
			return await buildGetByIDRequest (entityId).ExecuteAsync ();
		}

		/// <summary>
		/// Get all entities from a Kinvey collection.
		/// </summary>
		/// <returns>The async task.</returns>
		public async Task<List<T>> GetAsync(){
			return await buildGetRequest ().ExecuteAsync ();
		}

		public async Task<List<T>> GetAsync(string queryString){
			return await buildGetRequest (queryString).ExecuteAsync ();
		}


		/// <summary>
		/// Gets a count of all the entities in a collection
		/// </summary>
		/// <returns>The async task which returns the count.</returns>
		public async Task<uint> GetCountAsync()
		{
			uint count = 0;
			JObject countObj = await buildGetCountRequest().ExecuteAsync ();

			if (countObj != null)
			{
				JToken value = countObj.GetValue("count");
				count = value.ToObject<uint>();
			}

			return count;
		}

		/// <summary>
		/// Save the specified entity to a Kinvey collection.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="entity">the entity to save.</param>
		public async Task<T> SaveAsync(T entity)
		{
			// first, build save request
			SaveMode saveMode = SaveMode.PUT;
			SaveRequest<T> saveRequest = buildSaveRequest(entity, ref saveMode);

			// second, if cache is available, save in cache store
			string tempID = null;
			if (SaveMode.POST == saveMode)
			{
				tempID = PrepareCacheSave(ref entity);
			}

			cache.Save(entity);

			// third, save in network store
			T savedEntity = await saveRequest.ExecuteAsync();

			// fourth, update ID in cache if necessary
			if (tempID != null)
			{
				cache.UpdateCacheSave(savedEntity, tempID);
			}

			return savedEntity;
		}


		/// <summary>
		/// Deletes the entity associated with the provided id
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="entityId">the _id of the entity to delete.</param>
		public async Task<KinveyDeleteResponse> DeleteAsync(string entityID)
		{
			// first, build delete request
			DeleteRequest deleteRequest = buildDeleteRequest(entityID);

			// second, delete from cache
			cache.DeleteByIdAsync(entityID);

			// third, delete from network store and return delete response
			return await deleteRequest.ExecuteAsync();
		}

		#endregion

		#region Request Builders

		private GetEntityRequest<T> buildGetByIDRequest (string entityId)
		{
			//var urlParameters = new Dictionary<string, string> ();
			//urlParameters.Add ("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			//urlParameters.Add ("collectionName", CollectionName);
			//urlParameters.Add ("entityId", entityId);
			GetEntityRequest<T> getEntity = new GetEntityRequest<T> (entityId, client, CollectionName);
			client.InitializeRequest (getEntity);
			getEntity.Cache = this.cache;
			//getEntity.clientAppVersion = this.GetClientAppVersion ();
			getEntity.customRequestHeaders = this.GetCustomRequestProperties ();
			return getEntity;
		}


		private GetRequest<T> buildGetRequest ()
		{
			//var urlParameters = new Dictionary<string, string> ();
			//urlParameters.Add ("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			//urlParameters.Add ("collectionName", CollectionName);
			GetRequest<T> get = new GetRequest<T> (client, collectionName);
			client.InitializeRequest (get);
			get.Cache = this.cache;
			//get.clientAppVersion = this.GetClientAppVersion ();
			get.customRequestHeaders = this.GetCustomRequestProperties ();
			return get;
		}

		private GetQueryRequest<T> buildGetRequest (string queryString)
		{
			var urlParameters = new Dictionary<string, string>();
			//urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			//urlParameters.Add("collectionName", CollectionName);
			//urlParameters.Add("querystring", queryString);
		

			GetQueryRequest<T> getQuery = new GetQueryRequest<T>(queryString, client, CollectionName);
			client.InitializeRequest(getQuery);
			//getQuery.SetCache(this.store, storeType.ReadPolicy);
			//getQuery.clientAppVersion = this.GetClientAppVersion();
			getQuery.customRequestHeaders = this.GetCustomRequestProperties();
			return getQuery;
		}

		private GetCountRequest buildGetCountRequest()
		{
			//var urlParameters = new Dictionary<string, string>();
			//urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			//urlParameters.Add("collectionName", CollectionName);

			GetCountRequest getCount = new GetCountRequest(client, CollectionName);
			client.InitializeRequest(getCount);
			getCount.customRequestHeaders = this.GetCustomRequestProperties ();
			return getCount;
		}

		private GetCountQueryRequest buildGetCountRequest(string queryString)
		{
			//var urlParameters = new Dictionary<string, string> ();
			//urlParameters.Add ("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			//urlParameters.Add ("collectionName", CollectionName);
			//urlParameters.Add ("querystring", queryString);

			GetCountQueryRequest getCountQuery = new GetCountQueryRequest(queryString, client, CollectionName);
			client.InitializeRequest(getCountQuery);
			getCountQuery.customRequestHeaders = this.GetCustomRequestProperties ();
			return getCountQuery;
		}

		private SaveRequest<T> buildSaveRequest (T entity, ref SaveMode saveMode)
		{
			SaveRequest<T> save;
			var urlParameters = new Dictionary<string, string> ();
			//urlParameters.Add ("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			//urlParameters.Add ("collectionName", CollectionName);

			SaveMode mode;
			JToken idToken = JObject.FromObject (entity) ["_id"];
			string id = null;
			if (idToken != null) {
				id = idToken.ToString ();
			}
			if (id != null && id.Length > 0) {
				mode = SaveMode.PUT;
				//urlParameters.Add ("entityId", id);
			} else {
				mode = SaveMode.POST;
			}
			saveMode = mode;
			save = new SaveRequest<T> (entity, id, mode, client, this.CollectionName);
			//save.SetCache (this.cache, storeType.ReadPolicy);
			//save.Cache = this.cache;
			client.InitializeRequest (save);
			//save.clientAppVersion = this.GetClientAppVersion ();
			save.customRequestHeaders = this.GetCustomRequestProperties ();
			return save;
		}

		private string PrepareCacheSave(ref T entity)
		{
			string guid = System.Guid.NewGuid().ToString();
			string tempID = "temp_" + guid;

			JObject obj = JObject.FromObject(entity);
			obj["_id"] = tempID;
			entity = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(obj.ToString());

			return tempID;
		}

		private DeleteRequest buildDeleteRequest (string entityId)
		{

			//var urlParameters = new Dictionary<string, string> ();
			//urlParameters.Add ("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			//urlParameters.Add ("collectionName", CollectionName);
			//urlParameters.Add ("entityID", entityId);

			DeleteRequest delete = new DeleteRequest (entityId, client, this.CollectionName);
			//delete.SetCache (this.cache, storeType.ReadPolicy);
			//delete.Cache = this.cache;
			client.InitializeRequest (delete);
			//delete.clientAppVersion = this.GetClientAppVersion ();
			delete.customRequestHeaders = this.GetCustomRequestProperties ();
			return delete;
		}

		#endregion

		/// <summary>
		/// Save mode.
		/// </summary>
		public enum SaveMode
		{
			POST,
			PUT
		}


		#region Requests

		[JsonObject (MemberSerialization.OptIn)]
		public abstract class GetListRequest<T>:AbstractDataRequest<List<T>>{
			public ICache<T> Cache { get; set; }

			public GetListRequest (AbstractClient client, string REST_PATH, string collection)
				: base (client, "GET", REST_PATH, default(T[]), collection){

			}
//			public async override Task<List<T>> ExecuteAsync(){
//				List<T> ret = await this.Cache.GetAsync ();
//				if (ret != null && ret.Count > 0) {
//					//cached data found
//					//return ret;
//					//Cool! 
//				} else {
//				}
//				ret = await base.ExecuteAsync ();
//				this.Cache.SaveAsync (ret);
//
//				return ret;
//			}
		}
		/// <summary>
		/// A Get request, which is implemented synchronously
		/// </summary>
		[JsonObject (MemberSerialization.OptIn)]
		public class GetRequest <T> : GetListRequest<T>
		{
			private const string REST_PATH = "appdata/{appKey}/{collectionName}/";

			public GetRequest (AbstractClient client, string collection)
				: base (client, REST_PATH, collection)
			{
			}

		}

		/// <summary>
		/// Get entity request, which is implemented synchronously
		/// </summary>
		[JsonObject (MemberSerialization.OptIn)]
		public class GetEntityRequest <T> : AbstractDataRequest<T>
		{
			public ICache<T> Cache { get; set; }

			private const string REST_PATH = "appdata/{appKey}/{collectionName}/{entityId}";

			[JsonProperty]
			public string EntityId { get; set; }

			public GetEntityRequest (string entityId, AbstractClient client, string collection)
				: base (client, "GET", REST_PATH, default(T), collection)
			{
				this.EntityId = entityId;
				uriResourceParameters.Add ("entityId", entityId);

			}

		}

		/// <summary>
		/// Get query request, which is implemented synchronously
		/// </summary>
		[JsonObject (MemberSerialization.OptIn)]
		public class GetQueryRequest <T> : GetListRequest<T>
		{
			private const string REST_PATH = "appdata/{appKey}/{collectionName}/?query={querystring}";

			[JsonProperty]
			public string QueryString { get; set; }

			public GetQueryRequest (string queryString, AbstractClient client, string collection)
				: base (client, REST_PATH, collection)
			{
			
				string queryBuilder = "query=" + queryString;
			
				var decodedQueryMap = queryBuilder.Split('&')
						.ToDictionary(c => c.Split('=')[0],
						c => Uri.UnescapeDataString(c.Split('=')[1]));
			
				if (decodedQueryMap.ContainsKey("skip")){
					this.uriTemplate += "&skip={skip}";
					this.uriResourceParameters.Add("skip", decodedQueryMap["skip"]);
				}
				if (decodedQueryMap.ContainsKey("limit")){
					this.uriTemplate += "&limit={limit}";
					this.uriResourceParameters.Add("limit", decodedQueryMap["limit"]);		
				}

				if (decodedQueryMap.ContainsKey("sort")) {
					this.uriTemplate += "&sort={sort}";
					this.uriResourceParameters.Add("sort", decodedQueryMap["sort"]);
				}

				this.QueryString = decodedQueryMap["query"];
				this.uriResourceParameters["querystring"] = this.QueryString;

			}


		}

		/// <summary>
		/// Get the count request, which is implemented synchronously.
		/// </summary>
		[JsonObject (MemberSerialization.OptIn)]
		public class GetCountRequest : AbstractDataRequest<JObject>
		{
			private const string REST_PATH = "appdata/{appKey}/{collectionName}/_count";

			public GetCountRequest(AbstractClient client, string collection)
				: base(client, "GET", REST_PATH, default(JObject), collection)
			{
			}
		}

		/// <summary>
		/// Get the count request, which is implemented synchronously.
		/// </summary>
		[JsonObject (MemberSerialization.OptIn)]
		public class GetCountQueryRequest : AbstractDataRequest<JObject>
		{
			private const string REST_PATH = "appdata/{appKey}/{collectionName}/_count?query={querystring}";

			[JsonProperty]
			public string QueryString { get; set; }

			public GetCountQueryRequest(string queryString, AbstractClient client, string collection)
				: base(client, "GET", REST_PATH, default(JObject), collection)
			{
				string queryBuilder = "query=" + queryString;

				var decodedQueryMap = queryBuilder.Split('&')
					.ToDictionary(c => c.Split('=')[0],
						c => Uri.UnescapeDataString(c.Split('=')[1]));

				if (decodedQueryMap.ContainsKey("skip")){
					this.uriTemplate += "&skip={skip}";
					this.uriResourceParameters.Add("skip", decodedQueryMap["skip"]);
				}
				if (decodedQueryMap.ContainsKey("limit")){
					this.uriTemplate += "&limit={limit}";
					this.uriResourceParameters.Add("limit", decodedQueryMap["limit"]);
				}

				if (decodedQueryMap.ContainsKey("sort")) {
					this.uriTemplate += "&sort={sort}";
					this.uriResourceParameters.Add("sort", decodedQueryMap["sort"]);
				}

				this.QueryString = decodedQueryMap["query"];
				this.uriResourceParameters["querystring"] = this.QueryString;
			}
		}

		/// <summary>
		/// Save request, which is implemented synchronously.
		/// </summary>
		[JsonObject (MemberSerialization.OptIn)]
		public class SaveRequest <T> : AbstractDataRequest<T>
		{
			private const string REST_PATH = "appdata/{appKey}/{collectionName}";

			[JsonProperty]
			public string EntityId { get; set; }

			public SaveRequest(T entity, string entityId, SaveMode update, AbstractClient client, string collectionName)
				: base (client, update.ToString(), REST_PATH, entity, collectionName)
			{
				if (update.Equals(SaveMode.PUT))
				{
					this.EntityId = entityId;
					this.uriTemplate += "/{entityId}";
					uriResourceParameters.Add ("entityId", entityId);
				}
			}
		}

		/// <summary>
		/// Delete request, which is implemented synchronously.
		/// </summary>
		[JsonObject (MemberSerialization.OptIn)]
		public class DeleteRequest : AbstractDataRequest<KinveyDeleteResponse>
		{

			private const string REST_PATH = "appdata/{appKey}/{collectionName}/{entityID}";

			[JsonProperty]
			public string EntityId { get; set; }

			public DeleteRequest (string entityId, AbstractClient client, string collectionName)
				: base (client, "DELETE", REST_PATH, default(KinveyDeleteResponse), collectionName)
			{
				this.EntityId = entityId;
				uriResourceParameters.Add ("entityID", entityId);
			}

		}			


		public abstract class AbstractDataRequest<T> : AbstractKinveyClientRequest<T>{
			[JsonProperty]
			public string CollectionName { get; set; }

			public AbstractDataRequest (AbstractClient client, string method, string template, Object httpContent, string collection): base(client, method, template, httpContent, new Dictionary<string, string>()){
				this.CollectionName = collection;
				uriResourceParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
				uriResourceParameters.Add("collectionName", collection);
			}
		}
		#endregion
	}
}
