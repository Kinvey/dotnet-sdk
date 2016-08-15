// Copyright (c) 2016, Kinvey, Inc. All rights reserved.
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
using System.Threading.Tasks;
using System.Linq;
using Remotion.Linq;

namespace KinveyXamarin
{
	public abstract class ReadRequest <T, U> : Request <T, U>
	{
		public ICache<T> Cache { get; }
		public string Collection { get; }
		public ReadPolicy Policy { get; }
		protected IQueryable<T> Query { get; }
		protected bool DeltaSetFetchingEnabled { get; }

		public ReadRequest (AbstractClient client, string collection, ICache<T> cache, IQueryable<T> query, ReadPolicy policy, bool deltaSetFetchingEnabled)
			: base(client)
		{
			this.Cache = cache;
			this.Collection = collection;
			this.Query = query;
			this.Policy = policy;
			this.DeltaSetFetchingEnabled = deltaSetFetchingEnabled;
		}

		protected string BuildMongoQuery ()
		{
			if (Query != null) {
				StringQueryBuilder Writer = new StringQueryBuilder ();
				Writer.Reset ();

				KinveyQueryVisitor visitor = new KinveyQueryVisitor (Writer, typeof (T));

				QueryModel queryModel = (Query.Provider as KinveyQueryProvider).qm;

				Writer.Write ("{");
				queryModel.Accept (visitor);
				Writer.Write ("}");

				string mongoQuery = Writer.GetFullString ();

				return mongoQuery;
			}

			return default (string);
		}

	}
}
