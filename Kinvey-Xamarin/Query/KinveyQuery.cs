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
using System.Collections.Generic;
using System.Linq;

namespace KinveyXamarin
{
	public class KinveyQuery<T> : IObserver<T>
	{
		public IQueryable<T> Query { get; }
		public KinveyQueryDelegate<T> QueryDelegate { get; }
		private List<T> results;

		public KinveyQuery(IQueryable<T> query, KinveyQueryDelegate<T> queryDelegate)
		{
			Query = query;
			QueryDelegate = queryDelegate;
			results = new List<T>();
		}

		public void OnNext(T item)
		{
			results.Add(item);
		}

		public void OnError(Exception e)
		{
			QueryDelegate.onError(e);
		}

		public void OnCompleted()
		{
			QueryDelegate.onSuccess(results);
			results.Clear();
			QueryDelegate.onCompleted();
		}
	}
}
