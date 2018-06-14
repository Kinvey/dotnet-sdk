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
	public class DataStoreCacheIntegrationTests
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
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE, kinveyClient);

			// Assert
			Assert.NotNull(todoStore);
			Assert.True(string.Equals(todoStore.CollectionName, collectionName));
		}

		[Test]
		public async Task TestCollectionSharedClient()
		{
			// Arrange

			// Act
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);

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
			DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.CACHE);

			// Act
			todoStore.DeltaSetFetchingEnabled = true;

			// Assert
			Assert.True(todoStore.DeltaSetFetchingEnabled);
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestCacheStoreFindAsync()
		{
		}

		[Test]
		public async Task TestCacheStoreFindByIDAsync()
		{
			// Setup
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
			Assert.NotNull(networkEntity);
			Assert.True(string.Equals(networkEntity.ID, t.ID));
			Assert.NotNull(cacheEntity);
			Assert.True(string.Equals(cacheEntity.ID, t.ID));
			Assert.True(string.Equals(cacheEntity.ID, networkEntity.ID));

			// Teardown
			await todoStore.RemoveAsync(t.ID);
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestCacheStoreFindByIDsAsync()
		{
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
			Assert.IsNotEmpty(listToDo);
			Assert.AreEqual(4, listToDo.Count); // 2 from local, 2 from network
		}

		[Test]
		public async Task TestCacheStoreFindByQueryTake1()
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
			Assert.IsNotEmpty(listToDo);
			Assert.IsNotNull(listToDoCache);
			Assert.IsNotEmpty(listToDoCache);
			Assert.AreEqual(1, listToDoCache.Count); // take 1 from local instead of both
			Assert.AreEqual(2, listToDo.Count); // 1 from local, 1 from network
		}

		[Test]
		public async Task TestCacheStoreFindByQuerySkip1()
		{
			// Setup
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
			Assert.IsNotEmpty(listToDo);
			Assert.IsNotNull(listToDoCache);
			Assert.IsNotEmpty(listToDoCache);
			Assert.AreEqual(2, listToDoCache.Count); // take 2 from local instead all 3
			Assert.AreEqual(4, listToDo.Count); // 2 from local, 2 from network
		}

		[Test]
		public async Task TestCacheStoreFindByQuerySkip1Take1()
		{
			// Setup
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
			Assert.IsNotEmpty(listToDo);
			Assert.IsNotNull(listToDoCache);
			Assert.IsNotEmpty(listToDoCache);
			Assert.AreEqual(1, listToDoCache.Count); // take 1 from local instead of both
			Assert.AreEqual(2, listToDo.Count); // 1 from local, 1 from network
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
		public async Task TestGetCountAsyncBad()
		{
			// Arrange

			// Act

			// Assert
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
			Assert.NotNull(savedItem);
			Assert.True(string.Equals(newItem.Details, savedItem.Details));

			// Teardown
			await todoStore.RemoveAsync(savedItem.ID);
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		[Ignore("Placeholder - Not ready for testing yet")]
		public async Task TestSaveListOfItemsAsync()
		{
			// Setup
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
			kinveyClient.ActiveUser.Logout();
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
			Assert.NotNull(listEntities);
			Assert.AreEqual(3, listEntities.Count);

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			await todoStore.RemoveAsync(newItem3.ID);
			kinveyClient.ActiveUser.Logout();
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
			Assert.CatchAsync(async delegate ()
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

		[Test]
		public async Task TestRemoveAsync()
		{
			// Setup
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
			Assert.NotNull(kdr);
			Assert.AreEqual(1, kdr.count);

			// Teardown
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
			Assert.NotNull(kdr);
			Assert.AreEqual(3, kdr.count);

			// Teardown
			await todoStore.RemoveAsync(newItem1.ID);
			await todoStore.RemoveAsync(newItem2.ID);
			await todoStore.RemoveAsync(newItem3.ID);
			kinveyClient.ActiveUser.Logout();
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

        [Test(Description = "with enabled deltaset should return only changes since last request")]
        public async Task TestDeltaSetPullReturnOnlyChanges()
        {
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

        [Test(Description = "with enabled deltaset and query should return correct number of updated items")]
        public async Task TestDeltaSetPullReturnOnlyUpdates()
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

        [Test(Description = "with enabled deltaset should return correct number of items when updating")]
        public async Task TestDeltaSetPullReturnCorrectNumberOfUpdates()
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

        [Test(Description = "with enabled deltaset should return correct number of items when deleting and updating")]
        public async Task TestDeltaSetPullReturnCorrectNumberOfDeletesAndUpdates()
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

        [Test(Description = "with enabled deltaset should return only changes since last request")]
        public async Task TestDeltaSetSyncReturnOnlyChanges()
        {
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

        [Test(Description = "with enabled deltaset and query should return correct number of updated items")]
        public async Task TestDeltaSetSyncReturnOnlyUpdates()
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

        [Test(Description = "with enabled deltaset should return correct number of items when updating")]
        public async Task TestDeltaSetSyncReturnCorrectNumberOfUpdates()
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

        [Test(Description = "with enabled deltaset should return correct number of items when deleting and updating")]
        public async Task TestDeltaSetSyncReturnCorrectNumberOfDeletesAndUpdates()
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

