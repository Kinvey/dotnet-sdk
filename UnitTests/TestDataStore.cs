using System;
using System.Collections.Generic;
using NUnit.Framework;
using KinveyXamarin;
using System.Threading.Tasks;

namespace UnitTestFramework
{
	[TestFixture]
	public class TestDataStore
	{
		private Client kinveyClient;

		private const string collectionName = "ToDos";

		private const string db_dir = "../../../UnitTests/TestFiles/";
		private const string SQLiteOfflineStoreFilePath = db_dir + "kinveyOffline.sqlite";
		private const string SQLiteCredentialStoreFilePath = db_dir + "kinvey_tokens.sqlite";

		[SetUp]
		public void Setup ()
		{
			kinveyClient = new Client.Builder(TestSetup.app_key, TestSetup.app_secret)
				.setFilePath(db_dir)
				.setOfflinePlatform(new SQLite.Net.Platform.Generic.SQLitePlatformGeneric())
				.build();

		}

		[TearDown]
		public void Tear ()
		{
			kinveyClient.CurrentUser.Logout();
			System.IO.File.Delete(SQLiteOfflineStoreFilePath);
			System.IO.File.Delete(SQLiteCredentialStoreFilePath);
		}

		[Test]
		public async Task TestGetInstance()
		{
			// Arrange

			// Act
			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.NETWORK);

			// Assert
			Assert.NotNull(todoStore);
			Assert.True(string.Equals(todoStore.CollectionName, collectionName));
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestGetInstanceBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestGetEntityAsyncBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		public async Task TestNetworkStoreFindAsync()
		{
			// Setup
			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "Next Task";
			newItem.Details = "A test";
			newItem.DueDate = "2016-04-19T20:02:17.635Z";
			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.NETWORK);
			ToDo t = await todoStore.SaveAsync(newItem);

			ToDo anotherNewItem = new ToDo();
			anotherNewItem.Name = "Another Next Task";
			anotherNewItem.Details = "Another test";
			anotherNewItem.DueDate = "2016-05-19T20:02:17.635Z";
			ToDo t2 = await todoStore.SaveAsync(anotherNewItem);

			// Act
			List<ToDo> todoList = await todoStore.FindAsync();

			// Assert
			Assert.NotNull(todoList);
			Assert.AreEqual(2, todoList.Count);

			// Teardown
			await todoStore.RemoveAsync(t.ID);
			await todoStore.RemoveAsync(t2.ID);
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		public async Task TestSyncStoreFindAsync()
		{
			// Setup
			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "Next Task";
			newItem.Details = "A test";
			newItem.DueDate = "2016-04-19T20:02:17.635Z";
			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.SYNC);
			ToDo t = await todoStore.SaveAsync(newItem);

			ToDo anotherNewItem = new ToDo();
			anotherNewItem.Name = "Another Next Task";
			anotherNewItem.Details = "Another test";
			anotherNewItem.DueDate = "2016-05-19T20:02:17.635Z";
			ToDo t2 = await todoStore.SaveAsync(anotherNewItem);

			// Act
			List<ToDo> todoList = await todoStore.FindAsync();

			// Assert
			Assert.NotNull(todoList);
			Assert.AreEqual(2, todoList.Count);

			// Teardown
			await todoStore.RemoveAsync(t.ID);
			await todoStore.RemoveAsync(t2.ID);
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestCacheStoreFindAsync()
		{
		}

		[Test]
		public async Task TestNetworkStoreFindByIDAsync()
		{
			// Setup
			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "Next Task";
			newItem.Details = "A test";
			newItem.DueDate = "2016-04-19T20:02:17.635Z";
			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.NETWORK);
			ToDo t = await todoStore.SaveAsync(newItem);

			// Act
			ToDo entity = await todoStore.FindByIDAsync(t.ID);

			// Assert
			Assert.NotNull(entity);
			Assert.True(string.Equals(entity.ID, t.ID));

			// Teardown
			await todoStore.RemoveAsync(t.ID);
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		public async Task TestSyncStoreFindByIDAsync()
		{
			// Setup
			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "Next Task";
			newItem.Details = "A test";
			newItem.DueDate = "2016-04-19T20:02:17.635Z";
			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.SYNC);
			ToDo t = await todoStore.SaveAsync(newItem);

			// Act
			ToDo entity = await todoStore.FindByIDAsync(t.ID);

			// Assert
			Assert.NotNull(entity);
			Assert.True(string.Equals(entity.ID, t.ID));

			// Teardown
			await todoStore.RemoveAsync(t.ID);
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestCacheStoreFindByIDAsync()
		{
		}

		[Test]
		public async Task TestNetworkStoreFindByIDsAsync()
		{
			// Setup
			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "Next Task";
			newItem.Details = "A test";
			newItem.DueDate = "2016-04-19T20:02:17.635Z";
			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.NETWORK);
			ToDo t = await todoStore.SaveAsync(newItem);

