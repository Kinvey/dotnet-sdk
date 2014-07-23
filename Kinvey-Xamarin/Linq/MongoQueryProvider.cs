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

namespace KinveyXamarin
{
	public class MongoQueryProvider<T>
	{
		public string Collection;

		public MongoQueryProvider ()
		{
		}

		/// <summary>
		/// Executes a query. Calls the generic method Execute{{T}} to actually execute the query.
		/// </summary>
		/// <param name="expression">The query expression.</param>
		/// <returns>The result of the query.</returns>
		public object Execute(Expression expression)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}

			var translatedQuery = MongoQueryTranslator.Translate(this, expression);
			return translatedQuery.Execute();
		}

		public IMongoQuery BuildMongoQuery (AppData<T> query)
		{
			var translatedQuery = MongoQueryTranslator.Translate(this, ((IQueryable)query).Expression);
			return ((SelectQuery)translatedQuery).BuildQuery();
		}
	}
}

