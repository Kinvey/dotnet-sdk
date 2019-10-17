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
    public partial class Client
    {
        /// <summary>
		/// Builder for creating a new instance of a client.  Use this class to easily create a new client, as it uses the builder pattern so methods can be chained together.
		/// Once the builder is configured, call `.build()` to return an instance of a client.
		/// </summary>
        public partial class Builder
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Builder"/> class.
            /// </summary>
            /// <param name="appKey">App key from Kinvey.</param>
            /// <param name="appSecret">App secret from Kinvey.</param>
            public Builder(
                string appKey,
                string appSecret
            ) : this(
                appKey,
                appSecret,
                Environment.CurrentDirectory,
                Constants.DevicePlatform.NET
            )
            {
            }
        }
    }
}
