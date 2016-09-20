using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Moq;
using NUnit.Framework;

using KinveyXamarin;

namespace UnitTestFramework
{
	[TestFixture]
	public class TestDataStore
	{
		private Client kinveyClient;

		private const string collectionName = "ToDos";

		[SetUp]
		public async Task Setup()
		{
			Client.Builder builder = new Client.Builder(TestSetup.app_key, TestSetup.app_secret)
				.setFilePath(TestSetup.db_dir)
				.setOfflinePlatform(new SQLite.Net.Platform.Generic.SQLitePlatformGeneric());

			kinveyClient = await builder.Build();
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
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK, kinveyClient);

			// Assert
			Assert.NotNull(todoStore);
			Assert.True(string.Equals(todoStore.CollectionName, collectionName));
		}

		[Test]
		public async Task TestCollectionSharedClient()
		{
			// Arrange

			// Act
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

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
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

			// Act
			todoStore.DeltaSetFetchingEnabled = true;

			// Assert
			Assert.True(todoStore.DeltaSetFetchingEnabled);
		}

		[Test]
		public async Task TestDeltaSetPullAsync()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);
			todoStore.DeltaSetFetchingEnabled = true;

			ToDo newItem = new ToDo();
			newItem.Name = "Next Task";
			newItem.Details = "A test";
			newItem.DueDate = "2016-04-19T20:02:17.635Z";
			ToDo t = await todoStore.SaveAsync(newItem);

			ToDo anotherNewItem = new ToDo();
			anotherNewItem.Name = "Another Next Task";
			anotherNewItem.Details = "Another test";
			anotherNewItem.DueDate = "2016-05-19T20:02:18.876Z";
			ToDo t2 = await todoStore.SaveAsync(anotherNewItem);

			// Act
			List<ToDo> results = await todoStore.PullAsync();

			// Assert
			Assert.NotNull(results);
			Assert.IsEmpty(results);

			// Teardown
			await todoStore.RemoveAsync(t.ID);
			await todoStore.RemoveAsync(t2.ID);
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestDeltaSetPullTwiceAsync()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			DataStore<LongData> longdataStore = DataStore<LongData>.Collection("longdata", DataStoreType.CACHE);
			//for (int i = 1; i <= 10000; i++)
			//{
			//	LongData ld = new LongData();
			//	ld.FirstName = "Abe";
			//	ld.LastName = "Lincoln";
			//	ld.Age = 50;
			//	ld.City = "Washington D.C.";
			//	ld.Dollar = "$0.01";
			//	ld.Paragraph = "Four score and seven years ago...";
			//	ld.Pick = "UNION";
			//	ld.State = "IL";
			//	ld.Zip = "12345";
			//	ld.Sequence = i;
			//	ld.Street = "1600 Pennsylvania Avenue";
			//	await longdataStore.SaveAsync(ld);
			//}

			// Act

			//System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
			//stopwatch.Start();

			List<LongData> listResultsCache = new List<LongData>();
			await longdataStore.FindAsync();

			//stopwatch.Stop();
			//TimeSpan timeForFirstFetch = stopwatch.Elapsed;
			//stopwatch.Reset();

			//int count = 0;
			//int limit = 5000;
			//DataStore<LongData> longdataNetworkStore = DataStore<LongData>.Collection("longdata", DataStoreType.NETWORK);
			//List<LongData> networkhits = await longdataNetworkStore.FindAsync();
			//foreach (var longdata in networkhits)
			//{
			//	if (count >= limit)
			//	{
			//		break;
			//	}
			//
			//	if (longdata.State.Count() == 3)
			//	{
			//		//longdata.State += "Z";
			//		longdata.State = longdata.State.Substring(0,2);
			//	}
			//
			//	await longdataNetworkStore.SaveAsync(longdata);
			//	count++;
			//}

			//stopwatch.Start();

			longdataStore.DeltaSetFetchingEnabled = true;
			List<LongData> listResultsSecond = await longdataStore.PullAsync();

			//stopwatch.Stop();
			//TimeSpan timeForSecondFetch = stopwatch.Elapsed;

			// Assert
			Assert.NotNull(listResultsCache);
			Assert.IsEmpty(listResultsCache);

			Assert.NotNull(listResultsSecond);
			Assert.IsEmpty(listResultsSecond);

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestNetworkStoreFindAsyncBad()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			Mock<RestSharp.IRestClient> moqRC = new Mock<RestSharp.IRestClient>();
			RestSharp.IRestResponse resp = new RestSharp.RestResponse();
			resp.Content = "MOCK RESPONSE";
			moqRC.Setup(m => m.ExecuteAsync(It.IsAny<RestSharp.IRestRequest>())).ReturnsAsync(resp);

			Client.Builder cb = new Client.Builder(TestSetup.app_key, TestSetup.app_secret)
				.setFilePath(TestSetup.db_dir)
				.setOfflinePlatform(new SQLite.Net.Platform.Generic.SQLitePlatformGeneric())
				.SetRestClient(moqRC.Object);

			Client c = await cb.Build();

			// Arrange
			DataStore<ToDo> store = DataStore<ToDo>.Collection("todos", DataStoreType.NETWORK, c);

			// Act
			// Assert
			Exception er = Assert.CatchAsync(async delegate ()
			{
				await store.FindAsync();
			});

			Assert.NotNull(er);
			KinveyException ke = er as KinveyException;
			Assert.AreEqual(EnumErrorCode.ERROR_JSON_PARSE, ke.ErrorCode);

			// Teardown
			c.ActiveUser.Logout();
		}

		[Test]
		public async Task TestNetworkStoreFindAsync()
		{
			// Setup
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
			Assert.NotNull(todoList);
			Assert.AreEqual(2, todoList.Count);

			// Teardown
			await todoStore.RemoveAsync(t.ID);
			await todoStore.RemoveAsync(t2.ID);
			kinveyClient.ActiveUser.Logout();
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
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestCacheStoreFindAsync()
		{
		}

		[Test]
		public async Task TestNetworkStoreFindByIDAsync()
		{
			// Setup
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
			entity = (await todoStore.FindAsync(t.ID)).First();

			// Assert
			Assert.NotNull(entity);
			Assert.True(string.Equals(entity.ID, t.ID));

			// Teardown
			await todoStore.RemoveAsync(t.ID);
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
			entity = (await todoStore.FindAsync(t.ID)).First();

			// Assert
			Assert.NotNull(entity);
			Assert.True(string.Equals(entity.ID, t.ID));

			// Teardown
			await todoStore.RemoveAsync(t.ID);
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestCacheStoreFindByIDAsync()
		{
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestNetworkStoreFindByIDsAsync()
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
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestCacheStoreFindByIDsAsync()
		{
		}

		[Test]
		public async Task TestNetworkStoreFindByQuery()
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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(2, listToDo.Count);
		}

		[Test]
		public async Task TestNetworkStoreFindByQueryWithSkip()
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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(1, listToDo.Count);
		}

		[Test]
		public async Task TestNetworkStoreFindByQueryWithLimit()
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

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);

			// Act
			var query = todoStore.Where(x => x.Details.StartsWith("det")).Take(1);

			List<ToDo> listToDo = new List<ToDo>();

			listToDo = await todoStore.FindAsync(query);

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.IsNotNull(listToDo);
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(1, listToDo.Count);
		}

		[Test]
		public async Task TestNetworkStoreFindByQueryWithSortAscending()
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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(2, listToDo.Count);
			Assert.True(String.Compare(newItem2.Name, listToDo.First().Name) == 0);
		}

		[Test]
		public async Task TestNetworkStoreFindByQueryWithSortDescending()
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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(2, listToDo.Count);
			Assert.True(String.Compare(newItem1.Name, listToDo.First().Name) == 0);
		}

		[Test]
		public async Task TestNetworkStoreFindByQueryWithSelectField()
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

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(2, listToDo.Count);
			Assert.NotNull(listToDo[0].Name);
			Assert.Null(listToDo[0].Details);
			Assert.NotNull(listToDo[1].Name);
			Assert.Null(listToDo[1].Details);
		}

		[Test]
		public async Task TestNetworkStoreFindByQueryWithSelectFields()
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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(2, listToDo.Count);
			Assert.NotNull(listToDo[0].Name);
			Assert.NotNull(listToDo[0].Details);
			Assert.Null(listToDo[0].DueDate);
			Assert.NotNull(listToDo[1].Name);
			Assert.NotNull(listToDo[1].Details);
			Assert.Null(listToDo[1].DueDate);
		}

		[Test]
		public async Task TestNetworkStoreFindByQueryWithSelectFieldsBad()
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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(2, listToDo.Count);
			Assert.NotNull(listToDo[0].Name);
			Assert.NotNull(listToDo[0].Details);
			Assert.NotNull(listToDo[0].DueDate);
			Assert.NotNull(listToDo[1].Name);
			Assert.NotNull(listToDo[1].Details);
			Assert.NotNull(listToDo[1].DueDate);
		}

		[Test]
		public async Task TestNetworkStoreFindByQueryLogicalAnd()
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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(1, listToDo.Count);
		}

		[Test]
		public async Task TestNetworkStoreFindByQueryLogicalOr()
		{
			// Setup
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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(2, listToDo.Count);
		}

		[Test]
		public async Task TestNetworkStoreFindByQueryMultipleWhereClauses()
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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(1, listToDo.Count);
		}

		[Test]
		public async Task TestNetworkStoreFindByQueryLogicalAndWithOr()
		{
			// Setup
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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(2, listToDo.Count);
		}

		[Test]
		public async Task TestNetworkStoreFindByQueryLogicalOrWithAnd()
		{
			// Setup
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

			List<ToDo> listToDo = new List<ToDo>();

			listToDo = await todoStore.FindAsync(query);


			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			kinveyClient.ActiveUser.Logout();

			// Assert
			Assert.IsNotNull(listToDo);
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(1, listToDo.Count);
		}

		[Test]
		public async Task TestNetworkStoreFindByQueryMultipleWhereClausesWithLogicalAnd()
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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(1, listToDo.Count);
		}

		[Test]
		public async Task TestNetworkStoreFindByQueryMultipleWhereClausesWithLogicalOr()
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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(1, listToDo.Count);
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
		public async Task TestCacheStoreFindByQuery()
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

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);

			newItem1 = await todoStore.SaveAsync(newItem1);
			newItem2 = await todoStore.SaveAsync(newItem2);

			// Act
			//			var query = from todo in todoStore
			//						where todo.Details.StartsWith("details for 2")
			//						select todo;

			List<ToDo> listToDo = new List<ToDo>();
			List<ToDo> listToDoCache = new List<ToDo>();
			var query = todoStore.Where(x => x.Details.StartsWith("det", StringComparison.Ordinal));

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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(4, listToDo.Count); // 2 from local, 2 from network
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


			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);
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

			var query = personStore.Where(x => x.LastName.Equals("Bluth", StringComparison.Ordinal));

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

			var query = personStore.Where(x => x.LastName.Equals("Bluth", StringComparison.Ordinal));

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
		public async Task TestNetworkStoreGetSumAsync()
		{
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

			var query = personStore.Where(x => x.LastName.Equals("Bluth", StringComparison.Ordinal));

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

		[Test]
		public async Task TestNetworkStoreGetMinAsync()
		{
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

		[Test]
		public async Task TestNetworkStoreGetMaxAsync()
		{
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

		[Test]
		public async Task TestNetworkStoreGetAverageAsync()
		{
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

		[Test]
		public async Task TestCacheStoreGetSumAsync()
		{
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

			var query = personStore.Where(x => x.LastName.Equals("Bluth", StringComparison.Ordinal));

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

		[Test]
		public async Task TestCacheStoreGetMinAsync()
		{
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

		[Test]
		public async Task TestCacheStoreGetMaxAsync()
		{
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

		[Test]
		public async Task TestCacheStoreGetAverageAsync()
		{
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
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);

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
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK);
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
		public async Task TestStoreInvalidOperation () {
			// Setup
			await User.LoginAsync (TestSetup.user, TestSetup.pass, kinveyClient);

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK, kinveyClient);

			Assert.CatchAsync(async delegate () {
				await todoStore.PullAsync ();
			});

			Assert.CatchAsync(async delegate () {
				await todoStore.PushAsync ();
			});

			Assert.CatchAsync(async delegate () {
				await todoStore.SyncAsync ();
			});
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
			DataStoreResponse dsr = await todoStore.SyncAsync();

			// Assert
			Assert.NotNull(dsr);
			Assert.IsNotNull(dsr.Errors);
			Assert.AreEqual(2, dsr.Count);

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

			DataStoreResponse dsrDelete = await todoStore.SyncAsync();
			Assert.NotNull(dsrDelete);
			Assert.IsNotNull(dsrDelete.Errors);
			Assert.AreEqual(2, dsrDelete.Count);
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
			DataStoreResponse dsr = await todoStore.SyncAsync();

			// Assert
			Assert.NotNull(dsr);
			Assert.IsNotNull(dsr.Errors);
			Assert.AreEqual(1, dsr.Count);

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
			Assert.AreEqual(1, dsr.Count);
//			Assert.AreSame(dsr.Count, kdr.count);
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestSyncQueueCount ()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC, kinveyClient);
			ToDo newItem = new ToDo ();
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
		public async Task TestSyncStorePullAsync ()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC);

			List<ToDo> todosBeforeSave = await todoStore.PullAsync ();

			// Assert
			Assert.IsNotNull (todosBeforeSave);
			Assert.IsEmpty (todosBeforeSave);

			// Arrange
			ToDo newItem = new ToDo ();
			newItem.Name = "Next Task";
			newItem.Details = "A test";
			newItem.DueDate = "2016-04-19T20:02:17.635Z";

			ToDo t = await todoStore.SaveAsync (newItem);

			ToDo anotherNewItem = new ToDo ();
			anotherNewItem.Name = "Another Next Task";
			anotherNewItem.Details = "Another test";
			anotherNewItem.DueDate = "2016-05-19T20:02:17.635Z";
			ToDo t2 = await todoStore.SaveAsync (anotherNewItem);


			await todoStore.PushAsync ();

			List<ToDo> todosAfterSave = await todoStore.PullAsync ();

			// Assert
			Assert.NotNull (todosAfterSave);
			Assert.AreEqual (2, todosAfterSave.Count);

			// Teardown
			foreach (var todo in todosAfterSave)
			{
				await todoStore.RemoveAsync(todo.ID);
			}
			await todoStore.PushAsync();
			kinveyClient.ActiveUser.Logout ();
		}
	}
}
