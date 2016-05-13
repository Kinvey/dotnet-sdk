using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using KinveyXamarin;
using System.Threading.Tasks;

namespace UnitTestFramework
{
	[TestFixture]
	public class TestSQLiteCache
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
			System.IO.File.Delete(SQLiteOfflineStoreFilePath);
			System.IO.File.Delete(SQLiteCredentialStoreFilePath);
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestGetInstance()
		{
			// Arrange
			// Act
			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.CACHE);

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
		public async Task TestSaveAsync()
		{
			// Setup
			if (kinveyClient.CurrentUser.isUserLoggedIn())
			{
				kinveyClient.CurrentUser.Logout();
			}

			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "todo save";
			newItem.Details = "details for save";
			newItem.DueDate = "2016-04-22T19:56:00.961Z";
			//			KinveyMetaData kmd = new KinveyMetaData();
			//			kmd.entityCreationTime = "2016-04-22T19:56:00.900Z";
			//			kmd.lastModifiedTime = "2016-04-22T19:56:00.902Z";
			//			newItem.Metadata = kmd;

			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.CACHE);

			// Act
			ToDo savedItem = await todoStore.SaveAsync(newItem);

			// Assert
			Assert.NotNull(savedItem);
			Assert.True(string.Equals(newItem.Details, savedItem.Details));

			// Teardown
			await todoStore.DeleteAsync(savedItem.ID);
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
		[Ignore("Placeholder - Not ready for testing yet")]
		public async Task TestSaveListOfItemsAsync()
		{
			// Setup
			if (kinveyClient.CurrentUser.isUserLoggedIn())
			{
				kinveyClient.CurrentUser.Logout();
			}

			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

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

			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.CACHE);
//
//			newItem1 = await todoStore.SaveAsync(newItem1);
//			newItem2 = await todoStore.SaveAsync(newItem2);
//			newItem3 = await todoStore.SaveAsync(newItem3);

			List<ToDo> listToDos = new List<ToDo>();
			listToDos.Add(newItem1);
			listToDos.Add(newItem2);
			listToDos.Add(newItem3);

			// Act
			ICache<ToDo> cache = kinveyClient.CacheManager.GetCache<ToDo>(collectionName);
			List<ToDo> listEntities = cache.Save(listToDos);

			// Assert
			Assert.NotNull(listEntities);
			Assert.IsNotEmpty(listEntities);
			Assert.AreEqual(3, listEntities.Count);

