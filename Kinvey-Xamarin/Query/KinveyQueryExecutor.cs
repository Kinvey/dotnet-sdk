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
using Remotion.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using Remotion.Linq.Parsing.Structure;

namespace KinveyXamarin
{
	public class KinveyQueryExecutor<K> : IQueryExecutor
	{
		
		public StringQueryBuilder writer;
		public KinveyQueryable<K> queryable;

		public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
		{
			writer.Reset ();

			KinveyQueryVisitor visitor = new KinveyQueryVisitor(writer, typeof(K));

			writer.Write ("{");
			queryModel.Accept (visitor);
			writer.Write ("}");

			//Logger.Log (writer.GetFullString ());

			T[] cacheResults = (T[])queryable.executeQueryOnCache(KinveyQueryable<T>.express);
			//T[] cacheResults = (T[])queryable.executeQueryOnCache(visitor.cacheExpr);
			if (cacheResults != null)
			{
				foreach (T result in cacheResults)
				{
					yield return result;
				}
			}
			else
			{
				T[] results = (T[]) queryable.executeQuery (writer.GetFullString ());
				if (results != null)
				{
					foreach (T res in results)
					{
						yield return res;
					}
				}
			}
		}

		public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
		{
			var sequence = ExecuteCollection<T>(queryModel);
			return returnDefaultWhenEmpty ? sequence.SingleOrDefault() : sequence.Single();
		}

		public T ExecuteScalar<T>(QueryModel queryModel)
		{
			throw new NotImplementedException();
		}
	}
}
