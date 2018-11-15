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
using Moq.Protected;

using Kinvey;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net.Http;

namespace Kinvey.Tests
{
    [TestClass]
    public class DataStoreNetworkIntegrationTests : BaseTestClass
	{
		private Client kinveyClient;

		private const string collectionName = "ToDos";

        [TestInitialize]
        public override void Setup()
		{
            base.Setup();

            kinveyClient?.ActiveUser?.Logout();
            System.IO.File.Delete(TestSetup.SQLiteOfflineStoreFilePath);
            System.IO.File.Delete(TestSetup.SQLiteCredentialStoreFilePath);

            Client.Builder builder = ClientBuilder.setFilePath(TestSetup.db_dir);

            if (MockData)
            {
                builder.setBaseURL("http://localhost:8080");
            }

			kinveyClient = builder.Build();
		}

        [TestCleanup]
        public override void Tear()
		{
			kinveyClient.ActiveUser?.Logout();
			System.IO.File.Delete(TestSetup.SQLiteOfflineStoreFilePath);
			System.IO.File.Delete(TestSetup.SQLiteCredentialStoreFilePath);

            base.Tear();
		}

        [TestMethod]
        public async Task TestACLGloballyReadableSave()
        {
            // Setup
            if (MockData)
            {
                MockResponses(3);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            AccessControlList acl = new AccessControlList();
            acl.GloballyReadable = true;
            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK, kinveyClient);
            ToDo todo = new ToDo();
            todo.Acl = acl;

            // Act
            ToDo savedToDo = await todoStore.SaveAsync(todo);

            // Assert
            Assert.AreEqual(true, acl.GloballyReadable);
            Assert.AreEqual(false, acl.GloballyWriteable);
            Assert.AreNotEqual(todo.Acl, savedToDo.Acl);
            Assert.AreNotEqual(todo.Acl.Creator, savedToDo.Acl.Creator);
            Assert.AreEqual(todo.Acl.GloballyReadable, savedToDo.Acl.GloballyReadable);
            Assert.AreEqual(todo.Acl.GloballyWriteable, savedToDo.Acl.GloballyWriteable);

            // Teardown
            await todoStore.RemoveAsync(savedToDo.ID);
            kinveyClient.ActiveUser.Logout();
        }

        [TestMethod]
        public async Task TestACLGloballyWriteableSave()
        {
            // Setup
            if (MockData)
            {
                MockResponses(3);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            AccessControlList acl = new AccessControlList();
            acl.GloballyWriteable = true;
            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK, kinveyClient);
            ToDo todo = new ToDo();
            todo.Acl = acl;

            // Act
            ToDo savedToDo = await todoStore.SaveAsync(todo);

            // Assert
            Assert.AreEqual(false, acl.GloballyReadable);
            Assert.AreEqual(true, acl.GloballyWriteable);
            Assert.AreNotEqual(todo.Acl, savedToDo.Acl);
            Assert.AreNotEqual(todo.Acl.Creator, savedToDo.Acl.Creator);
            Assert.AreEqual(todo.Acl.GloballyReadable, savedToDo.Acl.GloballyReadable);
            Assert.AreEqual(todo.Acl.GloballyWriteable, savedToDo.Acl.GloballyWriteable);

            // Teardown
            await todoStore.RemoveAsync(savedToDo.ID);
            kinveyClient.ActiveUser.Logout();
        }

        [TestMethod]
        public async Task TestACLGroupReadListSave()
        {
            // Setup
            if (MockData)
            {
                MockResponses(3);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            AccessControlList acl = new AccessControlList();
            acl.Groups.Readers.Add("groupread1");
            acl.Groups.Readers.Add("groupread2");
            acl.Groups.Readers.Add("groupread3");
            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK, kinveyClient);
            ToDo todo = new ToDo();
            todo.Acl = acl;

            // Act
            ToDo savedToDo = await todoStore.SaveAsync(todo);

            // Assert
            Assert.AreEqual(false, acl.GloballyReadable);
            Assert.AreEqual(false, acl.GloballyWriteable);
            Assert.AreNotEqual(todo.Acl, savedToDo.Acl);
            Assert.AreNotEqual(todo.Acl.Creator, savedToDo.Acl.Creator);
            Assert.AreEqual(todo.Acl.GloballyReadable, savedToDo.Acl.GloballyReadable);
            Assert.AreEqual(todo.Acl.GloballyWriteable, savedToDo.Acl.GloballyWriteable);
            Assert.IsNotNull(savedToDo.Acl.Readers);
            Assert.IsNotNull(savedToDo.Acl.Writers);
            Assert.IsNotNull(savedToDo.Acl.Groups);
            Assert.IsNotNull(savedToDo.Acl.Groups.Writers);
            Assert.IsNotNull(savedToDo.Acl.Groups.Readers);
            CollectionAssert.AreEqual(todo.Acl.Groups.Readers, savedToDo.Acl.Groups.Readers);
            Assert.AreEqual(3, savedToDo.Acl.Groups.Readers.Count);
            Assert.IsTrue(savedToDo.Acl.Groups.Readers[0].Equals("groupread1"));
            Assert.IsTrue(savedToDo.Acl.Groups.Readers[1].Equals("groupread2"));
            Assert.IsTrue(savedToDo.Acl.Groups.Readers[2].Equals("groupread3"));

            // Teardown
            await todoStore.RemoveAsync(savedToDo.ID);
            kinveyClient.ActiveUser.Logout();
        }

