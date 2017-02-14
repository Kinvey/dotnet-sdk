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
using System.Collections.Generic;

namespace Kinvey
{
	/// <summary>
	/// Router object used to handle realtime messages and route them to the
	/// appropriate <see cref="Kinvey.DataStore{T}"/> handler.
	/// The handler will be a <see cref="Kinvey.KinveyRealtimeDelegate{T}"/> callback delegate.
	/// </summary>
	static public class RealtimeRouter
	{
		static PubNubMessaging.Core.Pubnub pubnubClient;

		static Dictionary<string, Action<string>> mapChannelToCallback;

		static internal AbstractClient KinveyClient { get; private set; }

		static string ChannelGroup { get; set; }

		static internal void Initialize(string channelGroup, string publishKey, string subscribeKey, string authKey, AbstractClient client)
		{
			if (pubnubClient == null)
			{
				ChannelGroup = channelGroup;

				pubnubClient = new PubNubMessaging.Core.Pubnub(publishKey, subscribeKey);
				pubnubClient.AuthenticationKey = authKey;

				pubnubClient.Subscribe<string>(string.Empty, ChannelGroup, SubscribeCallback, ConnectCallback, PubnubClientSubscribeErrorCallback);

				//FOR UNIQUE DEVICE GUID GENERATION --> Guid deviceGUID = pubnubClient.GenerateGuid(); string deviceID = deviceGUID.ToString();
				KinveyClient = client;
				mapChannelToCallback = new Dictionary<string, Action<string>>();
			}
		}

		static internal void Uninitialize()
		{
			KinveyClient = null;
			mapChannelToCallback.Clear();
			mapChannelToCallback = null;

			pubnubClient?.Unsubscribe<string>(string.Empty, ChannelGroup, UnsubscribeCallback, ConnectCallback, DisconnectCallback, PubnubClientUnsubscribeErrorCallback);

			pubnubClient.AuthenticationKey = String.Empty;
			pubnubClient = null;

			ChannelGroup = null;
		}

		static internal bool Publish(string channel, string receiverID, object message)
		{
			(message as IStreamable).SenderID = KinveyClient.ActiveUser.Id;

			return pubnubClient.Publish<string>(channel, message, PublishCallback, PubnubClientPublishErrorCallback);
		}

		static internal void SubscribeCollection(string collectionName, Action<string> callback)
		{
			string channel = Constants.STR_REALTIME_COLLECTION_CHANNEL_PREPEND + collectionName;
			AddChannel(channel, callback);
		}

		static internal void UnsubscribeCollection(string collectionName)
		{
			string channel = Constants.STR_REALTIME_COLLECTION_CHANNEL_PREPEND + collectionName;
			RemoveChannel(channel);
		}

		static internal void SubscribeStream(string streamName, Action<string> callback)
		{
			string channel = Constants.STR_REALTIME_STREAM_CHANNEL_PREPEND + streamName;
			AddChannel(channel, callback);
		}

		static internal void UnsubscribeStream(string streamName)
		{
			string channel = Constants.STR_REALTIME_STREAM_CHANNEL_PREPEND + streamName;
			RemoveChannel(channel);
		}

		#region Realtime Callbacks

		static void SubscribeCallback(string msgResult)
		{
			KinveyUtils.Logger.Log("Subscribe Callback: " + msgResult);

			string msg = String.Empty;
			string time = String.Empty;
			string group = String.Empty;
			string chan = String.Empty;

			if (ParsePubnubMessage(msgResult, ref msg, ref time, ref group, ref chan))
			{
				KinveyUtils.Logger.Log("Subscribe Callback Message: " + msg);
				KinveyUtils.Logger.Log("Subscribe Callback Timestamp: " + time);
				KinveyUtils.Logger.Log("Subscribe Callback Channel Group: " + group);
				KinveyUtils.Logger.Log("Subscribe Callback Channel: " + chan);

				var callback = mapChannelToCallback[chan];

				callback.Invoke(msg);
			}
		}

		static void UnsubscribeCallback(string msgResult)
		{
			KinveyUtils.Logger.Log("Unsubscribe Callback: " + msgResult);
		}

		static void ConnectCallback(string msgConnect)
		{
			KinveyUtils.Logger.Log("Connect Callback: " + msgConnect);
		}

		static void DisconnectCallback(string msgConnect)
		{
			KinveyUtils.Logger.Log("Disconnect Callback: " + msgConnect);
		}

		static void PubnubClientSubscribeErrorCallback(PubNubMessaging.Core.PubnubClientError error)
		{
			// TODO Map PubnubClientError objects to KinveyException objects before forwarding
			KinveyUtils.Logger.Log("Subscribe Error: " + error);
		}

		static void PubnubClientUnsubscribeErrorCallback(PubNubMessaging.Core.PubnubClientError error)
		{
			// TODO Map PubnubClientError objects to KinveyException objects before forwarding
			KinveyUtils.Logger.Log("Unsubscribe Error: " + error);
		}

		static void PublishCallback(string msgPublish)
		{
			KinveyUtils.Logger.Log("Publish Callback: " + msgPublish);
		}

		static void PubnubClientPublishErrorCallback(PubNubMessaging.Core.PubnubClientError error)
		{
			// TODO Map PubnubClientError objects to KinveyException objects before forwarding
			KinveyUtils.Logger.Log("Subscribe Error: " + error);
		}

		#endregion

		#region Helper methods

		static void AddChannel(string channel, Action<string> callback)
		{
			if (mapChannelToCallback.ContainsKey(channel))
			{
				mapChannelToCallback[channel] = callback;
			}
			else
			{
				mapChannelToCallback.Add(channel, callback);
			}
		}

		static void RemoveChannel(string channel)
		{
			mapChannelToCallback.Remove(channel);
		}

		static bool ParsePubnubMessage(string input, ref string message, ref string timestamp, ref string channelGroup, ref string channel)
		{
			bool result = false;

			if (input.StartsWith(Constants.STR_SQUARE_BRACKET_OPEN, StringComparison.Ordinal) &&
			    input.EndsWith(Constants.STR_SQUARE_BRACKET_CLOSE, StringComparison.Ordinal))
			{
				// TODO separate parsing into separate messages

				// Trim leading/trailing whitespace
				char[] trimChars = { ' ' };
				input = input.Trim(trimChars);

				// Remove leading square open bracket and trailing close square bracket
				input = input.Substring(1);
				input = input.Substring(0, input.Length - 1);

				char[] delimChars = { Constants.CHAR_COMMA };
				string[] arrMessage = input.Split(delimChars);

				if (arrMessage.Length >= 3)
				{
					channel = arrMessage[arrMessage.Length - 1];
					char[] trimQuotes = { Constants.CHAR_QUOTATION_MARK };
					channel = channel.Trim(trimQuotes);
					char[] delimsChannel = { Constants.CHAR_PERIOD };
					var splitChannel = channel.Split(delimsChannel);
					channel = splitChannel[1];

					channelGroup = arrMessage[arrMessage.Length - 2];
					timestamp = arrMessage[arrMessage.Length - 3];

					for (int i = 0; i < arrMessage.Length - 3; i++)
					{
						message += Constants.CHAR_COMMA + arrMessage[i];
					}

					// Trim leading comma
					message = message.TrimStart(delimChars);

					result = true;
				}
			}

			return result;
		}

		#endregion
	}
}
