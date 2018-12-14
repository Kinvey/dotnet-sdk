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

namespace Kinvey
{
    /// <summary>
    /// Realtime reconnection policy extension
    /// </summary>
    internal static class RealtimeReconnectionPolicyExtension
    {
        /// <summary>
        /// Converting RealtimeReconnectionPolicy enum value to PNReconnectionPolicy enum value
        /// </summary>
        internal static PubnubApi.PNReconnectionPolicy ConvertToPNReconnectionPolicy(this RealtimeReconnectionPolicy realtimeReconnectionPolicy)
        {
            switch (realtimeReconnectionPolicy)
            {
                case RealtimeReconnectionPolicy.NONE:
                    return PubnubApi.PNReconnectionPolicy.NONE;
                case RealtimeReconnectionPolicy.LINEAR:
                    return PubnubApi.PNReconnectionPolicy.LINEAR;
                case RealtimeReconnectionPolicy.EXPONENTIAL:
                    return PubnubApi.PNReconnectionPolicy.EXPONENTIAL;
                default:
                    return PubnubApi.PNReconnectionPolicy.NONE;
            }
        }
    }
}

