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
using System.Collections.Generic;
using System.Linq.Expressions;
using Kinvey.DotNet.Framework.Core;
using System.Linq;
using System.Reflection;

namespace KinveyXamarin
{
	public class MongoQueryProvider : IQueryProvider
	{
		public string Collection;

		public MongoQueryProvider (string Collection)
		{
			this.Collection = Collection;
		}

		/// <summary>
		/// Executes a query. Calls the generic method Execute{{T}} to actually execute the query.
		/// </summary>
		/// <param name="expression">The query expression.</param>
		/// <returns>The result of the query.</returns>
		public T Execute<T>(Expression expression)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
	

			var result = Execute(expression);
			if (result == null)
			{
				return default(T);
			}
			else
			{
				return (T)result;
			}
		}


		public IMongoQuery BuildMongoQuery<T> (AppData<T> query)
		{
			var translatedQuery = MongoQueryTranslator.Translate(this, ((IQueryable)query).Expression);
//			return ((SelectQuery)translatedQuery).BuildQuery();
			return null;
		}

		public IQueryable CreateQuery (Expression expression)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			//var elementType = TypeHelper.GetElementType(expression.Type);
			try
			{
				var queryableType = typeof(AppData<>).MakeGenericType(expression.Type);
				return (IQueryable)Activator.CreateInstance(queryableType, new object[] { this, expression });
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}

		public IQueryable<T> CreateQuery<T> (Expression expression)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
		
			return null;
		}

		public Object Execute (Expression expression)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}

			var translatedQuery = MongoQueryTranslator.Translate(this, expression);
			return translatedQuery.Execute(); 
		}
	}
}