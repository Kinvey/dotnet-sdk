using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Kinvey.Tests
{
    [TestClass]
    public class ClientUnitTest : KinveyUnitTest
    {

        [TestMethod]
        public void TestNewClientNoParams()
        {
            Assert.IsNotNull(new Client());
        }

        [TestMethod]
        public void TestNewClientAppKeyAppSecret()
        {
            Assert.IsNotNull(new Client("appKey", "appSecret"));
            Assert.IsNotNull(new Client(appSecret: "appSecret", appKey: "appKey"));
        }

        [TestMethod]
        public void TestNewClientAppKeyAppSecretInstanceId()
        {
            Assert.IsNotNull(new Client("appKey", "appSecret", "instanceId"));
        }

        [DataTestMethod]
        [DataRow(null, "appSecret")]
        [DataRow("appKey", null)]
        [ExpectedException(typeof(ArgumentException))]
        public void TestNewClientNullAppKey(string appKey, string appSecret)
        {
            var client = new Client(appKey, appSecret);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task TestPingClientNotInitialized()
        {
            await new Client().PingAsync();
        }

        [TestMethod]
        public async Task TestPing()
        {
            var client = Client;

            EnvironmentInfo expectedEnvironmentInfo = new EnvironmentInfo();
            if (UseMock)
            {
                expectedEnvironmentInfo.AppName = "AppName";
                expectedEnvironmentInfo.Version = "Version";
                expectedEnvironmentInfo.Kinvey = "Kinvey";
                expectedEnvironmentInfo.EnvironmentName = "EnvironmentName";

                client.HttpClient = MockHttpClient((request) => {
                    Assert.AreEqual(request.RequestUri.PathAndQuery, $"/appdata/{client.AppKey}");
                    return MockResponse(expectedEnvironmentInfo);
                });
            }
            else
            {
                expectedEnvironmentInfo.AppName = EnvironmentInfoAppName;
                expectedEnvironmentInfo.Version = EnvironmentInfoVersion;
                expectedEnvironmentInfo.Kinvey = EnvironmentInfoKinvey;
                expectedEnvironmentInfo.EnvironmentName = EnvironmentInfoEnvironmentName;
            }

            var environmentInfo = await client.PingAsync();
            Assert.AreEqual(environmentInfo.AppName, expectedEnvironmentInfo.AppName);
            Assert.AreEqual(environmentInfo.Version, expectedEnvironmentInfo.Version);
            Assert.AreEqual(environmentInfo.Kinvey, expectedEnvironmentInfo.Kinvey);
            Assert.AreEqual(environmentInfo.EnvironmentName, expectedEnvironmentInfo.EnvironmentName);
        }
    }
}
