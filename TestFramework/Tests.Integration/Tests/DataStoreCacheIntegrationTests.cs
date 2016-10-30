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

using KinveyXamarin;

namespace TestFramework
{
	[TestFixture]
	public class DataStoreCacheIntegrationTests
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
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestCacheStoreFindAsync()
		{
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestCacheStoreFindByIDAsync()
		{
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
			var query = todoStore.Where(x => x.Details.StartsWith("det", StringComparison.Ordinal)).Take(1);

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
			var query = todoStore.Where(x => x.Details.StartsWith("det", StringComparison.Ordinal)).Skip(1);

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
			var query = todoStore.Where(x => x.Details.StartsWith("det", StringComparison.Ordinal)).Skip(1).Take(1);

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
			Assert.True(listToDo.First().Details.Equals("details for 2"));
			Assert.True(listToDoCache.First().Details.Equals("details for 2"));
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
	}
}

