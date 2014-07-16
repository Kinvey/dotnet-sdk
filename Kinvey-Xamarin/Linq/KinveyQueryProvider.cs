using System;
using System.Linq;
using System.Linq.Expressions;

namespace KinveyXamarin
{
	public class KinveyQueryProvider : IQueryProvider
	{
	
		#region IQueryProvider implementation

		public IQueryable CreateQuery(Expression expression)
		{
			Type elementType = TypeSystem.GetElementType(expression.Type);
			try
			{
				return (IQueryable)Activator.CreateInstance(typeof(QueryableKinveyData<>).MakeGenericType(elementType), new object[] { this, expression });
			}
			catch (System.Reflection.TargetInvocationException tie)
			{
				throw tie.InnerException;
			}
		}

		// Queryable's collection-returning standard query operators call this method. 
		public IQueryable<TResult> CreateQuery<TResult>(Expression expression)
		{
			return new QueryableKinveyData<TResult>(this, expression);
		}

		public object Execute(Expression expression)
		{
			return KinveyQueryContext.Execute(expression, false);
		}

		// Queryable's "single value" standard query operators call this method.
		// It is also called from QueryableTerraServerData.GetEnumerator(). 
		public TResult Execute<TResult>(Expression expression)
		{
			bool IsEnumerable = (typeof(TResult).Name == "IEnumerable`1");

			return (TResult)KinveyQueryContext.Execute(expression, IsEnumerable);
		}
		#endregion
	}
}

