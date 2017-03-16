
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
				.setFilePath(TestSetup.db_dir)
				.setOfflinePlatform(new SQLite.Net.Platform.Generic.SQLitePlatformGeneric());

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
			entity = (await todoStore.FindByIDAsync(t.ID)).First();

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

		#endregion
	}
}
