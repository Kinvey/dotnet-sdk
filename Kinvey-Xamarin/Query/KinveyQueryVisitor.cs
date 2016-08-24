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

namespace KinveyXamarin
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

				builderMongoQuery.Write ("\"" + mapPropertyToName[member.Member.Name] + "\"");
				builderMongoQuery.Write (":");
				builderMongoQuery.Write (equality.Right);
			}
			else if (whereClause.Predicate.NodeType.ToString().Equals("AndAlso"))
			{
				BinaryExpression and = whereClause.Predicate as BinaryExpression;
				VisitWhereClause(new WhereClause(and.Right), queryModel, index);
				builderMongoQuery.Write (",");
				VisitWhereClause(new WhereClause(and.Left), queryModel, index);
			}
			else if (whereClause.Predicate.NodeType.ToString().Equals("OrElse"))
			{
				BinaryExpression or = whereClause.Predicate as BinaryExpression;

				builderMongoQuery.Write ("\"$or\":");
				builderMongoQuery.Write("[");
				builderMongoQuery.Write ("{");
				VisitWhereClause (new WhereClause(or.Left), queryModel, index);
				builderMongoQuery.Write ("},");
				builderMongoQuery.Write ("{");
				VisitWhereClause (new WhereClause(or.Right), queryModel, index);
				builderMongoQuery.Write ("}");
				builderMongoQuery.Write ("]");
			}
			else if (whereClause.Predicate.NodeType == ExpressionType.Call)
			{
				MethodCallExpression b = whereClause.Predicate as MethodCallExpression;

				string name = (b.Object as MemberExpression).Member.Name;
				string propertyName = mapPropertyToName[name];
				//name = name.Replace("\"", "\\\"");

				string argument = b.Arguments[0].ToString().Trim('"');
				argument = argument.Replace("\"", "\\\"");

				if (b.Method.Name.Equals("StartsWith"))
				{
					if (index > 0)
					{
						// multiple where clauses present, so separate with comma
						builderMongoQuery.Write(",");
					}

					builderMongoQuery.Write("\"" + propertyName + "\"");
					builderMongoQuery.Write(":{\"$regex\":\"^");
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
					builderMongoQuery.Write("\"" + argument + "\"");
				}
				else
				{
					throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "LINQ where clause method not supported.");
				}
			}
			else
			{
				throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "LINQ where clause method not supported.");
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
