using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kinvey.Tests.OfflineTests
{
    [TestClass]
    public class SQLiteCacheManagerTests : BaseTestClass
    {
        private const string toDosCollection = "ToDos";
        private Client kinveyClient;

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();

            var builder = ClientBuilder
                .SetFilePath(TestSetup.db_dir);

            if (MockData)
            {
                builder.setBaseURL("http://localhost:8080");
            }

            kinveyClient = builder.Build();
        }

        [TestMethod]
        public async Task TestDeleteQueryCacheItem()
        {
            // Setup
            if (MockData)
            {
                MockResponses(1, kinveyClient);
            }

            //Arrange
            var syncStore = DataStore<FlashCard>.Collection(toDosCollection, DataStoreType.SYNC);

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            var queryQacheItem = new QueryCacheItem
            {
                collectionName = toDosCollection,
                lastRequest = "lastRequest",
                query = "query"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            fc1 = await syncStore.SaveAsync(fc1);

            kinveyClient.CacheManager.SetQueryCacheItem(queryQacheItem);
            queryQacheItem = kinveyClient.CacheManager.GetQueryCacheItem(toDosCollection, queryQacheItem.query, queryQacheItem.lastRequest);

            // Act
            var result = kinveyClient.CacheManager.DeleteQueryCacheItem(queryQacheItem);

            //Assert
            Assert.IsTrue(result);
        }
    }
}
