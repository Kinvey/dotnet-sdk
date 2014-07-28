using System;
using System.Linq.Expressions;

namespace KinveyXamarin
{
	public class PredicateTranslator
	{
		// private fields
		private readonly BsonSerializationInfoHelper _serializationInfoHelper;

		// constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="PredicateTranslator"/> class.
		/// </summary>
		/// <param name="serializationHelper">The serialization helper.</param>
		public PredicateTranslator(BsonSerializationInfoHelper serializationHelper)
		{
			_serializationInfoHelper = serializationHelper;
		}

		// public methods
		/// <summary>
		/// Builds an IMongoQuery from an expression.
		/// </summary>
		/// <param name="expression">The expression.</param>
		/// <returns>An IMongoQuery.</returns>
//		public IMongoQuery BuildQuery(Expression expression)
//		{
//			IMongoQuery query = null;
//
//			switch (expression.NodeType)
//			{
//			case ExpressionType.And:
//				query = BuildAndQuery((BinaryExpression)expression);
//				break;
//			case ExpressionType.AndAlso:
//				query = BuildAndAlsoQuery((BinaryExpression)expression);
//				break;
//			case ExpressionType.ArrayIndex:
//				query = BuildBooleanQuery(expression);
//				break;
//			case ExpressionType.Call:
//				query = BuildMethodCallQuery((MethodCallExpression)expression);
//				break;
//			case ExpressionType.Constant:
//				query = BuildConstantQuery((ConstantExpression)expression);
//				break;
//			case ExpressionType.Equal:
//			case ExpressionType.GreaterThan:
//			case ExpressionType.GreaterThanOrEqual:
//			case ExpressionType.LessThan:
//			case ExpressionType.LessThanOrEqual:
//			case ExpressionType.NotEqual:
//				query = BuildComparisonQuery((BinaryExpression)expression);
//				break;
//			case ExpressionType.MemberAccess:
//				query = BuildBooleanQuery(expression);
//				break;
//			case ExpressionType.Not:
//				query = BuildNotQuery((UnaryExpression)expression);
//				break;
//			case ExpressionType.Or:
//				query = BuildOrQuery((BinaryExpression)expression);
//				break;
//			case ExpressionType.OrElse:
//				query = BuildOrElseQuery((BinaryExpression)expression);
//				break;
//			case ExpressionType.TypeIs:
//				query = BuildTypeIsQuery((TypeBinaryExpression)expression);
//				break;
//			}
//
//			if (query == null)
//			{
//				var message = string.Format("Unsupported where clause: {0}.", ExpressionFormatter.ToString(expression));
//				throw new ArgumentException(message);
//			}
//
//			return query;
//		}
	}
}

