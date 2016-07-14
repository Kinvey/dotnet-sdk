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

		[SetUp]
		public void Setup ()
		{
			kinveyClient = new Client.Builder(TestSetup.app_key, TestSetup.app_secret)
				.setFilePath(TestSetup.db_dir)
				.setOfflinePlatform(new SQLite.Net.Platform.Generic.SQLitePlatformGeneric())
				.build();
		}

		[TearDown]
		public void Tear ()
		{
			System.IO.File.Delete(TestSetup.SQLiteOfflineStoreFilePath);
			System.IO.File.Delete(TestSetup.SQLiteCredentialStoreFilePath);
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
			await todoStore.RemoveAsync(savedItem.ID);
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
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestGetAsyncBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestFindByIDAsyncBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		public async Task TestFindByListOfIDs()
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
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			await todoStore.RemoveAsync(newItem3.ID);
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestFindByListOfIDsBad()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestGetByQueryAsync()
		{
			// Arrange

			// Act

			// Assert
		}

		[Test]
		public async Task TestGetByQueryAsyncBad()
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

			// Assert
			Assert.CatchAsync(async delegate() {
				foreach (var todo in query)
				{
					listEntities.Add(todo);
				}
			});

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			await todoStore.RemoveAsync(newItem3.ID);
			kinveyClient.CurrentUser.Logout();
		}

		[Test]
		public async Task TestRemoveAsync()
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
			KinveyDeleteResponse kdr = await todoStore.RemoveAsync(savedItemID);

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
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			await todoStore.RemoveAsync(newItem3.ID);
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
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestLINQSelect()
		{
//			// Setup
//			if (kinveyClient.CurrentUser.isUserLoggedIn())
//			{
//				kinveyClient.CurrentUser.Logout();
//			}
//
//			await kinveyClient.CurrentUser.LoginAsync(TestSetup.user, TestSetup.pass);
//
//			// Arrange
//			ToDo newItem1 = new ToDo();
//			newItem1.Name = "todo";
//			newItem1.Details = "details for 1";
//			newItem1.DueDate = "2016-04-22T19:56:00.963Z";
//
//			ToDo newItem2 = new ToDo();
//			newItem2.Name = "another todo";
//			newItem2.Details = "details for 2";
//			newItem2.DueDate = "2016-04-22T19:56:00.963Z";
//
//			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.CACHE);
//
//			newItem1 = await todoStore.SaveAsync(newItem1);
//			newItem2 = await todoStore.SaveAsync(newItem2);
//
//			// Act
////			var query = from todo in todoStore
////						where todo.Details.StartsWith("details for 2")
////						select todo;
//
//			var query = todoStore.Where(x => x.Name.StartsWith("anoth"));
//			List<ToDo> listToDo = await todoStore.FindAsync(query);
//
////			foreach (ToDo td in query2)
////			{
////				listToDo2.Add(td);
////			}
//
//			// Assert
//			Assert.IsNotEmpty(listToDo);
//			Assert.AreEqual(1, listToDo.Count);
//
//			// Teardown
//			await todoStore.RemoveAsync(newItem1.ID);
//			await todoStore.RemoveAsync(newItem2.ID);
//			kinveyClient.CurrentUser.Logout();
		}
	}
}
