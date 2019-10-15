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
using System.Collections.Generic;
using System.Threading.Tasks;

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

        static internal void Initialize(string channelGroup, string publishKey, string subscribeKey, string authKey, AbstractClient client, RealtimeReconnectionPolicy realtimeReconnectionPolicy)
        {
            if (instance == null)
            {
                lock (lockObject)
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
                            PNconfig.Secure = true;
                            PNconfig.ReconnectionPolicy = realtimeReconnectionPolicy.ConvertToPNReconnectionPolicy();
                            instance.pubnubClient = new PubnubApi.Pubnub(PNconfig);

                            instance.subscribeCallback = new PubnubApi.SubscribeCallbackExt(
                                (pubnubObj, message) => { instance.SubscribeCallback(message.Channel, message.Message as string); },
                                (pubnubObj, presence) => { /* presence not currently supported */}, // TODO Support PubNub presence
                                (pubnubObj, status) => 
                                {
                                    instance.HandleStatusMessage(status);
                                }
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
                lock (lockObject)
                {
                    if (instance != null)
                    {
                        instance.pubnubClient.RemoveListener(instance.subscribeCallback);

                        instance.pubnubClient.Unsubscribe<string>().ChannelGroups(new string[] { instance.ChannelGroup }).Execute();

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

        internal async Task<bool> Publish(string channel, object message, Func<KinveyException, Task> errorCallback)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            var task = Task.Run(() => 
		        pubnubClient.Publish()
		                    .Channel(channel)
		                    .Message(message)
		                    .Async(new PubnubApi.PNPublishResultExt((result, status) => {
		                        if (status.Error)
		                        {
		                            // throw KinveyException
		                            var ke = HandleStatusMessage(status);
		                            errorCallback.Invoke(ke);
                                    taskCompletionSource.SetResult(false);
		                        }
		                        else
		                        {
									taskCompletionSource.SetResult(true);
		                        }
	                        }))
            );

            return await taskCompletionSource.Task;
        }

        internal void SubscribeCollection(string collectionName, KinveyRealtimeDelegate callback)
        {
            string appKey = (KinveyClient.RequestInitializer as KinveyClientRequestInitializer).AppKey;
            string channel = appKey + Constants.CHAR_PERIOD + Constants.STR_REALTIME_COLLECTION_CHANNEL_PREPEND + collectionName;
            AddChannel(channel, callback);

            // In the case of collection subscription, in addition to creating a
            // channel for the collection, another channel should be created for
            // this collection and the active user, since certain ACL rules may
            // not allow collection-wide subscription, but may allow this user
            // access to the update (see MLIBZ-2223 for more information).
            string activeUserChannel = BuildCollectionUserChannel(collectionName, Client.SharedClient.ActiveUser.Id);
            AddChannel(activeUserChannel, callback);
        }

        internal void UnsubscribeCollection(string collectionName)
        {
            // Remove specifically-created active user channel (see MLIBZ-2223 for more information)
            string activeUserChannel = BuildCollectionUserChannel(collectionName, Client.SharedClient.ActiveUser.Id);
            RemoveChannel(activeUserChannel);

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
	        var callback = mapChannelToCallback[channel].OnNext;

	        callback.Invoke(msgResult);
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

        string BuildCollectionUserChannel(string collectionName, string userId)
        {
            string appKey = (KinveyClient.RequestInitializer as KinveyClientRequestInitializer).AppKey;
            string collectionChannel = Constants.STR_REALTIME_COLLECTION_CHANNEL_PREPEND + collectionName;
            string userChannel = Constants.STR_REALTIME_USER_CHANNEL_PREPEND + userId;
            return appKey + Constants.CHAR_PERIOD + collectionChannel + Constants.CHAR_PERIOD + userChannel;
        }

        KinveyException HandleStatusMessage(PubnubApi.PNStatus status)
        {
            KinveyException ke = default(KinveyException);

            if (status.Error)
            {
                // Status indicates an error with PubNub
                var errorData = status.ErrorData;
                if (status.StatusCode == 403)
                {
                    ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME,
                                             EnumErrorCode.ERROR_REALTIME_CRITICAL_NOT_AUTHORIZED_ON_CHANNEL,
                                             errorData.Information,
                                             errorData.Throwable);
                }
                else
                {
                    ke = new KinveyException(EnumErrorCategory.ERROR_REALTIME,
                                             EnumErrorCode.ERROR_REALTIME_ERROR,
                                             errorData.Information,
                                             errorData.Throwable);
                }
            }
            else
            {
                // TODO Figure out which status operations need to be logged.
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
            }

            return ke;
        }

        #endregion
    }
}
