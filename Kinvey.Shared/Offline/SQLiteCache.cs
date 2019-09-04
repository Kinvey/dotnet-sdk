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
using SQLite;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Remotion.Linq;

namespace Kinvey
{
	/// <summary>
	/// This is an implementation of an OfflineStore, using SQLite to manage maintaining data.
	/// This class is responsible for breaking apart a request, and determing what actions to take
	/// Actual actions are performed on the OfflineTable class, using a SQLiteDatabaseHelper
	/// </summary>
	public class SQLiteCache <T> : ICache <T> where T : class, new()
    {

		private string collectionName;

		private SQLiteConnection dbConnectionSync;

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.SQLiteOfflineStore"/> class.
		/// </summary>
		/// <param name="collection">Collection.</param>
		/// <param name="connection">Connection.</param>
		public SQLiteCache(string collection, SQLiteAsyncConnection connectionAsync, SQLiteConnection connectionSync)
		{
			this.collectionName = collection;
			this.dbConnectionSync = connectionSync;

			createTable();
		}

		// Creates an SQLite table, which manages the local representation of the connection.
		private void createTable()
		{
			dbConnectionSync.CreateTable<T>();			
		}


		// Deletes the SQLite table associated with the local representation of this collection.
		private int dropTable()
		{
			return dbConnectionSync.DropTable<T>();
		}

		public bool IsCacheEmpty()
		{
            return dbConnectionSync.Table<T>().Count() == 0;
		}

		#region SQLite Cache CRUD APIs

		// CREATE APIs
		//
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
				dbConnectionSync.InsertOrReplace(item);
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
			try
			{
				dbConnectionSync.InsertAll(items);
			}
			catch (SQLiteException e)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_CACHE, EnumErrorCode.ERROR_DATASTORE_CACHE_SAVE_INSERT_ENTITY, "", e);
			}

			return items;
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
            catch (InvalidOperationException invalidOperationException)
            {
                throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_CACHE, EnumErrorCode.ERROR_DATASTORE_CACHE_FIND_BY_ID_NOT_FOUND, string.Empty, invalidOperationException);
            }
            catch (Exception exception)
            {
                throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_CACHE, EnumErrorCode.ERROR_DATASTORE_CACHE_FIND_BY_ID_GENERAL, string.Empty, exception);
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

		private TableQuery<T> BuildQuery(Expression expr)
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
			if (exprSort != null)
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
                dbConnectionSync.RunInTransaction(() => {
                    foreach (var item in items)
                    {
                        dbConnectionSync.InsertOrReplace(item);
                    }
                });
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
			else if (retType == typeof(DateTime))
			{
				var funcSort = exprSort as Expression<Func<T, DateTime>>;
				query = sortAscending? query.OrderBy(funcSort) : query.OrderByDescending(funcSort);
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

        public KinveyDeleteResponse DeleteByQuery(IQueryable<object> query)
        {            
            var kdr = new KinveyDeleteResponse();

            try
            {
                var visitor = new KinveyQueryVisitor(typeof(T), VisitorClause.Where);
                var queryModel = (query.Provider as KinveyQueryProvider)?.qm;
                //We call it here to find unsupported LINQ where clauses.
                queryModel?.Accept(visitor);

                int skipNumber = 0;
                int takeNumber = 0;
                bool sortAscending = true;
                LambdaExpression exprSort = null;

                var lambdaExpr = ConvertQueryExpressionToFunction(query.Expression, ref skipNumber, ref takeNumber, ref sortAscending, ref exprSort);

                var dataTable = dbConnectionSync.Table<T>();

                if (lambdaExpr == null)
                {
                    throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_DATASTORE_WHERE_CLAUSE_IS_ABSENT_IN_QUERY, "'Where' clause is absent in query.");
                }

                dataTable = dataTable.Where(lambdaExpr);

                var matchIDs = new List<string>();
                foreach (var item in dataTable.ToList())
                {
                    var entity = item as IPersistable;
                    matchIDs.Add(entity.ID);
                }

                kdr = this.DeleteByIDs(matchIDs);
            }
            catch (SQLiteException ex)
            {
                throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_CACHE, EnumErrorCode.ERROR_DATASTORE_CACHE_REMOVING_ENTITIES_ACCORDING_TO_QUERY, string.Empty, ex);
            }

            return kdr;
        }

		#endregion
	}

}