        [TestMethod]
        public async Task TestACLGroupWriteListSave()
        {
            // Setup
            if (MockData)
            {
                MockResponses(3);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            AccessControlList acl = new AccessControlList();
            acl.Groups.Writers.Add("groupwrite1");
            acl.Groups.Writers.Add("groupwrite2");
            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK, kinveyClient);
            ToDo todo = new ToDo();
            todo.Acl = acl;

            // Act
            ToDo savedToDo = await todoStore.SaveAsync(todo);

            // Assert
            Assert.AreEqual(false, acl.GloballyReadable);
            Assert.AreEqual(false, acl.GloballyWriteable);
            Assert.AreNotEqual(todo.Acl, savedToDo.Acl);
            Assert.AreNotEqual(todo.Acl.Creator, savedToDo.Acl.Creator);
            Assert.AreEqual(todo.Acl.GloballyReadable, savedToDo.Acl.GloballyReadable);
            Assert.AreEqual(todo.Acl.GloballyWriteable, savedToDo.Acl.GloballyWriteable);
            Assert.IsNotNull(savedToDo.Acl.Readers);
            Assert.IsNotNull(savedToDo.Acl.Writers);
            Assert.IsNotNull(savedToDo.Acl.Groups);
            Assert.IsNotNull(savedToDo.Acl.Groups.Readers);
            Assert.IsNotNull(savedToDo.Acl.Groups.Writers);
            CollectionAssert.AreEqual(todo.Acl.Groups.Writers, savedToDo.Acl.Groups.Writers);
            Assert.AreEqual(2, savedToDo.Acl.Groups.Writers.Count);
            Assert.IsTrue(savedToDo.Acl.Groups.Writers[0].Equals("groupwrite1"));
            Assert.IsTrue(savedToDo.Acl.Groups.Writers[1].Equals("groupwrite2"));

            // Teardown
            await todoStore.RemoveAsync(savedToDo.ID);
            kinveyClient.ActiveUser.Logout();
        }

        [TestMethod]
        public async Task TestACLReadListSave()
        {
            // Setup
            if (MockData)
            {
                MockResponses(3);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            AccessControlList acl = new AccessControlList();
            acl.Readers.Add("reader1");
            acl.Readers.Add("reader2");
            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK, kinveyClient);
            ToDo todo = new ToDo();
            todo.Acl = acl;

            // Act
            ToDo savedToDo = await todoStore.SaveAsync(todo);

            // Assert
            Assert.AreEqual(false, acl.GloballyReadable);
            Assert.AreEqual(false, acl.GloballyWriteable);
            Assert.AreNotEqual(todo.Acl, savedToDo.Acl);
            Assert.AreNotEqual(todo.Acl.Creator, savedToDo.Acl.Creator);
            Assert.AreEqual(todo.Acl.GloballyReadable, savedToDo.Acl.GloballyReadable);
            Assert.AreEqual(todo.Acl.GloballyWriteable, savedToDo.Acl.GloballyWriteable);
            Assert.IsNotNull(savedToDo.Acl.Writers);
            Assert.IsNotNull(savedToDo.Acl.Readers);
            CollectionAssert.AreEqual(todo.Acl.Readers, savedToDo.Acl.Readers);
            Assert.AreEqual(2, savedToDo.Acl.Readers.Count);
            Assert.IsTrue(savedToDo.Acl.Readers[0].Equals("reader1"));
            Assert.IsTrue(savedToDo.Acl.Readers[1].Equals("reader2"));

            // Teardown
            await todoStore.RemoveAsync(savedToDo.ID);
            kinveyClient.ActiveUser.Logout();
        }

        [TestMethod]
        public async Task TestACLWriteListSave()
        {
            // Setup
            if (MockData)
            {
                MockResponses(3);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            AccessControlList acl = new AccessControlList();
            acl.Writers.Add("writer1");
            acl.Writers.Add("writer2");
            acl.Writers.Add("writer3");
            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK, kinveyClient);
            ToDo todo = new ToDo();
            todo.Acl = acl;

            // Act
            ToDo savedToDo = await todoStore.SaveAsync(todo);

            // Assert
            Assert.AreEqual(false, acl.GloballyReadable);
            Assert.AreEqual(false, acl.GloballyWriteable);
            Assert.AreNotEqual(todo.Acl, savedToDo.Acl);
            Assert.AreNotEqual(todo.Acl.Creator, savedToDo.Acl.Creator);
            Assert.AreEqual(todo.Acl.GloballyReadable, savedToDo.Acl.GloballyReadable);
            Assert.AreEqual(todo.Acl.GloballyWriteable, savedToDo.Acl.GloballyWriteable);
            Assert.IsNotNull(savedToDo.Acl.Readers);
            Assert.IsNotNull(savedToDo.Acl.Writers);
            CollectionAssert.AreEqual(todo.Acl.Writers, savedToDo.Acl.Writers);
            Assert.AreEqual(3, savedToDo.Acl.Writers.Count);
            Assert.IsTrue(savedToDo.Acl.Writers[0].Equals("writer1"));
            Assert.IsTrue(savedToDo.Acl.Writers[1].Equals("writer2"));
            Assert.IsTrue(savedToDo.Acl.Writers[2].Equals("writer3"));

            // Teardown
            await todoStore.RemoveAsync(savedToDo.ID);
            kinveyClient.ActiveUser.Logout();
        }

        [TestMethod]
		public async Task TestCollection()
		{
			// Arrange

			// Act
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName);

			// Assert
			Assert.IsNotNull(todoStore);
			Assert.AreEqual(todoStore.CollectionName, collectionName);
			Assert.AreEqual(todoStore.StoreType, DataStoreType.CACHE);
		}

        [TestMethod]
        public async Task TestCollectionSharedClient()
        {
            // Arrange

            // Act
            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

            // Assert
            Assert.IsNotNull(todoStore);
            Assert.IsTrue(string.Equals(todoStore.CollectionName, collectionName));
        }

        [TestMethod]
		public async Task TestCollectionStoreType()
		{
			// Arrange

			// Act
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK, kinveyClient);

