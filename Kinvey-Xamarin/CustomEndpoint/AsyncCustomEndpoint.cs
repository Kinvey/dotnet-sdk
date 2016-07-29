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
using System.Threading.Tasks;

namespace KinveyXamarin
{
	public class AsyncCustomEndpoint<I, O> : CustomEndpoint<I, O>
	{
		public AsyncCustomEndpoint (AbstractClient client): base(client)
		{
		}

		/// <summary>
		/// Executes the custom endpoint, expecting a single result
		/// </summary>
		/// <param name="endpoint">Endpoint name.</param>
		/// <param name="input">Input object.</param>
		/// <param name="delegates">Delegates for success or failure.</param>
		public void ExecuteCustomEndpoint(string endpoint, I input, KinveyDelegate<O> delegates)
		{
			Task.Run (() => {
				try{
					O result = base.executeCustomEndpointBlocking(endpoint, input).Execute();
					delegates.onSuccess(result);
				}catch(Exception e){
					delegates.onError(e);
				}
			});
		}

		/// <summary>
		/// Executes the custom endpoint, expecting an array of results
		/// </summary>
		/// <param name="endpoint">Endpoint name.</param>
		/// <param name="input">Input object.</param>
		/// <param name="delegates">Delegates for success or failure.</param>
		public void ExecuteCustomEndpoint(string endpoint, I input, KinveyDelegate<O[]> delegates)
		{
			Task.Run (() => {
				try{
					O[] result = base.executeCustomEndpointArrayBlocking(endpoint, input).Execute();
					delegates.onSuccess(result);
				}catch(Exception e){
					delegates.onError(e);
				}
			});
		}
	}
}
