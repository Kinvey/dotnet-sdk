using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kinvey.Tests
{
    [TestClass]
    public class RealtimeRouterTests
    {
        [TestMethod]
        public void TestHandleStatusMessageNotAuthorizedException()
        {
            //Arrange
            var status = new PubnubApi.PNStatus
            {
                Error = true,
                StatusCode = 403,
                ErrorData = new PubnubApi.PNErrorData("Test", null)
            };

            //RealtimeRouter.Initialize(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), null, RealtimeReconnectionPolicy.NONE);

            Type t = typeof(RealtimeRouter);

            ConstructorInfo ci = t.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, new Type[] { }, null);

            var instance = (RealtimeRouter)ci.Invoke(null);

            // Act
            var method = instance.GetType().GetMethod("HandleStatusMessage", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = method.Invoke(instance, new object[] { status });

            //Assert
            Assert.AreEqual(typeof(KinveyException), result.GetType());
            var ke = result as KinveyException;
            Assert.AreEqual(EnumErrorCategory.ERROR_REALTIME, ke.ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_REALTIME_CRITICAL_NOT_AUTHORIZED_ON_CHANNEL, ke.ErrorCode);
        }

        [TestMethod]
        public void TestHandleStatusMessageGeneralException()
        {
            //Arrange
            var status = new PubnubApi.PNStatus
            {
                Error = true,
                ErrorData = new PubnubApi.PNErrorData("Test", null)
            };

            Type t = typeof(RealtimeRouter);

            ConstructorInfo ci = t.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, new Type[] { }, null);

            var instance = (RealtimeRouter)ci.Invoke(null);

            // Act
            var method = instance.GetType().GetMethod("HandleStatusMessage", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = method.Invoke(instance, new object[] { status });

            //Assert
            Assert.AreEqual(typeof(KinveyException), result.GetType());
            var ke = result as KinveyException;
            Assert.AreEqual(EnumErrorCategory.ERROR_REALTIME, ke.ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_REALTIME_ERROR, ke.ErrorCode);
        }

        [TestMethod]
        public void TestHandleStatusMessagePNPublishOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PNPublishOperation);
        }

        [TestMethod]
        public void TestHandleStatusMessagePNSubscribeOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PNSubscribeOperation);
        }

        [TestMethod]
        public void TestHandleStatusMessagePNUnsubscribeOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PNUnsubscribeOperation);
        }

        [TestMethod]
        public void TestHandleStatusMessageChannelGroupAllGetOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.ChannelGroupAllGet);
        }

        [TestMethod]
        public void TestHandleStatusMessageChannelGroupAuditAccessOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.ChannelGroupAuditAccess);
        }
       
        [TestMethod]
        public void TestHandleStatusMessageChannelGroupGetOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.ChannelGroupGet);
        }


        [TestMethod]
        public void TestHandleStatusMessageChannelGroupGrantAccessOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.ChannelGroupGrantAccess);
        }

        [TestMethod]
        public void TestHandleStatusMessageChannelGroupRevokeAccessOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.ChannelGroupRevokeAccess);
        }

        [TestMethod]
        public void TestHandleStatusMessagePNAddChannelsToGroupOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PNAddChannelsToGroupOperation);
        }

        [TestMethod]
        public void TestHandleStatusMessagePNChannelGroupsOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PNChannelGroupsOperation);
        }

        [TestMethod]
        public void TestHandleStatusMessagePNChannelsForGroupOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PNChannelsForGroupOperation);
        }

        [TestMethod]
        public void TestHandleStatusMessagePNRemoveChannelsFromGroupOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PNRemoveChannelsFromGroupOperation);
        }

        [TestMethod]
        public void TestHandleStatusMessagePNRemoveGroupOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PNRemoveGroupOperation);
        }

        [TestMethod]
        public void TestHandleStatusMessagePNAccessManagerAuditOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PNAccessManagerAudit);
        }

        [TestMethod]
        public void TestHandleStatusMessagePNAccessManagerGrantOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PNAccessManagerGrant);
        }

        [TestMethod]
        public void TestHandleStatusMessagePresenceOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.Presence);
        }

        [TestMethod]
        public void TestHandleStatusMessagePresenceUnsubscribeOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PresenceUnsubscribe);
        }

        [TestMethod]
        public void TestHandleStatusMessagePushGetOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PushGet);
        }

        [TestMethod]
        public void TestHandleStatusMessagePushRegisteOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PushRegister);
        }

        [TestMethod]
        public void TestHandleStatusMessagePushRemoveOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PushRemove);
        }

        [TestMethod]
        public void TestHandleStatusMessagePushUnregisterOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PushUnregister);
        }

        [TestMethod]
        public void TestHandleStatusMessageLeaveOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.Leave);
        }

        [TestMethod]
        public void TestHandleStatusMessageNoneOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.None);
        }

        [TestMethod]
        public void TestHandleStatusMessageRevokeAccessOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.RevokeAccess);
        }

        [TestMethod]
        public void TestHandleStatusMessagePNFireOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PNFireOperation);
        }

        [TestMethod]
        public void TestHandleStatusMessagePNGetStateOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PNGetStateOperation);
        }


        [TestMethod]
        public void TestHandleStatusMessagePNHeartbeatOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PNHeartbeatOperation);
        }

        [TestMethod]
        public void TestHandleStatusMessagePNHereNowOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PNHereNowOperation);
        }

        [TestMethod]
        public void TestHandleStatusMessagePNHistoryOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PNHistoryOperation);
        }

        [TestMethod]
        public void TestHandleStatusMessagePNSetStateOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PNSetStateOperation);
        }

        [TestMethod]
        public void TestHandleStatusMessagePNTimeOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PNTimeOperation);
        }

        [TestMethod]
        public void TestHandleStatusMessagePNWhereNowOperation()
        {
            HandleStatusMessageValidOperation(PubnubApi.PNOperationType.PNWhereNowOperation);
        }
     
        private void HandleStatusMessageValidOperation(PubnubApi.PNOperationType operationType)
        {
            //Arrange
            var status = new PubnubApi.PNStatus
            {
                Operation = operationType
            };

            Type t = typeof(RealtimeRouter);

            ConstructorInfo ci = t.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, new Type[] { }, null);

            var instance = (RealtimeRouter)ci.Invoke(null);

            // Act
            var method = instance.GetType().GetMethod("HandleStatusMessage", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = method.Invoke(instance, new object[] { status });

            //Assert
            Assert.IsNull(result);
        }
    }
}
