using System;
using System.Collections.Generic;
using NUnit.Framework;
using KinveyXamarin;
using System.Threading.Tasks;

namespace UnitTestFramework
{
	[TestFixture]
	public class TestAppData
	{
		private Client kinveyClient;

		private const string user = "testuser";
		private const string pass = "testpass";

		private const string app_key = "kid_Zy0JOYPKkZ";
		private const string app_secret = "d83de70e64d540e49acd6cfce31415df";

		private const string collectionName = "ToDos";

		private const string db_dir = "../../../UnitTests/TestFiles/";
		private const string SQLiteOfflineStoreFilePath = db_dir + "kinveyOffline.sqlite";
		private const string SQLiteCredentialStoreFilePath = db_dir + "kinvey_tokens.sqlite";

		[SetUp]
		public void Setup ()
		{
			kinveyClient = new Client.Builder(app_key, app_secret)
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
		public async Task TestGetEntityAsync()
		{
			// Setup
			await kinveyClient.CurrentUser.LoginAsync(user, pass);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "Next Task";
			newItem.Details = "A test";
			newItem.DueDate = "2016-04-19T20:02:17.635Z";
			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.NETWORK);
			ToDo t = await todoStore.SaveAsync(newItem);

			// Act
			List<ToDo> todoList = await todoStore.GetAsync();

			// Assert
			Assert.NotNull(todoList);
			Assert.AreEqual(1, todoList.Count);

			// Teardown
			await todoStore.DeleteAsync(t.ID);
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
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestGetAsync()
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
		public async Task TestGetCountAsync()
		{
			// Setup
			await kinveyClient.CurrentUser.LoginAsync(user, pass);

			// Arrange
			ToDo newItem = new ToDo();
			newItem.Name = "Next Task";
			newItem.Details = "A test";
			newItem.DueDate = "2016-04-19T20:02:17.635Z";
			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.NETWORK);
			ToDo t = await todoStore.SaveAsync(newItem);

			// Act
			uint count = await todoStore.GetCountAsync();

			// Assert
			Assert.GreaterOrEqual(count, 0);
			Assert.AreEqual(1, count);

			// Teardown
			await todoStore.DeleteAsync(t.ID);
			kinveyClient.CurrentUser.Logout();
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
			await kinveyClient.CurrentUser.LoginAsync(user, pass);

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
			await todoStore.DeleteAsync(savedToDo.ID);
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
			await kinveyClient.CurrentUser.LoginAsync(user, pass);

			// Arrange
			DataStore<ToDo> todoStore = kinveyClient.AppData<ToDo>(collectionName, DataStoreType.NETWORK);
			ToDo newItem = new ToDo();
			newItem.Name = "Task to Delete";
			newItem.Details = "A delete test";
			ToDo deleteToDo = await todoStore.SaveAsync(newItem);

			// Act
			KinveyDeleteResponse kdr = await todoStore.DeleteAsync(deleteToDo.ID);

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
	}
}
