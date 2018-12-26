// Copyright (c) 2018, Kinvey, Inc. All rights reserved.
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
using System.Linq;

namespace Kinvey
{
    /// <summary>
    /// The KinveyMongoQueryBuilder class provides methods to work with Mongo queries.
    /// </summary>
    internal static class KinveyMongoQueryBuilder
    {
        /// <summary>
        /// Builds the mongo-style query string for find operation to be run against the backend.
        /// </summary>
        /// <param name="query">LINQ-style query that can be used to filter delete results.</param>
        /// <returns>The mongo-style query string.</returns>
        internal static string GetQueryForFindOperation<T>(IQueryable<object> query)
        {
            return GetQuery<T>(query, VisitorClause.Order | VisitorClause.SkipTake | VisitorClause.Where | VisitorClause.Select);
        }

        /// <summary>
        ///  Builds the mongo-style query string for remove operation to be run against the backend.
        /// </summary>
        /// <param name="query">LINQ-style query that can be used to filter delete results.</param>
        /// <returns>The mongo-style query string.</returns>
        internal static string GetQueryForRemoveOperation<T>(IQueryable<object> query)
        {
            var mongoQuery = GetQuery<T>(query, VisitorClause.Where);
            if (string.IsNullOrEmpty(mongoQuery) || mongoQuery == "{}")
            {
                throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_DATASTORE_WHERE_CLAUSE_IS_ABSENT_IN_QUERY, "'Where' clause is absent in query.");
            }
            return mongoQuery;
        }

        /// <summary>
		/// Builds the mongo-style query string to be run against the backend.
		/// </summary>
        /// <param name="query">LINQ-style query that can be used to filter delete results.</param>
        /// <param name="visitorClause">Enum to filter LINQ clauses which should be used in a query for network request.</param>
		/// <returns>The mongo-style query string.</returns>
        private static string GetQuery<T>(IQueryable<object> query, VisitorClause visitorClause)
        {
            if (query != null)
            {
                StringQueryBuilder queryBuilder = new StringQueryBuilder();

                KinveyQueryVisitor visitor = new KinveyQueryVisitor(queryBuilder, typeof(T), visitorClause);
                QueryModel queryModel = (query.Provider as KinveyQueryProvider)?.qm;

                queryBuilder.Write("{");
                queryModel?.Accept(visitor);
                queryBuilder.Write("}");

                string mongoQuery = queryBuilder.BuildQueryString();
                return mongoQuery;
            }

            return default(string);
        }
    }
}
