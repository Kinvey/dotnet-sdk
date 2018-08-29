using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Kinvey;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Newtonsoft.Json;

namespace Kinvey.Tests
{
    [TestClass]
    public class DataStoreUnitTests
    {
        [TestMethod]
        public void TestSync()
        {
            var moqRestClient = new Mock<HttpClientHandler>();

            Client.Builder clientBuilder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret)
                .setFilePath(TestSetup.db_dir)
                .setOfflinePlatform(new SQLite.Net.Platform.Generic.SQLitePlatformGeneric())
                .SetRestClient(new HttpClient(moqRestClient.Object));

            Client client = clientBuilder.Build();

            if (client.ActiveUser != null)
                client.ActiveUser.Logout();

            {
                var moqResponse = new HttpResponseMessage();

                JObject moqResponseContent = new JObject
                {
                    ["_id"] = new Guid().ToString(),
                    ["username"] = new Guid().ToString()
                };

                var kmd = new JObject
                {
                    ["authtoken"] = new Guid().ToString()
                };
                moqResponseContent["_kmd"] = kmd;

                moqResponse.Content = new StringContent(JsonConvert.SerializeObject(moqResponseContent));
                moqResponse.StatusCode = System.Net.HttpStatusCode.OK; // Status Code - 504
                moqRestClient
                    .Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>()
                    )
                    .ReturnsAsync(moqResponse)
                    .Verifiable();
            }

            var user = User.LoginAsync(client).Result;

            var dataStore = DataStore<Person>.Collection("Person", DataStoreType.SYNC, client);
            dataStore.ClearCache();

            var person = new Person { FirstName = "Victor" };
            Assert.AreEqual(0, dataStore.GetSyncCount());
            person = dataStore.SaveAsync(person).Result;
            Assert.AreEqual(1, dataStore.GetSyncCount());

			{
                HttpRequestMessage request = null;
                moqRestClient
                    .Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>()
                    )
                    .Callback<HttpRequestMessage, CancellationToken>((req, token) => request = req)
                    .ReturnsAsync(() => new HttpResponseMessage
                    {
                        RequestMessage = request,
                        StatusCode = System.Net.HttpStatusCode.OK, // Status Code - 504
                        Content = new StringContent(JsonConvert.SerializeObject(new JObject
                        {
                            ["_id"] = Guid.NewGuid().ToString(),
                            ["FirstName"] = person.FirstName
                        }))
                    })
                    .Verifiable();
			}

            var syncResultTask = dataStore.SyncAsync();

            person.LastName = "Hugo";
            var person2 = dataStore.SaveAsync(person).Result;

            var syncResult = syncResultTask.Result;
            Assert.AreEqual(1, dataStore.GetSyncCount());
            Assert.AreEqual(1, syncResult.PushResponse.PushCount);
            Assert.AreEqual(1, syncResult.PushResponse.PushEntities.Count);

			person.LastName = "Barros";
			var person3 = dataStore.SaveAsync(person).Result;

			{
                HttpRequestMessage request = null;
                moqRestClient
                    .Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>()
                    )
                    .Callback<HttpRequestMessage, CancellationToken>((req, token) => request = req)
                    .ReturnsAsync(() => new HttpResponseMessage
                    {
                        RequestMessage = request,
                        StatusCode = System.Net.HttpStatusCode.OK, // Status Code - 504
                        Content = new StringContent(JsonConvert.SerializeObject(new JObject
                        {
                            ["_id"] = new Guid().ToString(),
                            ["LastName"] = person.LastName
                        }))
                    })
                    .Verifiable();
			}

            var syncResult2 = dataStore.SyncAsync().Result;
			Assert.AreEqual(0, dataStore.GetSyncCount());
			Assert.AreEqual(1, syncResult.PushResponse.PushCount);
			Assert.AreEqual(1, syncResult.PushResponse.PushEntities.Count);
        }
    }
}
