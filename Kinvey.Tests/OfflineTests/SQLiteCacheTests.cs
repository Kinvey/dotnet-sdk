using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kinvey.Tests
{
    [TestClass]
    public class SQLiteCacheTests : BaseTestClass
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
        public async Task TestSaveListSuccess()
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
                ID = Guid.NewGuid().ToString(),
                Name = "TestName1",
                Details = "TestDetails1"
            };

            var task2 = new ToDo
            {
                ID = Guid.NewGuid().ToString(),
                Name = "TestName2",
                Details = "TestDetails2"
            };

            var listTasks = new List<ToDo> { task1, task2 };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            kinveyClient.CacheManager.GetCache<ToDo>(toDosCollection).Save(listTasks);

            var existingToDos = kinveyClient.CacheManager.GetCache<ToDo>(toDosCollection).FindAll();

            //Assert
            Assert.IsNotNull(existingToDos);
            Assert.AreEqual(2, existingToDos.Count);
            Assert.IsNotNull(existingToDos.Find(e => e.Name.Equals(task1.Name)));
            Assert.IsNotNull(existingToDos.Find(e => e.Name.Equals(task2.Name)));
        }

        [TestMethod]
        public async Task TestSaveListFail()
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

            var task2 = new ToDo
            {
                Name = "TestName2",
                Details = "TestDetails2"
            };

            var listTasks = new List<ToDo> { task1, task2 };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            var exception = Assert.ThrowsException<KinveyException>(delegate
            {
                kinveyClient.CacheManager.GetCache<ToDo>(toDosCollection).Save(listTasks);
            });

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.GetType() , typeof(KinveyException));
            var ke = exception as KinveyException;
            Assert.AreEqual(EnumErrorCategory.ERROR_DATASTORE_CACHE, ke.ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_DATASTORE_CACHE_SAVE_INSERT_ENTITY, ke.ErrorCode);
        }
    }
}
