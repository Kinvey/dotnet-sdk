using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Kinvey.Tests
{
    [TestClass]
    public class PushTests : BaseTestClass
    {
        private Client kinveyClient;
        private AbstractPush testPush;
        private const string platform = "android";
        private string token;

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();

            Client.Builder builder = ClientBuilder
                .SetFilePath(TestSetup.db_dir);

            if (MockData)
            {
                builder.setBaseURL("http://localhost:8080");
            }

            kinveyClient = builder.Build();
            testPush = new TestPush(kinveyClient);
            token = Guid.NewGuid().ToString();
        }

        [TestMethod]
        public async Task TestEnablePushViaRestAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(2);
            }
            else
            {
                Assert.Fail("Use this test only with mocks.");
            }

            //Arrange
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            //Act
            var pushPayload = testPush.EnablePushViaRest(platform, token).Execute();

            //Assert
            Assert.IsNotNull(pushPayload);
        }

        [TestMethod]
        public async Task TestDisablePushViaRestAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(2);
            }
            else
            {
                Assert.Fail("Use this test only with mocks.");
            }

            //Arrange
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            //Act
            var pushPayload = testPush.DisablePushViaRest(platform, token).Execute();

            //Assert
            Assert.IsNotNull(pushPayload);
        }

        [TestMethod]
        public async Task TestEnablePushAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(2);
            }
            else
            {
                Assert.Fail("Use this test only with mocks.");
            }

            //Arrange
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            //Act
            var pushPayload = await testPush.EnablePushAsync(platform, token);

            //Assert
            Assert.IsNotNull(pushPayload);
        }

        [TestMethod]
        public async Task TestDisablePushAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(2);
            }
            else
            {
                Assert.Fail("Use this test only with mocks.");
            }

            //Arrange
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            //Act
            var pushPayload = await testPush.DisablePushAsync(platform, token);

            //Assert
            Assert.IsNotNull(pushPayload);
        }
    }
}
