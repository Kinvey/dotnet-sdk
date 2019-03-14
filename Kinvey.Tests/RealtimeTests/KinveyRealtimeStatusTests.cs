using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kinvey.Tests
{
    [TestClass]
    public class KinveyRealtimeStatusTests
    {
        private const int Status= 0;
        private const string Message= "Message";
        private const string TimeStamp = "TimeStamp";
        private const string Channel= "Channel";
        private const string ChannelGroup= "ChannelGroup";

        [TestMethod]
        public  void TestConstructorStatusConnect()
        {
            // Arrange
            var realtimeStatusType = KinveyRealtimeStatus.StatusType.STATUS_CONNECT;

            // Act
            var kinveyRealtimeStatusObj = new KinveyRealtimeStatus(realtimeStatusType, new string[3] { Status.ToString(), Message, ChannelGroup });

            // Assert
            Assert.AreEqual(realtimeStatusType, kinveyRealtimeStatusObj.RealtimeStatusType);
            Assert.AreEqual(Status, kinveyRealtimeStatusObj.Status);
            Assert.AreEqual(Message, kinveyRealtimeStatusObj.Message);
            Assert.IsNull(kinveyRealtimeStatusObj.TimeStamp);
            Assert.IsNull(kinveyRealtimeStatusObj.Channel);
            Assert.AreEqual(ChannelGroup, kinveyRealtimeStatusObj.ChannelGroup);                     
        }


        [TestMethod]
        public void TestConstructorStatusDisconnect()
        {
            // Arrange
            var realtimeStatusType = KinveyRealtimeStatus.StatusType.STATUS_DISCONNECT;

            // Act
            var kinveyRealtimeStatusObj = new KinveyRealtimeStatus(realtimeStatusType, new string[3] { Status.ToString(), Message, ChannelGroup });

            // Assert
            Assert.AreEqual(realtimeStatusType, kinveyRealtimeStatusObj.RealtimeStatusType);
            Assert.AreEqual(Status, kinveyRealtimeStatusObj.Status);
            Assert.AreEqual(Message, kinveyRealtimeStatusObj.Message);
            Assert.IsNull(kinveyRealtimeStatusObj.TimeStamp);
            Assert.IsNull(kinveyRealtimeStatusObj.Channel);
            Assert.AreEqual(ChannelGroup, kinveyRealtimeStatusObj.ChannelGroup);
        }

        [TestMethod]
        public void TestConstructorStatusPublish()
        {
            // Arrange
            var realtimeStatusType = KinveyRealtimeStatus.StatusType.STATUS_PUBLISH;

            // Act
            var kinveyRealtimeStatusObj = new KinveyRealtimeStatus(realtimeStatusType, new string[4] { Status.ToString(), Message, TimeStamp, Channel});

            // Assert
            Assert.AreEqual(realtimeStatusType, kinveyRealtimeStatusObj.RealtimeStatusType);
            Assert.AreEqual(Status, kinveyRealtimeStatusObj.Status);
            Assert.AreEqual(Message, kinveyRealtimeStatusObj.Message);
            Assert.AreEqual(TimeStamp, kinveyRealtimeStatusObj.TimeStamp);
            Assert.AreEqual(Channel, kinveyRealtimeStatusObj.Channel);
            Assert.IsNull(kinveyRealtimeStatusObj.ChannelGroup);
        }
    }
}
