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
	public class DataStore<T> : KinveyQueryable<T>
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

		private DataStore (DataStoreType type, AbstractClient client) : base (QueryParser.CreateDefault(), new KinveyQueryExecutor<T>(), typeof(T))
		{
			this.collectionName = typeof(T).FullName;
			this.client = client;
			this.storeType = type;
			this.customRequestProperties = client.GetCustomRequestProperties ();
		}

		#region Public interface
		public static DataStore<T> GetInstance(DataStoreType type, AbstractClient client)
		{
			return new DataStore<T> (type, client);
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
			T countObj = await buildGetCountRequest().ExecuteAsync ();
			if (countObj is JObject) {
				JToken value = (countObj as JObject).GetValue("count");
				count = value.ToObject<uint>();
			}
			return count;
		}

		/// <summary>
		/// Save the specified entity to a Kinvey collection.
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="entity">the entity to save.</param>
		public async Task<T> SaveAsync(T entity){
			return await buildSaveRequest (entity).ExecuteAsync ();
		}


		/// <summary>
		/// Deletes the entity associated with the provided id
		/// </summary>
		/// <returns>The async task.</returns>
		/// <param name="entityId">the _id of the entity to delete.</param>
		public async Task<KinveyDeleteResponse> DeleteAsync(string entityId){
			return await buildDeleteRequest (entityId).ExecuteAsync ();
		}
		#endregion

		#region Request Builders

		private GetEntityRequest<T> buildGetByIDRequest (string entityId)
		{
			var urlParameters = new Dictionary<string, string> ();
			urlParameters.Add ("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("collectionName", CollectionName);
			urlParameters.Add ("entityId", entityId);
			GetEntityRequest<T> getEntity = new GetEntityRequest<T> (entityId, typeof(T), client, urlParameters, CollectionName);
			client.InitializeRequest (getEntity);
			getEntity.Cache = this.cache;
			//getEntity.clientAppVersion = this.GetClientAppVersion ();
			getEntity.customRequestHeaders = this.GetCustomRequestProperties ();
			return getEntity;
		}


		private GetRequest<T> buildGetRequest ()
		{
			var urlParameters = new Dictionary<string, string> ();
			urlParameters.Add ("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("collectionName", CollectionName);
			GetRequest<T> get = new GetRequest<T> (client, urlParameters, collectionName);
			client.InitializeRequest (get);
			get.Cache = this.cache;
			//get.clientAppVersion = this.GetClientAppVersion ();
			get.customRequestHeaders = this.GetCustomRequestProperties ();
			return get;
		}

		private GetQueryRequest<T> buildGetRequest (string queryString)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("collectionName", CollectionName);
			urlParameters.Add("querystring", queryString);
		

			GetQueryRequest<T> getQuery = new GetQueryRequest<T>(queryString, typeof(T), client, urlParameters, CollectionName);
			client.InitializeRequest(getQuery);
			//getQuery.SetCache(this.store, storeType.ReadPolicy);
			//getQuery.clientAppVersion = this.GetClientAppVersion();
			getQuery.customRequestHeaders = this.GetCustomRequestProperties();
			return getQuery;
		}

		private GetCountRequest buildGetCountRequest()
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("collectionName", CollectionName);

			GetCountRequest getCount = new GetCountRequest(typeof(T), client, urlParameters, CollectionName);
			client.InitializeRequest(getCount);
			getCount.customRequestHeaders = this.GetCustomRequestProperties ();
			return getCount;
		}

		private GetCountQueryRequest buildGetCountRequest(string queryString)
		{
			var urlParameters = new Dictionary<string, string> ();
			urlParameters.Add ("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("collectionName", CollectionName);
			urlParameters.Add ("querystring", queryString);

			GetCountQueryRequest getCountQuery = new GetCountQueryRequest(queryString, typeof(T), client, urlParameters, CollectionName);
			client.InitializeRequest(getCountQuery);
			getCountQuery.customRequestHeaders = this.GetCustomRequestProperties ();
			return getCountQuery;
		}

		private SaveRequest<T> buildSaveRequest (T entity)
		{
			SaveRequest<T> save;
			var urlParameters = new Dictionary<string, string> ();
			urlParameters.Add ("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("collectionName", CollectionName);

			SaveMode mode;
			JToken idToken = JObject.FromObject (entity) ["_id"];
			string id = null;
			if (idToken != null) {
				id = idToken.ToString ();
			}
			if (id != null && id.Length > 0) {
				mode = SaveMode.PUT;
				urlParameters.Add ("entityId", id);
			} else {
				mode = SaveMode.POST;
			}
				

			save = new SaveRequest<T> (entity, id, typeof(T), mode, client, urlParameters, this.CollectionName);
			//save.SetCache (this.cache, storeType.ReadPolicy);
			//save.Cache = this.cache;
			client.InitializeRequest (save);
			//save.clientAppVersion = this.GetClientAppVersion ();
			save.customRequestHeaders = this.GetCustomRequestProperties ();
			return save;
		}
			

		private DeleteRequest buildDeleteRequest (string entityId)
		{

			var urlParameters = new Dictionary<string, string> ();
			urlParameters.Add ("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("collectionName", CollectionName);
			urlParameters.Add ("entityID", entityId);

			DeleteRequest delete = new DeleteRequest (entityId, typeof(T), client, urlParameters, this.CollectionName);
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
		public abstract class GetListRequest<T>:AbstractKinveyClientRequest<List<T>>{
			public ICache<T> Cache { get; set; }

			public GetListRequest (AbstractClient client, string REST_PATH, Dictionary<string, string> urlParameters)
				: base (client, "GET", REST_PATH, default(T[]), urlParameters){

			}
			
		}
		/// <summary>
		/// A Get request, which is implemented synchronously
		/// </summary>
		[JsonObject (MemberSerialization.OptIn)]
		public class GetRequest <T> : GetListRequest<T>
		{
			private const string REST_PATH = "appdata/{appKey}/{collectionName}/";

			[JsonProperty]
			public string collectionName { get; set; }

			public GetRequest (AbstractClient client, Dictionary<string, string> urlParameters, string collection)
				: base (client, REST_PATH, urlParameters)
			{
				this.collectionName = urlParameters ["collectionName"];
			}


		}

		/// <summary>
		/// Get entity request, which is implemented synchronously
		/// </summary>
		[JsonObject (MemberSerialization.OptIn)]
		public class GetEntityRequest <T> : AbstractKinveyClientRequest<T>
		{
			public ICache<T> Cache { get; set; }

			private const string REST_PATH = "appdata/{appKey}/{collectionName}/{entityId}";

			[JsonProperty]
			public string EntityId { get; set; }

			[JsonProperty]
			public string collectionName;

			public GetEntityRequest (string entityId, Type myClass, AbstractClient client, Dictionary<string, string> urlParameters, string collection)
				: base (client, "GET", REST_PATH, default(T), urlParameters)
			{
				this.collectionName = urlParameters ["collectionName"];
				this.EntityId = entityId;

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

			[JsonProperty]
			public string collectionName;


			public GetQueryRequest (string queryString, Type myClass, AbstractClient client, Dictionary<string, string> urlParameters, string collection)
				: base (client, REST_PATH, urlParameters)
			{
			
				this.collectionName = urlParameters ["collectionName"];
				string queryBuilder = "query=" + urlParameters["querystring"];
			
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
		public class GetCountRequest : AbstractKinveyClientRequest<T>
		{
			private const string REST_PATH = "appdata/{appKey}/{collectionName}/_count";

			[JsonProperty]
			public string collectionName;

			public GetCountRequest(Type myClass, AbstractClient client, Dictionary<string, string> urlParameters, string collection)
				: base(client, "GET", REST_PATH, default(T), urlParameters)
			{
				this.collectionName = urlParameters ["collectionName"];
			}
		}

		/// <summary>
		/// Get the count request, which is implemented synchronously.
		/// </summary>
		[JsonObject (MemberSerialization.OptIn)]
		public class GetCountQueryRequest : AbstractKinveyClientRequest<T>
		{
			private const string REST_PATH = "appdata/{appKey}/{collectionName}/_count?query={querystring}";

			[JsonProperty]
			public string QueryString { get; set; }

			[JsonProperty]
			public string collectionName;

			public GetCountQueryRequest(string queryString, Type myClass, AbstractClient client, Dictionary<string, string> urlParameters, string collection)
				: base(client, "GET", REST_PATH, default(T), urlParameters)
			{
				this.collectionName = urlParameters ["collectionName"];
				string queryBuilder = "query=" + urlParameters["querystring"];

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
		public class SaveRequest <T> : AbstractKinveyClientRequest<T>
		{
			private const string REST_PATH = "appdata/{appKey}/{collectionName}";

			[JsonProperty]
			public string CollectionName { get; set; }

			[JsonProperty]
			public string EntityId { get; set; }


			public SaveRequest (T entity, string entityId, Type myClass, SaveMode update, AbstractClient client, Dictionary<string, string> urlProperties, string collectionName)
				: base (client, update.ToString (), REST_PATH, entity, urlProperties)
			{
				this.CollectionName = urlProperties ["collectionName"];
				if (update.Equals (SaveMode.PUT)) {
					this.EntityId = entityId;
					this.uriTemplate += "/{entityId}";
				}
			}

		}

		/// <summary>
		/// Delete request, which is implemented synchronously.
		/// </summary>
		[JsonObject (MemberSerialization.OptIn)]
		public class DeleteRequest : AbstractKinveyClientRequest<KinveyDeleteResponse>
		{

			private const string REST_PATH = "appdata/{appKey}/{collectionName}/{entityID}";

			[JsonProperty]
			public string CollectionName { get; set; }

			[JsonProperty]
			public string EntityId { get; set; }

			public DeleteRequest (string entityId, Type myClass, AbstractClient client, Dictionary<string, string> urlProperties, string collectionName)
				: base (client, "DELETE", REST_PATH, default(KinveyDeleteResponse), urlProperties)
			{
				this.CollectionName = urlProperties ["collectionName"];
				this.EntityId = entityId;

			}

		}			

		#endregion
	}
}
