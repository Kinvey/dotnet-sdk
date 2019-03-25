using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Kinvey.Tests
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
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var task1 = new ToDo
            {
                Name = "TestName1",
                Details = "TestDetails1"
            };

            var queryQacheItem = new QueryCacheItem
            {
                collectionName = toDosCollection,
                lastRequest = "lastRequest",
                query = "query"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            task1 = await syncStore.SaveAsync(task1);

            kinveyClient.CacheManager.SetQueryCacheItem(queryQacheItem);
            queryQacheItem = kinveyClient.CacheManager.GetQueryCacheItem(toDosCollection, queryQacheItem.query, queryQacheItem.lastRequest);

            // Act
            var result = kinveyClient.CacheManager.DeleteQueryCacheItem(queryQacheItem);

            //Assert
            Assert.IsTrue(result);
        }
    }
}
