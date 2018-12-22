// Copyright (c) 2016, Kinvey, Inc. All rights reserved.
//
// This software is licensed to you under the Kinvey terms of service located at
// http://www.kinvey.com/terms-of-use. By downloading, accessing and/or using this
// software, you hereby accept such terms of service  (and any agreement referenced
// therein) and agree that you have read, understand and agree to be bound by such
// terms of service and are of legal age to agree to such terms with Kinvey.
//
// This software contains valuable confidential and proprietary information of
// KINVEY, INC and is subject to applicable licensing agreements.
// Unauthorized reproduction, transmission or distribution of this file and its
// contents is a violation of applicable laws.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Moq;

using Kinvey;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kinvey.Tests
{
    [TestClass]
    public class DataStoreCacheIntegrationTests : BaseTestClass
	{
		private Client kinveyClient;

		private const string collectionName = "ToDos";
        private const string CacheFindError = "Cache find returned an error";

        [TestInitialize]
		public override void Setup()
        {
            base.Setup();

            Client.Builder builder = ClientBuilder
                .SetFilePath(TestSetup.db_dir);

            if (MockData)
            {
                builder.setBaseURL("http://localhost:8080");
            }

			kinveyClient = builder.Build();
		}

        [TestCleanup]
		public override void Tear()
		{
            base.Tear();

			kinveyClient.ActiveUser?.Logout();
			System.IO.File.Delete(TestSetup.SQLiteOfflineStoreFilePath);
			System.IO.File.Delete(TestSetup.SQLiteCredentialStoreFilePath);
		}

        [TestMethod]
		public async Task TestCollection()
		{
			// Arrange

			// Act
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE, kinveyClient);

			// Assert
			Assert.IsNotNull(todoStore);
			Assert.IsTrue(string.Equals(todoStore.CollectionName, collectionName));
		}

        [TestMethod]
		public async Task TestCollectionSharedClient()
		{
			// Arrange

			// Act
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);

			// Assert
			Assert.IsNotNull(todoStore);
			Assert.IsTrue(string.Equals(todoStore.CollectionName, collectionName));
		}

        [TestMethod]
		public void TestDeltaSetFetchEnable()
		{
			// Arrange
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);

			// Act
			todoStore.DeltaSetFetchingEnabled = true;

			// Assert
			Assert.IsTrue(todoStore.DeltaSetFetchingEnabled);
		}

        [TestMethod]
		public async Task TestCacheStoreFindByIDAsync()
		{
			// Setup
            if (MockData)
            {
                MockResponses(4, kinveyClient);
            }
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "Next Task";
			newItem.Details = "A test";
			newItem.DueDate = "2016-04-19T20:02:17.635Z";
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
			ToDo t = await todoStore.SaveAsync(newItem);

			// Act
			ToDo networkEntity = null;
			ToDo cacheEntity = null;

			networkEntity = await todoStore.FindByIDAsync(t.ID, new KinveyDelegate<ToDo>
			{
				onSuccess = (result) => cacheEntity = result,
				onError = (error) => Assert.Fail("TestCacheStoreFindByIDAsync: Cache find returned error")
			});

			// Assert
			Assert.IsNotNull(networkEntity);
			Assert.IsTrue(string.Equals(networkEntity.ID, t.ID));
			Assert.IsNotNull(cacheEntity);
			Assert.IsTrue(string.Equals(cacheEntity.ID, t.ID));
			Assert.IsTrue(string.Equals(cacheEntity.ID, networkEntity.ID));

			// Teardown
			await todoStore.RemoveAsync(t.ID);
			kinveyClient.ActiveUser.Logout();
		}

        [TestMethod]
        public async Task TestCacheStoreDeleteByQueryStringValueStartsWithExpressionAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(7);
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var query = todoStore.Where(x => x.Details.StartsWith("Delet"));

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            var listToDoNetwork = new List<ToDo>();
            var listToDoCache = new List<ToDo>();

            KinveyDelegate<List<ToDo>> cacheResults = new KinveyDelegate<List<ToDo>>()
            {
                onSuccess = (List<ToDo> results) => listToDoCache.AddRange(results),
                onError = (Exception e) => Console.WriteLine(e.Message),
            };

            listToDoNetwork = await todoStore.FindAsync(cacheResults: cacheResults);

            var existingItemInNetwork1 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInNetwork2 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInNetwork3 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem3.ID);
            var existingItemInCache1 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInCache2 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInCache3 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem3.ID);

            // Teardown
            await todoStore.RemoveAsync(savedItem3.ID);

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
        public async Task TestCacheStoreDeleteByQueryOrExpressionAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(9);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var query = todoStore.Where(e => e.Name == "Task2 to delete" || e.BoolVal == true);

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await todoStore.FindByIDAsync(savedItem1.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache1 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await todoStore.FindByIDAsync(savedItem2.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache2 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await todoStore.FindByIDAsync(savedItem3.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache3 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);

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
        public async Task TestCacheStoreDeleteByQueryBoolValueExplicitEqualsExpressionAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(9);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var query = todoStore.Where(x => x.BoolVal.Equals(true));

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await todoStore.FindByIDAsync(savedItem1.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache1 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await todoStore.FindByIDAsync(savedItem2.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache2 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await todoStore.FindByIDAsync(savedItem3.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache3 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);

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
        public async Task TestCacheStoreDeleteByQueryBoolValueImplicitEqualExpressionAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(9);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var query = todoStore.Where(x => x.BoolVal);

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await todoStore.FindByIDAsync(savedItem1.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache1 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await todoStore.FindByIDAsync(savedItem2.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache2 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await todoStore.FindByIDAsync(savedItem3.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache3 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);

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
        public async Task TestCacheStoreDeleteByQueryDateTimeValueGreaterThanExpressionAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(9);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var endDate = new DateTime(2018, 1, 1, 0, 0, 0);

            var query = todoStore.Where(x => x.NewDate > endDate);

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await todoStore.FindByIDAsync(savedItem1.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache1 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await todoStore.FindByIDAsync(savedItem2.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache2 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await todoStore.FindByIDAsync(savedItem3.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache3 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);

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
        public async Task TestCacheStoreDeleteByQueryDateTimeValueGreaterThanOrEqualExpressionAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(9);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var endDate = new DateTime(2018, 1, 1, 0, 0, 0);

            var query = todoStore.Where(x => x.NewDate >= endDate);

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await todoStore.FindByIDAsync(savedItem1.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache1 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await todoStore.FindByIDAsync(savedItem2.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache2 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await todoStore.FindByIDAsync(savedItem3.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache3 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);

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
        public async Task TestCacheStoreDeleteByQueryDateTimeValueLessThanExpressionAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(9);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var endDate = new DateTime(2018, 12, 10, 0, 0, 0);

            var query = todoStore.Where(x => x.NewDate < endDate);

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await todoStore.FindByIDAsync(savedItem1.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache1 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await todoStore.FindByIDAsync(savedItem2.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache2 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await todoStore.FindByIDAsync(savedItem3.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache3 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);

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
        public async Task TestCacheStoreDeleteByQueryDateTimeValueLessThanOrEqualExpressionAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(9);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var endDate = new DateTime(2018, 12, 10, 0, 0, 0);

            var query = todoStore.Where(x => x.NewDate <= endDate);

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await todoStore.FindByIDAsync(savedItem1.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache1 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await todoStore.FindByIDAsync(savedItem2.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache2 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await todoStore.FindByIDAsync(savedItem3.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache3 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);

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
        public async Task TestCacheStoreDeleteByQueryIntValueGreaterThanExpressionAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(9);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var query = todoStore.Where(x => x.Value > 1);

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await todoStore.FindByIDAsync(savedItem1.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache1 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await todoStore.FindByIDAsync(savedItem2.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache2 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await todoStore.FindByIDAsync(savedItem3.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache3 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);

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
        public async Task TestCacheStoreDeleteByQueryIntValueGreaterThanOrEqualExpressionAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(9);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var endDate = new DateTime(2018, 12, 10, 0, 0, 0);

            var query = todoStore.Where(x => x.Value >= 2);

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await todoStore.FindByIDAsync(savedItem1.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache1 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await todoStore.FindByIDAsync(savedItem2.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache2 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await todoStore.FindByIDAsync(savedItem3.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache3 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);

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
        public async Task TestCacheStoreDeleteByQueryIntValueLessThanExpressionAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(9);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var endDate = new DateTime(2018, 12, 10, 0, 0, 0);

            var query = todoStore.Where(x => x.Value < 2);

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await todoStore.FindByIDAsync(savedItem1.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache1 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await todoStore.FindByIDAsync(savedItem2.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache2 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await todoStore.FindByIDAsync(savedItem3.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache3 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);

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
        public async Task TestCacheStoreDeleteByQueryIntValueLessThanOrEqualExpressionAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(9);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var query = todoStore.Where(x => x.Value <= 2);

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await todoStore.FindByIDAsync(savedItem1.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache1 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await todoStore.FindByIDAsync(savedItem2.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache2 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await todoStore.FindByIDAsync(savedItem3.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache3 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);

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
        public async Task TestCacheStoreDeleteByQueryIntValueEqualsExpressionAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(9);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var query = todoStore.Where(x => x.Value.Equals(1));

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await todoStore.FindByIDAsync(savedItem1.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache1 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await todoStore.FindByIDAsync(savedItem2.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache2 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await todoStore.FindByIDAsync(savedItem3.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache3 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);

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
        public async Task TestCacheStoreDeleteByQueryLogicalAndExpressionAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(9);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var query = todoStore.Where(todo => todo.Details.StartsWith("Deta") && todo.Name.Equals("Task to delete"));

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await todoStore.FindByIDAsync(savedItem1.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache1 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await todoStore.FindByIDAsync(savedItem2.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache2 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await todoStore.FindByIDAsync(savedItem3.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache3 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);

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
        public async Task TestCacheStoreDeleteByQueryLogicalAndWithOrExpressionAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(9);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var query = todoStore.Where(todo => todo.DueDate.Equals("2018-04-22T19:56:00.963Z") && (todo.Name.StartsWith("TaskDel") ||
                        todo.Details.Equals("Details for")));

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await todoStore.FindByIDAsync(savedItem1.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache1 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await todoStore.FindByIDAsync(savedItem2.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache2 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await todoStore.FindByIDAsync(savedItem3.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache3 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);

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
        public async Task TestCacheStoreDeleteByQueryLogicalOrExpressionAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(9);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var query = todoStore.Where(todo => todo.Name.StartsWith("Task Del") || todo.Details.Equals("Details for"));

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await todoStore.FindByIDAsync(savedItem1.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache1 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await todoStore.FindByIDAsync(savedItem2.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache2 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await todoStore.FindByIDAsync(savedItem3.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache3 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);

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
        public async Task TestCacheStoreDeleteByQueryLogicalOrWithAndExpressionAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(9);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var query = todoStore.Where(todo => todo.Name.StartsWith("Task Del") || todo.DueDate.Equals("2018-04-22T19:56:00.963Z") && todo.Details.Equals("Details for"));

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            ToDo existingItemInNetwork1 = null;
            ToDo existingItemInNetwork2 = null;
            ToDo existingItemInNetwork3 = null;
            ToDo existingItemInCache1 = null;
            ToDo existingItemInCache2 = null;
            ToDo existingItemInCache3 = null;

            try
            {
                existingItemInNetwork1 = await todoStore.FindByIDAsync(savedItem1.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache1 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork2 = await todoStore.FindByIDAsync(savedItem2.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache2 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            try
            {
                existingItemInNetwork3 = await todoStore.FindByIDAsync(savedItem3.ID, new KinveyDelegate<ToDo>
                {
                    onSuccess = (result) => existingItemInCache3 = result,
                    onError = (error) => Assert.Fail(CacheFindError)
                });
            }
            catch (Exception)
            {

            }

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);

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
        public async Task TestCacheStoreDeleteByQueryMultipleWhereClausesStartsWithAndEqualsExpressionsAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(8);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var query = todoStore.Where(x => x.Details.StartsWith("Details f")).Where(y => y.Name.StartsWith("Task D")).Where(z => z.DueDate.Equals("2018-04-22T19:56:00.963Z"));

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);
        
            var listToDoNetwork = new List<ToDo>();
            var listToDoCache = new List<ToDo>();

            KinveyDelegate<List<ToDo>> cacheResults = new KinveyDelegate<List<ToDo>>()
            {
                onSuccess = (List<ToDo> results) => listToDoCache.AddRange(results),
                onError = (Exception e) => Console.WriteLine(e.Message),
            };

            listToDoNetwork = await todoStore.FindAsync(cacheResults: cacheResults);

            var existingItemInNetwork1 = listToDoNetwork.FirstOrDefault(todo=> todo.ID == savedItem1.ID);
            var existingItemInNetwork2 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInNetwork3 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem3.ID);
            var existingItemInCache1 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInCache2 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInCache3 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem3.ID);

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);
            await todoStore.RemoveAsync(savedItem2.ID);

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
        public async Task TestCacheStoreDeleteByQueryMultipleWhereClausesEqualsExpressionsAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(8);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var query = todoStore.Where(x => x.Details.Equals("Details for")).Where(y => y.Name.Equals("Task Test")).Where(z => z.DueDate.Equals("2018-04-22T19:56:00.963Z"));

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            var listToDoNetwork = new List<ToDo>();
            var listToDoCache = new List<ToDo>();

            KinveyDelegate<List<ToDo>> cacheResults = new KinveyDelegate<List<ToDo>>()
            {
                onSuccess = (List<ToDo> results) => listToDoCache.AddRange(results),
                onError = (Exception e) => Console.WriteLine(e.Message),
            };

            listToDoNetwork = await todoStore.FindAsync(cacheResults: cacheResults);

            var existingItemInNetwork1 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInNetwork2 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInNetwork3 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem3.ID);
            var existingItemInCache1 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInCache2 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInCache3 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem3.ID);

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);
            await todoStore.RemoveAsync(savedItem2.ID);

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
        public async Task TestCacheStoreDeleteByQueryMultipleWhereClausesDifferentEqualsExpressionsAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(8);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var query = todoStore.Where(x => x.Details == ("Details for")).Where(y => y.BoolVal == true).Where(z => z.DueDate.Equals("2018-04-22T19:56:00.963Z"));

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            var listToDoNetwork = new List<ToDo>();
            var listToDoCache = new List<ToDo>();

            KinveyDelegate<List<ToDo>> cacheResults = new KinveyDelegate<List<ToDo>>()
            {
                onSuccess = (List<ToDo> results) => listToDoCache.AddRange(results),
                onError = (Exception e) => Console.WriteLine(e.Message),
            };

            listToDoNetwork = await todoStore.FindAsync(cacheResults: cacheResults);

            var existingItemInNetwork1 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInNetwork2 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInNetwork3 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem3.ID);
            var existingItemInCache1 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInCache2 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInCache3 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem3.ID);

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);
            await todoStore.RemoveAsync(savedItem2.ID);

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
        public async Task TestCacheStoreDeleteByQueryMultipleWhereClausesFluentSyntaxEqualExpressionsAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(8);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var query = from t in todoStore where t.Details == "Details for" where t.Name == "Task Del" where t.DueDate == "2018-04-22T19:56:00.963Z" select t;

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            var listToDoNetwork = new List<ToDo>();
            var listToDoCache = new List<ToDo>();

            KinveyDelegate<List<ToDo>> cacheResults = new KinveyDelegate<List<ToDo>>()
            {
                onSuccess = (List<ToDo> results) => listToDoCache.AddRange(results),
                onError = (Exception e) => Console.WriteLine(e.Message),
            };

            listToDoNetwork = await todoStore.FindAsync(cacheResults: cacheResults);

            var existingItemInNetwork1 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInNetwork2 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInNetwork3 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem3.ID);
            var existingItemInCache1 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInCache2 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInCache3 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem3.ID);

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);
            await todoStore.RemoveAsync(savedItem2.ID);

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
        public async Task TestCacheStoreDeleteByQueryMultipleWhereClausesWithLogicalAndExpressionsAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(8);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var query = todoStore.Where(x => x.Details.StartsWith("Details f") && x.DueDate.Equals("2018-04-22T19:56:00.963Z")).Where(y => y.Name.StartsWith("Task Del"));

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            var listToDoNetwork = new List<ToDo>();
            var listToDoCache = new List<ToDo>();

            KinveyDelegate<List<ToDo>> cacheResults = new KinveyDelegate<List<ToDo>>()
            {
                onSuccess = (List<ToDo> results) => listToDoCache.AddRange(results),
                onError = (Exception e) => Console.WriteLine(e.Message),
            };

            listToDoNetwork = await todoStore.FindAsync(cacheResults: cacheResults);

            var existingItemInNetwork1 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInNetwork2 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInNetwork3 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem3.ID);
            var existingItemInCache1 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInCache2 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInCache3 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem3.ID);

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);
            await todoStore.RemoveAsync(savedItem2.ID);

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
        public async Task TestCacheStoreDeleteByQueryMultipleWhereClausesWithLogicalOrExpressionsAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(8);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var query = todoStore.Where(x => x.Details.StartsWith("Details f") || x.DueDate.Equals("2018-04-22T19:56:00.963Z")).Where(y => y.Name.StartsWith("Task D"));

            // Act
            var kinveyDeleteResponse = await todoStore.RemoveAsync(query);

            var listToDoNetwork = new List<ToDo>();
            var listToDoCache = new List<ToDo>();

            KinveyDelegate<List<ToDo>> cacheResults = new KinveyDelegate<List<ToDo>>()
            {
                onSuccess = (List<ToDo> results) => listToDoCache.AddRange(results),
                onError = (Exception e) => Console.WriteLine(e.Message),
            };

            listToDoNetwork = await todoStore.FindAsync(cacheResults: cacheResults);

            var existingItemInNetwork1 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInNetwork2 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInNetwork3 = listToDoNetwork.FirstOrDefault(todo => todo.ID == savedItem3.ID);
            var existingItemInCache1 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem1.ID);
            var existingItemInCache2 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem2.ID);
            var existingItemInCache3 = listToDoCache.FirstOrDefault(todo => todo.ID == savedItem3.ID);

            // Teardown
            await todoStore.RemoveAsync(savedItem1.ID);
            await todoStore.RemoveAsync(savedItem2.ID);

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
        public async Task TestCacheStoreDeleteByQueryNotSupportedExpressionAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(8);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var query = todoStore.Where(x => true);
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
            Assert.IsTrue(ke.ErrorCategory == EnumErrorCategory.ERROR_DATASTORE_NETWORK);
            Assert.IsTrue(ke.ErrorCode == EnumErrorCode.ERROR_LINQ_WHERE_CLAUSE_NOT_SUPPORTED);
        }

        [TestMethod]
        public async Task TestCacheStoreDeleteByQueryNotSupportedStringExpressionAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(8);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            var todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
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

            var savedItem1 = await todoStore.SaveAsync(newItem1);
            var savedItem2 = await todoStore.SaveAsync(newItem2);
            var savedItem3 = await todoStore.SaveAsync(newItem3);

            var query = todoStore.Where(x => x.Name.Contains("support"));
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
            Assert.IsTrue(ke.ErrorCategory == EnumErrorCategory.ERROR_DATASTORE_NETWORK);
            Assert.IsTrue(ke.ErrorCode == EnumErrorCode.ERROR_LINQ_WHERE_CLAUSE_NOT_SUPPORTED);
        }

        [TestMethod]
		public async Task TestCacheStoreFindByQuery()
		{
			// Setup
            if (MockData)
            {
                MockResponses(6, kinveyClient);
            }
			if (kinveyClient.ActiveUser != null)
			{
				kinveyClient.ActiveUser.Logout();
			}

			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			ToDo newItem1 = new ToDo();
			newItem1.Name = "todo";
			newItem1.Details = "details for 1";
			newItem1.DueDate = "2016-04-22T19:56:00.963Z";

			ToDo newItem2 = new ToDo();
			newItem2.Name = "another todo";
			newItem2.Details = "details for 2";
			newItem2.DueDate = "2016-04-22T19:56:00.963Z";

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);

			// Act
			//			var query = from todo in todoStore
			//						where todo.Details.StartsWith("details for 2")
			//						select todo;

			List<ToDo> listToDo = new List<ToDo>();
			List<ToDo> listToDoCache = new List<ToDo>();
			var query = todoStore.Where(x => x.Details.StartsWith("det"));

			KinveyDelegate<List<ToDo>> cacheResults = new KinveyDelegate<List<ToDo>>()
			{
				onSuccess = (List<ToDo> results) => listToDoCache.AddRange(results),
				onError = (Exception e) => Console.WriteLine(e.Message),
			};

			listToDo = await todoStore.FindAsync(query, cacheResults);
			listToDo.AddRange(listToDoCache);

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.IsNotNull(listToDo);
            Assert.AreNotEqual(listToDo.Count, 0);
			Assert.AreEqual(4, listToDo.Count); // 2 from local, 2 from network
		}

        [TestMethod]
		public async Task TestCacheStoreFindByQueryTake1()
		{
			// Setup
            if (MockData)
            {
                MockResponses(6, kinveyClient);
            }
			if (kinveyClient.ActiveUser != null)
			{
				kinveyClient.ActiveUser.Logout();
			}

			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			ToDo newItem1 = new ToDo();
			newItem1.Name = "todo";
			newItem1.Details = "details for 1";
			newItem1.DueDate = "2016-04-22T19:56:00.963Z";

			ToDo newItem2 = new ToDo();
			newItem2.Name = "another todo";
			newItem2.Details = "details for 2";
			newItem2.DueDate = "2016-04-22T19:56:00.963Z";

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);

			// Act
			//			var query = from todo in todoStore
			//						where todo.Details.StartsWith("details for 2")
			//						select todo;

			List<ToDo> listToDo = new List<ToDo>();
			List<ToDo> listToDoCache = new List<ToDo>();
			var query = todoStore.Where(x => x.Details.StartsWith("det")).Take(1);

			KinveyDelegate<List<ToDo>> cacheResults = new KinveyDelegate<List<ToDo>>()
			{
				onSuccess = (List<ToDo> results) => listToDoCache.AddRange(results),
				onError = (Exception e) => Console.WriteLine(e.Message),
			};

			listToDo = await todoStore.FindAsync(query, cacheResults);
			listToDo.AddRange(listToDoCache);

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.IsNotNull(listToDo);
            Assert.AreNotEqual(listToDo.Count, 0);
			Assert.IsNotNull(listToDoCache);
            Assert.AreNotEqual(listToDoCache.Count, 0);
			Assert.AreEqual(1, listToDoCache.Count); // take 1 from local instead of both
			Assert.AreEqual(2, listToDo.Count); // 1 from local, 1 from network
		}

        [TestMethod]
		public async Task TestCacheStoreFindByQuerySkip1()
		{
			// Setup
            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }
			if (kinveyClient.ActiveUser != null)
			{
				kinveyClient.ActiveUser.Logout();
			}

			await User.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			ToDo newItem1 = new ToDo();
			newItem1.Name = "todo";
			newItem1.Details = "details for 1";
			newItem1.DueDate = "2016-04-22T19:56:00.963Z";

			ToDo newItem2 = new ToDo();
			newItem2.Name = "another todo";
			newItem2.Details = "details for 2";
			newItem2.DueDate = "2016-04-22T19:56:00.963Z";

			ToDo newItem3 = new ToDo();
			newItem3.Name = "yet another todo";
			newItem3.Details = "details for 3";
			newItem3.DueDate = "2016-04-22T19:56:00.963Z";

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);
			newItem3 = await todoStore.SaveAsync(newItem3);

			// Act
			List<ToDo> listToDo = new List<ToDo>();
			List<ToDo> listToDoCache = new List<ToDo>();
			var query = todoStore.Where(x => x.Details.StartsWith("det")).Skip(1);

			KinveyDelegate<List<ToDo>> cacheResults = new KinveyDelegate<List<ToDo>>()
			{
				onSuccess = (List<ToDo> results) => listToDoCache.AddRange(results),
				onError = (Exception e) => Console.WriteLine(e.Message),
			};

			listToDo = await todoStore.FindAsync(query, cacheResults);
			listToDo.AddRange(listToDoCache);

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			await todoStore.RemoveAsync(newItem3.ID);
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.IsNotNull(listToDo);
            Assert.AreNotEqual(listToDo.Count, 0);
			Assert.IsNotNull(listToDoCache);
            Assert.AreNotEqual(listToDoCache.Count, 0);
			Assert.AreEqual(2, listToDoCache.Count); // take 2 from local instead all 3
			Assert.AreEqual(4, listToDo.Count); // 2 from local, 2 from network
		}

        [TestMethod]
		public async Task TestCacheStoreFindByQuerySkip1Take1()
		{
			// Setup
            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }
			if (kinveyClient.ActiveUser != null)
			{
				kinveyClient.ActiveUser.Logout();
			}

			await User.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			ToDo newItem1 = new ToDo();
			newItem1.Name = "todo";
			newItem1.Details = "details for 1";
			newItem1.DueDate = "2016-04-22T19:56:00.963Z";

			ToDo newItem2 = new ToDo();
			newItem2.Name = "another todo";
			newItem2.Details = "details for 2";
			newItem2.DueDate = "2016-04-22T19:56:00.963Z";

			ToDo newItem3 = new ToDo();
			newItem3.Name = "yet another todo";
			newItem3.Details = "details for 3";
			newItem3.DueDate = "2016-04-22T19:56:00.963Z";

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);
			newItem3 = await todoStore.SaveAsync(newItem3);

			// Act
			List<ToDo> listToDo = new List<ToDo>();
			List<ToDo> listToDoCache = new List<ToDo>();
			var query = todoStore.Where(x => x.Details.StartsWith("det")).Skip(1).Take(1);

			KinveyDelegate<List<ToDo>> cacheResults = new KinveyDelegate<List<ToDo>>()
			{
				onSuccess = (List<ToDo> results) => listToDoCache.AddRange(results),
				onError = (Exception e) => Console.WriteLine(e.Message),
			};

			listToDo = await todoStore.FindAsync(query, cacheResults);
			listToDo.AddRange(listToDoCache);

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			await todoStore.RemoveAsync(newItem3.ID);
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.IsNotNull(listToDo);
            Assert.AreNotEqual(listToDo.Count, 0);
			Assert.IsNotNull(listToDoCache);
            Assert.AreNotEqual(listToDoCache, 0);
			Assert.AreEqual(1, listToDoCache.Count); // take 1 from local instead of both
			Assert.AreEqual(2, listToDo.Count); // 1 from local, 1 from network
		}

		[TestMethod]
		public async Task TestCacheStoreGetSumAsync()
		{
            // Setup
            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }

			// Arrange
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			DataStore<Person> personStore = DataStore<Person>.Collection("person", DataStoreType.CACHE);

			Person p1 = new Person();
			p1.FirstName = "Michael";
			p1.LastName = "Bluth";
			p1.Age = 40;
			p1 = await personStore.SaveAsync(p1);

			Person p2 = new Person();
			p2.FirstName = "George Michael";
			p2.LastName = "Bluth";
			p2.Age = 15;
			p2 = await personStore.SaveAsync(p2);

			Person p3 = new Person();
			p3.FirstName = "Tobias";
			p3.LastName = "Funke";
			p3.Age = 46;
			p3 = await personStore.SaveAsync(p3);

			var query = personStore.Where(x => x.LastName.Equals("Bluth"));

			// Act
			List<GroupAggregationResults> cacheResults = new List<GroupAggregationResults>();
			int cacheSum = 0;
			int sum = 0;
			List<GroupAggregationResults> arrGAR = await personStore.GroupAndAggregateAsync(EnumReduceFunction.REDUCE_FUNCTION_SUM, "", "Age", query, new KinveyDelegate<List<GroupAggregationResults>>
			{
				onSuccess = (result) => cacheResults.AddRange(result),
				onError = (e) => Console.WriteLine(e.Message)
			});

			foreach (var result in cacheResults)
			{
				cacheSum += result.Result;
			}

			foreach (var result in arrGAR)
			{
				sum += result.Result;
			}

			// Teardown
			await personStore.RemoveAsync(p3.ID);
			await personStore.RemoveAsync(p2.ID);
			await personStore.RemoveAsync(p1.ID);

			// Assert
			Assert.AreNotEqual(0, sum);
			Assert.AreEqual(55, sum);
			Assert.AreNotEqual(0, cacheSum);
			Assert.AreEqual(55, cacheSum);
			Assert.AreEqual(sum, cacheSum);
		}

		[TestMethod]
		public async Task TestCacheStoreGetMinAsync()
		{
            // Setup
            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }

			// Arrange
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			DataStore<Person> personStore = DataStore<Person>.Collection("person", DataStoreType.CACHE);

			Person p1 = new Person();
			p1.FirstName = "Michael";
			p1.LastName = "Bluth";
			p1.Age = 40;
			p1 = await personStore.SaveAsync(p1);

			Person p2 = new Person();
			p2.FirstName = "George Michael";
			p2.LastName = "Bluth";
			p2.Age = 15;
			p2 = await personStore.SaveAsync(p2);

			Person p3 = new Person();
			p3.FirstName = "Tobias";
			p3.LastName = "Funke";
			p3.Age = 46;
			p3 = await personStore.SaveAsync(p3);

			// Act
			List<GroupAggregationResults> cacheResults = new List<GroupAggregationResults>();
			int cacheMin = 0;
			int min = 0;
			List<GroupAggregationResults> arrGAR = await personStore.GroupAndAggregateAsync(EnumReduceFunction.REDUCE_FUNCTION_MIN, "", "Age", null, new KinveyDelegate<List<GroupAggregationResults>>
			{
				onSuccess = (result) => cacheResults.AddRange(result),
				onError = (e) => Console.WriteLine(e.Message)
			});

			foreach (var result in cacheResults)
			{
				cacheMin += result.Result;
			}

			foreach (var result in arrGAR)
			{
				min += result.Result;
			}

			// Teardown
			await personStore.RemoveAsync(p3.ID);
			await personStore.RemoveAsync(p2.ID);
			await personStore.RemoveAsync(p1.ID);

			// Assert
			Assert.AreNotEqual(0, min);
			Assert.AreEqual(15, min);
			Assert.AreNotEqual(0, cacheMin);
			Assert.AreEqual(15, cacheMin);
			Assert.AreEqual(min, cacheMin);
		}

		[TestMethod]
		public async Task TestCacheStoreGetMaxAsync()
		{
            // Setup
            if (MockData)
            {
                MockResponses(8, kinveyClient);
            }

			// Arrange
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			DataStore<Person> personStore = DataStore<Person>.Collection("person", DataStoreType.CACHE);

			Person p1 = new Person();
			p1.FirstName = "Michael";
			p1.LastName = "Bluth";
			p1.Age = 40;
			p1 = await personStore.SaveAsync(p1);

			Person p2 = new Person();
			p2.FirstName = "George Michael";
			p2.LastName = "Bluth";
			p2.Age = 15;
			p2 = await personStore.SaveAsync(p2);

			Person p3 = new Person();
			p3.FirstName = "Tobias";
			p3.LastName = "Funke";
			p3.Age = 46;
			p3 = await personStore.SaveAsync(p3);

			// Act
			List<GroupAggregationResults> cacheResults = new List<GroupAggregationResults>();
			int cacheMax = 0;
			int max = 0;
			List<GroupAggregationResults> arrGAR = await personStore.GroupAndAggregateAsync(EnumReduceFunction.REDUCE_FUNCTION_MAX, "", "Age", null, new KinveyDelegate<List<GroupAggregationResults>>
			{
				onSuccess = (result) => cacheResults.AddRange(result),
				onError = (e) => Console.WriteLine(e.Message)
			});

			foreach (var result in cacheResults)
			{
				cacheMax += result.Result;
			}

			foreach (var result in arrGAR)
			{
				max += result.Result;
			}

			// Teardown
			await personStore.RemoveAsync(p3.ID);
			await personStore.RemoveAsync(p2.ID);
			await personStore.RemoveAsync(p1.ID);

			// Assert
			Assert.AreNotEqual(0, max);
			Assert.AreEqual(46, max);
			Assert.AreNotEqual(0, cacheMax);
			Assert.AreEqual(46, cacheMax);
			Assert.AreEqual(max, cacheMax);
		}

		[TestMethod]
		public async Task TestCacheStoreGetAverageAsync()
		{
            // Setup
            if (MockData)
            {
                MockResponses(10, kinveyClient);
            }

			// Arrange
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			DataStore<Person> personStore = DataStore<Person>.Collection("person", DataStoreType.CACHE);

			Person p1 = new Person();
			p1.FirstName = "Michael";
			p1.LastName = "Bluth";
			p1.Age = 40;
			p1 = await personStore.SaveAsync(p1);

			Person p2 = new Person();
			p2.FirstName = "George Michael";
			p2.LastName = "Bluth";
			p2.Age = 15;
			p2 = await personStore.SaveAsync(p2);

			Person p3 = new Person();
			p3.FirstName = "Tobias";
			p3.LastName = "Funke";
			p3.Age = 46;
			p3 = await personStore.SaveAsync(p3);

			Person p4 = new Person();
			p4.FirstName = "Buster";
			p4.LastName = "Bluth";
			p4.Age = 19;
			p4 = await personStore.SaveAsync(p4);

			// Act
			List<GroupAggregationResults> cacheResults = new List<GroupAggregationResults>();
			int cacheAvg = 0;
			int avg = 0;
			List<GroupAggregationResults> arrGAR = await personStore.GroupAndAggregateAsync(EnumReduceFunction.REDUCE_FUNCTION_AVERAGE, "", "Age", null, new KinveyDelegate<List<GroupAggregationResults>>
			{
				onSuccess = (result) => cacheResults.AddRange(result),
				onError = (e) => Console.WriteLine(e.Message)
			});

			foreach (var result in cacheResults)
			{
				cacheAvg += result.Result;
			}

			foreach (var result in arrGAR)
			{
				avg += result.Result;
			}

			// Teardown
			await personStore.RemoveAsync(p4.ID);
			await personStore.RemoveAsync(p3.ID);
			await personStore.RemoveAsync(p2.ID);
			await personStore.RemoveAsync(p1.ID);

			// Assert
			Assert.AreNotEqual(0, avg);
			Assert.AreEqual(30, avg);
			Assert.AreNotEqual(0, cacheAvg);
			Assert.AreEqual(30, cacheAvg);
			Assert.AreEqual(avg, cacheAvg);
		}

		[TestMethod]
		public async Task TestSaveAsync()
		{
			// Setup
            if (MockData)
            {
                MockResponses(3, kinveyClient);
            }

			if (kinveyClient.IsUserLoggedIn())
			{
				kinveyClient.ActiveUser.Logout();
			}

			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "todo save";
			newItem.Details = "details for save";
			newItem.DueDate = "2016-04-22T19:56:00.961Z";
			//			KinveyMetaData kmd = new KinveyMetaData();
			//			kmd.entityCreationTime = "2016-04-22T19:56:00.900Z";
			//			kmd.lastModifiedTime = "2016-04-22T19:56:00.902Z";
			//			newItem.Metadata = kmd;

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);

			// Act
			ToDo savedItem = await todoStore.SaveAsync(newItem);

			// Assert
			Assert.IsNotNull(savedItem);
			Assert.IsTrue(string.Equals(newItem.Details, savedItem.Details));

			// Teardown
			await todoStore.RemoveAsync(savedItem.ID);
			kinveyClient.ActiveUser.Logout();
		}

		[TestMethod]
		public async Task TestFindByListOfIDs()
		{
			// Setup
            if (MockData)
            {
                MockResponses(7, kinveyClient);
            }

			if (kinveyClient.ActiveUser != null)
			{
				kinveyClient.ActiveUser.Logout();
			}

			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			ToDo newItem1 = new ToDo();
			newItem1.Name = "todo1";
			newItem1.Details = "details for 1";
			newItem1.DueDate = "2016-04-22T19:56:00.963Z";

			ToDo newItem2 = new ToDo();
			newItem2.Name = "todo2";
			newItem2.Details = "details for 2";
			newItem2.DueDate = "2016-04-22T19:56:00.963Z";

			ToDo newItem3 = new ToDo();
			newItem3.Name = "todo3";
			newItem3.Details = "details for 3";
			newItem3.DueDate = "2016-04-22T19:56:00.963Z";

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);
			newItem3 = await todoStore.SaveAsync(newItem3);

			List<string> listIDs = new List<string>();
			listIDs.Add(newItem1.ID);
			listIDs.Add(newItem2.ID);
			listIDs.Add(newItem3.ID);

			// Act
			ICache<ToDo> cache = kinveyClient.CacheManager.GetCache<ToDo>(collectionName);
			List<ToDo> listEntities = cache.FindByIDs(listIDs);

			// Assert
			Assert.IsNotNull(listEntities);
			Assert.AreEqual(3, listEntities.Count);

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			await todoStore.RemoveAsync(newItem3.ID);
			kinveyClient.ActiveUser.Logout();
		}

		[TestMethod]
		public async Task TestGetByQueryAsyncBad()
		{
			// Setup
            if (MockData)
            {
                MockResponses(7, kinveyClient);
            }

			if (kinveyClient.ActiveUser != null)
			{
				kinveyClient.ActiveUser.Logout();
			}

			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			ToDo newItem1 = new ToDo();
			newItem1.Name = "todo1";
			newItem1.Details = "details for 1";
			newItem1.DueDate = "2016-04-22T19:56:00.963Z";

			ToDo newItem2 = new ToDo();
			newItem2.Name = "todo2";
			newItem2.Details = "details for 2";
			newItem2.DueDate = "2016-04-22T19:56:00.963Z";

			ToDo newItem3 = new ToDo();
			newItem3.Name = "todo3";
			newItem3.Details = "details for 3";
			newItem3.DueDate = "2016-04-22T19:56:00.963Z";

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);
			newItem3 = await todoStore.SaveAsync(newItem3);

			List<string> listIDs = new List<string>();
			listIDs.Add(newItem1.ID);
			listIDs.Add(newItem2.ID);
			listIDs.Add(newItem3.ID);

			// Act
			List<ToDo> listEntities = new List<ToDo>();
			var query = from t in todoStore select t;

			// Assert
            await Assert.ThrowsExceptionAsync<KinveyException>(async delegate ()
			{
				foreach (var todo in query)
				{
					listEntities.Add(todo);
				}
			});

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			await todoStore.RemoveAsync(newItem3.ID);
			kinveyClient.ActiveUser.Logout();
		}

		[TestMethod]
		public async Task TestRemoveAsync()
		{
			// Setup
            if (MockData)
            {
                MockResponses(3, kinveyClient);
            }

			if (kinveyClient.ActiveUser != null)
			{
				kinveyClient.ActiveUser.Logout();
			}

			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "todo save";
			newItem.Details = "details for save";
			newItem.DueDate = "2016-04-22T19:56:00.961Z";
			//			KinveyMetaData kmd = new KinveyMetaData();
			//			kmd.entityCreationTime = "2016-04-22T19:56:00.900Z";
			//			kmd.lastModifiedTime = "2016-04-22T19:56:00.902Z";
			//			newItem.Metadata = kmd;

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
			ToDo savedItem = await todoStore.SaveAsync(newItem);
			string savedItemID = savedItem.ID;

			// Act
			KinveyDeleteResponse kdr = await todoStore.RemoveAsync(savedItemID);

			// Assert
			Assert.IsNotNull(kdr);
			Assert.AreEqual(1, kdr.count);

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}

		[TestMethod]
		public async Task TestDeleteByListOfIDs()
		{
			// Setup
            if (MockData)
            {
                MockResponses(7, kinveyClient);
            }
			if (kinveyClient.ActiveUser != null)
			{
				kinveyClient.ActiveUser.Logout();
			}

			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			ToDo newItem1 = new ToDo();
			newItem1.Name = "todo1";
			newItem1.Details = "details for 1";
			newItem1.DueDate = "2016-04-22T19:56:00.963Z";

			ToDo newItem2 = new ToDo();
			newItem2.Name = "todo2";
			newItem2.Details = "details for 2";
			newItem2.DueDate = "2016-04-22T19:56:00.963Z";

			ToDo newItem3 = new ToDo();
			newItem3.Name = "todo3";
			newItem3.Details = "details for 3";
			newItem3.DueDate = "2016-04-22T19:56:00.963Z";

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);
			newItem3 = await todoStore.SaveAsync(newItem3);

			List<string> listIDs = new List<string>();
			listIDs.Add(newItem1.ID);
			listIDs.Add(newItem2.ID);
			listIDs.Add(newItem3.ID);

			// Act
			ICache<ToDo> cache = kinveyClient.CacheManager.GetCache<ToDo>(collectionName);
			KinveyDeleteResponse kdr = cache.DeleteByIDs(listIDs);

			// Assert
			Assert.IsNotNull(kdr);
			Assert.AreEqual(3, kdr.count);

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			await todoStore.RemoveAsync(newItem3.ID);
			kinveyClient.ActiveUser.Logout();
		}

        #region Server-side Delta Sync (SSDS) Tests

        #region SSDS Pull Tests

        //[Test(Description = "with disabled deltaset should make just regular get requests")]
        //with enabled deltaset should return no items when no changes are made
        [TestMethod]
        public async Task TestDeltaSetPullNoChanges()
        {
            // Setup
            if (MockData)
            {
                MockResponses(7, kinveyClient);
            }

            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.CACHE);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard();
            fc1.Question = "What is 2 + 5?";
            fc1.Answer = "7";
            fc1 = await store.SaveAsync(fc1);
            await store.PushAsync();

            // Act
            var firstResponse = await store.PullAsync();
            var secondResponse = await store.PullAsync();
            var localEntities = await store.FindAsync();
            if (localEntities != null)
            {
                await store.RemoveAsync(localEntities.First().ID);
                await store.SyncAsync();
            }

            // Assert
            Assert.AreEqual(1, firstResponse.PullCount);
            Assert.AreEqual(0, secondResponse.PullCount);
        }

        //with enabled deltaset should return only changes since last request
        [TestMethod]
        public async Task TestDeltaSetPullReturnOnlyChanges()
        {
            // Setup
            if (MockData)
            {
                MockResponses(12, kinveyClient);
            }

            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.CACHE);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard();
            fc1.Question = "What is 2 + 5?";
            fc1.Answer = "7";

            var fc2 = new FlashCard();
            fc2.Question = "What is 3 + 5";
            fc2.Answer = "8";

            var fc3 = new FlashCard();
            fc3.Question = "Why is 6 afraid of 7?";
            fc3.Answer = "Because 7 8 9.";

            // Act
            fc1 = await store.SaveAsync(fc1);
            await store.PushAsync();
            var firstResponse = await store.PullAsync();

            fc2 = await store.SaveAsync(fc2);
            await store.PushAsync();
            var secondResponse = await store.PullAsync();

            fc3 = await store.SaveAsync(fc3);
            await store.PushAsync();
            var thirdResponse = await store.PullAsync();

            var localEntities = await store.FindAsync();
            if (localEntities != null)
            {
                foreach (var localEntity in localEntities)
                {
                    await store.RemoveAsync(localEntity.ID);
                }

                await store.SyncAsync();
            }

            // Assert
            Assert.AreEqual(1, firstResponse.PullCount);
            Assert.AreEqual(1, secondResponse.PullCount);
            Assert.AreEqual(1, thirdResponse.PullCount);
        }

        //with enabled deltaset and query should return correct number of updated items
        [TestMethod]
        public async Task TestDeltaSetPullReturnOnlyUpdates()
        {
            // Setup
            if (MockData)
            {
                MockResponses(13, kinveyClient);
            }

            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.CACHE);
            var networkStore = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.NETWORK);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard();
            fc1.Question = "What is 2 + 5?";
            fc1.Answer = "7";

            var fc2 = new FlashCard();
            fc2.Question = "What is 3 + 5";
            fc2.Answer = "8";

            var fc3 = new FlashCard();
            fc3.Question = "Why is 6 afraid of 7?";
            fc3.Answer = "Because 7 8 9.";

            // Act
            fc1 = await store.SaveAsync(fc1);
            fc2 = await store.SaveAsync(fc2);
            fc3 = await store.SaveAsync(fc3);
            var query = store.Where(x => x.Question.StartsWith("Wh"));
            await store.PushAsync();
            var firstResponse = await store.PullAsync(query);

            var fc2Query = store.Where(y => y.Answer.Equals("8"));
            fc2 = (await store.FindAsync(fc2Query)).First();
            fc2.Answer = "14";
            fc2 = await networkStore.SaveAsync(fc2);
            var query2 = store.Where(x => x.Question.StartsWith("Wh"));
            var secondResponse = await store.PullAsync(query2);

            var localEntities = await store.FindAsync();
            if (localEntities != null)
            {
                foreach (var localEntity in localEntities)
                {
                    await store.RemoveAsync(localEntity.ID);
                }

                await store.SyncAsync();
            }

            // Assert
            Assert.AreEqual(3, firstResponse.PullCount);
            Assert.AreEqual(1, secondResponse.PullCount);
        }

        //with enabled deltaset and query should return correct number of deleted items
        [TestMethod]
        public async Task TestDeltaSetPullReturnOnlyDeletes()
        {
            // Setup
            if (MockData)
            {
                MockResponses(12, kinveyClient);
            }

            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.CACHE);
            var networkStore = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.NETWORK);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard();
            fc1.Question = "What is 2 + 5?";
            fc1.Answer = "7";

            var fc2 = new FlashCard();
            fc2.Question = "What is 3 + 5";
            fc2.Answer = "8";

            var fc3 = new FlashCard();
            fc3.Question = "Why is 6 afraid of 7?";
            fc3.Answer = "Because 7 8 9.";

            // Act
            fc1 = await store.SaveAsync(fc1);
            fc2 = await store.SaveAsync(fc2);
            fc3 = await store.SaveAsync(fc3);
            await store.PushAsync();
            var query = store.Where(x => x.Question.StartsWith("Wh"));
            var firstResponse = await store.PullAsync(query);

            var fc2Query = store.Where(y => y.Answer.Equals("8"));
            fc2 = (await store.FindAsync(fc2Query)).First();
            int localDeleteCount = (await networkStore.RemoveAsync(fc2.ID)).count;
            var query2 = store.Where(x => x.Question.StartsWith("Wh"));
            var secondResponse = await store.PullAsync(query2);

            var localEntities = await store.FindAsync();
            int localCount = localEntities.Count;
            bool localCopy = false;

            if (localEntities != null)
            {
                foreach (var localEntity in localEntities)
                {
                    if (fc2.ID == localEntity.ID)
                    {
                        localCopy = true;
                    }

                    await store.RemoveAsync(localEntity.ID);
                }

                await store.SyncAsync();
            }

            // Assert
            Assert.AreEqual(3, firstResponse.PullCount);
            Assert.AreEqual(0, secondResponse.PullCount);
            Assert.AreEqual(2, localCount);
            Assert.AreEqual(1, localDeleteCount);
            Assert.IsTrue(localCopy == false);
        }

        //[Test(Description = "when deltaset is switched off should start sending regular GET requests")]
        //with enabled deltaset should return correct number of items when creating
        [TestMethod]
        public async Task TestDeltaSetPullReturnCorrectNumberOfCreatedItems()
        {
            // Setup
            if (MockData)
            {
                MockResponses(11, kinveyClient);
            }

            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.CACHE);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard();
            fc1.Question = "What is 2 + 5?";
            fc1.Answer = "7";

            var fc2 = new FlashCard();
            fc2.Question = "What is 3 + 5";
            fc2.Answer = "8";

            var fc3 = new FlashCard();
            fc3.Question = "Why is 6 afraid of 7?";
            fc3.Answer = "Because 7 8 9.";

            // Act
            fc1 = await store.SaveAsync(fc1);
            await store.PushAsync();
            var firstResponse = await store.PullAsync();

            fc2 = await store.SaveAsync(fc2);
            fc3 = await store.SaveAsync(fc3);
            await store.PushAsync();
            var secondResponse = await store.PullAsync();

            var localEntities = await store.FindAsync();
            if (localEntities != null)
            {
                foreach (var localEntity in localEntities)
                {
                    await store.RemoveAsync(localEntity.ID);
                }

                await store.SyncAsync();
            }

            // Assert
            Assert.AreEqual(1, firstResponse.PullCount);
            Assert.AreEqual(2, secondResponse.PullCount);
        }

        //with enabled deltaset should return correct number of items when updating
        [TestMethod]
        public async Task TestDeltaSetPullReturnCorrectNumberOfUpdates()
        {
            // Setup
            if (MockData)
            {
                MockResponses(16, kinveyClient);
            }

            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.CACHE);
            var networkStore = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.NETWORK);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard();
            fc1.Question = "What is 2 + 5?";
            fc1.Answer = "7";

            var fc2 = new FlashCard();
            fc2.Question = "What is 3 + 5";
            fc2.Answer = "8";

            var fc3 = new FlashCard();
            fc3.Question = "Why is 6 afraid of 7?";
            fc3.Answer = "Because 7 8 9.";

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

            var localEntities = await store.FindAsync();
            if (localEntities != null)
            {
                foreach (var localEntity in localEntities)
                {
                    await store.RemoveAsync(localEntity.ID);
                }

                await store.SyncAsync();
            }

            // Assert
            Assert.AreEqual(3, firstResponse.PullCount);
            Assert.AreEqual(1, secondResponse.PullCount);
            Assert.AreEqual(2, thirdResponse.PullCount);
        }

        //with enabled deltaset should return correct number of items when deleting
        [TestMethod]
        public async Task TestDeltaSetPullReturnCorrectNumberOfDeletes()
        {
            // Setup
            if (MockData)
            {
                MockResponses(13, kinveyClient);
            }

            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.CACHE);
            var networkStore = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.NETWORK);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard();
            fc1.Question = "What is 2 + 5?";
            fc1.Answer = "7";

            var fc2 = new FlashCard();
            fc2.Question = "What is 3 + 5";
            fc2.Answer = "8";

            var fc3 = new FlashCard();
            fc3.Question = "Why is 6 afraid of 7?";
            fc3.Answer = "Because 7 8 9.";

            // Act
            fc1 = await networkStore.SaveAsync(fc1);
            fc2 = await networkStore.SaveAsync(fc2);
            fc3 = await networkStore.SaveAsync(fc3);
            var firstResponse = await store.PullAsync();

            var firstDeleteResponse = await store.RemoveAsync(fc1.ID);
            await store.PushAsync();
            var secondResponse = await store.PullAsync();
            var firstStoreCount = (await store.FindAsync()).Count;

            var secondDeleteResponse = await store.RemoveAsync(fc2.ID);
            var thirdDeleteResponse = await store.RemoveAsync(fc3.ID);
            await store.PushAsync();
            var thirdResponse = await store.PullAsync();

            var localEntities = await store.FindAsync();
            if (localEntities != null)
            {
                foreach (var localEntity in localEntities)
                {
                    await store.RemoveAsync(localEntity.ID);
                }

                await store.SyncAsync();
            }

            // Assert
            Assert.AreEqual(3, firstResponse.PullCount);
            Assert.AreEqual(0, secondResponse.PullCount);
            Assert.AreEqual(1, firstDeleteResponse.count);
            Assert.AreEqual(0, thirdResponse.PullCount);
            Assert.AreEqual(2, secondDeleteResponse.count + thirdDeleteResponse.count);
        }

        //with enabled deltaset should return correct number of items when deleting and updating
        [TestMethod]
        public async Task TestDeltaSetPullReturnCorrectNumberOfDeletesAndUpdates()
        {
            // Setup
            if (MockData)
            {
                MockResponses(13, kinveyClient);
            }

            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.CACHE);
            var networkStore = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.NETWORK);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard();
            fc1.Question = "What is 2 + 5?";
            fc1.Answer = "7";

            var fc2 = new FlashCard();
            fc2.Question = "What is 3 + 5";
            fc2.Answer = "8";

            var fc3 = new FlashCard();
            fc3.Question = "Why is 6 afraid of 7?";
            fc3.Answer = "Because 7 8 9.";

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

            var localEntities = await store.FindAsync();
            if (localEntities != null)
            {
                foreach (var localEntity in localEntities)
                {
                    await store.RemoveAsync(localEntity.ID);
                }

                await store.SyncAsync();
            }

            // Assert
            Assert.AreEqual(3, firstResponse.PullCount);
            Assert.AreEqual(1, secondResponse.PullCount);
            Assert.AreEqual(1, deleteResponse.count);
        }

        //[Test(Description = "with enabled deltaset and tagged datastore should use the tagged cache collection")]

        //[Test(Description = "with enabled deltaset and autopagination should use AP for first request and DS for the next")]

        //[Test(Description = "with enable deltaset and limit and skip should not use deltaset and should not override lastRunAt")]

        //[Test(Description = "with enable deltaset and limit and skip should not use deltaset and should not cause inconsistent data")]

        #endregion

        #region SSDS Sync Tests

        //[Test(Description = "with disabled deltaset should make just regular get requests")]
        //with enabled deltaset should return no items when no changes are made
        [TestMethod]
        public async Task TestDeltaSetSyncNoChanges()
        {
            // Setup
            if (MockData)
            {
                MockResponses(7, kinveyClient);
            }

            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.CACHE);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard();
            fc1.Question = "What is 2 + 5?";
            fc1.Answer = "7";
            fc1 = await store.SaveAsync(fc1);

            // Act
            var firstResponse = await store.SyncAsync();
            var secondResponse = await store.SyncAsync();
            var localEntities = await store.FindAsync();
            if (localEntities != null)
            {
                await store.RemoveAsync(localEntities.First().ID);
                await store.SyncAsync();
            }

            // Assert
            Assert.AreEqual(1, firstResponse.PullResponse.PullCount);
            Assert.AreEqual(0, secondResponse.PullResponse.PullCount);
        }

        //with enabled deltaset should return only changes since last request
        [TestMethod]
        public async Task TestDeltaSetSyncReturnOnlyChanges()
        {
            // Setup
            if (MockData)
            {
                MockResponses(12, kinveyClient);
            }

            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.CACHE);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard();
            fc1.Question = "What is 2 + 5?";
            fc1.Answer = "7";

            var fc2 = new FlashCard();
            fc2.Question = "What is 3 + 5";
            fc2.Answer = "8";

            var fc3 = new FlashCard();
            fc3.Question = "Why is 6 afraid of 7?";
            fc3.Answer = "Because 7 8 9.";

            // Act
            fc1 = await store.SaveAsync(fc1);
            var firstResponse = await store.SyncAsync();

            fc2 = await store.SaveAsync(fc2);
            var secondResponse = await store.SyncAsync();

            fc3 = await store.SaveAsync(fc3);
            var thirdResponse = await store.SyncAsync();

            var localEntities = await store.FindAsync();
            if (localEntities != null)
            {
                foreach (var localEntity in localEntities)
                {
                    await store.RemoveAsync(localEntity.ID);
                }

                await store.SyncAsync();
            }

            // Assert
            Assert.AreEqual(1, firstResponse.PullResponse.PullCount);
            Assert.AreEqual(1, secondResponse.PullResponse.PullCount);
            Assert.AreEqual(1, thirdResponse.PullResponse.PullCount);
        }

        //with enabled deltaset and query should return correct number of updated items
        [TestMethod]
        public async Task TestDeltaSetSyncReturnOnlyUpdates()
        {
            // Setup
            if (MockData)
            {
                MockResponses(13, kinveyClient);
            }

            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.CACHE);
            var networkStore = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.NETWORK);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard();
            fc1.Question = "What is 2 + 5?";
            fc1.Answer = "7";

            var fc2 = new FlashCard();
            fc2.Question = "What is 3 + 5";
            fc2.Answer = "8";

            var fc3 = new FlashCard();
            fc3.Question = "Why is 6 afraid of 7?";
            fc3.Answer = "Because 7 8 9.";

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

            var localEntities = await store.FindAsync();
            if (localEntities != null)
            {
                foreach (var localEntity in localEntities)
                {
                    await store.RemoveAsync(localEntity.ID);
                }

                await store.SyncAsync();
            }

            // Assert
            Assert.AreEqual(3, firstResponse.PullResponse.PullCount);
            Assert.AreEqual(1, secondResponse.PullResponse.PullCount);
        }

        //with enabled deltaset and query should return correct number of deleted items
        [TestMethod]
        public async Task TestDeltaSetSyncReturnOnlyDeletes()
        {
            // Setup
            if (MockData)
            {
                MockResponses(12, kinveyClient);
            }

            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.CACHE);
            var networkStore = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.NETWORK);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard();
            fc1.Question = "What is 2 + 5?";
            fc1.Answer = "7";

            var fc2 = new FlashCard();
            fc2.Question = "What is 3 + 5";
            fc2.Answer = "8";

            var fc3 = new FlashCard();
            fc3.Question = "Why is 6 afraid of 7?";
            fc3.Answer = "Because 7 8 9.";

            // Act
            fc1 = await store.SaveAsync(fc1);
            fc2 = await store.SaveAsync(fc2);
            fc3 = await store.SaveAsync(fc3);
            var query = store.Where(x => x.Question.StartsWith("Wh"));
            var firstResponse = await store.SyncAsync(query);

            var fc2Query = store.Where(y => y.Answer.Equals("8"));
            fc2 = (await store.FindAsync(fc2Query)).First();
            int localDeleteCount = (await networkStore.RemoveAsync(fc2.ID)).count;
            var query2 = store.Where(x => x.Question.StartsWith("Wh"));
            var secondResponse = await store.SyncAsync(query2);

            var localEntities = await store.FindAsync();
            int localCount = localEntities.Count;
            bool localCopy = false;

            if (localEntities != null)
            {
                foreach (var localEntity in localEntities)
                {
                    if (fc2.ID == localEntity.ID)
                    {
                        localCopy = true;
                    }

                    await store.RemoveAsync(localEntity.ID);
                }

                await store.SyncAsync();
            }

            // Assert
            Assert.AreEqual(3, firstResponse.PullResponse.PullCount);
            Assert.AreEqual(0, secondResponse.PullResponse.PullCount);
            Assert.AreEqual(2, localCount);
            Assert.AreEqual(1, localDeleteCount);
            Assert.IsTrue(localCopy == false);
        }

        //[Test(Description = "when deltaset is switched off should start sending regular GET requests")]
        //with enabled deltaset should return correct number of items when creating
        [TestMethod]
        public async Task TestDeltaSetSyncReturnCorrectNumberOfCreatedItems()
        {
            // Setup
            if (MockData)
            {
                MockResponses(11, kinveyClient);
            }

            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.CACHE);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard();
            fc1.Question = "What is 2 + 5?";
            fc1.Answer = "7";

            var fc2 = new FlashCard();
            fc2.Question = "What is 3 + 5";
            fc2.Answer = "8";

            var fc3 = new FlashCard();
            fc3.Question = "Why is 6 afraid of 7?";
            fc3.Answer = "Because 7 8 9.";

            // Act
            fc1 = await store.SaveAsync(fc1);
            var firstResponse = await store.SyncAsync();

            fc2 = await store.SaveAsync(fc2);
            fc3 = await store.SaveAsync(fc3);
            var secondResponse = await store.SyncAsync();

            var localEntities = await store.FindAsync();
            if (localEntities != null)
            {
                foreach (var localEntity in localEntities)
                {
                    await store.RemoveAsync(localEntity.ID);
                }

                await store.SyncAsync();
            }

            // Assert
            Assert.AreEqual(1, firstResponse.PullResponse.PullCount);
            Assert.AreEqual(2, secondResponse.PullResponse.PullCount);
        }

        //with enabled deltaset should return correct number of items when updating
        [TestMethod]
        public async Task TestDeltaSetSyncReturnCorrectNumberOfUpdates()
        {
            // Setup
            if (MockData)
            {
                MockResponses(16, kinveyClient);
            }

            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.CACHE);
            var networkStore = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.NETWORK);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard();
            fc1.Question = "What is 2 + 5?";
            fc1.Answer = "7";

            var fc2 = new FlashCard();
            fc2.Question = "What is 3 + 5";
            fc2.Answer = "8";

            var fc3 = new FlashCard();
            fc3.Question = "Why is 6 afraid of 7?";
            fc3.Answer = "Because 7 8 9.";

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

            var localEntities = await store.FindAsync();
            if (localEntities != null)
            {
                foreach (var localEntity in localEntities)
                {
                    await store.RemoveAsync(localEntity.ID);
                }

                await store.SyncAsync();
            }

            // Assert
            Assert.AreEqual(3, firstResponse.PullResponse.PullCount);
            Assert.AreEqual(1, secondResponse.PullResponse.PullCount);
            Assert.AreEqual(2, thirdResponse.PullResponse.PullCount);
        }

        //with enabled deltaset should return correct number of items when deleting
        [TestMethod]
        public async Task TestDeltaSetSyncReturnCorrectNumberOfDeletes()
        {
            // Setup
            if (MockData)
            {
                MockResponses(13, kinveyClient);
            }

            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.CACHE);
            var networkStore = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.NETWORK);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard();
            fc1.Question = "What is 2 + 5?";
            fc1.Answer = "7";

            var fc2 = new FlashCard();
            fc2.Question = "What is 3 + 5";
            fc2.Answer = "8";

            var fc3 = new FlashCard();
            fc3.Question = "Why is 6 afraid of 7?";
            fc3.Answer = "Because 7 8 9.";

            // Act
            fc1 = await networkStore.SaveAsync(fc1);
            fc2 = await networkStore.SaveAsync(fc2);
            fc3 = await networkStore.SaveAsync(fc3);
            var firstResponse = await store.SyncAsync();

            var firstDeleteResponse = await store.RemoveAsync(fc1.ID);
            var secondResponse = await store.SyncAsync();
            var firstStoreCount = (await store.FindAsync()).Count;

            var secondDeleteResponse = await store.RemoveAsync(fc2.ID);
            var thirdDeleteResponse = await store.RemoveAsync(fc3.ID);
            var thirdResponse = await store.SyncAsync();

            var localEntities = await store.FindAsync();
            if (localEntities != null)
            {
                foreach (var localEntity in localEntities)
                {
                    await store.RemoveAsync(localEntity.ID);
                }

                await store.SyncAsync();
            }

            // Assert
            Assert.AreEqual(3, firstResponse.PullResponse.PullCount);
            Assert.AreEqual(0, secondResponse.PullResponse.PullCount);
            Assert.AreEqual(1, firstDeleteResponse.count);
            Assert.AreEqual(0, thirdResponse.PullResponse.PullCount);
            Assert.AreEqual(2, secondDeleteResponse.count + thirdDeleteResponse.count);
        }

        //with enabled deltaset should return correct number of items when deleting and updating
        [TestMethod]
        public async Task TestDeltaSetSyncReturnCorrectNumberOfDeletesAndUpdates()
        {
            // Setup
            if (MockData)
            {
                MockResponses(13, kinveyClient);
            }

            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.CACHE);
            var networkStore = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.NETWORK);
            store.DeltaSetFetchingEnabled = true;

            var fc1 = new FlashCard();
            fc1.Question = "What is 2 + 5?";
            fc1.Answer = "7";

            var fc2 = new FlashCard();
            fc2.Question = "What is 3 + 5";
            fc2.Answer = "8";

            var fc3 = new FlashCard();
            fc3.Question = "Why is 6 afraid of 7?";
            fc3.Answer = "Because 7 8 9.";

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

            var localEntities = await store.FindAsync();
            if (localEntities != null)
            {
                foreach (var localEntity in localEntities)
                {
                    await store.RemoveAsync(localEntity.ID);
                }

                await store.SyncAsync();
            }

            // Assert
            Assert.AreEqual(3, firstResponse.PullResponse.PullCount);
            Assert.AreEqual(1, secondResponse.PullResponse.PullCount);
            Assert.AreEqual(1, deleteResponse.count);
        }

        //[Test(Description = "with enabled deltaset and tagged datastore should use the tagged cache collection")]

        //[Test(Description = "with enabled deltaset and autopagination should use AP for first request and DS for the next")]

        //[Test(Description = "with enable deltaset and limit and skip should not use deltaset and should not override lastRunAt")]

        //[Test(Description = "with enable deltaset and limit and skip should not use deltaset and should not cause inconsistent data")]

        #endregion

        #endregion
	}
}

