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
	/// Class which represents Kinvey realtime status messages.
	/// </summary>
	public class KinveyRealtimeStatus
	{
		/// <summary>
		/// Gets the type of the realtime status.
		/// </summary>
		/// <value>The type of the realtime status.</value>
		public StatusType RealtimeStatusType { get; private set; }

		/// <summary>
		/// Gets the status code.
		/// </summary>
		/// <value>The status.</value>
		public int Status { get; private set; }

		/// <summary>
		/// Gets the status message.
		/// </summary>
		/// <value>The message.</value>
		public string Message { get; private set; }

		/// <summary>
		/// Gets the time stamp of the status message.
		/// </summary>
		/// <value>The time stamp.</value>
		public string TimeStamp { get; private set; }

		/// <summary>
		/// Gets the channel of the status message.
		/// </summary>
		/// <value>The channel.</value>
		public string Channel { get; private set; }

		/// <summary>
		/// Gets the channel group of the status message.
		/// </summary>
		/// <value>The channel group.</value>
		public string ChannelGroup { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Kinvey.KinveyRealtimeStatus"/> class.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="pubnubMessage">Pubnub message.</param>
		public KinveyRealtimeStatus(KinveyRealtimeStatus.StatusType type, string[] arrMessage)
		{
			RealtimeStatusType = type;

			switch (RealtimeStatusType)
			{
				case StatusType.STATUS_CONNECT:  // Message Format --> [status, statusmessage, channelgroup]
				case StatusType.STATUS_DISCONNECT:  // Message Format --> [status, statusmessage, channelgroup]
					Status = int.Parse(arrMessage[0]);
					Message = arrMessage[1];
					ChannelGroup = arrMessage[2];
					break;

				case StatusType.STATUS_PUBLISH:  // Message Format --> [status,statusmessage,timestamp,channel]
					Status = int.Parse(arrMessage[0]);
					Message = arrMessage[1];
					TimeStamp = arrMessage[2];
					Channel = arrMessage[3];
					break;

				default:
					break;
			}
		}

		/// <summary>
		/// Enum representing all the types of status messages that can be received for realtime.
		/// </summary>
		public enum StatusType
		{
			/// <summary>
			/// Enum representing a connection status message
			/// </summary>
			STATUS_CONNECT,

			/// <summary>
			/// Enum representing a disconnection status message
			/// </summary>
			STATUS_DISCONNECT,

			/// <summary>
			/// Enum representing a publish status message
			/// </summary>
			STATUS_PUBLISH
		}
	}
}
