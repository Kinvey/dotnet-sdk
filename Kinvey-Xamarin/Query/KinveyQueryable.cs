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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;
using KinveyUtils;

namespace KinveyXamarin
{
	public class KinveyQueryable<T> : QueryableBase<T>
	{
		public StringQueryBuilder writer;
		static public Expression express;  // TODO find a way to not use a static class variable to capture query Expression

		public KinveyQueryable(IQueryParser queryParser, IQueryExecutor executor, Type myClass)
			: base(new DefaultQueryProvider(typeof(KinveyQueryable<>), queryParser, executor))
		{
			var kExecutor = executor as KinveyQueryExecutor<T>;
			if (kExecutor != null) {
				writer = new StringQueryBuilder ();
				kExecutor.writer = writer;
				kExecutor.queryable = this;
			}

		}

		public KinveyQueryable(IQueryProvider provider, Expression expression)
			: base(provider, expression)
		{
			express = expression;
		}

		/// <summary>
		/// Executes the query.
		/// </summary>
		/// <returns>The query.</returns>
		/// <param name="query">Mongo-style query to be executed on backend.</param>
		public virtual object executeQuery(string query)
		{
			Logger.Log ("can't execute a query without overriding this method!");
			return default(object);
		}

		/// <summary>
		/// Executes the query on cache.
		/// </summary>
		/// <returns>The query on cache.</returns>
		/// <param name="expr">Query expression to be executed on cache.</param>
		public virtual object executeQueryOnCache(Expression expr)
		{
			Logger.Log ("can't execute a query on cache without overriding this method!");
			return default(object);
		}
	}
}