			ToDo anotherNewItem = new ToDo();
			anotherNewItem.Name = "Another Next Task";
			anotherNewItem.Details = "Another test";
			anotherNewItem.DueDate = "2016-05-19T20:02:17.635Z";
			ToDo t2 = await todoStore.SaveAsync(anotherNewItem);

			List<string> listFind = new List<string>();
			listFind.Add(t.ID);
			listFind.Add(t2.ID);

			// Act
			List<ToDo> todoList = await todoStore.FindByIDsAsync(listFind);

			// Assert
			Assert.NotNull(todoList);
			Assert.AreEqual(2, todoList.Count);

			// Teardown
			await todoStore.RemoveAsync(t.ID);
			await todoStore.RemoveAsync(t2.ID);
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		public async Task TestSyncStoreFindByIDsAsync()
		{
			// Setup
			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "Next Task";
			newItem.Details = "A test";
			newItem.DueDate = "2016-04-19T20:02:17.635Z";
			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.SYNC);
			ToDo t = await todoStore.SaveAsync(newItem);

			ToDo anotherNewItem = new ToDo();
			anotherNewItem.Name = "Another Next Task";
			anotherNewItem.Details = "Another test";
			anotherNewItem.DueDate = "2016-05-19T20:02:17.635Z";
			ToDo t2 = await todoStore.SaveAsync(anotherNewItem);

			List<string> listFind = new List<string>();
			listFind.Add(t.ID);
			listFind.Add(t2.ID);

			// Act
			List<ToDo> todoList = await todoStore.FindByIDsAsync(listFind);

			// Assert
			Assert.NotNull(todoList);
			Assert.AreEqual(2, todoList.Count);

			// Teardown
			await todoStore.RemoveAsync(t.ID);
			await todoStore.RemoveAsync(t2.ID);
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestCacheStoreFindByIDsAsync()
		{
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
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestGetCountAsync()
		{
//			// Setup
//			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);
//
//			// Arrange
//			ToDo newItem = new ToDo();
//			newItem.Name = "Next Task";
//			newItem.Details = "A test";
//			newItem.DueDate = "2016-04-19T20:02:17.635Z";
//			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.NETWORK);
//			ToDo t = await todoStore.SaveAsync(newItem);
//
//			// Act
//			uint count = await todoStore.GetCountAsync();
//
//			// Assert
//			Assert.GreaterOrEqual(count, 0);
//			Assert.AreEqual(1, count);
//
//			// Teardown
//			await todoStore.RemoveAsync(t.ID);
//			kinveyClient.CurrentUser.Logout();
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
		public async Task TestSaveAsync()
		{
			// Setup
			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "Next Task";
			newItem.Details = "A test";
			newItem.DueDate = "2016-04-19T20:02:17.635Z";
			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.NETWORK);

			// Act
			ToDo savedToDo = await todoStore.SaveAsync(newItem);

			// Assert
			Assert.NotNull(savedToDo);
			Assert.True(string.Equals(savedToDo.Name, newItem.Name));

			// Teardown
			await todoStore.RemoveAsync(savedToDo.ID);
			kinveyClient.CurrentUser.Logout();
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
			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.NETWORK);
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
			kinveyClient.CurrentUser.Logout();
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
			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.SYNC);
			ToDo newItem = new ToDo();
			newItem.Name = "Task to save to SyncQ";
			newItem.Details = "A sync add test";
			newItem = await todoStore.SaveAsync(newItem);

			// Act
			PendingWriteAction pwa = kinveyClient.CacheManager.GetSyncQueue(collectionName).Peek();

			// Assert
			Assert.NotNull(pwa);
			Assert.IsNotNullOrEmpty(pwa.entityId);
			Assert.True(String.Equals(collectionName, pwa.collection));
			Assert.True(String.Equals("POST", pwa.action));

