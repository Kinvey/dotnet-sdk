// Copyright (c) 2017, Kinvey, Inc. All rights reserved.
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
using System.Threading.Tasks;
using NUnit.Framework;
using Kinvey;

namespace TestFramework
{
	[TestFixture]
	public class RealtimeIntegrationTests
	{
		private Client kinveyClient;

		private const string collectionName = "ToDos";

		[SetUp]
		public void Setup()
		{
			string appKey = "kid_Zy0JOYPKkZ", appSecret = "d83de70e64d540e49acd6cfce31415df"; // UnitTestFramework
			Client.Builder builder = new Client.Builder(appKey, appSecret)
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
		public async Task TestRealtimeRegistration()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange

			// Act
			await Client.SharedClient.ActiveUser.RegisterRealtimeAsync();

			// Assert
			Assert.True(true);

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestRealtimeUnregistration()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange

			// Act
			await Client.SharedClient.ActiveUser.UnregisterRealtimeAsync();

			// Assert
			Assert.True(true);

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestRealtimeCollectionSubscription()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			var autoEvent = new System.Threading.AutoResetEvent(false);
			await Client.SharedClient.ActiveUser.RegisterRealtimeAsync();
			DataStore<ToDo> store = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK, Client.SharedClient);

			// Act
			ToDo ent = null;
			var realtimeDelegate = new KinveyDataStoreDelegate<ToDo>
			{
				OnError = (err) => { },
				OnNext = (entity) => {
					ent = entity;
					autoEvent.Set();
				},
				OnStatus = (status) => { }
			};

			bool result = await store.Subscribe(realtimeDelegate);

			// save to collection to trigger realtime update
			var todo = new ToDo();
			todo.Name = "Test Todo";
			todo.Details = "Test Todo Details";
			todo = await store.SaveAsync(todo);

			// Assert
			Assert.True(result);
			bool signal = autoEvent.WaitOne(10000);
			Assert.True(signal);
			Assert.NotNull(ent);
			Assert.AreEqual(0, ent.Name.CompareTo("Test Todo"));
			Assert.AreEqual(0, ent.Details.CompareTo("Test Todo Details"));

			// Teardown
			await store.RemoveAsync(todo.ID);
			await store.Unsubscribe();
			await Client.SharedClient.ActiveUser.UnregisterRealtimeAsync();
			kinveyClient.ActiveUser.Logout();
		}

		[Test]
		public async Task TestRealtimeUserCommunication()
		{
			// Setup
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			var autoEvent = new System.Threading.AutoResetEvent(false);
			await Client.SharedClient.ActiveUser.RegisterRealtimeAsync();

			// Create stream object corresponding to "meddevcmds" stream created on the backend
			Stream<ToDo> stream = new Stream<ToDo>("todo_stream");

			// Grant stream access to active user for both publish and subscribe actions
			var streamACL = new StreamAccessControlList();
			streamACL.Publishers.Add(Client.SharedClient.ActiveUser.Id);
			streamACL.Subscribers.Add(Client.SharedClient.ActiveUser.Id);
			bool resultGrant = await stream.GrantStreamAccess(Client.SharedClient.ActiveUser.Id, streamACL);

			// Realtime delegate setup
			ToDo ent = null;
			string sender = String.Empty;
			KinveyStreamDelegate<ToDo> realtimeDelegate = new KinveyStreamDelegate<ToDo>
			{
				OnError = (err) => Console.WriteLine("STREAM Error: " + err.Message),
				OnNext = (senderID, message) => {
					ent = message;
					sender = senderID;
					autoEvent.Set();
				},
				OnStatus = (status) => {
					Console.WriteLine("Status: " + status.Status);
					Console.WriteLine("Status Message: " + status.Message);
					Console.WriteLine("Status Channel: " + status.Channel);
					Console.WriteLine("Status Channel Group: " + status.ChannelGroup);
				}
			};

			// Act
			bool result = await stream.Listen(realtimeDelegate);
			var streamTodo = new ToDo();
			streamTodo.Name = "stream test";
			streamTodo.Details = "Stream Details";
			bool publishResult = await stream.Send(Client.SharedClient.ActiveUser.Id, streamTodo);

			// Assert
			Assert.True(result);
			Assert.True(publishResult);
			bool signal = autoEvent.WaitOne(10000);
			Assert.True(signal);
			Assert.NotNull(ent);
			Assert.AreEqual(0, ent.Name.CompareTo("stream test"));
			Assert.AreEqual(0, ent.Details.CompareTo("Stream Details"));
			Assert.AreEqual(Client.SharedClient.ActiveUser.Id, sender);

			// Teardown
			await stream.StopListening();
			await Client.SharedClient.ActiveUser.UnregisterRealtimeAsync();
			kinveyClient.ActiveUser.Logout();
		}
	}
}
