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

using System;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace Kinvey
{
	/// <summary>
	/// QueryProvider class.
	/// </summary>
	public class KinveyQueryProvider : QueryProviderBase
	{
        /// <summary>
        /// The query expression used to create the QueryModel.
        /// </summary>
        ///  <value>The instance of the <see cref="Expression"/>.</value>
        public Expression queryExpression;

        /// <summary>
        /// The QueryModel from the given expression.
        /// </summary>
        /// <value>The instance of the <see cref="QueryModel"/> </value>
        public QueryModel qm;

		internal KinveyQueryProvider(Type queryableType, IQueryParser parser, IQueryExecutor executor)
			: base(parser, executor)
		{
		}

		/// <summary>
		/// Override of the query creation method from QueryProviderBase.  Used to capture the raw expression and the QueryModel.
		/// </summary>
		/// <returns>The query.</returns>
		/// <param name="expression">The query expression.</param>
		/// <typeparam name="T">The type the query returns.</typeparam>
		public override IQueryable<T> CreateQuery<T> (Expression expression)
		{
			queryExpression = expression;

			qm = GenerateQueryModel(expression);

			return (IQueryable<T>) Activator.CreateInstance (typeof(KinveyQueryable<>).MakeGenericType (typeof (T)), this, expression);
		}
	}
}
