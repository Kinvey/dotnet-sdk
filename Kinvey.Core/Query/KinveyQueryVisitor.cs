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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;

namespace Kinvey
{
	public class KinveyQueryVisitor : QueryModelVisitorBase
	{
		IQueryBuilder builderMongoQuery;
		Dictionary<string, string> mapPropertyToName;

		public KinveyQueryVisitor(IQueryBuilder builder, Type type)
		{
			builderMongoQuery = builder;

			LoadMapOfKeysForType(type);
		}

		private void LoadMapOfKeysForType(Type type)
		{
			mapPropertyToName = new Dictionary<string, string>();

			var properties = type.GetRuntimeProperties();

			foreach (PropertyInfo propertyInfo in properties)
			{
				var propertyAttributes = propertyInfo.GetCustomAttributes(true);
				foreach (var attribute in propertyAttributes)
				{
					JsonPropertyAttribute jsonPropertyAttribute = attribute as JsonPropertyAttribute;
					if (jsonPropertyAttribute != null)
					{
						if (jsonPropertyAttribute.PropertyName == null)
						{
							mapPropertyToName.Add (propertyInfo.Name, propertyInfo.Name);
						}
						else
						{
							mapPropertyToName.Add (propertyInfo.Name, jsonPropertyAttribute.PropertyName);
						}
					}
				}
			}
		}

		public override void VisitQueryModel(QueryModel queryModel)
		{
			base.VisitQueryModel(queryModel);
		}

		protected override void VisitBodyClauses(ObservableCollection<IBodyClause> bodyClauses, QueryModel queryModel)
		{
			base.VisitBodyClauses(bodyClauses, queryModel);
		}

		protected override void VisitOrderings(ObservableCollection<Ordering> orderings, QueryModel queryModel, OrderByClause orderByClause)
		{
			base.VisitOrderings(orderings, queryModel, orderByClause);

			foreach (var ordering in orderings)
			{
				var member = ordering.Expression as MemberExpression;
				string sort = "&sort={\"" + mapPropertyToName[member.Member.Name] + "\":" + (ordering.OrderingDirection.ToString().Equals("Asc") ? "1" : "-1") + "}";
				builderMongoQuery.AddModifier(sort);
			}
		}

		public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
		{
			base.VisitResultOperator(resultOperator, queryModel, index);

			if (resultOperator.ToString().Contains("Skip"))
			{
				SkipResultOperator skip = resultOperator as SkipResultOperator;
				builderMongoQuery.AddModifier("&skip=" + skip.Count);
			}
			else if (resultOperator.ToString().Contains("Take"))
			{
				TakeResultOperator take = resultOperator as TakeResultOperator;
				builderMongoQuery.AddModifier("&limit=" + take.Count);
			}
			else
			{
				throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "LINQ result operator not supported.");
			}
		}

