
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
using NUnit.Framework;

using Kinvey;

namespace TestFramework
{
	[TestFixture]
	public class DataStoreSyncIntegrationTests
	{
		private Client kinveyClient;

		private const string collectionName = "ToDos";

		[SetUp]
		public void Setup()
		{
            Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret)
                .setFilePath(TestSetup.db_dir);

			kinveyClient = builder.Build();
		}

		[TearDown]
		public void Tear()
		{
			kinveyClient.ActiveUser?.Logout();
			System.IO.File.Delete(TestSetup.SQLiteOfflineStoreFilePath);
			System.IO.File.Delete(TestSetup.SQLiteCredentialStoreFilePath);
		}

		[Test]
		public async Task TestCollection()
		{
			// Arrange

			// Act
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC, kinveyClient);

			// Assert
			Assert.NotNull(todoStore);
			Assert.True(string.Equals(todoStore.CollectionName, collectionName));
		}

		[Test]
		public async Task TestCollectionSharedClient()
		{
			// Arrange

			// Act
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC);

			// Assert
			Assert.NotNull(todoStore);
			Assert.True(string.Equals(todoStore.CollectionName, collectionName));
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestCollectionBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		public void TestDeltaSetFetchEnable()
		{
			// Arrange
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC);

			// Act
			todoStore.DeltaSetFetchingEnabled = true;

			// Assert
			Assert.True(todoStore.DeltaSetFetchingEnabled);
		}

		[Ignore("Placeholder - No unit test yet")]
		[Test]
		public async Task TestDeltaSetPullAsync()
		{
			// Setup

			// Arrange

			// Act

			// Assert

			// Teardown
		}

		[Ignore("Placeholder - No unit test yet")]
		[Test]
		public async Task TestDeltaSetPullTwiceAsync()
		{
			// Setup

			// Arrange

			// Act

			// Assert

			// Teardown
		}

		[Test]
		public async Task TestSyncStoreFindAsync()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "Next Task";
			newItem.Details = "A test";
			newItem.DueDate = "2016-04-19T20:02:17.635Z";
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC);
			ToDo t = await todoStore.SaveAsync(newItem);

			ToDo anotherNewItem = new ToDo();
			anotherNewItem.Name = "Another Next Task";
			anotherNewItem.Details = "Another test";
			anotherNewItem.DueDate = "2016-05-19T20:02:17.635Z";
			ToDo t2 = await todoStore.SaveAsync(anotherNewItem);

			// Act
			List<ToDo> listToDo = new List<ToDo>();

			listToDo = await todoStore.FindAsync();

			// Assert
			Assert.NotNull(listToDo);
			Assert.AreEqual(2, listToDo.Count);

			// Teardown
			await todoStore.RemoveAsync(t.ID);
			await todoStore.RemoveAsync(t2.ID);
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestSyncStoreFindByIDAsync()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "Next Task";
			newItem.Details = "A test";
			newItem.DueDate = "2016-04-19T20:02:17.635Z";
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC);
			ToDo t = await todoStore.SaveAsync(newItem);

			// Act
			ToDo entity = null;
			entity = await todoStore.FindByIDAsync(t.ID);

			// Assert
			Assert.NotNull(entity);
			Assert.True(string.Equals(entity.ID, t.ID));

			// Teardown
			await todoStore.RemoveAsync(t.ID);
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestSyncStoreFindByIDsAsync()
		{
			//			// Setup
			//			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);
			//
			//			// Arrange
			//			ToDo newItem = new ToDo();
			//			newItem.Name = "Next Task";
			//			newItem.Details = "A test";
			//			newItem.DueDate = "2016-04-19T20:02:17.635Z";
			//			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.SYNC);
			//			ToDo t = await todoStore.SaveAsync(newItem);
			//
			//			ToDo anotherNewItem = new ToDo();
			//			anotherNewItem.Name = "Another Next Task";
			//			anotherNewItem.Details = "Another test";
			//			anotherNewItem.DueDate = "2016-05-19T20:02:17.635Z";
			//			ToDo t2 = await todoStore.SaveAsync(anotherNewItem);
			//
			//			List<string> listFind = new List<string>();
			//			listFind.Add(t.ID);
			//			listFind.Add(t2.ID);
			//
			//			// Act
			//			List<ToDo> todoList = await todoStore.FindByIDsAsync(listFind);
			//
			//			// Assert
			//			Assert.NotNull(todoList);
			//			Assert.AreEqual(2, todoList.Count);
			//
			//			// Teardown
			//			await todoStore.RemoveAsync(t.ID);
			//			await todoStore.RemoveAsync(t2.ID);
			//			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		public async Task TestSyncStoreFindByQuery()
		{
			// Setup
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

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);

			// Act
			//			var query = from todo in todoStore
			//						where todo.Details.StartsWith("details for 2")
			//						select todo;

			List<ToDo> listToDo = new List<ToDo>();
			var query = todoStore.Where(x => x.Details.StartsWith("det"));

			listToDo = await todoStore.FindAsync(query);


			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.IsNotNull(listToDo);
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(2, listToDo.Count);
		}

		[Test]
		public async Task TestSyncStoreFindByQueryInequalityGreaterThan()
		{
			// Setup
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

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.SYNC);

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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(2, listToDo.Count);
		}

		[Test]
		public async Task TestSyncStoreFindByQueryInequalityGreaterThanOrEqual()
		{
			// Setup
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
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.SYNC);

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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(2, listToDo.Count);
		}

		[Test]
		public async Task TestSyncStoreFindByQueryInequalityLessThan()
		{
			// Setup
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

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.SYNC);

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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(1, listToDo.Count);
		}

		[Test]
		public async Task TestSyncStoreFindByQueryInequalityLessThanOrEqual()
		{
			// Setup
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

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.SYNC);

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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(2, listToDo.Count);
		}

		[Test]
		public async Task TestSyncStoreFindByQueryInequalityDateTimeObjectGreaterThan()
		{
			// Setup
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
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.SYNC);

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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(1, listToDo.Count);
		}

		[Test]
		public async Task TestSyncStoreFindByQueryInequalityDateTimeObjectGreaterThanOrEqual()
		{
			// Setup
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
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.SYNC);

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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(1, listToDo.Count);
		}

		[Test]
		public async Task TestSyncStoreFindByQueryInequalityDateTimeObjectLessThan()
		{
			// Setup
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
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.SYNC);

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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(2, listToDo.Count);
		}

		[Test]
		public async Task TestSyncStoreFindByQueryInequalityDateTimeObjectLessThanOrEqual()
		{
			// Setup
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
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection("ToDos", DataStoreType.SYNC);

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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(3, listToDo.Count);
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestGetAsyncBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		public async Task TestGetCountAsync()
		{
			// Setup
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


			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC);
			ToDo t1 = await todoStore.SaveAsync(newItem);
			ToDo t2 = await todoStore.SaveAsync(newItem2);

			// Act
			uint count = 0;
			count = await todoStore.GetCountAsync();

			// Assert
			//Assert.GreaterOrEqual(count, 0);
			Assert.AreEqual(2, count);

			// Teardown
			await todoStore.RemoveAsync(t1.ID);
			await todoStore.RemoveAsync(t2.ID);
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestGetCountAsyncBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		public async Task TestSyncStoreGetSumAsync()
		{
			// Arrange
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			DataStore<Person> personStore = DataStore<Person>.Collection("person", DataStoreType.SYNC);

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
			List<GroupAggregationResults> arrGAR = await personStore.GroupAndAggregateAsync(EnumReduceFunction.REDUCE_FUNCTION_SUM, "", "Age", query);
			foreach (var gar in arrGAR)
			{
				sum = gar.Result;
				break;
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

		[Test]
		public async Task TestSyncStoreGetMinAsync()
		{
			// Arrange
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			DataStore<Person> personStore = DataStore<Person>.Collection("person", DataStoreType.SYNC);

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
			int min = 0;
			List<GroupAggregationResults> arrGAR = await personStore.GroupAndAggregateAsync(EnumReduceFunction.REDUCE_FUNCTION_MIN, "", "Age", query);
			foreach (var gar in arrGAR)
			{
				min = gar.Result;
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

		[Test]
		public async Task TestSyncStoreGetMaxAsync()
		{
			// Arrange
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			DataStore<Person> personStore = DataStore<Person>.Collection("person", DataStoreType.SYNC);

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
			List<GroupAggregationResults> arrGAR = await personStore.GroupAndAggregateAsync(EnumReduceFunction.REDUCE_FUNCTION_MAX, "LastName", "Age");
			foreach (var gar in arrGAR)
			{
				if (gar.GroupField.Equals("Funke"))
				{
					max = gar.Result;
				}
			}

			// Teardown
			await personStore.RemoveAsync(p3.ID);
			await personStore.RemoveAsync(p2.ID);
			await personStore.RemoveAsync(p1.ID);

			// Assert
			Assert.AreNotEqual(0, max);
			Assert.AreEqual(46, max);
			Assert.AreEqual(2, arrGAR.Count());
		}

		[Test]
		public async Task TestSyncStoreGetAverageAsync()
		{
			// Arrange
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			DataStore<Person> personStore = DataStore<Person>.Collection("person", DataStoreType.SYNC);

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

		[Test]
		public async Task TestSaveAsync()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "Next Task";
			newItem.Details = "A test";
			newItem.DueDate = "2016-04-19T20:02:17.635Z";
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC);

			// Act
			ToDo savedToDo = await todoStore.SaveAsync(newItem);

			// Assert
			Assert.NotNull(savedToDo);
			Assert.True(string.Equals(savedToDo.Name, newItem.Name));

			// Teardown
			await todoStore.RemoveAsync(savedToDo.ID);
			kinveyClient.ActiveUser.Logout();
		}

        [Test]
        public async Task TestSaveCustomIDAsync()
        {
            // Setup
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            ToDo newItem = new ToDo();
            newItem.Name = "Next Task";
            newItem.Details = "A test";
            newItem.DueDate = "2016-04-19T20:02:17.635Z";
            newItem.ID = "12345";
            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC);

            // Act
            ToDo savedToDo = await todoStore.SaveAsync(newItem);
            PendingWriteAction pwaBefore = kinveyClient.CacheManager.GetSyncQueue(collectionName).Peek();
            int countBefore = kinveyClient.CacheManager.GetSyncQueue(collectionName).Count(true);
            savedToDo.BoolVal = true;
            savedToDo = await todoStore.SaveAsync(savedToDo);
            PendingWriteAction pwaAfter = kinveyClient.CacheManager.GetSyncQueue(collectionName).Peek();
            int countAfter = kinveyClient.CacheManager.GetSyncQueue(collectionName).Count(true);

            // Assert
            Assert.NotNull(savedToDo);
            Assert.True(string.Equals(savedToDo.Name, newItem.Name));
            Assert.NotNull(pwaBefore);
            Assert.NotNull(pwaAfter);
            Assert.AreEqual(1, countAfter);
            Assert.AreEqual(countBefore, countAfter);
            Assert.True(string.Compare("12345", pwaBefore.entityId) == 0);
            Assert.True(string.Compare("12345", pwaAfter.entityId) == 0);

            // Teardown
            await todoStore.RemoveAsync(savedToDo.ID);
            kinveyClient.ActiveUser.Logout();
        }

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestSaveAsyncBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		public async Task TestDeleteAsync()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC);
			ToDo newItem = new ToDo();
			newItem.Name = "Task to Delete";
			newItem.Details = "A delete test";
			ToDo deleteToDo = await todoStore.SaveAsync(newItem);

			// Act
			KinveyDeleteResponse kdr = await todoStore.RemoveAsync(deleteToDo.ID);

			// Assert
			Assert.NotNull(kdr);
			Assert.AreEqual(1, kdr.count);

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestDeleteCustomIDAsync()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "Next Task";
			newItem.Details = "A test";
			newItem.DueDate = "2016-04-19T20:02:17.635Z";
			newItem.ID = "12345";
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC);

			// Act
			ToDo savedToDo = await todoStore.SaveAsync(newItem);
			PendingWriteAction pwaBefore = kinveyClient.CacheManager.GetSyncQueue(collectionName).Peek();
			int countBefore = kinveyClient.CacheManager.GetSyncQueue(collectionName).Count(true);
			var kdr = await todoStore.RemoveAsync("12345");
			PendingWriteAction pwaAfter = kinveyClient.CacheManager.GetSyncQueue(collectionName).Peek();
			int countAfter = kinveyClient.CacheManager.GetSyncQueue(collectionName).Count(true);

			// Assert
			Assert.NotNull(pwaBefore);
			Assert.NotNull(pwaAfter);
			Assert.AreEqual(1, countBefore);
			Assert.AreEqual(countBefore, countAfter);
			Assert.AreEqual(1, kdr.count);
			Assert.True(string.Compare("12345", pwaBefore.entityId) == 0);
			Assert.True(string.Compare("12345", pwaAfter.entityId) == 0);

			// Teardown
			await todoStore.RemoveAsync(savedToDo.ID);
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestDeleteAsyncBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		public async Task TestSyncQueueAdd()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC);
			ToDo newItem = new ToDo();
			newItem.Name = "Task to save to SyncQ";
			newItem.Details = "A sync add test";
			newItem = await todoStore.SaveAsync(newItem);

			// Act
			PendingWriteAction pwa = kinveyClient.CacheManager.GetSyncQueue(collectionName).Peek();

			// Assert
			Assert.NotNull(pwa);
			Assert.IsNotNull(pwa.entityId);
			Assert.IsNotEmpty(pwa.entityId);
			Assert.True(String.Equals(collectionName, pwa.collection));
			Assert.True(String.Equals("POST", pwa.action));

			// Teardown
			await todoStore.RemoveAsync(newItem.ID);
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestSyncQueueAddWithID()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC);
			ToDo newItem = new ToDo();
			newItem.Name = "Task to save to SyncQ";
			newItem.Details = "A sync add test";
			newItem.ID = "12345";
			newItem = await todoStore.SaveAsync(newItem);

			// Act
			PendingWriteAction pwa = kinveyClient.CacheManager.GetSyncQueue(collectionName).Peek();
			List<ToDo> t = await todoStore.FindAsync();

			// Assert
			Assert.NotNull(pwa);
			Assert.IsNotNull(pwa.entityId);
			Assert.IsNotEmpty(pwa.entityId);
			Assert.True(String.Equals(collectionName, pwa.collection));
			Assert.True(String.Equals("PUT", pwa.action));
			Assert.NotNull(t);
			Assert.AreEqual(1, t.Count);
			Assert.AreEqual("12345", t.First().ID);
			Assert.True(String.Equals(t.First().ID, pwa.entityId));

			// Teardown
			await todoStore.RemoveAsync(newItem.ID);
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestSyncQueueAddThenDelete()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC);
			ToDo newItem = new ToDo();
			newItem.Name = "Task to save to SyncQ";
			newItem.Details = "A sync add test";
			newItem = await todoStore.SaveAsync(newItem);
			var responseDelete = await todoStore.RemoveAsync(newItem.ID);

			// Act
			PendingWriteAction pwa = kinveyClient.CacheManager.GetSyncQueue(collectionName).Peek();
			var pushresp = await todoStore.PushAsync();
			int syncQueueCount = kinveyClient.CacheManager.GetSyncQueue(collectionName).Count(true);

			// Assert
			Assert.Null(pwa);
			//Assert.IsNull(pwa.entityId);
			//Assert.IsNotEmpty(pwa.entityId);
			//Assert.True(String.Equals(collectionName, pwa.collection));
			//Assert.True(String.Equals("DELETE", pwa.action));
			Assert.NotNull(pushresp);
			Assert.NotNull(pushresp.KinveyExceptions);
			Assert.AreEqual(0, pushresp.KinveyExceptions.Count);
			//Assert.AreEqual(EnumErrorCode.ERROR_JSON_RESPONSE, pushresp.KinveyExceptions.First().ErrorCode);
			Assert.AreEqual(0, syncQueueCount);

			// Teardown
			await todoStore.RemoveAsync(newItem.ID);
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestSyncQueuePush()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC, kinveyClient);
			ToDo newItem = new ToDo();
			newItem.Name = "Task to update to SyncQ";
			newItem.Details = "A sync add test";
			newItem = await todoStore.SaveAsync(newItem);

			ToDo newItem2 = new ToDo();
			newItem2.Name = "Task to add another item to SyncQ";
			newItem2.Details = "Another sync add test";
			newItem2 = await todoStore.SaveAsync(newItem2);

			DataStore<FlashCard> flashCardStore = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.SYNC, kinveyClient);
			FlashCard firstFlashCard = new FlashCard();
			firstFlashCard.Question = "What is capital of Djibouti?";
			firstFlashCard.Answer = "Djibouti";
			firstFlashCard = await flashCardStore.SaveAsync(firstFlashCard);

			// Act
			SyncDataStoreResponse<ToDo> dsr = await todoStore.SyncAsync();

			// Assert
			Assert.NotNull(dsr);
			Assert.NotNull(dsr.PullResponse);
			Assert.NotNull(dsr.PushResponse);
			Assert.NotNull(dsr.PushResponse.PushCount);
			Assert.AreEqual(2, dsr.PushResponse.PushCount);

			// Teardown
			List<ToDo> listRemoveToDo = new List<ToDo>();
			listRemoveToDo = await todoStore.FindAsync();

			foreach (ToDo td in listRemoveToDo)
			{
				await todoStore.RemoveAsync(td.ID);
			}

			List<FlashCard> listRemoveFlash = new List<FlashCard>();
			listRemoveFlash = await flashCardStore.FindAsync();

			foreach (FlashCard fc in listRemoveFlash)
			{
				await flashCardStore.RemoveAsync(fc.ID);
			}

			SyncDataStoreResponse<ToDo> dsrDelete = await todoStore.SyncAsync();
			Assert.NotNull(dsrDelete);
			Assert.NotNull(dsrDelete.PushResponse);
			Assert.NotNull(dsrDelete.PullResponse);
			Assert.AreEqual(2, dsrDelete.PushResponse.PushCount);
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestSyncQueuePushUpdate()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC, kinveyClient);
			ToDo newItem = new ToDo();
			newItem.Name = "Task to update to SyncQ";
			newItem.Details = "A sync add test";
			newItem = await todoStore.SaveAsync(newItem);

			newItem.DueDate = "2016-04-19T20:02:17.635Z";
			ToDo updatedItem = await todoStore.SaveAsync(newItem);

			// Act
			SyncDataStoreResponse<ToDo> dsr = await todoStore.SyncAsync();

			// Assert
			Assert.NotNull(dsr);
			Assert.NotNull(dsr.PushResponse);
			Assert.NotNull(dsr.PullResponse);
			Assert.IsNotNull(dsr.PushResponse.KinveyExceptions);
			Assert.IsNotNull(dsr.PullResponse.KinveyExceptions);
			Assert.AreEqual(1, dsr.PushResponse.PushCount);
			Assert.NotNull(dsr.PullResponse.PullEntities);
			Assert.IsNotEmpty(dsr.PullResponse.PullEntities);
			Assert.AreEqual(dsr.PullResponse.PullCount, dsr.PullResponse.PullEntities.Count);

			// Teardown
			List<ToDo> listRemoveToDo = new List<ToDo>();

			listRemoveToDo = await todoStore.FindAsync();

			KinveyDeleteResponse kdr;
			foreach (ToDo td in listRemoveToDo)
			{
				kdr = await todoStore.RemoveAsync(td.ID);
			}

			dsr = await todoStore.SyncAsync();
			Assert.NotNull(dsr);
			Assert.AreEqual(1, dsr.PushResponse.PushCount);
			//			Assert.AreSame(dsr.Count, kdr.count);
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestSyncQueueCount()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC, kinveyClient);
			ToDo newItem = new ToDo();
			newItem.Name = "Task to update to SyncQ";
			newItem.Details = "A sync add test";
			newItem = await todoStore.SaveAsync(newItem);

			newItem.DueDate = "2016-04-19T20:02:17.635Z";
			ToDo updatedItem = await todoStore.SaveAsync(newItem);

			DataStore<FlashCard> flashCardStore = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.SYNC, kinveyClient);
			FlashCard firstFlashCard = new FlashCard();
			firstFlashCard.Question = "What is capital of Djibouti?";
			firstFlashCard.Answer = "Djibouti";
			firstFlashCard = await flashCardStore.SaveAsync(firstFlashCard);

			// Act
			int syncCountToDo = todoStore.GetSyncCount();
			int syncCountFlashCard = flashCardStore.GetSyncCount();
			int syncCountTotal = todoStore.GetSyncCount(true);

			// Assert
			Assert.AreEqual(1, syncCountToDo);
			Assert.AreEqual(1, syncCountFlashCard);
			Assert.AreEqual(2, syncCountTotal);

			// Teardown
			await todoStore.RemoveAsync(newItem.ID);
			await todoStore.RemoveAsync(firstFlashCard.ID);
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestSyncQueuePush10Items()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC, kinveyClient);

			for (int i = 0; i < 10; i++)
			{
				ToDo newItem = new ToDo();
				newItem.Name = "Task to update to SyncQ";
				newItem.Details = "A sync add test";
				newItem = await todoStore.SaveAsync(newItem);
			}

			// Act
			SyncDataStoreResponse<ToDo> dsr = await todoStore.SyncAsync();

			// Assert
			Assert.NotNull(dsr);
			Assert.IsNotNull(dsr.PushResponse.KinveyExceptions);
			Assert.IsNotNull(dsr.PullResponse.KinveyExceptions);
			Assert.AreEqual(10, dsr.PushResponse.PushCount);
			Assert.NotNull(dsr.PullResponse.PullEntities);
			Assert.IsNotEmpty(dsr.PullResponse.PullEntities);
			Assert.AreEqual(10, dsr.PullResponse.PullEntities.Count);

			// Teardown
			List<ToDo> listRemoveToDo = new List<ToDo>();
			listRemoveToDo = await todoStore.FindAsync();

			foreach (ToDo t in listRemoveToDo)
			{
				await todoStore.RemoveAsync(t.ID);
			}

			await todoStore.SyncAsync();
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestSyncStorePullAsync()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC);

			PullDataStoreResponse<ToDo> todosBeforeSave = await todoStore.PullAsync();

			// Assert
			Assert.NotNull(todosBeforeSave);
			Assert.IsEmpty(todosBeforeSave.PullEntities);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "Next Task";
			newItem.Details = "A test";
			newItem.DueDate = "2016-04-19T20:02:17.635Z";

			ToDo t = await todoStore.SaveAsync(newItem);

			ToDo anotherNewItem = new ToDo();
			anotherNewItem.Name = "Another Next Task";
			anotherNewItem.Details = "Another test";
			anotherNewItem.DueDate = "2016-05-19T20:02:17.635Z";
			ToDo t2 = await todoStore.SaveAsync(anotherNewItem);


			await todoStore.PushAsync();

			PullDataStoreResponse<ToDo> todosAfterSave = await todoStore.PullAsync();

			// Assert
			Assert.NotNull(todosAfterSave);
			Assert.AreEqual(2, todosAfterSave.PullCount);

			// Teardown
			foreach (var todo in todosAfterSave.PullEntities)
			{
				await todoStore.RemoveAsync(todo.ID);
			}
			await todoStore.PushAsync();
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestSyncStorePullWithQueryAsync()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC);

			PullDataStoreResponse<ToDo> todosBeforeSave = await todoStore.PullAsync();

			// Assert
			Assert.IsNotNull(todosBeforeSave);
			Assert.IsEmpty(todosBeforeSave.PullEntities);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "Next Task";
			newItem.Details = "A test";
			newItem.DueDate = "2016-04-19T20:02:17.635Z";

			ToDo t = await todoStore.SaveAsync(newItem);

			ToDo anotherNewItem = new ToDo();
			anotherNewItem.Name = "Another Next Task";
			anotherNewItem.Details = "Another test";
			anotherNewItem.DueDate = "2016-05-19T20:02:17.635Z";
			ToDo t2 = await todoStore.SaveAsync(anotherNewItem);


			await todoStore.PushAsync();

			var query = from x in todoStore where x.Details.StartsWith("Another") select x;

			PullDataStoreResponse<ToDo> todosAfterSave = await todoStore.PullAsync(query);

			// Assert
			Assert.NotNull(todosAfterSave);
			Assert.AreEqual(1, todosAfterSave.PullCount);

			// Teardown
			PullDataStoreResponse<ToDo> todoCleanup = await todoStore.PullAsync();
			foreach (var todo in todoCleanup.PullEntities)
			{
				await todoStore.RemoveAsync(todo.ID);
			}
			await todoStore.PushAsync();
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestSyncStoreSyncWithQueryAsync()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC);

			PullDataStoreResponse<ToDo> todosBeforeSave = await todoStore.PullAsync();

			// Assert
			Assert.IsNotNull(todosBeforeSave);
			Assert.IsEmpty(todosBeforeSave.PullEntities);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "Next Task";
			newItem.Details = "A test";
			newItem.DueDate = "2016-04-19T20:02:17.635Z";

			ToDo t = await todoStore.SaveAsync(newItem);

			ToDo anotherNewItem = new ToDo();
			anotherNewItem.Name = "Another Next Task";
			anotherNewItem.Details = "Another test";
			anotherNewItem.DueDate = "2016-05-19T20:02:17.635Z";
			ToDo t2 = await todoStore.SaveAsync(anotherNewItem);

			var query = from x in todoStore where x.Details.StartsWith("Another") select x;

			SyncDataStoreResponse<ToDo> todosAfterSave = await todoStore.SyncAsync(query);

			// Assert
			Assert.NotNull(todosAfterSave);
			Assert.AreEqual(1, todosAfterSave.PullResponse.PullCount);

			// Teardown
			PullDataStoreResponse<ToDo> todoCleanup = await todoStore.PullAsync();
			foreach (var todo in todoCleanup.PullEntities)
			{
				await todoStore.RemoveAsync(todo.ID);
			}
			await todoStore.PushAsync();
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestSyncStoreFindByQueryWithSortAscending()
		{
			// Setup
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
			newItem1.NewDate = new DateTime(2016, 4, 22, 19, 56, 00);

			ToDo newItem2 = new ToDo();
			newItem2.Name = "another todo";
			newItem2.Details = "details for 2";
			newItem2.DueDate = "2016-04-22T19:56:00.963Z";
			newItem2.NewDate = new DateTime(2017, 4, 22, 19, 56, 00);

			ToDo newItem3 = new ToDo();
			newItem3.Name = "z another todo";
			newItem3.Details = "details for 3";
			newItem3.DueDate = "2016-04-22T19:56:00.963Z";
			newItem3.NewDate = new DateTime(2016, 3, 22, 19, 56, 00);

			ToDo newItem4 = new ToDo();
			newItem4.Name = "c another todo";
			newItem4.Details = "details for 4";
			newItem4.DueDate = "2016-04-22T19:56:00.963Z";
			newItem4.NewDate = new DateTime(2016, 4, 21, 19, 56, 00);

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);
			newItem3 = await todoStore.SaveAsync(newItem3);
			newItem4 = await todoStore.SaveAsync(newItem4);

			// Act
			var query = todoStore.Where(x => x.Details.StartsWith("det")).OrderBy(x => x.Name);

			List<ToDo> listToDo = new List<ToDo>();

			listToDo = await todoStore.FindAsync(query);

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			await todoStore.RemoveAsync(newItem3.ID);
			await todoStore.RemoveAsync(newItem4.ID);
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.IsNotNull(listToDo);
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(4, listToDo.Count);
			Assert.True(String.Compare(newItem2.Name, listToDo.First().Name) == 0);
		}

		[Test]
		public async Task TestSyncStoreFindByQueryWithSortDescending()
		{
			// Setup
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
			newItem1.NewDate = new DateTime(2016, 4, 22, 19, 56, 00);

			ToDo newItem2 = new ToDo();
			newItem2.Name = "another todo";
			newItem2.Details = "details for 2";
			newItem2.DueDate = "2016-04-22T19:56:00.963Z";
			newItem2.NewDate = new DateTime(2017, 4, 22, 19, 56, 00);

			ToDo newItem3 = new ToDo();
			newItem3.Name = "z another todo";
			newItem3.Details = "details for 3";
			newItem3.DueDate = "2016-04-22T19:56:00.963Z";
			newItem3.NewDate = new DateTime(2016, 3, 22, 19, 56, 00);

			ToDo newItem4 = new ToDo();
			newItem4.Name = "c another todo";
			newItem4.Details = "details for 4";
			newItem4.DueDate = "2016-04-22T19:56:00.963Z";
			newItem4.NewDate = new DateTime(2016, 4, 21, 19, 56, 00);

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);
			newItem3 = await todoStore.SaveAsync(newItem3);
			newItem4 = await todoStore.SaveAsync(newItem4);

			// Act
			var query = todoStore.Where(x => x.Details.StartsWith("det")).OrderByDescending(x => x.NewDate);

			List<ToDo> listToDo = new List<ToDo>();

			listToDo = await todoStore.FindAsync(query);

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			await todoStore.RemoveAsync(newItem3.ID);
			await todoStore.RemoveAsync(newItem4.ID);
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.IsNotNull(listToDo);
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(4, listToDo.Count);
			Assert.True(String.Compare(newItem2.Name, listToDo.First().Name) == 0);
		}


		#region ORM Tests

		[Test]
		public async Task TestORM_IPersistable()
		{
			// Setup
			if (kinveyClient.ActiveUser != null)
			{
				kinveyClient.ActiveUser.Logout();
			}

			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			string collectionAddressName = "Address";
			string collectionPersonName = "Person";

			Address addr = new Address();
			addr.IsApartment = true;
			addr.Street = "1 Infinite Loop";
			DataStore<Address> addrStore = DataStore<Address>.Collection(collectionAddressName, DataStoreType.SYNC, kinveyClient);
			addr = await addrStore.SaveAsync(addr);

			Person p = new Person();
			p.FirstName = "Steve";
			p.LastName = "Wozniak";
			p.MailAddress = addr;
			DataStore<Person> personStore = DataStore<Person>.Collection(collectionPersonName, DataStoreType.SYNC, kinveyClient);
			p = await personStore.SaveAsync(p);

			// Act
			ICache<Person> cache = kinveyClient.CacheManager.GetCache<Person>(collectionPersonName);
			List<Person> listPerson = cache.FindAll();

			// Assert
			Assert.NotNull(listPerson);
			Assert.IsNotEmpty(listPerson);
			Person savedPerson = listPerson.First();
			Assert.NotNull(savedPerson);
			Assert.IsTrue(String.Compare(p.FirstName, savedPerson.FirstName) == 0);
			Address savedAddr = savedPerson.MailAddress;
			Assert.IsTrue(String.Compare(addr.Street, savedAddr.Street) == 0);

			// Teardown
			await personStore.RemoveAsync(addr.ID);
			await addrStore.RemoveAsync(addr.ID);
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestORM_Entity()
		{
			// Setup
			if (kinveyClient.ActiveUser != null)
			{
				kinveyClient.ActiveUser.Logout();
			}

			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			string collectionAddressName = "AddressEntity";
			string collectionPersonName = "PersonEntity";

			AddressEntity addr = new AddressEntity();
			addr.IsApartment = true;
			addr.Street = "1 Infinite Loop";
			DataStore<AddressEntity> addrStore = DataStore<AddressEntity>.Collection(collectionAddressName, DataStoreType.SYNC, kinveyClient);
			addr = await addrStore.SaveAsync(addr);

			PersonEntity p = new PersonEntity();
			p.FirstName = "Steve";
			p.LastName = "Wozniak";
			p.MailAddress = addr;
			DataStore<PersonEntity> personStore = DataStore<PersonEntity>.Collection(collectionPersonName, DataStoreType.SYNC, kinveyClient);
			p = await personStore.SaveAsync(p);

			// Act
			ICache<PersonEntity> cache = kinveyClient.CacheManager.GetCache<PersonEntity>(collectionPersonName);
			List<PersonEntity> listPerson = cache.FindAll();

			// Assert
			Assert.NotNull(listPerson);
			Assert.IsNotEmpty(listPerson);
			PersonEntity savedPerson = listPerson.First();
			Assert.NotNull(savedPerson);
			Assert.IsTrue(String.Compare(p.FirstName, savedPerson.FirstName) == 0);
			AddressEntity savedAddr = savedPerson.MailAddress;
			Assert.IsTrue(String.Compare(addr.Street, savedAddr.Street) == 0);

			// Teardown
			await personStore.RemoveAsync(addr.ID);
			await addrStore.RemoveAsync(addr.ID);
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestPurge() { 
			if (kinveyClient.ActiveUser != null)
			{
				kinveyClient.ActiveUser.Logout();
			}

			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			var store = DataStore<Person>.Collection("Person", DataStoreType.SYNC);
			await store.SaveAsync(new Person());
			Assert.AreEqual(store.GetSyncCount(), 1);

			store.Purge();
			Assert.AreEqual(store.GetSyncCount(), 0);
			ICache<Person> cache = kinveyClient.CacheManager.GetCache<Person>("Person");
			Assert.AreEqual(cache.CountAll(), 1);

			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestPurgeByQuery()
		{
			if (kinveyClient.ActiveUser != null)
			{
				kinveyClient.ActiveUser.Logout();
			}

			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			var store = DataStore<Person>.Collection("Person", DataStoreType.SYNC);
			var person1 = new Person();
			person1.FirstName = "james";
			await store.SaveAsync(person1);

			var person2 = new Person();
			person2.FirstName = "bond";
			await store.SaveAsync(person2);

			ICache<Person> cache = kinveyClient.CacheManager.GetCache<Person>("Person");
			Assert.AreEqual(cache.CountAll(), 2);
			Assert.AreEqual(store.GetSyncCount(), 2);

			var query = store.Where(x => x.FirstName.Equals(person2.FirstName));
			var result = store.Purge(query);
			Assert.AreEqual(result, 1);
			Assert.AreEqual(cache.CountAll(), 2);
			Assert.AreEqual(store.GetSyncCount(), 1);

			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestClear() { 
			if (kinveyClient.ActiveUser != null)
			{
				kinveyClient.ActiveUser.Logout();
			}

			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			var store = DataStore<Person>.Collection("Person", DataStoreType.SYNC);
			await store.SaveAsync(new Person());

			ICache<Person> cache = kinveyClient.CacheManager.GetCache<Person>("Person");
			Assert.AreEqual(cache.CountAll(), 1);
			Assert.AreEqual(store.GetSyncCount(), 1);

			var result = store.ClearCache();
			Assert.AreEqual(result.count, 1);
			Assert.AreEqual(cache.CountAll(), 0);
			Assert.AreEqual(store.GetSyncCount(), 0);

			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestClearByQuery()
		{
			if (kinveyClient.ActiveUser != null)
			{
				kinveyClient.ActiveUser.Logout();
			}

			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			var store = DataStore<Person>.Collection("Person", DataStoreType.SYNC);
			var person1 = new Person();
			person1.FirstName = "james";
			await store.SaveAsync(person1);

			var person2 = new Person();
			person2.FirstName = "bond";
			await store.SaveAsync(person2);

			ICache<Person> cache = kinveyClient.CacheManager.GetCache<Person>("Person");
			Assert.AreEqual(cache.CountAll(), 2);
			Assert.AreEqual(store.GetSyncCount(), 2);

			var query = store.Where(x => x.FirstName.Equals(person2.FirstName));
			var result = store.ClearCache(query);
			Assert.AreEqual(result.count, 1);
			Assert.AreEqual(cache.CountAll(), 1);
			Assert.AreEqual(store.GetSyncCount(), 1);

			kinveyClient.ActiveUser.Logout();
		}
		#endregion

        #region Server-side Delta Sync (SSDS) Tests

        #region SSDS Pull Tests

        //[Test(Description = "with disabled deltaset should make just regular get requests")]

        [Test(Description = "with enabled deltaset should return no items when no changes are made")]
        public async Task TestDeltaSetPullNoChanges()
        {
            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.SYNC);
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

        [Test(Description = "with enabled deltaset should return only changes since last request")]
        public async Task TestDeltaSetPullReturnOnlyChanges()
        {
            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.SYNC);
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

        [Test(Description = "with enabled deltaset and query should return correct number of updated items")]
        public async Task TestDeltaSetPullReturnOnlyUpdates()
        {
            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.SYNC);
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

        [Test(Description = "with enabled deltaset and query should return correct number of deleted items")]
        public async Task TestDeltaSetPullReturnOnlyDeletes()
        {
            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.SYNC);
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
            Assert.That(localCopy == false);
        }

        //[Test(Description = "when deltaset is switched off should start sending regular GET requests")]

        [Test(Description = "with enabled deltaset should return correct number of items when creating")]
        public async Task TestDeltaSetPullReturnCorrectNumberOfCreatedItems()
        {
            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.SYNC);
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

        [Test(Description = "with enabled deltaset should return correct number of items when updating")]
        public async Task TestDeltaSetPullReturnCorrectNumberOfUpdates()
        {
            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.SYNC);
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

        [Test(Description = "with enabled deltaset should return correct number of items when deleting")]
        public async Task TestDeltaSetPullReturnCorrectNumberOfDeletes()
        {
            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.SYNC);
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

        [Test(Description = "with enabled deltaset should return correct number of items when deleting and updating")]
        public async Task TestDeltaSetPullReturnCorrectNumberOfDeletesAndUpdates()
        {
            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.SYNC);
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

        [Test(Description = "with enabled deltaset should return no items when no changes are made")]
        public async Task TestDeltaSetSyncNoChanges()
        {
            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.SYNC);
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

        [Test(Description = "with enabled deltaset should return only changes since last request")]
        public async Task TestDeltaSetSyncReturnOnlyChanges()
        {
            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.SYNC);
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

        [Test(Description = "with enabled deltaset and query should return correct number of updated items")]
        public async Task TestDeltaSetSyncReturnOnlyUpdates()
        {
            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.SYNC);
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

        [Test(Description = "with enabled deltaset and query should return correct number of deleted items")]
        public async Task TestDeltaSetSyncReturnOnlyDeletes()
        {
            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.SYNC);
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
            Assert.That(localCopy == false);
        }

        //[Test(Description = "when deltaset is switched off should start sending regular GET requests")]

        [Test(Description = "with enabled deltaset should return correct number of items when creating")]
        public async Task TestDeltaSetSyncReturnCorrectNumberOfCreatedItems()
        {
            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.SYNC);
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

        [Test(Description = "with enabled deltaset should return correct number of items when updating")]
        public async Task TestDeltaSetSyncReturnCorrectNumberOfUpdates()
        {
            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.SYNC);
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

        [Test(Description = "with enabled deltaset should return correct number of items when deleting")]
        public async Task TestDeltaSetSyncReturnCorrectNumberOfDeletes()
        {
            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.SYNC);
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

        [Test(Description = "with enabled deltaset should return correct number of items when deleting and updating")]
        public async Task TestDeltaSetSyncReturnCorrectNumberOfDeletesAndUpdates()
        {
            // Arrange
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }

            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            var store = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.SYNC);
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

        [Test(Description = "with enable deltaset and limit and skip should not use deltaset and should not override lastRunAt")]
        public async Task TestDeltaSetSyncLimitAndSkipShouldNotUseDS()
        {
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

            var fc2Query = store.Where(y => y.Question.StartsWith("W")).Skip(1).Take(1);
            int pullCount1 = (await store.PullAsync(fc2Query)).PullCount;
            int pullCount2 = (await store.PullAsync(fc2Query)).PullCount;

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
            Assert.AreEqual(1, pullCount1);
            Assert.AreEqual(1, pullCount2);
        }

        [Test]
        public async Task TestDeltaSetSyncSkipShouldNotUseDS()
        {
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

            var fc2Query = store.Where(y => y.Question.StartsWith("W")).Skip(1);
            int pullCount1 = (await store.PullAsync(fc2Query)).PullCount;
            int pullCount2 = (await store.PullAsync(fc2Query)).PullCount;

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
            Assert.AreEqual(2, pullCount1);
            Assert.AreEqual(2, pullCount2);
        }

        [Test]
        public async Task TestDeltaSetSyncLimitShouldNotUseDS()
        {
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

            var fc2Query = store.Where(y => y.Question.StartsWith("W")).Take(1);
            int pullCount1 = (await store.PullAsync(fc2Query)).PullCount;
            int pullCount2 = (await store.PullAsync(fc2Query)).PullCount;

            await store.RemoveAsync(fc1.ID);
            await store.RemoveAsync(fc2.ID);
            await store.RemoveAsync(fc3.ID);

            // Assert
            Assert.AreEqual(1, pullCount1);
            Assert.AreEqual(1, pullCount2);
        }

        //[Test(Description = "with enable deltaset and limit and skip should not use deltaset and should not cause inconsistent data")]

        #endregion

        #endregion
    }
}
