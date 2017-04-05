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
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using SQLite.Net.Async;
using SQLite.Net;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using SQLite.Net.Interop;
using System.Reflection;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Remotion.Linq;
using KinveyUtils;

namespace Kinvey
{
	/// <summary>
	/// This is an implementation of an OfflineStore, using SQLite to manage maintaining data.
	/// This class is responsible for breaking apart a request, and determing what actions to take
	/// Actual actions are performed on the OfflineTable class, using a SQLiteDatabaseHelper
	/// </summary>
	public class SQLiteCache <T> : ICache <T> where T:class, IPersistable
	{

		private string collectionName;

//		private SQLiteAsyncConnection dbConnectionAsync;

		private SQLiteConnection dbConnectionSync;

		private ISQLitePlatform platform;

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.SQLiteOfflineStore"/> class.
		/// </summary>
		/// <param name="collection">Collection.</param>
		/// <param name="connection">Connection.</param>
		public SQLiteCache(string collection, SQLiteAsyncConnection connectionAsync, SQLiteConnection connectionSync, ISQLitePlatform platform)
		{
			this.collectionName = collection;
//			this.dbConnectionAsync = connectionAsync;
			this.dbConnectionSync = connectionSync;
			this.platform = platform;

			//dropTable();
			createTable();
		}

		// Creates an SQLite table, which manages the local representation of the connection.
		private int createTable()
		{
			//dbConnection.CreateTableAsync<T> ();
			int retVal = dbConnectionSync.CreateTable<T>();

			//set primary key
//			IEnumerable<PropertyInfo> props = platform.ReflectionService.GetPublicInstanceProperties (typeof (T));
//			//var primaryKey = null;
//			foreach (var p in props) {
//				if (p.IsDefined (typeof(JsonPropertyAttribute), true)){
//					//if _id is specified, set primary key on the SQL
//					foreach (CustomAttributeData attr in p.CustomAttributes) {
//						//JsonPropertyAttribute jsonAttr = attr as JsonPropertyAttribute;
//						string propName = attr.ConstructorArguments.First ().Value;
//						//string propValue = attr.ConstructorArguments.First ().();
//							
//						//string propName = jsonAttr.PropertyName;
//						//if (propName.Equals ("_id")) {
//							//string sqlStmt = string.Format ("ALTER TABLE \"{ 0}\" ADD PRIMARY KEY \"{ 1}\" = ?", typeof(T), p);
//
//						//}
//					}
//				}
//			}

			return retVal;
		}


		// Deletes the SQLite table associated with the local representation of this collection.
		private int dropTable()
		{
			//dbConnection.DropTableAsync<T> ();
			return dbConnectionSync.DropTable<T>();
		}

		public bool IsCacheEmpty()
		{
			return dbConnectionSync.Table<T>().Count() == 0;
		}

		#region SQLite Cache CRUD APIs

		// CREATE APIs
		//

		//public async Task<T> SaveAsync (T item)
		public T Save(T item)
		{
			try
			{
				dbConnectionSync.Insert(item);
			}
			catch (SQLiteException e)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_CACHE, EnumErrorCode.ERROR_DATASTORE_CACHE_SAVE_INSERT_ENTITY, "", e);
			}