		public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
		{
			base.VisitWhereClause(whereClause, queryModel, index);

			if (whereClause.Predicate.NodeType.ToString().Equals("Equal"))
			{
				BinaryExpression equality = whereClause.Predicate as BinaryExpression;
				var member = equality.Left as MemberExpression;
				var argument = equality.Right.ToString();

				if (index > 0)
				{
					// multiple where clauses present, so separate with comma
					builderMongoQuery.Write(",");
				}

				builderMongoQuery.Write("\"" + mapPropertyToName[member.Member.Name] + "\"");
				builderMongoQuery.Write(":");

				if (equality.Right.Type.Name.Equals("Boolean"))
				{
					// case where boolean value is explicitly checked with double equal sign (example below)
					// 		var query = from e in todoStore
					//					where e.BoolVal == true
					//					select e;
					argument = equality.Right.ToString().ToLower();
				}

				builderMongoQuery.Write(argument);
			}
			else if (whereClause.Predicate.NodeType.ToString().Equals("GreaterThan"))
			{
				BinaryExpression equality = whereClause.Predicate as BinaryExpression;
				var member = equality.Left as MemberExpression;
				var argument = equality.Right.ToString();

				builderMongoQuery.Write("\"" + mapPropertyToName[member.Member.Name] + "\"");

				if (equality.Right.Type.Name.Equals("DateTime"))
				{
					DateTime dt = DateTime.Parse(equality.Right.ToString());
					argument = Newtonsoft.Json.JsonConvert.SerializeObject(dt);
				}

				builderMongoQuery.Write(":{\"$gt\":");
				builderMongoQuery.Write(argument);
				builderMongoQuery.Write("}");
			}
			else if (whereClause.Predicate.NodeType.ToString().Equals("LessThan"))
			{
				BinaryExpression equality = whereClause.Predicate as BinaryExpression;
				var member = equality.Left as MemberExpression;
				var argument = equality.Right.ToString();

				builderMongoQuery.Write("\"" + mapPropertyToName[member.Member.Name] + "\"");

				if (equality.Right.Type.Name.Equals("DateTime"))
				{
					DateTime dt = DateTime.Parse(equality.Right.ToString());
					argument = Newtonsoft.Json.JsonConvert.SerializeObject(dt);
				}

				builderMongoQuery.Write(":{\"$lt\":");
				builderMongoQuery.Write(argument);
				builderMongoQuery.Write("}");
			}
			else if (whereClause.Predicate.NodeType.ToString().Equals("LessThanOrEqual"))
			{
				BinaryExpression equality = whereClause.Predicate as BinaryExpression;
				var member = equality.Left as MemberExpression;
				var argument = equality.Right.ToString();

				builderMongoQuery.Write("\"" + mapPropertyToName[member.Member.Name] + "\"");

				if (equality.Right.Type.Name.Equals("DateTime"))
				{
					DateTime dt = DateTime.Parse(equality.Right.ToString());
					argument = Newtonsoft.Json.JsonConvert.SerializeObject(dt);
				}

				builderMongoQuery.Write(":{\"$lte\":");
				builderMongoQuery.Write(argument);
				builderMongoQuery.Write("}");
			}
			else if (whereClause.Predicate.NodeType.ToString().Equals("GreaterThanOrEqual"))
			{
				BinaryExpression equality = whereClause.Predicate as BinaryExpression;
				var member = equality.Left as MemberExpression;
				var argument = equality.Right.ToString();

				builderMongoQuery.Write("\"" + mapPropertyToName[member.Member.Name] + "\"");

				if (equality.Right.Type.Name.Equals("DateTime"))
				{
					DateTime dt = DateTime.Parse(equality.Right.ToString());
				argument = Newtonsoft.Json.JsonConvert.SerializeObject(dt);
				}

				builderMongoQuery.Write(":{\"$gte\":");
				builderMongoQuery.Write(argument);
				builderMongoQuery.Write("}");
			}
			else if (whereClause.Predicate.NodeType.ToString().Equals("AndAlso"))
			{
				BinaryExpression and = whereClause.Predicate as BinaryExpression;
				VisitWhereClause(new WhereClause(and.Right), queryModel, index);
				builderMongoQuery.Write(",");
				VisitWhereClause(new WhereClause(and.Left), queryModel, index);
			}
			else if (whereClause.Predicate.NodeType.ToString().Equals("OrElse"))
			{
				BinaryExpression or = whereClause.Predicate as BinaryExpression;

				builderMongoQuery.Write("\"$or\":");
				builderMongoQuery.Write("[");
				builderMongoQuery.Write("{");
				VisitWhereClause(new WhereClause(or.Left), queryModel, index);
				builderMongoQuery.Write("},");
				builderMongoQuery.Write("{");
				VisitWhereClause(new WhereClause(or.Right), queryModel, index);
				builderMongoQuery.Write("}");
				builderMongoQuery.Write("]");
			}
			else if (whereClause.Predicate.NodeType == ExpressionType.MemberAccess &&
					 whereClause.Predicate.Type == typeof(System.Boolean))
			{
				// Case where query is against boolean value, where value is not specified (example below)
				// 		var query = from e in todoStore
				//					where e.BoolVal
				//					select e;

				MemberExpression ma = whereClause.Predicate as MemberExpression;

				string name = ma.Member.Name;
				string propertyName = mapPropertyToName[name];

				if (index > 0)
				{
					// multiple where clauses present, so separate with comma
					builderMongoQuery.Write(",");
				}

				builderMongoQuery.Write("\"" + propertyName + "\"");
				builderMongoQuery.Write(":");
				builderMongoQuery.Write("true");
			}
			else if (whereClause.Predicate.NodeType == ExpressionType.Call)
			{
				MethodCallExpression b = whereClause.Predicate as MethodCallExpression;

				string name = (b.Object as MemberExpression).Member.Name;
				string propertyName = mapPropertyToName[name];
				//name = name.Replace("\"", "\\\"");

				var arg = b.Arguments[0];
				var argType = arg.Type;
				string argument = arg.ToString().Trim('"');

				if (b.Method.Name.Equals("StartsWith"))
				{
					if (index > 0)
					{
						// multiple where clauses present, so separate with comma
						builderMongoQuery.Write(",");
					}

					builderMongoQuery.Write("\"" + propertyName + "\"");
					builderMongoQuery.Write(":{\"$regex\":\"^");
					argument = argument.Replace("\"", "\\\"");
					builderMongoQuery.Write(argument);
					builderMongoQuery.Write("\"}");
				}
				else if (b.Method.Name.Equals("Equals"))
				{
					if (index > 0)
					{
						// multiple where clauses present, so separate with comma
						builderMongoQuery.Write(",");
					}

					builderMongoQuery.Write("\"" + propertyName + "\"");
					builderMongoQuery.Write(":");
					if (argType.Name.Equals("String"))
					{
						argument = argument.Replace("\"", "\\\"");
						argument = "\"" + argument + "\"";
					}
					else if (argType.Name.Equals("Boolean"))
					{
						// Case where query is against boolean value, using the "Equals" expression (example below)
						// 		var query = todoStore.Where(x => x.BoolVal.Equals(true));
						argument = argument.ToLower();
					}
					builderMongoQuery.Write(argument);
				}
				else
				{
					throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_NETWORK, EnumErrorCode.ERROR_LINQ_WHERE_CLAUSE_NOT_SUPPORTED, "LINQ where clause method not supported.");
				}
			}
			else
			{
				throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_NETWORK, EnumErrorCode.ERROR_LINQ_WHERE_CLAUSE_NOT_SUPPORTED, "LINQ where clause method not supported.");
			}
		}

		public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
		{
			base.VisitSelectClause(selectClause, queryModel);

			// possible building of fields modifiers
			if (selectClause.Selector.NodeType == ExpressionType.MemberAccess)
			{
				// single field
				var selectFields = selectClause.Selector as MemberExpression;
				string field = selectFields.Member.Name;
				builderMongoQuery.AddModifier("&fields=" + this.mapPropertyToName[field]);
			}
			else if (selectClause.Selector.NodeType == ExpressionType.New)
			{
				// anonymous type - multiple fields
				var selectFields = selectClause.Selector as NewExpression;
				var members = selectFields.Members;
				int? count = members?.Count;

				if (members != null &&
					count > 0)
				{
					string fieldsQuery = "&fields=";
					for (int i = 0; i < count; i++)
					{
						if (i > 0)
						{
							fieldsQuery += ",";
						}

						string field = members[i].Name;
						fieldsQuery += this.mapPropertyToName[field];
					}

					builderMongoQuery.AddModifier(fieldsQuery);
				}
			}
		}

		//	Methods available for override from Remotion:
		//
		//	public virtual void VisitAdditionalFromClause (AdditionalFromClause fromClause, QueryModel queryModel, int index);
		//
		//	public virtual void VisitGroupJoinClause (GroupJoinClause groupJoinClause, QueryModel queryModel, int index);
		//
		//	public virtual void VisitJoinClause (JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause);
		//
		//	public virtual void VisitJoinClause (JoinClause joinClause, QueryModel queryModel, int index);
		//
		//	public virtual void VisitMainFromClause (MainFromClause fromClause, QueryModel queryModel);
		//
		//	protected virtual void VisitOrderings (ObservableCollection<Ordering> orderings, QueryModel queryModel, OrderByClause orderByClause);
		//
		//	public virtual void VisitOrderByClause (OrderByClause orderByClause, QueryModel queryModel, int index);
		//
		//	public virtual void VisitResultOperator (ResultOperatorBase resultOperator, QueryModel queryModel, int index);
		//
		//	protected virtual void VisitResultOperators (ObservableCollection<ResultOperatorBase> resultOperators, QueryModel queryModel);
		//
		//	public virtual void VisitSelectClause (SelectClause selectClause, QueryModel queryModel);
	}
}
