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
using System.Linq;
using System.Linq.Expressions;

namespace KinveyXamarin
{
	/// <summary>
	/// A translator from LINQ expression queries to Mongo queries.
	/// </summary>
	public static class MongoQueryTranslator
	{

		// public static methods
		/// <summary>
		/// Translate a MongoDB LINQ query.
		/// </summary>
		/// <param name="query">The MongoDB LINQ query.</param>
		/// <returns>A TranslatedQuery.</returns>
		public static TranslatedQuery Translate(IQueryable query)
		{
			return Translate((MongoQueryProvider)query.Provider, query.Expression);
		}

		/// <summary>
		/// Translate a MongoDB LINQ query.
		/// </summary>
		/// <param name="provider">The MongoDB query provider.</param>
		/// <param name="expression">The LINQ query expression.</param>
		/// <returns>A TranslatedQuery.</returns>
		public static TranslatedQuery Translate(MongoQueryProvider provider, Expression expression)
		{
			expression = PartialEvaluator.Evaluate(expression, provider);
			expression = ExpressionNormalizer.Normalize(expression);
			// assume for now it's a SelectQuery
			Type documentType = typeof(Expression);
			var selectQuery = new SelectQuery(provider.Collection, documentType);
			selectQuery.Translate(expression);
			return selectQuery;
		}


	}
}

