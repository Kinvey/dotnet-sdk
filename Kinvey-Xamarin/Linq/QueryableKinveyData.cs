using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace KinveyXamarin
{
	public class QueryableKinveyData<T> : IOrderedQueryable<T>
	{

		public IQueryProvider Provider { get; private set; }
		public Expression Expression { get; private set; }

		public QueryableKinveyData ()
		{
			Provider = new KinveyQueryProvider();
			Expression = Expression.Constant(this);
		}

		public QueryableKinveyData(KinveyQueryProvider provider, Expression expression){

			if (provider == null)
			{
				throw new ArgumentNullException("provider");
			}

			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}

			Provider = provider;
			Expression = expression;
		}

		public Type ElementType
		{
			get { return typeof(T); }
		}

		public IEnumerator<T> GetEnumerator()
		{
			return (Provider.Execute<IEnumerable<T>>(Expression)).GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return (Provider.Execute<System.Collections.IEnumerable>(Expression)).GetEnumerator();
		}

	}

}

