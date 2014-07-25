using System;
using System.Linq.Expressions;

namespace KinveyXamarin
{
	/// <summary>
	/// A class that replaces all occurences of one parameter with a different parameter.
	/// </summary>
	public class ExpressionParameterReplacer : ExpressionVisitor
	{
		// private fields
		private ParameterExpression _fromParameter;
		private Expression _toExpression;

		// constructors
		/// <summary>
		/// Initializes a new instance of the ExpressionParameterReplacer class.
		/// </summary>
		/// <param name="fromParameter">The parameter to be replaced.</param>
		/// <param name="toExpression">The expression that replaces the parameter.</param>
		public ExpressionParameterReplacer (ParameterExpression fromParameter, Expression toExpression)
		{
			_fromParameter = fromParameter;
			_toExpression = toExpression;
		}

		// public methods
		/// <summary>
		/// Replaces all occurences of one parameter with a different parameter.
		/// </summary>
		/// <param name="node">The expression containing the parameter that should be replaced.</param>
		/// <param name="fromParameter">The from parameter.</param>
		/// <param name="toExpression">The expression that replaces the parameter.</param>
		/// <returns>The expression with all occurrences of the parameter replaced.</returns>
		public static Expression ReplaceParameter (Expression node, ParameterExpression fromParameter, Expression toExpression)
		{
			var replacer = new ExpressionParameterReplacer (fromParameter, toExpression);
			return replacer.Visit (node);
		}

		/// <summary>
		/// Replaces the from parameter with the two parameter if it maches.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <returns>The parameter (replaced if it matched).</returns>
		protected override Expression VisitParameter (ParameterExpression node)
		{
			if (node == _fromParameter) {
				return _toExpression;
			}
			return node;
		}
	}
}