			// Assert
			Assert.IsNotNull(todoStore);
			Assert.AreEqual(todoStore.CollectionName, collectionName);
			Assert.AreEqual(todoStore.StoreType, DataStoreType.NETWORK);

		}

        [TestMethod]
        public async Task TestDeleteAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(3);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);
            ToDo newItem = new ToDo();
            newItem.Name = "Task to Delete";
            newItem.Details = "A delete test";
            ToDo deleteToDo = await todoStore.SaveAsync(newItem);

            // Act
            KinveyDeleteResponse kdr = await todoStore.RemoveAsync(deleteToDo.ID);

            // Assert
            Assert.IsNotNull(kdr);
            Assert.AreEqual(1, kdr.count);

            // Teardown
            kinveyClient.ActiveUser.Logout();
        }

        [TestMethod]
		public void TestDeltaSetFetchEnable()
		{
			// Arrange
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

			// Act
			todoStore.DeltaSetFetchingEnabled = true;

			// Assert
			Assert.IsTrue(todoStore.DeltaSetFetchingEnabled);
		}

        [TestMethod]
        public async Task TestGetCountAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(6);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            ToDo newItem = new ToDo();
            newItem.Name = "Next Task";
            newItem.Details = "A test";
            newItem.DueDate = "2016-04-19T20:02:17.635Z";

            ToDo newItem2 = new ToDo();
            newItem2.Name = "another todo";
            newItem2.Details = "details for 2";
            newItem2.DueDate = "2016-04-22T19:56:00.963Z";


            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);
            ToDo t1 = await todoStore.SaveAsync(newItem);
            ToDo t2 = await todoStore.SaveAsync(newItem2);

            // Act
            var count = 0u;
            count = await todoStore.GetCountAsync();

            // Assert
            //Assert.GreaterOrEqual(count, 0);
            Assert.AreEqual(2u, count);

            // Teardown
            await todoStore.RemoveAsync(t1.ID);
            await todoStore.RemoveAsync(t2.ID);
            kinveyClient.ActiveUser.Logout();
        }

        [TestMethod]
        public async Task TestNetworkStoreFindAsync()
        {
            // Setup
            if (MockData)
            {
                MockResponses(6);
            }
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            ToDo newItem = new ToDo();
            newItem.Name = "Next Task";
            newItem.Details = "A test";
            newItem.DueDate = "2016-04-19T20:02:17.635Z";
            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);
            ToDo t = await todoStore.SaveAsync(newItem);

            ToDo anotherNewItem = new ToDo();
            anotherNewItem.Name = "Another Next Task";
            anotherNewItem.Details = "Another test";
            anotherNewItem.DueDate = "2016-05-19T20:02:17.635Z";
            ToDo t2 = await todoStore.SaveAsync(anotherNewItem);

            // Act
            List<ToDo> todoList = new List<ToDo>();

            todoList = await todoStore.FindAsync();

            // Assert
            Assert.IsNotNull(todoList);
            Assert.AreEqual(2, todoList.Count);

            // Teardown
            await todoStore.RemoveAsync(t.ID);
            await todoStore.RemoveAsync(t2.ID);
            kinveyClient.ActiveUser.Logout();
        }

        [TestMethod]
		public async Task TestNetworkStoreFindAsyncBad()
		{
		    // Setup
            if (MockData)
            {
                MockResponses(1);
            }
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var moqRC = new Mock<HttpClientHandler>(MockBehavior.Strict);
            HttpRequestMessage request = null;
            moqRC
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Callback<HttpRequestMessage, CancellationToken>((req, token) => request = req)
                .ReturnsAsync(() => new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("MOCK RESPONSE"),
                    RequestMessage = request
                })
                .Verifiable();

			Client.Builder cb = new Client.Builder(TestSetup.app_key, TestSetup.app_secret)
				.SetFilePath(TestSetup.db_dir)
                .SetRestClient(new HttpClient(moqRC.Object));

			Client c = cb.Build();

			// Arrange
			DataStore<ToDo> store = DataStore<ToDo>.Collection("todos", DataStoreType.NETWORK, c);

			// Act
			// Assert
            Exception er = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate ()
			{
				await store.FindAsync();
			});

			Assert.IsNotNull(er);
			KinveyException ke = er as KinveyException;
            Assert.AreEqual(EnumErrorCode.ERROR_JSON_PARSE, ke.ErrorCode);

			// Teardown
			c.ActiveUser.Logout();
		}

        [TestMethod]
		public async Task TestNetworkStoreFindByIDAsync()
		{
			// Setup
            if (MockData)
            {
                MockResponses(4);
            }
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "Next Task";
			newItem.Details = "A test";
			newItem.DueDate = "2016-04-19T20:02:17.635Z";
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);
			ToDo t = await todoStore.SaveAsync(newItem);

			// Act
			ToDo entity = null;
			entity = await todoStore.FindByIDAsync(t.ID);

			// Assert
			Assert.IsNotNull(entity);
			Assert.IsTrue(string.Equals(entity.ID, t.ID));

			// Teardown
			await todoStore.RemoveAsync(t.ID);
			kinveyClient.ActiveUser.Logout();
		}

        [TestMethod]
        public async Task TestNetworkStoreFindByMongoQuery()
        {
            // Setup
            if (MockData)
            {
                MockResponses(6);
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

            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);

            // Act
            string mongoQuery = "{\"details\":\"details for 2\"}";
            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindWithMongoQueryAsync(mongoQuery);

            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
            Assert.AreEqual(1, listToDo.Count);
            Assert.IsNotNull(listToDo[0].Name);
            Assert.IsNotNull(listToDo[0].Details);
            Assert.IsTrue(listToDo[0].Details.Equals("details for 2"));
        }

        [TestMethod]
		public async Task TestNetworkStoreFindByQuery()
		{
			// Setup
            if (MockData)
            {
                MockResponses(6);
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

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);

			// Act
			//var query = from todo in todoStore
			//			where todo.Details.StartsWith("deta")
			//	                      where todo.Name.StartsWith("todo a")
			//			select todo;

			//var query = from todo in todoStore
			//			where todo.Details.StartsWith("deta") && todo.Name.StartsWith("todo a")
			//			select todo;

			//var query = todoStore.Where(x => x.Details.StartsWith("det")).Where(y => y.Name.StartsWith("anoth"));

			var query = todoStore.Where(x => x.Details.StartsWith("det"));

			List<ToDo> listToDo = new List<ToDo>();

			listToDo = await todoStore.FindAsync(query);

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.IsNotNull(listToDo);
            Assert.AreNotEqual(listToDo.Count, 0);
			Assert.AreEqual(2, listToDo.Count);
		}

        [TestMethod]
		public async Task TestNetworkStoreFindByQueryBoolValueExplicit()
		{
			// Setup
            if (MockData)
            {
                MockResponses(6);
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
			newItem2.BoolVal = true;

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.NETWORK);

			var query = from e in todoStore
						where e.Name == "another todo" || e.BoolVal == true
						select e;

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);

			//var query = todoStore.Where(x => x.BoolVal.Equals(true));

			List<ToDo> listToDo = new List<ToDo>();

			listToDo = await todoStore.FindAsync(query);

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
			Assert.AreEqual(1, listToDo.Count);
		}

        [TestMethod]
        public async Task TestNetworkStoreFindByQueryBoolValueExplicitEqualsExpression()
        {
            // Setup
            if (MockData)
            {
                MockResponses(6);
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
            newItem2.BoolVal = true;

            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.NETWORK);

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);

            var query = todoStore.Where(x => x.BoolVal.Equals(true));

            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindAsync(query);

            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
            Assert.AreEqual(1, listToDo.Count);
        }

        [TestMethod]
        public async Task TestNetworkStoreFindByQueryBoolValueImplicit()
        {
            // Setup
            if (MockData)
            {
                MockResponses(6);
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
            newItem2.BoolVal = true;

            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.NETWORK);

            var query = from e in todoStore
                        where e.BoolVal
                        select e;

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);

            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindAsync(query);

            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
            Assert.AreEqual(1, listToDo.Count);
        }

        [TestMethod]
        public async Task TestNetworkStoreFindByQueryInequalityDateTimeObjectGreaterThan()
        {
            // Setup
            if (MockData)
            {
                MockResponses(8);
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
            newItem1.NewDate = new DateTime(2016, 4, 21, 19, 56, 0);

            ToDo newItem2 = new ToDo();
            newItem2.Name = "another todo";
            newItem2.Details = "details for 2";
            newItem2.DueDate = "2016-04-22T19:56:00.963Z";
            newItem2.NewDate = new DateTime(2016, 4, 22, 19, 56, 0);

            ToDo newItem3 = new ToDo();
            newItem3.Name = "another todo";
            newItem3.Details = "details for 2";
            newItem3.DueDate = "2016-04-22T19:56:00.963Z";
            newItem3.NewDate = new DateTime(2017, 4, 22, 19, 56, 0);

            var endDate = new DateTime(2017, 1, 1, 0, 0, 0);
            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.NETWORK);

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);
            newItem3 = await todoStore.SaveAsync(newItem3);

            // Act
            var query = todoStore.Where(x => x.NewDate > endDate);

            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindAsync(query);

            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            await todoStore.RemoveAsync(newItem3.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.AreNotEqual(listToDo.Count, 0);
            Assert.AreEqual(1, listToDo.Count);
        }

        [TestMethod]
        public async Task TestNetworkStoreFindByQueryInequalityDateTimeObjectGreaterThanOrEqual()
        {
            // Setup
            if (MockData)
            {
                MockResponses(8);
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
            newItem1.NewDate = new DateTime(2016, 4, 21, 19, 56, 0);

            ToDo newItem2 = new ToDo();
            newItem2.Name = "another todo";
            newItem2.Details = "details for 2";
            newItem2.DueDate = "2016-04-22T19:56:00.963Z";
            newItem2.NewDate = new DateTime(2016, 4, 22, 19, 56, 0);

            ToDo newItem3 = new ToDo();
            newItem3.Name = "another todo";
            newItem3.Details = "details for 2";
            newItem3.DueDate = "2016-04-22T19:56:00.963Z";
            newItem3.NewDate = new DateTime(2017, 1, 1, 0, 0, 0);

            var endDate = new DateTime(2017, 1, 1, 0, 0, 0);
            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.NETWORK);

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);
            newItem3 = await todoStore.SaveAsync(newItem3);

            // Act
            var query = todoStore.Where(x => x.NewDate >= endDate);

            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindAsync(query);

            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            await todoStore.RemoveAsync(newItem3.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.AreNotEqual(listToDo.Count, 0);
            Assert.AreEqual(1, listToDo.Count);
        }

        [TestMethod]
        public async Task TestNetworkStoreFindByQueryInequalityDateTimeObjectLessThan()
        {
            // Setup
            if (MockData)
            {
                MockResponses(8);
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
            newItem1.NewDate = new DateTime(2016, 4, 21, 19, 56, 0);

            ToDo newItem2 = new ToDo();
            newItem2.Name = "another todo";
            newItem2.Details = "details for 2";
            newItem2.DueDate = "2016-04-22T19:56:00.963Z";
            newItem2.NewDate = new DateTime(2016, 4, 22, 19, 56, 0);

            ToDo newItem3 = new ToDo();
            newItem3.Name = "another todo";
            newItem3.Details = "details for 2";
            newItem3.DueDate = "2016-04-22T19:56:00.963Z";
            newItem3.NewDate = new DateTime(2017, 1, 1, 0, 0, 1);

            var endDate = new DateTime(2017, 1, 1, 0, 0, 0);
            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.NETWORK);

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);
            newItem3 = await todoStore.SaveAsync(newItem3);

            // Act
            var query = todoStore.Where(x => x.NewDate < endDate);

            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindAsync(query);

            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            await todoStore.RemoveAsync(newItem3.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
            Assert.AreEqual(2, listToDo.Count);
        }

        [TestMethod]
        public async Task TestNetworkStoreFindByQueryInequalityDateTimeObjectLessThanOrEqual()
        {
            // Setup
            if (MockData)
            {
                MockResponses(8);
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
            newItem1.NewDate = new DateTime(2016, 4, 21, 19, 56, 0);

            ToDo newItem2 = new ToDo();
            newItem2.Name = "another todo";
            newItem2.Details = "details for 2";
            newItem2.DueDate = "2016-04-22T19:56:00.963Z";
            newItem2.NewDate = new DateTime(2016, 4, 22, 19, 56, 0);

            ToDo newItem3 = new ToDo();
            newItem3.Name = "another todo";
            newItem3.Details = "details for 2";
            newItem3.DueDate = "2016-04-22T19:56:00.963Z";
            newItem3.NewDate = new DateTime(2017, 1, 1, 0, 0, 0);

            var endDate = new DateTime(2017, 1, 1, 0, 0, 0);
            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.NETWORK);

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);
            newItem3 = await todoStore.SaveAsync(newItem3);

            // Act
            var query = todoStore.Where(x => x.NewDate <= endDate);

            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindAsync(query);

            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            await todoStore.RemoveAsync(newItem3.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
            Assert.AreEqual(3, listToDo.Count);
        }

        [TestMethod]
		public async Task TestNetworkStoreFindByQueryInequalityGreaterThan()
		{
			// Setup
            if (MockData)
            {
                MockResponses(8);
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
			newItem1.Value = 1;

			ToDo newItem2 = new ToDo();
			newItem2.Name = "another todo";
			newItem2.Details = "details for 2";
			newItem2.DueDate = "2016-04-22T19:56:00.963Z";
			newItem2.Value = 2;

			ToDo newItem3 = new ToDo();
			newItem3.Name = "another todo";
			newItem3.Details = "details for 2";
			newItem3.DueDate = "2016-04-22T19:56:00.963Z";
			newItem3.Value = 2;

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.NETWORK);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);
			newItem3 = await todoStore.SaveAsync(newItem3);

			// Act
			var query = todoStore.Where(x => x.Value > 1);

			List<ToDo> listToDo = new List<ToDo>();

			listToDo = await todoStore.FindAsync(query);

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			await todoStore.RemoveAsync(newItem3.ID);
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
			Assert.AreEqual(2, listToDo.Count);
		}

        [TestMethod]
		public async Task TestNetworkStoreFindByQueryInequalityGreaterThanOrEqual()
		{
			// Setup
            if (MockData)
            {
                MockResponses(8);
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
			newItem1.Value = 1;

			ToDo newItem2 = new ToDo();
			newItem2.Name = "another todo";
			newItem2.Details = "details for 2";
			newItem2.DueDate = "2016-04-22T19:56:00.963Z";
			newItem2.Value = 2;

			ToDo newItem3 = new ToDo();
			newItem3.Name = "another todo";
			newItem3.Details = "details for 2";
			newItem3.DueDate = "2016-04-22T19:56:00.963Z";
			newItem3.Value = 2;

			//var endDate = new DateTime(2017, 1, 1, 0, 0, 0);
			//string end_date = "2016-04-22T19:56:00.963Z";
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.NETWORK);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);
			newItem3 = await todoStore.SaveAsync(newItem3);

			// Act
			var query = todoStore.Where(x => x.Value >= 2);

			List<ToDo> listToDo = new List<ToDo>();

			listToDo = await todoStore.FindAsync(query);

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			await todoStore.RemoveAsync(newItem3.ID);
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
			Assert.AreEqual(2, listToDo.Count);
		}

        [TestMethod]
		public async Task TestNetworkStoreFindByQueryInequalityLessThan()
		{
			// Setup
            if (MockData)
            {
                MockResponses(8);
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
			newItem1.Value = 1;

			ToDo newItem2 = new ToDo();
			newItem2.Name = "another todo";
			newItem2.Details = "details for 2";
			newItem2.DueDate = "2016-04-22T19:56:00.963Z";
			newItem2.Value = 2;

			ToDo newItem3 = new ToDo();
			newItem3.Name = "another todo";
			newItem3.Details = "details for 2";
			newItem3.DueDate = "2016-04-22T19:56:00.963Z";
			newItem3.Value = 2;

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.NETWORK);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);
			newItem3 = await todoStore.SaveAsync(newItem3);

			// Act
			var query = todoStore.Where(x => x.Value < 2);

			List<ToDo> listToDo = new List<ToDo>();

			listToDo = await todoStore.FindAsync(query);

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			await todoStore.RemoveAsync(newItem3.ID);
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
			Assert.AreEqual(1, listToDo.Count);
		}

        [TestMethod]
		public async Task TestNetworkStoreFindByQueryInequalityLessThanOrEqual()
		{
			// Setup
            if (MockData)
            {
                MockResponses(8);
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
			newItem1.Value = 1;

			ToDo newItem2 = new ToDo();
			newItem2.Name = "another todo";
			newItem2.Details = "details for 2";
			newItem2.DueDate = "2016-04-22T19:56:00.963Z";
			newItem2.Value = 2;

			ToDo newItem3 = new ToDo();
			newItem3.Name = "another todo";
			newItem3.Details = "details for 2";
			newItem3.DueDate = "2016-04-22T19:56:00.963Z";
			newItem3.Value = 3;

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.NETWORK);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);
			newItem3 = await todoStore.SaveAsync(newItem3);

			// Act
			var query = todoStore.Where(x => x.Value <= 2);

			List<ToDo> listToDo = new List<ToDo>();

			listToDo = await todoStore.FindAsync(query);

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			await todoStore.RemoveAsync(newItem3.ID);
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
			Assert.AreEqual(2, listToDo.Count);
		}

        [TestMethod]
        public async Task TestNetworkStoreFindByQueryIntValue()
        {
            // Setup
            if (MockData)
            {
                MockResponses(6);
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
            newItem2.Value = 1;

            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);

            var query = todoStore.Where(x => x.Value.Equals(1));

            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindAsync(query);

            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
            Assert.AreEqual(1, listToDo.Count);
        }

        [TestMethod]
        public async Task TestNetworkStoreFindByQueryLogicalAnd()
        {
            // Setup
            if (MockData)
            {
                MockResponses(6);
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
            newItem2.Name = "todo again";
            newItem2.Details = "details for 2";
            newItem2.DueDate = "2016-04-22T19:56:00.963Z";

            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);

            // Act
            var query = from todo in todoStore
                        where todo.Details.StartsWith("deta") && todo.Name.Equals("todo")
                        select todo;

            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindAsync(query);


            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
            Assert.AreEqual(1, listToDo.Count);
        }

        [TestMethod]
        public async Task TestNetworkStoreFindByQueryLogicalAndWithOr()
        {
            // Setup
            if (MockData)
            {
                MockResponses(6);
            }
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            ToDo newItem1 = new ToDo();
            newItem1.Name = "a todo";
            newItem1.Details = "details for 1";
            newItem1.DueDate = "2016-04-22T19:56:00.963Z";

            ToDo newItem2 = new ToDo();
            newItem2.Name = "b todo again";
            newItem2.Details = "details for 2";
            newItem2.DueDate = "2016-04-22T19:56:00.963Z";

            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);

            // Act
            var query = from todo in todoStore
                        where todo.DueDate.Equals("2016-04-22T19:56:00.963Z") && (todo.Name.StartsWith("a to") || todo.Details.Equals("details for 2"))
                        select todo;

            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindAsync(query);


            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
            Assert.AreEqual(2, listToDo.Count);
        }

        [TestMethod]
        public async Task TestNetworkStoreFindByQueryLogicalOr()
        {
            // Setup
            if (MockData)
            {
                MockResponses(6);
            }
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            ToDo newItem1 = new ToDo();
            newItem1.Name = "a todo";
            newItem1.Details = "details for 1";
            newItem1.DueDate = "2016-04-22T19:56:00.963Z";

            ToDo newItem2 = new ToDo();
            newItem2.Name = "b todo again";
            newItem2.Details = "details for 2";
            newItem2.DueDate = "2016-04-22T19:56:00.963Z";

            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);

            // Act
            var query = from todo in todoStore
                        where todo.Name.StartsWith("a to") || todo.Details.Equals("details for 2")
                        select todo;

            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindAsync(query);


            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
            Assert.AreEqual(2, listToDo.Count);
        }

        [TestMethod]
        public async Task TestNetworkStoreFindByQueryLogicalOrWithAnd()
        {
            // Setup
            if (MockData)
            {
                MockResponses(6);
            }
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            ToDo newItem1 = new ToDo();
            newItem1.Name = "a todo";
            newItem1.Details = "details for 1";
            newItem1.DueDate = "2016-04-22T19:56:00.963Z";

            ToDo newItem2 = new ToDo();
            newItem2.Name = "b todo again";
            newItem2.Details = "details for 2";
            newItem2.DueDate = "2016-04-22T19:56:00.963Z";

            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);

            // Act
            var query = from todo in todoStore
                        where (todo.Name.StartsWith("b to") || todo.DueDate.Equals("2016-04-22T19:56:00.963Z") && todo.Details.Equals("details for 2"))
                        select todo;

            var expectedResults =
                        from todo in new ToDo[] { newItem1, newItem2 }
                        where (todo.Name.StartsWith("b to") || todo.DueDate.Equals("2016-04-22T19:56:00.963Z") && todo.Details.Equals("details for 2"))
                        select todo;

            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindAsync(query);


            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
            Assert.AreEqual(expectedResults.Count(), listToDo.Count);
        }

        [TestMethod]
        public async Task TestNetworkStoreFindByQueryMultipleWhereClauses()
        {
            // Setup
            if (MockData)
            {
                MockResponses(6);
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
            newItem2.Name = "todo again";
            newItem2.Details = "details for 2";
            newItem2.DueDate = "2016-04-22T19:56:00.963Z";

            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);

            // Act
            var query = todoStore.Where(x => x.Details.StartsWith("det")).Where(y => y.Name.StartsWith("todo a")).Where(z => z.DueDate.Equals("2016-04-22T19:56:00.963Z"));

            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindAsync(query);


            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
            Assert.AreEqual(1, listToDo.Count);
        }

        [TestMethod]
        public async Task TestNetworkStoreFindByQueryMultipleWhereClausesEquals()
        {
            // Setup
            if (MockData)
            {
                MockResponses(6);
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
            newItem2.Name = "todo again";
            newItem2.Details = "details for 2";
            newItem2.DueDate = "2016-04-22T19:56:00.963Z";

            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);

            // Act
            var query = todoStore.Where(x => x.Details.Equals("details for 2")).Where(y => y.Name.Equals("todo again")).Where(z => z.DueDate.Equals("2016-04-22T19:56:00.963Z"));

            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindAsync(query);


            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
            Assert.AreEqual(1, listToDo.Count);
        }

        [TestMethod]
        public async Task TestNetworkStoreFindByQueryMultipleWhereClausesEqualSign()
        {
            // Setup
            if (MockData)
            {
                MockResponses(6);
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
            newItem2.Name = "todo again";
            newItem2.Details = "details for 2";
            newItem2.DueDate = "2016-04-22T19:56:00.963Z";

            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);

            // Act
            var query = todoStore.Where(x => x.Details == ("details for 2")).Where(y => y.Name == ("todo again")).Where(z => z.DueDate.Equals("2016-04-22T19:56:00.963Z"));

            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindAsync(query);


            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
            Assert.AreEqual(1, listToDo.Count);
        }

        [TestMethod]
        public async Task TestNetworkStoreFindByQueryMultipleWhereClausesFluentSyntaxEqualSign()
        {
            // Setup
            if (MockData)
            {
                MockResponses(6);
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
            newItem2.Name = "todo again";
            newItem2.Details = "details for 2";
            newItem2.DueDate = "2016-04-22T19:56:00.963Z";

            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);

            // Act
            var query = from t in todoStore where t.Details == "details for 2" where t.Name == "todo again" where t.DueDate == "2016-04-22T19:56:00.963Z" select t;

            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindAsync(query);


            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
            Assert.AreEqual(1, listToDo.Count);
        }

        [TestMethod]
        public async Task TestNetworkStoreFindByQueryMultipleWhereClausesWithLogicalAnd()
        {
            // Setup
            if (MockData)
            {
                MockResponses(6);
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
            newItem2.Name = "todo again";
            newItem2.Details = "details for 2";
            newItem2.DueDate = "2016-04-22T19:56:00.963Z";

            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);

            // Act
            var query = todoStore.Where(x => x.Details.StartsWith("det") && x.DueDate.Equals("2016-04-22T19:56:00.963Z")).Where(y => y.Name.StartsWith("todo a"));

            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindAsync(query);


            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
            Assert.AreEqual(1, listToDo.Count);
        }

        [TestMethod]
        public async Task TestNetworkStoreFindByQueryMultipleWhereClausesWithLogicalOr()
        {
            // Setup
            if (MockData)
            {
                MockResponses(6);
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
            newItem2.Name = "todo again";
            newItem2.Details = "details for 2";
            newItem2.DueDate = "2016-04-22T19:56:00.963Z";

            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);

            // Act
            var query = todoStore.Where(x => x.Details.StartsWith("det") || x.DueDate.Equals("2016-04-22T19:56:00.963Z")).Where(y => y.Name.StartsWith("todo a"));

            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindAsync(query);


            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
            Assert.AreEqual(1, listToDo.Count);
        }

        [TestMethod]
		public async Task TestNetworkStoreFindByQueryNotSupported()
		{
			// Setup
            if (MockData)
            {
                MockResponses(5);
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

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);

			var query = todoStore.Where(x => true);

			List<ToDo> listToDo = new List<ToDo>();

			// Act
            Exception e = await Assert.ThrowsExceptionAsync<KinveyException>(async delegate
			{
				listToDo = await todoStore.FindAsync(query);
			});

			// Assert
			Assert.IsTrue(e.GetType() == typeof(KinveyException));
			KinveyException ke = e as KinveyException;
			Assert.IsTrue(ke.ErrorCategory == EnumErrorCategory.ERROR_DATASTORE_NETWORK);
			Assert.IsTrue(ke.ErrorCode == EnumErrorCode.ERROR_LINQ_WHERE_CLAUSE_NOT_SUPPORTED);

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			kinveyClient.ActiveUser.Logout();
		}

        [TestMethod]
        public async Task TestNetworkStoreFindByQueryWithLimit()
        {
            // Setup
            if (MockData)
            {
                MockResponses(6);
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

            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);

            // Act
            var query = todoStore.Where(x => x.Details.StartsWith("det")).Take(1);

            var expectedResults = new ToDo[] { newItem1, newItem2 }.Where(x => x.Details.StartsWith("det")).Take(1);

            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindAsync(query);

            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
            Assert.AreEqual(expectedResults.Count(), listToDo.Count);
        }

        [TestMethod]
        public async Task TestNetworkStoreFindByQueryWithSelectField()
        {
            // Setup
            if (MockData)
            {
                MockResponses(7);
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

            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);
            foreach (var item in await todoStore.FindAsync())
            {
                await todoStore.RemoveAsync(item.ID);
            }

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);

            // Act
            var query = from todo in todoStore
                        where todo.Details.StartsWith("deta")
                        select todo.Name;

            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindAsync(query);

            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
            Assert.AreEqual(2, listToDo.Count);
            Assert.IsNotNull(listToDo[0].Name);
            Assert.IsNull(listToDo[0].Details);
            Assert.IsNotNull(listToDo[1].Name);
            Assert.IsNull(listToDo[1].Details);
        }

        [TestMethod]
        public async Task TestNetworkStoreFindByQueryWithSelectFields()
        {
            // Setup
            if (MockData)
            {
                MockResponses(6);
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

            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);

            // Act
            var query = from todo in todoStore
                        where todo.Details.StartsWith("deta")
                        select new { todo.Name, todo.Details };

            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindAsync(query);

            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
            Assert.AreEqual(2, listToDo.Count);
            Assert.IsNotNull(listToDo[0].Name);
            Assert.IsNotNull(listToDo[0].Details);
            Assert.IsNull(listToDo[0].DueDate);
            Assert.IsNotNull(listToDo[1].Name);
            Assert.IsNotNull(listToDo[1].Details);
            Assert.IsNull(listToDo[1].DueDate);
        }

        [TestMethod]
        public async Task TestNetworkStoreFindByQueryWithSelectFieldsBad()
        {
            // Setup
            if (MockData)
            {
                MockResponses(6);
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

            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

            newItem1 = await todoStore.SaveAsync(newItem1);
            newItem2 = await todoStore.SaveAsync(newItem2);

            // Act
            var query = from todo in todoStore
                        where todo.Details.StartsWith("deta")
                        select new { };

            List<ToDo> listToDo = new List<ToDo>();

            listToDo = await todoStore.FindAsync(query);

            // Teardown
            await todoStore.RemoveAsync(newItem1.ID);
            await todoStore.RemoveAsync(newItem2.ID);
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
            Assert.AreEqual(2, listToDo.Count);
            Assert.IsNotNull(listToDo[0].Name);
            Assert.IsNotNull(listToDo[0].Details);
            Assert.IsNotNull(listToDo[0].DueDate);
            Assert.IsNotNull(listToDo[1].Name);
            Assert.IsNotNull(listToDo[1].Details);
            Assert.IsNotNull(listToDo[1].DueDate);
        }

        [TestMethod]
		public async Task TestNetworkStoreFindByQueryWithSkip()
		{
			// Setup
            if (MockData)
            {
                MockResponses(6);
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

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);

			// Act
			var query = todoStore.Where(x => x.Details.StartsWith("det")).Skip(1);

			List<ToDo> listToDo = new List<ToDo>();

			listToDo = await todoStore.FindAsync(query);

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.IsNotNull(listToDo);
			Assert.IsTrue(listToDo.Count > 0);
			Assert.AreEqual(1, listToDo.Count);
		}

        [TestMethod]
		public async Task TestNetworkStoreFindByQueryWithSortAscending()
		{
			// Setup
            if (MockData)
            {
                MockResponses(6);
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

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);

			// Act
			var query = todoStore.Where(x => x.Details.StartsWith("det")).OrderBy(x => x.Name);

			List<ToDo> listToDo = new List<ToDo>();

			listToDo = await todoStore.FindAsync(query);

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.IsNotNull(listToDo);
            Assert.IsTrue(listToDo.Count > 0);
			Assert.AreEqual(2, listToDo.Count);
			Assert.IsTrue(String.Compare(newItem2.Name, listToDo.First().Name) == 0);
		}

        [TestMethod]
		public async Task TestNetworkStoreFindByQueryWithSortDescending()
		{
			// Setup
            if (MockData)
            {
                MockResponses(6);
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

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);

			// Act
			var query = todoStore.Where(x => x.Details.StartsWith("det")).OrderByDescending(x => x.Name);

			List<ToDo> listToDo = new List<ToDo>();

			listToDo = await todoStore.FindAsync(query);

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.IsNotNull(listToDo);
			Assert.IsTrue(listToDo.Count > 0);
			Assert.AreEqual(2, listToDo.Count);
			Assert.IsTrue(String.Compare(newItem1.Name, listToDo.First().Name) == 0);
		}

        [TestMethod]
        public async Task TestNetworkStoreGetAverageAsync()
        {
            //Setup
            if (MockData)
            {
                MockResponses(10);
            }

            // Arrange
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            DataStore<Person> personStore = DataStore<Person>.Collection("person", DataStoreType.NETWORK);

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
            int avg = 0;
            List<GroupAggregationResults> arrGAR = await personStore.GroupAndAggregateAsync(EnumReduceFunction.REDUCE_FUNCTION_AVERAGE, "", "Age");
            foreach (var gar in arrGAR)
            {
                avg = gar.Result;
                break;
            }

            // Teardown
            await personStore.RemoveAsync(p4.ID);
            await personStore.RemoveAsync(p3.ID);
            await personStore.RemoveAsync(p2.ID);
            await personStore.RemoveAsync(p1.ID);

            // Assert
            Assert.AreNotEqual(0, avg);
            Assert.AreEqual(30, avg);
            Assert.AreEqual(1, arrGAR.Count());
        }

        [TestMethod]
        public async Task TestNetworkStoreGetMaxAsync()
        {
            //Setup
            if (MockData)
            {
                MockResponses(8);
            }

            // Arrange
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            DataStore<Person> personStore = DataStore<Person>.Collection("person", DataStoreType.NETWORK);

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
            int max = 0;
            List<GroupAggregationResults> arrGAR = await personStore.GroupAndAggregateAsync(EnumReduceFunction.REDUCE_FUNCTION_MAX, "", "Age");
            foreach (var gar in arrGAR)
            {
                max = gar.Result;
                break;
            }

            // Teardown
            await personStore.RemoveAsync(p3.ID);
            await personStore.RemoveAsync(p2.ID);
            await personStore.RemoveAsync(p1.ID);

            // Assert
            Assert.AreNotEqual(0, max);
            Assert.AreEqual(46, max);
            Assert.AreEqual(1, arrGAR.Count());
        }

        [TestMethod]
		public async Task TestNetworkStoreGetMinAsync()
		{
            //Setup
            if (MockData)
            {
                MockResponses(8);
            }

			// Arrange
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			DataStore<Person> personStore = DataStore<Person>.Collection("person", DataStoreType.NETWORK);

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
			int min = 0;
			List<GroupAggregationResults> arrGAR = await personStore.GroupAndAggregateAsync(EnumReduceFunction.REDUCE_FUNCTION_MIN, "", "Age");
			foreach (var gar in arrGAR)
			{
				min = gar.Result;
				break;
			}

			// Teardown
			await personStore.RemoveAsync(p3.ID);
			await personStore.RemoveAsync(p2.ID);
			await personStore.RemoveAsync(p1.ID);

			// Assert
			Assert.AreNotEqual(0, min);
			Assert.AreEqual(15, min);
			Assert.AreEqual(1, arrGAR.Count());
		}

        [TestMethod]
        public async Task TestNetworkStoreGetSumAsync()
        {
            //Setup
            if (MockData)
            {
                MockResponses(8);
            }

            // Arrange
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            DataStore<Person> personStore = DataStore<Person>.Collection("person", DataStoreType.NETWORK);

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
            int sum = 0;
            List<GroupAggregationResults> arrGAR = await personStore.GroupAndAggregateAsync(EnumReduceFunction.REDUCE_FUNCTION_SUM, "LastName", "Age", query);
            foreach (var gar in arrGAR)
            {
                if (gar.GroupField.Equals("Bluth"))
                {
                    sum = gar.Result;
                    break;
                }
            }

            // Teardown
            await personStore.RemoveAsync(p3.ID);
            await personStore.RemoveAsync(p2.ID);
            await personStore.RemoveAsync(p1.ID);

            // Assert
            Assert.AreNotEqual(0, sum);
            Assert.AreEqual(55, sum);
            Assert.AreEqual(1, arrGAR.Count());
        }

		[TestMethod]
		public async Task TestSaveAsync()
		{
            // Setup
            if (MockData)
            {
                MockResponses(3);
            }
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "Next Task";
			newItem.Details = "A test";
			newItem.DueDate = "2016-04-19T20:02:17.635Z";
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

			// Act
			ToDo savedToDo = await todoStore.SaveAsync(newItem);

			// Assert
			Assert.IsNotNull(savedToDo);
			Assert.IsTrue(string.Equals(savedToDo.Name, newItem.Name));

			// Teardown
			await todoStore.RemoveAsync(savedToDo.ID);
			kinveyClient.ActiveUser.Logout();
		}

		[TestMethod]
		public async Task TestStoreInvalidOperation()
		{
            // Setup
            if (MockData)
            {
                MockResponses(1);
            }
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK, kinveyClient);

            await Assert.ThrowsExceptionAsync<KinveyException>(async delegate ()
			{
				await todoStore.PullAsync();
			});

            await Assert.ThrowsExceptionAsync<KinveyException>(async delegate ()
			{
				await todoStore.PushAsync();
			});

            await Assert.ThrowsExceptionAsync<KinveyException>(async delegate ()
			{
				await todoStore.SyncAsync();
			});
		}
	}
}
