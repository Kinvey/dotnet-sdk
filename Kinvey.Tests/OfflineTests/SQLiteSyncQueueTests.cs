using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var task1 = new ToDo
            {
                Name = "TestName",
                Details = "TestDetails"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            task1 = await syncStore.SaveAsync(task1);

            // Act
            var removedPendingWriteAction = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).Pop();
            var count = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).Count(false);

            //Assert
            Assert.AreEqual(task1.ID, removedPendingWriteAction.entityId);
            Assert.AreEqual("POST", removedPendingWriteAction.action);
            Assert.AreEqual(toDosCollection, removedPendingWriteAction.collection);
            Assert.AreEqual(0, count);
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
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var task1 = new ToDo
            {
                Name = "TestName",
                Details = "TestDetails"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            task1 = await syncStore.SaveAsync(task1);

            kinveyClient.CacheManager.GetSyncQueue(toDosCollection).Pop();

            // Act
            var removedPendingWriteAction = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).Pop();

            //Assert
            Assert.IsNull(removedPendingWriteAction);
        }
    }
}
