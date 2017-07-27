using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Moq;
using Kinvey;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace TestFramework
{
    [TestFixture]
    public class DataStoreUnitTests
    {
        [Test]
        public void TestSync()
        {
            Mock<RestSharp.IRestClient> moqRestClient = new Mock<RestSharp.IRestClient>();

            Client.Builder clientBuilder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret)
                .setFilePath(TestSetup.db_dir)
                .setOfflinePlatform(new SQLite.Net.Platform.Generic.SQLitePlatformGeneric())
                .SetRestClient(moqRestClient.Object);

            Client client = clientBuilder.Build();

            if (client.ActiveUser != null)
                client.ActiveUser.Logout();

            {
                RestSharp.IRestResponse moqResponse = new RestSharp.RestResponse();

                JObject moqResponseContent = new JObject();
                moqResponseContent["_id"] = new Guid().ToString();
                moqResponseContent["username"] = new Guid().ToString();

                var kmd = new JObject();
                kmd["authtoken"] = new Guid().ToString();
                moqResponseContent["_kmd"] = kmd;

                moqResponse.Content = moqResponseContent.ToString();
                moqResponse.StatusCode = System.Net.HttpStatusCode.OK; // Status Code - 504
                moqRestClient.Setup(m => m.ExecuteAsync(It.IsAny<RestSharp.IRestRequest>())).ReturnsAsync(moqResponse);
            }

            var user = User.LoginAsync(client).Result;

            var dataStore = DataStore<Person>.Collection("Person", DataStoreType.SYNC, client);
            dataStore.ClearCache();

            var person = new Person { FirstName = "Victor" };
            Assert.AreEqual(0, dataStore.GetSyncCount());
            person = dataStore.SaveAsync(person).Result;
            Assert.AreEqual(1, dataStore.GetSyncCount());

			{
				RestSharp.IRestResponse moqResponse = new RestSharp.RestResponse();

				JObject moqResponseContent = new JObject();
				moqResponseContent["_id"] = new Guid().ToString();
                moqResponseContent["FirstName"] = person.FirstName;

				moqResponse.Content = moqResponseContent.ToString();
				moqResponse.StatusCode = System.Net.HttpStatusCode.OK; // Status Code - 504
				moqRestClient.Setup(m => m.ExecuteAsync(It.IsAny<RestSharp.IRestRequest>())).ReturnsAsync(moqResponse);
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
				RestSharp.IRestResponse moqResponse = new RestSharp.RestResponse();

				JObject moqResponseContent = new JObject();
				moqResponseContent["_id"] = new Guid().ToString();
				moqResponseContent["LastName"] = person.LastName;

				moqResponse.Content = moqResponseContent.ToString();
				moqResponse.StatusCode = System.Net.HttpStatusCode.OK; // Status Code - 504
				moqRestClient.Setup(m => m.ExecuteAsync(It.IsAny<RestSharp.IRestRequest>())).ReturnsAsync(moqResponse);
			}

            var syncResult2 = dataStore.SyncAsync().Result;
			Assert.AreEqual(0, dataStore.GetSyncCount());
			Assert.AreEqual(1, syncResult.PushResponse.PushCount);
			Assert.AreEqual(1, syncResult.PushResponse.PushEntities.Count);
        }
    }
}
