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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kinvey;

namespace Kinvey.Tests
{
	[TestClass]
    public class RealtimeIntegrationTests : BaseTestClass
	{
		private Client kinveyClient;

		private const string collectionName = "ToDos";

        [TestInitialize]
        public override void Setup()
		{
            base.Setup();

			string appKey = "kid_Zy0JOYPKkZ", appSecret = "d83de70e64d540e49acd6cfce31415df"; // UnitTestFramework
            Client.Builder builder = ClientBuilder
                .setFilePath(TestSetup.db_dir);

            if (MockData) builder.setBaseURL("http://localhost:8080");

			kinveyClient = builder.Build();
		}

        [TestCleanup]
        public override void Tear()
		{
			kinveyClient.ActiveUser?.Logout();
			System.IO.File.Delete(TestSetup.SQLiteOfflineStoreFilePath);
			System.IO.File.Delete(TestSetup.SQLiteCredentialStoreFilePath);

            base.Tear();
		}

		[TestMethod]
		public async Task TestRealtimeRegistration()
		{
            // Setup
            if (MockData) MockResponses(2);
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange

			// Act
			await Client.SharedClient.ActiveUser.RegisterRealtimeAsync();

			// Assert
			Assert.IsTrue(true);

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}

		[TestMethod]
		public async Task TestRealtimeUnregistration()
		{
            // Setup
            if (MockData) MockResponses(2);
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange

			// Act
			await Client.SharedClient.ActiveUser.UnregisterRealtimeAsync();

			// Assert
			Assert.IsTrue(true);

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}

		//[TestMethod]
  //      [Ignore("Fix inconsistent test")]
		//public async Task TestRealtimeCollectionSubscription()
		//{
		//	// Setup
		//	await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

		//	// Arrange
		//	var autoEvent = new System.Threading.AutoResetEvent(false);
		//	await Client.SharedClient.ActiveUser.RegisterRealtimeAsync();
		//	DataStore<ToDo> store = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK, Client.SharedClient);

		//	// Act
		//	ToDo ent = null;
		//	var realtimeDelegate = new KinveyDataStoreDelegate<ToDo>
		//	{
		//		OnError = (err) => { },
		//		OnNext = (entity) => {
		//			ent = entity;
		//			autoEvent.Set();
		//		},
		//		OnStatus = (status) => { }
		//	};

		//	bool result = await store.Subscribe(realtimeDelegate);

		//	// save to collection to trigger realtime update
		//	var todo = new ToDo();
		//	todo.Name = "Test Todo";
		//	todo.Details = "Test Todo Details";
		//	todo = await store.SaveAsync(todo);

  //          bool signal = autoEvent.WaitOne(20000);

  //          // Teardown
  //          await store.RemoveAsync(todo.ID);
  //          await store.Unsubscribe();
  //          await Client.SharedClient.ActiveUser.UnregisterRealtimeAsync();
  //          kinveyClient.ActiveUser.Logout();

		//	// Assert
  //          Assert.IsTrue(result);
		//	Assert.IsTrue(signal);
		//	Assert.IsNotNull(ent);
		//	Assert.AreEqual(0, ent.Name.CompareTo("Test Todo"));
		//	Assert.AreEqual(0, ent.Details.CompareTo("Test Todo Details"));
		//}

        //[TestMethod]
        //[Ignore("Fix inconsistent test")]
        //public async Task TestRealtimeCollectionSubscriptionUserACL()
        //{
        //    // Setup
        //    await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

        //    // Arrange
        //    var autoEvent = new System.Threading.AutoResetEvent(false);
        //    await Client.SharedClient.ActiveUser.RegisterRealtimeAsync();
        //    DataStore<ToDo> store = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK, Client.SharedClient);

        //    // Act
        //    ToDo ent = null;
        //    var realtimeDelegate = new KinveyDataStoreDelegate<ToDo>
        //    {
        //        OnError = (err) => { },
        //        OnNext = (entity) => {
        //            ent = entity;
        //            autoEvent.Set();
        //        },
        //        OnStatus = (status) => { }
        //    };

        //    bool result = await store.Subscribe(realtimeDelegate);

        //    // save to collection to trigger realtime update
        //    var todo = new ToDo();
        //    todo.Name = "Test Todo";
        //    todo.Details = "Test Todo Details";
        //    var acl = new AccessControlList();
        //    acl.GloballyReadable = false;
        //    acl.Readers.Add(Client.SharedClient.ActiveUser.Id);
        //    todo.ACL = acl;
        //    todo = await store.SaveAsync(todo);

        //    bool signal = autoEvent.WaitOne(10000);

        //    // Teardown
        //    await store.RemoveAsync(todo.ID);
        //    await store.Unsubscribe();
        //    await Client.SharedClient.ActiveUser.UnregisterRealtimeAsync();
        //    kinveyClient.ActiveUser.Logout();

        //    // Assert
        //    Assert.IsTrue(result);
        //    Assert.IsTrue(signal);
        //    Assert.IsNotNull(ent);
        //    Assert.AreEqual(0, ent.Name.CompareTo("Test Todo"));
        //    Assert.AreEqual(0, ent.Details.CompareTo("Test Todo Details"));
        //}

        //[TestMethod]
        //[Ignore("Fix inconsistent test")]
        //public async Task TestRealtimeCollectionSubscriptionUserACLFilteredOut()
        //{
        //    // Setup
        //    await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

        //    // Arrange
        //    var autoEvent = new System.Threading.AutoResetEvent(false);
        //    await Client.SharedClient.ActiveUser.RegisterRealtimeAsync();
        //    DataStore<ToDo> store = DataStore<ToDo>.Collection(collectionName, DataStoreType.NETWORK, Client.SharedClient);

        //    // Act
        //    ToDo ent = null;
        //    var realtimeDelegate = new KinveyDataStoreDelegate<ToDo>
        //    {
        //        OnError = (err) => { },
        //        OnNext = (entity) => {
        //            ent = entity;
        //            autoEvent.Set();
        //        },
        //        OnStatus = (status) => { }
        //    };

        //    bool result = await store.Subscribe(realtimeDelegate);

        //    // save to collection to trigger realtime update
        //    var todo = new ToDo();
        //    todo.Name = "Test Todo";
        //    todo.Details = "Test Todo Details";
        //    var acl = new AccessControlList();
        //    acl.GloballyReadable = false;
        //    todo.ACL = acl;
        //    todo = await store.SaveAsync(todo);

        //    bool signal = autoEvent.WaitOne(10000);

        //    // Teardown
        //    await store.RemoveAsync(todo.ID);
        //    await store.Unsubscribe();
        //    await Client.SharedClient.ActiveUser.UnregisterRealtimeAsync();
        //    kinveyClient.ActiveUser.Logout();

        //    // Assert
        //    Assert.IsTrue(result);
        //    Assert.IsFalse(signal);
        //    Assert.IsNull(ent);
        //}
	}
}
