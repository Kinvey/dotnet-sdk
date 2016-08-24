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

namespace KinveyXamarin
{
	/// <summary>
	/// Kinvey queryable base class.  Used to provide access to LINQ queries in order to process them.
	/// </summary>
	public class KinveyQueryable<T> : QueryableBase<T>
	{
		/// <summary>
		/// Builds the mongo query corresponding to the LINQ query.
		/// </summary>
		public StringQueryBuilder writer;

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KinveyQueryable{T}"/> class.
		/// </summary>
		/// <param name="queryProvider">My query provider.</param>
		/// <param name="myClass">My class.</param>
		public KinveyQueryable(KinveyQueryProvider queryProvider, Type myClass)
			: base(queryProvider)
		{
			var kExecutor = queryProvider.Executor as KinveyQueryExecutor<T>;

			if (kExecutor != null)
			{
				writer = new StringQueryBuilder ();
				kExecutor.writer = writer;
				kExecutor.queryable = this;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KinveyQueryable{T}"/> class.
		/// </summary>
		/// <param name="provider">Provider.</param>
		/// <param name="expression">Expression.</param>
		public KinveyQueryable(IQueryProvider provider, Expression expression)
			: base(provider, expression)
		{
		}

		///// <summary>
		///// Executes the query.
		///// </summary>
		///// <returns>The query.</returns>
		///// <param name="query">Mongo-style query to be executed on backend.</param>
		//public virtual object executeQuery(string query)
		//{
		//	Logger.Log ("can't execute a query without overriding this method!");
		//	return default(object);
		//}

		///// <summary>
		///// Executes the query on cache.
		///// </summary>
		///// <returns>The query on cache.</returns>
		//public virtual object executeQueryOnCache()
		//{
		//	Logger.Log ("can't execute a query on cache without overriding this method!");
		//	return default(object);
		//}
	}
}
