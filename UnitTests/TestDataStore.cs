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
		public async Task TestFindByIDAsync()
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
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestGetEntityAsyncBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		public async Task TestFindAsync()
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
		public async Task TestSyncQueueUpdate()
		{
			// Setup
			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.SYNC);
			ToDo newItem = new ToDo();
			newItem.Name = "Task to update to SyncQ";
			newItem.Details = "A sync add test";
			newItem = await todoStore.SaveAsync(newItem);

			newItem.Details = "A sync update test";
			ToDo updatedItem = await todoStore.SaveAsync(newItem);

			// Act
			PendingWriteAction pwa = kinveyClient.CacheManager.GetSyncQueue(collectionName).Peek();

			// Assert
			Assert.NotNull(pwa);
			Assert.IsNotNullOrEmpty(pwa.entityId);
			Assert.True(String.Equals(collectionName, pwa.collection));
			Assert.True(String.Equals("PUT", pwa.action));

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
//			PendingWriteAction pwa = kinveyClient.CacheManager.GetSyncQueue(collectionName).Peek();

			// Teardown
			await todoStore.RemoveAsync(newItem.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			await flashCardStore.RemoveAsync(firstFlashCard.ID);
			kinveyClient.CurrentUser.Logout();

			// Assert
			Assert.NotNull(dsr);
			Assert.IsNotNull(dsr.Errors);
			Assert.AreEqual(2, dsr.Count);
		}
	}
}
