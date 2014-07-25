/* Copyright 2010-2013 10gen Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections;
using Kinvey.DotNet.Framework.Core;
using Kinvey.DotNet.Framework;
using System.Collections.ObjectModel;
using System.Linq;

namespace KinveyXamarin
{
	public class SelectQuery : TranslatedQuery
	{
		private LambdaExpression _where;
		private Type _ofType;
		private List<OrderByClause> _orderBy;
		private LambdaExpression _projection;
		private int? _skip;
		private int? _take;
		private Func<IEnumerable, object> _elementSelector;
		// used for First, Last, etc...
		private LambdaExpression _distinct;
		private Expression _lastExpression;

		public SelectQuery (string Collection, Type type) : base (Collection, type)
		{
		}

		public void Translate (Expression expression)
		{
			// when we reach the original MongoQueryable<TDocument> we're done
			var constantExpression = expression as ConstantExpression;
			if (constantExpression != null) {
				if (constantExpression.Type == typeof(AppData<>).MakeGenericType (DocumentType)) {
					return;
				}
			}

			var methodCallExpression = expression as MethodCallExpression;
			if (methodCallExpression != null) {
				TranslateMethodCall (methodCallExpression);
				return;
			}

			var message = string.Format ("Don't know how to translate expression: {0}.", expression.ToString ());
			throw new NotSupportedException (message);
		}

		public IMongoQuery BuildQuery ()
		{
			throw new NotImplementedException ();
		}

		private void TranslateMethodCall (MethodCallExpression methodCallExpression)
		{
			if (methodCallExpression.Arguments.Count == 0) {
				var message = string.Format ("Method call expression has no arguments: {0}.", methodCallExpression.ToString ());
				throw new ArgumentOutOfRangeException ("methodCallExpression", message);
			}

			var source = methodCallExpression.Arguments [0];
			Translate (source);
			_lastExpression = source;

			if (_distinct != null) {
				var message = "No further operators may follow Distinct in a LINQ query.";
				throw new NotSupportedException (message);
			}

			var methodName = methodCallExpression.Method.Name;
			switch (methodName) {
			case "Any":
				TranslateAny (methodCallExpression);
				break;
			case "Count":
			case "LongCount":
				TranslateCount (methodCallExpression);
				break;
			case "Distinct":
				TranslateDistinct (methodCallExpression);
				break;
			case "ElementAt":
			case "ElementAtOrDefault":
				TranslateElementAt (methodCallExpression);
				break;
			case "First":
			case "FirstOrDefault":
			case "Single":
			case "SingleOrDefault":
				TranslateFirstOrSingle (methodCallExpression);
				break;
			case "Last":
			case "LastOrDefault":
				TranslateLast (methodCallExpression);
				break;
			case "Max":
			case "Min":
				TranslateMaxMin (methodCallExpression);
				break;
			case "OfType":
				break;
			case "OrderBy":
			case "OrderByDescending":
				TranslateOrderBy (methodCallExpression);
				break;
			case "Select":
				TranslateSelect (methodCallExpression);
				break;
			case "Skip":
				TranslateSkip (methodCallExpression);
				break;
			case "Take":
				TranslateTake (methodCallExpression);
				break;
			case "ThenBy":
			case "ThenByDescending":
				TranslateThenBy (methodCallExpression);
				break;
			case "WithIndex":

				break;
			case "Where":
				TranslateWhere (methodCallExpression);
				break;
			default:
				var message = string.Format ("The {0} query operator is not supported.", methodName);
				throw new NotSupportedException (message);
			}
		}

		private void TranslateAny (MethodCallExpression methodCallExpression)
		{
			LambdaExpression predicate = null;
			switch (methodCallExpression.Arguments.Count) {
			case 1:
				break;
			case 2:
				predicate = (LambdaExpression)StripQuote (methodCallExpression.Arguments [1]);
				break;
			default:
				throw new ArgumentOutOfRangeException ("methodCallExpression");
			}
			CombinePredicateWithWhereClause (methodCallExpression, predicate);

			// ignore any projection since we only are interested in the count
			_projection = null;

			// note: recall that cursor method Size respects Skip and Limit while Count does not
			SetElementSelector (methodCallExpression, source => ((int)((IProjector)source).Cursor.Size ()) > 0);
		}


		private void TranslateCount(MethodCallExpression methodCallExpression)
		{
			LambdaExpression predicate = null;
			switch (methodCallExpression.Arguments.Count)
			{
			case 1:
				break;
			case 2:
				predicate = (LambdaExpression)StripQuote(methodCallExpression.Arguments[1]);
				break;
			default:
				throw new ArgumentOutOfRangeException("methodCallExpression");
			}
			CombinePredicateWithWhereClause(methodCallExpression, predicate);

			// ignore any projection since we only are interested in the count
			_projection = null;

			// note: recall that cursor method Size respects Skip and Limit while Count does not
			switch (methodCallExpression.Method.Name)
			{
			case "Count":
				SetElementSelector(methodCallExpression, source => (int)((IProjector)source).Cursor.Size());
				break;
			case "LongCount":
				SetElementSelector(methodCallExpression, source => ((IProjector)source).Cursor.Size());
				break;
			}
		}


		private void TranslateDistinct(MethodCallExpression methodCallExpression)
		{
			var arguments = methodCallExpression.Arguments.ToArray();
			if (arguments.Length != 1)
			{
				var message = "The version of the Distinct query operator with an equality comparer is not supported.";
				throw new NotSupportedException(message);
			}

			if (_projection == null)
			{
				var message = "Distinct must be used with Select to identify the field whose distinct values are to be found.";
				throw new NotSupportedException(message);
			}

//			if (_indexHint != null)
//			{
//				var message = "Distinct cannot be used together with WithIndex.";
//				throw new NotSupportedException(message);
//			}

			_distinct = _projection;
			_projection = null;
		}

		private void TranslateElementAt(MethodCallExpression methodCallExpression)
		{
			if (methodCallExpression.Arguments.Count != 2)
			{
				throw new ArgumentOutOfRangeException("methodCallExpression");
			}

			// ElementAt can be implemented more efficiently in terms of Skip, Limit and First
			var index = ToInt32(methodCallExpression.Arguments[1]);
			_skip = index;
			_take = 1;

			switch (methodCallExpression.Method.Name)
			{
			case "ElementAt":
				SetElementSelector(methodCallExpression, source => source.Cast<object>().First());
				break;
			case "ElementAtOrDefault":
				SetElementSelector(methodCallExpression, source => source.Cast<object>().FirstOrDefault());
				break;
			}
		}


		private void TranslateFirstOrSingle(MethodCallExpression methodCallExpression)
		{
			LambdaExpression predicate = null;
			switch (methodCallExpression.Arguments.Count)
			{
			case 1:
				break;
			case 2:
				predicate = (LambdaExpression)StripQuote(methodCallExpression.Arguments[1]);
				break;
			default:
				throw new ArgumentOutOfRangeException("methodCallExpression");
			}
			CombinePredicateWithWhereClause(methodCallExpression, predicate);

			switch (methodCallExpression.Method.Name)
			{
			case "First":
				_take = 1;
				SetElementSelector(methodCallExpression, source => source.Cast<object>().First());
				break;
			case "FirstOrDefault":
				_take = 1;
				SetElementSelector(methodCallExpression, source => source.Cast<object>().FirstOrDefault());
				break;
			case "Single":
				_take = 2;
				SetElementSelector(methodCallExpression, source => source.Cast<object>().Single());
				break;
			case "SingleOrDefault":
				_take = 2;
				SetElementSelector(methodCallExpression, source => source.Cast<object>().SingleOrDefault());
				break;
			}
		}

		private void TranslateLast(MethodCallExpression methodCallExpression)
		{
			LambdaExpression predicate = null;
			switch (methodCallExpression.Arguments.Count)
			{
			case 1:
				break;
			case 2:
				predicate = (LambdaExpression)StripQuote(methodCallExpression.Arguments[1]);
				break;
			default:
				throw new ArgumentOutOfRangeException("methodCallExpression");
			}
			CombinePredicateWithWhereClause(methodCallExpression, predicate);

			// when using OrderBy without Take Last can be much faster by reversing the sort order and using First instead of Last
			if (_orderBy != null && _take == null)
			{
				for (int i = 0; i < _orderBy.Count; i++)
				{
					var clause = _orderBy[i];
					var oppositeDirection = (clause.Direction == OrderByDirection.Descending) ? OrderByDirection.Ascending : OrderByDirection.Descending;
					_orderBy[i] = new OrderByClause(clause.Key, oppositeDirection);
				}
				_take = 1;

				switch (methodCallExpression.Method.Name)
				{
				case "Last":
					SetElementSelector(methodCallExpression, source => source.Cast<object>().First());
					break;
				case "LastOrDefault":
					SetElementSelector(methodCallExpression, source => source.Cast<object>().FirstOrDefault());
					break;
				}
			}
			else
			{
				switch (methodCallExpression.Method.Name)
				{
				case "Last":
					SetElementSelector(methodCallExpression, source => source.Cast<object>().Last());
					break;
				case "LastOrDefault":
					SetElementSelector(methodCallExpression, source => source.Cast<object>().LastOrDefault());
					break;
				}
			}
		}


		private void TranslateMaxMin(MethodCallExpression methodCallExpression)
		{
			var methodName = methodCallExpression.Method.Name;

			if (_orderBy != null)
			{
				var message = string.Format("{0} cannot be used with OrderBy.", methodName);
				throw new NotSupportedException(message);
			}
			if (_skip != null || _take != null)
			{
				var message = string.Format("{0} cannot be used with Skip or Take.", methodName);
				throw new NotSupportedException(message);
			}

			switch (methodCallExpression.Arguments.Count)
			{
			case 1:
				break;
			case 2:
				if (_projection != null)
				{
					var message = string.Format("{0} must be used with either Select or a selector argument, but not both.", methodName);
					throw new NotSupportedException(message);
				}
				_projection = (LambdaExpression)StripQuote(methodCallExpression.Arguments[1]);
				break;
			default:
				throw new ArgumentOutOfRangeException("methodCallExpression");
			}
			if (_projection == null)
			{
				var message = string.Format("{0} must be used with either Select or a selector argument.", methodName);
				throw new NotSupportedException(message);
			}

			// implement Max/Min by sorting on the relevant field(s) and taking the first result
			_orderBy = new List<OrderByClause>();
			if (_projection.Body.NodeType == ExpressionType.New)
			{
				// take the individual constructor arguments and make new lambdas out of them for the OrderByClauses
				var newExpression = (NewExpression)_projection.Body;
				foreach (var keyExpression in newExpression.Arguments)
				{
					var delegateTypeGenericDefinition = typeof(Func<,>);
					var delegateType = delegateTypeGenericDefinition.MakeGenericType(_projection.Parameters[0].Type, keyExpression.Type);
					var keyLambda = Expression.Lambda(delegateType, keyExpression, _projection.Parameters);
					var clause = new OrderByClause(keyLambda, (methodName == "Min") ? OrderByDirection.Ascending : OrderByDirection.Descending);
					_orderBy.Add(clause);
				}
			}
			else
			{
				var clause = new OrderByClause(_projection, (methodName == "Min") ? OrderByDirection.Ascending : OrderByDirection.Descending);
				_orderBy.Add(clause);
			}

			_take = 1;
			SetElementSelector(methodCallExpression, source => source.Cast<object>().First());
		}


	


		private void TranslateOrderBy(MethodCallExpression methodCallExpression)
		{
			if (methodCallExpression.Arguments.Count != 2)
			{
				throw new ArgumentOutOfRangeException("methodCallExpression");
			}

			if (_orderBy != null)
			{
				throw new NotSupportedException("Only one OrderBy or OrderByDescending clause is allowed (use ThenBy or ThenByDescending for multiple order by clauses).");
			}

			var key = (LambdaExpression)StripQuote(methodCallExpression.Arguments[1]);
			var direction = (methodCallExpression.Method.Name == "OrderByDescending") ? OrderByDirection.Descending : OrderByDirection.Ascending;
			var clause = new OrderByClause(key, direction);

			_orderBy = new List<OrderByClause>();
			_orderBy.Add(clause);
		}

		private void TranslateSelect(MethodCallExpression methodCallExpression)
		{
			if (methodCallExpression.Arguments.Count != 2)
			{
				throw new ArgumentOutOfRangeException("methodCallExpression");
			}

			var lambdaExpression = (LambdaExpression)StripQuote(methodCallExpression.Arguments[1]);
			if (lambdaExpression.Parameters.Count == 2)
			{
				var message = "The indexed version of the Select query operator is not supported.";
				throw new NotSupportedException(message);
			}
			if (lambdaExpression.Parameters.Count != 1)
			{
				throw new ArgumentOutOfRangeException("expression");
			}
			// ignore trivial projections of the form: d => d
			if (lambdaExpression.Body == lambdaExpression.Parameters[0])
			{
				return;
			}
			_projection = lambdaExpression;
		}

		private void TranslateSkip(MethodCallExpression methodCallExpression)
		{
			if (methodCallExpression.Arguments.Count != 2)
			{
				throw new ArgumentOutOfRangeException("methodCallExpression");
			}

			if (_skip.HasValue || _take.HasValue)
			{
				EnsurePreviousExpressionIsSkipOrTake();
			}

			var value = ToInt32(StripQuote(methodCallExpression.Arguments[1]));

			if (_take.HasValue)
			{
				if (value > _take.Value)
				{
					_skip = null;
					_take = 0;
					return;
				}

				_take = Math.Max(0, _take.Value - value);
			}

			if (_skip.HasValue)
			{
				value += _skip.Value;
			}

			_skip = value;
		}

		private void TranslateTake(MethodCallExpression methodCallExpression)
		{
			if (methodCallExpression.Arguments.Count != 2)
			{
				throw new ArgumentOutOfRangeException("methodCallExpression");
			}

			if (_skip.HasValue || _take.HasValue)
			{
				EnsurePreviousExpressionIsSkipOrTake();
			}

			var value = ToInt32(StripQuote(methodCallExpression.Arguments[1]));

			if (_take.HasValue && value > _take.Value)
			{
				value = _take.Value;
			}

			_take = value;
		}

		private void TranslateThenBy(MethodCallExpression methodCallExpression)
		{
			if (methodCallExpression.Arguments.Count != 2)
			{
				throw new ArgumentOutOfRangeException("methodCallExpression");
			}

			if (_orderBy == null)
			{
				throw new NotSupportedException("ThenBy or ThenByDescending can only be used after OrderBy or OrderByDescending.");
			}

			var key = (LambdaExpression)StripQuote(methodCallExpression.Arguments[1]);
			var direction = (methodCallExpression.Method.Name == "ThenByDescending") ? OrderByDirection.Descending : OrderByDirection.Ascending;
			var clause = new OrderByClause(key, direction);

			_orderBy.Add(clause);
		}



		private void TranslateWhere(MethodCallExpression methodCallExpression)
		{
			if (methodCallExpression.Arguments.Count != 2)
			{
				throw new ArgumentOutOfRangeException("methodCallExpression");
			}

			var predicate = (LambdaExpression)StripQuote(methodCallExpression.Arguments[1]);
			if (predicate.Parameters.Count == 2)
			{
				var message = "The indexed version of the Where query operator is not supported.";
				throw new NotSupportedException(message);
			}

			CombinePredicateWithWhereClause(methodCallExpression, predicate);
		}





		//Helpers



		private void CombinePredicateWithWhereClause (MethodCallExpression methodCallExpression, LambdaExpression predicate)
		{
			if (predicate != null) {
				if (_projection != null) {
					var message = string.Format ("{0} with predicate after a projection is not supported.", methodCallExpression.Method.Name);
					throw new NotSupportedException (message);
				}

				if (_where == null) {
					_where = predicate;
					return;
				}

				if (_where.Parameters.Count != 1) {
					throw new KinveyException ("Where lambda expression should have one parameter.");
				}
				var whereBody = _where.Body;
				var whereParameter = _where.Parameters [0];

				if (predicate.Parameters.Count != 1) {
					throw new KinveyException ("Predicate lambda expression should have one parameter.");
				}
				var predicateBody = predicate.Body;
				var predicateParameter = predicate.Parameters [0];

				// when using OfType the parameter types might not match (but they do have to be compatible)

				//TODO not sure about all this
				//if (predicateParameter.getType().getTypeInfo().IsAssignableFrom (whereParameter.Type)) {
				predicateBody = ExpressionParameterReplacer.ReplaceParameter (predicateBody, predicateParameter, whereParameter);
				var	parameter = whereParameter;
				//} else if (whereParameter.Type.IsAssignableFrom (predicateParameter.Type)) {
				//	whereBody = ExpressionParameterReplacer.ReplaceParameter (whereBody, whereParameter, predicateParameter);
				//	parameter = predicateParameter;
				//} else {
				//	throw new NotSupportedException ("Can't combine existing where clause with new predicate because parameter types are incompatible.");
				//}

				var combinedBody = Expression.AndAlso (whereBody, predicateBody);
				_where = Expression.Lambda (combinedBody, parameter);
			}
		}

		private void SetElementSelector(MethodCallExpression methodCallExpression, Func<IEnumerable, object> elementSelector)
		{
			if (_elementSelector != null)
			{
				var message = string.Format("{0} cannot be combined with any other element selector.", methodCallExpression.Method.Name);
				throw new NotSupportedException(message);
			}
			_elementSelector = elementSelector;
		}

		private Expression StripQuote(Expression expression)
		{
			if (expression.NodeType == ExpressionType.Quote)
			{
				return ((UnaryExpression)expression).Operand;
			}
			return expression;
		}


		private int ToInt32(Expression expression)
		{
			if (expression.Type != typeof(int))
			{
				throw new ArgumentOutOfRangeException("expression", "Expected an Expression of Type Int32.");
			}

			var constantExpression = expression as ConstantExpression;
			if (constantExpression == null)
			{
				throw new ArgumentOutOfRangeException("expression", "Expected a ConstantExpression.");
			}

			return (int)constantExpression.Value;
		}
		private void EnsurePreviousExpressionIsSkipOrTake()
		{
			var lastExpressionAsMethodCall = _lastExpression as MethodCallExpression;
			if (lastExpressionAsMethodCall == null || (lastExpressionAsMethodCall.Method.Name != "Skip" && lastExpressionAsMethodCall.Method.Name != "Take"))
			{
				throw new KinveyException("Skip and Take may only be used in conjunction with each other and cannot be separated by other operations.");
			}
		}
	}
}

