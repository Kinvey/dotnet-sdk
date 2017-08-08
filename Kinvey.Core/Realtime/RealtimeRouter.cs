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
	/// The handler will be a <see cref="KinveyRealtimeDelegate"/> callback delegate.
	/// </summary>
	internal sealed class RealtimeRouter
	{
		private static volatile RealtimeRouter instance;

		private static object lockObject = new object();

		private PubnubApi.Pubnub pubnubClient;
		private PubnubApi.SubscribeCallbackExt subscribeCallback;
		private Dictionary<string, KinveyRealtimeDelegate> mapChannelToCallback;

		private AbstractClient KinveyClient { get; set; }

		private string ChannelGroup { get; set; }

		// Make constructor inaccessible, so that all access to the singleton
		// RealtimeRouter happens through the Instance property
		private RealtimeRouter() { }

		public static RealtimeRouter Instance
		{
			get
			{
				if (instance == null)
				{
					// throw error stating that Reatime has not been initialized
					var ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_NOT_REGISTERED, String.Empty);
					throw ke;
				}

				return instance;
			}
		}

		static internal void Initialize(string channelGroup, string publishKey, string subscribeKey, string authKey, AbstractClient client)
		{
			if (instance == null)
			{
				lock(lockObject)
				{
					if (instance == null)
					{
						instance = new RealtimeRouter();
						if (instance.pubnubClient == null)
						{
							PubnubApi.PNConfiguration PNconfig = new PubnubApi.PNConfiguration();
							PNconfig.SubscribeKey = subscribeKey;
							PNconfig.PublishKey = publishKey;
							PNconfig.AuthKey = authKey;
							//PNconfig.Secure = true; // TODO Enable SSL
							instance.pubnubClient = new PubnubApi.Pubnub(PNconfig);

							//instance.pubnubClient.Subscribe<string>(string.Empty, instance.ChannelGroup, instance.SubscribeCallback, instance.ConnectCallback, instance.PubnubClientSubscribeErrorCallback);
							instance.subscribeCallback = new PubnubApi.SubscribeCallbackExt(
                                (pubnubObj, message) => { instance.SubscribeCallback(message.Channel, message.Message as string); },
								(pubnubObj, presence) => { /* presence not currently supported */}, // TODO Support PubNub presence
								(pubnubObj, status) => { instance.HandleStatusMessage(status); }
							);

                            instance.pubnubClient.AddListener(instance.subscribeCallback);

                            instance.ChannelGroup = channelGroup;
							instance.pubnubClient.Subscribe<string>().ChannelGroups(new string[] { instance.ChannelGroup }).Execute();

							//FOR UNIQUE DEVICE GUID GENERATION --> Guid deviceGUID = pubnubClient.GenerateGuid(); string deviceID = deviceGUID.ToString();
							instance.KinveyClient = client;
							instance.mapChannelToCallback = new Dictionary<string, KinveyRealtimeDelegate>();
						}
					}
				}
			}
		}

		static internal void Uninitialize()
		{
			if (instance != null)
			{
				lock(lockObject)
				{
					if (instance != null)
					{
						instance.pubnubClient.RemoveListener(instance.subscribeCallback);
						instance.pubnubClient.Unsubscribe<string>().ChannelGroups(new string[] { instance.ChannelGroup }).Execute();
						//instance.pubnubClient?.Unsubscribe<string>(string.Empty, instance.ChannelGroup, instance.UnsubscribeCallback, instance.ConnectCallback, instance.DisconnectCallback, instance.PubnubClientUnsubscribeErrorCallback);
						//instance.pubnubClient.AuthenticationKey = String.Empty;
						instance.pubnubClient.Destroy();
						instance.pubnubClient = null;

						instance.ChannelGroup = null;

						instance.mapChannelToCallback.Clear();
						instance.mapChannelToCallback = null;

						instance.KinveyClient = null;

						instance = null;
					}
				}
			}
		}

		internal bool Publish(string channel, object message, Func<KinveyException, System.Threading.Tasks.Task> errorCallback)
		{
            bool result = false;

            //Action<PubNubMessaging.Core.PubnubClientError> publishError = (error) => {
            //	KinveyUtils.Logger.Log("RealtimeRouter: publish error");
            //	KinveyUtils.Logger.Log("Publish Error: " + error);
            //	KinveyUtils.Logger.Log("Publish Error Status Code: " + error.StatusCode);
            //	KinveyUtils.Logger.Log("Publish Error Message: " + error.Message);

            //	var exception = HandleErrorMessage(error);
            //	errorCallback.Invoke(exception);
            //};

            //return pubnubClient.Publish<string>(channel, message, PublishCallback, publishError);
            //return false;


            PubnubApi.PNPublishResult publishResult = pubnubClient.Publish().Channel(channel).Message(message).Sync();
            if (publishResult?.Timetoken != 0L)
            {
                result = true;
            }

            return result;
		}

		internal void SubscribeCollection(string collectionName, KinveyRealtimeDelegate callback)
		{
            string appKey = (KinveyClient.RequestInitializer as KinveyClientRequestInitializer).AppKey;
            string channel = appKey + Constants.CHAR_PERIOD + Constants.STR_REALTIME_COLLECTION_CHANNEL_PREPEND + collectionName;
			AddChannel(channel, callback);
		}

		internal void UnsubscribeCollection(string collectionName)
		{
			string appKey = (KinveyClient.RequestInitializer as KinveyClientRequestInitializer).AppKey;
			string channel = appKey + Constants.CHAR_PERIOD + Constants.STR_REALTIME_COLLECTION_CHANNEL_PREPEND + collectionName;
			RemoveChannel(channel);
		}

		internal void SubscribeStream(string channelName, KinveyRealtimeDelegate callback)
		{
            AddChannel(channelName, callback);
		}

		internal void UnsubscribeStream(string channelName)
		{
            RemoveChannel(channelName);
		}

		#region Realtime Callbacks

		void SubscribeCallback(string channel, string msgResult)
		{
			// Message Format --> [message,timestamp,channelgroup,channel]
			KinveyUtils.Logger.Log("Subscribe Callback: " + msgResult);

			//string msg = String.Empty;
			//string time = String.Empty;
			//string group = String.Empty;
			//string chan = String.Empty;

			//if (ParsePubnubMessage(msgResult, ref msg, ref time, ref group, ref chan))
			{
				//KinveyUtils.Logger.Log("Subscribe Callback Message: " + msg);
				//KinveyUtils.Logger.Log("Subscribe Callback Timestamp: " + time);
				//KinveyUtils.Logger.Log("Subscribe Callback Channel Group: " + group);
				//KinveyUtils.Logger.Log("Subscribe Callback Channel: " + chan);

				//var callback = mapChannelToCallback[chan].OnNext;
				var callback = mapChannelToCallback[channel].OnNext;

				callback.Invoke(msgResult);
			}
		}

		void UnsubscribeCallback(string msgResult)
		{
			KinveyUtils.Logger.Log("Unsubscribe Callback: " + msgResult);
		}

		void ConnectCallback(string msgConnect)
		{
			KinveyUtils.Logger.Log("Connect Callback: " + msgConnect);

			// Since a connect or disconnect affects all those in the channel group, this status message has
			// to be broadcast to all the callbacks.
			var pubnubMessage = PrepPubnubMessage(msgConnect);
			var status = new KinveyRealtimeStatus(KinveyRealtimeStatus.StatusType.STATUS_CONNECT, pubnubMessage);
			foreach (var entry in mapChannelToCallback)
			{
				var callback = mapChannelToCallback[entry.Key].OnStatus;
				callback.Invoke(status);
			}
		}

		void DisconnectCallback(string msgDisonnect)
		{
			KinveyUtils.Logger.Log("Disconnect Callback: " + msgDisonnect);

			// Since a connect or disconnect affects all those in the channel group, this status message has
			// to be broadcast to all the callbacks.
			var pubnubMessage = PrepPubnubMessage(msgDisonnect);
			var status = new KinveyRealtimeStatus(KinveyRealtimeStatus.StatusType.STATUS_DISCONNECT, pubnubMessage);
			foreach (var entry in mapChannelToCallback)
			{
				var callback = mapChannelToCallback[entry.Key].OnStatus;
				callback.Invoke(status);
			}
		}

		//void PubnubClientSubscribeErrorCallback(PubnubApi.PNErrorData error)
		//{
		//	KinveyUtils.Logger.Log("Subscribe Error: " + error);
		//	KinveyUtils.Logger.Log("Subscribe Error Status Code: " + error.StatusCode);
		//	KinveyUtils.Logger.Log("Subscribe Error Message: " + error.Message);

		//	var exception = HandleErrorMessage(error);
		//	if (exception != default(KinveyException))
		//	{
		//		var channel = GetChannelFromFullName(error.Channel);
		//		var callback = mapChannelToCallback[channel].OnError;
		//		callback.Invoke(exception);
		//	}
		//}

		//void PubnubClientUnsubscribeErrorCallback(PubNubMessaging.Core.PubnubClientError error)
		//{
		//	KinveyUtils.Logger.Log("Unsubscribe Error: " + error);
		//	KinveyUtils.Logger.Log("Unsubscribe Error Status Code: " + error.StatusCode);
		//	KinveyUtils.Logger.Log("Unsubscribe Error Message: " + error.Message);

		//	var exception = HandleErrorMessage(error);
		//	if (exception != default(KinveyException))
		//	{
		//		var channel = GetChannelFromFullName(error.Channel);
		//		var callback = mapChannelToCallback[channel].OnError;
		//		callback.Invoke(exception);
		//	}
		//}

		void PublishCallback(string msgPublish)
		{
			KinveyUtils.Logger.Log("Publish Callback: " + msgPublish);
			var pubnubMessage = PrepPubnubMessage(msgPublish);
			var status = new KinveyRealtimeStatus(KinveyRealtimeStatus.StatusType.STATUS_PUBLISH, pubnubMessage);
			var channel = GetChannelFromFullName(status.Channel);
			//var callback = mapChannelToCallback[channel].OnStatus;
			//callback.Invoke(status);
		}

		#endregion

		#region Helper methods

		void AddChannel(string channel, KinveyRealtimeDelegate callback)
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

		void RemoveChannel(string channel)
		{
			mapChannelToCallback.Remove(channel);
		}

		string[] PrepPubnubMessage(string input)
		{
			string[] arrMessage = null;

			if (input.StartsWith(Constants.STR_SQUARE_BRACKET_OPEN, StringComparison.Ordinal) &&
				input.EndsWith(Constants.STR_SQUARE_BRACKET_CLOSE, StringComparison.Ordinal))
			{
				// Trim leading/trailing whitespace
				char[] trimChars = { ' ' };
				input = input.Trim(trimChars);

				// Remove leading square open bracket and trailing close square bracket
				input = input.Substring(1);
				input = input.Substring(0, input.Length - 1);

				char[] delimChars = { Constants.CHAR_COMMA };
				arrMessage = input.Split(delimChars);
			}

			return arrMessage;
		}

		//bool ParsePubnubMessage(string input, ref string message, ref string timestamp, ref string channelGroup, ref string channel)
		//{
		//	bool result = false;

		//	var arrMessage = PrepPubnubMessage(input);
		//	if (arrMessage != null)
		//	{
		//		if (arrMessage.Length >= 3)
		//		{
		//			channel = arrMessage[arrMessage.Length - 1];
		//			channel = GetChannelFromFullName(channel);

		//			channelGroup = arrMessage[arrMessage.Length - 2];
		//			timestamp = arrMessage[arrMessage.Length - 3];

		//			for (int i = 0; i < arrMessage.Length - 3; i++)
		//			{
		//				message += Constants.CHAR_COMMA + arrMessage[i];
		//			}

		//			// Trim leading comma
		//			char[] delimChars = { Constants.CHAR_COMMA };
		//			message = message.TrimStart(delimChars);

		//			result = true;
		//		}
		//	}

		//	return result;
		//}

		string GetChannelFromFullName(string fullChannelName)
		{
			char[] trimQuotes = { Constants.CHAR_QUOTATION_MARK };
			string channel = fullChannelName.Trim(trimQuotes);
			char[] delimsChannel = { Constants.CHAR_PERIOD };
			var splitChannel = channel.Split(delimsChannel);
			channel = splitChannel[1];
			return channel;
		}

		KinveyException HandleStatusMessage(PubnubApi.PNStatus status)
		{
			KinveyException ke = default(KinveyException);

			if (status.Error)
			{
				// Status indicates an error with PubNub.
				// TODO Handle error by creating a LiveService KinveyException, with the error information contained.
				var errorData = status.ErrorData;
			}

			switch (status.Operation)
			{
				// Publish/Subscribe
				case PubnubApi.PNOperationType.PNPublishOperation:
					break;
				case PubnubApi.PNOperationType.PNSubscribeOperation:
					break;
				case PubnubApi.PNOperationType.PNUnsubscribeOperation:
					break;

				// Channel Group and Channels
				case PubnubApi.PNOperationType.ChannelGroupAllGet:
					break;
				case PubnubApi.PNOperationType.ChannelGroupAuditAccess:
					break;
				case PubnubApi.PNOperationType.ChannelGroupGet:
					break;
				case PubnubApi.PNOperationType.ChannelGroupGrantAccess:
					break;
				case PubnubApi.PNOperationType.ChannelGroupRevokeAccess:
					break;
				case PubnubApi.PNOperationType.PNAddChannelsToGroupOperation:
					break;
				case PubnubApi.PNOperationType.PNChannelGroupsOperation:
					break;
				case PubnubApi.PNOperationType.PNChannelsForGroupOperation:
					break;
				case PubnubApi.PNOperationType.PNRemoveChannelsFromGroupOperation:
					break;
				case PubnubApi.PNOperationType.PNRemoveGroupOperation:
					break;

				// Access Manager
				case PubnubApi.PNOperationType.PNAccessManagerAudit:
					break;
				case PubnubApi.PNOperationType.PNAccessManagerGrant:
					break;

				// Presence
				case PubnubApi.PNOperationType.Presence:
					break;
				case PubnubApi.PNOperationType.PresenceUnsubscribe:
					break;

				// Push
				case PubnubApi.PNOperationType.PushGet:
					break;
				case PubnubApi.PNOperationType.PushRegister:
					break;
				case PubnubApi.PNOperationType.PushRemove:
					break;
				case PubnubApi.PNOperationType.PushUnregister:
					break;

				// Miscellaneous
				case PubnubApi.PNOperationType.Leave:
					break;
				case PubnubApi.PNOperationType.None:
					break;
				case PubnubApi.PNOperationType.RevokeAccess:
					break;
				case PubnubApi.PNOperationType.PNFireOperation:
					break;
				case PubnubApi.PNOperationType.PNGetStateOperation:
					break;
				case PubnubApi.PNOperationType.PNHeartbeatOperation:
					break;
				case PubnubApi.PNOperationType.PNHereNowOperation:
					break;
				case PubnubApi.PNOperationType.PNHistoryOperation:
					break;
				case PubnubApi.PNOperationType.PNSetStateOperation:
					break;
				case PubnubApi.PNOperationType.PNTimeOperation:
					break;
				case PubnubApi.PNOperationType.PNWhereNowOperation:
					break;

				default:
					break;
			}

			return ke;
		}

		//KinveyException HandleErrorMessage(PubNubMessaging.Core.PubnubClientError error)
		//{
		//	KinveyException ke = default(KinveyException);

		//	switch (error.Severity)
		//	{
		//		case PubNubMessaging.Core.PubnubErrorSeverity.Critical:
		//			switch (error.StatusCode)
		//			{
		//				case 104: //ERROR_REALTIME_CRITICAL_VERIFY_CIPHER_KEY
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_CRITICAL_VERIFY_CIPHER_KEY, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 4010: //ERROR_REALTIME_CRITICAL_INCORRECT_SUBSBRIBE_KEY
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_CRITICAL_INCORRECT_SUBSBRIBE_KEY, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 4030: //ERROR_REALTIME_CRITICAL_NOT_AUTHORIZED_ON_CHANNEL
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_CRITICAL_NOT_AUTHORIZED_ON_CHANNEL, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 5000: //ERROR_REALTIME_CRITICAL_INTERNAL_SERVER_ERROR
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_CRITICAL_INTERNAL_SERVER_ERROR, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 5020: //ERROR_REALTIME_CRITICAL_BAD_GATEWAY
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_CRITICAL_BAD_GATEWAY, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 5040: //ERROR_REALTIME_CRITICAL_GATEWAY_TIMEOUT
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_CRITICAL_GATEWAY_TIMEOUT, "PubNub status code: " + error.StatusCode);
		//					break;
		//				default: //ERROR_REALTIME_CRITICAL_UNKNOWN
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_CRITICAL_UNKNOWN, "PubNub status code: " + error.StatusCode);
		//					break;
		//			}
		//		break;

		//		case PubNubMessaging.Core.PubnubErrorSeverity.Warn:
		//			switch (error.StatusCode)
		//			{
		//				case 103: //ERROR_REALTIME_WARNING_VERIFY_HOSTNAME
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_WARNING_VERIFY_HOSTNAME, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 106: //ERROR_REALTIME_WARNING_CHECK_NETWORK_CONNECTION
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_WARNING_CHECK_NETWORK_CONNECTION, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 108: //ERROR_REALTIME_WARNING_CHECK_NETWORK_CONNECTION
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_WARNING_CHECK_NETWORK_CONNECTION, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 109: //ERROR_REALTIME_WARNING_NO_NETWORK_CONNECTION
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_WARNING_NO_NETWORK_CONNECTION, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 114: //ERROR_REALTIME_WARNING_VERIFY_CIPHER_KEY
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_WARNING_VERIFY_CIPHER_KEY, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 115: //ERROR_REALTIME_WARNING_PROTOCOL_ERROR
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_WARNING_PROTOCOL_ERROR, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 116: //ERROR_REALTIME_WARNING_SERVER_PROTOCOL_VIOLATION
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_WARNING_SERVER_PROTOCOL_VIOLATION, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 4000: //ERROR_REALTIME_WARNING_MESSAGE_TOO_LARGE
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_WARNING_MESSAGE_TOO_LARGE, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 4001: //ERROR_REALTIME_WARNING_BAD_REQUEST
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_WARNING_BAD_REQUEST, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 4002: //ERROR_REALTIME_WARNING_INVALID_PUBLISH_KEY
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_WARNING_INVALID_PUBLISH_KEY, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 4031: //ERROR_REALTIME_WARNING_INCORRECT_PUBLIC_OR_SECRET_KEY
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_WARNING_INCORRECT_PUBLIC_OR_SECRET_KEY, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 4040: //ERROR_REALTIME_WARNING_URL_LENGTH_TOO_LONG
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_WARNING_URL_LENGTH_TOO_LONG, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 0: //ERROR_REALTIME_WARNING_UNDOCUMENTED_ERROR
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_WARNING_UNDOCUMENTED_ERROR, "PubNub status code: " + error.StatusCode);
		//					break;

		//				case 4020: //ERROR_REALTIME_WARNING_PAM_NOT_ENABLED
		//					// These case(s) are not applicable to our implementation
		//					break;

		//				default: //ERROR_REALTIME_WARNING_UNKNOWN
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_WARNING_UNKNOWN, error.StatusCode.ToString());
		//					break;
		//			}
		//			break;

		//		case PubNubMessaging.Core.PubnubErrorSeverity.Info:
		//			switch (error.StatusCode)
		//			{
		//				case 110: //ERROR_REALTIME_INFORMATIONAL_NO_NETWORK_CONNECTION
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_INFORMATIONAL_NO_NETWORK_CONNECTION, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 111: //ERROR_REALTIME_INFORMATIONAL_DUPLICATE_CHANNEL_SUBSCRIPTION
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_INFORMATIONAL_DUPLICATE_CHANNEL_SUBSCRIPTION, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 112: //ERROR_REALTIME_INFORMATIONAL_DUPLICATE_CHANNEL_SUBSCRIPTION
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_INFORMATIONAL_DUPLICATE_CHANNEL_SUBSCRIPTION, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 117: //ERROR_REALTIME_INFORMATIONAL_INVALID_CHANNEL_NAME
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_INFORMATIONAL_INVALID_CHANNEL_NAME, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 118: //ERROR_REALTIME_INFORMATIONAL_CHANNEL_NOT_SUBSCRIBED
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_INFORMATIONAL_CHANNEL_NOT_SUBSCRIBED, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 120: //ERROR_REALTIME_INFORMATIONAL_UNSUBSCRIBE_INCOMPLETE
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_INFORMATIONAL_UNSUBSCRIBE_INCOMPLETE, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 122: //ERROR_REALTIME_INFORMATIONAL_NETWORK_NOT_AVAILABLE
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_INFORMATIONAL_NETWORK_NOT_AVAILABLE, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 123: //ERROR_REALTIME_INFORMATIONAL_NETWORK_MAX_RETRIES_REACHED
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_INFORMATIONAL_NETWORK_MAX_RETRIES_REACHED, "PubNub status code: " + error.StatusCode);
		//					break;
		//				case 125: //ERROR_REALTIME_INFORMATIONAL_PUBLISH_OPERATION_TIMEOUT
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_INFORMATIONAL_PUBLISH_OPERATION_TIMEOUT, "PubNub status code: " + error.StatusCode);
		//					break;

		//				case 113: // Duplicate channel subscription for presence
		//				case 119: // Channel not subscribed for presence yet
		//				case 121: // Incomplete presence-unsubscribe
		//				case 124: // Max retries for presence
		//				case 126: // HereNow operation timeout occurred
		//				case 127: // Detailed History operation timeout occurred
		//				case 128: // Time operation timeout occurred
		//					// These case(s) are not applicable to our implementation
		//					break;

		//				default: //ERROR_REALTIME_INFORMATIONAL_UNKNOWN
		//					ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME, EnumErrorCode.ERROR_REALTIME_INFORMATIONAL_UNKNOWN, "PubNub status code: " + error.StatusCode);
		//					break;
		//			}
		//			break;
		//	}

		//	return ke;
		//}

		#endregion
	}
}
