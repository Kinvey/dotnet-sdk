// Copyright (c) 2014, Kinvey, Inc. All rights reserved.
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

namespace Kinvey.DotNet.Framework.Core
{
//    public class AppData { }

	public class AppData<T> : IOrderedQueryable<T>
    {
        private String collectionName;
        private Type myClass;
        private AbstractClient client;

		private ICache<String, T> cache = null;
		private CachePolicy cachePolicy = CachePolicy.NO_CACHE;

		private IOfflineStore<T> store = null;
		private OfflinePolicy offlinePolicy = OfflinePolicy.ALWAYS_ONLINE;

        public const string IdFieldName = "_id";

		/// <summary>
		/// Initializes a new instance of the <see cref="Kinvey.DotNet.Framework.Core.AppData`1"/> class.
		/// </summary>
		/// <param name="collectionName">Collection name.</param>
		/// <param name="myClass">My class.</param>
		/// <param name="client">Client.</param>
        public AppData(string collectionName, Type myClass, AbstractClient client)
        {
            this.collectionName = collectionName;
            this.myClass = myClass;
            this.client = client;
			this._expression = Expression.Constant (this);
        }

		public AppData(MongoQueryProvider provider, Expression e){
			this._provider = _provider;
			this._expression = e;
			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Kinvey.DotNet.Framework.Core.AppData`1"/> class.
		/// </summary>
		/// <param name="collectionName">Collection name.</param>
		/// <param name="myClass">My class.</param>
		/// <param name="client">Client.</param>
		public AppData(string collectionName, Type myClass, AbstractClient client, MongoQueryProvider provider)
		{
			this.collectionName = collectionName;
			this.myClass = myClass;
			this.client = client;
			this._expression = Expression.Constant (this);
			this._provider = provider;
		}



		/// <summary>
		/// Gets or sets the name of the collection.
		/// </summary>
		/// <value>The name of the collection.</value>
        public string CollectionName
        {
            get { return this.collectionName; }
            set { this.collectionName = value; }
        }

		/// <summary>
		/// Gets or sets the type of the current.
		/// </summary>
		/// <value>The type of the current.</value>
        public Type CurrentType
        {
            get { return this.myClass; }
            set { this.myClass = value; }
        }

		/// <summary>
		/// Gets or sets the kinvey client.
		/// </summary>
		/// <value>The kinvey client.</value>
        public AbstractClient KinveyClient
        {
            get { return this.client; }
            set { this.client = value; }
        }

		/// <summary>
		/// Sets the cache.
		/// </summary>
		/// <param name="cache">Cache.</param>
		/// <param name="policy">Policy.</param>
		public void setCache(ICache<String, T> cache, CachePolicy policy)
		{
			this.cache = cache;
			this.cachePolicy = policy;
		}

		public void setOffline(IOfflineStore<T> store, OfflinePolicy policy){

			this.store = store;
			this.offlinePolicy = policy;

			this.store.dbpath = Path.Combine(((Client) KinveyClient).filePath,  "kinveyOffline.sqlite") ;
			this.store.platform = ((Client) KinveyClient).offline_platform;

		}


		public GetEntityRequest GetEntityBlocking(string entityId)
        {
            var urlParameters = new Dictionary<string, string>();
            urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
            urlParameters.Add("collectionName", CollectionName);
            urlParameters.Add("entityId", entityId);
			GetEntityRequest getEntity = new GetEntityRequest(entityId, myClass, client, urlParameters, CollectionName);
            client.InitializeRequest(getEntity);
			getEntity.setCache (this.cache, this.cachePolicy);
			getEntity.SetStore (this.store, this.offlinePolicy);
            return getEntity;
        }

		public GetRequest GetBlocking()
        {
            var urlParameters = new Dictionary<string, string>();
            urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
            urlParameters.Add("collectionName", CollectionName);
			GetRequest get = new GetRequest(myClass, client, urlParameters, collectionName);
            client.InitializeRequest(get);
			get.setCache (this.cache, this.cachePolicy);
			get.SetStore (this.store, this.offlinePolicy);
            return get;
        }

		public SaveRequest SaveBlocking(T entity)
        {
            SaveRequest save;
			var urlParameters = new Dictionary<string, string>();
            urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
            urlParameters.Add("collectionName", CollectionName);
			save = new SaveRequest(entity, myClass, null, SaveMode.POST, client, urlParameters, this.CollectionName);
			save.SetStore (this.store, this.offlinePolicy);
            client.InitializeRequest(save);
            return save;
        }

        public enum SaveMode
        {
            POST,
            PUT
        }



		#region LINQ

		// private fields
		private MongoQueryProvider _provider;
		private Expression _expression;

		// public methods
		/// <summary>
		/// Gets an enumerator for the results of a MongoDB LINQ query.
		/// </summary>
		/// <returns>An enumerator for the results of a MongoDB LINQ query.</returns>
		public IEnumerator<T> GetEnumerator()
		{
			return ((IEnumerable<T>)_provider.Execute<T>(_expression)).GetEnumerator();
		}

		/// <summary>
		/// Gets the MongoDB query that will be sent to the server when this LINQ query is executed.
		/// </summary>
		/// <returns>The MongoDB query.</returns>
		public IMongoQuery GetMongoQuery()
		{
			return _provider.BuildMongoQuery(this);
		}

		// explicit implementation of IEnumerable
		IEnumerator IEnumerable.GetEnumerator()
		{
			//TODO generics?
			throw new NotImplementedException ();
			//return ((IEnumerable)_provider.Execute(_expression)).GetEnumerator();
		}

		// explicit implementation of IQueryable
		Type IQueryable.ElementType
		{
			get { return typeof(T); }
		}

		Expression IQueryable.Expression
		{
			get { return _expression; }
		}

		IQueryProvider IQueryable.Provider
		{
			get { return (IQueryProvider) _provider; }
		}
		#endregion



        [JsonObject(MemberSerialization.OptIn)]
		public class GetRequest : AbstractKinveyCachedClientRequest<T>
        {
            private const string REST_PATH = "appdata/{appKey}/{collectionName}/";

            [JsonProperty]
            public string collectionName { get; set; }

			public GetRequest(Type myClass, AbstractClient client, Dictionary<string, string> urlParameters, string collection)
				: base(client, "GET", REST_PATH, default(T), urlParameters, collection)
            {
                this.collectionName = urlParameters["collectionName"];
            }

            public override T Execute()
            {
                return base.Execute();
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
		public class GetEntityRequest : AbstractKinveyCachedClientRequest<T>
        {
            private const string REST_PATH = "appdata/{appKey}/{collectionName}/{entityId}";

            [JsonProperty]
            public string EntityId { get; set; }

            [JsonProperty]
            public string collectionName;

			public GetEntityRequest(string entityId, Type myClass, AbstractClient client, Dictionary<string, string> urlParameters, string collection)
				: base(client, "GET", REST_PATH, default(T), urlParameters, collection)
            {
                this.collectionName = urlParameters["collectionName"];
                this.EntityId = entityId;
            }

            public override T Execute()
            {
                T myEntity = base.Execute();
                return myEntity;
            }

        }

        [JsonObject(MemberSerialization.OptIn)]
		public class SaveRequest : AbstractKinveyOfflineClientRequest<T>
        {
            private const string REST_PATH = "appdata/{appKey}/{collectionName}/";

            [JsonProperty]
            public string CollectioName { get; set; }

            [JsonProperty]
            public string EntityId { get; set; }

			public SaveRequest(T entity, Type myClass, string entityId, SaveMode update, AbstractClient client, Dictionary<string, string> urlProperties, string collectionName)
				: base(client, update.ToString(), REST_PATH, entity, urlProperties, collectionName)
            {
                this.CollectioName = urlProperties["collectionName"];
                if (update.Equals(SaveMode.PUT))
                {
                    this.EntityId = entityId;
                }
            }

			public override T Execute()
            {
                T myEntity = base.Execute();
                return myEntity;
            }
        }
    }
}
