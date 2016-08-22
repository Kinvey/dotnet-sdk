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
//using Remotion.Linq.Clauses.ExpressionTreeVisitors;
//using Newtonsoft.Json.Linq;

namespace KinveyXamarin
{
	public class KinveyQueryVisitor : QueryModelVisitorBase
	{
		private IQueryBuilder writer;
		private Dictionary<string, string> mapPropertyToName;
		public Expression cacheExpr { get; set; }

		public KinveyQueryVisitor(IQueryBuilder builder, Type type)
		{
			writer = builder;

			//			Type.GetTypeInfo (type).GetCustomAttributes(

			//var scratch = Activator.CreateInstance (type);

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

		public override void VisitQueryModel (QueryModel queryModel)
		{
			base.VisitQueryModel (queryModel);
		}

		protected override void VisitBodyClauses (ObservableCollection<IBodyClause> bodyClauses, QueryModel queryModel)
		{
			base.VisitBodyClauses (bodyClauses, queryModel);
			//Logger.Log ("visiting body clause");
		}

		protected override void VisitOrderings (ObservableCollection<Ordering> orderings, QueryModel queryModel, OrderByClause orderByClause)
		{
			base.VisitOrderings (orderings, queryModel, orderByClause);

			//Logger.Log ("visiting ordering clause");
			foreach (var ordering in orderings) {
				var member = ordering.Expression as MemberExpression;

				string sort = "&sort={\"" + mapPropertyToName[member.Member.Name] + "\":" + (ordering.OrderingDirection.ToString().Equals("Asc") ? "1" : "-1") + "}";
				writer.AddModifier (sort);

				//				Logger.Log (ordering.OrderingDirection);
			}
		}

		//		protected override void VisitResultOperators (ObservableCollection<ResultOperatorBase> resultOperators, QueryModel queryModel)
		//		{
		//			base.VisitResultOperators (resultOperators, queryModel);
		//			Logger.Log ("visiting result clauses:");
		//			foreach (var res in resultOperators) {
		//				Logger.Log (res.ToString ());
		//			}
		//		}

		public override void VisitResultOperator (ResultOperatorBase resultOperator, QueryModel queryModel, int index)
		{
			base.VisitResultOperator (resultOperator, queryModel, index);

			//Logger.Log ("visiting result clause:" + resultOperator.ToString ());
			if (resultOperator.ToString().Contains("Skip"))
			{
				SkipResultOperator skip = resultOperator as SkipResultOperator;
				//				Logger.Log (skip.Count);
				//				Logger.Log(skip.
				writer.AddModifier("&skip=" + skip.Count);
			}
			else if (resultOperator.ToString().Contains("Take"))
			{
				TakeResultOperator take = resultOperator as TakeResultOperator;
				writer.AddModifier("&limit=" + take.Count);
			}
			else
			{
				throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "LINQ result operator not supported.");
			}
		}

		public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
		{
			base.VisitWhereClause (whereClause, queryModel, index);
			cacheExpr = whereClause.Predicate;

			//Logger.Log ("visiting where clause: " + whereClause.Predicate.ToString());
			if (whereClause.Predicate.NodeType.ToString ().Equals ("Equal"))
			{
				BinaryExpression equality = whereClause.Predicate as BinaryExpression;
				var member = equality.Left as MemberExpression;

				writer.Write ("\"" + mapPropertyToName[member.Member.Name] + "\"");
				writer.Write (":");
				writer.Write (equality.Right);
			}
			else if (whereClause.Predicate.NodeType.ToString().Equals("AndAlso"))
			{
				BinaryExpression and = whereClause.Predicate as BinaryExpression;
				//recursively traverse tree
				//				var rightSide = and.Right as BinaryExpression;
				//				if (rightSide != null){
				//					
				//				}
				VisitWhereClause(new WhereClause(and.Right), queryModel, index);
				writer.Write (",");
				VisitWhereClause(new WhereClause(and.Left), queryModel, index);



				//				Logger.Log (and.Right.ToString());
				//				Logger.Log (and.Left.ToString());


			}
			else if (whereClause.Predicate.NodeType.ToString().Equals("OrElse"))
			{
				BinaryExpression or = whereClause.Predicate as BinaryExpression;

				writer.Write ("$or:");
				writer.Write("[");
				writer.Write ("{");
				VisitWhereClause (new WhereClause(or.Left), queryModel, index);
				writer.Write ("},");
				writer.Write ("{");
				VisitWhereClause (new WhereClause(or.Right), queryModel, index);
				writer.Write ("}");
				writer.Write ("]");


				//				Logger.Log (or.Right.ToString());
				//				Logger.Log (or.Left.ToString());

			}
			else if (whereClause.Predicate.NodeType == ExpressionType.Call)
			{
				MethodCallExpression b = whereClause.Predicate as MethodCallExpression;

				string name = (b.Object as MemberExpression).Member.Name.ToString();
				string propertyName = mapPropertyToName[name];
//				name = name.Replace("\"", "\\\"");

				string argument = b.Arguments[0].ToString().Trim('"');
				argument = argument.Replace("\"", "\\\"");

				if (b.Method.Name.ToString().Equals("StartsWith"))
				{
					writer.Write("\"" + propertyName + "\"");
					writer.Write(":{\"$regex\":\"^");
					writer.Write(argument);
					writer.Write("\"}");
				}
				else if (b.Method.Name.ToString().Equals("Equals"))
				{
					writer.Write("\"" + propertyName + "\"");
					writer.Write(":");
					writer.Write("\"" + argument + "\"");
				}
				else
				{
					throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "LINQ where clause method not supported.");
				}
			}
			else
			{
				throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "LINQ where clause method not supported.");
				//				Logger.Log (whereClause.Predicate);
				//				Logger.Log (whereClause.Predicate.NodeType.ToString());
			}
		}

		public override void VisitOrderByClause (OrderByClause orderByClause, QueryModel queryModel, int index)
		{
			base.VisitOrderByClause (orderByClause, queryModel, index);
			throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "LINQ OrderBy clause not supported.");
			//Logger.Log ("visiting orderby clause");
			//			foreach (var ordering in orderByClause.Orderings) {
			//				Logger.Log (ordering.Expression);
			//			}
		}

//		public virtual void VisitAdditionalFromClause (AdditionalFromClause fromClause, QueryModel queryModel, int index);
//
//		public virtual void VisitGroupJoinClause (GroupJoinClause groupJoinClause, QueryModel queryModel, int index);
//
//		public virtual void VisitJoinClause (JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause);
//
//		public virtual void VisitJoinClause (JoinClause joinClause, QueryModel queryModel, int index);
//
//		public virtual void VisitMainFromClause (MainFromClause fromClause, QueryModel queryModel);
//
//		protected virtual void VisitOrderings (ObservableCollection<Ordering> orderings, QueryModel queryModel, OrderByClause orderByClause);
//
//		public virtual void VisitResultOperator (ResultOperatorBase resultOperator, QueryModel queryModel, int index);
//
//		protected virtual void VisitResultOperators (ObservableCollection<ResultOperatorBase> resultOperators, QueryModel queryModel);
//
//		public virtual void VisitSelectClause (SelectClause selectClause, QueryModel queryModel);

	}
}
