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
	public class Stream<T> where T : IStreamable
	{
		KinveyRealtimeDelegate<T> RealtimeDelegate { get; set; }

		/// <summary>
		/// Represents the name of the stream.
		/// </summary>
		public string StreamName { get; set; }

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

		/// <summary>
		/// Publish a message of type {T} to the specified user.
		/// </summary>
		/// <param name="receiverID">Receiver identifier.</param>
		/// <param name="message">Message.</param>
		public async Task<bool> Publish(string receiverID, T message)
		{
			bool result = false;

			// Make KCS request for publish access for the given receiverID
			// KCS will return, if successful, a response which will include the PubNub channel name
			JObject response = await RequestPublishAccess(receiverID);

			if (response != null)
			{
				string publishChannel = response[Constants.STR_REALTIME_PUBLISH_SUBSTREAM_CHANNEL_NAME].ToString();
				result = RealtimeRouter.Instance.Publish(publishChannel, receiverID, message);
			}

			return result;
		}

		/// <summary>
		/// Subscribe the specified callback.
		/// </summary>
		/// <param name="subscribeID">The ID of the user to subscribe to.</param>
		/// <param name="realtimeHandler">Delegate used to forward realtime messages.</param>
		public async Task<bool> Subscribe(string subscribeID, KinveyRealtimeDelegate<T> realtimeHandler)
		{
			bool success = false;

			if (realtimeHandler == null)
			{
				// No callback was supplied
				return success;
			}

			// Make KCS request for subscribe access to a user with the given subscribeID
			success = await RequestSubscribeAccess(subscribeID);

			if (success)
			{
				RealtimeDelegate = realtimeHandler;

				KinveyRealtimeDelegate<string> routerCallback = new KinveyRealtimeDelegate<string>
				{
					onError = (error) => RealtimeDelegate.onError(error),
					onSuccess = (message) => {
						var messageObj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(message);
						RealtimeDelegate.onSuccess(messageObj);
					},
					OnStatus = (status) => RealtimeDelegate.OnStatus(status)
				};

				RealtimeRouter.Instance.SubscribeStream(StreamName, routerCallback);
				success = true;
			}

			return success;
		}

		/// <summary>
		/// Unsubscribe this instance.
		/// </summary>
		public async Task Unsubscribe(string subscribeID)
		{
			RealtimeRouter.Instance.UnsubscribeStream(StreamName);
			RealtimeDelegate = null;

			// Make KCS request to unsubscribe access to a substream for the given subscribeID
			bool success = await RequestUnsubscribeAccess(subscribeID);
		}

		internal async Task<JObject> RequestPublishAccess(string receiverID, CancellationToken ct = default(CancellationToken))
		{
			StreamPublishAccessRequest requestPublish = BuildStreamPublishAccessRequest(receiverID);
			ct.ThrowIfCancellationRequested();
			return await requestPublish.ExecuteAsync();
		}

		internal async Task<bool> RequestSubscribeAccess(string subscribeID, CancellationToken ct = default(CancellationToken))
		{
			StreamSubscribeAccessRequest requestSubscribe = BuildStreamSubscribeAccessRequest(subscribeID);
			ct.ThrowIfCancellationRequested();
			var result =  await requestSubscribe.ExecuteAsync();
			return true;
		}

		internal async Task<bool> RequestUnsubscribeAccess(string subscribeID, CancellationToken ct = default(CancellationToken))
		{
			StreamUnsubscribeAccessRequest requestUnsubscribe = BuildStreamUnsubscribeAccessRequest(subscribeID);
			ct.ThrowIfCancellationRequested();
			var result = await requestUnsubscribe.ExecuteAsync();
			return true;
		}

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
			urlParameters.Add("userID", KinveyClient.ActiveUser.Id);

			var requestStreamPublish = new StreamPublishAccessRequest(receiverID, KinveyClient, urlParameters);
			KinveyClient.InitializeRequest(requestStreamPublish);

			return requestStreamPublish;
		}

		private StreamSubscribeAccessRequest BuildStreamSubscribeAccessRequest(string subscribeID)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add(Constants.STR_APP_KEY, ((KinveyClientRequestInitializer)KinveyClient.RequestInitializer).AppKey);
			urlParameters.Add(Constants.STR_REALTIME_STREAM_NAME, StreamName);
			urlParameters.Add("userID", KinveyClient.ActiveUser.Id);

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