			// Teardown
			await todoStore.RemoveAsync(newItem.ID);
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		public async Task TestSyncQueuePush()
		{
			// Setup
			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			DataStore<ToDo> todoStore = DataStore<ToDo>.GetInstance(DataStoreType.SYNC, collectionName, kinveyClient);
			ToDo newItem = new ToDo();
			newItem.Name = "Task to update to SyncQ";
			newItem.Details = "A sync add test";
			newItem = await todoStore.SaveAsync(newItem);

			ToDo newItem2 = new ToDo();
			newItem2.Name = "Task to add another item to SyncQ";
			newItem2.Details = "Another sync add test";
			newItem2 = await todoStore.SaveAsync(newItem2);

			DataStore<FlashCard> flashCardStore = DataStore<FlashCard>.GetInstance(DataStoreType.SYNC, "FlashCard", kinveyClient);
			FlashCard firstFlashCard = new FlashCard();
			firstFlashCard.Question = "What is capital of Djibouti?";
			firstFlashCard.Answer = "Djibouti";
			firstFlashCard = await flashCardStore.SaveAsync(firstFlashCard);

			// Act
			DataStoreResponse dsr = await todoStore.SyncAsync();

			// Assert
			Assert.NotNull(dsr);
			Assert.IsNotNull(dsr.Errors);
			Assert.AreEqual(2, dsr.Count);

			// Teardown
			List<ToDo> listRemoveToDo = await todoStore.FindAsync();
			foreach (ToDo td in listRemoveToDo)
			{
				await todoStore.RemoveAsync(td.ID);
			}

			List<FlashCard> listRemoveFlash = await flashCardStore.FindAsync();
			foreach (FlashCard fc in listRemoveFlash)
			{
				await flashCardStore.RemoveAsync(fc.ID);
			}

			DataStoreResponse dsrDelete = await todoStore.SyncAsync();
			Assert.NotNull(dsrDelete);
			Assert.IsNotNull(dsrDelete.Errors);
			Assert.AreEqual(2, dsrDelete.Count);
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		public async Task TestSyncQueuePushUpdate()
		{
			// Setup
			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			DataStore<ToDo> todoStore = DataStore<ToDo>.GetInstance(DataStoreType.SYNC, collectionName, kinveyClient);
			ToDo newItem = new ToDo();
			newItem.Name = "Task to update to SyncQ";
			newItem.Details = "A sync add test";
			newItem = await todoStore.SaveAsync(newItem);

			newItem.DueDate = "2016-04-19T20:02:17.635Z";
			ToDo updatedItem = await todoStore.SaveAsync(newItem);

			// Act
			DataStoreResponse dsr = await todoStore.SyncAsync();

			// Assert
			Assert.NotNull(dsr);
			Assert.IsNotNull(dsr.Errors);
			Assert.AreEqual(1, dsr.Count);

			// Teardown
			List<ToDo> listToDo = await todoStore.FindAsync();
			KinveyDeleteResponse kdr = await todoStore.RemoveAsync(listToDo[0].ID);
			dsr = await todoStore.SyncAsync();
			Assert.NotNull(dsr);
			Assert.AreEqual(1, dsr.Count);
//			Assert.AreSame(dsr.Count, kdr.count);
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		public async Task TestSyncQueuePush10Items()
		{
			// Setup
			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			DataStore<ToDo> todoStore = DataStore<ToDo>.GetInstance(DataStoreType.SYNC, collectionName, kinveyClient);
			ToDo newItem1 = new ToDo();
			newItem1.Name = "Task to update to SyncQ";
			newItem1.Details = "A sync add test";
			newItem1 = await todoStore.SaveAsync(newItem1);

			ToDo newItem2 = new ToDo();
			newItem2.Name = "Task to update to SyncQ";
			newItem2.Details = "A sync add test";
			newItem2 = await todoStore.SaveAsync(newItem2);

			ToDo newItem3 = new ToDo();
			newItem3.Name = "Task to update to SyncQ";
			newItem3.Details = "A sync add test";
			newItem3 = await todoStore.SaveAsync(newItem3);

			ToDo newItem4 = new ToDo();
			newItem4.Name = "Task to update to SyncQ";
			newItem4.Details = "A sync add test";
			newItem4 = await todoStore.SaveAsync(newItem4);

			ToDo newItem5 = new ToDo();
			newItem5.Name = "Task to update to SyncQ";
			newItem5.Details = "A sync add test";
			newItem5 = await todoStore.SaveAsync(newItem5);

			ToDo newItem6 = new ToDo();
			newItem6.Name = "Task to update to SyncQ";
			newItem6.Details = "A sync add test";
			newItem6 = await todoStore.SaveAsync(newItem6);

			ToDo newItem7 = new ToDo();
			newItem7.Name = "Task to update to SyncQ";
			newItem7.Details = "A sync add test";
			newItem7 = await todoStore.SaveAsync(newItem7);

			ToDo newItem8 = new ToDo();
			newItem8.Name = "Task to update to SyncQ";
			newItem8.Details = "A sync add test";
			newItem8 = await todoStore.SaveAsync(newItem8);

			ToDo newItem9 = new ToDo();
			newItem9.Name = "Task to update to SyncQ";
			newItem9.Details = "A sync add test";
			newItem9 = await todoStore.SaveAsync(newItem9);

			ToDo newItem10 = new ToDo();
			newItem10.Name = "Task to update to SyncQ";
			newItem10.Details = "A sync add test";
			newItem10 = await todoStore.SaveAsync(newItem10);

			// Act
			DataStoreResponse dsr = await todoStore.SyncAsync();

			// Assert
			Assert.NotNull(dsr);
			Assert.IsNotNull(dsr.Errors);
			Assert.AreEqual(10, dsr.Count);

			// Teardown
			List<ToDo> listToDo = await todoStore.FindAsync();
			foreach (ToDo t in listToDo)
			{
				await todoStore.RemoveAsync(t.ID);
			}

			await todoStore.SyncAsync();
			kinveyClient.CurrentUser.Logout();
		}
	}
}
