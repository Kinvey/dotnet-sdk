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
using LinqExtender;
using Ast = LinqExtender.Ast;
using Newtonsoft.Json.Linq;
using Remotion.Linq.Parsing.Structure;
using KinveyUtils;

namespace KinveyXamarin
{
	/// <summary>
	/// Class for managing appData access to the Kinvey backend.
	/// </summary>
	public class AppData<T> : KinveyQueryable<T>
	{
		/// <summary>
		/// The name of the collection.
		/// </summary>
		private String collectionName;
		/// <summary>
		/// The Type of the class.
		/// </summary>
		private Type myClass;

		/// <summary>
		/// The client.
		/// </summary>
		protected AbstractClient client;

		/// <summary>
		/// The cache.
		/// </summary>
		private Cache<String, T> cache = null;
		/// <summary>
		/// The query cache.
		/// </summary>
		private Cache<String, T[]> queryCache = null;
		/// <summary>
		/// The cache policy.
		/// </summary>
		private CachePolicy cachePolicy = CachePolicy.NO_CACHE;

		/// <summary>
		/// The Offline Store
		/// </summary>
		private IOfflineStore store = null;
		/// <summary>
		/// The offline policy.
		/// </summary>
		private OfflinePolicy offlinePolicy = OfflinePolicy.ALWAYS_ONLINE;

		/// <summary>
		/// The name of the identifier field.
		/// </summary>
		public const string IdFieldName = "_id";


		private string clientAppVersion = null;

		private JObject customRequestProperties = new JObject();

		public void SetClientAppVersion(string appVersion){
			this.clientAppVersion = appVersion;	
		}

		public void SetClientAppVersion(int major, int minor, int revision){
			SetClientAppVersion(major + "." + minor + "." + revision);
		}

		public string GetClientAppVersion(){
			return this.clientAppVersion;
		}

		public void SetCustomRequestProperties(JObject customheaders){
			this.customRequestProperties = customheaders;
		}

		public void SetCustomRequestProperty(string key, JObject value){
			if (this.customRequestProperties == null){
				this.customRequestProperties = new JObject();
			}
			this.customRequestProperties.Add (key, value);
		}

		public void ClearCustomRequestProperties(){
			this.customRequestProperties = new JObject();
		}

