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

using Newtonsoft.Json;
using System;

namespace Kinvey
{
    /// <summary>
	/// The class represents Kinvey realtime message.
	/// </summary>
    /// <typeparam name="T">The type of message.</typeparam>
    [Obsolete("This class has been deprecated.")]
	[JsonObject(MemberSerialization.OptOut)]
	public class RealtimeMessage<T>
	{
        /// <summary>
		/// Sender identifier.
		/// </summary>
		/// <value>The SenderID property gets/sets the value of the string field, _senderID.</value>
		public string SenderID { get; set; }

        /// <summary>
		/// Message.
		/// </summary>
		/// <value>The Message property gets/sets the value of the T field, _message.</value>
		public T Message { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealtimeMessage{T}"/> class.
        /// </summary>
        public RealtimeMessage()
		{
		}

		internal RealtimeMessage(string senderID, T message)
		{
			SenderID = senderID;
			Message = message;
		}
	}
}