using System;
using System.Linq.Expressions;

namespace KinveyXamarin
{
	/// <summary>
	/// Represents an order by clause.
	/// </summary>
	public class OrderByClause
	{
		// private fields
		private LambdaExpression _key;
		private OrderByDirection _direction;

		// constructors
		/// <summary>
		/// Initializes an instance of the OrderByClause class.
		/// </summary>
		/// <param name="key">An expression identifying the key of the order by clause.</param>
		/// <param name="direction">The direction of the order by clause.</param>
		public OrderByClause(LambdaExpression key, OrderByDirection direction)
		{
			_key = key;
			_direction = direction;
		}

		// public properties
		/// <summary>
		/// Gets the lambda expression identifying the key of the order by clause.
		/// </summary>
		public LambdaExpression Key
		{
			get { return _key; }
		}

		/// <summary>
		/// Gets the direction of the order by clause.
		/// </summary>
		public OrderByDirection Direction
		{
			get { return _direction; }
		}
	}

	public enum OrderByDirection{
		/// <summary>
		/// Ascending order.
		/// </summary>
		Ascending,
		/// <summary>
		/// Descending order.
		/// </summary>
		Descending
	}
}