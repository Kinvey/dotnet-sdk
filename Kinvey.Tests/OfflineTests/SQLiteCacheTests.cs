using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kinvey.Tests.OfflineTests
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
            var syncStore = DataStore<FlashCard>.Collection(toDosCollection, DataStoreType.SYNC);

            var fc1 = new FlashCard
            {
                ID = Guid.NewGuid().ToString(),
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            var fc2 = new FlashCard
            {
                ID = Guid.NewGuid().ToString(),
                Question = "What is 3 + 5?",
                Answer = "8"
            };

            var listFlashCards = new List<FlashCard> { fc1, fc2 };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            kinveyClient.CacheManager.GetCache<FlashCard>(toDosCollection).Save(listFlashCards);

            var existingFlashCards = kinveyClient.CacheManager.GetCache<FlashCard>(toDosCollection).FindAll();

            //Assert
            Assert.IsNotNull(existingFlashCards);
            Assert.AreEqual(2, existingFlashCards.Count);
            Assert.IsNotNull(existingFlashCards.Find(e=> e.Answer.Equals(fc1.Answer)));
            Assert.IsNotNull(existingFlashCards.Find(e => e.Answer.Equals(fc2.Answer)));
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
            var syncStore = DataStore<FlashCard>.Collection(toDosCollection, DataStoreType.SYNC);

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            var fc2 = new FlashCard
            {
                Question = "What is 3 + 5?",
                Answer = "8"
            };

            var listFlashCards = new List<FlashCard> { fc1, fc2 };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            var exception = Assert.ThrowsException<KinveyException>(delegate
            {
                kinveyClient.CacheManager.GetCache<FlashCard>(toDosCollection).Save(listFlashCards);
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