			// Teardown
//			await todoStore.DeleteAsync(newItem1.ID);
//			await todoStore.DeleteAsync(newItem2.ID);
//			await todoStore.DeleteAsync(newItem3.ID);
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestSaveListOfItemsAsyncBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		public async Task TestGetAsync()
		{
			// Setup
			if (kinveyClient.CurrentUser.isUserLoggedIn())
			{
				kinveyClient.CurrentUser.Logout();
			}

			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

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

			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.CACHE);
			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);
			newItem3 = await todoStore.SaveAsync(newItem3);
			DataStore<ToDo> todoStore1 = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.CACHE);

			// Act
			List<ToDo> todoList = await todoStore1.GetAsync();

			// Assert
			Assert.NotNull(todoList);
			Assert.AreEqual(3, todoList.Count);

			// Teardown
			await todoStore.DeleteAsync(newItem1.ID);
			await todoStore1.DeleteAsync(newItem2.ID);
			await todoStore.DeleteAsync(newItem3.ID);
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
		public async Task TestGetByIDAsync()
		{
			// Setup
			if (kinveyClient.CurrentUser.isUserLoggedIn())
			{
				kinveyClient.CurrentUser.Logout();
			}

			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

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

			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.CACHE);
			newItem1 = await todoStore.SaveAsync(newItem1);
			ToDo savedItemToFind = await todoStore.SaveAsync(newItem2);
			newItem3 = await todoStore.SaveAsync(newItem3);

			// Act
			ToDo entity = await todoStore.GetEntityAsync(savedItemToFind.ID);

			// Assert
			Assert.NotNull(entity);
			Assert.True(string.Equals(entity.ID, savedItemToFind.ID));

			// Teardown
			await todoStore.DeleteAsync(newItem1.ID);
			await todoStore.DeleteAsync(savedItemToFind.ID);
			await todoStore.DeleteAsync(newItem3.ID);
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestGetByIDAsyncBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		public async Task TestGetByListOfIDs()
		{
			// Setup
			if (kinveyClient.CurrentUser.isUserLoggedIn())
			{
				kinveyClient.CurrentUser.Logout();
			}

			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

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

			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.CACHE);

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
			Assert.NotNull(listEntities);
			Assert.AreEqual(3, listEntities.Count);

			// Teardown
			await todoStore.DeleteAsync(newItem1.ID);
			await todoStore.DeleteAsync(newItem2.ID);
			await todoStore.DeleteAsync(newItem3.ID);
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestGetByListOfIDsBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		public async Task TestGetByQueryAsync()
		{
			// Setup
			if (kinveyClient.CurrentUser.isUserLoggedIn())
			{
				kinveyClient.CurrentUser.Logout();
			}

			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

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

			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.CACHE);

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
			foreach (var todo in query)
			{
				listEntities.Add(todo);
			}

			// Assert
			Assert.NotNull(listEntities);
			Assert.AreEqual(3, listEntities.Count);

			// Teardown
			await todoStore.DeleteAsync(newItem1.ID);
			await todoStore.DeleteAsync(newItem2.ID);
			await todoStore.DeleteAsync(newItem3.ID);
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestGetByQueryAsyncBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		public async Task TestDeleteAsync()
		{
			// Setup
			if (kinveyClient.CurrentUser.isUserLoggedIn())
			{
				kinveyClient.CurrentUser.Logout();
			}

			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "todo save";
			newItem.Details = "details for save";
			newItem.DueDate = "2016-04-22T19:56:00.961Z";
			//			KinveyMetaData kmd = new KinveyMetaData();
			//			kmd.entityCreationTime = "2016-04-22T19:56:00.900Z";
			//			kmd.lastModifiedTime = "2016-04-22T19:56:00.902Z";
			//			newItem.Metadata = kmd;

			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.CACHE);
			ToDo savedItem = await todoStore.SaveAsync(newItem);
			string savedItemID = savedItem.ID;

			// Act
			KinveyDeleteResponse kdr = await todoStore.DeleteAsync(savedItemID);

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
		public async Task TestDeleteByListOfIDs()
		{
			// Setup
			if (kinveyClient.CurrentUser.isUserLoggedIn())
			{
				kinveyClient.CurrentUser.Logout();
			}

			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

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

			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.CACHE);

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
			Assert.NotNull(kdr);
			Assert.AreEqual(3, kdr.count);

			// Teardown
			await todoStore.DeleteAsync(newItem1.ID);
			await todoStore.DeleteAsync(newItem2.ID);
			await todoStore.DeleteAsync(newItem3.ID);
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestDeleteByListOfIDsBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		public async Task TestLINQSelect()
		{
			// Setup
			if (kinveyClient.CurrentUser.isUserLoggedIn())
			{
				kinveyClient.CurrentUser.Logout();
			}

			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			ToDo newItem1 = new ToDo();
			newItem1.Name = "todo";
			newItem1.Details = "details for 1";
			newItem1.DueDate = "2016-04-22T19:56:00.963Z";

			ToDo newItem2 = new ToDo();
			newItem2.Name = "another todo";
			newItem2.Details = "details for 2";
			newItem2.DueDate = "2016-04-22T19:56:00.963Z";

			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.CACHE);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);

			// Act
//			var query = from todo in todoStore
//						where todo.Details.StartsWith("details for 2")
//						select todo;

			var query = todoStore.Where(x => x.Name.StartsWith("anoth"));
			List<ToDo> listToDo = new List<ToDo>();
			foreach (ToDo todo in query)
			{
				listToDo.Add(todo);
			}

			// Assert
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(1, listToDo.Count);

			// Teardown
			await todoStore.DeleteAsync(newItem1.ID);
			await todoStore.DeleteAsync(newItem2.ID);
			kinveyClient.CurrentUser.Logout();
		}
	}
}
