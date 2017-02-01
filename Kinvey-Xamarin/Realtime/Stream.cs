// Copyright (c) 2017, Kinvey, Inc. All rights reserved.
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
	/// </summary>
	public class Stream<T> where T : IStreamable
	{
		KinveyRealtimeDelegate<T> RealtimeCallback { get; set; }
		Action<string> routerCallback;

		/// <summary>
		/// Represents the name of the stream.
		/// </summary>
		public string StreamName { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Kinvey.Stream`1"/> class.
		/// </summary>
		/// <param name="streamName">Stream name.</param>
		public Stream(string streamName)
		{
			StreamName = streamName;
		}

		/// <summary>
		/// Publish a message of type {T} to the specified user.
		/// </summary>
		/// <param name="receiverID">Receiver identifier.</param>
		/// <param name="message">Message.</param>
		public bool Publish(string receiverID, T message)
		{
			// TODO make KCS request for publish access for the given receiverID
			// KCS will return, if successful, a response which will include the PubNub channel name

			string publishChannel = Constants.PUBNUB_TEST_CHANNEL; // HACK will eventually come from KCS response

			return RealtimeRouter.Publish(publishChannel, receiverID, message);
		}

		/// <summary>
		/// Subscribe the specified callback.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public bool Subscribe(KinveyRealtimeDelegate<T> callback)
		{
			bool success = false;

			if (callback != null)
			{
				RealtimeCallback = callback;

				routerCallback = new Action<string>((message) => 
				{
					var messageObj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(message);
					RealtimeCallback.onSuccess(messageObj);
				});

				RealtimeRouter.SubscribeStream(StreamName, routerCallback);
				success = true;
			}

			return success;
		}

		/// <summary>
		/// Unsubscribe this instance.
		/// </summary>
		public void Unsubscribe()
		{
			RealtimeRouter.UnsubscribeStream(StreamName);
			RealtimeCallback = null;
		}
	}
}
