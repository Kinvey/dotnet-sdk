using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kinvey.Tests
{
    [TestClass]
    public class SQLiteSyncQueueTests : BaseTestClass
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
        public async Task TestPopSuccess()
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

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            fc1 = await syncStore.SaveAsync(fc1);

            // Act
            var removedPendingWriteAction = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).Pop();

            //Assert
            Assert.AreEqual("POST", removedPendingWriteAction.action);
            Assert.AreEqual(toDosCollection, removedPendingWriteAction.collection);
        }

        [TestMethod]
        public async Task TestPopFail()
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

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            fc1 = await syncStore.SaveAsync(fc1);

            kinveyClient.CacheManager.GetSyncQueue(toDosCollection).Pop();

            // Act
            var removedPendingWriteAction = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).Pop();

            //Assert
            Assert.IsNull(removedPendingWriteAction);
        }
    }
}
