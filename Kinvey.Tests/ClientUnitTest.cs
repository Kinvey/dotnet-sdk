using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;

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
            var client = new Client(AppKey ?? "_kid_", AppSecret ?? "AppSecret");
            EnvironmentInfo expectedEnvironmentInfo = new EnvironmentInfo();
            var environmentInfoType = typeof(EnvironmentInfo);
            var appNameProperty = environmentInfoType.GetProperty("AppName");
            var versionProperty = environmentInfoType.GetProperty("Version");
            var kinveyProperty = environmentInfoType.GetProperty("Kinvey");
            var envNameProperty = environmentInfoType.GetProperty("EnvironmentName");
            if (UseMock)
            {
                appNameProperty.SetValue(expectedEnvironmentInfo, "AppName");
                versionProperty.SetValue(expectedEnvironmentInfo, "Version");
                kinveyProperty.SetValue(expectedEnvironmentInfo, "Kinvey");
                envNameProperty.SetValue(expectedEnvironmentInfo, "EnvironmentName");

                client.HttpClient = MockHttpClient((request) => {
                    Assert.AreEqual(request.RequestUri.PathAndQuery, $"/appdata/{client.AppKey}");
                    return MockResponse(expectedEnvironmentInfo);
                });
            }
            else
            {
                appNameProperty.SetValue(expectedEnvironmentInfo, EnvironmentInfoAppName);
                versionProperty.SetValue(expectedEnvironmentInfo, EnvironmentInfoVersion);
                kinveyProperty.SetValue(expectedEnvironmentInfo, EnvironmentInfoKinvey);
                envNameProperty.SetValue(expectedEnvironmentInfo, EnvironmentInfoEnvironmentName);
            }

            var environmentInfo = await client.PingAsync();
            Assert.AreEqual(environmentInfo.AppName, expectedEnvironmentInfo.AppName);
            Assert.AreEqual(environmentInfo.Version, expectedEnvironmentInfo.Version);
            Assert.AreEqual(environmentInfo.Kinvey, expectedEnvironmentInfo.Kinvey);
            Assert.AreEqual(environmentInfo.EnvironmentName, expectedEnvironmentInfo.EnvironmentName);
        }
    }
}