			return item;
		}

		public T Update(T item)
		{
			try
			{
				dbConnectionSync.Update(item);
			}
			catch (SQLiteException e)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_CACHE, EnumErrorCode.ERROR_DATASTORE_CACHE_SAVE_UPDATE_ENTITY, "", e);
			}

			return item;
		}

		public List<T> Save(List<T> items)
		{
			// TODO implement
//			int result = dbConnectionSync.InsertAll(items); 
//			if (result > 0)
//			{
//				return items;
//			}

			return default(List<T>);
		}

		public T UpdateCacheSave(T item, string tempID)
		{
			try
			{
				JObject obj = JObject.FromObject(item);
				string ID = obj["_id"].ToString();
				string tableName = typeof(T).Name;
				string query = $"update {tableName} set _id=\"{ID}\" where _id=\"{tempID}\"";

				//
				dbConnectionSync.Execute(query);

				dbConnectionSync.Update(item);
			}
			catch (SQLiteException e)
			{
				throw  new KinveyException(EnumErrorCategory.ERROR_DATASTORE_CACHE, EnumErrorCode.ERROR_DATASTORE_CACHE_SAVE_UPDATE_ID, "", e);
			}

			return item;
		}

		// READ APIs
		//

		public List<T> FindAll()
		{
			return dbConnectionSync.Table<T>().ToList();
		}

		public int CountAll() 
		{
			return dbConnectionSync.Table<T>().Count();
		}


		public T FindByID(string ID)
		{
			T item = default(T);
			try
			{
				item = dbConnectionSync.Get<T>(ID);
			}
			catch (Exception e)
			{
				// item not found, just return the default item
				Logger.Log("Kinvey exception in cache find: item not found.  " + e.Message);
			}

			return item;
		}

		public List<T> FindByIDs(List<string> IDs)
		{
			List<T> listEntities = new List<T>();

			foreach (string ID in IDs)
			{
				listEntities.Add(this.FindByID(ID));
			}

			return listEntities;
		}

		public List<T> FindByQuery(Expression expr)
		{
			try
			{
				var query = BuildQuery(expr);
				return query.ToList();
			}
			catch (Exception e)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_CACHE, EnumErrorCode.ERROR_DATASTORE_CACHE_FIND_QUERY, "", e);
			}
		}

		public T LastModifiedEntity(Expression expr) {
			try
			{
				var query = BuildQuery(expr, false).OrderByDescending(x => x.KMD);
				var obj = query.FirstOrDefault();
				return obj;

				//string tableName = typeof(T).Name;

				////var query = $"select * from {tableName} order by _kmd desc limit 1";
				//Stopwatch sw = new Stopwatch();
				//sw.Start();
				//var item = dbConnectionSync.FindWithQuery<T>(query);

				//sw.Stop();
				//Debug.WriteLine("Found last object in: " + sw.Elapsed);
				//return item;

			}
			catch (Exception e) { 
				throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_CACHE, EnumErrorCode.ERROR_DATASTORE_CACHE_FIND_QUERY, "", e);
			}
		}
		public int CountByQuery(Expression expr)
		{
			try
			{
				var query = BuildQuery(expr);
				if (query == null) { return 0; }

				return query.Count();
			}
			catch (Exception e)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_CACHE, EnumErrorCode.ERROR_DATASTORE_CACHE_FIND_QUERY, "", e);
			}
		}

		private TableQuery<T> BuildQuery(Expression expr, bool shouldApplySort = true)
		{
			int skipNumber = 0;
			int takeNumber = 0;
			bool sortAscending = true;
			LambdaExpression exprSort = null;
			var lambdaExpr = ConvertQueryExpressionToFunction(expr, ref skipNumber, ref takeNumber, ref sortAscending, ref exprSort);

			var query = dbConnectionSync.Table<T>();
			if (lambdaExpr != null)
			{
				query = query.Where(lambdaExpr);
			}
			if (skipNumber != 0)
			{
				query = query.Skip(skipNumber);
			}
			if (takeNumber != 0)
			{
				query = query.Take(takeNumber);
			}
			if (exprSort != null && shouldApplySort)
			{
				ApplySort(ref query, sortAscending, exprSort);
			}

			return query;
		}

		private Expression<Func<T, bool>> ConvertQueryExpressionToFunction(Expression expr, ref int skipNumber, ref int takeNumber, ref bool sortAscending, ref LambdaExpression exprSort)
		{
			Expression<Func<T, bool>> lambdaExpr = null;
			if (expr?.NodeType == ExpressionType.Call)
			{
				MethodCallExpression mcb = expr as MethodCallExpression;

				var args = mcb?.Arguments;
				if (args.Count >= 2)
				{
					var nodeType = args[1]?.NodeType;
					if (nodeType == ExpressionType.Quote)
					{
						MethodInfo methodInfo = mcb.Method;

						if (methodInfo.Name.Equals("OrderBy") ||
						    methodInfo.Name.Equals("OrderByDescending"))
						{
							if (methodInfo.Name.Equals("OrderByDescending"))
							{
								sortAscending = false;
							}

							// sort modifier added
							UnaryExpression quote = mcb.Arguments[1] as UnaryExpression;

							if (quote.Operand.NodeType == ExpressionType.Lambda)
							{
								LambdaExpression le = quote.Operand as LambdaExpression;
								exprSort = le;
								return ConvertQueryExpressionToFunction(args[0], ref skipNumber, ref takeNumber, ref sortAscending, ref exprSort);
							}
						}
						else
						{
							UnaryExpression quote = mcb.Arguments[1] as UnaryExpression;

							if (quote.Operand.NodeType == ExpressionType.Lambda)
							{
								LambdaExpression le = quote.Operand as LambdaExpression;
								lambdaExpr = le as Expression<Func<T, bool>>;
							}
						}
					}
					else if (nodeType == ExpressionType.Constant)
					{
						MethodInfo methodInfo = mcb.Method;

						if (methodInfo.Name.Equals("Skip"))
						{
							if (IsTypeNumber(args[1]?.Type))
							{
								skipNumber = int.Parse(args[1].ToString());
								return ConvertQueryExpressionToFunction(args[0], ref skipNumber, ref takeNumber, ref sortAscending, ref exprSort);
							}
						}
						else if (methodInfo.Name.Equals("Take"))
						{
							if (IsTypeNumber(args[1]?.Type))
							{
								takeNumber = int.Parse(args[1].ToString());
								return ConvertQueryExpressionToFunction(args[0], ref skipNumber, ref takeNumber, ref sortAscending, ref exprSort);
							}

						}
					}
				}
			}

			return lambdaExpr;
		}

		private bool IsTypeNumber(Type type)
		{
			if (type == typeof(short) ||
				type == typeof(int) ||
				type == typeof(long) ||
				type == typeof(Int16) ||
				type == typeof(Int32) ||
				type == typeof(Int64) ||
				type == typeof(UInt16) ||
				type == typeof(UInt32) ||
				type == typeof(UInt64))
			{
				return true;
			}

			return false;
		}

		public List<GroupAggregationResults> GetAggregateResult(EnumReduceFunction reduceFunction, string groupField, string aggregateField, Expression query)
		{
			List<GroupAggregationResults> localAggregateResults = new List<GroupAggregationResults>();
			List<object> listValues = new List<object>();

			PropertyInfo propInfo = typeof(T).GetRuntimeProperty(aggregateField);

			if (propInfo != null &&
				IsTypeNumber(propInfo.PropertyType))
			{
				int skipNumber = 0;
				int takeNumber = 0;
				bool sort = false;
				LambdaExpression exprSort = null;

				var lambdaExpr = ConvertQueryExpressionToFunction(query, ref skipNumber, ref takeNumber, ref sort, ref exprSort);

				if (String.IsNullOrEmpty(groupField))
				{
					// Not grouping results be a specified field, so just aggregate over all entities
					// that pass through the query filter, if provided.
					GroupAggregationResults gar = new GroupAggregationResults();
					gar.GroupField = null;

					// TODO do "skip" and "take" have to be taken into account in group aggregate functions?
					if (lambdaExpr != null)
					{
						listValues = (from t in dbConnectionSync.Table<T>().Where(lambdaExpr) select t.GetType().GetRuntimeProperty(aggregateField).GetValue(t, null)).ToList();
					}
					else
					{
						listValues = (from t in dbConnectionSync.Table<T>() select t.GetType().GetRuntimeProperty(aggregateField).GetValue(t, null)).ToList();
					}

					switch (reduceFunction)
					{
						case EnumReduceFunction.REDUCE_FUNCTION_SUM:
							foreach (int val in listValues)
							{
								gar.Result += val;
							}
							break;

						case EnumReduceFunction.REDUCE_FUNCTION_MIN:
							gar.Result = int.MaxValue;
							foreach (int val in listValues)
							{
								gar.Result = Math.Min(gar.Result, val);
							}
							break;

						case EnumReduceFunction.REDUCE_FUNCTION_MAX:
							gar.Result = int.MinValue;
							foreach (int val in listValues)
							{
								gar.Result = Math.Max(gar.Result, val);
							}
							break;

						case EnumReduceFunction.REDUCE_FUNCTION_AVERAGE:
							int count = 0;
							int total = 0;
							foreach (int val in listValues)
							{
								total += val;
								count++;
							}
							gar.Result = total / count;
							break;

						default:
							// TODO throw new KinveyException
							break;
					}

					localAggregateResults.Add(gar);
				}
				else
				{
					// A grouping field was supplied, so aggregate
					// result per group created on the group field
					IEnumerable<IGrouping<object, T>> grouplist;
					if (lambdaExpr != null)
					{
						grouplist = from t in dbConnectionSync.Table<T>().Where(lambdaExpr)
									group t by t.GetType().GetRuntimeProperty(groupField).GetValue(t, null);
					}
					else
					{
						grouplist = from t in dbConnectionSync.Table<T>()
									group t by t.GetType().GetRuntimeProperty(groupField).GetValue(t, null);
					}

					foreach (var grouping in grouplist)
					{
						int result = 0;
						listValues = (from x in grouping select x.GetType().GetRuntimeProperty(aggregateField).GetValue(x, null)).ToList();

						GroupAggregationResults gar = new GroupAggregationResults();
						gar.GroupField = grouping.Key.ToString();

						switch (reduceFunction)
						{
							case EnumReduceFunction.REDUCE_FUNCTION_SUM:
								foreach (int val in listValues)
								{
									result += val;
								}
								break;

							case EnumReduceFunction.REDUCE_FUNCTION_MIN:
								result = int.MaxValue;
								foreach (int val in listValues)
								{
									result = Math.Min(result, val);
								}
								break;

							case EnumReduceFunction.REDUCE_FUNCTION_MAX:
								result = int.MinValue;
								foreach (int val in listValues)
								{
									result = Math.Max(result, val);
								}
								break;

							case EnumReduceFunction.REDUCE_FUNCTION_AVERAGE:
								int count = 0;
								int total = 0;
								foreach (int val in listValues)
								{
									total += val;
									count++;
								}
								result = total / count;
								break;

							default:
								// TODO throw new KinveyException
								break;
						}

						gar.Result = result;
						localAggregateResults.Add(gar);
					}
				}
			}

			return localAggregateResults;
		}

		public async Task<List<T>> GetAsync(string query)
		{
			// TODO implement
			return default(List<T>);
		}


		// UPDATE APIs
		//
		public List<T> RefreshCache(List<T> items)
		{
			try
			{
				dbConnectionSync.InsertOrReplaceAll(items);
			}
			catch (SQLiteException e)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_CACHE, EnumErrorCode.ERROR_DATASTORE_CACHE_REFRESH, "", e);
			}

			return items;
		}

		// DELETE APIs
		//

		/// <summary>
		/// Clear this local cache table of all its content.
		/// </summary>
		public KinveyDeleteResponse Clear(Expression expr = null)
		{
			KinveyDeleteResponse kdr = new KinveyDeleteResponse();

			try
			{
				if (expr == null)
				{
					kdr.count = dbConnectionSync.DeleteAll<T>();
				}
				else { 
					int skipNumber = 0;
					int takeNumber = 0;
					bool sortAscending = true;
					LambdaExpression exprSort = null;

					var lambdaExpr = ConvertQueryExpressionToFunction(expr, ref skipNumber, ref takeNumber, ref sortAscending, ref exprSort);

					if (lambdaExpr == null && skipNumber == 0 && takeNumber == 0)
					{
						kdr.count = dbConnectionSync.DeleteAll<T>();
					}
					else if (skipNumber == 0)
					{
						List<T> results;

						var query = dbConnectionSync.Table<T>();
						if (lambdaExpr != null)
						{
							query = query.Where(lambdaExpr);
						}

						if (takeNumber != 0)
						{
							query = query.Take(takeNumber);
						}

						if (exprSort != null)
						{
							ApplySort(ref query, sortAscending, exprSort);
						}

						results = query.ToList();

						List<string> matchIDs = new List<string>();
						foreach (var match in results)
						{
							IPersistable entity = match as IPersistable;
							matchIDs.Add(entity.ID);
						}

						kdr = this.DeleteByIDs(matchIDs);
					}
					else
					{
						// Pagination appears to be happening here, so we should not delete any cached items because the complete pull is no finished.
						// Do nothing here.					
					}
				}

			}
			catch (SQLiteException e)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_CACHE, EnumErrorCode.ERROR_DATASTORE_CACHE_CLEAR, "", e);
			}

			return kdr;
		}

		private void ApplySort(ref TableQuery<T> query, bool sortAscending, LambdaExpression exprSort)
		{
			Type retType = exprSort.ReturnType;
			if (retType == typeof(string))
			{
				var funcSort = exprSort as Expression<Func<T, string>>;
				query = sortAscending ? query.OrderBy(funcSort) : query.OrderByDescending(funcSort);
			}
			else if (retType == typeof(int))
			{
				var funcSort = exprSort as Expression<Func<T, int>>;
				query = sortAscending ? query.OrderBy(funcSort) : query.OrderByDescending(funcSort);
			}
			else if (retType == typeof(uint))
			{
				var funcSort = exprSort as Expression<Func<T, uint>>;
				query = sortAscending ? query.OrderBy(funcSort) : query.OrderByDescending(funcSort);
			}
		}

		public KinveyDeleteResponse DeleteByID(string id)
		{
			KinveyDeleteResponse kdr = new KinveyDeleteResponse();

			try
			{
				kdr.count = dbConnectionSync.Delete<T>(id);
				var ids = new List<string>();
				ids.Add(id);
				kdr.IDs = ids;
			}
			catch (SQLiteException e)
			{
				Logger.Log("Kinvey exception in cache remove: item not found.  " + e.Message);
			}
			catch (Exception e)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_CACHE, EnumErrorCode.ERROR_DATASTORE_CACHE_REMOVE_ENTITY, "", e);
			}

			return kdr;
		}

		public KinveyDeleteResponse DeleteByIDs(List<string> IDs)
		{
			KinveyDeleteResponse kdr = new KinveyDeleteResponse();

			foreach (string ID in IDs)
			{
				kdr.count += DeleteByID(ID).count;
			}

			kdr.IDs = new List<string>(IDs);
			return kdr;
		}

		public async Task<KinveyDeleteResponse> DeleteAsync (string query)
		{
			// TODO implement
			return null;
		}

		#endregion