		public JObject GetCustomRequestProperties(){
			return this.customRequestProperties;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Kinvey.DotNet.Framework.Core.AppData`1"/> class.
		/// </summary>
		/// <param name="collectionName">Collection name.</param>
		/// <param name="myClass">My class.</param>
		/// <param name="client">Client.</param>
		public AppData (string collectionName, Type myClass, AbstractClient client) : base (QueryParser.CreateDefault(), new KinveyQueryExecutor<T>(), myClass)
		{
			this.collectionName = collectionName;
			this.myClass = myClass;
			this.client = client;
			this.customRequestProperties = client.GetCustomRequestProperties ();
			this.clientAppVersion = client.GetClientAppVersion ();
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
		public Type CurrentType {
			get { return this.myClass; }
			set { this.myClass = value; }
		}

		/// <summary>
		/// Gets or sets the kinvey client.
		/// </summary>
		/// <value>The kinvey client.</value>
		public AbstractClient KinveyClient {
			get { return this.client; }
			set { this.client = value; }
		}

		/// <summary>
		/// Sets the cache.
		/// </summary>
		/// <param name="cache">Cache.</param>
		/// <param name="policy">Policy.</param>
		public void setCache (Cache<String, T> cache, CachePolicy policy)
		{
			this.cache = cache;
			this.cachePolicy = policy;
		}

		/// <summary>
		/// Sets the cache for query requests
		/// </summary>
		/// <param name="cache">Cache.</param>
		/// <param name="policy">Policy.</param>
		public void setCache (Cache<String, T[]> cache, CachePolicy policy)
		{
			this.queryCache = cache;
			this.cachePolicy = policy;
		}

		/// <summary>
		/// Sets the offline store and policy
		/// </summary>
		/// <param name="store">Store.</param>
		/// <param name="policy">Policy.</param>
		public void setOffline (IOfflineStore store, OfflinePolicy policy)
		{

			this.store = store;
			this.offlinePolicy = policy;

			this.store.dbpath = Path.Combine (((Client)KinveyClient).filePath, "kinveyOffline.sqlite");
			this.store.platform = ((Client)KinveyClient).offline_platform;

		}
			
		/// <summary>
		/// gets the specified entity.
		/// </summary>
		/// <returns>The request, ready to execute..</returns>
		/// <param name="entityId">Entity's _id.</param>
		public GetEntityRequest GetEntityBlocking (string entityId)
		{
			var urlParameters = new Dictionary<string, string> ();
			urlParameters.Add ("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("collectionName", CollectionName);
			urlParameters.Add ("entityId", entityId);
			GetEntityRequest getEntity = new GetEntityRequest (entityId, myClass, client, urlParameters, CollectionName);
			client.InitializeRequest (getEntity);
			getEntity.setCache (this.cache, this.cachePolicy);
			getEntity.SetStore (this.store, this.offlinePolicy);
			getEntity.clientAppVersion = this.GetClientAppVersion ();
			getEntity.customRequestHeaders = this.GetCustomRequestProperties ();
			return getEntity;
		}

		/// <summary>
		/// gets all entities
		/// </summary>
		/// <returns>The blocking.</returns>
		public GetRequest GetBlocking ()
		{
			var urlParameters = new Dictionary<string, string> ();
			urlParameters.Add ("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("collectionName", CollectionName);
			GetRequest get = new GetRequest (myClass, client, urlParameters, collectionName);
			client.InitializeRequest (get);
			get.setCache (this.queryCache, this.cachePolicy);
			get.SetStore (this.store, this.offlinePolicy);
			get.clientAppVersion = this.GetClientAppVersion ();
			get.customRequestHeaders = this.GetCustomRequestProperties ();
			return get;
		}

		/// <summary>
		///gets the specified query string
		/// </summary>
		/// <returns>The query blocking.</returns>
		/// <param name="queryString">Query string.</param>
		public GetQueryRequest getQueryBlocking (string queryString)
		{
			var urlParameters = new Dictionary<string, string> ();
			urlParameters.Add ("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("collectionName", CollectionName);
			urlParameters.Add ("querystring", queryString);
		

			GetQueryRequest getQuery = new GetQueryRequest (queryString, myClass, client, urlParameters, CollectionName);
			client.InitializeRequest (getQuery);
			getQuery.setCache (this.queryCache, this.cachePolicy);
			getQuery.SetStore (this.store, this.offlinePolicy);
			getQuery.clientAppVersion = this.GetClientAppVersion ();
			getQuery.customRequestHeaders = this.GetCustomRequestProperties ();
			return getQuery;
		}

		/// <summary>
		/// saves the specified entity
		/// </summary>
		/// <returns>The blocking.</returns>
		/// <param name="entity">Entity.</param>
		public SaveRequest SaveBlocking (T entity)
		{
			SaveRequest save;
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
				

			save = new SaveRequest (entity, id, myClass, mode, client, urlParameters, this.CollectionName);
			save.SetStore (this.store, this.offlinePolicy);
			client.InitializeRequest (save);
			save.clientAppVersion = this.GetClientAppVersion ();
			save.customRequestHeaders = this.GetCustomRequestProperties ();
			return save;
		}

		/// <summary>
		/// Deletes the specified entity
		/// </summary>
		/// <returns>The blocking.</returns>
		/// <param name="entityId">Entity _id.</param>
		public DeleteRequest DeleteBlocking (string entityId)
		{

			var urlParameters = new Dictionary<string, string> ();
			urlParameters.Add ("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("collectionName", CollectionName);
			urlParameters.Add ("entityID", entityId);

			DeleteRequest delete = new DeleteRequest (entityId, myClass, client, urlParameters, this.CollectionName);
			delete.SetStore (this.store, this.offlinePolicy);

			client.InitializeRequest (delete);
			delete.clientAppVersion = this.GetClientAppVersion ();
			delete.customRequestHeaders = this.GetCustomRequestProperties ();
			return delete;
		}



		/// <summary>
		/// Save mode.
		/// </summary>
		public enum SaveMode
		{
			POST,
			PUT
		}

		/// <summary>
		/// Executes the query.
		/// </summary>
		/// <returns>The query.</returns>
		/// <param name="query">the results of the query, executed synchronously.</param>
		public override object executeQuery (string query)
		{
			return getQueryBlocking (query).Execute ();
		}


		/// <summary>
		/// A Get request, which is implemented synchronously
		/// </summary>
		[JsonObject (MemberSerialization.OptIn)]
		public class GetRequest : AbstractKinveyCachedClientRequest<T[]>
		{
			private const string REST_PATH = "appdata/{appKey}/{collectionName}/";

			[JsonProperty]
			public string collectionName { get; set; }

			public GetRequest (Type myClass, AbstractClient client, Dictionary<string, string> urlParameters, string collection)
				: base (client, "GET", REST_PATH, default(T[]), urlParameters, collection)
			{
				this.collectionName = urlParameters ["collectionName"];
			}


		}

		/// <summary>
		/// Get entity request, which is implemented synchronously
		/// </summary>
		[JsonObject (MemberSerialization.OptIn)]
		public class GetEntityRequest : AbstractKinveyCachedClientRequest<T>
		{
			private const string REST_PATH = "appdata/{appKey}/{collectionName}/{entityId}";

			[JsonProperty]
			public string EntityId { get; set; }

			[JsonProperty]
			public string collectionName;

			public GetEntityRequest (string entityId, Type myClass, AbstractClient client, Dictionary<string, string> urlParameters, string collection)
				: base (client, "GET", REST_PATH, default(T), urlParameters, collection)
			{
				this.collectionName = urlParameters ["collectionName"];
				this.EntityId = entityId;

			}



		}

		/// <summary>
		/// Get query request, which is implemented synchronously
		/// </summary>
		[JsonObject (MemberSerialization.OptIn)]
		public class GetQueryRequest : AbstractKinveyCachedClientRequest<T[]>
		{
			private const string REST_PATH = "appdata/{appKey}/{collectionName}/?query={querystring}";

			[JsonProperty]
			public string QueryString { get; set; }

			[JsonProperty]
			public string collectionName;


			public GetQueryRequest (string queryString, Type myClass, AbstractClient client, Dictionary<string, string> urlParameters, string collection)
				: base (client, "GET", REST_PATH, default(T[]), urlParameters, collection)
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

				this.QueryString = decodedQueryMap["query"];
				this.uriResourceParameters["querystring"] = this.QueryString;

			}


		}

		/// <summary>
		/// Save request, which is implemented synchronously.
		/// </summary>
		[JsonObject (MemberSerialization.OptIn)]
		public class SaveRequest : AbstractKinveyOfflineClientRequest<T>
		{
			private const string REST_PATH = "appdata/{appKey}/{collectionName}";

			[JsonProperty]
			public string CollectionName { get; set; }

			[JsonProperty]
			public string EntityId { get; set; }


			public SaveRequest (T entity, string entityId, Type myClass, SaveMode update, AbstractClient client, Dictionary<string, string> urlProperties, string collectionName)
				: base (client, update.ToString (), REST_PATH, entity, urlProperties, collectionName)
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
		public class DeleteRequest : AbstractKinveyOfflineClientRequest<KinveyDeleteResponse>
		{

			private const string REST_PATH = "appdata/{appkey}/{collectionName}/{entityID}";

			[JsonProperty]
			public string CollectionName { get; set; }

			[JsonProperty]
			public string EntityId { get; set; }

			public DeleteRequest (string entityId, Type myClass, AbstractClient client, Dictionary<string, string> urlProperties, string collectionName)
				: base (client, "DELETE", REST_PATH, default(KinveyDeleteResponse), urlProperties, collectionName)
			{
				this.CollectionName = urlProperties ["collectionName"];
				this.EntityId = entityId;

			}

		}
			
	}
}
