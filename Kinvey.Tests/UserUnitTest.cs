using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Kinvey.Tests
{
    [TestClass]
    public class UserUnitTest : KinveyUnitTest
    {
        [TestMethod]
        public async Task TestSignup()
        {
            var client = Client;

            Assert.IsNull(client.ActiveUser);

            if (UseMock)
            {
                client.HttpClient = MockHttpClient((request) =>
                {
                    Assert.AreEqual(request.Method, HttpMethod.Post);
                    Assert.AreEqual(request.RequestUri.PathAndQuery, $"/user/{client.AppKey}");
                    var json = JObject.FromObject(new
                    {
                        _id = Guid.NewGuid().ToString(),
                        username = Guid.NewGuid().ToString(),
                        password = Guid.NewGuid().ToString(),
                        _kmd = new
                        {
                            lmt = "2018-06-18T23:56:09.079Z",
                            ect = "2018-06-18T23:56:09.079Z",
                            authtoken = Guid.NewGuid().ToString()
                        },
                        _acl = new
                        {
                            creator = Guid.NewGuid().ToString()
                        }
                    });
                    return MockResponse(json);
                });
            }

            var user = await User.SignupAsync(options: new Options() { Client = client });
            Assert.IsNotNull(user);
            Assert.IsNotNull(user?.userId);
            Assert.IsNotNull(user?.Username);
            Assert.IsNotNull(user?.Metadata);
            Assert.IsNotNull(user?.Metadata?.EntityCreationTime);
            Assert.IsNotNull(user?.Metadata?.LastModifiedTime);
            Assert.IsNotNull(user?.Metadata?.Authtoken);

            Assert.IsNotNull(client.ActiveUser);

            Assert.AreSame(user, client.ActiveUser);
        }
    }
}
