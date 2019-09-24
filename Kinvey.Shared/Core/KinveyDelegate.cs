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

namespace Kinvey
{

    /// <summary>
    /// The Kinvey Delegate class is used for the callback pattern when executing requests asynchronously.  All Async* methods will take one as a parameter.
    /// </summary>
    /// <typeparam name="T">The type of Kinvey delegate.</typeparam>
    public class KinveyDelegate<T>
	{
        /// <summary>
        /// This Action is executed when an asynchronously operation completes successfully.  T represents the expected response type.
        /// </summary>
        /// <value>The action having the type of Kinvey delegate as the parameter.</value>
        public Action<T> onSuccess;
        /// <summary>
        /// This Action is executed when an error occurs, either on the device itself, or returned from the service.
        /// </summary>
        /// <value>The action having the Exception type as the parameter.</value>
        public Action<Exception> onError;

	}
}

