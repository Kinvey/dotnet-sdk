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
	public class KinveyObserver<T> : IObserver<T>
	{
		//private List<T> results;

		//public KinveyObserver()
		//{
		//	results = new List<T>();
		//}

		///// <summary>
		///// This Action is executed when an asynchronously operation completes successfully.  T represents the expected response type.
		///// </summary>
		public Action<T> onSuccess;

		///// <summary>
		///// This Action is executed when an error occurs, either on the device itself, or returned from the service.
		///// </summary>
		public Action<Exception> onError;

		/// <summary>
		/// This Action is executed when the operation is completed.
		/// </summary>
		public Action onCompleted;

		public void OnNext (T item) {
			onSuccess (item);
		}
		public void OnError (Exception e) {
			onError (e);
		}
		public void OnCompleted () {
			onCompleted ();
		}
		//public void OnNext(T item)
		//{
		//	//results.Add(item);
		//}

		//public void OnError(Exception e)
		//{
		//	onError(e);
		//}

		//public void OnCompleted()
		//{
		//	//onSuccess(results);
		//	//results.Clear();
		//	//onCompleted();
		//}
	}
}
