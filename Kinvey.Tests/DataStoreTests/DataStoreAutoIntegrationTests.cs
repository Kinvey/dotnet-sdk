
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Kinvey.Tests
{
    [TestClass]
    public class DataStoreAutoIntegrationTests : BaseTestClass
    {
        private Client kinveyClient;             
                             
        private const string unreachableUrl = "http://localhost:12345/";
        private string kinveyUrl
        {
            get { return MockData ? "http://localhost:8080" : "https://baas.kinvey.com/"; }
        }

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();
        }

        private void SetRootUrlToKinveyClient(string url)
        {
            var property = kinveyClient.GetType().BaseType.BaseType.GetField("rootUrl", BindingFlags.NonPublic | BindingFlags.Instance);
            property.SetValue(kinveyClient, url);
        }

        #region Collection

        [TestMethod]
        public void TestCollectionWithDefaultParameters()
        {
            // Arrange
            kinveyClient = BuildClient();

            // Act
            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(toDosCollection);

            // Assert
            Assert.IsNotNull(todoStore);
            Assert.IsTrue(string.Equals(todoStore.CollectionName, toDosCollection));
            Assert.IsNotNull(todoStore.KinveyClient);
        }

        [TestMethod]
        public void TestCollectionWithParameters()
        {
            // Arrange
            kinveyClient = BuildClient();

            // Act
            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);

            // Assert
            Assert.IsNotNull(todoStore);
            Assert.IsTrue(string.Equals(todoStore.CollectionName, toDosCollection));
            Assert.AreEqual(todoStore.StoreType, DataStoreType.AUTO);
            Assert.IsNotNull(todoStore.KinveyClient);
        }

        #endregion Collection

        #region Remove by query

        [TestMethod]
        public async Task TestDeleteByQueryConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(7);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "Task1 to delete",
                Details = "Delete details1"
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 to delete",
                Details = "Delete details2"
            };
            var newItem3 = new ToDo
            {
                Name = "Task3 not to delete",
                Details = "Not delete details3"
            };

            var query = autoStore.Where(x => x.Details.StartsWith("Delete d"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);

            // Act            
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            var networkEntities = await networkStore.FindAsync();
            var localEntities = await syncStore.FindAsync();

            // Teardown
            await networkStore.RemoveAsync(savedItem3.ID);

            // Assert            
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.IsNotNull(networkEntities);
            Assert.IsNotNull(localEntities);
            Assert.AreEqual(2, kinveyDeleteResponse.count);
            Assert.AreEqual(1, networkEntities.Count);
            Assert.AreEqual(savedItem3.ID, networkEntities[0].ID);
            Assert.AreEqual(savedItem3.Name, networkEntities[0].Name);
            Assert.AreEqual(savedItem3.Details, networkEntities[0].Details);
            Assert.AreEqual(1, localEntities.Count);
            Assert.AreEqual(savedItem3.ID, localEntities[0].ID);
            Assert.AreEqual(savedItem3.Name, localEntities[0].Name);
            Assert.AreEqual(savedItem3.Details, localEntities[0].Details);
        }

        [TestMethod]
        public async Task TestDeleteByQueryInvalidPermissionsConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(2, kinveyClient);
            }

            // Arrange
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var query = autoStore.Where(x => x.Details.StartsWith("Delete d"));

            await User.LoginAsync(TestSetup.user_without_permissions, TestSetup.pass_for_user_without_permissions, kinveyClient);

            var newItem = new ToDo
            {
                Name = "Task1 to delete",
                Details = "Delete details1"
            };

            var savedItem = await syncStore.SaveAsync(newItem);

            // Act
            var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                await autoStore.RemoveAsync(query);
            });

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
            var existingToDos = await syncStore.FindAsync();

            // Assert
            Assert.AreEqual(typeof(KinveyException), exception.GetType());
            var ke = exception as KinveyException;
            Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, ke.ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, ke.ErrorCode);
            Assert.AreEqual(401, ke.StatusCode);

            Assert.IsNotNull(pendingWriteActions);
            Assert.AreEqual(0, pendingWriteActions.Count);
            Assert.IsNotNull(existingToDos);
            Assert.AreEqual(0, existingToDos.Count);
        }

        [TestMethod]
        public async Task TestDeleteByQueryDeleteItemsFromBackendAfterLocalDeletionConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(7);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "Task1 to delete",
                Details = "Delete details1"
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 to delete",
                Details = "Not delete"
            };

            var query = autoStore.Where(x => x.Details.StartsWith("Delete d"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);

            // Act
            var localDeleteResponse = await syncStore.RemoveAsync(query);
            var networkDeleteResponse = await autoStore.RemoveAsync(query);

            ToDo existingItem1 = null;
            KinveyException exception = null;
            try
            {
                existingItem1 = await networkStore.FindByIDAsync(savedItem1.ID);
            }
            catch (KinveyException kinveyException)
            {
                exception = kinveyException;
            }

            var networkEntities = await networkStore.FindAsync();
            var localEntities = await syncStore.FindAsync();

            // Teardown
            await networkStore.RemoveAsync(savedItem2.ID);

            // Assert            
            Assert.IsNotNull(localDeleteResponse);
            Assert.IsNotNull(networkDeleteResponse);
            Assert.IsNotNull(localEntities);
            Assert.IsNotNull(networkEntities);
            if (MockData)
            {
                Assert.IsNull(existingItem1);
            }
            else
            {
                Assert.IsNotNull(exception);
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, exception.ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, exception.ErrorCode);
            }
            Assert.AreEqual(1, localDeleteResponse.count);
            Assert.AreEqual(1, networkDeleteResponse.count);
            Assert.AreEqual(1, localEntities.Count);
            Assert.AreEqual(1, networkEntities.Count);
        }

        [TestMethod]
        public async Task TestDeleteByQueryNoItemsToBeDeletedConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(9);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "Task1 to delete",
                Details = "Delete details1"
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 to delete",
                Details = "Delete details2"
            };
            var newItem3 = new ToDo
            {
                Name = "Task3 not to delete",
                Details = "Not delete details3"
            };

            var query = autoStore.Where(x => x.Details.StartsWith("Test"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);

            // Act            
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            var networkEntities = await networkStore.FindAsync();
            var localEntities = await syncStore.FindAsync();

            // Teardown
            await networkStore.RemoveAsync(savedItem1.ID);
            await networkStore.RemoveAsync(savedItem2.ID);
            await networkStore.RemoveAsync(savedItem3.ID);

            // Assert            
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.IsNotNull(networkEntities);
            Assert.IsNotNull(localEntities);
            Assert.AreEqual(0, kinveyDeleteResponse.count);
            Assert.AreEqual(3, networkEntities.Count);
            Assert.AreEqual(3, localEntities.Count);
        }

        [TestMethod]
        public async Task TestDeleteByQueryNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "Task1 to delete",
                Details = "Delete details1"
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 to delete",
                Details = "Delete details2"
            };
            var newItem3 = new ToDo
            {
                Name = "Task3 not to delete",
                Details = "Not delete details3"
            };

            var query = autoStore.Where(x => x.Details.StartsWith("Delet"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);

            // Act            
            SetRootUrlToKinveyClient(unreachableUrl);
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);
            SetRootUrlToKinveyClient(kinveyUrl);

            var pendingWriteActions1 = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
            var pushResult = await autoStore.PushAsync();
            var pendingWriteActions2 = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();

            var localEntities = await syncStore.FindAsync(query);
            var networkEntities = await networkStore.FindAsync();

            // Teardown
            await networkStore.RemoveAsync(savedItem3.ID);

            // Assert            
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.IsNotNull(pendingWriteActions1);
            Assert.IsNotNull(pushResult);
            Assert.IsNotNull(pendingWriteActions2);
            Assert.IsNotNull(localEntities);
            Assert.IsNotNull(networkEntities);
            Assert.AreEqual(2, kinveyDeleteResponse.count);
            Assert.AreEqual(2, pendingWriteActions1.Count);
            Assert.AreEqual(2, pushResult.PushCount);
            Assert.AreEqual(0, pendingWriteActions2.Count);            
            Assert.AreEqual(0, localEntities.Count);
            Assert.AreEqual("DELETE", pendingWriteActions1[0].action);
            Assert.AreEqual("DELETE", pendingWriteActions1[1].action);
            Assert.IsNotNull(pendingWriteActions1.FirstOrDefault(e => e.entityId == savedItem1.ID));
            Assert.IsNotNull(pendingWriteActions1.FirstOrDefault(e => e.entityId == savedItem2.ID));
            Assert.AreEqual(1, networkEntities.Count);
            Assert.AreEqual(savedItem3.ID, networkEntities[0].ID);
            Assert.AreEqual(savedItem3.Name, networkEntities[0].Name);
            Assert.AreEqual(savedItem3.Details, networkEntities[0].Details);
        }

        [TestMethod]
        public async Task TestDeleteByQueryInvalidPermissionsNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(1, kinveyClient);
            }

            // Arrange
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var query = autoStore.Where(x => x.Details.StartsWith("Delete d"));

            await User.LoginAsync(TestSetup.user_without_permissions, TestSetup.pass_for_user_without_permissions, kinveyClient);

            var newItem = new ToDo
            {
                Name = "Task1 to delete",
                Details = "Delete details1"
            };

            var savedItem = await syncStore.SaveAsync(newItem);

            // Act
            SetRootUrlToKinveyClient(unreachableUrl);
            await autoStore.RemoveAsync(query);
            SetRootUrlToKinveyClient(kinveyUrl);

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
            var existingToDos = await syncStore.FindAsync();

            // Assert
            Assert.IsNotNull(pendingWriteActions);
            Assert.AreEqual(0, pendingWriteActions.Count);
            Assert.IsNotNull(existingToDos);
            Assert.AreEqual(0, existingToDos.Count);
        }

        [TestMethod]
        public async Task TestDeleteByQueryDeleteItemsFromLocalStorageAfterNetworkDeletionConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(7);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "Task1 to delete",
                Details = "Delete details1"
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 to delete",
                Details = "Not delete"
            };

            var query = autoStore.Where(x => x.Details.StartsWith("Delete d"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);

            // Act
            var networkDeleteResponse = await networkStore.RemoveAsync(query);

            KinveyDeleteResponse autoDeleteResponse = null;
            KinveyException exception = null;
            try
            {
                autoDeleteResponse = await autoStore.RemoveAsync(query);
            }
            catch (KinveyException kinveyException)
            {
                exception = kinveyException;
            }

            KinveyException syncStoreException = null;
            ToDo existingItem1InLocalStorage = null;
            try
            {
                existingItem1InLocalStorage = await syncStore.FindByIDAsync(savedItem1.ID);
            }
            catch (KinveyException kinveyException)
            {
                syncStoreException = kinveyException;
            }
           
            var networkEntities = await networkStore.FindAsync();
            var localEntities = await syncStore.FindAsync();

            // Teardown
            await networkStore.RemoveAsync(savedItem2.ID);

            // Assert                                   
            Assert.IsNotNull(networkDeleteResponse);
            Assert.IsNotNull(autoDeleteResponse);
            Assert.IsNotNull(networkEntities);
            Assert.IsNotNull(localEntities);            
            Assert.IsNull(existingItem1InLocalStorage);
            Assert.IsNotNull(syncStoreException);
            Assert.AreEqual(EnumErrorCategory.ERROR_DATASTORE_CACHE, syncStoreException.ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_DATASTORE_CACHE_FIND_BY_ID_NOT_FOUND, syncStoreException.ErrorCode);
            Assert.AreEqual(1, networkDeleteResponse.count);
            Assert.AreEqual(0, autoDeleteResponse.count);
            Assert.AreEqual(1, networkEntities.Count);
            Assert.AreEqual(1, localEntities.Count);
        }

        [TestMethod]
        public async Task TestDeleteByQueryStringValueStartsWithExpressionAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(7);
            }
           
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Task1 to delete",
                Details = "Delete details1"
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 to delete",
                Details = "Delete details2"
            };
            var newItem3 = new ToDo
            {
                Name = "Task3 not to delete",
                Details = "Not delete details3"
            };

            var query = autoStore.Where(x => x.Details.StartsWith("Delet"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);

            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            var listToDoNetwork = await autoStore.FindAsync();
            var listToDoCache = await syncStore.FindAsync();

            var existingItemInNetwork1 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInNetwork2 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInNetwork3 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem3.ID);
            var existingItemInCache1 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInCache2 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInCache3 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem3.ID);

            // Teardown
            await autoStore.RemoveAsync(savedItem3.ID);

            // Assert            
            Assert.IsNull(existingItemInCache1);
            Assert.IsNull(existingItemInCache2);
            Assert.IsNotNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(2, kinveyDeleteResponse.count);
            Assert.IsNull(existingItemInCache1);
            Assert.IsNull(existingItemInCache2);
            Assert.IsNotNull(existingItemInCache3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryOrExpressionAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(9);
            }
            
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "Details1"
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 to delete",
                Details = "Details2",
            };
            var newItem3 = new ToDo
            {
                Name = "Task3 to delete",
                Details = "Details3",
                BoolVal = true
            };

            var query = autoStore.Where(e => e.Name == "Task2 to delete" || e.BoolVal == true);

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);

            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await autoStore.FindByIDAsync(savedItem1.ID);
                existingItemInCache1 = await syncStore.FindByIDAsync(savedItem1.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await autoStore.FindByIDAsync(savedItem2.ID);
                existingItemInCache2 = await syncStore.FindByIDAsync(savedItem2.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await autoStore.FindByIDAsync(savedItem3.ID);
                existingItemInCache3 = await syncStore.FindByIDAsync(savedItem3.ID);
            }
            catch (Exception)
            {

            }

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);

            // Assert            
            Assert.IsNotNull(existingItemInCache1);
            Assert.IsNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(2, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryBoolValueExplicitEqualsExpressionAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(9);
            }
            
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "Details1"
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 to delete",
                Details = "Details2",
                BoolVal = true
            };
            var newItem3 = new ToDo
            {
                Name = "Task3 to delete",
                Details = "Details3",
                BoolVal = true
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);

            var query = autoStore.Where(x => x.BoolVal.Equals(true));

            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await autoStore.FindByIDAsync(savedItem1.ID);
                existingItemInCache1 = await syncStore.FindByIDAsync(savedItem1.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await autoStore.FindByIDAsync(savedItem2.ID);
                existingItemInCache2 = await syncStore.FindByIDAsync(savedItem2.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await autoStore.FindByIDAsync(savedItem3.ID);
                existingItemInCache3 = await syncStore.FindByIDAsync(savedItem3.ID);
            }
            catch (Exception)
            {

            }

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);

            // Assert            
            Assert.IsNotNull(existingItemInCache1);
            Assert.IsNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(2, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryBoolValueImplicitEqualExpressionAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(9);
            }
            
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "Details1"
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 to delete",
                Details = "Details2",
                BoolVal = true
            };
            var newItem3 = new ToDo
            {
                Name = "Task3 to delete",
                Details = "Details3",
                BoolVal = true
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);

            var query = autoStore.Where(x => x.BoolVal);

            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await autoStore.FindByIDAsync(savedItem1.ID);
                existingItemInCache1 = await syncStore.FindByIDAsync(savedItem1.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await autoStore.FindByIDAsync(savedItem2.ID);
                existingItemInCache2 = await syncStore.FindByIDAsync(savedItem2.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await autoStore.FindByIDAsync(savedItem3.ID);
                existingItemInCache3 = await syncStore.FindByIDAsync(savedItem3.ID);
            }
            catch (Exception)
            {

            }

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);

            // Assert            
            Assert.IsNotNull(existingItemInCache1);
            Assert.IsNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(2, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryDateTimeValueGreaterThanExpressionAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(9);
            }
            
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "Details1",
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2017, 4, 20, 19, 56, 0)
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 to delete",
                Details = "Details2",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 4, 21, 19, 56, 0)
            };
            var newItem3 = new ToDo
            {
                Name = "Task3 to delete",
                Details = "Details3",
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 4, 22, 19, 56, 0)
            };

            var endDate = new DateTime(2018, 1, 1, 0, 0, 0);
            var query = autoStore.Where(x => x.NewDate > endDate);

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);
        
            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await autoStore.FindByIDAsync(savedItem1.ID);
                existingItemInCache1 = await syncStore.FindByIDAsync(savedItem1.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await autoStore.FindByIDAsync(savedItem2.ID);
                existingItemInCache2 = await syncStore.FindByIDAsync(savedItem2.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await autoStore.FindByIDAsync(savedItem3.ID);
                existingItemInCache3 = await syncStore.FindByIDAsync(savedItem3.ID);
            }
            catch (Exception)
            {

            }

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);

            // Assert            
            Assert.IsNotNull(existingItemInCache1);
            Assert.IsNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(2, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryDateTimeValueGreaterThanOrEqualExpressionAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(9);
            }
            
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "Details1",
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2017, 4, 20, 19, 56, 0)
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 to delete",
                Details = "Details2",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 1, 1, 0, 0, 0)
            };
            var newItem3 = new ToDo
            {
                Name = "Task3 to delete",
                Details = "Details3",
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 4, 22, 19, 56, 0)
            };

            var endDate = new DateTime(2018, 1, 1, 0, 0, 0);
            var query = autoStore.Where(x => x.NewDate >= endDate);

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);

            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await autoStore.FindByIDAsync(savedItem1.ID);
                existingItemInCache1 = await syncStore.FindByIDAsync(savedItem1.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await autoStore.FindByIDAsync(savedItem2.ID);
                existingItemInCache2 = await syncStore.FindByIDAsync(savedItem2.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await autoStore.FindByIDAsync(savedItem3.ID);
                existingItemInCache3 = await syncStore.FindByIDAsync(savedItem3.ID);
            }
            catch (Exception)
            {

            }

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);

            // Assert            
            Assert.IsNotNull(existingItemInCache1);
            Assert.IsNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(2, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryDateTimeValueLessThanExpressionAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(9);
            }
            
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "Details1",
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0)
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 to delete",
                Details = "Details2",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 1, 1, 0, 0, 0)
            };
            var newItem3 = new ToDo
            {
                Name = "Task3 to delete",
                Details = "Details3",
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 4, 22, 19, 56, 0)
            };

            var endDate = new DateTime(2018, 12, 10, 0, 0, 0);
            var query = autoStore.Where(x => x.NewDate < endDate);

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);

            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await autoStore.FindByIDAsync(savedItem1.ID);
                existingItemInCache1 = await syncStore.FindByIDAsync(savedItem1.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await autoStore.FindByIDAsync(savedItem2.ID);
                existingItemInCache2 = await syncStore.FindByIDAsync(savedItem2.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await autoStore.FindByIDAsync(savedItem3.ID);
                existingItemInCache3 = await syncStore.FindByIDAsync(savedItem3.ID);
            }
            catch (Exception)
            {

            }

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);

            // Assert            
            Assert.IsNotNull(existingItemInCache1);
            Assert.IsNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(2, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryDateTimeValueLessThanOrEqualExpressionAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(9);
            }
            
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "Details1",
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0)
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 to delete",
                Details = "Details2",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0)
            };
            var newItem3 = new ToDo
            {
                Name = "Task3 to delete",
                Details = "Details3",
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 4, 22, 19, 56, 0)
            };

            var endDate = new DateTime(2018, 12, 10, 0, 0, 0);
            var query = autoStore.Where(x => x.NewDate <= endDate);

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);
         
            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await autoStore.FindByIDAsync(savedItem1.ID);
                existingItemInCache1 = await syncStore.FindByIDAsync(savedItem1.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await autoStore.FindByIDAsync(savedItem2.ID);
                existingItemInCache2 = await syncStore.FindByIDAsync(savedItem2.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await autoStore.FindByIDAsync(savedItem3.ID);
                existingItemInCache3 = await syncStore.FindByIDAsync(savedItem3.ID);
            }
            catch (Exception)
            {

            }

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);

            // Assert            
            Assert.IsNotNull(existingItemInCache1);
            Assert.IsNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(2, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryIntValueGreaterThanExpressionAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(9);
            }
            
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "Details1",
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0),
                Value = 1
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 to delete",
                Details = "Details2",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0),
                Value = 2
            };
            var newItem3 = new ToDo
            {
                Name = "Task3 to delete",
                Details = "Details3",
                Value = 3
            };

            var query = autoStore.Where(x => x.Value > 1);

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);
            
            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await autoStore.FindByIDAsync(savedItem1.ID);
                existingItemInCache1 = await syncStore.FindByIDAsync(savedItem1.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await autoStore.FindByIDAsync(savedItem2.ID);
                existingItemInCache2 = await syncStore.FindByIDAsync(savedItem2.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await autoStore.FindByIDAsync(savedItem3.ID);
                existingItemInCache3 = await syncStore.FindByIDAsync(savedItem3.ID);
            }
            catch (Exception)
            {

            }

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);

            // Assert            
            Assert.IsNotNull(existingItemInCache1);
            Assert.IsNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(2, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryIntValueGreaterThanOrEqualExpressionAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(9);
            }
 
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "Details1",
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0),
                Value = 1
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 to delete",
                Details = "Details2",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0),
                Value = 2
            };
            var newItem3 = new ToDo
            {
                Name = "Task3 to delete",
                Details = "Details3",
                Value = 3
            };

            var endDate = new DateTime(2018, 12, 10, 0, 0, 0);
            var query = autoStore.Where(x => x.Value >= 2);

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);
         
            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await autoStore.FindByIDAsync(savedItem1.ID);
                existingItemInCache1 = await syncStore.FindByIDAsync(savedItem1.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await autoStore.FindByIDAsync(savedItem2.ID);
                existingItemInCache2 = await syncStore.FindByIDAsync(savedItem2.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await autoStore.FindByIDAsync(savedItem3.ID);
                existingItemInCache3 = await syncStore.FindByIDAsync(savedItem3.ID);
            }
            catch (Exception)
            {

            }

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);

            // Assert            
            Assert.IsNotNull(existingItemInCache1);
            Assert.IsNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(2, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryIntValueLessThanExpressionAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(9);
            }
          
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "Details1",
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0),
                Value = 2
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 to delete",
                Details = "Details2",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0),
                Value = 1
            };
            var newItem3 = new ToDo
            {
                Name = "Task3 to delete",
                Details = "Details3",
                Value = 1
            };

            var endDate = new DateTime(2018, 12, 10, 0, 0, 0);
            var query = autoStore.Where(x => x.Value < 2);

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);
          
            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await autoStore.FindByIDAsync(savedItem1.ID);
                existingItemInCache1 = await syncStore.FindByIDAsync(savedItem1.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await autoStore.FindByIDAsync(savedItem2.ID);
                existingItemInCache2 = await syncStore.FindByIDAsync(savedItem2.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await autoStore.FindByIDAsync(savedItem3.ID);
                existingItemInCache3 = await syncStore.FindByIDAsync(savedItem3.ID);
            }
            catch (Exception)
            {

            }

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);

            // Assert            
            Assert.IsNotNull(existingItemInCache1);
            Assert.IsNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(2, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryIntValueLessThanOrEqualExpressionAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(9);
            }
            
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "Details1",
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0),
                Value = 3
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 to delete",
                Details = "Details2",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0),
                Value = 2
            };
            var newItem3 = new ToDo
            {
                Name = "Task3 to delete",
                Details = "Details3",
                Value = 1
            };

            var query = autoStore.Where(x => x.Value <= 2);

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);            

            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await autoStore.FindByIDAsync(savedItem1.ID);
                existingItemInCache1 = await syncStore.FindByIDAsync(savedItem1.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await autoStore.FindByIDAsync(savedItem2.ID);
                existingItemInCache2 = await syncStore.FindByIDAsync(savedItem2.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await autoStore.FindByIDAsync(savedItem3.ID);
                existingItemInCache3 = await syncStore.FindByIDAsync(savedItem3.ID);
            }
            catch (Exception)
            {

            }

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);

            // Assert            
            Assert.IsNotNull(existingItemInCache1);
            Assert.IsNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(2, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryIntValueEqualsExpressionAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(9);
            }
           
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "Details1",
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0),
                Value = 3
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 to delete",
                Details = "Details2",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0),
                Value = 1
            };
            var newItem3 = new ToDo
            {
                Name = "Task3 to delete",
                Details = "Details3",
                Value = 1
            };

            var query = autoStore.Where(x => x.Value.Equals(1));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);          

            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await autoStore.FindByIDAsync(savedItem1.ID);
                existingItemInCache1 = await syncStore.FindByIDAsync(savedItem1.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await autoStore.FindByIDAsync(savedItem2.ID);
                existingItemInCache2 = await syncStore.FindByIDAsync(savedItem2.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await autoStore.FindByIDAsync(savedItem3.ID);
                existingItemInCache3 = await syncStore.FindByIDAsync(savedItem3.ID);
            }
            catch (Exception)
            {

            }

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);

            // Assert            
            Assert.IsNotNull(existingItemInCache1);
            Assert.IsNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(2, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryLogicalAndExpressionAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(9);
            }
            
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "TestDetails1",
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0),
                Value = 3
            };
            var newItem2 = new ToDo
            {
                Name = "Task to delete",
                Details = "Details2",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0),
                Value = 1
            };
            var newItem3 = new ToDo
            {
                Name = "Task to delete",
                Details = "Details3",
                Value = 1
            };

            var query = autoStore.Where(todo => todo.Details.StartsWith("Deta") && todo.Name.Equals("Task to delete"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);
            
            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await autoStore.FindByIDAsync(savedItem1.ID);
                existingItemInCache1 = await syncStore.FindByIDAsync(savedItem1.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await autoStore.FindByIDAsync(savedItem2.ID);
                existingItemInCache2 = await syncStore.FindByIDAsync(savedItem2.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await autoStore.FindByIDAsync(savedItem3.ID);
                existingItemInCache3 = await syncStore.FindByIDAsync(savedItem3.ID);
            }
            catch (Exception)
            {

            }

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);

            // Assert            
            Assert.IsNotNull(existingItemInCache1);
            Assert.IsNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(2, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryLogicalAndWithOrExpressionAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(9);
            }
            
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "TestDetails1",
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0),
                Value = 3
            };
            var newItem2 = new ToDo
            {
                Name = "TaskDel2",
                Details = "Details2",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0),
                Value = 1
            };
            var newItem3 = new ToDo
            {
                Name = "Task3",
                Details = "Details for",
                DueDate = "2018-04-22T19:56:00.963Z",
                Value = 1
            };

            var query = autoStore.Where(todo => todo.DueDate.Equals("2018-04-22T19:56:00.963Z") && (todo.Name.StartsWith("TaskDel") ||
                        todo.Details.Equals("Details for")));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);
            
            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await autoStore.FindByIDAsync(savedItem1.ID);
                existingItemInCache1 = await syncStore.FindByIDAsync(savedItem1.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await autoStore.FindByIDAsync(savedItem2.ID);
                existingItemInCache2 = await syncStore.FindByIDAsync(savedItem2.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await autoStore.FindByIDAsync(savedItem3.ID);
                existingItemInCache3 = await syncStore.FindByIDAsync(savedItem3.ID);
            }
            catch (Exception)
            {

            }

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);

            // Assert            
            Assert.IsNotNull(existingItemInCache1);
            Assert.IsNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(2, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryLogicalOrExpressionAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(9);
            }
            
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "TestDetails1",
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0),
                Value = 3
            };
            var newItem2 = new ToDo
            {
                Name = "Task Del2",
                Details = "Details2",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0),
                Value = 1
            };
            var newItem3 = new ToDo
            {
                Name = "Task3",
                Details = "Details for",
                DueDate = "2018-04-22T19:56:00.963Z",
                Value = 1
            };

            var query = autoStore.Where(todo => todo.Name.StartsWith("Task Del") || todo.Details.Equals("Details for"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);
         
            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await autoStore.FindByIDAsync(savedItem1.ID);
                existingItemInCache1 = await syncStore.FindByIDAsync(savedItem1.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await autoStore.FindByIDAsync(savedItem2.ID);
                existingItemInCache2 = await syncStore.FindByIDAsync(savedItem2.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await autoStore.FindByIDAsync(savedItem3.ID);
                existingItemInCache3 = await syncStore.FindByIDAsync(savedItem3.ID);
            }
            catch (Exception)
            {

            }

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);

            // Assert            
            Assert.IsNotNull(existingItemInCache1);
            Assert.IsNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(2, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryLogicalOrWithAndExpressionAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(9);
            }
            
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "TestDetails1",
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0),
                Value = 3
            };
            var newItem2 = new ToDo
            {
                Name = "Task Del2",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0),
                Value = 1
            };
            var newItem3 = new ToDo
            {
                Name = "Task3",
                Details = "Details for",
                DueDate = "2018-04-22T19:56:00.963Z",
                Value = 1
            };

            var query = autoStore.Where(todo => todo.Name.StartsWith("Task Del") || todo.DueDate.Equals("2018-04-22T19:56:00.963Z") && todo.Details.Equals("Details for"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);
           
            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await autoStore.FindByIDAsync(savedItem1.ID);
                existingItemInCache1 = await syncStore.FindByIDAsync(savedItem1.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await autoStore.FindByIDAsync(savedItem2.ID);
                existingItemInCache2 = await syncStore.FindByIDAsync(savedItem2.ID);
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await autoStore.FindByIDAsync(savedItem3.ID);
                existingItemInCache3 = await syncStore.FindByIDAsync(savedItem3.ID);
            }
            catch (Exception)
            {

            }

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);

            // Assert            
            Assert.IsNotNull(existingItemInCache1);
            Assert.IsNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(2, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryMultipleWhereClausesStartsWithAndEqualsExpressionsAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8);
            }
            
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Task1",
                Details = "Details for",
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0),
                Value = 3
            };
            var newItem2 = new ToDo
            {
                Name = "Task Del2",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0),
                Value = 1
            };
            var newItem3 = new ToDo
            {
                Name = "Task Del3",
                Details = "Details for",
                DueDate = "2018-04-22T19:56:00.963Z",
                Value = 1
            };

            var query = autoStore.Where(x => x.Details.StartsWith("Details f")).Where(y => y.Name.StartsWith("Task D")).Where(z => z.DueDate.Equals("2018-04-22T19:56:00.963Z"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);
       
            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            var listToDoNetwork = await networkStore.FindAsync();
            var listToDoCache = await syncStore.FindAsync();

            var existingItemInNetwork1 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInNetwork2 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInNetwork3 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem3.ID);
            var existingItemInCache1 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInCache2 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInCache3 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem3.ID);

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);
            await autoStore.RemoveAsync(savedItem2.ID);

            // Assert            
            Assert.IsNull(existingItemInCache1);
            Assert.IsNotNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(1, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNotNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryMultipleWhereClausesEqualsExpressionsAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8);
            }
           
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Task1",
                Details = "Details for",
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0),
                Value = 3
            };
            var newItem2 = new ToDo
            {
                Name = "Task Test",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0),
                Value = 1
            };
            var newItem3 = new ToDo
            {
                Name = "Task Test",
                Details = "Details for",
                DueDate = "2018-04-22T19:56:00.963Z",
                Value = 1
            };

            var query = autoStore.Where(x => x.Details.Equals("Details for")).Where(y => y.Name.Equals("Task Test")).Where(z => z.DueDate.Equals("2018-04-22T19:56:00.963Z"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);
           
            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            var listToDoNetwork = await networkStore.FindAsync();
            var listToDoCache = await syncStore.FindAsync();

            var existingItemInNetwork1 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInNetwork2 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInNetwork3 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem3.ID);
            var existingItemInCache1 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInCache2 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInCache3 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem3.ID);

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);
            await autoStore.RemoveAsync(savedItem2.ID);

            // Assert            
            Assert.IsNull(existingItemInCache1);
            Assert.IsNotNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(1, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNotNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryMultipleWhereClausesDifferentEqualsExpressionsAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8);
            }
            
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Task1",
                Details = "Details for",
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0),
                Value = 3
            };
            var newItem2 = new ToDo
            {
                Name = "Task Del",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0),

            };
            var newItem3 = new ToDo
            {
                Name = "Task Del",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                Value = 1
            };

            var query = autoStore.Where(x => x.Details == ("Details for")).Where(y => y.BoolVal == true).Where(z => z.DueDate.Equals("2018-04-22T19:56:00.963Z"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);
            
            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            var listToDoNetwork = await networkStore.FindAsync();
            var listToDoCache = await syncStore.FindAsync();

            var existingItemInNetwork1 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInNetwork2 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInNetwork3 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem3.ID);
            var existingItemInCache1 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInCache2 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInCache3 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem3.ID);

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);
            await autoStore.RemoveAsync(savedItem2.ID);

            // Assert            
            Assert.IsNull(existingItemInCache1);
            Assert.IsNotNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(1, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNotNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryMultipleWhereClausesFluentSyntaxEqualExpressionsAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8);
            }
            
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Task1",
                Details = "Details for",
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0),
                Value = 3
            };
            var newItem2 = new ToDo
            {
                Name = "Task Del",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0),

            };
            var newItem3 = new ToDo
            {
                Name = "Task Del",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                Value = 1
            };

            var query = from t in autoStore where t.Details == "Details for" where t.Name == "Task Del" where t.DueDate == "2018-04-22T19:56:00.963Z" select t;

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);
           
            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            var listToDoNetwork = await networkStore.FindAsync();
            var listToDoCache = await syncStore.FindAsync();

            var existingItemInNetwork1 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInNetwork2 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInNetwork3 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem3.ID);
            var existingItemInCache1 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInCache2 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInCache3 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem3.ID);

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);
            await autoStore.RemoveAsync(savedItem2.ID);

            // Assert            
            Assert.IsNull(existingItemInCache1);
            Assert.IsNotNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(1, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNotNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }


        [TestMethod]
        public async Task TestDeleteByQueryMultipleWhereClausesWithLogicalAndExpressionsAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8);
            }
            
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Task Delete",
                Details = "Details for",
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0),
                Value = 3
            };
            var newItem2 = new ToDo
            {
                Name = "Task Not Delete",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0),

            };
            var newItem3 = new ToDo
            {
                Name = "Task Delete",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                Value = 1
            };

            var query = autoStore.Where(x => x.Details.StartsWith("Details f") && x.DueDate.Equals("2018-04-22T19:56:00.963Z")).Where(y => y.Name.StartsWith("Task Del"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);
           
            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            var listToDoNetwork = await networkStore.FindAsync();
            var listToDoCache = await syncStore.FindAsync();

            var existingItemInNetwork1 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInNetwork2 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInNetwork3 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem3.ID);
            var existingItemInCache1 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInCache2 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInCache3 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem3.ID);

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);
            await autoStore.RemoveAsync(savedItem2.ID);

            // Assert            
            Assert.IsNull(existingItemInCache1);
            Assert.IsNotNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(1, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNotNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryMultipleWhereClausesWithLogicalOrExpressionsAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8);
            }
            
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Task Del1",
                Details = "Details1",
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0),
                Value = 3
            };
            var newItem2 = new ToDo
            {
                Name = "Task Not Del",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2018-04-21T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0),

            };
            var newItem3 = new ToDo
            {
                Name = "Task Del2",
                Details = "Details",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                Value = 1
            };

            var query = autoStore.Where(x => x.Details.StartsWith("Details f") || x.DueDate.Equals("2018-04-22T19:56:00.963Z")).Where(y => y.Name.StartsWith("Task D"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);
          
            // Act
            var kinveyDeleteResponse = await autoStore.RemoveAsync(query);

            var listToDoNetwork = await networkStore.FindAsync();
            var listToDoCache = await syncStore.FindAsync();

            var existingItemInNetwork1 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInNetwork2 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInNetwork3 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem3.ID);
            var existingItemInCache1 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInCache2 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInCache3 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem3.ID);

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);
            await autoStore.RemoveAsync(savedItem2.ID);

            // Assert            
            Assert.IsNull(existingItemInCache1);
            Assert.IsNotNull(existingItemInCache2);
            Assert.IsNull(existingItemInCache3);
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(1, kinveyDeleteResponse.count);
            Assert.IsNotNull(existingItemInNetwork1);
            Assert.IsNotNull(existingItemInNetwork2);
            Assert.IsNull(existingItemInNetwork3);
        }

        [TestMethod]
        public async Task TestDeleteByQueryWhereClauseIsAbsentInQueryUsingSelectClauseAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(7);
            }
           
            // Arrange
            var todoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "Details1",
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0),
                Value = 3
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 not to delete",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0),

            };
            var newItem3 = new ToDo
            {
                Name = "Task3 not to delete",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                Value = 1
            };

            var query = todoStore.Select(x => x.Details);

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);
            
            KinveyDeleteResponse kinveyDeleteResponse;

            // Act
            var e = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                kinveyDeleteResponse = await todoStore.RemoveAsync(query);
            });

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);
            await todoStore.RemoveAsync(savedItem2.ID);
            await todoStore.RemoveAsync(savedItem3.ID);

            // Assert
            Assert.IsTrue(e.GetType() == typeof(KinveyException));
            KinveyException ke = e as KinveyException;
            Assert.IsTrue(ke.ErrorCategory == EnumErrorCategory.ERROR_GENERAL);
            Assert.IsTrue(ke.ErrorCode == EnumErrorCode.ERROR_DATASTORE_WHERE_CLAUSE_IS_ABSENT_IN_QUERY);
        }

        [TestMethod]
        public async Task TestDeleteByQueryWhereClauseIsAbsentInQueryUsingOrderClauseAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(7);
            }
            
            // Arrange
            var todoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "Details1",
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0),
                Value = 3
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 not to delete",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0),

            };
            var newItem3 = new ToDo
            {
                Name = "Task3 not to delete",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                Value = 1
            };

            var query = todoStore.OrderBy(x => x.Details);

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);
            
            KinveyDeleteResponse kinveyDeleteResponse;

            // Act
            var e = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                kinveyDeleteResponse = await todoStore.RemoveAsync(query);
            });

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);
            await todoStore.RemoveAsync(savedItem2.ID);
            await todoStore.RemoveAsync(savedItem3.ID);

            // Assert
            Assert.IsTrue(e.GetType() == typeof(KinveyException));
            KinveyException ke = e as KinveyException;
            Assert.IsTrue(ke.ErrorCategory == EnumErrorCategory.ERROR_GENERAL);
            Assert.IsTrue(ke.ErrorCode == EnumErrorCode.ERROR_DATASTORE_WHERE_CLAUSE_IS_ABSENT_IN_QUERY);
        }

        [TestMethod]
        public async Task TestDeleteByQueryWhereClauseIsAbsentInQueryUsingTakeClauseAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(7);
            }
            
            // Arrange
            var todoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "Details1",
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0),
                Value = 3
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 not to delete",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0),

            };
            var newItem3 = new ToDo
            {
                Name = "Task3 not to delete",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                Value = 1
            };

            var query = todoStore.Take(1);

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);
         
            KinveyDeleteResponse kinveyDeleteResponse;

            // Act
            Exception e = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                kinveyDeleteResponse = await todoStore.RemoveAsync(query);
            });

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);
            await todoStore.RemoveAsync(savedItem2.ID);
            await todoStore.RemoveAsync(savedItem3.ID);

            // Assert
            Assert.IsTrue(e.GetType() == typeof(KinveyException));
            KinveyException ke = e as KinveyException;
            Assert.IsTrue(ke.ErrorCategory == EnumErrorCategory.ERROR_GENERAL);
            Assert.IsTrue(ke.ErrorCode == EnumErrorCode.ERROR_DATASTORE_WHERE_CLAUSE_IS_ABSENT_IN_QUERY);
        }

        [TestMethod]
        public async Task TestDeleteByQueryNullQueryAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(7);
            }
            
            // Arrange
            var todoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "Details1",
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0),
                Value = 3
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 not to delete",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0),

            };
            var newItem3 = new ToDo
            {
                Name = "Task3 not to delete",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                Value = 1
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            KinveyDeleteResponse kinveyDeleteResponse;

            // Act
            var e = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                kinveyDeleteResponse = await todoStore.RemoveAsync(query: null);
            });

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);
            await todoStore.RemoveAsync(savedItem2.ID);
            await todoStore.RemoveAsync(savedItem3.ID);

            // Assert
            Assert.IsTrue(e.GetType() == typeof(KinveyException));
            KinveyException ke = e as KinveyException;
            Assert.IsTrue(ke.ErrorCategory == EnumErrorCategory.ERROR_GENERAL);
            Assert.IsTrue(ke.ErrorCode == EnumErrorCode.ERROR_DATASTORE_NULL_QUERY);
        }

        [TestMethod]
        public async Task TestDeleteByQueryNotSupportedExpressionAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(7);
            }
            
            // Arrange
            var todoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "Details1",
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0),
                Value = 3
            };
            var newItem2 = new ToDo
            {
                Name = "Task Del11",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0),

            };
            var newItem3 = new ToDo
            {
                Name = "Task Del22",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                Value = 1
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var query = todoStore.Where(x => true);

            KinveyDeleteResponse kinveyDeleteResponse;

            // Act
            var e = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                kinveyDeleteResponse = await todoStore.RemoveAsync(query);
            });

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);
            await todoStore.RemoveAsync(savedItem2.ID);
            await todoStore.RemoveAsync(savedItem3.ID);

            // Assert
            Assert.IsTrue(e.GetType() == typeof(KinveyException));
            KinveyException ke = e as KinveyException;
            Assert.IsTrue(ke.ErrorCategory == EnumErrorCategory.ERROR_DATASTORE_NETWORK);
            Assert.IsTrue(ke.ErrorCode == EnumErrorCode.ERROR_LINQ_WHERE_CLAUSE_NOT_SUPPORTED);
        }

        [TestMethod]
        public async Task TestDeleteByQueryNotSupportedStringExpressionAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(7);
            }
            
            // Arrange
            var todoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "Task1 not to delete",
                Details = "Details1",
                DueDate = "2017-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 18, 19, 56, 0),
                BoolVal = false,
                Value = 3
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 to delete support",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                NewDate = new DateTime(2018, 12, 10, 0, 0, 0),

            };
            var newItem3 = new ToDo
            {
                Name = "Task3 to delete support",
                Details = "Details for",
                BoolVal = true,
                DueDate = "2018-04-22T19:56:00.963Z",
                Value = 1
            };

            var query = todoStore.Where(x => x.Name.Contains("support"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);
           
            KinveyDeleteResponse kinveyDeleteResponse;

            // Act
            var e = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                kinveyDeleteResponse = await todoStore.RemoveAsync(query);
            });

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);
            await todoStore.RemoveAsync(savedItem2.ID);
            await todoStore.RemoveAsync(savedItem3.ID);

            // Assert
            Assert.IsTrue(e.GetType() == typeof(KinveyException));
            KinveyException ke = e as KinveyException;
            Assert.IsTrue(ke.ErrorCategory == EnumErrorCategory.ERROR_DATASTORE_NETWORK);
            Assert.IsTrue(ke.ErrorCode == EnumErrorCode.ERROR_LINQ_WHERE_CLAUSE_NOT_SUPPORTED);
        }

        #endregion Remove by query

        #region Remove by id

        [TestMethod]
        public async Task TestDeleteByIdConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(6);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "Task1 to delete",
                Details = "Delete details1"
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 to delete",
                Details = "Delete details2"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);

            // Act            
            var kinveyDeleteResponse = await autoStore.RemoveAsync(savedItem1.ID);
           
            KinveyException networkStoreException = null;
            ToDo existingItem1InNetwork = null;
            try
            {
                existingItem1InNetwork = await networkStore.FindByIDAsync(savedItem1.ID);
            }
            catch (KinveyException kinveyException)
            {
                networkStoreException = kinveyException;
            }

            KinveyException syncStoreException = null;
            ToDo existingItem1InLocal = null;
            try
            {
                existingItem1InLocal = await syncStore.FindByIDAsync(savedItem1.ID);
            }
            catch (KinveyException kinveyException)
            {
                syncStoreException = kinveyException;
            }

            // Teardown
            await networkStore.RemoveAsync(savedItem2.ID);

            // Assert   
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.AreEqual(1, kinveyDeleteResponse.count);

            Assert.IsNull(existingItem1InNetwork);
            if (!MockData)
            {            
                Assert.IsNotNull(networkStoreException);
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, networkStoreException.ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, networkStoreException.ErrorCode);
            }

            Assert.IsNull(existingItem1InLocal);
            Assert.IsNotNull(syncStoreException);
            Assert.AreEqual(EnumErrorCategory.ERROR_DATASTORE_CACHE, syncStoreException.ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_DATASTORE_CACHE_FIND_BY_ID_NOT_FOUND, syncStoreException.ErrorCode);        
        }

        [TestMethod]
        public async Task TestDeleteByIdNotFoundConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(2, kinveyClient);
            }

            // Arrange
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            await User.LoginAsync(TestSetup.user_without_permissions, TestSetup.pass_for_user_without_permissions, kinveyClient);

            var newItem = new ToDo
            {
                Name = "Task1 to delete",
                Details = "Delete details1"
            };

            var savedItem = await syncStore.SaveAsync(newItem);

            // Act
            var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                await autoStore.RemoveAsync(savedItem.ID);
            });

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
            var existingToDos = await syncStore.FindAsync();

            // Assert
            Assert.AreEqual(typeof(KinveyException), exception.GetType());
            var ke = exception as KinveyException;
            Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, ke.ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, ke.ErrorCode);

            Assert.IsNotNull(pendingWriteActions);
            Assert.AreEqual(0, pendingWriteActions.Count);
            Assert.IsNotNull(existingToDos);
            Assert.AreEqual(0, existingToDos.Count);
        }

        [TestMethod]
        public async Task TestDeleteByIdNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(4);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "Task1 to delete",
                Details = "Delete details1"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);

            // Act            
            SetRootUrlToKinveyClient(unreachableUrl);
            var kinveyDeleteResponse = await autoStore.RemoveAsync(savedItem1.ID);
            SetRootUrlToKinveyClient(kinveyUrl);

            var pendingWriteActions1 = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
            var pushResult = await autoStore.PushAsync();
            var pendingWriteActions2 = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();

            var localEntity = await syncStore.FindByIDAsync(newItem1.ID);
            var networkEntities = await networkStore.FindAsync();

            // Assert            
            Assert.IsNotNull(kinveyDeleteResponse);
            Assert.IsNotNull(pendingWriteActions1);
            Assert.IsNotNull(pushResult);
            Assert.IsNotNull(pendingWriteActions2);
            Assert.IsNull(localEntity);
            Assert.IsNotNull(networkEntities);
            Assert.AreEqual(1, kinveyDeleteResponse.count);
            Assert.AreEqual(1, pendingWriteActions1.Count);
            Assert.AreEqual(1, pushResult.PushCount);
            Assert.AreEqual(0, pendingWriteActions2.Count);
            Assert.AreEqual("DELETE", pendingWriteActions1[0].action);
            Assert.IsNotNull(pendingWriteActions1.FirstOrDefault(e => e.entityId == savedItem1.ID));
            Assert.AreEqual(0, networkEntities.Count);
        }

        [TestMethod]
        public async Task TestDeleteByIdNotFoundConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(1, kinveyClient);
            }

            // Arrange
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            await User.LoginAsync(TestSetup.user_without_permissions, TestSetup.pass_for_user_without_permissions, kinveyClient);

            var newItem = new ToDo
            {
                Name = "Task1 to delete",
                Details = "Delete details1"
            };

            var savedItem = await syncStore.SaveAsync(newItem);

            // Act
            SetRootUrlToKinveyClient(unreachableUrl);
            await autoStore.RemoveAsync(savedItem.ID);
            SetRootUrlToKinveyClient(kinveyUrl);

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
            var existingToDos = await syncStore.FindAsync();

            // Assert
            Assert.IsNotNull(pendingWriteActions);
            Assert.AreEqual(0, pendingWriteActions.Count);
            Assert.IsNotNull(existingToDos);
            Assert.AreEqual(0, existingToDos.Count);
        }

        [TestMethod]
        public async Task TestDeleteByIdNotExistingIdConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(2);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act            
            KinveyException exception = null;
            KinveyDeleteResponse kinveyDeleteResponse = null;
            try
            {
                kinveyDeleteResponse = await autoStore.RemoveAsync(Guid.NewGuid().ToString());
            }
            catch (KinveyException kinveyException)
            {
                exception = kinveyException;
            }

            // Assert                      
            Assert.IsNull(kinveyDeleteResponse);
            Assert.IsNotNull(exception);
            Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, exception.ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, exception.ErrorCode);
            Assert.AreEqual(404, exception.StatusCode);
        }

        [TestMethod]
        public async Task TestDeleteByIdDeleteItemsFromLocalStorageAfterNetworkDeletionConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(6);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "Task1 to delete",
                Details = "Delete details1"
            };
            var newItem2 = new ToDo
            {
                Name = "Task2 to delete",
                Details = "Delete details2"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);

            // Act            
            var kinveyDeleteResponse1 = await networkStore.RemoveAsync(savedItem1.ID);

            KinveyException exception = null;
            KinveyDeleteResponse kinveyDeleteResponse2 = null;
            try
            {
                kinveyDeleteResponse2 = await autoStore.RemoveAsync(savedItem1.ID);
            }
            catch (KinveyException kinveyException)
            {
                exception = kinveyException;
            }

            KinveyException syncStoreException = null;
            ToDo existingItem1InLocal = null;
            try
            {
                existingItem1InLocal = await syncStore.FindByIDAsync(savedItem1.ID);
            }
            catch (KinveyException kinveyException)
            {
                syncStoreException = kinveyException;
            }

            // Teardown
            await networkStore.RemoveAsync(savedItem2.ID);

            // Assert            
            Assert.IsNull(existingItem1InLocal);
            Assert.IsNotNull(syncStoreException);
            Assert.AreEqual(EnumErrorCategory.ERROR_DATASTORE_CACHE, syncStoreException.ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_DATASTORE_CACHE_FIND_BY_ID_NOT_FOUND, syncStoreException.ErrorCode);

            Assert.IsNotNull(exception);
            Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, exception.ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, exception.ErrorCode);
            Assert.AreEqual(404, exception.StatusCode);
        }

        [TestMethod]
        public async Task TestRemoveByIdAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(3, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem = new ToDo
            {
                Name = "todo save",
                Details = "details for save",
                DueDate = "2016-04-22T19:56:00.961Z"
            };
           
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem = await autoStore.SaveAsync(newItem);

            // Act
            var kdr = await autoStore.RemoveAsync(savedItem.ID);

            // Assert
            Assert.IsNotNull(kdr);
            Assert.AreEqual(1, kdr.count);
        }

        #endregion Remove by id

        #region Find

        [TestMethod]
        public async Task TestFindNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(9, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem3 = new ToDo
            {
                Name = "todo3",
                Details = "details for 3 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await networkStore.SaveAsync(newItem1);
            newItem2 = await networkStore.SaveAsync(newItem2);
            
            // Act          
            var listToDoAuto1 = await autoStore.FindAsync();

            var listToDoSync1 = await syncStore.FindAsync();

            newItem3 = await networkStore.SaveAsync(newItem3);

            var listToDoAuto2 = await autoStore.FindAsync();
            var listToDoSync2 = await syncStore.FindAsync();

            // Teardown
            await networkStore.RemoveAsync(newItem1.ID);
            await networkStore.RemoveAsync(newItem2.ID);
            await networkStore.RemoveAsync(newItem3.ID);

            // Assert
            Assert.IsNotNull(listToDoAuto1);
            Assert.IsNotNull(listToDoSync1);
            Assert.IsNotNull(listToDoAuto2);
            Assert.IsNotNull(listToDoSync2);
            Assert.AreEqual(2, listToDoAuto1.Count);
            Assert.AreEqual(2, listToDoSync1.Count);
            Assert.AreEqual(3, listToDoAuto2.Count);
            Assert.AreEqual(3, listToDoSync2.Count);
        }

        [TestMethod]
        public async Task TestFindWithQueryNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem3 = new ToDo
            {
                Name = "todo3",
                Details = "details for 3 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var query = autoStore.Where(x => x.Details.StartsWith("details for t"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await networkStore.SaveAsync(newItem1);
            newItem2 = await networkStore.SaveAsync(newItem2);
            newItem3 = await networkStore.SaveAsync(newItem3);

            // Act          
            var listToDoAuto = await autoStore.FindAsync(query);
            var listToDoSync = await syncStore.FindAsync(query);

            // Teardown
            await networkStore.RemoveAsync(newItem1.ID);
            await networkStore.RemoveAsync(newItem2.ID);
            await networkStore.RemoveAsync(newItem3.ID);

            // Assert
            Assert.IsNotNull(listToDoAuto);
            Assert.IsNotNull(listToDoSync);
            Assert.AreEqual(2, listToDoAuto.Count);
            Assert.AreEqual(2, listToDoSync.Count);
        }

        [TestMethod]
        public async Task TestFindWithSkipLimitQueryNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem3 = new ToDo
            {
                Name = "todo3",
                Details = "details for 3 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var query = autoStore.Take(1).Skip(1);

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await networkStore.SaveAsync(newItem1);
            newItem2 = await networkStore.SaveAsync(newItem2);
            newItem3 = await networkStore.SaveAsync(newItem3);

            // Act          
            var listToDoAuto = await autoStore.FindAsync(query);
            var listToDoSync = await syncStore.FindAsync(query);

            // Teardown
            await networkStore.RemoveAsync(newItem1.ID);
            await networkStore.RemoveAsync(newItem2.ID);
            await networkStore.RemoveAsync(newItem3.ID);

            // Assert
            Assert.IsNotNull(listToDoAuto);
            Assert.IsNotNull(listToDoSync);
            Assert.AreEqual(1, listToDoAuto.Count);
            Assert.AreEqual(0, listToDoSync.Count);
        }

        [TestMethod]
        public async Task TestFindWithDeltaSetTurnOnNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(12, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);
            var syncStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.SYNC);

            var newItem1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };
            var newItem2 = new FlashCard
            {
                Question = "What is 3 + 5?",
                Answer = "8"
            };
            var newItem3 = new FlashCard
            {
                Question = "What is 4 + 5?",
                Answer = "9"
            };
            var newItem4 = new FlashCard
            {
                Question = "What is 5 + 5?",
                Answer = "10"
            };

            autoStore.DeltaSetFetchingEnabled = true;

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await networkStore.SaveAsync(newItem1);
            newItem2 = await networkStore.SaveAsync(newItem2);           

            // Act          
            var listToDoAuto1 = await autoStore.FindAsync();
            newItem3 = await networkStore.SaveAsync(newItem3);
            var listToDoAuto2 = await autoStore.FindAsync();

            var queryCacheItems = kinveyClient.CacheManager.GetCache<QueryCacheItem>("QueryCacheItem");
            var queryCacheItemBefore = queryCacheItems.FindAll().FirstOrDefault();

            newItem4 = await networkStore.SaveAsync(newItem4);

            var listToDoAuto3 = await autoStore.FindAsync();
            var queryCacheItemAfter = queryCacheItems.FindAll().FirstOrDefault();

            var listToDoSync = await syncStore.FindAsync();

            // Teardown
            await networkStore.RemoveAsync(newItem1.ID);
            await networkStore.RemoveAsync(newItem2.ID);
            await networkStore.RemoveAsync(newItem3.ID);
            await networkStore.RemoveAsync(newItem4.ID);

            // Assert
            Assert.IsNotNull(listToDoAuto1);
            Assert.IsNotNull(listToDoAuto2);
            Assert.IsNotNull(listToDoAuto3);
            Assert.IsNotNull(listToDoSync);
            Assert.AreEqual(2, listToDoAuto1.Count);
            Assert.AreEqual(3, listToDoAuto2.Count);
            Assert.AreEqual(4, listToDoAuto3.Count);
            Assert.AreEqual(4, listToDoSync.Count);
            Assert.AreNotEqual(queryCacheItemBefore.lastRequest, queryCacheItemAfter.lastRequest);
        }

        [TestMethod]
        public async Task TestFindDescendingOrderNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem3 = new ToDo
            {
                Name = "todo3",
                Details = "details for 3 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };         

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem2 = await networkStore.SaveAsync(newItem2);
            newItem3 = await networkStore.SaveAsync(newItem3);
            newItem1 = await networkStore.SaveAsync(newItem1);
                       
            var query = autoStore.OrderByDescending(task => task.Details);

            // Act          
            var listToDoAuto = await autoStore.FindAsync(query);

            // Teardown
            await networkStore.RemoveAsync(newItem1.ID);
            await networkStore.RemoveAsync(newItem2.ID);
            await networkStore.RemoveAsync(newItem3.ID);

            // Assert
            Assert.IsNotNull(listToDoAuto);
            Assert.AreEqual(newItem3.Details, listToDoAuto[0].Details);
            Assert.AreEqual(newItem2.Details, listToDoAuto[1].Details);
            Assert.AreEqual(newItem1.Details, listToDoAuto[2].Details);
        }

        [TestMethod]
        public async Task TestFindDeletingItemNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(5, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await autoStore.SaveAsync(newItem1);
            newItem2 = await autoStore.SaveAsync(newItem2);

            await autoStore.RemoveAsync(newItem1.ID);

            // Act          
            var listToDoSync = await syncStore.FindAsync();

            // Teardown
            await autoStore.RemoveAsync(newItem2.ID);

            // Assert
            Assert.IsNotNull(listToDoSync);
            Assert.AreEqual(newItem2.Name, listToDoSync[0].Name);
            Assert.AreEqual(newItem2.Details, listToDoSync[0].Details);
            Assert.AreEqual(newItem2.DueDate, listToDoSync[0].DueDate);
        }

        [TestMethod]
        public async Task TestFindInvalidQueryNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(5, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await networkStore.SaveAsync(newItem1);
            newItem2 = await networkStore.SaveAsync(newItem2);

            var query = autoStore.Where(x => true);

            // Act
            var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                await autoStore.FindAsync(query);
            });

            // Teardown
            await networkStore.RemoveAsync(newItem1.ID);
            await networkStore.RemoveAsync(newItem2.ID);

            // Assert
            Assert.IsTrue(exception.GetType() == typeof(KinveyException));
            KinveyException ke = exception as KinveyException;
            Assert.IsTrue(ke.ErrorCategory == EnumErrorCategory.ERROR_DATASTORE_NETWORK);
            Assert.IsTrue(ke.ErrorCode == EnumErrorCode.ERROR_LINQ_WHERE_CLAUSE_NOT_SUPPORTED);
        }

        [TestMethod]
        public async Task TestFindInvalidPermissionsNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(2, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            await User.LoginAsync(TestSetup.user_without_permissions, TestSetup.pass_for_user_without_permissions, kinveyClient);

            // Act
            var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                await autoStore.FindAsync();
            });

            // Assert
            Assert.IsTrue(exception.GetType() == typeof(KinveyException));
            KinveyException ke = exception as KinveyException;
            Assert.IsTrue(ke.ErrorCategory == EnumErrorCategory.ERROR_BACKEND);
            Assert.IsTrue(ke.ErrorCode == EnumErrorCode.ERROR_JSON_RESPONSE);
        }

        [TestMethod]
        public async Task TestFindNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem3 = new ToDo
            {
                Name = "todo3",
                Details = "details for 3 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);
            
            newItem1 = await networkStore.SaveAsync(newItem1);
            newItem2 = await networkStore.SaveAsync(newItem2);

            // Act          
            var listToDoAuto1 = await autoStore.FindAsync();

            newItem3 = await networkStore.SaveAsync(newItem3);

            SetRootUrlToKinveyClient(unreachableUrl);           
            var listToDoAuto2 = await autoStore.FindAsync();
            SetRootUrlToKinveyClient(kinveyUrl);

            // Teardown
            await networkStore.RemoveAsync(newItem1.ID);
            await networkStore.RemoveAsync(newItem2.ID);
            await networkStore.RemoveAsync(newItem3.ID);

            // Assert
            Assert.IsNotNull(listToDoAuto1);
            Assert.IsNotNull(listToDoAuto2);
            Assert.AreEqual(2, listToDoAuto1.Count);
            Assert.AreEqual(2, listToDoAuto2.Count);
        }

        [TestMethod]
        public async Task TestFindNetworkConnectionEliminatedAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(11, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem3 = new ToDo
            {
                Name = "todo3",
                Details = "details for 3 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem4 = new ToDo
            {
                Name = "todo4",
                Details = "details for 4 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await networkStore.SaveAsync(newItem1);
            newItem2 = await networkStore.SaveAsync(newItem2);

            // Act          
            var listToDoAuto1 = await autoStore.FindAsync();

            newItem3 = await networkStore.SaveAsync(newItem3);

            SetRootUrlToKinveyClient(unreachableUrl);
            var listToDoAuto2 = await autoStore.FindAsync();
            SetRootUrlToKinveyClient(kinveyUrl);

            newItem4 = await networkStore.SaveAsync(newItem4);

            var listToDoAuto3 = await autoStore.FindAsync();

            // Teardown
            await networkStore.RemoveAsync(newItem1.ID);
            await networkStore.RemoveAsync(newItem2.ID);
            await networkStore.RemoveAsync(newItem3.ID);
            await networkStore.RemoveAsync(newItem4.ID);

            // Assert
            Assert.IsNotNull(listToDoAuto1);
            Assert.IsNotNull(listToDoAuto2);
            Assert.IsNotNull(listToDoAuto3);
            Assert.AreEqual(2, listToDoAuto1.Count);
            Assert.AreEqual(2, listToDoAuto2.Count);
            Assert.AreEqual(4, listToDoAuto3.Count);
        }

        [TestMethod]
        public async Task TestFindWithQueryNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem3 = new ToDo
            {
                Name = "todo3",
                Details = "details for 3",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var query = autoStore.Where(x => x.Details.StartsWith("details for t"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);
            
            newItem1 = await networkStore.SaveAsync(newItem1);
            newItem2 = await networkStore.SaveAsync(newItem2);
            newItem3 = await networkStore.SaveAsync(newItem3);

            // Act          
            var listToDoAuto1 = await autoStore.FindAsync();

            SetRootUrlToKinveyClient(unreachableUrl);
            var listToDoAuto2 = await autoStore.FindAsync(query);
            SetRootUrlToKinveyClient(kinveyUrl);

            // Teardown
            await networkStore.RemoveAsync(newItem1.ID);
            await networkStore.RemoveAsync(newItem2.ID);
            await networkStore.RemoveAsync(newItem3.ID);

            // Assert
            Assert.IsNotNull(listToDoAuto1);
            Assert.IsNotNull(listToDoAuto2);
            Assert.AreEqual(3, listToDoAuto1.Count);
            Assert.AreEqual(2, listToDoAuto2.Count);
        }

        [TestMethod]
        public async Task TestFindSkip1Take1NetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "todo",
                Details = "details for 1",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem2 = new ToDo
            {
                Name = "another todo",
                Details = "details for 2",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem3 = new ToDo
            {
                Name = "yet another todo",
                Details = "details for 3",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var query = autoStore.Skip(1).Take(1);

            await User.LoginAsync(TestSetup.user, TestSetup.pass);
           
            newItem1 = await networkStore.SaveAsync(newItem1);
            newItem2 = await networkStore.SaveAsync(newItem2);
            newItem3 = await networkStore.SaveAsync(newItem3);

            // Act
            var listToDoAuto1 = await autoStore.FindAsync();

            SetRootUrlToKinveyClient(unreachableUrl);
            var listToDoAuto2 = await autoStore.FindAsync(query);
            SetRootUrlToKinveyClient(kinveyUrl);

            // Teardown
            await networkStore.RemoveAsync(newItem1.ID);
            await networkStore.RemoveAsync(newItem2.ID);
            await networkStore.RemoveAsync(newItem3.ID);

            // Assert
            Assert.IsNotNull(listToDoAuto1);
            Assert.IsNotNull(listToDoAuto2);
            Assert.AreEqual(listToDoAuto1.Count, 3);
            Assert.AreEqual(listToDoAuto2.Count, 1);
        }

        [TestMethod]
        public async Task TestFindDescendingOrderNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem3 = new ToDo
            {
                Name = "todo3",
                Details = "details for 3 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem2 = await networkStore.SaveAsync(newItem2);
            newItem3 = await networkStore.SaveAsync(newItem3);
            newItem1 = await networkStore.SaveAsync(newItem1);

            var query = autoStore.OrderByDescending(task => task.Details);

            // Act          
            var listToDoAuto1 = await autoStore.FindAsync(query);

            SetRootUrlToKinveyClient(unreachableUrl);
            var listToDoAuto2 = await autoStore.FindAsync(query);
            SetRootUrlToKinveyClient(kinveyUrl);

            // Teardown
            await networkStore.RemoveAsync(newItem1.ID);
            await networkStore.RemoveAsync(newItem2.ID);
            await networkStore.RemoveAsync(newItem3.ID);

            // Assert
            Assert.IsNotNull(listToDoAuto1);
            Assert.IsNotNull(listToDoAuto2);
            Assert.AreEqual(newItem3.Details, listToDoAuto2[0].Details);
            Assert.AreEqual(newItem2.Details, listToDoAuto2[1].Details);
            Assert.AreEqual(newItem1.Details, listToDoAuto2[2].Details);
        }

        [TestMethod]
        public async Task TestFindWithDeltaSetTurnOnNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(6, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };            

            autoStore.DeltaSetFetchingEnabled = true;

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await networkStore.SaveAsync(newItem1);
            newItem2 = await networkStore.SaveAsync(newItem2);

            // Act          
            var listToDoAuto1 = await autoStore.FindAsync();

            SetRootUrlToKinveyClient(unreachableUrl);
            var listToDoAuto2 = await autoStore.FindAsync();
            SetRootUrlToKinveyClient(kinveyUrl);

            // Teardown
            await networkStore.RemoveAsync(newItem1.ID);
            await networkStore.RemoveAsync(newItem2.ID);

            // Assert
            Assert.IsNotNull(listToDoAuto1);
            Assert.IsNotNull(listToDoAuto2);
            Assert.AreEqual(2, listToDoAuto1.Count);
            Assert.AreEqual(2, listToDoAuto2.Count);
        }

        [TestMethod]
        public async Task TestFindDeletedItemNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(7, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await networkStore.SaveAsync(newItem1);
            newItem2 = await networkStore.SaveAsync(newItem2);

            // Act          
            var listToDoAuto1 = await autoStore.FindAsync();
            await networkStore.RemoveAsync(newItem1.ID);
            var listToDoAuto2 = await autoStore.FindAsync();
           
            KinveyException syncStoreException = null;
            ToDo existingToDo = null;
            try
            {
                existingToDo = await syncStore.FindByIDAsync(newItem1.ID);
            }
            catch (KinveyException kinveyException)
            {
                syncStoreException = kinveyException;
            }

            // Teardown
            await networkStore.RemoveAsync(newItem2.ID);

            // Assert
            Assert.IsNotNull(listToDoAuto1);
            Assert.IsNotNull(listToDoAuto2);
            Assert.AreEqual(2, listToDoAuto1.Count);
            Assert.AreEqual(1, listToDoAuto2.Count);
            Assert.AreEqual(newItem2.Name, listToDoAuto2[0].Name);
            Assert.AreEqual(newItem2.Details, listToDoAuto2[0].Details);
            Assert.AreEqual(newItem2.DueDate, listToDoAuto2[0].DueDate);
            Assert.IsNull(existingToDo);
            Assert.IsNotNull(syncStoreException);
            Assert.AreEqual(EnumErrorCategory.ERROR_DATASTORE_CACHE, syncStoreException.ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_DATASTORE_CACHE_FIND_BY_ID_NOT_FOUND, syncStoreException.ErrorCode);
        }

        [TestMethod]
        public async Task TestFindByQueryNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(6, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "todo",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem2 = new ToDo
            {
                Name = "another todo",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
           
            var query = autoStore.Where(x => x.Details.StartsWith("details for 1"));
           
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await autoStore.SaveAsync(newItem1);
            newItem2 = await autoStore.SaveAsync(newItem2);
           
            // Act          
            var listToDoNetwork = await autoStore.FindAsync(query);

            // Teardown
            await autoStore.RemoveAsync(newItem1.ID);
            await autoStore.RemoveAsync(newItem2.ID);

            // Assert
            Assert.IsNotNull(listToDoNetwork);
            Assert.AreEqual(1, listToDoNetwork.Count);
            Assert.AreEqual(newItem1.Name, listToDoNetwork.First().Name);
            Assert.AreEqual(newItem1.Details, listToDoNetwork.First().Details);
            Assert.AreEqual(newItem1.DueDate, listToDoNetwork.First().DueDate);
        }

        [TestMethod]
        public async Task TestFindByQueryNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(1, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "todo",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem2 = new ToDo
            {
                Name = "another todo",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var query = autoStore.Where(x => x.Details.StartsWith("details for 1"));
           
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            SetRootUrlToKinveyClient(unreachableUrl);

            newItem1 = await syncStore.SaveAsync(newItem1);
            newItem2 = await syncStore.SaveAsync(newItem2);
           
            // Act          
            var listToDoCache = await autoStore.FindAsync(query);

            // Teardown
            await syncStore.RemoveAsync(newItem1.ID);
            await syncStore.RemoveAsync(newItem2.ID);

            // Assert
            Assert.IsNotNull(listToDoCache);
            Assert.AreEqual(1, listToDoCache.Count);
            Assert.AreEqual(newItem1.Name, listToDoCache.First().Name);
            Assert.AreEqual(newItem1.Details, listToDoCache.First().Details);
            Assert.AreEqual(newItem1.DueDate, listToDoCache.First().DueDate);
        }

        [TestMethod]
        public async Task TestFindByQueryTake1NetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(6, kinveyClient);
            }
          
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "todo",
                Details = "details for 1",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem2 = new ToDo
            {
                Name = "details for 2",
                Details = "details for 2",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var query = autoStore.Where(x => x.Details.StartsWith("det")).Take(1);
         
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await autoStore.SaveAsync(newItem1);
            newItem2 = await autoStore.SaveAsync(newItem2);

            //Act
            var listToDoNetwork = await autoStore.FindAsync(query);

            // Teardown
            await autoStore.RemoveAsync(newItem1.ID);
            await autoStore.RemoveAsync(newItem2.ID);

            // Assert
            Assert.IsNotNull(listToDoNetwork);
            Assert.AreEqual(1, listToDoNetwork.Count);
        }

        [TestMethod]
        public async Task TestFindByQueryTake1NetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(1, kinveyClient);
            }
                    
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "todo",
                Details = "details for 1",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem2 = new ToDo
            {
                Name = "details for 2",
                Details = "details for 2",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
          
            var query = autoStore.Where(x => x.Details.StartsWith("det")).Take(1);

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            SetRootUrlToKinveyClient(unreachableUrl);

            newItem1 = await syncStore.SaveAsync(newItem1);
            newItem2 = await syncStore.SaveAsync(newItem2);
           
            //Act
            var listToDoCache = await autoStore.FindAsync(query);

            // Teardown
            await syncStore.RemoveAsync(newItem1.ID);
            await syncStore.RemoveAsync(newItem2.ID);

            // Assert
            Assert.IsNotNull(listToDoCache);
            Assert.AreEqual(1, listToDoCache.Count);
        }

        [TestMethod]
        public async Task TestFindByQuerySkip1NetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(6, kinveyClient);
            }
           
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "todo",
                Details = "details for 1",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem2 = new ToDo
            {
                Name = "details for 2",
                Details = "details for 2",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var query = autoStore.Where(x => x.Details.StartsWith("det")).Skip(1);

            await User.LoginAsync(TestSetup.user, TestSetup.pass);

            newItem1 = await autoStore.SaveAsync(newItem1);
            newItem2 = await autoStore.SaveAsync(newItem2);
            
            //Act
            var listToDoNetwork = await autoStore.FindAsync(query);

            // Teardown
            await autoStore.RemoveAsync(newItem1.ID);
            await autoStore.RemoveAsync(newItem2.ID);

            // Assert
            Assert.IsNotNull(listToDoNetwork);
            Assert.AreEqual(1, listToDoNetwork.Count);
        }

        [TestMethod]
        public async Task TestFindByQuerySkip1NetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(1, kinveyClient);
            }
          
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "todo",
                Details = "details for 1",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem2 = new ToDo
            {
                Name = "details for 2",
                Details = "details for 2",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var query = autoStore.Where(x => x.Details.StartsWith("det")).Skip(1);
           
            await User.LoginAsync(TestSetup.user, TestSetup.pass);

            SetRootUrlToKinveyClient(unreachableUrl);

            newItem1 = await syncStore.SaveAsync(newItem1);
            newItem2 = await syncStore.SaveAsync(newItem2);
           
            //Act
            var listToDoCache = await autoStore.FindAsync(query);

            // Teardown
            await syncStore.RemoveAsync(newItem1.ID);
            await syncStore.RemoveAsync(newItem2.ID);

            // Assert
            Assert.IsNotNull(listToDoCache);
            Assert.AreEqual(1, listToDoCache.Count);
        }

        [TestMethod]
        public async Task TestFindByQuerySkip1Take1NetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }
           
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "todo",
                Details = "details for 1",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem2 = new ToDo
            {
                Name = "another todo",
                Details = "details for 2",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem3 = new ToDo
            {
                Name = "yet another todo",
                Details = "details for 3",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var query = autoStore.Where(x => x.Details.StartsWith("det")).Skip(1).Take(1);

            await User.LoginAsync(TestSetup.user, TestSetup.pass);

            newItem1 = await autoStore.SaveAsync(newItem1);
            newItem2 = await autoStore.SaveAsync(newItem2);
            newItem3 = await autoStore.SaveAsync(newItem3);
            
            // Act
            var listToDoNetwork = await autoStore.FindAsync(query);

            // Teardown
            await autoStore.RemoveAsync(newItem1.ID);
            await autoStore.RemoveAsync(newItem2.ID);
            await autoStore.RemoveAsync(newItem3.ID);

            // Assert
            Assert.IsNotNull(listToDoNetwork);
            Assert.AreEqual(listToDoNetwork.Count, 1);
        }

        [TestMethod]
        public async Task TestFindByQuerySkip1Take1NetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(1, kinveyClient);
            }
                     
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "todo",
                Details = "details for 1",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem2 = new ToDo
            {
                Name = "another todo",
                Details = "details for 2",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem3 = new ToDo
            {
                Name = "yet another todo",
                Details = "details for 3",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var query = syncStore.Where(x => x.Details.StartsWith("det")).Skip(1).Take(1);
           
            await User.LoginAsync(TestSetup.user, TestSetup.pass);

            SetRootUrlToKinveyClient(unreachableUrl);

            newItem1 = await syncStore.SaveAsync(newItem1);
            newItem2 = await syncStore.SaveAsync(newItem2);
            newItem3 = await syncStore.SaveAsync(newItem3);
            
            // Act
            var listToDoCache = await autoStore.FindAsync(query);

            // Teardown
            await syncStore.RemoveAsync(newItem1.ID);
            await syncStore.RemoveAsync(newItem2.ID);
            await syncStore.RemoveAsync(newItem3.ID);

            // Assert
            Assert.IsNotNull(listToDoCache);
            Assert.AreEqual(listToDoCache.Count, 1);
        }

        #endregion Find

        #region Find by id

        [TestMethod]
        public async Task TestFindByIDExistingItemNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(6, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "Next Task1",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            var newItem2 = new ToDo
            {
                Name = "Next Task2",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await networkStore.SaveAsync(newItem1);
            var savedItem2 = await networkStore.SaveAsync(newItem2);

            // Act
            var existingItemInNetwork = await autoStore.FindByIDAsync(savedItem1.ID);
            var existingItemInLocalStorage = await syncStore.FindByIDAsync(savedItem1.ID);

            // Teardown
            await networkStore.RemoveAsync(savedItem1.ID);
            await networkStore.RemoveAsync(savedItem2.ID);

            // Assert
            Assert.IsNotNull(existingItemInNetwork);
            Assert.IsNotNull(existingItemInLocalStorage);
            Assert.AreEqual(existingItemInNetwork.ID, savedItem1.ID);
            Assert.AreEqual(existingItemInNetwork.Name, savedItem1.Name);
            Assert.AreEqual(existingItemInNetwork.Details, savedItem1.Details);
            Assert.AreEqual(existingItemInNetwork.DueDate, savedItem1.DueDate);
            Assert.AreEqual(existingItemInLocalStorage.ID, savedItem1.ID);
            Assert.AreEqual(existingItemInLocalStorage.Name, savedItem1.Name);
            Assert.AreEqual(existingItemInLocalStorage.Details, savedItem1.Details);
            Assert.AreEqual(existingItemInLocalStorage.DueDate, savedItem1.DueDate);
        }

        [TestMethod]
        public async Task TestFindByIDNotExistingItemNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(4, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem = new ToDo
            {
                Name = "Next Task1",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            KinveyException exception = null;
            ToDo existingItem = null;

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem = await networkStore.SaveAsync(newItem);

            // Act
            try
            {
                existingItem = await autoStore.FindByIDAsync(Guid.NewGuid().ToString());
            }
            catch (KinveyException kinveyException)
            {
                exception = kinveyException;
            }

            // Teardown
            await autoStore.RemoveAsync(savedItem.ID);

            // Assert
            if (MockData)
            {
                Assert.IsNull(existingItem);
            }
            else
            {
                Assert.IsTrue(exception.GetType() == typeof(KinveyException));
                KinveyException ke = exception as KinveyException;
                Assert.IsTrue(ke.ErrorCategory == EnumErrorCategory.ERROR_BACKEND);
                Assert.IsTrue(ke.ErrorCode == EnumErrorCode.ERROR_JSON_RESPONSE);
            }
        }

        [TestMethod]
        public async Task TestFindByIDInvalidPermissionsNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(2, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            await User.LoginAsync(TestSetup.user_without_permissions, TestSetup.pass_for_user_without_permissions, kinveyClient);

            // Act
            var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                await autoStore.FindByIDAsync(Guid.NewGuid().ToString());
            });

            // Assert
            Assert.IsTrue(exception.GetType() == typeof(KinveyException));
            KinveyException ke = exception as KinveyException;
            Assert.IsTrue(ke.ErrorCategory == EnumErrorCategory.ERROR_BACKEND);
            Assert.IsTrue(ke.ErrorCode == EnumErrorCode.ERROR_JSON_RESPONSE);
        }

        [TestMethod]
        public async Task TestFindByIDExistingItemNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(6, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "Next Task1",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            var newItem2 = new ToDo
            {
                Name = "Next Task2",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await networkStore.SaveAsync(newItem1);
            var savedItem2 = await networkStore.SaveAsync(newItem2);

            // Act            
            var existingItemInNetwork = await autoStore.FindByIDAsync(savedItem1.ID);

            SetRootUrlToKinveyClient(unreachableUrl);
            var existingItemInLocalStorage = await autoStore.FindByIDAsync(savedItem1.ID);
            SetRootUrlToKinveyClient(kinveyUrl);

            // Teardown
            await networkStore.RemoveAsync(savedItem1.ID);
            await networkStore.RemoveAsync(savedItem2.ID);

            // Assert
            Assert.IsNotNull(existingItemInNetwork);
            Assert.IsNotNull(existingItemInLocalStorage);
            Assert.AreEqual(existingItemInNetwork.ID, savedItem1.ID);
            Assert.AreEqual(existingItemInNetwork.Name, savedItem1.Name);
            Assert.AreEqual(existingItemInNetwork.Details, savedItem1.Details);
            Assert.AreEqual(existingItemInNetwork.DueDate, savedItem1.DueDate);
            Assert.AreEqual(existingItemInLocalStorage.ID, savedItem1.ID);
            Assert.AreEqual(existingItemInLocalStorage.Name, savedItem1.Name);
            Assert.AreEqual(existingItemInLocalStorage.Details, savedItem1.Details);
            Assert.AreEqual(existingItemInLocalStorage.DueDate, savedItem1.DueDate);
        }

        [TestMethod]
        public async Task TestFindByIDDeletedItemNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(6, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Next Task1",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            var newItem2 = new ToDo
            {
                Name = "Next Task2",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            ToDo existingItemInNetwork = null;
            KinveyException exception = null;

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);

            // Act
            var existingItemInLocalStorage1 = await syncStore.FindByIDAsync(savedItem1.ID);

            await autoStore.RemoveAsync(savedItem1.ID);

            try
            {
                existingItemInNetwork = await autoStore.FindByIDAsync(savedItem1.ID);
            }
            catch (KinveyException kinveyException)
            {
                exception = kinveyException;
            }

            KinveyException syncStoreException = null;
            ToDo existingItemInLocalStorage2 = null;
            try
            {
                existingItemInLocalStorage2 = await syncStore.FindByIDAsync(savedItem1.ID);
            }
            catch (KinveyException kinveyException)
            {
                syncStoreException = kinveyException;
            }
           
            // Teardown
            await autoStore.RemoveAsync(savedItem2.ID);

            // Assert
            if (MockData)
            {
                Assert.IsNull(existingItemInNetwork);
            }
            else
            {
                Assert.IsTrue(exception.GetType() == typeof(KinveyException));
                KinveyException ke = exception as KinveyException;
                Assert.IsTrue(ke.ErrorCategory == EnumErrorCategory.ERROR_BACKEND);
                Assert.IsTrue(ke.ErrorCode == EnumErrorCode.ERROR_JSON_RESPONSE);
            }

            Assert.IsNotNull(existingItemInLocalStorage1);
            Assert.IsNull(existingItemInLocalStorage2);
            Assert.IsNotNull(syncStoreException);
            Assert.AreEqual(EnumErrorCategory.ERROR_DATASTORE_CACHE, syncStoreException.ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_DATASTORE_CACHE_FIND_BY_ID_NOT_FOUND, syncStoreException.ErrorCode);
        }

        [TestMethod]
        public async Task TestFindByIDNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(4, kinveyClient);
            }
            
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem = new ToDo
            {
                Name = "Next Task",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem = await autoStore.SaveAsync(newItem);

            // Act
            var networkEntity = await autoStore.FindByIDAsync(savedItem.ID);

            // Teardown
            await autoStore.RemoveAsync(savedItem.ID);

            // Assert
            Assert.IsNotNull(networkEntity);
            Assert.AreEqual(savedItem.ID, networkEntity.ID);
            Assert.AreEqual(savedItem.Name, networkEntity.Name);
            Assert.AreEqual(savedItem.Details, networkEntity.Details);
            Assert.AreEqual(savedItem.DueDate, networkEntity.DueDate);
        }

        [TestMethod]
        public async Task TestFindByIDNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(1, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem = new ToDo
            {
                Name = "Next Task",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };
            
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            SetRootUrlToKinveyClient(unreachableUrl);

            var savedItem = await syncStore.SaveAsync(newItem);

            // Act
            var cacheEntity = await autoStore.FindByIDAsync(savedItem.ID);

            // Teardown
            await syncStore.RemoveAsync(savedItem.ID);

            // Assert
            Assert.IsNotNull(cacheEntity);
            Assert.AreEqual(savedItem.ID, cacheEntity.ID);
            Assert.AreEqual(savedItem.Name, cacheEntity.Name);
            Assert.AreEqual(savedItem.Details, cacheEntity.Details);
            Assert.AreEqual(savedItem.DueDate, cacheEntity.DueDate);
        }

        #endregion Find by id

        #region Group and aggregate

        [TestMethod]
        public async Task TestGetSumNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }

            // Arrange           
            var autoStore = DataStore<Person>.Collection(personCollection, DataStoreType.AUTO);

            var p1 = new Person
            {
                FirstName = "Michael",
                LastName = "Bluth",
                Age = 40
            };           
            var p2 = new Person
            {
                FirstName = "George Michael",
                LastName = "Bluth",
                Age = 15
            };            
            var p3 = new Person
            {
                FirstName = "Tobias",
                LastName = "Funke",
                Age = 46
            };

            var query = autoStore.Where(x => x.LastName.Equals("Bluth"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            p1 = await autoStore.SaveAsync(p1);
            p2 = await autoStore.SaveAsync(p2);
            p3 = await autoStore.SaveAsync(p3);
            
            // Act
            var networkResult = await autoStore.GroupAndAggregateAsync(EnumReduceFunction.REDUCE_FUNCTION_SUM, "", "Age", query);

            // Teardown
            await autoStore.RemoveAsync(p3.ID);
            await autoStore.RemoveAsync(p2.ID);
            await autoStore.RemoveAsync(p1.ID);

            // Assert
            Assert.IsNotNull(networkResult);
            Assert.AreEqual(p1.Age + p2.Age, networkResult.First().Result);
        }

        [TestMethod]
        public async Task TestGetSumNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(1, kinveyClient);
            }

            // Arrange           
            var autoStore = DataStore<Person>.Collection(personCollection, DataStoreType.AUTO);
            var syncStore = DataStore<Person>.Collection(personCollection, DataStoreType.SYNC);

            var p1 = new Person
            {
                FirstName = "Michael",
                LastName = "Bluth",
                Age = 40
            };           
            var p2 = new Person
            {
                FirstName = "George Michael",
                LastName = "Bluth",
                Age = 15
            };            
            var p3 = new Person
            {
                FirstName = "Tobias",
                LastName = "Funke",
                Age = 46
            };
            
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            SetRootUrlToKinveyClient(unreachableUrl);

            p1 = await syncStore.SaveAsync(p1);
            p2 = await syncStore.SaveAsync(p2);
            p3 = await syncStore.SaveAsync(p3);

            var query = autoStore.Where(x => x.LastName.Equals("Bluth"));

            // Act
            var cacheResult = await autoStore.GroupAndAggregateAsync(EnumReduceFunction.REDUCE_FUNCTION_SUM, "", "Age", query);

            // Teardown
            await syncStore.RemoveAsync(p3.ID);
            await syncStore.RemoveAsync(p2.ID);
            await syncStore.RemoveAsync(p1.ID);

            // Assert
            Assert.IsNotNull(cacheResult);
            Assert.AreEqual(p1.Age + p2.Age, cacheResult.First().Result);
        }

        [TestMethod]
        public async Task TestGetMinNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }

            // Arrange            
            var autoStore = DataStore<Person>.Collection(personCollection, DataStoreType.AUTO);

            var p1 = new Person
            {
                FirstName = "Michael",
                LastName = "Bluth",
                Age = 40
            };           
            var p2 = new Person
            {
                FirstName = "George Michael",
                LastName = "Bluth",
                Age = 15
            };
            var p3 = new Person
            {
                FirstName = "Tobias",
                LastName = "Funke",
                Age = 46
            };
            
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            p1 = await autoStore.SaveAsync(p1);
            p2 = await autoStore.SaveAsync(p2);
            p3 = await autoStore.SaveAsync(p3);

            // Act
            var networkResult = await autoStore.GroupAndAggregateAsync(EnumReduceFunction.REDUCE_FUNCTION_MIN, "", "Age", null);

            // Teardown
            await autoStore.RemoveAsync(p3.ID);
            await autoStore.RemoveAsync(p2.ID);
            await autoStore.RemoveAsync(p1.ID);

            // Assert
            Assert.IsNotNull(networkResult);
            Assert.AreEqual(p2.Age, networkResult.First().Result);
        }

        [TestMethod]
        public async Task TestGetMinNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(1, kinveyClient);
            }

            // Arrange          
            var autoStore = DataStore<Person>.Collection(personCollection, DataStoreType.AUTO);
            var syncStore = DataStore<Person>.Collection(personCollection, DataStoreType.SYNC);

            var p1 = new Person
            {
                FirstName = "Michael",
                LastName = "Bluth",
                Age = 40
            };           
            var p2 = new Person
            {
                FirstName = "George Michael",
                LastName = "Bluth",
                Age = 15
            };            
            var p3 = new Person
            {
                FirstName = "Tobias",
                LastName = "Funke",
                Age = 46
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            SetRootUrlToKinveyClient(unreachableUrl);

            p1 = await syncStore.SaveAsync(p1);
            p2 = await syncStore.SaveAsync(p2);
            p3 = await syncStore.SaveAsync(p3);

            // Act
            var cacheResult = await autoStore.GroupAndAggregateAsync(EnumReduceFunction.REDUCE_FUNCTION_MIN, "", "Age", null);

            // Teardown
            await syncStore.RemoveAsync(p3.ID);
            await syncStore.RemoveAsync(p2.ID);
            await syncStore.RemoveAsync(p1.ID);

            // Assert
            Assert.IsNotNull(cacheResult);
            Assert.AreEqual(p2.Age, cacheResult.First().Result);
        }

        [TestMethod]
        public async Task TestGetMaxNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }

            // Arrange           
            var autoStore = DataStore<Person>.Collection(personCollection, DataStoreType.AUTO);

            var p1 = new Person
            {
                FirstName = "Michael",
                LastName = "Bluth",
                Age = 40
            };            
            var p2 = new Person
            {
                FirstName = "George Michael",
                LastName = "Bluth",
                Age = 15
            };           
            var p3 = new Person
            {
                FirstName = "Tobias",
                LastName = "Funke",
                Age = 46
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            p1 = await autoStore.SaveAsync(p1);
            p2 = await autoStore.SaveAsync(p2);
            p3 = await autoStore.SaveAsync(p3);

            // Act
            var networkResult = await autoStore.GroupAndAggregateAsync(EnumReduceFunction.REDUCE_FUNCTION_MAX, "", "Age", null);

            // Teardown
            await autoStore.RemoveAsync(p3.ID);
            await autoStore.RemoveAsync(p2.ID);
            await autoStore.RemoveAsync(p1.ID);

            // Assert
            Assert.IsNotNull(networkResult);
            Assert.AreEqual(p3.Age, networkResult.First().Result);
        }

        [TestMethod]
        public async Task TestGetMaxNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(1, kinveyClient);
            }

            // Arrange                       
            var autoStore = DataStore<Person>.Collection(personCollection, DataStoreType.AUTO);
            var syncStore = DataStore<Person>.Collection(personCollection, DataStoreType.SYNC);

            var p1 = new Person
            {
                FirstName = "Michael",
                LastName = "Bluth",
                Age = 40
            };            
            var p2 = new Person
            {
                FirstName = "George Michael",
                LastName = "Bluth",
                Age = 15
            };            
            var p3 = new Person
            {
                FirstName = "Tobias",
                LastName = "Funke",
                Age = 46
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            SetRootUrlToKinveyClient(unreachableUrl);

            p1 = await syncStore.SaveAsync(p1);
            p2 = await syncStore.SaveAsync(p2);
            p3 = await syncStore.SaveAsync(p3);

            // Act
            var cacheResult = await autoStore.GroupAndAggregateAsync(EnumReduceFunction.REDUCE_FUNCTION_MAX, "", "Age", null);

            // Teardown
            await syncStore.RemoveAsync(p3.ID);
            await syncStore.RemoveAsync(p2.ID);
            await syncStore.RemoveAsync(p1.ID);

            // Assert
            Assert.IsNotNull(cacheResult);
            Assert.AreEqual(p3.Age, cacheResult.First().Result);
        }

        [TestMethod]
        public async Task TestGetAverageNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(10, kinveyClient);
            }

            // Arrange            
            var autoStore = DataStore<Person>.Collection(personCollection, DataStoreType.AUTO);

            var p1 = new Person
            {
                FirstName = "Michael",
                LastName = "Bluth",
                Age = 40
            };
            var p2 = new Person
            {
                FirstName = "George Michael",
                LastName = "Bluth",
                Age = 15
            };
            var p3 = new Person
            {
                FirstName = "Tobias",
                LastName = "Funke",
                Age = 46
            };            
            var p4 = new Person
            {
                FirstName = "Buster",
                LastName = "Bluth",
                Age = 19
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            p1 = await autoStore.SaveAsync(p1);
            p2 = await autoStore.SaveAsync(p2);
            p3 = await autoStore.SaveAsync(p3);
            p4 = await autoStore.SaveAsync(p4);

            // Act
            var networkResult = await autoStore.GroupAndAggregateAsync(EnumReduceFunction.REDUCE_FUNCTION_AVERAGE, "", "Age", null);

            // Teardown
            await autoStore.RemoveAsync(p4.ID);
            await autoStore.RemoveAsync(p3.ID);
            await autoStore.RemoveAsync(p2.ID);
            await autoStore.RemoveAsync(p1.ID);

            // Assert
            Assert.IsNotNull(networkResult);
            Assert.AreEqual((p1.Age + p2.Age + p3.Age + p4.Age) / 4, networkResult.First().Result);
        }

        [TestMethod]
        public async Task TestGetAverageNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(1, kinveyClient);
            }

            // Arrange                      
            var autoStore = DataStore<Person>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<Person>.Collection(toDosCollection, DataStoreType.SYNC);

            var p1 = new Person
            {
                FirstName = "Michael",
                LastName = "Bluth",
                Age = 40
            };           
            var p2 = new Person
            {
                FirstName = "George Michael",
                LastName = "Bluth",
                Age = 15
            };            
            var p3 = new Person
            {
                FirstName = "Tobias",
                LastName = "Funke",
                Age = 46
            };            
            var p4 = new Person
            {
                FirstName = "Buster",
                LastName = "Bluth",
                Age = 19
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            SetRootUrlToKinveyClient(unreachableUrl);

            p1 = await syncStore.SaveAsync(p1);
            p2 = await syncStore.SaveAsync(p2);
            p3 = await syncStore.SaveAsync(p3);
            p4 = await syncStore.SaveAsync(p4);

            // Act
            var cacheResult = await autoStore.GroupAndAggregateAsync(EnumReduceFunction.REDUCE_FUNCTION_AVERAGE, "", "Age", null);

            // Teardown
            await syncStore.RemoveAsync(p4.ID);
            await syncStore.RemoveAsync(p3.ID);
            await syncStore.RemoveAsync(p2.ID);
            await syncStore.RemoveAsync(p1.ID);

            // Assert
            Assert.IsNotNull(cacheResult);
            Assert.AreEqual((p1.Age + p2.Age + p3.Age + p4.Age) / 4, cacheResult.First().Result);
        }

        #endregion Group and aggregate  

        #region Save

        [TestMethod]
        public async Task TestSaveCreatingItemWithoutProvidedIdNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(4, kinveyClient);
            }

            //Arrange
            var autoStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);
            var syncStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.SYNC);

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            fc1 = await autoStore.SaveAsync(fc1);

            var networkEntities = await networkStore.FindAsync();
            var localEntities = await syncStore.FindAsync();

            //Teardown
            foreach (var networkEntity in networkEntities)
            {
                await networkStore.RemoveAsync(networkEntity.ID);
            }

            // Assert
            Assert.IsNotNull(networkEntities);
            Assert.AreEqual(1, networkEntities.Count);
            Assert.AreEqual(1, localEntities.Count);
            Assert.IsNotNull(networkEntities[0].ID);
            Assert.AreEqual(fc1.Question, networkEntities[0].Question);
            Assert.AreEqual(fc1.Answer, networkEntities[0].Answer);
            Assert.IsNotNull(localEntities[0].ID);
            Assert.AreEqual(fc1.Question, localEntities[0].Question);
            Assert.AreEqual(fc1.Answer, localEntities[0].Answer);
        }

        [TestMethod]
        public async Task TestSaveCreatingItemWithProvidedIdNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(4, kinveyClient);
            }

            //Arrange
            var autoStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);
            var syncStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.SYNC);

            var id = Guid.NewGuid().ToString();

            var fc1 = new FlashCard
            {
                ID = id,
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            fc1 = await autoStore.SaveAsync(fc1);

            var networkEntities = await networkStore.FindAsync();
            var localEntities = await syncStore.FindAsync();

            //Teardown
            foreach (var networkEntity in networkEntities)
            {
                await networkStore.RemoveAsync(networkEntity.ID);
            }

            // Assert

            Assert.IsNotNull(networkEntities);
            Assert.AreEqual(1, networkEntities.Count);
            Assert.AreEqual(1, localEntities.Count);
            Assert.AreEqual(id, networkEntities[0].ID);
            Assert.AreEqual(fc1.Question, networkEntities[0].Question);
            Assert.AreEqual(fc1.Answer, networkEntities[0].Answer);
            Assert.AreEqual(id, localEntities[0].ID);
            Assert.AreEqual(fc1.Question, localEntities[0].Question);
            Assert.AreEqual(fc1.Answer, localEntities[0].Answer);
        }

        [TestMethod]
        public async Task TestSaveUpdatingItemWithExistingIdNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(5, kinveyClient);
            }

            //Arrange
            var autoStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            fc1 = await autoStore.SaveAsync(fc1);
            fc1.Answer = "7!";
            fc1 = await autoStore.SaveAsync(fc1);

            var networkEntities = await networkStore.FindAsync();

            //Teardown
            foreach (var networkEntity in networkEntities)
            {
                await networkStore.RemoveAsync(networkEntity.ID);
            }

            // Assert

            Assert.IsNotNull(networkEntities);
            Assert.AreEqual(1, networkEntities.Count);
            Assert.AreEqual(fc1.Question, networkEntities[0].Question);
            Assert.AreEqual(fc1.Answer, networkEntities[0].Answer);
        }

        [TestMethod]
        public async Task TestSaveDataNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(1, kinveyClient);
            }

            //Arrange
            var autoStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            SetRootUrlToKinveyClient(unreachableUrl);

            fc1 = await autoStore.SaveAsync(fc1);           
            var localEntities = await autoStore.FindAsync();

            SetRootUrlToKinveyClient(kinveyUrl);

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(flashCardCollection).GetAll();

            // Assert            
            Assert.IsNotNull(localEntities);
            Assert.IsNotNull(pendingWriteActions);
            Assert.AreEqual(1, localEntities.Count);
            Assert.AreEqual(fc1.ID, localEntities[0].ID);
            Assert.AreEqual(fc1.Question, localEntities[0].Question);
            Assert.AreEqual(fc1.Answer, localEntities[0].Answer);
            Assert.AreEqual(1, pendingWriteActions.Count);
            Assert.AreEqual(fc1.ID, pendingWriteActions[0].entityId);
            Assert.AreEqual("POST", pendingWriteActions[0].action);
            Assert.AreEqual(flashCardCollection, pendingWriteActions[0].collection);
        }

        [TestMethod]
        public async Task TestSaveInvalidPermissionsNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(2, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem = new ToDo
            {
                Name = "todo",
                Details = "details for task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            await User.LoginAsync(TestSetup.user_without_permissions, TestSetup.pass_for_user_without_permissions, kinveyClient);

            // Act
            var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                await autoStore.SaveAsync(newItem);
            });

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
            var existingItemsCache = await syncStore.FindAsync();

            // Assert
            Assert.AreEqual(typeof(KinveyException), exception.GetType());
            var ke = exception as KinveyException;
            Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, ke.ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, ke.ErrorCode);
            Assert.AreEqual(401, ke.StatusCode);

            Assert.IsNotNull(pendingWriteActions);
            Assert.AreEqual(1, pendingWriteActions.Count);
            Assert.IsNotNull(existingItemsCache);
            Assert.AreEqual(1, existingItemsCache.Count);
            Assert.AreEqual(newItem.Name, existingItemsCache[0].Name);
            Assert.AreEqual(newItem.Details, existingItemsCache[0].Details);
            Assert.AreEqual(newItem.DueDate, existingItemsCache[0].DueDate);

            Assert.AreEqual(existingItemsCache[0].ID, pendingWriteActions[0].entityId);
            Assert.AreEqual("POST", pendingWriteActions[0].action);
            Assert.AreEqual(toDosCollection, pendingWriteActions[0].collection);
        }

        [TestMethod]
        public async Task TestSaveDataAndPushNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(6, kinveyClient);
            }

            //Arrange
            var autoStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);

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

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            SetRootUrlToKinveyClient(unreachableUrl);

            fc1 = await autoStore.SaveAsync(fc1);
            fc2 = await autoStore.SaveAsync(fc2);
            var pendingWriteActions1 = kinveyClient.CacheManager.GetSyncQueue(flashCardCollection).GetAll();
            var localEntities = await autoStore.FindAsync();

            SetRootUrlToKinveyClient(kinveyUrl);

            var pushResult = await autoStore.PushAsync();

            var pendingWriteActions2 = kinveyClient.CacheManager.GetSyncQueue(flashCardCollection).GetAll();

            var networkEntities = await networkStore.FindAsync();

            //Teardown
            foreach (var networkEntity in networkEntities)
            {
                await networkStore.RemoveAsync(networkEntity.ID);
            }

            // Assert
            Assert.IsNotNull(pendingWriteActions1);
            Assert.IsNotNull(localEntities);
            Assert.IsNotNull(pushResult);
            Assert.IsNotNull(pendingWriteActions2);
            Assert.IsNotNull(networkEntities);
            Assert.AreEqual(2, pendingWriteActions1.Count);
            Assert.AreEqual(2, localEntities.Count);
            Assert.AreEqual(2, pushResult.PushCount);
            Assert.AreEqual(0, pendingWriteActions2.Count);
            Assert.AreEqual(2, networkEntities.Count);
        }

        [TestMethod]
        public async Task TestSaveUpdatingDataAndPushNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(5, kinveyClient);
            }

            //Arrange
            var autoStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            fc1 = await autoStore.SaveAsync(fc1);

            // Act
            SetRootUrlToKinveyClient(unreachableUrl);

            fc1.Answer = "7!";
            fc1 = await autoStore.SaveAsync(fc1);

            var pendingWriteActions1 = kinveyClient.CacheManager.GetSyncQueue(flashCardCollection).GetAll();
            var localEntities = await autoStore.FindAsync();

            SetRootUrlToKinveyClient(kinveyUrl);

            var pushResult = await autoStore.PushAsync();

            var pendingWriteActions2 = kinveyClient.CacheManager.GetSyncQueue(flashCardCollection).GetAll();

            var networkEntities = await networkStore.FindAsync();

            //Teardown
            foreach (var networkEntity in networkEntities)
            {
                await networkStore.RemoveAsync(networkEntity.ID);
            }

            // Assert
            Assert.IsNotNull(pendingWriteActions1);
            Assert.IsNotNull(localEntities);
            Assert.IsNotNull(pushResult);
            Assert.IsNotNull(pendingWriteActions2);
            Assert.IsNotNull(networkEntities);
            Assert.AreEqual(1, pendingWriteActions1.Count);
            Assert.AreEqual(fc1.ID, pendingWriteActions1[0].entityId);
            Assert.AreEqual("PUT", pendingWriteActions1[0].action);
            Assert.AreEqual(1, localEntities.Count);
            Assert.AreEqual(fc1.ID, localEntities[0].ID);
            Assert.AreEqual(fc1.Question, localEntities[0].Question);
            Assert.AreEqual(fc1.Answer, localEntities[0].Answer);
            Assert.AreEqual(1, pushResult.PushCount);
            Assert.AreEqual(0, pendingWriteActions2.Count);
            Assert.AreEqual(1, networkEntities.Count);
            Assert.AreEqual(fc1.ID, networkEntities[0].ID);
            Assert.AreEqual(fc1.Question, networkEntities[0].Question);
            Assert.AreEqual(fc1.Answer, networkEntities[0].Answer);
        }

        [TestMethod]
        public async Task TestSaveAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(4, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem = new ToDo
            {
                Name = "todo save",
                Details = "details for save",
                DueDate = "2016-04-22T19:56:00.961Z"
            };
           
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            var savedItem = await autoStore.SaveAsync(newItem);

            var existingItemNetwork = await autoStore.FindByIDAsync(savedItem.ID);
            var existingItemCache = await syncStore.FindByIDAsync(savedItem.ID);

            //Teardown
            await autoStore.RemoveAsync(savedItem.ID);

            // Assert
            Assert.IsNotNull(savedItem);
            Assert.IsNotNull(existingItemNetwork);
            Assert.IsNotNull(existingItemCache);
            Assert.AreEqual(savedItem.ID, existingItemNetwork.ID);
            Assert.AreEqual(savedItem.Details, existingItemNetwork.Details);
            Assert.AreEqual(savedItem.DueDate, existingItemNetwork.DueDate);
            Assert.AreEqual(savedItem.ID, existingItemCache.ID);
            Assert.AreEqual(savedItem.Details, existingItemCache.Details);
            Assert.AreEqual(savedItem.DueDate, existingItemCache.DueDate);
        }

        [TestMethod]
        public async Task TestSaveCreateVersion5ConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient("5");

            if (MockData)
            {
                MockResponses(4);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem = new ToDo
            {
                Name = "todo save",
                Details = "details for save",
                DueDate = "2016-04-22T19:56:00.961Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            var savedItem = await autoStore.SaveAsync(newItem);

            var existingItemsCache = await syncStore.FindAsync();
            var existingItemsNetwork = await networkStore.FindAsync();
            
            //Teardown
            await networkStore.RemoveAsync(savedItem.ID);

            // Assert
            Assert.IsNotNull(savedItem);
            Assert.AreEqual(newItem.Name, savedItem.Name);
            Assert.AreEqual(newItem.Details, savedItem.Details);
            Assert.AreEqual(newItem.DueDate, savedItem.DueDate);
            Assert.IsNotNull(existingItemsCache);
            Assert.AreEqual(1, existingItemsCache.Count);
            Assert.AreEqual(newItem.Name, existingItemsCache[0].Name);
            Assert.AreEqual(newItem.Details, existingItemsCache[0].Details);
            Assert.AreEqual(newItem.DueDate, existingItemsCache[0].DueDate);
            Assert.IsNotNull(existingItemsCache[0].Acl);
            Assert.IsNotNull(existingItemsCache[0].Kmd);
            Assert.IsFalse(string.IsNullOrEmpty(existingItemsCache[0].Kmd.entityCreationTime));
            Assert.IsFalse(string.IsNullOrEmpty(existingItemsCache[0].Kmd.lastModifiedTime));
            Assert.IsNotNull(existingItemsNetwork);
            Assert.AreEqual(1, existingItemsNetwork.Count);
            Assert.AreEqual(newItem.Name, existingItemsNetwork[0].Name);
            Assert.AreEqual(newItem.Details, existingItemsNetwork[0].Details);
            Assert.AreEqual(newItem.DueDate, existingItemsNetwork[0].DueDate);          
        }

        [TestMethod]
        public async Task TestSaveCreateVersion5ConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient("5");

            if (MockData)
            {
                MockResponses(1);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem = new ToDo
            {
                Name = "todo save",
                Details = "details for save",
                DueDate = "2016-04-22T19:56:00.961Z",
                GeoLoc = "[200,200]"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act          
            SetRootUrlToKinveyClient(unreachableUrl);
            var savedItem = await autoStore.SaveAsync(newItem);
            SetRootUrlToKinveyClient(kinveyUrl);

            var existingItemsCache = await syncStore.FindAsync();
            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();

            //Teardown
            await syncStore.RemoveAsync(savedItem.ID);

            // Assert
            Assert.IsNotNull(savedItem);
            Assert.AreEqual(newItem.Name, savedItem.Name);
            Assert.AreEqual(newItem.Details, savedItem.Details);
            Assert.AreEqual(newItem.DueDate, savedItem.DueDate);
            Assert.IsNotNull(existingItemsCache);
            Assert.AreEqual(1, existingItemsCache.Count);
            Assert.AreEqual(newItem.Name, existingItemsCache[0].Name);
            Assert.AreEqual(newItem.Details, existingItemsCache[0].Details);
            Assert.AreEqual(newItem.DueDate, existingItemsCache[0].DueDate);
            Assert.IsNull(existingItemsCache[0].Acl);
            Assert.IsNull(existingItemsCache[0].Kmd);
            Assert.AreEqual(1, pendingWriteActions.Count);
            var pendingWriteAction1 = pendingWriteActions.FirstOrDefault(e => e.entityId == savedItem.ID);
            Assert.IsNotNull(pendingWriteAction1);
            Assert.AreEqual("POST", pendingWriteAction1.action);
        }

        [TestMethod]
        public async Task TestSaveUpdateVersion5ConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient("5");

            if (MockData)
            {
                MockResponses(4);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem = new ToDo
            {
                ID = Guid.NewGuid().ToString(),
                Name = "todo save",
                Details = "details for save",
                DueDate = "2016-04-22T19:56:00.961Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            var savedItem = await autoStore.SaveAsync(newItem);

            var existingItemsCache = await syncStore.FindAsync();
            var existingItemsNetwork = await networkStore.FindAsync();


            //Teardown
            await networkStore.RemoveAsync(savedItem.ID);

            // Assert
            Assert.IsNotNull(savedItem);
            Assert.AreEqual(newItem.ID, savedItem.ID);
            Assert.AreEqual(newItem.Name, savedItem.Name);
            Assert.AreEqual(newItem.Details, savedItem.Details);
            Assert.AreEqual(newItem.DueDate, savedItem.DueDate);
            Assert.IsNotNull(existingItemsCache);
            Assert.AreEqual(1, existingItemsCache.Count);
            Assert.AreEqual(newItem.ID, existingItemsCache[0].ID);
            Assert.AreEqual(newItem.Name, existingItemsCache[0].Name);
            Assert.AreEqual(newItem.Details, existingItemsCache[0].Details);
            Assert.AreEqual(newItem.DueDate, existingItemsCache[0].DueDate);
            Assert.IsNotNull(existingItemsCache[0].Acl);
            Assert.IsNotNull(existingItemsCache[0].Kmd);
            Assert.IsFalse(string.IsNullOrEmpty(existingItemsCache[0].Acl.Creator));
            Assert.IsFalse(string.IsNullOrEmpty(existingItemsCache[0].Kmd.entityCreationTime));
            Assert.IsNotNull(existingItemsNetwork);
            Assert.AreEqual(1, existingItemsNetwork.Count);
            Assert.AreEqual(newItem.ID, existingItemsNetwork[0].ID);
            Assert.AreEqual(newItem.Name, existingItemsNetwork[0].Name);
            Assert.AreEqual(newItem.Details, existingItemsNetwork[0].Details);
            Assert.AreEqual(newItem.DueDate, existingItemsNetwork[0].DueDate);
        }

        [TestMethod]
        public async Task TestSaveUpdateVersion5ConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient("5");

            if (MockData)
            {
                MockResponses(1);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem = new ToDo
            {
                ID = Guid.NewGuid().ToString(),
                Name = "todo save",
                Details = "details for save",
                DueDate = "2016-04-22T19:56:00.961Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            SetRootUrlToKinveyClient(unreachableUrl);
            var savedItem = await autoStore.SaveAsync(newItem);
            SetRootUrlToKinveyClient(kinveyUrl);

            var existingItemsCache = await syncStore.FindAsync();
            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();

            //Teardown
            await syncStore.RemoveAsync(savedItem.ID);

            // Assert
            Assert.IsNotNull(savedItem);
            Assert.AreEqual(newItem.ID, savedItem.ID);
            Assert.AreEqual(newItem.Name, savedItem.Name);
            Assert.AreEqual(newItem.Details, savedItem.Details);
            Assert.AreEqual(newItem.DueDate, savedItem.DueDate);
            Assert.IsNotNull(existingItemsCache);
            Assert.AreEqual(1, existingItemsCache.Count);
            Assert.AreEqual(newItem.ID, existingItemsCache[0].ID);
            Assert.AreEqual(newItem.Name, existingItemsCache[0].Name);
            Assert.AreEqual(newItem.Details, existingItemsCache[0].Details);
            Assert.AreEqual(newItem.DueDate, existingItemsCache[0].DueDate);
            Assert.IsNull(existingItemsCache[0].Acl);
            Assert.IsNull(existingItemsCache[0].Kmd);
            Assert.AreEqual(1, pendingWriteActions.Count);
            var pendingWriteAction1 = pendingWriteActions.FirstOrDefault(e => e.entityId == newItem.ID);
            Assert.IsNotNull(pendingWriteAction1);
            Assert.AreEqual("PUT", pendingWriteAction1.action);
        }

        [TestMethod]
        public async Task TestSaveMultiInsertNewItemsConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient("5");

            if (MockData)
            {
                MockResponses(5);
            }

            // Arrange
            var user = await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var autoToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);
            var syncToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC, kinveyClient);
            var networkToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK, kinveyClient);

            var toDos = new List<ToDo>
            {
                new ToDo { Name = "Name1", Details = "Details1", Value = 1 },
                new ToDo { Name = "Name2", Details = "Details2", Value = 2 }
            };

            // Act
            var savedToDos = await autoToDoStore.SaveAsync(toDos);

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
            var existingToDosCache = await syncToDoStore.FindAsync();
            var existingToDosNetwork = await networkToDoStore.FindAsync();

            // Teardown
            await autoToDoStore.RemoveAsync(savedToDos.Entities[0].ID);
            await autoToDoStore.RemoveAsync(savedToDos.Entities[1].ID);

            // Assert
            Assert.AreEqual(2, savedToDos.Entities.Count);
            Assert.AreEqual(0, savedToDos.Errors.Count);
            Assert.AreEqual(toDos[0].Name, savedToDos.Entities[0].Name);
            Assert.AreEqual(toDos[0].Details, savedToDos.Entities[0].Details);
            Assert.AreEqual(toDos[0].Value, savedToDos.Entities[0].Value);
            Assert.AreEqual(user.Id, savedToDos.Entities[0].Acl.Creator);
            Assert.AreEqual(toDos[1].Name, savedToDos.Entities[1].Name);
            Assert.AreEqual(toDos[1].Details, savedToDos.Entities[1].Details);
            Assert.AreEqual(toDos[1].Value, savedToDos.Entities[1].Value);
            Assert.AreEqual(user.Id, savedToDos.Entities[1].Acl.Creator);
            Assert.AreEqual(0, pendingWriteActions.Count);
            Assert.AreEqual(2, existingToDosCache.Count);
            Assert.IsNotNull(existingToDosCache.FirstOrDefault(e=> e.ID == savedToDos.Entities[0].ID && savedToDos.Entities[0].Kmd != null && savedToDos.Entities[0].Acl != null && !string.IsNullOrEmpty(e.Kmd.entityCreationTime) && !string.IsNullOrEmpty(e.Kmd.lastModifiedTime)));
            Assert.IsNotNull(existingToDosCache.FirstOrDefault(e => e.ID == savedToDos.Entities[1].ID && savedToDos.Entities[1].Kmd != null && savedToDos.Entities[1].Acl != null && !string.IsNullOrEmpty(e.Kmd.entityCreationTime) && !string.IsNullOrEmpty(e.Kmd.lastModifiedTime)));
            Assert.AreEqual(2, existingToDosNetwork.Count);
            Assert.IsNotNull(existingToDosNetwork.FirstOrDefault(e => e.ID == savedToDos.Entities[0].ID));
            Assert.IsNotNull(existingToDosNetwork.FirstOrDefault(e => e.ID == savedToDos.Entities[1].ID));
        }

        [TestMethod]
        public async Task TestSaveMultiInsertNewItemsConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient("5");

            if (MockData)
            {
                MockResponses(1);
            }

            // Arrange
            var user = await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var autoToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);
            var syncToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC, kinveyClient);

            var toDos = new List<ToDo>
            {
                new ToDo { Name = "Name1", Details = "Details1", Value = 1 },
                new ToDo { Name = "Name2", Details = "Details2", Value = 2 }
            };

            // Act
            SetRootUrlToKinveyClient(unreachableUrl);

            var savedToDos = await autoToDoStore.SaveAsync(toDos);

            SetRootUrlToKinveyClient(kinveyUrl);

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
            var existingToDos = await syncToDoStore.FindAsync();

            // Teardown
            await syncToDoStore.RemoveAsync(savedToDos.Entities[0].ID);
            await syncToDoStore.RemoveAsync(savedToDos.Entities[1].ID);
           
            // Assert
            Assert.AreEqual(2, savedToDos.Entities.Count);
            Assert.AreEqual(0, savedToDos.Errors.Count);
            Assert.AreEqual(toDos[0].Name, savedToDos.Entities[0].Name);
            Assert.AreEqual(toDos[0].Details, savedToDos.Entities[0].Details);
            Assert.AreEqual(toDos[0].Value, savedToDos.Entities[0].Value);
            Assert.IsNull(savedToDos.Entities[0].Acl);
            Assert.AreEqual(toDos[1].Name, savedToDos.Entities[1].Name);
            Assert.AreEqual(toDos[1].Details, savedToDos.Entities[1].Details);
            Assert.AreEqual(toDos[1].Value, savedToDos.Entities[1].Value);
            Assert.IsNull(savedToDos.Entities[1].Acl);

            Assert.AreEqual(2, pendingWriteActions.Count);
            var pendingWriteAction1 = pendingWriteActions.FirstOrDefault(e => e.entityId == savedToDos.Entities[0].ID);
            Assert.IsNotNull(pendingWriteAction1);
            Assert.AreEqual("POST", pendingWriteAction1.action);
            Assert.AreEqual(toDosCollection, pendingWriteAction1.collection);
            var pendingWriteAction2 = pendingWriteActions.FirstOrDefault(e => e.entityId == savedToDos.Entities[1].ID);
            Assert.IsNotNull(pendingWriteAction2);
            Assert.AreEqual("POST", pendingWriteAction2.action);
            Assert.AreEqual(toDosCollection, pendingWriteAction2.collection);

            Assert.AreEqual(2, existingToDos.Count);
            Assert.IsNotNull(existingToDos.FirstOrDefault(e => e.ID == savedToDos.Entities[0].ID));
            Assert.IsNotNull(existingToDos.FirstOrDefault(e => e.ID == savedToDos.Entities[1].ID));
        }

        [TestMethod]
        public async Task TestSaveMultiInsertExistingItemsConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient("5");

            if (MockData)
            {
                MockResponses(6);
            }

            // Arrange
            var user = await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var autoToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);
            var syncToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC, kinveyClient);
            var networkToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK, kinveyClient);

            var toDos = new List<ToDo>
            {
                new ToDo { ID = Guid.NewGuid().ToString(), Name = "Name1", Details = "Details1", Value = 1 },
                new ToDo { ID = Guid.NewGuid().ToString(), Name = "Name2", Details = "Details2", Value = 2 }
            };

            // Act
            var savedToDos = await autoToDoStore.SaveAsync(toDos);

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
            var existingToDosCache = await syncToDoStore.FindAsync();
            var existingToDosNetwork = await networkToDoStore.FindAsync();

            // Teardown
            await autoToDoStore.RemoveAsync(savedToDos.Entities[0].ID);
            await autoToDoStore.RemoveAsync(savedToDos.Entities[1].ID);

            // Assert
            Assert.AreEqual(2, savedToDos.Entities.Count);
            Assert.AreEqual(0, savedToDos.Errors.Count);
            Assert.AreEqual(toDos[0].ID, savedToDos.Entities[0].ID);
            Assert.AreEqual(toDos[0].Name, savedToDos.Entities[0].Name);
            Assert.AreEqual(toDos[0].Details, savedToDos.Entities[0].Details);
            Assert.AreEqual(toDos[0].Value, savedToDos.Entities[0].Value);
            Assert.AreEqual(user.Id, savedToDos.Entities[0].Acl.Creator);
            Assert.AreEqual(toDos[1].ID, savedToDos.Entities[1].ID);
            Assert.AreEqual(toDos[1].Name, savedToDos.Entities[1].Name);
            Assert.AreEqual(toDos[1].Details, savedToDos.Entities[1].Details);
            Assert.AreEqual(toDos[1].Value, savedToDos.Entities[1].Value);
            Assert.AreEqual(user.Id, savedToDos.Entities[1].Acl.Creator);
            Assert.AreEqual(0, pendingWriteActions.Count);
            Assert.AreEqual(2, existingToDosCache.Count);
            Assert.IsNotNull(existingToDosCache.FirstOrDefault(e => e.ID == toDos[0].ID && e.Name == toDos[0].Name && e.Details == toDos[0].Details && e.Value == toDos[0].Value && e.Kmd != null && e.Acl != null && !string.IsNullOrEmpty(e.Kmd.entityCreationTime) && !string.IsNullOrEmpty(e.Kmd.lastModifiedTime)));
            Assert.IsNotNull(existingToDosCache.FirstOrDefault(e => e.ID == toDos[1].ID && e.Name == toDos[1].Name && e.Details == toDos[1].Details && e.Value == toDos[1].Value && e.Kmd != null && e.Acl != null && !string.IsNullOrEmpty(e.Kmd.entityCreationTime) && !string.IsNullOrEmpty(e.Kmd.lastModifiedTime)));
            Assert.AreEqual(2, existingToDosNetwork.Count);
            Assert.IsNotNull(existingToDosNetwork.FirstOrDefault(e => e.ID == toDos[0].ID && e.Name == toDos[0].Name && e.Details == toDos[0].Details && e.Value == toDos[0].Value));
            Assert.IsNotNull(existingToDosNetwork.FirstOrDefault(e => e.ID == toDos[1].ID && e.Name == toDos[1].Name && e.Details == toDos[1].Details && e.Value == toDos[1].Value));
            Assert.AreEqual(2, existingToDosNetwork.Count);
        }

        [TestMethod]
        public async Task TestSaveMultiInsertNewItemsExistingItemsConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient("5");

            if (MockData)
            {
                MockResponses(10);
            }

            // Arrange
            var user = await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var autoToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);
            var syncToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC, kinveyClient);
            var networkToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK, kinveyClient);

            var toDos = new List<ToDo>();

            var toDo1 = new ToDo { Name = "Name1", Details = "Details1", Value = 1 };
            toDos.Add(toDo1);

            var toDo2 = new ToDo { Name = "Name2", Details = "Details2", Value = 2 };
            toDo2 = await autoToDoStore.SaveAsync(toDo2);
            toDo2.Name = "Name22";
            toDo2.Details = "Details22";
            toDo2.Value = 22;
            toDos.Add(toDo2);

            var toDo3 = new ToDo { Name = "Name3", Details = "Details3", Value = 3 };
            toDos.Add(toDo3);

            var toDo4 = new ToDo { ID = Guid.NewGuid().ToString(), Name = "Name4", Details = "Details4", Value = 4 };
            toDos.Add(toDo4);

            // Act
            var savedToDos = await autoToDoStore.SaveAsync(toDos);

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
            var existingToDosLocal = await syncToDoStore.FindAsync();
            var existingToDosNetwork = await networkToDoStore.FindAsync();

            // Teardown
            await autoToDoStore.RemoveAsync(savedToDos.Entities[0].ID);
            await autoToDoStore.RemoveAsync(savedToDos.Entities[1].ID);
            await autoToDoStore.RemoveAsync(savedToDos.Entities[2].ID);
            await autoToDoStore.RemoveAsync(savedToDos.Entities[3].ID);

            // Assert
            Assert.AreEqual(4, savedToDos.Entities.Count);
            Assert.AreEqual(0, savedToDos.Errors.Count);
            Assert.AreEqual(toDos[0].Name, savedToDos.Entities[0].Name);
            Assert.AreEqual(toDos[0].Details, savedToDos.Entities[0].Details);
            Assert.AreEqual(toDos[0].Value, savedToDos.Entities[0].Value);
            Assert.AreEqual(user.Id, savedToDos.Entities[0].Acl.Creator);
            Assert.AreEqual(toDos[1].ID, savedToDos.Entities[1].ID);
            Assert.AreEqual(toDos[1].Name, savedToDos.Entities[1].Name);
            Assert.AreEqual(toDos[1].Details, savedToDos.Entities[1].Details);
            Assert.AreEqual(toDos[1].Value, savedToDos.Entities[1].Value);
            Assert.AreEqual(user.Id, savedToDos.Entities[1].Acl.Creator);            
            Assert.AreEqual(toDos[2].Name, savedToDos.Entities[2].Name);
            Assert.AreEqual(toDos[2].Details, savedToDos.Entities[2].Details);
            Assert.AreEqual(toDos[2].Value, savedToDos.Entities[2].Value);
            Assert.AreEqual(user.Id, savedToDos.Entities[2].Acl.Creator);
            Assert.AreEqual(toDos[3].ID, savedToDos.Entities[3].ID);
            Assert.AreEqual(toDos[3].Name, savedToDos.Entities[3].Name);
            Assert.AreEqual(toDos[3].Details, savedToDos.Entities[3].Details);
            Assert.AreEqual(toDos[3].Value, savedToDos.Entities[3].Value);
            Assert.AreEqual(user.Id, savedToDos.Entities[3].Acl.Creator);
            Assert.AreEqual(0, pendingWriteActions.Count);
            Assert.AreEqual(4, existingToDosLocal.Count);
            Assert.IsNotNull(existingToDosLocal.FirstOrDefault(e => e.Name == toDos[0].Name && e.Details == toDos[0].Details && e.Value == toDos[0].Value && e.Kmd != null && e.Acl != null && !string.IsNullOrEmpty(e.Kmd.entityCreationTime) && !string.IsNullOrEmpty(e.Kmd.lastModifiedTime)));
            Assert.IsNotNull(existingToDosLocal.FirstOrDefault(e => e.ID == toDos[1].ID && e.Name == toDos[1].Name && e.Details == toDos[1].Details && e.Value == toDos[1].Value && e.Kmd != null && e.Acl != null && !string.IsNullOrEmpty(e.Kmd.entityCreationTime) && !string.IsNullOrEmpty(e.Kmd.lastModifiedTime)));
            Assert.IsNotNull(existingToDosLocal.FirstOrDefault(e => e.Name == toDos[2].Name && e.Details == toDos[2].Details && e.Value == toDos[2].Value && e.Kmd != null && e.Acl != null && !string.IsNullOrEmpty(e.Kmd.entityCreationTime) && !string.IsNullOrEmpty(e.Kmd.lastModifiedTime)));
            Assert.IsNotNull(existingToDosLocal.FirstOrDefault(e => e.ID == toDos[3].ID && e.Name == toDos[3].Name && e.Details == toDos[3].Details && e.Value == toDos[3].Value && e.Kmd != null && e.Acl != null && !string.IsNullOrEmpty(e.Kmd.entityCreationTime) && !string.IsNullOrEmpty(e.Kmd.lastModifiedTime)));

            Assert.AreEqual(4, existingToDosNetwork.Count);
            Assert.IsNotNull(existingToDosNetwork.FirstOrDefault(e => e.Name == toDos[0].Name && e.Details == toDos[0].Details && e.Value == toDos[0].Value));
            Assert.IsNotNull(existingToDosNetwork.FirstOrDefault(e => e.ID == toDos[1].ID && e.Name == toDos[1].Name && e.Details == toDos[1].Details && e.Value == toDos[1].Value));
            Assert.IsNotNull(existingToDosNetwork.FirstOrDefault(e => e.Name == toDos[2].Name && e.Details == toDos[2].Details && e.Value == toDos[2].Value ));
            Assert.IsNotNull(existingToDosNetwork.FirstOrDefault(e => e.ID == toDos[3].ID && e.Name == toDos[3].Name && e.Details == toDos[3].Details && e.Value == toDos[3].Value));
        }

        [TestMethod]
        public async Task TestSaveMultiInsertNewItemsExistingItemsConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient("5");

            if (MockData)
            {
                MockResponses(3);
            }

            // Arrange
            var user = await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var autoToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);
            var syncToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC, kinveyClient);

            var toDos = new List<ToDo>
            {
                new ToDo { Name = "Name1", Details = "Details1", Value = 1 },
                new ToDo { Name = "Name2", Details = "Details2", Value = 2 }
            };

            var toDo1 = new ToDo { Name = "Name3", Details = "Details3", Value = 3 };
            toDo1 = await autoToDoStore.SaveAsync(toDo1);
            toDo1.Name = "Name33";
            toDo1.Details = "Details33";
            toDo1.Value = 33;
            toDos.Add(toDo1);

            var toDo2 = new ToDo { ID = Guid.NewGuid().ToString(), Name = "Name4", Details = "Details4", Value = 4 };
            toDos.Add(toDo2);

            // Act
            SetRootUrlToKinveyClient(unreachableUrl);

            var savedToDos = await autoToDoStore.SaveAsync(toDos);

            SetRootUrlToKinveyClient(kinveyUrl);

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
            var existingToDos = await syncToDoStore.FindAsync();

            // Teardown
            await syncToDoStore.RemoveAsync(savedToDos.Entities[0].ID);
            await syncToDoStore.RemoveAsync(savedToDos.Entities[1].ID);
            await autoToDoStore.RemoveAsync(savedToDos.Entities[2].ID);
            await syncToDoStore.RemoveAsync(savedToDos.Entities[3].ID);

            // Assert
            Assert.AreEqual(4, savedToDos.Entities.Count);
            Assert.AreEqual(0, savedToDos.Errors.Count);
            Assert.AreEqual(toDos[0].Name, savedToDos.Entities[0].Name);
            Assert.AreEqual(toDos[0].Details, savedToDos.Entities[0].Details);
            Assert.AreEqual(toDos[0].Value, savedToDos.Entities[0].Value);
            Assert.IsNull(savedToDos.Entities[0].Acl);
            Assert.AreEqual(toDos[1].Name, savedToDos.Entities[1].Name);
            Assert.AreEqual(toDos[1].Details, savedToDos.Entities[1].Details);
            Assert.AreEqual(toDos[1].Value, savedToDos.Entities[1].Value);
            Assert.IsNull(savedToDos.Entities[1].Acl);
            Assert.AreEqual(toDos[2].ID, savedToDos.Entities[2].ID);
            Assert.AreEqual(toDos[2].Name, savedToDos.Entities[2].Name);
            Assert.AreEqual(toDos[2].Details, savedToDos.Entities[2].Details);
            Assert.AreEqual(toDos[2].Value, savedToDos.Entities[2].Value);
            Assert.AreEqual(user.Id, savedToDos.Entities[2].Acl.Creator);
            Assert.AreEqual(toDos[3].ID, savedToDos.Entities[3].ID);
            Assert.AreEqual(toDos[3].Name, savedToDos.Entities[3].Name);
            Assert.AreEqual(toDos[3].Details, savedToDos.Entities[3].Details);
            Assert.AreEqual(toDos[3].Value, savedToDos.Entities[3].Value);
            Assert.IsNull(savedToDos.Entities[3].Acl);
            Assert.AreEqual(4, pendingWriteActions.Count);
            var pendingWriteAction1 = pendingWriteActions.FirstOrDefault(e => e.entityId == savedToDos.Entities[0].ID);
            Assert.IsNotNull(pendingWriteAction1);
            Assert.AreEqual("POST", pendingWriteAction1.action);
            var pendingWriteAction2 = pendingWriteActions.FirstOrDefault(e => e.entityId == savedToDos.Entities[1].ID);
            Assert.IsNotNull(pendingWriteAction2);
            Assert.AreEqual("POST", pendingWriteAction2.action);
            var pendingWriteAction3 = pendingWriteActions.FirstOrDefault(e => e.entityId == savedToDos.Entities[2].ID);
            Assert.IsNotNull(pendingWriteAction3);
            Assert.AreEqual("PUT", pendingWriteAction3.action);
            var pendingWriteAction4 = pendingWriteActions.FirstOrDefault(e => e.entityId == savedToDos.Entities[3].ID);
            Assert.IsNotNull(pendingWriteAction4);
            Assert.AreEqual("PUT", pendingWriteAction4.action);
            Assert.AreEqual(4, existingToDos.Count);
            Assert.IsNotNull(existingToDos.FirstOrDefault(e => e.ID == savedToDos.Entities[0].ID));
            Assert.IsNotNull(existingToDos.FirstOrDefault(e => e.ID == savedToDos.Entities[1].ID));
            Assert.IsNotNull(existingToDos.FirstOrDefault(e => e.ID == savedToDos.Entities[2].ID));
            Assert.IsNotNull(existingToDos.FirstOrDefault(e => e.ID == savedToDos.Entities[3].ID));
        }

        [TestMethod]
        public async Task TestSaveMultiInsertEmptyArrayAsync()
        {
            // Setup
            kinveyClient = BuildClient("5");

            if (MockData)
            {
                MockResponses(1);
            }

            // Arrange
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var todoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);

            // Act
            var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                await todoStore.SaveAsync(new List<ToDo>());
            });

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();          

            // Assert
            Assert.AreEqual(typeof(KinveyException), exception.GetType());
            var kinveyException = exception as KinveyException;
            Assert.AreEqual(EnumErrorCategory.ERROR_GENERAL, kinveyException.ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_DATASTORE_EMPTY_ARRAY_OF_ENTITIES, kinveyException.ErrorCode);
            Assert.AreEqual(0, pendingWriteActions.Count);
        }

        [TestMethod]
        public async Task TestSaveMultiInsertInvalidPermissionsAsync()
        {
            // Setup
            kinveyClient = BuildClient("5");

            if (MockData)
            {
                MockResponses(2);
            }

            // Arrange
            await User.LoginAsync(TestSetup.user_without_permissions, TestSetup.pass_for_user_without_permissions, kinveyClient);

            var todoSyncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC, kinveyClient);
            var todoAutoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);

            var toDos = new List<ToDo>
            {
                new ToDo { Name = "Name1", Details = "Details1", Value = 1 },
                new ToDo { Name = "Name2", Details = "Details2", Value = 2 }
            };

            // Act
            var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                await todoAutoStore.SaveAsync(toDos);
            });


            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
            var existingToDos = await todoSyncStore.FindAsync();

            //Teardown
            await todoSyncStore.RemoveAsync(todoSyncStore.Where(e => e.Name.StartsWith("Name")));

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.GetType(), typeof(KinveyException));
            var ke = exception as KinveyException;
            Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, ke.ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, ke.ErrorCode);
            Assert.AreEqual(401, ke.StatusCode);

            Assert.IsNotNull(existingToDos);
            Assert.AreEqual(2, existingToDos.Count);
            Assert.IsNotNull(existingToDos.FirstOrDefault(e => e.Name.Equals(toDos[0].Name) && e.Details.Equals(toDos[0].Details) && e.Value == toDos[0].Value && e.Acl == null && e.Kmd == null));
            Assert.IsNotNull(existingToDos.FirstOrDefault(e => e.Name.Equals(toDos[1].Name) && e.Details.Equals(toDos[1].Details) && e.Value == toDos[1].Value && e.Acl == null && e.Kmd == null));

            Assert.IsNotNull(pendingWriteActions);
            Assert.AreEqual(2, pendingWriteActions.Count);
            Assert.IsNotNull(pendingWriteActions.FirstOrDefault(e=> e.entityId == existingToDos[0].ID && e.collection.Equals(toDosCollection) && e.action.Equals("POST")));
            Assert.IsNotNull(pendingWriteActions.FirstOrDefault(e => e.entityId == existingToDos[1].ID && e.collection.Equals(toDosCollection) && e.action.Equals("POST")));
        }

        [TestMethod]
        public async Task TestSaveMultiInsertCountLimitConnectionAvailableAsync()
        {
            // Setup
            const int countOfEntities = 100;

            kinveyClient = BuildClient("5");

            if (MockData)
            {
                MockResponses(3 + (countOfEntities / Constants.NUMBER_LIMIT_OF_ENTITIES));
            }

            // Arrange
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var todoStoreAuto = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);

            var toDos = new List<ToDo>();

            for (var index = 0; index < countOfEntities; index++)
            {
                toDos.Add(new ToDo { Name = "Name" + index.ToString(), Details = "Details" + index.ToString(), Value = 0 });
            }

            // Act
            var savedToDos = await todoStoreAuto.SaveAsync(toDos);

            var existingToDos = await todoStoreAuto.FindAsync();

            //Teardown
            await todoStoreAuto.RemoveAsync(todoStoreAuto.Where(e => e.Name.StartsWith("Name")));

            // Assert
            Assert.AreEqual(countOfEntities, savedToDos.Entities.Count);
            Assert.AreEqual(0, savedToDos.Errors.Count);
            Assert.AreEqual(countOfEntities, existingToDos.Count);
        }

        [TestMethod]
        public async Task TestSaveMultiInsertCountLimitConnectionIssueAsync()
        {
            // Setup
            const int countOfEntities = 100;

            kinveyClient = BuildClient("5");

            if (MockData)
            {
                MockResponses(2);
            }

            // Arrange
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var todoStoreAuto = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);
            var todoStoreNetwork = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK, kinveyClient);
            var todoStoreSync = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC, kinveyClient);

            var toDos = new List<ToDo>();

            for (var index = 0; index < countOfEntities; index++)
            {
                toDos.Add(new ToDo { Name = "Name" + index.ToString(), Details = "Details" + index.ToString(), Value = 0 });
            }

            // Act
            SetRootUrlToKinveyClient(unreachableUrl);
            var savedToDos = await todoStoreAuto.SaveAsync(toDos);
            SetRootUrlToKinveyClient(kinveyUrl);

            var existingToDosSync = await todoStoreSync.FindAsync();
            var existingToDosNetwork = await todoStoreNetwork.FindAsync();            

            //Teardown
            await todoStoreSync.RemoveAsync(todoStoreAuto.Where(e => e.Name.StartsWith("Name")));

            // Assert
            Assert.AreEqual(countOfEntities, savedToDos.Entities.Count);
            Assert.AreEqual(0, savedToDos.Errors.Count);
            Assert.AreEqual(0, existingToDosNetwork.Count);
            Assert.AreEqual(countOfEntities, existingToDosSync.Count);
        }

        [TestMethod]
        public async Task TestSaveMultiInsertIncorrectKinveyApiVersionAsync()
        {
            // Setup
            kinveyClient = BuildClient("4");

            if (MockData)
            {
                MockResponses(1);
            }

            // Arrange
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var todoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);

            var toDos = new List<ToDo>
            {
                new ToDo { Name = "Name1", Details = "Details1", Value = 1 },
                new ToDo { Name = "Name2", Details = "Details2", Value = 2 }
            };

            // Act
            var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                await todoStore.SaveAsync(toDos);
            });

            // Assert
            Assert.AreEqual(typeof(KinveyException), exception.GetType());
            var kinveyException = exception as KinveyException;
            Assert.AreEqual(EnumErrorCategory.ERROR_GENERAL, kinveyException.ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_DATASTORE_NOT_COMPATIBLE_KINVEY_API_VERSION, kinveyException.ErrorCode);
        }

        [TestMethod]
        public async Task TestSaveMultiInsertNetworkStoreWithErrorsAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient("5");

                MockResponses(2);

                // Arrange
                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                var todoStoreNetwork = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);

                var toDos = new List<ToDo>
                {
                    new ToDo { Name = TestSetup.entity_name_for_400_response_error, Details = "Details1", Value = 1 },
                    new ToDo { Name = TestSetup.entity_name_for_400_response_error, Details = "Details3", Value = 3 }
                };

                // Act
                var response = await todoStoreNetwork.SaveAsync(toDos);

                // Assert
                Assert.AreEqual(toDos.Count, response.Entities.Count);
                Assert.IsTrue(response.Entities.All(e => e == null));
                Assert.AreEqual(toDos.Count, response.Errors.Count);
                Assert.AreEqual(response.Errors[0].Errmsg, TestSetup.entity_name_for_400_response_error);
            }
        }

        [TestMethod]
        public async Task TestSaveMultiInsertNewItemsWithErrorsConnectionAvailableAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient("5");

                MockResponses(4);

                // Arrange
                var user = await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                var autoToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);
                var syncToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC, kinveyClient);
                var networkToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK, kinveyClient);

                var toDos = new List<ToDo>
                {
                    new ToDo { Name = "Name1", Details = "Details1", Value = 1 },
                    new ToDo { Name = "Name2", Details = "Details2", Value = 2,  GeoLoc = "[200,200]" }
                };

                // Act
                var savedToDos = await autoToDoStore.SaveAsync(toDos);

                var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
                var existingToDosCache = await syncToDoStore.FindAsync();
                var existingToDosNetwork = await networkToDoStore.FindAsync();

                // Teardown
                await autoToDoStore.RemoveAsync(savedToDos.Entities[0].ID);

                // Assert
                Assert.AreEqual(2, savedToDos.Entities.Count);
                Assert.AreEqual(1, savedToDos.Errors.Count);
                Assert.AreEqual(toDos[0].Name, savedToDos.Entities[0].Name);
                Assert.AreEqual(toDos[0].Details, savedToDos.Entities[0].Details);
                Assert.AreEqual(toDos[0].Value, savedToDos.Entities[0].Value);
                Assert.AreEqual(user.Id, savedToDos.Entities[0].Acl.Creator);
                Assert.IsNull(savedToDos.Entities[1]);
                Assert.AreEqual(1, savedToDos.Errors[0].Index);

                var existingToDo1 = existingToDosCache.FirstOrDefault(e => e.Name == toDos[1].Name && e.Details == toDos[1].Details && e.Value == toDos[1].Value);

                Assert.AreEqual(1, pendingWriteActions.Count);
                var pendingWriteAction1 = pendingWriteActions.FirstOrDefault(e => e.entityId == existingToDo1.ID);
                Assert.IsNotNull(pendingWriteAction1);
                Assert.AreEqual("POST", pendingWriteAction1.action);

                Assert.AreEqual(2, existingToDosCache.Count);
                Assert.IsNotNull(existingToDosCache.FirstOrDefault(e => e.ID == savedToDos.Entities[0].ID && savedToDos.Entities[0].Kmd != null && savedToDos.Entities[0].Acl != null && !string.IsNullOrEmpty(e.Kmd.entityCreationTime) && !string.IsNullOrEmpty(e.Kmd.lastModifiedTime)));
                Assert.IsNotNull(existingToDosCache.FirstOrDefault(e => e.ID == existingToDo1.ID && existingToDo1.Kmd == null && existingToDo1.Acl == null));

                Assert.AreEqual(1, existingToDosNetwork.Count);
                Assert.IsNotNull(existingToDosNetwork.FirstOrDefault(e => e.ID == savedToDos.Entities[0].ID));
            }
        }

        [TestMethod]
        public async Task TestSaveMultiInsertGeolocationErrorsConnectionAvailableAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient("5");

                MockResponses(6);

                // Arrange
                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                var toDoAutoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);
                var syncToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC, kinveyClient);

                var toDos = new List<ToDo>
                {
                    new ToDo { Name = "Name1", Details = "Details1", Value = 1, GeoLoc = "[200,200]" },
                    new ToDo { ID = Guid.NewGuid().ToString(), Name = "Name2", Details = "Details2", Value = 2, },
                    new ToDo { ID = Guid.NewGuid().ToString(), Name = "Name3", Details = "Details3", Value = 3, GeoLoc = "[200,200]" },
                    new ToDo { Name = "Name4", Details = "Details4", Value = 1 }
                };

                // Act
                var savedToDos = await toDoAutoStore.SaveAsync(toDos);

                var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
                var existingToDos = await syncToDoStore.FindAsync();

                // Teardown
                await toDoAutoStore.RemoveAsync(savedToDos.Entities[1].ID);
                await toDoAutoStore.RemoveAsync(savedToDos.Entities[3].ID);

                // Assert
                Assert.AreEqual(4, savedToDos.Entities.Count);
                Assert.AreEqual(2, savedToDos.Errors.Count);
                Assert.IsNull(savedToDos.Entities[0]);
                Assert.IsNull(savedToDos.Entities[2]);
                Assert.IsNotNull(savedToDos.Entities[1]);
                Assert.IsNotNull(savedToDos.Entities[3]);
                Assert.AreEqual(toDos[1].ID, savedToDos.Entities[1].ID);
                Assert.AreEqual(toDos[1].Name, savedToDos.Entities[1].Name);
                Assert.AreEqual(toDos[1].Details, savedToDos.Entities[1].Details);
                Assert.AreEqual(toDos[1].Value, savedToDos.Entities[1].Value);
                Assert.AreEqual(toDos[3].Name, savedToDos.Entities[3].Name);
                Assert.AreEqual(toDos[3].Details, savedToDos.Entities[3].Details);
                Assert.AreEqual(toDos[3].Value, savedToDos.Entities[3].Value);
                Assert.AreEqual(0, savedToDos.Errors[0].Index);
                Assert.AreEqual(2, savedToDos.Errors[1].Index);

                var existingToDo1 = existingToDos.Find(e=> e.Name.Equals(toDos[0].Name) && e.Details.Equals(toDos[0].Details) && e.Value == toDos[0].Value);

                Assert.AreEqual(2, pendingWriteActions.Count);
                var pendingWriteAction1 = pendingWriteActions.FirstOrDefault(e => e.entityId == existingToDo1.ID);
                Assert.IsNotNull(pendingWriteAction1);
                Assert.AreEqual("POST", pendingWriteAction1.action);
                var pendingWriteAction2 = pendingWriteActions.FirstOrDefault(e => e.entityId == toDos[2].ID);
                Assert.IsNotNull(pendingWriteAction2);
                Assert.AreEqual("PUT", pendingWriteAction2.action);
                
                Assert.AreEqual(4, existingToDos.Count);
                Assert.IsNotNull(existingToDos.FirstOrDefault(e => e.ID == existingToDo1.ID));
                Assert.IsNotNull(existingToDos.FirstOrDefault(e => e.ID == savedToDos.Entities[1].ID));
                Assert.IsNotNull(existingToDos.FirstOrDefault(e => e.ID == toDos[2].ID));                
                Assert.IsNotNull(existingToDos.FirstOrDefault(e => e.ID == savedToDos.Entities[3].ID));
            }
        }

        [TestMethod]
        public async Task TestSaveMultiInsertTestSetupEntityErrorsConnectionAvailableAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient("5");

                MockResponses(8);

                // Arrange
                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                var toDoAutoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);
                var syncToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC, kinveyClient);

                var toDos = new List<ToDo>();

                var toDo1 = new ToDo { Name = "Name3", Details = "Details3", Value = 3 };
                toDo1 = await toDoAutoStore.SaveAsync(toDo1);
                toDo1.Name = TestSetup.entity_name_for_400_response_error;
                toDo1.Details = "Details33";
                toDo1.Value = 33;

                toDos.Add(toDo1);
                toDos.Add(new ToDo { Name = TestSetup.entity_name_for_400_response_error, Details = "Details1", Value = 1 });
                toDos.Add(toDo1);
                toDos.Add(new ToDo { Name = "Name2", Details = "Details2", Value = 2 });
                toDos.Add(toDo1);
                toDos.Add(new ToDo { Name = TestSetup.entity_name_for_400_response_error, Details = "Details3", Value = 3 });
                toDos.Add(toDo1);

                // Act
                var savedToDos = await toDoAutoStore.SaveAsync(toDos);

                var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
                var existingToDos = await syncToDoStore.FindAsync();

                // Teardown
                await toDoAutoStore.RemoveAsync(savedToDos.Entities[3].ID);

                // Assert
                Assert.AreEqual(7, savedToDos.Entities.Count);
                Assert.AreEqual(6, savedToDos.Errors.Count);
                Assert.IsNull(savedToDos.Entities[0]);
                Assert.IsNull(savedToDos.Entities[1]);
                Assert.IsNull(savedToDos.Entities[2]);
                Assert.IsNotNull(savedToDos.Entities[3]);
                Assert.IsNotNull(savedToDos.Entities[3].Acl);
                Assert.IsNull(savedToDos.Entities[4]);
                Assert.IsNull(savedToDos.Entities[5]);
                Assert.IsNull(savedToDos.Entities[6]);
                Assert.AreEqual(toDos[3].Name, savedToDos.Entities[3].Name);
                Assert.AreEqual(toDos[3].Details, savedToDos.Entities[3].Details);
                Assert.AreEqual(toDos[3].Value, savedToDos.Entities[3].Value);
                Assert.AreEqual(3, pendingWriteActions.Count);
                Assert.AreEqual(0, savedToDos.Errors[0].Index);
                Assert.AreEqual(1, savedToDos.Errors[1].Index);
                Assert.AreEqual(2, savedToDos.Errors[2].Index);
                Assert.AreEqual(4, savedToDos.Errors[3].Index);
                Assert.AreEqual(5, savedToDos.Errors[4].Index);
                Assert.AreEqual(6, savedToDos.Errors[5].Index);
                Assert.AreEqual(4, existingToDos.Count);
                Assert.IsNotNull(existingToDos.FirstOrDefault(e => e.ID == savedToDos.Entities[3].ID));
            }
        }

        [TestMethod]
        public async Task TestSaveMultiInsertAutoStoreWithErrorsAsync()
        {
            // Setup
            kinveyClient = BuildClient("5");

            if (MockData)
            {
                MockResponses(1);
            }

            // Arrange
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var todoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);

            var toDos = new List<ToDo>
            {
                null, null
            };

            // Act
            var response = await todoStore.SaveAsync(toDos);

            // Assert
            Assert.AreEqual(toDos.Count, response.Entities.Count);
            Assert.IsTrue(response.Entities.All(e => e == null));
            Assert.AreEqual(toDos.Count, response.Errors.Count);
            Assert.AreEqual("Value cannot be null. (Parameter 'o')", response.Errors[0].Errmsg);
        }

        [TestMethod]
        public async Task TestSaveMultiInsert2kCountAsync()
        {
            // Setup
            kinveyClient = BuildClient("5");
            uint countToUpdate = 21;

            var countToAdd = 20 * Constants.NUMBER_LIMIT_OF_ENTITIES + 1;

            if (MockData)
            {
                MockResponses(20 + 2 * countToUpdate + 3);
            }

            // Arrange
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var todoNetworkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK, kinveyClient);
            var todoAutoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);
            var todoSyncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC, kinveyClient);

            var toDosToAdd = new List<ToDo>();
            for (var index = 0; index < countToAdd; index++)
            {
                var toDo = new ToDo { Name = "Name" + index.ToString(), Details = "Details" + index.ToString(), Value = index };

                if (index % 100 == 0)
                {
                    toDo = await todoAutoStore.SaveAsync(toDo);
                    toDo.Name = string.Concat(toDo.Name, "updated");
                    toDo.Details = string.Concat(toDo.Details, "updated");
                }

                toDosToAdd.Add(toDo);
            }

            // Act
            var savedToDosToAdd = await todoAutoStore.SaveAsync(toDosToAdd);

            var existingToDosNetwork = await todoNetworkStore.FindAsync();
            var existingToDosSync = await todoSyncStore.FindAsync();

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();

            // Teardown
            await todoAutoStore.RemoveAsync(todoAutoStore.Where(e => e.Name.StartsWith("Name")));

            // Assert
            Assert.AreEqual(toDosToAdd.Count, savedToDosToAdd.Entities.Count);
            Assert.AreEqual(0, savedToDosToAdd.Errors.Count);

            for (var index = 0; index < toDosToAdd.Count; index++)
            {
                Assert.AreEqual(toDosToAdd[index].Name, savedToDosToAdd.Entities[index].Name);
                Assert.AreEqual(toDosToAdd[index].Details, savedToDosToAdd.Entities[index].Details);
                Assert.AreEqual(toDosToAdd[index].Value, savedToDosToAdd.Entities[index].Value);
            }

            Assert.AreEqual(toDosToAdd.Count, existingToDosNetwork.Count);

            foreach (var toDo in toDosToAdd)
            {
                Assert.IsNotNull(existingToDosNetwork.Find(e => e.Name.Equals(toDo.Name) && e.Details.Equals(toDo.Details) &&
                e.Value.Equals(toDo.Value)));
            }

            Assert.AreEqual(toDosToAdd.Count, existingToDosSync.Count);
            Assert.IsTrue(existingToDosSync.Any(e => e.Acl != null && e.Kmd != null));
            Assert.IsTrue(existingToDosSync.Any(e => !e.ID.StartsWith("temp")));

            Assert.AreEqual(0, pendingWriteActions.Count);
        }

        [TestMethod]
        public async Task TestSaveMultiInsert2kCountWithErrorsAsync()
        {
            // Setup
            if (MockData)
            {
                kinveyClient = BuildClient("5");
                uint countToUpdate = 21;

                var countToAdd = 20 * Constants.NUMBER_LIMIT_OF_ENTITIES + 1;

                if (MockData)
                {
                    MockResponses(20 + 2 * countToUpdate + 3);
                }

                // Arrange
                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                var todoNetworkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK, kinveyClient);
                var todoAutoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);
                var todoSyncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC, kinveyClient);

                var toDosToAdd = new List<ToDo>();
                var toDosAdded = new List<ToDo>();
                var updateCount = 0;
                var errorCount = 0;
                var errorAddCount = 0;
                var errorUpdateCount = 0;
                var successCount = 0;

                for (var index = 0; index < countToAdd; index++)
                {
                    var toDo = new ToDo { Name = "Name" + index.ToString(), Details = "Details" + index.ToString(), Value = index };

                    if (index % 100 == 0)
                    {
                        toDo = await todoAutoStore.SaveAsync(toDo);
                        successCount++;
                        toDo.Name = string.Concat(toDo.Name, "updated");
                        toDo.Details = string.Concat(toDo.Details, "updated");

                        if (updateCount % 2 != 0)
                        {
                            toDo.GeoLoc = "[200,200]";
                            errorUpdateCount++;
                        }
                        else
                        {
                            toDosAdded.Add(toDo);
                        }

                        updateCount++;
                    }
                    else
                    {
                        if (index % 2 != 0)
                        {
                            toDo.GeoLoc = "[200,200]";
                            errorAddCount++;
                        }
                        else
                        {
                            toDosAdded.Add(toDo);
                            successCount++;
                        }
                    }

                    toDosToAdd.Add(toDo);
                }

                errorCount = errorAddCount + errorUpdateCount;

                // Act
                var savedToDosToAdd = await todoAutoStore.SaveAsync(toDosToAdd);

                var existingToDosNetwork = await todoNetworkStore.FindAsync();
                var existingToDosSync = await todoSyncStore.FindAsync();

                var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();

                // Teardown
                await todoAutoStore.RemoveAsync(todoAutoStore.Where(e => e.Name.StartsWith("Name")));

                // Assert
                Assert.AreEqual(toDosToAdd.Count, savedToDosToAdd.Entities.Count);
                Assert.AreEqual(errorCount, savedToDosToAdd.Errors.Count);

                for (var index = 0; index < toDosToAdd.Count; index++)
                {
                    if (savedToDosToAdd.Entities[index] != null)
                    {
                        Assert.AreEqual(toDosToAdd[index].Name, savedToDosToAdd.Entities[index].Name);
                        Assert.AreEqual(toDosToAdd[index].Details, savedToDosToAdd.Entities[index].Details);
                        Assert.AreEqual(toDosToAdd[index].Value, savedToDosToAdd.Entities[index].Value);
                    }
                    else
                    {
                        Assert.IsNotNull(savedToDosToAdd.Errors.Find(e => e.Index == index));
                    }
                }

                Assert.AreEqual(successCount, existingToDosNetwork.Count);

                foreach (var toDo in toDosAdded)
                {
                    Assert.IsNotNull(existingToDosNetwork.Find(e => e.Name.Equals(toDo.Name) && e.Details.Equals(toDo.Details) &&
                    e.Value.Equals(toDo.Value)));
                }

                Assert.AreEqual(toDosToAdd.Count, existingToDosSync.Count);
                Assert.AreEqual(errorAddCount, existingToDosSync.Count(e => e.Acl == null && e.Kmd == null));
                Assert.AreEqual(toDosToAdd.Count - errorAddCount, existingToDosSync.Count(e => !e.ID.StartsWith("temp")));

                Assert.AreEqual(errorCount, pendingWriteActions.Count);
            }
        }

        [TestMethod]
        public async Task TestSaveMultiInsert201CountWithErrorsAsync()
        {
            // Setup
            kinveyClient = BuildClient("5");
        
            if (MockData)
            {
                MockResponses(6);
            }

            // Arrange
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var todoNetworkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK, kinveyClient);
            var todoAutoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);
            var todoSyncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC, kinveyClient);

            var toDosToAdd = new List<ToDo>();
            var toDosAdded = new List<ToDo>();
            var successCount = 0;
            var errorAddCount = 0;

            for (var index = 0; index < 201; index++)
            {
                ToDo toDo = null;

                if (index > 99 && index < 200)
                {
                    toDo = new ToDo { Name = "Name" + index.ToString(), Details = "Details" + index.ToString(), Value = index, GeoLoc = "[200,200]" };
                    errorAddCount++;
                }
                else
                {
                    toDo = new ToDo { Name = "Name" + index.ToString(), Details = "Details" + index.ToString(), Value = index };
                    successCount++;
                    toDosAdded.Add(toDo);
                }

                toDosToAdd.Add(toDo);
            }

            // Act
            var savedToDosToAdd = await todoAutoStore.SaveAsync(toDosToAdd);

            var existingToDosNetwork = await todoNetworkStore.FindAsync();
            var existingToDosSync = await todoSyncStore.FindAsync();

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();

            // Teardown
            await todoAutoStore.RemoveAsync(todoAutoStore.Where(e => e.Name.StartsWith("Name")));

            // Assert
            Assert.AreEqual(toDosToAdd.Count, savedToDosToAdd.Entities.Count);
            Assert.AreEqual(errorAddCount, savedToDosToAdd.Errors.Count);

            for (var index = 0; index < toDosToAdd.Count; index++)
            {
                if (savedToDosToAdd.Entities[index] != null)
                {
                    Assert.AreEqual(toDosToAdd[index].Name, savedToDosToAdd.Entities[index].Name);
                    Assert.AreEqual(toDosToAdd[index].Details, savedToDosToAdd.Entities[index].Details);
                    Assert.AreEqual(toDosToAdd[index].Value, savedToDosToAdd.Entities[index].Value);
                }
                else
                {
                    Assert.IsNotNull(savedToDosToAdd.Errors.Find(e => e.Index == index));
                }
            }

            Assert.AreEqual(toDosAdded.Count, existingToDosNetwork.Count);

            foreach (var toDo in toDosAdded)
            {
                Assert.IsNotNull(existingToDosNetwork.Find(e => e.Name.Equals(toDo.Name) && e.Details.Equals(toDo.Details) &&
                e.Value.Equals(toDo.Value)));
            }

            Assert.AreEqual(toDosToAdd.Count, existingToDosSync.Count);
            Assert.AreEqual(errorAddCount, existingToDosSync.Count(e => e.Acl == null && e.Kmd == null));
            Assert.AreEqual(toDosToAdd.Count - errorAddCount, existingToDosSync.Count(e => !e.ID.StartsWith("temp")));
            Assert.AreEqual(errorAddCount, pendingWriteActions.Count);
        }


        #endregion Save

        #region Get count

        [TestMethod]
        public async Task TestGetCountAllItemsNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(9, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "Next Task1",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            var newItem2 = new ToDo
            {
                Name = "Next Task2",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            var newItem3 = new ToDo
            {
                Name = "Next Task3",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await networkStore.SaveAsync(newItem1);
            var savedItem2 = await networkStore.SaveAsync(newItem2);
          
            // Act
            var count1 = await autoStore.GetCountAsync();

            var savedItem3 = await networkStore.SaveAsync(newItem3);

            var count2 = await autoStore.GetCountAsync();

            // Teardown
            await networkStore.RemoveAsync(savedItem1.ID);
            await networkStore.RemoveAsync(savedItem2.ID);
            await networkStore.RemoveAsync(savedItem3.ID);

            // Assert
            Assert.IsNotNull(count1);
            Assert.IsNotNull(count2);
            Assert.AreEqual(2u, count1);
            Assert.AreEqual(3u, count2);
        }

        [TestMethod]
        public async Task TestGetCountByQueryNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "Next Task1",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            var newItem2 = new ToDo
            {
                Name = "Next Task2",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            var newItem3 = new ToDo
            {
                Name = "Next Task3",
                Details = "Details",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            var query = autoStore.Where(t => t.Details.StartsWith("A t"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await networkStore.SaveAsync(newItem1);
            var savedItem2 = await networkStore.SaveAsync(newItem2);
            var savedItem3 = await networkStore.SaveAsync(newItem3);

            // Act
            var count = await autoStore.GetCountAsync(query);

            // Teardown
            await networkStore.RemoveAsync(savedItem1.ID);
            await networkStore.RemoveAsync(savedItem2.ID);
            await networkStore.RemoveAsync(savedItem3.ID);

            // Assert
            Assert.IsNotNull(count);
            Assert.AreEqual(2u, count);
        }

        [TestMethod]
        public async Task TestGetCountInvalidQueryNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(5, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await networkStore.SaveAsync(newItem1);
            newItem2 = await networkStore.SaveAsync(newItem2);

            var query = autoStore.Where(x => true);

            // Act
            var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                await autoStore.GetCountAsync(query);
            });

            // Teardown
            await networkStore.RemoveAsync(newItem1.ID);
            await networkStore.RemoveAsync(newItem2.ID);

            // Assert
            Assert.IsTrue(exception.GetType() == typeof(KinveyException));
            KinveyException ke = exception as KinveyException;
            Assert.IsTrue(ke.ErrorCategory == EnumErrorCategory.ERROR_DATASTORE_NETWORK);
            Assert.IsTrue(ke.ErrorCode == EnumErrorCode.ERROR_LINQ_WHERE_CLAUSE_NOT_SUPPORTED);
        }

        [TestMethod]
        public async Task TestGetCountAllItemsNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);

            var newItem1 = new ToDo
            {
                Name = "Next Task1",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            var newItem2 = new ToDo
            {
                Name = "Next Task2",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            var newItem3 = new ToDo
            {
                Name = "Next Task3",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await networkStore.SaveAsync(newItem1);
            var savedItem2 = await networkStore.SaveAsync(newItem2);

            // Act
            var listAutoToDo = await autoStore.FindAsync();
           
            var savedItem3 = await networkStore.SaveAsync(newItem3);

            SetRootUrlToKinveyClient(unreachableUrl);
            var count = await autoStore.GetCountAsync();
            SetRootUrlToKinveyClient(kinveyUrl);
            
            // Teardown
            await networkStore.RemoveAsync(savedItem1.ID);
            await networkStore.RemoveAsync(savedItem2.ID);
            await networkStore.RemoveAsync(savedItem3.ID);

            // Assert
            Assert.IsNotNull(count);
            Assert.IsNotNull(listAutoToDo);
            Assert.AreEqual(2u, count);
            Assert.AreEqual(2, listAutoToDo.Count);
        }

        [TestMethod]
        public async Task TestGetCountInvalidPermissionsNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(2, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            await User.LoginAsync(TestSetup.user_without_permissions, TestSetup.pass_for_user_without_permissions, kinveyClient);

            // Act
            var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                await autoStore.GetCountAsync();
            });

            // Assert
            Assert.IsTrue(exception.GetType() == typeof(KinveyException));
            KinveyException ke = exception as KinveyException;
            Assert.IsTrue(ke.ErrorCategory == EnumErrorCategory.ERROR_BACKEND);
            Assert.IsTrue(ke.ErrorCode == EnumErrorCode.ERROR_JSON_RESPONSE);
        }

        [TestMethod]
        public async Task TestGetCountNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }
            
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "Next Task1",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            var newItem2 = new ToDo
            {
                Name = "Next Task2",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            var newItem3 = new ToDo
            {
                Name = "Next Task3",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            ToDo savedItem1 = await autoStore.SaveAsync(newItem1);
            ToDo savedItem2 = await autoStore.SaveAsync(newItem2);
            ToDo savedItem3 = await autoStore.SaveAsync(newItem3);

            // Act
            var networkResult = await autoStore.GetCountAsync();

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);
            await autoStore.RemoveAsync(savedItem2.ID);
            await autoStore.RemoveAsync(savedItem3.ID);

            // Assert
            Assert.IsNotNull(networkResult);
            Assert.AreEqual(3u, networkResult);
        }

        [TestMethod]
        public async Task TestGetCountNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(1, kinveyClient);
            }
                        
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Next Task1",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            var newItem2 = new ToDo
            {
                Name = "Next Task2",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            var newItem3 = new ToDo
            {
                Name = "Next Task3",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };
                      
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            SetRootUrlToKinveyClient(unreachableUrl);

            var savedItem1 = await syncStore.SaveAsync(newItem1);
            var savedItem2 = await syncStore.SaveAsync(newItem2);
            var savedItem3 = await syncStore.SaveAsync(newItem3);

            // Act
            var networkResult = await autoStore.GetCountAsync();

            // Teardown
            await syncStore.RemoveAsync(savedItem1.ID);
            await syncStore.RemoveAsync(savedItem2.ID);
            await syncStore.RemoveAsync(savedItem3.ID);

            // Assert
            Assert.IsNotNull(networkResult);
            Assert.AreEqual(3u, networkResult);
        }

        [TestMethod]
        public async Task TestGetCountWithQueryNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }
            
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "Next Task1",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            var newItem2 = new ToDo
            {
                Name = "Next Task2",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            var newItem3 = new ToDo
            {
                Name = "Next Task3",
                Details = "Details",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            var query = autoStore.Where(t => t.Details.StartsWith("A t"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var savedItem1 = await autoStore.SaveAsync(newItem1);
            var savedItem2 = await autoStore.SaveAsync(newItem2);
            var savedItem3 = await autoStore.SaveAsync(newItem3);
            
            // Act
            var networkResult = await autoStore.GetCountAsync(query);

            // Teardown
            await autoStore.RemoveAsync(savedItem1.ID);
            await autoStore.RemoveAsync(savedItem2.ID);
            await autoStore.RemoveAsync(savedItem3.ID);

            // Assert
            Assert.IsNotNull(networkResult);
            Assert.AreEqual(2u, networkResult);
        }

        [TestMethod]
        public async Task TestGetCountWithQueryNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(1, kinveyClient);
            }
           
            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "Next Task1",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            var newItem2 = new ToDo
            {
                Name = "Next Task2",
                Details = "A test",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            var newItem3 = new ToDo
            {
                Name = "Next Task3",
                Details = "Details",
                DueDate = "2016-04-19T20:02:17.635Z"
            };

            var query = autoStore.Where(t => t.Details.StartsWith("A t"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            SetRootUrlToKinveyClient(unreachableUrl);

            var savedItem1 = await syncStore.SaveAsync(newItem1);
            var savedItem2 = await syncStore.SaveAsync(newItem2);
            var savedItem3 = await syncStore.SaveAsync(newItem3);
            
            // Act
            var networkResult = await autoStore.GetCountAsync(query);

            // Teardown
            await syncStore.RemoveAsync(savedItem1.ID);
            await syncStore.RemoveAsync(savedItem2.ID);
            await syncStore.RemoveAsync(savedItem3.ID);

            // Assert
            Assert.IsNotNull(networkResult);
            Assert.AreEqual(2u, networkResult);
        }

        #endregion Get count

        #region Push

        #region Positive tests

        [TestMethod]
        public async Task TestPushCreatedDataNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(6, kinveyClient);
            }

            //Arrange
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await syncStore.SaveAsync(newItem1);
            newItem2 = await syncStore.SaveAsync(newItem2);

            // Act
            var pushedEntities = await autoStore.PushAsync();

            var networkEntities = await networkStore.FindAsync();

            var pendingWriteActionCount = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).Count(false);

            //Teardown
            await networkStore.RemoveAsync(networkEntities[0].ID);
            await networkStore.RemoveAsync(networkEntities[1].ID);

            // Assert
            Assert.IsNotNull(networkEntities);
            Assert.AreEqual(2, networkEntities.Count);
            Assert.AreEqual(0, pendingWriteActionCount);
        }

        [TestMethod]
        public async Task TestPushUpdatedDataNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }

            //Arrange
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await networkStore.SaveAsync(newItem1);
            newItem2 = await networkStore.SaveAsync(newItem2);

            // Act
            var pulledEntities = await autoStore.PullAsync();

            newItem1.Details = "New details";
            await syncStore.SaveAsync(newItem1);
            await autoStore.PushAsync();

            var networkEntities = await networkStore.FindAsync();

            var pendingWriteActionCount = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).Count(false);

            //Teardown
            await networkStore.RemoveAsync(newItem1.ID);
            await networkStore.RemoveAsync(newItem2.ID);

            // Assert
            Assert.IsNotNull(pulledEntities);
            Assert.IsNotNull(networkEntities);
            Assert.AreEqual(2, pulledEntities.PullCount);
            Assert.AreEqual(2, networkEntities.Count);
            Assert.AreEqual(newItem1.Details, networkEntities.FirstOrDefault(e=> e.ID == newItem1.ID).Details);
            Assert.AreEqual(newItem1.Name, networkEntities.FirstOrDefault(e => e.ID == newItem1.ID).Name);
            Assert.AreEqual(newItem2.Details, networkEntities.FirstOrDefault(e => e.ID == newItem2.ID).Details);
            Assert.AreEqual(newItem2.Name, networkEntities.FirstOrDefault(e => e.ID == newItem2.ID).Name);
            Assert.AreEqual(0, pendingWriteActionCount);
        }

        [TestMethod]
        public async Task TestPushDeletedDataNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(9, kinveyClient);
            }

            //Arrange
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem3 = new ToDo
            {
                Name = "todo3",
                Details = "details for 3 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await networkStore.SaveAsync(newItem1);
            newItem2 = await networkStore.SaveAsync(newItem2);
            newItem3 = await networkStore.SaveAsync(newItem3);

            // Act
            var pulledEntities = await autoStore.PullAsync();

            await syncStore.RemoveAsync(newItem1.ID);
            await syncStore.RemoveAsync(newItem2.ID);
            var pushedEntities = await autoStore.PushAsync();

            var networkEntities = await networkStore.FindAsync();

            var pendingWriteActionCount = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).Count(false);

            //Teardown
            await networkStore.RemoveAsync(newItem3.ID);

            // Assert
            Assert.IsNotNull(pulledEntities);
            Assert.IsNotNull(networkEntities);
            Assert.IsNotNull(pulledEntities);
            Assert.AreEqual(3, pulledEntities.PullCount);
            Assert.AreEqual(1, networkEntities.Count);
            Assert.AreEqual(2, pushedEntities.PushCount);
            Assert.AreEqual(newItem3.Name, networkEntities.FirstOrDefault(e => e.ID == newItem3.ID).Name);
            Assert.AreEqual(newItem3.Details, networkEntities.FirstOrDefault(e => e.ID == newItem3.ID).Details);           
            Assert.AreEqual(0, pendingWriteActionCount);
        }

        [TestMethod]
        public async Task TestPushCreatedDataNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(1, kinveyClient);
            }

            //Arrange
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await syncStore.SaveAsync(newItem1);
            newItem2 = await syncStore.SaveAsync(newItem2);

            // Act
            SetRootUrlToKinveyClient(unreachableUrl);

            Exception exception = null;
            try
            {
                var pushedEntities = await autoStore.PushAsync();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(2, pendingWriteActions.Count);
            Assert.AreEqual(typeof(AggregateException), exception.GetType());
            Assert.AreEqual("POST", pendingWriteActions[0].action);
            Assert.AreEqual("POST", pendingWriteActions[1].action);
        }

        [TestMethod]
        public async Task TestPushRecreatedDataNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(9, kinveyClient);
            }

            //Arrange
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await syncStore.SaveAsync(newItem1);
            newItem2 = await syncStore.SaveAsync(newItem2);

            // Act
            var pushedEntitiesCount1 = await autoStore.PushAsync();

            newItem1.Details = "Details";
            newItem1 = await syncStore.SaveAsync(newItem1);

            var networkEntities1 = await networkStore.FindAsync();
            var idToBeRemoved = networkEntities1.FirstOrDefault(e => e.Name.Equals("todo1")).ID;
            await networkStore.RemoveAsync(idToBeRemoved);

            var pushedEntities2 = await autoStore.PushAsync();

            var networkEntities2 = await networkStore.FindAsync();

            //Teardown
            await networkStore.RemoveAsync(networkEntities2[0].ID);
            await networkStore.RemoveAsync(networkEntities2[1].ID);

            // Assert
            Assert.AreEqual(1, pushedEntities2.PushCount);
            Assert.AreEqual(2, networkEntities2.Count);
            Assert.AreEqual(newItem1.Name, networkEntities2.FirstOrDefault(e => e.ID == newItem1.ID).Name);
            Assert.AreEqual(newItem1.Details, networkEntities2.FirstOrDefault(e=> e.ID == newItem1.ID).Details);
        }

        [TestMethod]
        public async Task TestPushNewItemsConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient("5");

            if (MockData)
            {
                MockResponses(5);
            }

            // Arrange
            var user = await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var autoToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);
            var syncToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC, kinveyClient);
            var networkToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK, kinveyClient);

            var toDos = new List<ToDo>
            {
                new ToDo { Name = "Name1", Details = "Details1", Value = 1 },
                new ToDo { Name = "Name2", Details = "Details2", Value = 2 }
            };

            // Act
            SetRootUrlToKinveyClient(unreachableUrl);
            var savedToDos = await autoToDoStore.SaveAsync(toDos);
            SetRootUrlToKinveyClient(kinveyUrl);

            var pushResponse = await autoToDoStore.PushAsync();

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
            var existingToDosNetwork = await networkToDoStore.FindAsync();

            // Teardown
            await networkToDoStore.RemoveAsync(existingToDosNetwork[0].ID);
            await networkToDoStore.RemoveAsync(existingToDosNetwork[1].ID);

            // Assert
            Assert.AreEqual(2, pushResponse.PushCount);
            Assert.IsNotNull(pushResponse.PushEntities.FirstOrDefault(e => e.Name.Equals(toDos[0].Name) && e.Details.Equals(toDos[0].Details) && e.Value == toDos[0].Value));
            Assert.IsNotNull(pushResponse.PushEntities.FirstOrDefault(e => e.Name.Equals(toDos[1].Name) && e.Details.Equals(toDos[1].Details) && e.Value == toDos[1].Value));

            Assert.AreEqual(0, pendingWriteActions.Count);

            Assert.AreEqual(2, existingToDosNetwork.Count);
            Assert.IsNotNull(existingToDosNetwork.FirstOrDefault(e => e.ID == pushResponse.PushEntities[0].ID));
            Assert.IsNotNull(existingToDosNetwork.FirstOrDefault(e => e.ID == pushResponse.PushEntities[1].ID));
        }

        [TestMethod]
        public async Task TestPushNewItemsExistingItemsConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient("5");

            if (MockData)
            {
                MockResponses(10);
            }

            // Arrange
            var user = await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var autoToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);
            var syncToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC, kinveyClient);
            var networkToDoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK, kinveyClient);

            var toDos = new List<ToDo>
            {
               
            };

            var toDo1 = new ToDo { Name = "Name1", Details = "Details1", Value = 1 };
            toDos.Add(toDo1);

            var toDo2 = new ToDo { Name = "Name2", Details = "Details2", Value = 2 };
            toDo2 = await autoToDoStore.SaveAsync(toDo2);
            toDo2.Name = "Name22";
            toDo2.Details = "Details22";
            toDo2.Value = 22;
            toDos.Add(toDo2);

            var toDo3 = new ToDo { Name = "Name3", Details = "Details3", Value = 3 };
            toDos.Add(toDo3);

            var toDo4 = new ToDo { ID = Guid.NewGuid().ToString(), Name = "Name4", Details = "Details4", Value = 4 };
            toDos.Add(toDo4);

            // Act
            SetRootUrlToKinveyClient(unreachableUrl);
            var savedToDos = await autoToDoStore.SaveAsync(toDos);
            SetRootUrlToKinveyClient(kinveyUrl);

            var pushResponse = await autoToDoStore.PushAsync();

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
            var existingToDosNetwork = await networkToDoStore.FindAsync();

            // Teardown
            await networkToDoStore.RemoveAsync(pushResponse.PushEntities[0].ID);
            await networkToDoStore.RemoveAsync(pushResponse.PushEntities[1].ID);
            await networkToDoStore.RemoveAsync(pushResponse.PushEntities[2].ID);
            await networkToDoStore.RemoveAsync(pushResponse.PushEntities[3].ID);

            // Assert
            Assert.AreEqual(4, pushResponse.PushCount);
            Assert.IsNotNull(pushResponse.PushEntities.FirstOrDefault(e => e.Name.Equals(toDos[0].Name) && e.Details.Equals(toDos[0].Details) && e.Value == toDos[0].Value));
            Assert.IsNotNull(pushResponse.PushEntities.FirstOrDefault(e => e.ID.Equals(toDos[1].ID) && e.Name.Equals(toDos[1].Name) && e.Details.Equals(toDos[1].Details) && e.Value == toDos[1].Value));
            Assert.IsNotNull(pushResponse.PushEntities.FirstOrDefault(e => e.Name.Equals(toDos[2].Name) && e.Details.Equals(toDos[2].Details) && e.Value == toDos[2].Value));
            Assert.IsNotNull(pushResponse.PushEntities.FirstOrDefault(e => e.ID.Equals(toDos[3].ID) && e.Name.Equals(toDos[3].Name) && e.Details.Equals(toDos[3].Details) && e.Value == toDos[3].Value));

            Assert.AreEqual(0, pendingWriteActions.Count);

            Assert.AreEqual(4, existingToDosNetwork.Count);
            Assert.IsNotNull(existingToDosNetwork.FirstOrDefault(e => e.ID == pushResponse.PushEntities[0].ID));
            Assert.IsNotNull(existingToDosNetwork.FirstOrDefault(e => e.ID == pushResponse.PushEntities[1].ID));
            Assert.IsNotNull(existingToDosNetwork.FirstOrDefault(e => e.ID == pushResponse.PushEntities[2].ID));
            Assert.IsNotNull(existingToDosNetwork.FirstOrDefault(e => e.ID == pushResponse.PushEntities[3].ID));
        }

        [TestMethod]
        public async Task TestPushNewItemsInvalidPermissionsAsync()
        {
            // Setup
            kinveyClient = BuildClient("5");

            if (MockData)
            {
                MockResponses(2);
            }

            // Arrange
            await User.LoginAsync(TestSetup.user_without_permissions, TestSetup.pass_for_user_without_permissions, kinveyClient);

            var todoSyncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC, kinveyClient);
            var todoNetworkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK, kinveyClient);
            var todoAutoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);

            var toDos = new List<ToDo>
            {
                new ToDo { Name = "Name1", Details = "Details1", Value = 1 },
                new ToDo { Name = "Name2", Details = "Details2", Value = 2 }
            };

            // Act
            var savedToDos = await todoSyncStore.SaveAsync(toDos);

            var pushResponse = await todoSyncStore.PushAsync();

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
            var existingToDos = await todoSyncStore.FindAsync();

            // Teardown
            await todoSyncStore.RemoveAsync(todoNetworkStore.Where(e => e.Name.StartsWith("Name")));

            // Assert
            Assert.AreEqual(1, pushResponse.KinveyExceptions.Count);
            Assert.AreEqual(2, pushResponse.PushCount);
            Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, pushResponse.KinveyExceptions[0].ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, pushResponse.KinveyExceptions[0].ErrorCode);
            Assert.AreEqual(401, pushResponse.KinveyExceptions[0].StatusCode);

            Assert.AreEqual(2, existingToDos.Count);
            Assert.IsNotNull(existingToDos.FirstOrDefault(e => e.Name.Equals(toDos[0].Name) && e.Details.Equals(toDos[0].Details) && e.Value == toDos[0].Value && e.Acl == null && e.Kmd == null));
            Assert.IsNotNull(existingToDos.FirstOrDefault(e => e.Name.Equals(toDos[1].Name) && e.Details.Equals(toDos[1].Details) && e.Value == toDos[1].Value && e.Acl == null && e.Kmd == null));

            Assert.AreEqual(2, pendingWriteActions.Count);
            var pendingWriteAction1 = pendingWriteActions.FirstOrDefault(e => e.entityId == existingToDos[0].ID);
            Assert.IsNotNull(pendingWriteAction1);
            Assert.AreEqual("POST", pendingWriteAction1.action);
            var pendingWriteAction2 = pendingWriteActions.FirstOrDefault(e => e.entityId == existingToDos[1].ID);
            Assert.IsNotNull(pendingWriteAction2);
            Assert.AreEqual("POST", pendingWriteAction2.action);
        }

        [TestMethod]
        public async Task TestPushNewItemsWithErrorsAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient("5");

                MockResponses(4);

                // Arrange
                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                var todoAutoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);
                var todoSyncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC, kinveyClient);
                var todoNetworkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK, kinveyClient);

                var toDos = new List<ToDo>
                {
                    new ToDo { Name = "Name1", Details = "Details1", Value = 1, GeoLoc = "[200,200]" },
                    new ToDo { Name = TestSetup.entity_name_for_400_response_error, Details = "Details2", Value = 2 },
                    new ToDo { Name = "Name3", Details = "Details3", Value = 3 }
                };

                // Act
                SetRootUrlToKinveyClient(unreachableUrl);
                var savedToDos = await todoAutoStore.SaveAsync(toDos);
                SetRootUrlToKinveyClient(kinveyUrl);

                var pushResponse = await todoAutoStore.PushAsync();

                var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
                var existingToDosLocal = await todoSyncStore.FindAsync();
                var existingToDosNetwork = await todoNetworkStore.FindAsync();

                // Teardown
                await todoNetworkStore.RemoveAsync(pushResponse.PushEntities[0].ID);

                // Assert
                Assert.AreEqual(3, pushResponse.PushCount);
                Assert.IsNull(pushResponse.PushEntities.FirstOrDefault(e => e.Name.Equals(toDos[0].Name) && e.Details.Equals(toDos[0].Details) && e.Value == toDos[0].Value));
                Assert.IsNull(pushResponse.PushEntities.FirstOrDefault(e => e.Name.Equals(toDos[1].Name) && e.Details.Equals(toDos[1].Details) && e.Value == toDos[1].Value));
                Assert.IsNotNull(pushResponse.PushEntities.FirstOrDefault(e => e.Name.Equals(toDos[2].Name) && e.Details.Equals(toDos[2].Details) && e.Value == toDos[2].Value));

                Assert.AreEqual(2, pushResponse.KinveyExceptions.Count);
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, pushResponse.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_GENERAL, pushResponse.KinveyExceptions[0].ErrorCode);

                Assert.AreEqual(2, pendingWriteActions.Count);
                var pendingWriteAction1 = pendingWriteActions.FirstOrDefault(e => e.entityId == savedToDos.Entities[0].ID);
                Assert.IsNotNull(pendingWriteAction1);
                Assert.AreEqual("POST", pendingWriteAction1.action);
                var pendingWriteAction2 = pendingWriteActions.FirstOrDefault(e => e.entityId == savedToDos.Entities[1].ID);
                Assert.IsNotNull(pendingWriteAction2);
                Assert.AreEqual("POST", pendingWriteAction2.action);

                Assert.AreEqual(3, existingToDosLocal.Count);
                Assert.IsNotNull(existingToDosLocal.FirstOrDefault(e => e.Name.Equals(toDos[0].Name) && e.Details.Equals(toDos[0].Details) && e.Value == toDos[0].Value && e.Acl == null && e.Kmd == null));
                Assert.IsNotNull(existingToDosLocal.FirstOrDefault(e => e.Name.Equals(toDos[1].Name) && e.Details.Equals(toDos[1].Details) && e.Value == toDos[1].Value && e.Acl == null && e.Kmd == null));
                Assert.IsNotNull(existingToDosLocal.FirstOrDefault(e => e.Name.Equals(toDos[2].Name) && e.Details.Equals(toDos[2].Details) && e.Value == toDos[2].Value && e.Acl != null && e.Kmd != null && !string.IsNullOrEmpty(e.Kmd.entityCreationTime) && !string.IsNullOrEmpty(e.Kmd.lastModifiedTime)));

                Assert.AreEqual(1, existingToDosNetwork.Count);
                Assert.IsNotNull(existingToDosNetwork.FirstOrDefault(e => e.Name.Equals(toDos[2].Name) && e.Details.Equals(toDos[2].Details) && e.Value == toDos[2].Value));
            }
        }

        [TestMethod]
        public async Task TestPushNewSeparateItemsAsync()
        {           
            // Setup
            kinveyClient = BuildClient("5");

            if (MockData)
            {
                MockResponses(5);
            }

            // Arrange
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var todoAutoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);
            var todoSyncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC, kinveyClient);
            var todoNetworkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK, kinveyClient);

            var toDo1 = new ToDo { Name = "Name1", Details = "Details1", Value = 1 };
            var toDo2 = new ToDo { Name = "Name2", Details = "Details2", Value = 2 };

            // Act
            SetRootUrlToKinveyClient(unreachableUrl);
            toDo1 = await todoAutoStore.SaveAsync(toDo1);
            toDo2 = await todoAutoStore.SaveAsync(toDo2);
            SetRootUrlToKinveyClient(kinveyUrl);

            var pushResponse = await todoAutoStore.PushAsync();

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
            var existingToDos = await todoNetworkStore.FindAsync();

            // Teardown
            await todoNetworkStore.RemoveAsync(existingToDos[0].ID);
            await todoNetworkStore.RemoveAsync(existingToDos[1].ID);

            // Assert
            Assert.AreEqual(2, pushResponse.PushCount);
            Assert.IsNotNull(pushResponse.PushEntities.FirstOrDefault(e => e.Name.Equals(toDo1.Name) && e.Details.Equals(toDo1.Details) && e.Value == toDo1.Value));
            Assert.IsNotNull(pushResponse.PushEntities.FirstOrDefault(e => e.Name.Equals(toDo2.Name) && e.Details.Equals(toDo2.Details) && e.Value == toDo2.Value));
            Assert.AreEqual(0, pushResponse.KinveyExceptions.Count);

            Assert.AreEqual(0, pushResponse.KinveyExceptions.Count);

            Assert.AreEqual(0, pendingWriteActions.Count);

            Assert.AreEqual(2, existingToDos.Count);
            Assert.IsNotNull(existingToDos.FirstOrDefault(e => e.Name.Equals(toDo1.Name) && e.Details.Equals(toDo1.Details) && e.Value == toDo1.Value && e.Acl != null && e.Kmd != null && !string.IsNullOrEmpty(e.Kmd.entityCreationTime) && !string.IsNullOrEmpty(e.Kmd.lastModifiedTime)));
            Assert.IsNotNull(existingToDos.FirstOrDefault(e => e.Name.Equals(toDo2.Name) && e.Details.Equals(toDo2.Details) && e.Value == toDo2.Value && e.Acl != null && e.Kmd != null && !string.IsNullOrEmpty(e.Kmd.entityCreationTime) && !string.IsNullOrEmpty(e.Kmd.lastModifiedTime)));
        }

        #endregion Positive tests

        #region Negative tests

        [TestMethod]
        public async Task TestPush400ErrorResponseAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient();

                MockResponses(2);

                // Arrange
                var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
                var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

                var newItem1 = new ToDo
                {
                    Name = TestSetup.entity_name_for_400_response_error
                };

                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                await syncStore.SaveAsync(newItem1);

                // Act
                var pushResult = await autoStore.PushAsync();

                // Assert
                Assert.AreEqual(0, pushResult.PushEntities.Count);
                Assert.AreEqual(1, pushResult.KinveyExceptions.Count);
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, pushResult.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, pushResult.KinveyExceptions[0].ErrorCode);
                Assert.AreEqual(400, pushResult.KinveyExceptions[0].StatusCode);
            }
        }

        [TestMethod]
        public async Task TestPush401ErrorResponseAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(2);
            }

            // Arrange
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "Test"
            };

            await User.LoginAsync(TestSetup.user_without_permissions, TestSetup.pass_for_user_without_permissions, kinveyClient);

            await syncStore.SaveAsync(newItem1);

            // Act
            var pushResult = await autoStore.PushAsync();

            // Assert
            Assert.AreEqual(0, pushResult.PushEntities.Count);
            Assert.AreEqual(1, pushResult.KinveyExceptions.Count);
            Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, pushResult.KinveyExceptions[0].ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, pushResult.KinveyExceptions[0].ErrorCode);
            Assert.AreEqual(401, pushResult.KinveyExceptions[0].StatusCode);

        }

        [TestMethod]
        public async Task TestPush403ErrorResponseAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient();

                MockResponses(2);

                // Arrange
                var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
                var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

                var newItem1 = new ToDo
                {
                    Name = TestSetup.entity_name_for_403_response_error
                };

                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                await syncStore.SaveAsync(newItem1);

                // Act
                var pushResult = await autoStore.PushAsync();

                // Assert
                Assert.AreEqual(0, pushResult.PushEntities.Count);
                Assert.AreEqual(1, pushResult.KinveyExceptions.Count);
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, pushResult.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, pushResult.KinveyExceptions[0].ErrorCode);
                Assert.AreEqual(403, pushResult.KinveyExceptions[0].StatusCode);
            }
        }

        [TestMethod]
        public async Task TestPush404ErrorResponseAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient();

                MockResponses(2);

                // Arrange
                var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
                var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

                var newItem1 = new ToDo
                {
                    Name = TestSetup.entity_name_for_404_response_error
                };

                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                await syncStore.SaveAsync(newItem1);

                // Act
                var pushResult = await autoStore.PushAsync();

                // Assert
                Assert.AreEqual(0, pushResult.PushEntities.Count);
                Assert.AreEqual(1, pushResult.KinveyExceptions.Count);
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, pushResult.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, pushResult.KinveyExceptions[0].ErrorCode);
                Assert.AreEqual(404, pushResult.KinveyExceptions[0].StatusCode);
            }
        }

        [TestMethod]
        public async Task TestPush409ErrorResponseAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient();

                MockResponses(2);

                // Arrange
                var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
                var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

                var newItem1 = new ToDo
                {
                    Name = TestSetup.entity_name_for_409_response_error
                };

                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                await syncStore.SaveAsync(newItem1);

                // Act
                var pushResult = await autoStore.PushAsync();

                // Assert
                Assert.AreEqual(0, pushResult.PushEntities.Count);
                Assert.AreEqual(1, pushResult.KinveyExceptions.Count);
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, pushResult.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, pushResult.KinveyExceptions[0].ErrorCode);
                Assert.AreEqual(409, pushResult.KinveyExceptions[0].StatusCode);
            }
        }

        [TestMethod]
        public async Task TestPush500ErrorResponseAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient();

                MockResponses(2);

                // Arrange
                var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
                var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

                var newItem1 = new ToDo
                {
                    Name = TestSetup.entity_name_for_500_response_error
                };

                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                await syncStore.SaveAsync(newItem1);

                // Act
                var pushResult = await autoStore.PushAsync();

                // Assert
                Assert.AreEqual(0, pushResult.PushEntities.Count);
                Assert.AreEqual(1, pushResult.KinveyExceptions.Count);
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, pushResult.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, pushResult.KinveyExceptions[0].ErrorCode);
                Assert.AreEqual(500, pushResult.KinveyExceptions[0].StatusCode);
            }
        }

        #endregion Negative tests

        #endregion Push

        #region Pull

        #region Positive tests

        [TestMethod]
        public async Task TestPullDataNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(6, kinveyClient);
            }

            //Arrange
            var autoStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var syncStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.SYNC);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);

            autoStore.DeltaSetFetchingEnabled = true;

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

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            fc1 = await networkStore.SaveAsync(fc1);
            fc2 = await networkStore.SaveAsync(fc2);

            // Act
            var pullEntities = await autoStore.PullAsync();

            var localEntities = await syncStore.FindAsync();

            //Teardown
            await networkStore.RemoveAsync(fc1.ID);
            await networkStore.RemoveAsync(fc2.ID);

            // Assert
            Assert.IsNotNull(pullEntities);
            Assert.IsNotNull(localEntities);
            Assert.AreEqual(2, pullEntities.PullCount);
            Assert.AreEqual(2, localEntities.Count);
        }

        [TestMethod]
        public async Task TestPullDataNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(5, kinveyClient);
            }

            //Arrange
            var autoStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);

            autoStore.DeltaSetFetchingEnabled = true;

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

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            fc1 = await networkStore.SaveAsync(fc1);
            fc2 = await networkStore.SaveAsync(fc2);

            // Act
            SetRootUrlToKinveyClient(unreachableUrl);
            var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                await autoStore.PullAsync();
            });            
            SetRootUrlToKinveyClient(kinveyUrl);
            
            //Teardown
            await networkStore.RemoveAsync(fc1.ID);
            await networkStore.RemoveAsync(fc2.ID);

            // Assert
            Assert.AreEqual(typeof(KinveyException), exception.GetType());
            KinveyException ke = exception as KinveyException;
            Assert.AreEqual(EnumErrorCategory.ERROR_DATASTORE_NETWORK, ke.ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_GENERAL, ke.ErrorCode);
        }

        [TestMethod]
        public async Task TestPullDataWithQueryNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();
            
            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }

            //Arrange
            var autoStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var syncStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.SYNC);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);

            autoStore.DeltaSetFetchingEnabled = true;

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
            var fc3 = new FlashCard
            {
                Question = "Hello?",
                Answer = "Hi"
            };

            var query = autoStore.Where(e => e.Question.StartsWith("What is"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            fc1 = await networkStore.SaveAsync(fc1);
            fc2 = await networkStore.SaveAsync(fc2);
            fc3 = await networkStore.SaveAsync(fc3);

            // Act
            var pullEntities = await autoStore.PullAsync(query);

            var localEntities = await syncStore.FindAsync();

            //Teardown
            await networkStore.RemoveAsync(fc1.ID);
            await networkStore.RemoveAsync(fc2.ID);
            await networkStore.RemoveAsync(fc3.ID);

            // Assert
            Assert.IsNotNull(pullEntities);
            Assert.IsNotNull(localEntities);
            Assert.AreEqual(2, pullEntities.PullCount);
            Assert.AreEqual(2, localEntities.Count);
        }

        [TestMethod]
        public async Task TestPullDataDeletedFromBackendNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(11, kinveyClient);
            }

            //Arrange
            var autoStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var syncStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.SYNC);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);

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
            var fc3 = new FlashCard
            {
                Question = "Hello?",
                Answer = "Hi"
            };
            var fc4 = new FlashCard
            {
                Question = "What is 3 + 7?",
                Answer = "10"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            fc1 = await networkStore.SaveAsync(fc1);
            fc2 = await networkStore.SaveAsync(fc2);
            fc3 = await networkStore.SaveAsync(fc3);
            fc4 = await networkStore.SaveAsync(fc4);

            // Act
            var pullEntities1 = await autoStore.PullAsync();
            var localEntities1 = await syncStore.FindAsync();

            await networkStore.RemoveAsync(fc1.ID);
            await networkStore.RemoveAsync(fc2.ID);

            var pullEntities2 = await autoStore.PullAsync();
            var localEntities2 = await syncStore.FindAsync();

            //Teardown
            await networkStore.RemoveAsync(fc3.ID);
            await networkStore.RemoveAsync(fc4.ID);

            // Assert
            Assert.IsNotNull(pullEntities1);
            Assert.IsNotNull(localEntities1);
            Assert.IsNotNull(pullEntities2);
            Assert.IsNotNull(localEntities2);
            Assert.AreEqual(4, pullEntities1.PullCount);
            Assert.AreEqual(4, localEntities1.Count);
            Assert.AreEqual(2, pullEntities2.PullCount);
            Assert.AreEqual(2, localEntities2.Count);
        }

        [TestMethod]
        public async Task TestPullDataChangedInBackendNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(13, kinveyClient);
            }

            //Arrange
            var autoStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var syncStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.SYNC);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);

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
            var fc3 = new FlashCard
            {
                Question = "Hello?",
                Answer = "Hi"
            };
            var fc4 = new FlashCard
            {
                Question = "What is 3 + 7?",
                Answer = "10"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            fc1 = await networkStore.SaveAsync(fc1);
            fc2 = await networkStore.SaveAsync(fc2);
            fc3 = await networkStore.SaveAsync(fc3);
            fc4 = await networkStore.SaveAsync(fc4);

            // Act
            var pullEntities1 = await autoStore.PullAsync();

            fc1.Answer = "7!";
            fc2.Answer = "8!";

            fc1 = await networkStore.SaveAsync(fc1);
            fc2 = await networkStore.SaveAsync(fc2);
            
            var pullEntities2 = await autoStore.PullAsync();
            var localEntities = await syncStore.FindAsync();

            //Teardown
            await networkStore.RemoveAsync(fc1.ID);
            await networkStore.RemoveAsync(fc2.ID);
            await networkStore.RemoveAsync(fc3.ID);
            await networkStore.RemoveAsync(fc4.ID);

            // Assert
            Assert.IsNotNull(pullEntities1);            
            Assert.IsNotNull(pullEntities2);
            Assert.IsNotNull(localEntities);
            Assert.AreEqual(4, pullEntities1.PullCount);
            Assert.AreEqual(4, pullEntities2.PullCount);
            Assert.AreEqual(4, localEntities.Count);
            Assert.AreEqual(fc1.Answer, localEntities.FirstOrDefault(e=> e.ID == fc1.ID).Answer);
            Assert.AreEqual(fc2.Answer, localEntities.FirstOrDefault(e => e.ID == fc2.ID).Answer);
        }

        [TestMethod]
        public async Task TestPullDataInvalidPermissionsNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(2, kinveyClient);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            await User.LoginAsync(TestSetup.user_without_permissions, TestSetup.pass_for_user_without_permissions, kinveyClient);

            // Act
            var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                await autoStore.PullAsync();
            });

            // Assert
            Assert.IsTrue(exception.GetType() == typeof(KinveyException));
            KinveyException ke = exception as KinveyException;
            Assert.IsTrue(ke.ErrorCategory == EnumErrorCategory.ERROR_BACKEND);
            Assert.IsTrue(ke.ErrorCode == EnumErrorCode.ERROR_JSON_RESPONSE);
        }

        [TestMethod]
        public async Task TestPullDataNotSyncedNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(1, kinveyClient);
            }

            //Arrange
            var autoStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var syncStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.SYNC);

            autoStore.DeltaSetFetchingEnabled = true;

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

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            fc1 = await syncStore.SaveAsync(fc1);
            fc2 = await syncStore.SaveAsync(fc2);

            // Act
            var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                await autoStore.PullAsync();
            });

            // Assert
            Assert.IsTrue(exception.GetType() == typeof(KinveyException));
            KinveyException ke = exception as KinveyException;
            Assert.IsTrue(ke.ErrorCategory == EnumErrorCategory.ERROR_DATASTORE_NETWORK);
            Assert.IsTrue(ke.ErrorCode == EnumErrorCode.ERROR_DATASTORE_PULL_ONLY_ON_CLEAN_SYNC_QUEUE);
        }

        [TestMethod]
        public async Task TestPullDataDeletedAndChangedInBackendNetworkConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(10, kinveyClient);
            }

            //Arrange
            var autoStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);
            var syncStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.SYNC);

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
            var fc3 = new FlashCard
            {
                Question = "What is 4 + 5?",
                Answer = "9"
            };

            autoStore.DeltaSetFetchingEnabled = true;

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            fc1 = await networkStore.SaveAsync(fc1);
            fc2 = await networkStore.SaveAsync(fc2);
            fc3 = await networkStore.SaveAsync(fc3);

            // Act
            var networkEntities1 = await autoStore.PullAsync();

            fc1.Answer = "7!";
            fc1 = await networkStore.SaveAsync(fc1);
            await networkStore.RemoveAsync(fc2.ID);

            var networkEntities2 = await autoStore.PullAsync();

            var localEntities = await syncStore.FindAsync();

            //Teardown
            await networkStore.RemoveAsync(fc1.ID);
            await networkStore.RemoveAsync(fc3.ID);

            // Assert
            Assert.IsNotNull(networkEntities1);
            Assert.IsNotNull(networkEntities2);
            Assert.IsNotNull(localEntities);
            Assert.AreEqual(3, networkEntities1.PullCount);
            Assert.AreEqual(1, networkEntities2.PullCount);
            Assert.AreEqual(fc1.Answer, localEntities.FirstOrDefault(e=> e.ID == fc1.ID).Answer);
            Assert.AreEqual(fc3.Answer, localEntities.FirstOrDefault(e => e.ID == fc3.ID).Answer);
        }

        [TestMethod]
        public async Task TestDeltaSetPullNoChanges()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(6, kinveyClient);
            }

            //Arrange
            var store = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            fc1 = await store.SaveAsync(fc1);

            // Act
            var firstResponse = await store.PullAsync();
            var secondResponse = await store.PullAsync();

            //Teardown
            var existingEntities = await store.FindAsync();
            if (existingEntities != null)
            {
                await store.RemoveAsync(existingEntities.First().ID);
            }

            // Assert
            Assert.AreEqual(1, firstResponse.PullCount);
            Assert.AreEqual(0, secondResponse.PullCount);
        }

        [TestMethod]
        public async Task TestDeltaSetPullReturnOnlyChanges()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(11, kinveyClient);
            }

            //Arrange
            var store = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            var fc2 = new FlashCard
            {
                Question = "What is 3 + 5",
                Answer = "8"
            };

            var fc3 = new FlashCard
            {
                Question = "Why is 6 afraid of 7?",
                Answer = "Because 7 8 9."
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            fc1 = await store.SaveAsync(fc1);
            var firstResponse = await store.PullAsync();

            fc2 = await store.SaveAsync(fc2);
            var secondResponse = await store.PullAsync();

            fc3 = await store.SaveAsync(fc3);
            var thirdResponse = await store.PullAsync();

            //Teardown
            var existingEntities = await store.FindAsync();
            if (existingEntities != null)
            {
                foreach (var existingEntity in existingEntities)
                {
                    await store.RemoveAsync(existingEntity.ID);
                }
            }

            // Assert
            Assert.AreEqual(1, firstResponse.PullCount);
            Assert.AreEqual(1, secondResponse.PullCount);
            Assert.AreEqual(1, thirdResponse.PullCount);
        }

        [TestMethod]
        public async Task TestDeltaSetPullReturnOnlyUpdates()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(12, kinveyClient);
            }

            //Arrange
            var store = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            var fc2 = new FlashCard
            {
                Question = "What is 3 + 5",
                Answer = "8"
            };

            var fc3 = new FlashCard
            {
                Question = "Why is 6 afraid of 7?",
                Answer = "Because 7 8 9."
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            fc1 = await store.SaveAsync(fc1);
            fc2 = await store.SaveAsync(fc2);
            fc3 = await store.SaveAsync(fc3);
            var query = store.Where(x => x.Question.StartsWith("Wh"));
            var firstResponse = await store.PullAsync(query);

            var fc2Query = store.Where(y => y.Answer.Equals("8"));
            fc2 = (await store.FindAsync(fc2Query)).First();
            fc2.Answer = "14";
            fc2 = await networkStore.SaveAsync(fc2);
            var query2 = store.Where(x => x.Question.StartsWith("Wh"));
            var secondResponse = await store.PullAsync(query2);

            //Teardown
            var existingEntities = await store.FindAsync();
            if (existingEntities != null)
            {
                foreach (var existingEntity in existingEntities)
                {
                    await store.RemoveAsync(existingEntity.ID);
                }
            }

            // Assert
            Assert.AreEqual(3, firstResponse.PullCount);
            Assert.AreEqual(1, secondResponse.PullCount);
        }

        [TestMethod]
        public async Task TestDeltaSetPullReturnOnlyDeletes()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(11, kinveyClient);
            }
           
            //Arrange
            var store = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            var fc2 = new FlashCard
            {
                Question = "What is 3 + 5",
                Answer = "8"
            };

            var fc3 = new FlashCard
            {
                Question = "Why is 6 afraid of 7?",
                Answer = "Because 7 8 9."
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            fc1 = await store.SaveAsync(fc1);
            fc2 = await store.SaveAsync(fc2);
            fc3 = await store.SaveAsync(fc3);
            var query = store.Where(x => x.Question.StartsWith("Wh"));
            var firstResponse = await store.PullAsync(query);

            var fc2Query = store.Where(y => y.Answer.Equals("8"));
            fc2 = (await store.FindAsync(fc2Query)).First();
            int networkDeleteCount = (await networkStore.RemoveAsync(fc2.ID)).count;
            var query2 = store.Where(x => x.Question.StartsWith("Wh"));
            var secondResponse = await store.PullAsync(query2);

            var networkEntities = await store.FindAsync();
            int networkCount = networkEntities.Count;

            //Teardown
            if (networkEntities != null)
            {
                foreach (var networkEntity in networkEntities)
                {
                    await store.RemoveAsync(networkEntity.ID);
                }
            }

            // Assert
            Assert.AreEqual(3, firstResponse.PullCount);
            Assert.AreEqual(0, secondResponse.PullCount);
            Assert.AreEqual(2, networkCount);
            Assert.AreEqual(1, networkDeleteCount);
        }

        [TestMethod]
        public async Task TestDeltaSetPullReturnCorrectNumberOfCreatedItems()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(10, kinveyClient);
            }
            
            //Arrange
            var store = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            var fc2 = new FlashCard
            {
                Question = "What is 3 + 5",
                Answer = "8"
            };

            var fc3 = new FlashCard
            {
                Question = "Why is 6 afraid of 7?",
                Answer = "Because 7 8 9."
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            fc1 = await store.SaveAsync(fc1);
            var firstResponse = await store.PullAsync();

            fc2 = await store.SaveAsync(fc2);
            fc3 = await store.SaveAsync(fc3);
            var secondResponse = await store.PullAsync();

            //Teardown
            var networkEntities = await store.FindAsync();
            if (networkEntities != null)
            {
                foreach (var networkEntity in networkEntities)
                {
                    await store.RemoveAsync(networkEntity.ID);
                }
            }

            // Assert
            Assert.AreEqual(1, firstResponse.PullCount);
            Assert.AreEqual(2, secondResponse.PullCount);
        }

        [TestMethod]
        public async Task TestDeltaSetPullReturnCorrectNumberOfUpdates()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(15, kinveyClient);
            }
           
            //Arrange
            var store = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            var fc2 = new FlashCard
            {
                Question = "What is 3 + 5",
                Answer = "8"
            };

            var fc3 = new FlashCard
            {
                Question = "Why is 6 afraid of 7?",
                Answer = "Because 7 8 9."
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            fc1 = await networkStore.SaveAsync(fc1);
            fc2 = await networkStore.SaveAsync(fc2);
            fc3 = await networkStore.SaveAsync(fc3);
            var query = store.Where(x => x.Question.StartsWith("Wh"));
            var firstResponse = await store.PullAsync(query);

            var fc2Query = store.Where(y => y.Answer.Equals("8"));
            fc2 = (await store.FindAsync(fc2Query)).First();
            fc2.Answer = "14";
            fc2 = await networkStore.SaveAsync(fc2);
            var query2 = store.Where(x => x.Question.StartsWith("Wh"));
            var secondResponse = await store.PullAsync(query2);

            fc1.Answer = "15";
            fc1 = await networkStore.SaveAsync(fc1);

            fc2.Answer = "16";
            fc2 = await networkStore.SaveAsync(fc2);
            var query3 = store.Where(x => x.Question.StartsWith("Wh"));
            var thirdResponse = await store.PullAsync(query3);

            //Teardown
            var networkEntities = await store.FindAsync();
            if (networkEntities != null)
            {
                foreach (var networkEntity in networkEntities)
                {
                    await store.RemoveAsync(networkEntity.ID);
                }
            }

            // Assert
            Assert.AreEqual(3, firstResponse.PullCount);
            Assert.AreEqual(1, secondResponse.PullCount);
            Assert.AreEqual(2, thirdResponse.PullCount);
        }

        [TestMethod]
        public async Task TestDeltaSetWithQueryPullReturnCorrectNumberOfUpdates()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(12, kinveyClient);
            }

            // Arrange           
            var store = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard
            {
                Question = "What+?",
                Answer = "7"
            };

            var fc2 = new FlashCard
            {
                Question = "What+?",
                Answer = "8"
            };

            var fc3 = new FlashCard
            {
                Question = "What+?",
                Answer = "Because 7 8 9."
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            fc1 = await networkStore.SaveAsync(fc1);
            fc2 = await networkStore.SaveAsync(fc2);
            fc3 = await networkStore.SaveAsync(fc3);
            var query = store.Where(x => x.Question.Equals("What+?"));
            var firstResponse = await store.PullAsync(query);

            var fc2Query = store.Where(y => y.Answer.Equals("8"));
            fc2 = (await store.FindAsync(fc2Query)).First();
            fc2.Answer = "14";
            fc2 = await networkStore.SaveAsync(fc2);
            var query2 = store.Where(x => x.Question.Equals("What+?"));
            var secondResponse = await store.PullAsync(query2);

            //Teardown
            var networkEntities = await store.FindAsync();
            if (networkEntities != null)
            {
                foreach (var networkEntity in networkEntities)
                {
                    await store.RemoveAsync(networkEntity.ID);
                }
            }

            // Assert
            Assert.AreEqual(3, firstResponse.PullCount);
            Assert.AreEqual(1, secondResponse.PullCount);
        }

        [TestMethod]
        public async Task TestDeltaSetPullReturnCorrectNumberOfDeletes()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(11, kinveyClient);
            }
          
            //Arrange
            var store = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            var fc2 = new FlashCard
            {
                Question = "What is 3 + 5",
                Answer = "8"
            };

            var fc3 = new FlashCard
            {
                Question = "Why is 6 afraid of 7?",
                Answer = "Because 7 8 9."
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            fc1 = await networkStore.SaveAsync(fc1);
            fc2 = await networkStore.SaveAsync(fc2);
            fc3 = await networkStore.SaveAsync(fc3);
            var firstResponse = await store.PullAsync();

            var firstDeleteResponse = await store.RemoveAsync(fc1.ID);
            var secondResponse = await store.PullAsync();

            var secondDeleteResponse = await store.RemoveAsync(fc2.ID);
            var thirdDeleteResponse = await store.RemoveAsync(fc3.ID);
            var thirdResponse = await store.PullAsync();

            //Teardown
            var networkEntities = await store.FindAsync();
            if (networkEntities != null)
            {
                foreach (var networkEntity in networkEntities)
                {
                    await store.RemoveAsync(networkEntity.ID);
                }
            }

            // Assert
            Assert.AreEqual(3, firstResponse.PullCount);
            Assert.AreEqual(0, secondResponse.PullCount);
            Assert.AreEqual(0, thirdResponse.PullCount);
            Assert.AreEqual(1, firstDeleteResponse.count);
            Assert.AreEqual(1, secondDeleteResponse.count);
            Assert.AreEqual(1, thirdDeleteResponse.count);
        }

        [TestMethod]
        public async Task TestDeltaSetPullReturnCorrectNumberOfDeletesAndUpdates()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(12, kinveyClient);
            }

            //Arrange
            var store = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            var fc2 = new FlashCard
            {
                Question = "What is 3 + 5",
                Answer = "8"
            };

            var fc3 = new FlashCard
            {
                Question = "Why is 6 afraid of 7?",
                Answer = "Because 7 8 9."
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            fc1 = await networkStore.SaveAsync(fc1);
            fc2 = await networkStore.SaveAsync(fc2);
            fc3 = await networkStore.SaveAsync(fc3);
            var firstResponse = await store.PullAsync();

            var fc2Query = store.Where(y => y.Answer.Equals("8"));
            fc2 = (await store.FindAsync(fc2Query)).First();
            fc2.Answer = "14";
            fc2 = await networkStore.SaveAsync(fc2);
            var deleteResponse = await networkStore.RemoveAsync(fc3.ID);
            var secondResponse = await store.PullAsync();

            //Teardown
            var networkEntities = await store.FindAsync();
            if (networkEntities != null)
            {
                foreach (var networkEntity in networkEntities)
                {
                    await store.RemoveAsync(networkEntity.ID);
                }
            }

            // Assert
            Assert.AreEqual(3, firstResponse.PullCount);
            Assert.AreEqual(1, secondResponse.PullCount);
            Assert.AreEqual(1, deleteResponse.count);
        }

        #endregion Positive tests

        #region Negative tests

        [TestMethod]
        public async Task TestPullDataNetwork400ErrorResponseAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient();

                MockResponses(2);
              
                // Arrange
                var autoStore = DataStore<BadRequestErrorEntity>.Collection(badRequestErrorEntityCollection, DataStoreType.AUTO);

                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                // Act
                var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
                {
                    await autoStore.PullAsync();
                });

                // Assert
                Assert.AreEqual(typeof(KinveyException), exception.GetType());
                var kinveyException = exception as KinveyException;
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, kinveyException.ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, kinveyException.ErrorCode);
                Assert.AreEqual(400, kinveyException.StatusCode);
            }
        }

        [TestMethod]
        public async Task TestPullDataNetwork401ErrorResponseAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(2);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            await User.LoginAsync(TestSetup.user_without_permissions, TestSetup.pass_for_user_without_permissions, kinveyClient);

            // Act
            var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
            {
                await autoStore.PullAsync();
            });

            // Assert
            Assert.AreEqual(typeof(KinveyException), exception.GetType());
            var kinveyException = exception as KinveyException;
            Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, kinveyException.ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, kinveyException.ErrorCode);
            Assert.AreEqual(401, kinveyException.StatusCode);
        }

        [TestMethod]
        public async Task TestPullDataNetwork403ErrorResponseAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient();

                MockResponses(2);

                // Arrange
                var autoStore = DataStore<ForbiddenErrorEntity>.Collection(forbiddenErrorEntityCollection, DataStoreType.AUTO);

                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                // Act
                var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
                {
                    await autoStore.PullAsync();
                });

                // Assert
                Assert.AreEqual(typeof(KinveyException), exception.GetType());
                var kinveyException = exception as KinveyException;
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, kinveyException.ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, kinveyException.ErrorCode);
                Assert.AreEqual(403, kinveyException.StatusCode);
            }
        }

        [TestMethod]
        public async Task TestPullDataNetwork404ErrorResponseAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient();

                MockResponses(2);

                // Arrange
                var autoStore = DataStore<NotFoundErrorEntity>.Collection(notFoundErrorEntityCollection, DataStoreType.AUTO);

                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                // Act
                var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
                {
                    await autoStore.PullAsync();
                });

                // Assert
                Assert.AreEqual(typeof(KinveyException), exception.GetType());
                var kinveyException = exception as KinveyException;
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, kinveyException.ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, kinveyException.ErrorCode);
                Assert.AreEqual(404, kinveyException.StatusCode);
            }
        }

        [TestMethod]
        public async Task TestPullDataNetwork409ErrorResponseAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient();

                MockResponses(2);

                // Arrange
                var autoStore = DataStore<ConflictErrorEntity>.Collection(conflictErrorEntityCollection, DataStoreType.AUTO);

                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                // Act
                var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
                {
                    await autoStore.PullAsync();
                });

                // Assert
                Assert.AreEqual(typeof(KinveyException), exception.GetType());
                var kinveyException = exception as KinveyException;
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, kinveyException.ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, kinveyException.ErrorCode);
                Assert.AreEqual(409, kinveyException.StatusCode);
            }
        }

        [TestMethod]
        public async Task TestPullDataNetwork500ErrorResponseAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient();

                MockResponses(2);

                // Arrange
                var autoStore = DataStore<InternalServerErrorEntity>.Collection(internalServerErrorEntityCollection, DataStoreType.AUTO);

                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                // Act
                var exception = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
                {
                    await autoStore.PullAsync();
                });

                // Assert
                Assert.AreEqual(typeof(KinveyException), exception.GetType());
                var kinveyException = exception as KinveyException;
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, kinveyException.ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, kinveyException.ErrorCode);
                Assert.AreEqual(500, kinveyException.StatusCode);
            }
        }

        #endregion Negative tests

        #endregion Pull

        #region Sync

        #region Positive tests

        [TestMethod]
        public async Task TestSyncDataConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(13, kinveyClient);
            }

            //Arrange
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem3 = new ToDo
            {
                Name = "todo3",
                Details = "details for 3 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem4 = new ToDo
            {
                Name = "todo4",
                Details = "details for 4 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem5 = new ToDo
            {
                Name = "todo5",
                Details = "details for 5 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await syncStore.SaveAsync(newItem1);
            newItem2 = await syncStore.SaveAsync(newItem2);
            newItem3 = await syncStore.SaveAsync(newItem3);

            newItem4 = await networkStore.SaveAsync(newItem4);
            newItem5 = await networkStore.SaveAsync(newItem5);

            // Act
            var syncedEntities = await autoStore.SyncAsync();

            var localEntities = await syncStore.FindAsync();
            var networkEntities = await networkStore.FindAsync();

            //Teardown
            foreach (var networkEntity in networkEntities)
            {
                await networkStore.RemoveAsync(networkEntity.ID);
            }
            
            // Assert
            Assert.IsNotNull(networkEntities);
            Assert.AreEqual(3, syncedEntities.PushResponse.PushCount);
            Assert.AreEqual(5, syncedEntities.PullResponse.PullCount);
            Assert.AreEqual(5, networkEntities.Count);
            Assert.AreEqual(5, localEntities.Count);
        }

        [TestMethod]
        public async Task TestSyncDataWithQueryConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(13, kinveyClient);
            }

            //Arrange
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem3 = new ToDo
            {
                Name = "todo3",
                Details = "details3",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem4 = new ToDo
            {
                Name = "todo4",
                Details = "details for 4 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem5 = new ToDo
            {
                Name = "todo5",
                Details = "details5",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var query = autoStore.Where(e=> e.Details.StartsWith("details f"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await syncStore.SaveAsync(newItem1);
            newItem2 = await syncStore.SaveAsync(newItem2);
            newItem3 = await syncStore.SaveAsync(newItem3);

            newItem4 = await networkStore.SaveAsync(newItem4);
            newItem5 = await networkStore.SaveAsync(newItem5);

            // Act
            var syncedEntities = await autoStore.SyncAsync(query);

            var localEntities = await syncStore.FindAsync(query);
            var networkEntities = await networkStore.FindAsync();

            //Teardown
            foreach (var networkEntity in networkEntities)
            {
                await networkStore.RemoveAsync(networkEntity.ID);
            }

            // Assert
            Assert.IsNotNull(networkEntities);
            Assert.AreEqual(3, syncedEntities.PushResponse.PushCount);
            Assert.AreEqual(3, syncedEntities.PullResponse.PullCount);
            Assert.AreEqual(5, networkEntities.Count);
            Assert.AreEqual(3, localEntities.Count);
        }

        [TestMethod]
        public async Task TestSyncDataNetworkConnectionIssueAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(2, kinveyClient);
            }

            //Arrange
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            Exception exception = null;

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await syncStore.SaveAsync(newItem1);
            newItem2 = await syncStore.SaveAsync(newItem2);

            // Act           
            SetRootUrlToKinveyClient(unreachableUrl);            
            try
            {
                await autoStore.SyncAsync();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            SetRootUrlToKinveyClient(kinveyUrl);

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();

            var networkEntities = await networkStore.FindAsync();

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsNotNull(networkEntities);
            Assert.AreEqual(exception.GetType(), typeof(AggregateException));
            Assert.AreEqual(2, pendingWriteActions.Count);
            Assert.AreEqual("POST", pendingWriteActions[0].action);
            Assert.AreEqual("POST", pendingWriteActions[1].action);
            Assert.AreEqual(0, networkEntities.Count);
        }

        [TestMethod]
        public async Task TestDeltaSetSyncNoChanges()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(6, kinveyClient);
            }
          
            var store = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            fc1 = await store.SaveAsync(fc1);

            // Act
            var firstResponse = await store.SyncAsync();
            var secondResponse = await store.SyncAsync();

            //Teardown
            var networkEntities = await store.FindAsync();
            if (networkEntities != null)
            {
                await store.RemoveAsync(networkEntities.First().ID);
            }

            // Assert
            Assert.AreEqual(1, firstResponse.PullResponse.PullCount);
            Assert.AreEqual(0, secondResponse.PullResponse.PullCount);
        }

        [TestMethod]
        public async Task TestDeltaSetSyncReturnOnlyChanges()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(11, kinveyClient);
            }
           
            var store = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            var fc2 = new FlashCard
            {
                Question = "What is 3 + 5",
                Answer = "8"
            };

            var fc3 = new FlashCard
            {
                Question = "Why is 6 afraid of 7?",
                Answer = "Because 7 8 9."
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            fc1 = await store.SaveAsync(fc1);
            var firstResponse = await store.SyncAsync();

            fc2 = await store.SaveAsync(fc2);
            var secondResponse = await store.SyncAsync();

            fc3 = await store.SaveAsync(fc3);
            var thirdResponse = await store.SyncAsync();

            //Teardown
            var networkEntities = await store.FindAsync();
            if (networkEntities != null)
            {
                foreach (var networkEntity in networkEntities)
                {
                    await store.RemoveAsync(networkEntity.ID);
                }
            }

            // Assert
            Assert.AreEqual(1, firstResponse.PullResponse.PullCount);
            Assert.AreEqual(1, secondResponse.PullResponse.PullCount);
            Assert.AreEqual(1, thirdResponse.PullResponse.PullCount);
        }

        [TestMethod]
        public async Task TestDeltaSetSyncReturnOnlyUpdates()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(12, kinveyClient);
            }
            
            var store = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            var fc2 = new FlashCard
            {
                Question = "What is 3 + 5",
                Answer = "8"
            };

            var fc3 = new FlashCard
            {
                Question = "Why is 6 afraid of 7?",
                Answer = "Because 7 8 9."
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            fc1 = await store.SaveAsync(fc1);
            fc2 = await store.SaveAsync(fc2);
            fc3 = await store.SaveAsync(fc3);
            var query = store.Where(x => x.Question.StartsWith("Wh"));
            var firstResponse = await store.SyncAsync(query);

            var fc2Query = store.Where(y => y.Answer.Equals("8"));
            fc2 = (await store.FindAsync(fc2Query)).First();
            fc2.Answer = "14";
            fc2 = await networkStore.SaveAsync(fc2);
            var query2 = store.Where(x => x.Question.StartsWith("Wh"));
            var secondResponse = await store.SyncAsync(query2);

            //Teardown
            var networkEntities = await store.FindAsync();
            if (networkEntities != null)
            {
                foreach (var networkEntity in networkEntities)
                {
                    await store.RemoveAsync(networkEntity.ID);
                }
            }

            // Assert
            Assert.AreEqual(3, firstResponse.PullResponse.PullCount);
            Assert.AreEqual(1, secondResponse.PullResponse.PullCount);
        }

        [TestMethod]
        public async Task TestDeltaSetSyncReturnOnlyDeletes()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(11, kinveyClient);
            }
            
            var store = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            var fc2 = new FlashCard
            {
                Question = "What is 3 + 5",
                Answer = "8"
            };

            var fc3 = new FlashCard
            {
                Question = "Why is 6 afraid of 7?",
                Answer = "Because 7 8 9."
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            fc1 = await store.SaveAsync(fc1);
            fc2 = await store.SaveAsync(fc2);
            fc3 = await store.SaveAsync(fc3);
            var query = store.Where(x => x.Question.StartsWith("Wh"));
            var firstResponse = await store.SyncAsync(query);

            var fc2Query = store.Where(y => y.Answer.Equals("8"));
            fc2 = (await store.FindAsync(fc2Query)).First();
            int removedCount = (await networkStore.RemoveAsync(fc2.ID)).count;
            var query2 = store.Where(x => x.Question.StartsWith("Wh"));
            var secondResponse = await store.SyncAsync(query2);

            var networkEntities = await store.FindAsync();
            int networkCount = networkEntities.Count;

            //Teardown
            if (networkEntities != null)
            {
                foreach (var networkEntity in networkEntities)
                {
                    await store.RemoveAsync(networkEntity.ID);
                }
            }

            // Assert
            Assert.AreEqual(3, firstResponse.PullResponse.PullCount);
            Assert.AreEqual(0, secondResponse.PullResponse.PullCount);
            Assert.AreEqual(2, networkCount);
            Assert.AreEqual(1, removedCount);
        }

        [TestMethod]
        public async Task TestDeltaSetSyncReturnCorrectNumberOfCreatedItems()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(10, kinveyClient);
            }
           
            var store = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            var fc2 = new FlashCard
            {
                Question = "What is 3 + 5",
                Answer = "8"
            };

            var fc3 = new FlashCard
            {
                Question = "Why is 6 afraid of 7?",
                Answer = "Because 7 8 9."
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            fc1 = await store.SaveAsync(fc1);
            var firstResponse = await store.SyncAsync();

            fc2 = await store.SaveAsync(fc2);
            fc3 = await store.SaveAsync(fc3);
            var secondResponse = await store.SyncAsync();

            //Teardown
            var networkEntities = await store.FindAsync();
            if (networkEntities != null)
            {
                foreach (var networkEntity in networkEntities)
                {
                    await store.RemoveAsync(networkEntity.ID);
                }
            }

            // Assert
            Assert.AreEqual(1, firstResponse.PullResponse.PullCount);
            Assert.AreEqual(2, secondResponse.PullResponse.PullCount);
        }

        [TestMethod]
        public async Task TestDeltaSetSyncReturnCorrectNumberOfUpdates()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(15, kinveyClient);
            }
           
            var store = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            var fc2 = new FlashCard
            {
                Question = "What is 3 + 5",
                Answer = "8"
            };

            var fc3 = new FlashCard
            {
                Question = "Why is 6 afraid of 7?",
                Answer = "Because 7 8 9."
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            fc1 = await networkStore.SaveAsync(fc1);
            fc2 = await networkStore.SaveAsync(fc2);
            fc3 = await networkStore.SaveAsync(fc3);
            var query = store.Where(x => x.Question.StartsWith("Wh"));
            var firstResponse = await store.SyncAsync(query);

            var fc2Query = store.Where(y => y.Answer.Equals("8"));
            fc2 = (await store.FindAsync(fc2Query)).First();
            fc2.Answer = "14";
            fc2 = await networkStore.SaveAsync(fc2);
            var query2 = store.Where(x => x.Question.StartsWith("Wh"));
            var secondResponse = await store.SyncAsync(query2);

            fc1.Answer = "15";
            fc1 = await networkStore.SaveAsync(fc1);

            fc2.Answer = "16";
            fc2 = await networkStore.SaveAsync(fc2);
            var query3 = store.Where(x => x.Question.StartsWith("Wh"));
            var thirdResponse = await store.SyncAsync(query3);

            //Teardown
            var networkEntities = await store.FindAsync();
            if (networkEntities != null)
            {
                foreach (var networkEntity in networkEntities)
                {
                    await store.RemoveAsync(networkEntity.ID);
                }
            }

            // Assert
            Assert.AreEqual(3, firstResponse.PullResponse.PullCount);
            Assert.AreEqual(1, secondResponse.PullResponse.PullCount);
            Assert.AreEqual(2, thirdResponse.PullResponse.PullCount);
        }

        [TestMethod]
        public async Task TestDeltaSetSyncReturnCorrectNumberOfDeletes()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(11, kinveyClient);
            }
           
            var store = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            var fc2 = new FlashCard
            {
                Question = "What is 3 + 5",
                Answer = "8"
            };

            var fc3 = new FlashCard
            {
                Question = "Why is 6 afraid of 7?",
                Answer = "Because 7 8 9."
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            fc1 = await networkStore.SaveAsync(fc1);
            fc2 = await networkStore.SaveAsync(fc2);
            fc3 = await networkStore.SaveAsync(fc3);
            var firstResponse = await store.SyncAsync();

            var firstDeleteResponse = await store.RemoveAsync(fc1.ID);
            var secondResponse = await store.SyncAsync();

            var secondDeleteResponse = await store.RemoveAsync(fc2.ID);
            var thirdDeleteResponse = await store.RemoveAsync(fc3.ID);
            var thirdResponse = await store.SyncAsync();

            //Teardown
            var networkEntities = await store.FindAsync();
            if (networkEntities != null)
            {
                foreach (var networkEntity in networkEntities)
                {
                    await store.RemoveAsync(networkEntity.ID);
                }
            }

            // Assert
            Assert.AreEqual(3, firstResponse.PullResponse.PullCount);
            Assert.AreEqual(0, secondResponse.PullResponse.PullCount);
            Assert.AreEqual(0, thirdResponse.PullResponse.PullCount);
            Assert.AreEqual(1, firstDeleteResponse.count);
            Assert.AreEqual(1, secondDeleteResponse.count);
            Assert.AreEqual(1, thirdDeleteResponse.count);
        }

        [TestMethod]
        public async Task TestDeltaSetSyncReturnCorrectNumberOfDeletesAndUpdates()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(12, kinveyClient);
            }
          
            var store = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.AUTO);
            var networkStore = DataStore<FlashCard>.Collection(flashCardCollection, DataStoreType.NETWORK);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard
            {
                Question = "What is 2 + 5?",
                Answer = "7"
            };

            var fc2 = new FlashCard
            {
                Question = "What is 3 + 5",
                Answer = "8"
            };

            var fc3 = new FlashCard
            {
                Question = "Why is 6 afraid of 7?",
                Answer = "Because 7 8 9."
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            fc1 = await networkStore.SaveAsync(fc1);
            fc2 = await networkStore.SaveAsync(fc2);
            fc3 = await networkStore.SaveAsync(fc3);
            var firstResponse = await store.SyncAsync();

            var fc2Query = store.Where(y => y.Answer.Equals("8"));
            fc2 = (await store.FindAsync(fc2Query)).First();
            fc2.Answer = "14";
            fc2 = await networkStore.SaveAsync(fc2);
            var deleteResponse = await networkStore.RemoveAsync(fc3.ID);
            var secondResponse = await store.SyncAsync();

            //Teardown
            var networkEntities = await store.FindAsync();
            if (networkEntities != null)
            {
                foreach (var networkEntity in networkEntities)
                {
                    await store.RemoveAsync(networkEntity.ID);
                }
            }

            // Assert
            Assert.AreEqual(3, firstResponse.PullResponse.PullCount);
            Assert.AreEqual(1, secondResponse.PullResponse.PullCount);
            Assert.AreEqual(1, deleteResponse.count);
        }

        [TestMethod]
        public async Task TestSyncNewItemsWithErrorsAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient("5");

                MockResponses(5);

                // Arrange
                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                var todoAutoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO, kinveyClient);
                var todoSyncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC, kinveyClient);
                var todoNetworkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK, kinveyClient);

                var toDos = new List<ToDo>
                {
                    new ToDo { Name = "Name1", Details = "Details1", Value = 1 },
                    new ToDo { Name = "Name2", Details = "Details2", Value = 2, GeoLoc = "[200,200]" },
                    new ToDo { Name = "Name3", Details = "Details3", Value = 3 }
                };

                // Act
                SetRootUrlToKinveyClient(unreachableUrl);
                var savedToDos = await todoAutoStore.SaveAsync(toDos);
                SetRootUrlToKinveyClient(kinveyUrl);

                var syncResponse = await todoAutoStore.SyncAsync();

                var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();
                var existingToDosNetwork = await todoNetworkStore.FindAsync();
                var existingToDosLocal = await todoSyncStore.FindAsync();

                // Teardown
                await todoNetworkStore.RemoveAsync(existingToDosNetwork[0].ID);
                await todoNetworkStore.RemoveAsync(existingToDosNetwork[1].ID);

                // Assert
                Assert.AreEqual(3, syncResponse.PushResponse.PushCount);
                Assert.IsNotNull(syncResponse.PushResponse.PushEntities.FirstOrDefault(e => e.Name.Equals(toDos[0].Name) && e.Details.Equals(toDos[0].Details) && e.Value == toDos[0].Value));
                Assert.IsNull(syncResponse.PushResponse.PushEntities.FirstOrDefault(e => e.Name.Equals(toDos[1].Name) && e.Details.Equals(toDos[1].Details) && e.Value == toDos[1].Value));
                Assert.IsNotNull(syncResponse.PushResponse.PushEntities.FirstOrDefault(e => e.Name.Equals(toDos[2].Name) && e.Details.Equals(toDos[2].Details) && e.Value == toDos[2].Value));

                Assert.AreEqual(1, syncResponse.PullResponse.KinveyExceptions.Count);
                Assert.AreEqual(EnumErrorCode.ERROR_DATASTORE_PULL_ONLY_ON_CLEAN_SYNC_QUEUE, syncResponse.PullResponse.KinveyExceptions[0].ErrorCode);
                Assert.AreEqual(EnumErrorCategory.ERROR_DATASTORE_NETWORK, syncResponse.PullResponse.KinveyExceptions[0].ErrorCategory);

                Assert.AreEqual(1, syncResponse.PushResponse.KinveyExceptions.Count);
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, syncResponse.PushResponse.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_GENERAL, syncResponse.PushResponse.KinveyExceptions[0].ErrorCode);

                Assert.AreEqual(1, pendingWriteActions.Count);
                var pendingWriteAction1 = pendingWriteActions.FirstOrDefault(e => e.entityId == savedToDos.Entities[1].ID);
                Assert.IsNotNull(pendingWriteAction1);
                Assert.AreEqual("POST", pendingWriteAction1.action);

                Assert.AreEqual(2, existingToDosNetwork.Count);
                Assert.IsNotNull(existingToDosNetwork.FirstOrDefault(e => e.Name.Equals(toDos[0].Name) && e.Details.Equals(toDos[0].Details) && e.Value == toDos[0].Value));
                Assert.IsNotNull(existingToDosNetwork.FirstOrDefault(e => e.Name.Equals(toDos[2].Name) && e.Details.Equals(toDos[2].Details) && e.Value == toDos[2].Value));

                Assert.AreEqual(3, existingToDosLocal.Count);
                Assert.IsNotNull(existingToDosLocal.FirstOrDefault(e => e.Name.Equals(toDos[0].Name) && e.Details.Equals(toDos[0].Details) && e.Value == toDos[0].Value && e.Acl != null && e.Kmd != null && !string.IsNullOrEmpty(e.Kmd.entityCreationTime) && !string.IsNullOrEmpty(e.Kmd.lastModifiedTime)));
                Assert.IsNotNull(existingToDosLocal.FirstOrDefault(e => e.Name.Equals(toDos[1].Name) && e.Details.Equals(toDos[1].Details) && e.Value == toDos[1].Value && e.Acl == null && e.Kmd == null));
                Assert.IsNotNull(existingToDosLocal.FirstOrDefault(e => e.Name.Equals(toDos[2].Name) && e.Details.Equals(toDos[2].Details) && e.Value == toDos[2].Value && e.Acl != null && e.Kmd != null && !string.IsNullOrEmpty(e.Kmd.entityCreationTime) && !string.IsNullOrEmpty(e.Kmd.lastModifiedTime)));
            }
        }

        #endregion Positive tests

        #region Negative tests

        [TestMethod]
        public async Task TestSyncPush400ErrorResponseAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient();

                MockResponses(2);

                // Arrange
                var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
                var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

                var newItem1 = new ToDo
                {
                    Name = TestSetup.entity_name_for_400_response_error
                };

                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                await syncStore.SaveAsync(newItem1);

                // Act
                var syncResult = await autoStore.SyncAsync();

                // Assert
                Assert.AreEqual(0, syncResult.PushResponse.PushEntities.Count);
                Assert.AreEqual(1, syncResult.PushResponse.KinveyExceptions.Count);
                Assert.AreEqual(0, syncResult.PullResponse.PullEntities.Count);
                Assert.AreEqual(1, syncResult.PullResponse.KinveyExceptions.Count);
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, syncResult.PushResponse.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, syncResult.PushResponse.KinveyExceptions[0].ErrorCode);
                Assert.AreEqual(400, syncResult.PushResponse.KinveyExceptions[0].StatusCode);
                Assert.AreEqual(EnumErrorCategory.ERROR_DATASTORE_NETWORK, syncResult.PullResponse.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_DATASTORE_PULL_ONLY_ON_CLEAN_SYNC_QUEUE, syncResult.PullResponse.KinveyExceptions[0].ErrorCode);
            }
        }

        [TestMethod]
        public async Task TestSyncPull400ErrorResponseAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient();

                MockResponses(2);

                // Arrange
                var autoStore = DataStore<BadRequestErrorEntity>.Collection(badRequestErrorEntityCollection, DataStoreType.AUTO);

                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                // Act
                var syncResult = await autoStore.SyncAsync();

                // Assert
                Assert.AreEqual(0, syncResult.PushResponse.PushEntities.Count);
                Assert.AreEqual(0, syncResult.PushResponse.KinveyExceptions.Count);
                Assert.AreEqual(0, syncResult.PullResponse.PullEntities.Count);
                Assert.AreEqual(1, syncResult.PullResponse.KinveyExceptions.Count);
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, syncResult.PullResponse.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, syncResult.PullResponse.KinveyExceptions[0].ErrorCode);
                Assert.AreEqual(400, syncResult.PullResponse.KinveyExceptions[0].StatusCode);
            }
        }

        [TestMethod]
        public async Task TestSyncPush401ErrorResponseAsync()
        {
            // Setup
            kinveyClient = BuildClient();
            if (MockData)
            {
                MockResponses(2);
            }

            // Arrange
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "Test"
            };

            await User.LoginAsync(TestSetup.user_without_permissions, TestSetup.pass_for_user_without_permissions, kinveyClient);

            await syncStore.SaveAsync(newItem1);

            // Act
            var syncResult = await autoStore.SyncAsync();

            // Assert
            Assert.AreEqual(0, syncResult.PushResponse.PushEntities.Count);
            Assert.AreEqual(1, syncResult.PushResponse.KinveyExceptions.Count);
            Assert.AreEqual(0, syncResult.PullResponse.PullEntities.Count);
            Assert.AreEqual(1, syncResult.PullResponse.KinveyExceptions.Count);
            Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, syncResult.PushResponse.KinveyExceptions[0].ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, syncResult.PushResponse.KinveyExceptions[0].ErrorCode);
            Assert.AreEqual(401, syncResult.PushResponse.KinveyExceptions[0].StatusCode);
            Assert.AreEqual(EnumErrorCategory.ERROR_DATASTORE_NETWORK, syncResult.PullResponse.KinveyExceptions[0].ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_DATASTORE_PULL_ONLY_ON_CLEAN_SYNC_QUEUE, syncResult.PullResponse.KinveyExceptions[0].ErrorCode);
        }

        [TestMethod]
        public async Task TestSyncPull401ErrorResponseAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(2);
            }

            // Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            await User.LoginAsync(TestSetup.user_without_permissions, TestSetup.pass_for_user_without_permissions, kinveyClient);

            // Act
            var syncResult = await autoStore.SyncAsync();

            // Assert
            Assert.AreEqual(0, syncResult.PushResponse.PushEntities.Count);
            Assert.AreEqual(0, syncResult.PushResponse.KinveyExceptions.Count);
            Assert.AreEqual(0, syncResult.PullResponse.PullEntities.Count);
            Assert.AreEqual(1, syncResult.PullResponse.KinveyExceptions.Count);
            Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, syncResult.PullResponse.KinveyExceptions[0].ErrorCategory);
            Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, syncResult.PullResponse.KinveyExceptions[0].ErrorCode);
            Assert.AreEqual(401, syncResult.PullResponse.KinveyExceptions[0].StatusCode);
        }

        [TestMethod]
        public async Task TestSyncPush403ErrorResponseAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient();

                MockResponses(2);

                // Arrange
                var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
                var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

                var newItem1 = new ToDo
                {
                    Name = TestSetup.entity_name_for_403_response_error
                };

                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                await syncStore.SaveAsync(newItem1);

                // Act
                var syncResult = await autoStore.SyncAsync();

                // Assert
                Assert.AreEqual(0, syncResult.PushResponse.PushEntities.Count);
                Assert.AreEqual(1, syncResult.PushResponse.KinveyExceptions.Count);
                Assert.AreEqual(0, syncResult.PullResponse.PullEntities.Count);
                Assert.AreEqual(1, syncResult.PullResponse.KinveyExceptions.Count);
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, syncResult.PushResponse.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, syncResult.PushResponse.KinveyExceptions[0].ErrorCode);
                Assert.AreEqual(403, syncResult.PushResponse.KinveyExceptions[0].StatusCode);
                Assert.AreEqual(EnumErrorCategory.ERROR_DATASTORE_NETWORK, syncResult.PullResponse.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_DATASTORE_PULL_ONLY_ON_CLEAN_SYNC_QUEUE, syncResult.PullResponse.KinveyExceptions[0].ErrorCode);
            }
        }

        [TestMethod]
        public async Task TestSyncPull403ErrorResponseAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient();

                MockResponses(2);

                // Arrange
                var autoStore = DataStore<ForbiddenErrorEntity>.Collection(forbiddenErrorEntityCollection, DataStoreType.AUTO);

                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                // Act
                var syncResult = await autoStore.SyncAsync();

                // Assert
                Assert.AreEqual(0, syncResult.PushResponse.PushEntities.Count);
                Assert.AreEqual(0, syncResult.PushResponse.KinveyExceptions.Count);
                Assert.AreEqual(0, syncResult.PullResponse.PullEntities.Count);
                Assert.AreEqual(1, syncResult.PullResponse.KinveyExceptions.Count);
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, syncResult.PullResponse.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, syncResult.PullResponse.KinveyExceptions[0].ErrorCode);
                Assert.AreEqual(403, syncResult.PullResponse.KinveyExceptions[0].StatusCode);
            }
        }

        [TestMethod]
        public async Task TestSyncPull404ErrorResponseAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient();

                MockResponses(2);

                // Arrange
                var autoStore = DataStore<NotFoundErrorEntity>.Collection(notFoundErrorEntityCollection, DataStoreType.AUTO);

                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                // Act
                var syncResult = await autoStore.SyncAsync();

                // Assert
                Assert.AreEqual(0, syncResult.PushResponse.PushEntities.Count);
                Assert.AreEqual(0, syncResult.PushResponse.KinveyExceptions.Count);
                Assert.AreEqual(0, syncResult.PullResponse.PullEntities.Count);
                Assert.AreEqual(1, syncResult.PullResponse.KinveyExceptions.Count);
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, syncResult.PullResponse.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, syncResult.PullResponse.KinveyExceptions[0].ErrorCode);
                Assert.AreEqual(404, syncResult.PullResponse.KinveyExceptions[0].StatusCode);
            }
        }

        [TestMethod]
        public async Task TestSyncPush404ErrorResponseAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient();

                MockResponses(2);

                // Arrange
                var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
                var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

                var newItem1 = new ToDo
                {
                    Name = TestSetup.entity_name_for_404_response_error
                };

                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                await syncStore.SaveAsync(newItem1);

                // Act
                var syncResult = await autoStore.SyncAsync();

                // Assert
                Assert.AreEqual(0, syncResult.PushResponse.PushEntities.Count);
                Assert.AreEqual(1, syncResult.PushResponse.KinveyExceptions.Count);
                Assert.AreEqual(0, syncResult.PullResponse.PullEntities.Count);
                Assert.AreEqual(1, syncResult.PullResponse.KinveyExceptions.Count);
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, syncResult.PushResponse.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, syncResult.PushResponse.KinveyExceptions[0].ErrorCode);
                Assert.AreEqual(404, syncResult.PushResponse.KinveyExceptions[0].StatusCode);
                Assert.AreEqual(EnumErrorCategory.ERROR_DATASTORE_NETWORK, syncResult.PullResponse.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_DATASTORE_PULL_ONLY_ON_CLEAN_SYNC_QUEUE, syncResult.PullResponse.KinveyExceptions[0].ErrorCode);
            }
        }

        [TestMethod]
        public async Task TestSyncPull409ErrorResponseAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient();

                MockResponses(2);

                // Arrange
                var autoStore = DataStore<ConflictErrorEntity>.Collection(conflictErrorEntityCollection, DataStoreType.AUTO);

                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                // Act
                var syncResult = await autoStore.SyncAsync();

                // Assert
                Assert.AreEqual(0, syncResult.PushResponse.PushEntities.Count);
                Assert.AreEqual(0, syncResult.PushResponse.KinveyExceptions.Count);
                Assert.AreEqual(0, syncResult.PullResponse.PullEntities.Count);
                Assert.AreEqual(1, syncResult.PullResponse.KinveyExceptions.Count);
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, syncResult.PullResponse.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, syncResult.PullResponse.KinveyExceptions[0].ErrorCode);
                Assert.AreEqual(409, syncResult.PullResponse.KinveyExceptions[0].StatusCode);
            }
        }

        [TestMethod]
        public async Task TestSyncPush409ErrorResponseAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient();

                MockResponses(2);

                // Arrange
                var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
                var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

                var newItem1 = new ToDo
                {
                    Name = TestSetup.entity_name_for_409_response_error
                };

                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                await syncStore.SaveAsync(newItem1);

                // Act
                var syncResult = await autoStore.SyncAsync();

                // Assert
                Assert.AreEqual(0, syncResult.PushResponse.PushEntities.Count);
                Assert.AreEqual(1, syncResult.PushResponse.KinveyExceptions.Count);
                Assert.AreEqual(0, syncResult.PullResponse.PullEntities.Count);
                Assert.AreEqual(1, syncResult.PullResponse.KinveyExceptions.Count);
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, syncResult.PushResponse.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, syncResult.PushResponse.KinveyExceptions[0].ErrorCode);
                Assert.AreEqual(409, syncResult.PushResponse.KinveyExceptions[0].StatusCode);
                Assert.AreEqual(EnumErrorCategory.ERROR_DATASTORE_NETWORK, syncResult.PullResponse.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_DATASTORE_PULL_ONLY_ON_CLEAN_SYNC_QUEUE, syncResult.PullResponse.KinveyExceptions[0].ErrorCode);
            }
        }

        [TestMethod]
        public async Task TestSyncPush500ErrorResponseAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient();

                MockResponses(2);

                // Arrange
                var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
                var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

                var newItem1 = new ToDo
                {
                    Name = TestSetup.entity_name_for_500_response_error
                };

                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                await syncStore.SaveAsync(newItem1);

                // Act
                var syncResult = await autoStore.SyncAsync();

                // Assert
                Assert.AreEqual(0, syncResult.PushResponse.PushEntities.Count);
                Assert.AreEqual(1, syncResult.PushResponse.KinveyExceptions.Count);
                Assert.AreEqual(0, syncResult.PullResponse.PullEntities.Count);
                Assert.AreEqual(1, syncResult.PullResponse.KinveyExceptions.Count);
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, syncResult.PushResponse.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, syncResult.PushResponse.KinveyExceptions[0].ErrorCode);
                Assert.AreEqual(500, syncResult.PushResponse.KinveyExceptions[0].StatusCode);
                Assert.AreEqual(EnumErrorCategory.ERROR_DATASTORE_NETWORK, syncResult.PullResponse.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_DATASTORE_PULL_ONLY_ON_CLEAN_SYNC_QUEUE, syncResult.PullResponse.KinveyExceptions[0].ErrorCode);
            }
        }

        [TestMethod]
        public async Task TestSyncPull500ErrorResponseAsync()
        {
            if (MockData)
            {
                // Setup
                kinveyClient = BuildClient();

                MockResponses(2);

                // Arrange
                var autoStore = DataStore<InternalServerErrorEntity>.Collection(internalServerErrorEntityCollection, DataStoreType.AUTO);

                await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

                // Act
                var syncResult = await autoStore.SyncAsync();

                // Assert
                Assert.AreEqual(0, syncResult.PushResponse.PushEntities.Count);
                Assert.AreEqual(0, syncResult.PushResponse.KinveyExceptions.Count);
                Assert.AreEqual(0, syncResult.PullResponse.PullEntities.Count);
                Assert.AreEqual(1, syncResult.PullResponse.KinveyExceptions.Count);
                Assert.AreEqual(EnumErrorCategory.ERROR_BACKEND, syncResult.PullResponse.KinveyExceptions[0].ErrorCategory);
                Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, syncResult.PullResponse.KinveyExceptions[0].ErrorCode);
                Assert.AreEqual(500, syncResult.PullResponse.KinveyExceptions[0].StatusCode);
            }
        }

        #endregion Negative tests

        #endregion Sync

        #region Get sync count

        [TestMethod]
        public async Task TestGetSyncCountConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(1, kinveyClient);
            }

            //Arrange
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem3 = new ToDo
            {
                Name = "todo3",
                Details = "details for 3 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };          

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await syncStore.SaveAsync(newItem1);
            newItem2 = await syncStore.SaveAsync(newItem2);
            newItem3 = await syncStore.SaveAsync(newItem3);

            // Act
            var syncCount = autoStore.GetSyncCount();

            // Assert
            Assert.AreEqual(3, syncCount);
        }

        #endregion Get sync count

        #region Purge

        [TestMethod]
        public async Task TestPurgeAllItemsConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(1, kinveyClient);
            }

            //Arrange
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem3 = new ToDo
            {
                Name = "todo3",
                Details = "details3",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await syncStore.SaveAsync(newItem1);
            newItem2 = await syncStore.SaveAsync(newItem2);
            newItem3 = await syncStore.SaveAsync(newItem3);

            // Act
            var purgedCount = autoStore.Purge();

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();

            // Assert
            Assert.AreEqual(3, purgedCount);
            Assert.AreEqual(0, pendingWriteActions.Count);
        }

        [TestMethod]
        public async Task TestPurgeCreatedItemsAccordingToQueryConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(1, kinveyClient);
            }

            //Arrange
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem3 = new ToDo
            {
                Name = "todo3",
                Details = "details3",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var query = autoStore.Where(e => e.Details.StartsWith("details f"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await syncStore.SaveAsync(newItem1);
            newItem2 = await syncStore.SaveAsync(newItem2);
            newItem3 = await syncStore.SaveAsync(newItem3);

            // Act
            var purgedCount = autoStore.Purge(query);

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();

            // Assert
            Assert.AreEqual(2, purgedCount);
            Assert.AreEqual(1, pendingWriteActions.Count);
            Assert.AreEqual(newItem3.ID, pendingWriteActions[0].entityId);
        }

        [TestMethod]
        public async Task TestPurgeUpdatedItemsAccordingToQueryConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }

            //Arrange
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem3 = new ToDo
            {
                Name = "todo3",
                Details = "details3",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var query = autoStore.Where(e => e.Details.StartsWith("details f"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await syncStore.SaveAsync(newItem1);
            newItem2 = await syncStore.SaveAsync(newItem2);
            newItem3 = await syncStore.SaveAsync(newItem3);

            await autoStore.PushAsync();

            newItem1.Name = "todo11";
            newItem1 = await syncStore.SaveAsync(newItem1);
            newItem3.Name = "todo33";
            newItem3 = await syncStore.SaveAsync(newItem3);

            // Act
            var purgedCount = autoStore.Purge(query);

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();

            //Teardown
            var networkEntities = await networkStore.FindAsync();
            foreach (var networkEntity in networkEntities)
            {
                await networkStore.RemoveAsync(networkEntity.ID);
            }

            // Assert
            Assert.AreEqual(1, purgedCount);
            Assert.AreEqual(1, pendingWriteActions.Count);
            Assert.AreEqual(newItem3.ID, pendingWriteActions[0].entityId);
        }

        #endregion Purge

        #region Clear cache

        [TestMethod]
        public async Task TestClearAllItemsConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(6, kinveyClient);
            }

            //Arrange
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await autoStore.SaveAsync(newItem1);
            newItem2 = await autoStore.SaveAsync(newItem2);

            // Act
            var clearResponse = autoStore.ClearCache();

            var networkEntities = await networkStore.FindAsync();

            //Teardown
            await networkStore.RemoveAsync(newItem1.ID);
            await networkStore.RemoveAsync(newItem2.ID);

            // Assert
            Assert.IsNotNull(networkEntities);
            Assert.AreEqual(2, clearResponse.count);
            Assert.AreEqual(2, networkEntities.Count);
        }

        [TestMethod]
        public async Task TestClearItemsByQueryConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }

            //Arrange
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem3 = new ToDo
            {
                Name = "todo3",
                Details = "details3",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var query = autoStore.Where(e => e.Details.StartsWith("details f"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await autoStore.SaveAsync(newItem1);
            newItem2 = await autoStore.SaveAsync(newItem2);
            newItem3 = await autoStore.SaveAsync(newItem3);

            // Act
            var clearResponse = autoStore.ClearCache(query);

            var networkEntities = await networkStore.FindAsync();
            var localEntities = await syncStore.FindAsync();

            //Teardown
            await networkStore.RemoveAsync(newItem1.ID);
            await networkStore.RemoveAsync(newItem2.ID);
            await networkStore.RemoveAsync(newItem3.ID);

            // Assert
            Assert.IsNotNull(networkEntities);
            Assert.IsNotNull(localEntities);
            Assert.AreEqual(2, clearResponse.count);
            Assert.AreEqual(3, networkEntities.Count);
            Assert.AreEqual(1, localEntities.Count);
        }

        [TestMethod]
        public async Task TestClearItemsByQueryFromSyncQueueConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(7, kinveyClient);
            }

            //Arrange
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem3 = new ToDo
            {
                Name = "todo3",
                Details = "details3",
                DueDate = "2016-04-22T19:56:00.963Z"
            };

            var query = autoStore.Where(e => e.Details.StartsWith("details f"));

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await autoStore.SaveAsync(newItem1);
            newItem2 = await autoStore.SaveAsync(newItem2);
            newItem3 = await autoStore.SaveAsync(newItem3);

            newItem1.Name = "todo11";
            newItem1 = await syncStore.SaveAsync(newItem1);
            newItem2.Name = "todo22";
            newItem2 = await syncStore.SaveAsync(newItem2);
            newItem3.Name = "todo33";
            newItem3 = await syncStore.SaveAsync(newItem3);

            // Act
            var clearResponse = autoStore.ClearCache(query);

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();

            //Teardown
            await networkStore.RemoveAsync(newItem1.ID);
            await networkStore.RemoveAsync(newItem2.ID);
            await networkStore.RemoveAsync(newItem3.ID);

            // Assert
            Assert.IsNotNull(clearResponse);
            Assert.IsNotNull(pendingWriteActions);
            Assert.AreEqual(2, clearResponse.count);
            Assert.AreEqual(1, pendingWriteActions.Count);
            Assert.AreEqual(newItem3.ID, pendingWriteActions[0].entityId);
        }

        [TestMethod]
        public async Task TestClearAllItemsFromSyncQueueConnectionAvailableAsync()
        {
            // Setup
            kinveyClient = BuildClient();

            if (MockData)
            {
                MockResponses(1, kinveyClient);
            }

            //Arrange
            var syncStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.SYNC);
            var networkStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.NETWORK);
            var autoStore = DataStore<ToDo>.Collection(toDosCollection, DataStoreType.AUTO);

            var newItem1 = new ToDo
            {
                Name = "todo1",
                Details = "details for 1 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };
            var newItem2 = new ToDo
            {
                Name = "todo2",
                Details = "details for 2 task",
                DueDate = "2016-04-22T19:56:00.963Z"
            };           

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            newItem1 = await syncStore.SaveAsync(newItem1);
            newItem2 = await syncStore.SaveAsync(newItem2);

            // Act
            var clearResponse = autoStore.ClearCache();

            var localEntities = await syncStore.FindAsync();

            var pendingWriteActions = kinveyClient.CacheManager.GetSyncQueue(toDosCollection).GetAll();

            // Assert
            Assert.IsNotNull(clearResponse);
            Assert.IsNotNull(localEntities);
            Assert.IsNotNull(pendingWriteActions);          
            Assert.AreEqual(2, clearResponse.count);
            Assert.AreEqual(0, localEntities.Count);
            Assert.AreEqual(0, pendingWriteActions.Count);
        }

        #endregion Clear cache       
    }
}
