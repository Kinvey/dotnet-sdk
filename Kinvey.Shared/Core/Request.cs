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

using System.Threading.Tasks;

namespace Kinvey
{
    /// <summary>
    /// Base class for network requests. 
    /// </summary>
    /// <typeparam name="T">The type of the network request.</typeparam>
    /// <typeparam name="U">The type of the network response.</typeparam>
	abstract public class Request <T, U>
	{
        ///<summary>
        /// Client that the user is logged in
        ///</summary>
        ///<value>The instance of the class inherited from the <see cref="AbstractClient"/> class.</value>
		protected AbstractClient Client { get;}

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="client">Client that the user is logged in.</param>
        public Request (AbstractClient client)
		{
			this.Client = client;
		}

        /// <summary>
        /// Executes a request asynchronously .
        /// </summary>
        /// <returns> The async task with the type of the response. </returns>
        public abstract Task<U> ExecuteAsync ();

        /// <summary>
        /// Communicates a request for cancellation.
        /// </summary>
        /// <returns>The async task with the boolean result.</returns>
		public abstract Task<bool> Cancel();
	}
}