//		public async Task<object> GetAsync(AbstractKinveyOfflineClientRequest<T> request){
//
//			//expand the URL
//			string targetURI = request.uriTemplate;
//			foreach (var p in request.uriResourceParameters)
//			{
//				targetURI = targetURI.Replace("{" + p.Key + "}", p.Value.ToString());
//			}
//
//			int idIndex = targetURI.IndexOf(this.collectionName) + this.collectionName.Length + 1;
//
//
//
//			object ret = null;
//			//is it a query?  (12 is magic number for decoding empty query string)
//			if (targetURI.Contains ("query") && (targetURI.IndexOf ("query") + 12) != targetURI.Length) {
//			
//				//it's a query!
//				//pull the actual query string out and get rid of the "?query"
//				String query = targetURI.Substring(idIndex, targetURI.Length - idIndex);
//				query = query.Replace("?query=","");
//				query = WebUtility.UrlDecode(query);
//
//				await createTableAsync ();
//
//				T[] ok = await getQueryAsync(query);
//
//
//				await enqueueRequestAsync("QUERY", query, request);
//				return ok;
//
//			} else if (idIndex == (targetURI.Length + 1)|| targetURI.Contains ("query")) {
//				//it's a get all request (no query, no id)
//				await createTableAsync ();
//				List<T> ok = await getAllAsync ();
//				return ok;
//			} else {
//				//it's a get by id
//				String targetID = targetURI.Substring(idIndex, targetURI.Length - idIndex);
//				await createTableAsync ();
//				ret = (T) await getEntityAsync (targetID);
//
//				await enqueueRequestAsync("GET", targetURI.Substring(idIndex, targetURI.Length - idIndex), request);
//			}
//
//
//			return ret;
//		}
//
//		private async Task<T[]> getQueryAsync (string queryString)
//		{
//			
//			SQLTemplates.QueryItem query = await dbConnection.Table<SQLTemplates.QueryItem>().Where(t => t.query == queryString && t.collection == collectionName).FirstOrDefaultAsync();
//
//			if (query == null) {
//				return null;
//			}
//
//			List<SQLTemplates.OfflineEntity> entities = new List<SQLTemplates.OfflineEntity>();
//
//			string[] ids = query.commaDelimitedIds.Split (',');
//
//			foreach (string id in ids){
//				entities.Add(dbConnection.Table<SQLTemplates.OfflineEntity>().Where(t => t.id == id && t.collection == collectionName).FirstOrDefaultAsync().Result);
//			}
//
//			T[] results = new T[ids.Length];
//
//			for (int i = 0; i < results.Length; i++){
//				results[i] = JsonConvert.DeserializeObject<T>(entities[i].json);
//			}
//
//			return results;
//
//		}
//
//
//		public async Task<object> SaveAsync (AbstractKinveyOfflineClientRequest<T> request){
//			//DatabaseHelper<T> handler = SQLiteCacheManager.getDatabaseHelper<T> ();
//
//			//grab json content and put it in the store
//			string jsonContent = null;
//			if (request.HttpContent != null) {
//				jsonContent = JsonConvert.SerializeObject (request.HttpContent);
//			}
//		
//
//			//grab the ID
//			JToken token = JObject.Parse(jsonContent);
//			string id = (string)token.SelectToken("_id");
//
//			//insert the entity into the database
//			await createTableAsync ();
//			await upsertEntityAsync(id, jsonContent);
//			//enque the request
//			await enqueueRequestAsync("PUT", id, request);
//
//			return request.HttpContent;
//		}
//
//
//		public async Task<KinveyDeleteResponse> DeleteAsync (AbstractKinveyOfflineClientRequest<T> request){
//		
//		}
//
//		public async Task<int> saveQueryResultsAsync (string queryString, string collection, List<string> ids)
//		{
//			SQLTemplates.QueryItem query = new SQLTemplates.QueryItem ();
//			query.query = queryString;
//			query.collection = collection;
//			query.commaDelimitedIds = String.Join (",", ids); 
//
//
//			int count = await dbConnection.UpdateAsync (query);
//			if (count == 0) {
//				await dbConnection.InsertAsync (query);
//			}
//
//			return 0;
//		}
//
//
//		public async Task<int> enqueueRequestAsync (string action, string id, AbstractKinveyOfflineClientRequest<T> req)
//		{
//			SQLTemplates.QueueItem queue = new SQLTemplates.QueueItem ();
//			queue.action = action;
//			queue.collection = collectionName;
//
//			SQLTemplates.OfflineMetaData metadata = new SQLTemplates.OfflineMetaData ();
//			metadata.id = id;
//			metadata.customHeaders = req.customRequestHeaders;
//
//			queue.OfflineMetaDataAsJson = JsonConvert.SerializeObject (metadata);
//			//queue.id = metadata;
//
//			await dbConnection.InsertAsync (queue);
//
//			return 0;
//		}
//
//		public async Task<int> enqueueRequestAsync (string action,SQLTemplates.OfflineMetaData metadata)
//		{
//			SQLTemplates.QueueItem queue = new SQLTemplates.QueueItem ();
//			queue.action = action;
//			queue.collection = collectionName;
//
//			queue.OfflineMetaDataAsJson = JsonConvert.SerializeObject (metadata);
//
//			await dbConnection.InsertAsync (queue);
//
//			return 0;
//		}
//
//		public async Task<List<T>> getAllAsync ()
//		{
//
//			List<SQLTemplates.OfflineEntity> entities = await dbConnection.Table<SQLTemplates.OfflineEntity> ().Where (t => t.collection == collectionName).ToListAsync ();
//
//			List<T> results = new List<T>();
//
//			foreach (SQLTemplates.OfflineEntity ent in entities) {
//				results.Add(JsonConvert.DeserializeObject<T>(ent.json));
//			}
//
//			return results;
//		}
//
//
//		public async Task<T> getEntityAsync (string id)
//		{
//
//			SQLTemplates.OfflineEntity entity = await dbConnection.Table<SQLTemplates.OfflineEntity> ().Where (t => t.collection == collectionName && t.id == id).FirstOrDefaultAsync ();
//
//			if (entity == default(SQLTemplates.OfflineEntity)) {
//				return default(T);
//			}
//			return JsonConvert.DeserializeObject<T> (entity.json);
//
//		}
//
//		public async Task<KinveyDeleteResponse> deleteAsync(string id)
//		{
//
//			SQLTemplates.OfflineEntity entity = await dbConnection.Table<SQLTemplates.OfflineEntity> ().Where (t => t.collection == collectionName && t.id == id).FirstOrDefaultAsync ();
//
//			int count = await dbConnection.DeleteAsync (entity.id);
//
//
//			KinveyDeleteResponse resp = new KinveyDeleteResponse ();
//			resp.count = count;
//
//			return resp;
//		}
//
//		public async Task<SQLTemplates.QueueItem> popQueueAsync (){
//			SQLTemplates.QueueItem item = await dbConnection.Table<SQLTemplates.QueueItem> ().FirstOrDefaultAsync ();
//			await removeFromQueueAsync (item.key);
//			return item;
//
//		}
//
//		public async Task<int> removeFromQueueAsync (int primaryKey)
//		{
//
//			await dbConnection.DeleteAsync<SQLTemplates.QueueItem> (primaryKey);
//			return 1;
//
//		}
//
//		/// <summary>
//		/// Executes a delete request.
//		/// </summary>
//		/// <returns>The delete.</returns>
//		/// <param name="client">Client.</param>
//		/// <param name="appdata">Appdata.</param>
//		/// <param name="request">Request.</param>
//		/// <typeparam name="T">The type of the response.</typeparam>
//		/// <param name="appData">App data.</param>
//		public async Task<KinveyDeleteResponse> executeDeleteAsync<T>(AbstractKinveyClient client, AbstractKinveyOfflineClientRequest<T> request){
//			//DatabaseHelper<T> handler = SQLiteCacheManager.getDatabaseHelper<T> ();
//
//			//expand the URL
//			string targetURI = request.uriTemplate;
//			foreach (var p in request.uriResourceParameters)
//			{
//				targetURI = targetURI.Replace("{" + p.Key + "}", p.Value.ToString());
//			}
//			int idIndex = targetURI.IndexOf(this.collectionName) + this.collectionName.Length + 1;
//
//			String targetID = targetURI.Substring(idIndex, targetURI.Length - idIndex);
//
//
//
//
//			await createTableAsync ();
//			KinveyDeleteResponse ret = await deleteAsync(targetID);
//
//			await enqueueRequestAsync("DELETE", targetURI.Substring(idIndex, targetURI.Length - idIndex), request);
//			return ret;
//		}
//
//		/// <summary>
//		/// Inserts an entity directly into the database.
//		/// </summary>
//		/// <param name="client">Client.</param>
//		/// <param name="appdata">Appdata.</param>
//		/// <param name="entity">Entity.</param>
//		/// <typeparam name="T">The type of the response.</typeparam>
//		/// <param name="appData">App data.</param>
//		public async Task<int> insertEntityAsync<T>(AbstractKinveyClient client, T entity){
//
//			//DatabaseHelper<T> handler = SQLiteCacheManager.getDatabaseHelper<T> ();
//
//			string jsonContent = JsonConvert.SerializeObject (entity);
//
//			//grab the ID
//			JToken token = JObject.Parse(jsonContent);
//			string id = (string)token.SelectToken("_id");
//
//			await createTableAsync ();
//
//			await upsertEntityAsync( id, jsonContent);
//			return 0;
//
//		}
//
//		/// <summary>
//		/// Upserts a specific entity, adding it directly to to the offline table.
//		/// </summary>
//		/// <param name="id">Identifier.</param>
//		/// <param name="collection">Collection.</param>
//		/// <param name="json">Json.</param>
//		public async Task<T> upsertEntityAsync(string id, string json){
//			SQLTemplates.OfflineEntity entity = new SQLTemplates.OfflineEntity ();
//			entity.id = id;
//			entity.json = json;
//			entity.collection = collectionName;
//
//
//			int count = await dbConnection.UpdateAsync (entity);
//			if (count == 0) {
//				await dbConnection.InsertAsync (entity);
//			}
//
//			return JsonConvert.DeserializeObject<T> (json);
//
//		}
//
//

	}

}

