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
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kinvey
{
    /// <summary>
    /// </summary>
    public class Stream<T>
    {
        KinveyStreamDelegate<T> RealtimeDelegate { get; set; }

        private Dictionary<string, string> mapPublishReceiverToChannel;

        // Represents the name of the stream.
        private string StreamName { get; set; }

		// Represents the name of the channel used by the RealtimeRouter class.
		private string ChannelName { get; set; }

		private AbstractClient KinveyClient { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Kinvey.Stream`1"/> class.
		/// </summary>
		/// <param name="streamName">Name of the stream during stream creation.</param>
		/// <param name="client">[optional] <see cref="AbstractClient"/> object, default is null.</param>
		public Stream(string streamName, AbstractClient client = null)
		{
			StreamName = streamName;

			KinveyClient = client ?? Client.SharedClient;

			mapPublishReceiverToChannel = new Dictionary<string, string>();
		}

		/// <summary>
		/// Grant access to the stream for publishing and/or subscribing to specified users and/or user groups.
		/// </summary>
		/// <returns>True if the grant request was successful, otherwise false.</returns>
		/// <param name="streamACL"><see cref="StreamAccessControlList"/> object used to determine which users should be granted access for publish and/or subscribe access.</param>
		/// <param name="ct">[optional] CancellationToken used to cancel the request.</param>
		public async Task<bool> GrantStreamAccess(string userID, StreamAccessControlList streamACL, CancellationToken ct = default(CancellationToken))
		{
			GrantStreamAccessRequest requestGrant = BuildGrantStreamAccessRequest(userID, streamACL);
			ct.ThrowIfCancellationRequested();
			var success = await requestGrant.ExecuteAsync();
			return (success != null);
		}

		#region Directed Communication

		/// <summary>
		/// In live stream directed communication, sends a message of type {T} to the specified user.
		/// </summary>
		/// <param name="toUserID">ID of the user that this message is being sent to.</param>
		/// <param name="message">Message to be sent.</param>
		public async Task<bool> Send(string toUserID, T message)
		{
			return await Publish(toUserID, message);
		}

		/// <summary>
		/// In live stream directed communication, receives messages of type {T} that are sent to the active users.
		/// </summary>
		/// <param name="liveServiceHandler">Delegate used to receive live service messages posted by the active user.</param>
		public async Task<bool> Listen(KinveyStreamDelegate<T> liveServiceHandler)
		{
			return await Subscribe(KinveyClient.ActiveUser.Id, liveServiceHandler);
		}

		/// <summary>
		/// In live stream directed communication, elects to stop receiving messages of type {T} sent to the active user.
		/// </summary>
		public async Task StopListening()
		{
			await Unsubscribe(KinveyClient.ActiveUser.Id);
		}

		#endregion

		#region Feed Communication

		/// <summary>
		/// In live stream feed communication, posts a message of type {T} to the active user.
		/// </summary>
		/// <param name="message">Message to be posted to the active user.</param>
		public async Task<bool> Post(T message)
		{
			return await Publish(KinveyClient.ActiveUser.Id, message);
		}

		/// <summary>
		/// In live stream feed communication, receives messages of type {T} posted by the specified user.
		/// </summary>
		/// <param name="userID">The ID of the user to follow.</param>
		/// <param name="liveServiceHandler">Delegate used to receive live service messages posted by the specified user.</param>
		public async Task<bool> Follow(string userID, KinveyStreamDelegate<T> liveServiceHandler)
		{
			return await Subscribe(userID, liveServiceHandler);
		}

		/// <summary>
		/// In live stream feed communication, elects to stop receiving messages of type {T} posted by the specified user.
		/// </summary>
		/// <param name="userID">The ID of the user to follow.</param>
		public async Task Unfollow(string userID)
		{
			await Unsubscribe(userID);
		}

		#endregion

		#region Send/Post helper methods

		internal async Task<bool> Publish(string receiverID, T message)
		{
			Func<KinveyException, Task> publishError = async (error) => {
				KinveyUtils.Logger.Log("publish error");

				if (error.ErrorCode == EnumErrorCode.ERROR_REALTIME_CRITICAL_NOT_AUTHORIZED_ON_CHANNEL)
				{
					// Clear out cached channel for receiver
					mapPublishReceiverToChannel.Remove(receiverID);

					// Attempt to re-request access once
					Func<KinveyException, Task> requestPublishError = (requestError) => {
						KinveyUtils.Logger.Log("publish retry error");
						RealtimeDelegate.OnError(error);
						return new Task<bool>(() => false);
					};

					bool success = await PublishHelper(receiverID, message, requestPublishError);
					if (success)
					{
						// Subscribe access was granted
						return;
					}
				}

				RealtimeDelegate.OnError(error);
			};

			return await PublishHelper(receiverID, message, publishError);
		}

		internal async Task<bool> PublishHelper(string receiverID, T message, Func<KinveyException, Task> publishError)
		{
			bool result = false;

			// If we do not have a channel for this receiverID, make KCS request for publish access for the
			// given receiverID.  KCS will return, if successful, a response including the PubNub channel name
			string publishChannel = String.Empty;
			if (mapPublishReceiverToChannel.ContainsKey(receiverID))
			{
				publishChannel = mapPublishReceiverToChannel[receiverID];
			}
			else
			{
				JObject response = await RequestPublishAccess(receiverID);

				if (response != null)
				{
					publishChannel = response[Constants.STR_REALTIME_PUBLISH_SUBSTREAM_CHANNEL_NAME].ToString();
					mapPublishReceiverToChannel.Add(receiverID, publishChannel);
				}
			}

			if (!String.IsNullOrEmpty(publishChannel))
			{
				var realtimeMessage = new RealtimeMessage<T>(KinveyClient.ActiveUser.Id, message);
				result = await RealtimeRouter.Instance.Publish(publishChannel, realtimeMessage, publishError);
			}

			return result;
		}

		internal async Task<JObject> RequestPublishAccess(string receiverID, CancellationToken ct = default(CancellationToken))
		{
			StreamPublishAccessRequest requestPublish = BuildStreamPublishAccessRequest(receiverID);
			ct.ThrowIfCancellationRequested();
			return await requestPublish.ExecuteAsync();
		}

		#endregion

		#region Listen/Follow helper methods

		internal async Task<bool> Subscribe(string subscribeID, KinveyStreamDelegate<T> realtimeHandler)
		{
			bool success = false;
			bool attemptRetry = false;

			if (realtimeHandler == null)
			{
				// No callback was supplied
				return success;
			}

			// Make KCS request for subscribe access to a user with the given subscribeID
			var result = await RequestSubscribeAccess(subscribeID);

			if (result != null)
			{
				RealtimeDelegate = realtimeHandler;

				var routerDelegate = new KinveyRealtimeDelegate
				{
					OnError = async (error) => {
						var kinveyException = error as KinveyException;

						if (kinveyException != null &&
							kinveyException.ErrorCode == EnumErrorCode.ERROR_REALTIME_CRITICAL_NOT_AUTHORIZED_ON_CHANNEL)
						{
							if (attemptRetry)
							{
								// If we have not attempted to re-subscribe, try once
								attemptRetry = false;

								// Make KCS request for subscribe access to a user with the given subscribeID
								result = await RequestSubscribeAccess(subscribeID);
                                if (result != null)
								{
									// Re-request failed, unsubscribe stream
                                    RealtimeRouter.Instance.UnsubscribeStream(ChannelName);
								}
								else
								{
									// Re-request was succssful, reset retry flag
									attemptRetry = true;
									return;
								}
							}
						}

						RealtimeDelegate.OnError(error);
					},
					OnNext = (message) => {
						var realtimeMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<RealtimeMessage<T>>(message);
						RealtimeDelegate.OnNext(realtimeMessage.SenderID, realtimeMessage.Message);
					},
					OnStatus = (status) => RealtimeDelegate.OnStatus(status)
				};

                // Get stream name from subscribe request
                ChannelName = result[Constants.STR_REALTIME_PUBLISH_SUBSTREAM_CHANNEL_NAME].ToString();
				RealtimeRouter.Instance.SubscribeStream(ChannelName, routerDelegate);
				attemptRetry = true;
				success = true;
			}

			return success;
		}

		internal async Task Unsubscribe(string subscribeID)
		{
			RealtimeRouter.Instance.UnsubscribeStream(ChannelName);
			RealtimeDelegate = null;

			// Make KCS request to unsubscribe access to a substream for the given subscribeID
			bool success = await RequestUnsubscribeAccess(subscribeID);
		}

        internal async Task<JObject> RequestSubscribeAccess(string subscribeID, CancellationToken ct = default(CancellationToken))
		{
			StreamSubscribeAccessRequest requestSubscribe = BuildStreamSubscribeAccessRequest(subscribeID);
			ct.ThrowIfCancellationRequested();
			var result =  await requestSubscribe.ExecuteAsync();
			return result;
		}

		internal async Task<bool> RequestUnsubscribeAccess(string subscribeID, CancellationToken ct = default(CancellationToken))
		{
			StreamUnsubscribeAccessRequest requestUnsubscribe = BuildStreamUnsubscribeAccessRequest(subscribeID);
			ct.ThrowIfCancellationRequested();
			var result = await requestUnsubscribe.ExecuteAsync();
			return true;
		}

		#endregion

		#region Stream request builders

		private GrantStreamAccessRequest BuildGrantStreamAccessRequest(string userID, StreamAccessControlList streamACL)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add(Constants.STR_APP_KEY, ((KinveyClientRequestInitializer)KinveyClient.RequestInitializer).AppKey);
			urlParameters.Add(Constants.STR_REALTIME_STREAM_NAME, StreamName);
			urlParameters.Add("userID", userID);

			var requestStreamGrantAccess = new GrantStreamAccessRequest(streamACL, KinveyClient, urlParameters);
			KinveyClient.InitializeRequest(requestStreamGrantAccess);

			return requestStreamGrantAccess;
		}

		private StreamPublishAccessRequest BuildStreamPublishAccessRequest(string receiverID)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add(Constants.STR_APP_KEY, ((KinveyClientRequestInitializer)KinveyClient.RequestInitializer).AppKey);
			urlParameters.Add(Constants.STR_REALTIME_STREAM_NAME, StreamName);
			urlParameters.Add("userID", receiverID);

			var requestStreamPublish = new StreamPublishAccessRequest(receiverID, KinveyClient, urlParameters);
			KinveyClient.InitializeRequest(requestStreamPublish);

			return requestStreamPublish;
		}

		private StreamSubscribeAccessRequest BuildStreamSubscribeAccessRequest(string subscribeID)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add(Constants.STR_APP_KEY, ((KinveyClientRequestInitializer)KinveyClient.RequestInitializer).AppKey);
			urlParameters.Add(Constants.STR_REALTIME_STREAM_NAME, StreamName);
			urlParameters.Add("userID", subscribeID);

			var requestStreamSubscribe = new StreamSubscribeAccessRequest(subscribeID, KinveyClient, urlParameters);
			KinveyClient.InitializeRequest(requestStreamSubscribe);

			return requestStreamSubscribe;
		}

		private StreamUnsubscribeAccessRequest BuildStreamUnsubscribeAccessRequest(string subscribeID)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add(Constants.STR_APP_KEY, ((KinveyClientRequestInitializer)KinveyClient.RequestInitializer).AppKey);
			urlParameters.Add(Constants.STR_REALTIME_STREAM_NAME, StreamName);
			urlParameters.Add("userID", KinveyClient.ActiveUser.Id);

			var requestStreamUnsubscribe = new StreamUnsubscribeAccessRequest(subscribeID, KinveyClient, urlParameters);
			KinveyClient.InitializeRequest(requestStreamUnsubscribe);

			return requestStreamUnsubscribe;
		}

		#endregion

		#region Stream request classes

		// Build request to grant access to the stream
		private class GrantStreamAccessRequest : AbstractKinveyClientRequest<JObject>
		{
			private const string REST_PATH = "stream/{appKey}/{streamName}/{userID}";

			//[JsonProperty]
			//private string userID;

			internal GrantStreamAccessRequest(StreamAccessControlList streamACL, AbstractClient client, Dictionary<string, string> urlProperties) :
				base(client, "PUT", REST_PATH, default(JObject), urlProperties)
			{
				//this.userID = userID;

				JObject requestPayload = new JObject();
				//requestPayload.Add(Constants.STR_REALTIME_DEVICEID, deviceID);
				base.HttpContent = streamACL;
			}
		}

		// Build request to publish to the substream
		private class StreamPublishAccessRequest : AbstractKinveyClientRequest<JObject>
		{
			private const string REST_PATH = "stream/{appKey}/{streamName}/{userID}/publish";

			[JsonProperty("userId")]
			private string userID;

			internal StreamPublishAccessRequest(string subscribeID, AbstractClient client, Dictionary<string, string> urlProperties) :
				base(client, "POST", REST_PATH, default(JObject), urlProperties)
			{
				this.userID = subscribeID;

				JObject requestPayload = new JObject();
				requestPayload.Add("userId", subscribeID);
				base.HttpContent = requestPayload;
			}
		}

		// Build request to subscribe to the substream
		private class StreamSubscribeAccessRequest : AbstractKinveyClientRequest<JObject>
		{
			private const string REST_PATH = "stream/{appKey}/{streamName}/{userID}/subscribe";

			[JsonProperty("userId")]
			private string userID;

			internal StreamSubscribeAccessRequest(string subscribeID, AbstractClient client, Dictionary<string, string> urlProperties) :
				base(client, "POST", REST_PATH, default(JObject), urlProperties)
			{
				this.userID = subscribeID;

				JObject requestPayload = new JObject();
				requestPayload.Add("userId", subscribeID);
				requestPayload.Add(Constants.STR_REALTIME_DEVICEID, client.DeviceID);
				base.HttpContent = requestPayload;
			}
		}

		// Build request to unsubscribe from the substream
		private class StreamUnsubscribeAccessRequest : AbstractKinveyClientRequest<JObject>
		{
			private const string REST_PATH = "stream/{appKey}/{streamName}/{userID}/unsubscribe";

			[JsonProperty("userId")]
			private string userID;

			internal StreamUnsubscribeAccessRequest(string subscribeID, AbstractClient client, Dictionary<string, string> urlProperties) :
				base(client, "POST", REST_PATH, default(JObject), urlProperties)
			{
				this.userID = subscribeID;

				JObject requestPayload = new JObject();
				requestPayload.Add("userId", subscribeID);
				requestPayload.Add(Constants.STR_REALTIME_DEVICEID, client.DeviceID);
				base.HttpContent = requestPayload;
			}
		}

		#endregion
	}
}
