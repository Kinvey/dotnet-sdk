// Copyright (c) 2019, Kinvey, Inc. All rights reserved.
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

using Remotion.Linq;
using System.Collections.Generic;

namespace Kinvey
{
    /// <summary>
    /// Constitutes the bridge between re-linq and a concrete query provider implementation.
    /// </summary>
    /// <typeparam name="K">The type of the result items yielded by this query.</typeparam>
    public class KinveyQueryExecutor<K> : IQueryExecutor
	{
        /// <summary>
        /// Builds the mongo query corresponding to the LINQ query.
        /// </summary>
        /// <value>String query builder.</value>
        public StringQueryBuilder writer;

        /// <summary>
        /// Kinvey queryable instance used as an entry point (the main data source) of a LINQ query.
        /// </summary>
        /// <value>The instance of the <see cref="KinveyQueryable{T}"/>.</value>
        public KinveyQueryable<K> queryable;

        /// <summary>
        /// Executes the given queryModel as a collection query, i.e. as a query returning objects of type T.
        /// </summary>
        /// <param name="queryModel"> The <see cref="QueryModel"/> representing the query to be executed. </param>
        /// <returns> An instance implementing the <see cref="IEnumerable{T}"/> interface  that represents the query's result. </returns>
        /// <typeparam name="T">The type of the items returned by the query.</typeparam>
        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
		{
			throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "ExecuteCollection<T> method on KinveyQueryExecutor not implemented.");
		}

        /// <summary>
        /// Executes the given queryModel as a single object query, i.e. as a query returning a single object of type T.
        /// </summary>
        /// <param name="queryModel"> The <see cref="QueryModel"/> representing the query to be executed. </param>
        /// <param name="returnDefaultWhenEmpty"> If <c> true </c>, the executor must return a default value when its result set is empty;
        /// if  <c> false </c>, it should throw an <see cref="System.InvalidOperationException"/> when its result set is empty.</param>
        /// <returns> A single value of type T that represents the query's result. </returns>
        /// <typeparam name="T">The type of the single value returned by the query.</typeparam>
        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
		{
			throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "ExecuteSingle<T> method on KinveyQueryExecutor not implemented.");
		}

        /// <summary>
        /// Executes the given queryModel as a scalar query, i.e. as a query returning a scalar value of type T.
        /// </summary>
        /// <param name="queryModel"> The <see cref="QueryModel"/> representing the query to be executed. </param>
        /// <returns> A scalar value of type T that represents the query's result. </returns>
        /// <typeparam name="T">The type of the scalar value returned by the query.</typeparam>
        public T ExecuteScalar<T>(QueryModel queryModel)
		{
			throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "ExecuteScalar<T> method on KinveyQueryExecutor not implemented.");
		}
	}
}
