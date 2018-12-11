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
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kinvey;

namespace Kinvey.Tests
{
    [TestClass]
    public class UserIntegrationTests : BaseTestClass
    {
        private Client kinveyClient;

        private const string newuser = "newuser1";
        private const string newpass = "newpass1";

        private const string collectionName = "ToDos";

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();

            Client.Builder builder = ClientBuilder
                .setFilePath(TestSetup.db_dir);

            if (MockData) builder.setBaseURL("http://localhost:8080");
            if (MockData) builder.setMICHostName("http://localhost:8081");

            kinveyClient = builder.Build();
        }

        [TestCleanup]
        public override void Tear()
        {
            if (kinveyClient.ActiveUser != null)
            {
                kinveyClient.ActiveUser.Logout();
            }
            System.IO.File.Delete(TestSetup.SQLiteOfflineStoreFilePath);
            System.IO.File.Delete(TestSetup.SQLiteCredentialStoreFilePath);

            base.Tear();
        }

        #region Login/Logout Tests

        [TestMethod]
        public async Task TestLoginAsync()
        {
            // Arrange
            if (MockData) MockResponses(1);

            // Act
            User u = await User.LoginAsync(kinveyClient);

            // Assert
            Assert.IsNotNull(kinveyClient.ActiveUser);
            Assert.IsTrue(u.IsActive());

            // Teardown
            kinveyClient.ActiveUser.Logout();
        }

        [TestMethod]
        public async Task TestSharedClientLoginAsync()
        {
            // Arrange
            if (MockData) MockResponses(1);

            // Act
            User u = await User.LoginAsync(Client.SharedClient);

            // Assert
            Assert.IsNotNull(Client.SharedClient.ActiveUser);
            Assert.IsTrue(u.IsActive());

            // Teardown
            Client.SharedClient.ActiveUser.Logout();
        }

        [TestMethod]
        public async Task TestLoginAsyncBad()
        {
            // Arrange
            Client.Builder builder = ClientBuilderFake;
            if (MockData) builder.setBaseURL("http://localhost:8080");
            Client fakeClient = builder.Build();

            if (MockData) MockResponses(3);

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<KinveyException>(async delegate ()
            {
                await User.LoginAsync(fakeClient);
            });

            await Assert.ThrowsExceptionAsync<KinveyException>(async delegate ()
            {
                await User.LoginAsync(TestSetup.user, TestSetup.pass, fakeClient);
            });
        }

        [TestMethod]
        public async Task TestLoginUserPassAsync()
        {
            // Arrange
            if (MockData) MockResponses(1);

            // Act
            User u = await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Assert
            Assert.IsNotNull(kinveyClient.ActiveUser);
            Assert.IsTrue(u.IsActive());

            // Teardown
            kinveyClient.ActiveUser.Logout();
        }

        //[TestMethod]
        //[Ignore("Placeholder - Need Access Token To Run Test")]
        //public async Task TestLoginFacebookAsync()
        //{
        //    // Arrange
        //    string facebookAccessToken = "";

        //    // Act
        //    User fbUser = await User.LoginFacebookAsync(facebookAccessToken, kinveyClient);

        //    // Assert
        //    Assert.IsNotNull(fbUser);
        //    Assert.IsNotNull(fbUser.Attributes["_socialIdentity"]);
        //    JToken socID = fbUser.Attributes["_socialIdentity"];
        //    Assert.IsNotNull(socID["facebook"]);
        //    Assert.IsTrue(socID["facebook"].HasValues);
        //}

        [TestMethod]
        public async Task TestLoginFacebookAsyncBad()
        {
            // Arrange
            string facebookAccessTokenBad = "blahblahblah";

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<KinveyException>(async delegate ()
            {
                await User.LoginFacebookAsync(facebookAccessTokenBad, kinveyClient);
            });
        }

        //[TestMethod]
        //[Ignore("Placeholder - Need Access Token To Run Test")]
        //public async Task TestLoginGoogleAsync()
        //{
        //    // Arrange
        //    string googleAccessToken = "";

        //    // Act
        //    User googleUser = await User.LoginGoogleAsync(googleAccessToken, kinveyClient);

        //    // Assert
        //    Assert.IsNotNull(googleUser);
        //    Assert.IsNotNull(googleUser.Attributes["_socialIdentity"]);
        //    JToken socID = googleUser.Attributes["_socialIdentity"];
        //    Assert.IsNotNull(socID["google"]);
        //    Assert.IsTrue(socID["google"].HasValues);
        //}

        [TestMethod]
        public async Task TestLoginGoogleAsyncBad()
        {
            // Arrange
            string googleAccessTokenBad = "blahblahblah";

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<KinveyException>(async delegate ()
            {
                await User.LoginGoogleAsync(googleAccessTokenBad, kinveyClient);
            });
        }

        //[TestMethod]
        //[Ignore("Placeholder - Need Access Token To Run Test")]
        //public async Task TestLoginTwitterAsync()
        //{
        //    // Arrange
        //    string accessTokenKey = "";
        //    string accessTokenSecret = "";
        //    string consumerKey = "";
        //    string consumerKeySecret = "";

        //    // Act
        //    User twitterUser = await User.LoginTwitterAsync(accessTokenKey, accessTokenSecret, consumerKey, consumerKeySecret, kinveyClient);

        //    // Assert
        //    Assert.IsNotNull(twitterUser);
        //    Assert.IsNotNull(twitterUser.Attributes["_socialIdentity"]);
        //    JToken socID = twitterUser.Attributes["_socialIdentity"];
        //    Assert.IsNotNull(socID["twitter"]);
        //    Assert.IsTrue(socID["twitter"].HasValues);
        //}

        [TestMethod]
        public async Task TestLoginTwitterAsyncBad()
        {
            // Arrange
            string accessTokenKey = "twitterAccessTokenBad";
            string accessTokenSecret = "twitterAccessTokenSecretBad";
            string consumerKey = "twitterConsumerKeyBad";
            string consumerKeySecret = "twitterConsumerKeySecretBad";

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<KinveyException>(async delegate ()
            {
                await User.LoginTwitterAsync(accessTokenKey, accessTokenSecret, consumerKey, consumerKeySecret, kinveyClient);
            });
        }

        //[TestMethod]
        //[Ignore("Placeholder - Need Access Token To Run Test")]
        //public async Task TestLoginLinkedInAsync()
        //{
        //    // Arrange
        //    string accessTokenKey = "";
        //    string accessTokenSecret = "";
        //    string consumerKey = "";
        //    string consumerKeySecret = "";

        //    // Act
        //    User linkedinUser = await User.LoginLinkedinAsync(accessTokenKey, accessTokenSecret, consumerKey, consumerKeySecret, kinveyClient);

        //    // Assert
        //    Assert.IsNotNull(linkedinUser);
        //    Assert.IsNotNull(linkedinUser.Attributes["_socialIdentity"]);
        //    JToken socID = linkedinUser.Attributes["_socialIdentity"];
        //    Assert.IsNotNull(socID["linkedin"]);
        //    Assert.IsTrue(socID["linkedin"].HasValues);
        //}

        [TestMethod]
        public async Task TestLoginLinkedInAsyncBad()
        {
            // Arrange
            string accessTokenKey = "twitterAccessTokenBad";
            string accessTokenSecret = "twitterAccessTokenSecretBad";
            string consumerKey = "twitterConsumerKeyBad";
            string consumerKeySecret = "twitterConsumerKeySecretBad";

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<KinveyException>(async delegate ()
            {
                await User.LoginLinkedinAsync(accessTokenKey, accessTokenSecret, consumerKey, consumerKeySecret, kinveyClient);
            });
        }

        //[TestMethod]
        //[Ignore("Placeholder - Need Access Token To Run Test")]
        //public async Task TestLoginSalesforceAsync()
        //{
        //    // Arrange
        //    string access = "";
        //    string reauth = "";
        //    string clientID = "";
        //    string ID = "";

        //    // Act
        //    User salesforceUser = await User.LoginSalesforceAsync(access, reauth, clientID, ID, kinveyClient);

        //    // Assert
        //    Assert.IsNotNull(salesforceUser);
        //    Assert.IsNotNull(salesforceUser.Attributes["_socialIdentity"]);
        //    JToken socID = salesforceUser.Attributes["_socialIdentity"];
        //    Assert.IsNotNull(socID["salesforce"]);
        //    Assert.IsTrue(socID["salesforce"].HasValues);
        //}

        [TestMethod]
        public async Task TestLoginSalesforceAsyncBad()
        {
            // Arrange
            string access = "";
            string reauth = "";
            string clientID = "";
            string ID = "";

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<KinveyException>(async delegate ()
            {
                await User.LoginSalesforceAsync(access, reauth, clientID, ID, kinveyClient);
            });
        }

        // MIC LOGIN TESTS
        //
        [TestMethod]
        public void TestMIC_LoginWithMIC_NormalFlow()
        {
            // Arrange
            string redirectURI = "http://test.redirect";
            User loggedInUser = null;

            // Act
            string renderURL = null;
            User.LoginWithMIC(redirectURI, new KinveyMICDelegate<User>
            {
                onSuccess = (user) => { loggedInUser = user; },
                onError = (e) => { Console.WriteLine("TEST MIC ERROR"); },
                onReadyToRender = (url) => { renderURL = url; }
            });

            // Assert
            Assert.IsNotNull(renderURL);
            Assert.IsFalse(string.IsNullOrEmpty(renderURL));
            Assert.IsTrue(renderURL.StartsWith(kinveyClient.MICHostName + Constants.STR_MIC_DEFAULT_VERSION + "/oauth/auth?client_id=" + (kinveyClient.RequestInitializer as KinveyClientRequestInitializer).AppKey, StringComparison.Ordinal));
        }

        [TestMethod]
        public void TestMIC_LoginWithMIC_NormalFlow_ClientID()
        {
            // Arrange
            string redirectURI = "http://test.redirect";
            User loggedInUser = null;

            // Act
            string renderURL = null;
            string micID = "12345";

            User.LoginWithMIC(redirectURI, new KinveyMICDelegate<User>
            {
                onSuccess = (user) => { loggedInUser = user; },
                onError = (e) => { Console.WriteLine("TEST MIC ERROR"); },
                onReadyToRender = (url) => { renderURL = url; }
            }, micID);

            System.Diagnostics.Debug.WriteLine("\tClientID: " + micID);

            // Assert
            Assert.IsNotNull(renderURL);
            Assert.IsFalse(string.IsNullOrEmpty(renderURL));
            Assert.IsTrue(renderURL.StartsWith(kinveyClient.MICHostName + Constants.STR_MIC_DEFAULT_VERSION + "/oauth/auth?client_id=" + (kinveyClient.RequestInitializer as KinveyClientRequestInitializer).AppKey + "." + micID, StringComparison.Ordinal));
        }

        //[TestMethod]
        //[Ignore("Placeholder - Need configured backend to run test")]
        //public async Task TestMIC_LoginWithMIC_HeadlessFlow()
        //{
        //    // Arrange
        //    string username = "testuser";
        //    string password = "testpass";
        //    string redirectURI = "kinveyAuthDemo://";
        //    string saml_app_key = "kid_ZkPDb_34T";
        //    string saml_app_secret = "c3752d5079f34353ab89d07229efaf63";
        //    Client.Builder localBuilder = new Client.Builder(saml_app_key, saml_app_secret);
        //    Client localClient = localBuilder.Build();
        //    localClient.MICApiVersion = "v2";

        //    // Act
        //    await User.LoginWithMIC(username, password, redirectURI);

        //    // Assert
        //    Assert.IsNotNull(localClient.ActiveUser);

        //    // Teardown
        //    localClient.ActiveUser.Logout();
        //}

        //[TestMethod]
        //[Ignore("Placeholder - Need configured backend to run test")]
        //public async Task TestMIC_LoginWithMIC_HeadlessFlow_ClientID()
        //{
        //    // Arrange
        //    string username = "testuser";
        //    string password = "testpass";
        //    string redirectURI = "kinveyAuthDemo://";
        //    string saml_app_key = "kid_ZkPDb_34T";
        //    string saml_app_secret = "c3752d5079f34353ab89d07229efaf63";
        //    Client.Builder localBuilder = new Client.Builder(saml_app_key, saml_app_secret);
        //    Client localClient = localBuilder.Build();
        //    localClient.MICApiVersion = "v2";

        //    // Act
        //    string micID = "12345";
        //    await User.LoginWithMIC(username, password, redirectURI, micID);

        //    // Assert
        //    Assert.IsNotNull(localClient.ActiveUser);

        //    // Teardown
        //    localClient.ActiveUser.Logout();
        //}

        [TestMethod]
        public async Task TestLogout()
        {
            // Arrange
            if (MockData) MockResponses(2);
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);
            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC, kinveyClient);
            ToDo td = new ToDo
            {
                Name = "test"
            };
            await todoStore.SaveAsync(td);

            DataStore<FlashCard> flashCardStore = DataStore<FlashCard>.Collection("FlashCard", DataStoreType.SYNC, kinveyClient);
            FlashCard fc = new FlashCard();
            fc.Answer = "huh";
            await flashCardStore.SaveAsync(fc);

            // Act
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNull(kinveyClient.ActiveUser);
            Assert.AreEqual(0, kinveyClient.CacheManager.GetSyncQueue(collectionName).GetFirstN(1, 0).Count);

            // Check that all state is cleared out properly in logout by verifying that re-login works correctly
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);
            DataStore<ToDo> todoStoreRelogin = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC, kinveyClient);
            await todoStoreRelogin.FindAsync();

            // Teardown
            kinveyClient.ActiveUser.Logout();
        }

        [TestMethod]
        public async Task TestLogoutWithNoDatabaseTables()
        {
            // Arrange
            if (MockData) MockResponses(1);
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Act
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNull(kinveyClient.ActiveUser);
            Assert.AreEqual(0, kinveyClient.CacheManager.GetSyncQueue(collectionName).GetFirstN(1, 0).Count);
        }

        [TestMethod]
        public async Task TestLogoutWithDatabaseTablesButNoAPICalls()
        {
            // Arrange
            if (MockData) MockResponses(1);
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);
            DataStore<ToDo> todoStore = DataStore<ToDo>.Collection(collectionName, DataStoreType.SYNC, kinveyClient);

            // Act
            kinveyClient.ActiveUser.Logout();

            // Assert
            Assert.IsNull(kinveyClient.ActiveUser);
            Assert.AreEqual(0, kinveyClient.CacheManager.GetSyncQueue(collectionName).GetFirstN(1, 0).Count);
        }

        #endregion

        #region CRUD Tests

        //[TestMethod]
        //[Ignore("Placeholder - No unit test yet")]
        //public async Task TestCreateUserAsync()
        //{
        //    // Arrange
        //    string email = "newuser@test.com";
        //    Dictionary<string, JToken> customFields = new Dictionary<string, JToken>();
        //    customFields.Add("email", email);

        //    // Act
        //    User newUser = await User.SignupAsync("newuser1", "newpass1", customFields, kinveyClient);

        //    // Teardown
        //    //await kinveyClient.ActiveUser.DeleteAsync(newUser.Id, true);
        //    kinveyClient.ActiveUser.Logout();

        //    // Assert
        //    Assert.IsNotNull(newUser);
        //    Assert.IsNotNull(newUser.Attributes);
        //    Assert.IsTrue(String.Compare((newUser.Attributes["email"]).ToString(), email) == 0);
        //}

        //[TestMethod]
        //public async Task TestCreateUserAsyncBad()
        //{
        //	// Setup
        //	await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

        //	// Arrange
        //	string email = "newuser@test.com";
        //	Dictionary<string, JToken> customFields = new Dictionary<string, JToken>();
        //	customFields.Add("email", email);

        //	// Act
        //	Exception er = await Assert.ThrowsExceptionAsync<Exception>(async delegate () {
        //		await User.SignupAsync("newuser1", "newpass1", customFields, kinveyClient);
        //	});

        //	// Assert
        //	Assert.IsNotNull(er);
        //	KinveyException ke = er as KinveyException;
        //	Assert.AreEqual(EnumErrorCode.ERROR_USER_ALREADY_LOGGED_IN, ke.ErrorCode);

        //	// Teardown
        //	kinveyClient.ActiveUser.Logout();
        //}

        [TestMethod]
        public async Task TestFindUserAsync()
        {
            // Setup
            if (MockData) MockResponses(2);
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange

            // Act
            User me = await kinveyClient.ActiveUser.RefreshAsync();

            // Assert
            Assert.IsNotNull(me);
            Assert.IsTrue(string.Equals(kinveyClient.ActiveUser.Id, me.Id));

            // Teardown
            kinveyClient.ActiveUser.Logout();
        }

        [TestMethod]
        public async Task TestLookupUsersAsync()
        {
            // Setup
            if (MockData) MockResponses(2);
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            UserDiscovery criteria = new UserDiscovery();
            criteria.FirstName = "George";

            // Act
            User[] users = await kinveyClient.ActiveUser.LookupAsync(criteria);

            // Assert
            Assert.IsNotNull(users);
            Assert.AreEqual(3, users.Length);

            // Teardown
            kinveyClient.ActiveUser.Logout();
        }

        [TestMethod]
        public async Task TestDoesUsernameExist()
        {
            // Arrange
            if (MockData) MockResponses(1);
            string username = "testuser";

            // Act
            bool exists = await User.HasUser(username);

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public async Task TestDoesUsernameExistBad()
        {
            // Setup
            if (MockData) MockResponses(2);
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            string username = "homer_simpson";

            // Act
            bool exists = await User.HasUser(username);

            // Assert
            Assert.IsFalse(exists);

            // Teardown
            kinveyClient.ActiveUser.Logout();
        }

        [TestMethod]
        public async Task TestForgotUsername()
        {
            // Setup
            if (MockData) MockResponses(2);
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            string email = "vinay@kinvey.com";

            // Act
            // Assert
            await User.ForgotUsername(email);

            // Teardown
            kinveyClient.ActiveUser.Logout();
        }

        [TestMethod]
        public async Task TestResetPassword()
        {
            // Arrange
            if (MockData) MockResponses(1);
            string email = "vinay@kinvey.com";

            // Act
            // Assert
            await User.ResetPasswordAsync(email);
        }

        [TestMethod]
        public async Task TestUpdateUserAsync()
        {
            // Setup
            if (MockData) MockResponses(3);
            await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);
            const string TEST_KEY = "test_key";
            const string TEST_VALUE = "test_value";

            // Arrange
            kinveyClient.ActiveUser.Attributes.Remove(TEST_KEY);
            kinveyClient.ActiveUser.Attributes.Add(TEST_KEY, TEST_VALUE);
            Assert.IsTrue(kinveyClient.ActiveUser.Attributes.ContainsKey(TEST_KEY));

            // Act
            // Assert
            var u = await kinveyClient.ActiveUser.UpdateAsync();

            Assert.IsTrue(u != null);
            Assert.IsTrue(u.Attributes.ContainsKey(TEST_KEY));
            Assert.IsTrue(kinveyClient.ActiveUser.Attributes.ContainsKey(TEST_KEY));
            Assert.IsTrue(kinveyClient.ActiveUser.Attributes.Count == u.Attributes.Count);

            // Teardown
            kinveyClient.ActiveUser.Attributes.Remove(TEST_KEY);
            await kinveyClient.ActiveUser.UpdateAsync();
            kinveyClient.ActiveUser.Logout();
        }

        #endregion

        [TestMethod]
        public async Task TestUserDisabledAsync()
        {
            // Setup
            if (MockData) MockResponses(2);
            User myUser = await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            User deletedSoftUser = await myUser.RetrieveAsync("5808de04e87d27107142f686");

            // Act
            // Assert
            Assert.IsTrue(deletedSoftUser.Disabled);

            // Teardown
            kinveyClient.ActiveUser.Logout();
        }

        [TestMethod]
        public async Task TestUserDisabledFalseAsync()
        {
            // Setup
            if (MockData) MockResponses(1);
            User myUser = await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange

            // Act

            // Assert
            Assert.IsFalse(myUser.Disabled);

            // Teardown
            kinveyClient.ActiveUser.Logout();
        }

        [TestMethod]
        public async Task TestUserKMDEmailVerification()
        {
            // Setup
            if (MockData) MockResponses(1);
            User u = await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            u.Metadata = new KinveyUserMetaData();
            u.Metadata.EmailVerification.Status = "sent";

            // Act
            string status = u.Metadata.EmailVerification.Status;

            // Assert
            Assert.IsTrue(String.Equals(status, "sent"));

            // Teardown
            kinveyClient.ActiveUser.Logout();
        }

        [TestMethod]
        public async Task TestUserKMDPasswordReset()
        {
            // Setup
            if (MockData) MockResponses(1);
            User u = await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

            // Arrange
            u.Metadata = new KinveyUserMetaData();
            u.Metadata.PasswordReset.Status = "InProgress";

            // Act
            string status = u.Metadata.PasswordReset.Status;

            // Assert
            Assert.IsTrue(String.Equals(status, "InProgress"));

            // Teardown
            kinveyClient.ActiveUser.Logout();
        }

        [TestMethod]
        public async Task TestUserInitFromCredential()
        {
            // Setup
            Client.Builder builder1 = ClientBuilder
                .setFilePath(TestSetup.db_dir);

            if (MockData) builder1.setBaseURL("http://localhost:8080");

            Client kinveyClient1 = builder1.Build();

            if (MockData) MockResponses(1);

            // Arrange
            User activeUser = await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient1);

            // Act
            Client.Builder builder2 = ClientBuilder
                .setFilePath(TestSetup.db_dir);

            if (MockData) builder2.setBaseURL("http://localhost:8080");

            Client kinveyClient2 = builder2.Build();

            // Assert
            Assert.IsTrue(activeUser?.AccessToken == kinveyClient2?.ActiveUser?.AccessToken);
            Assert.AreEqual(2, kinveyClient2?.ActiveUser?.Attributes.Count);
            Assert.AreEqual(activeUser?.Attributes.Count, kinveyClient2?.ActiveUser?.Attributes.Count);
            Assert.AreEqual(activeUser?.Attributes["email"], kinveyClient2?.ActiveUser?.Attributes["email"]);
            Assert.AreEqual(activeUser?.Attributes["_acl"]["creator"], kinveyClient2?.ActiveUser?.Attributes["_acl"]["creator"]);
            Assert.IsTrue(activeUser?.AuthToken == kinveyClient2?.ActiveUser?.AuthToken);
            Assert.IsTrue(activeUser?.Id == kinveyClient2?.ActiveUser?.Id);
            Assert.AreEqual(0, kinveyClient2?.ActiveUser?.Metadata.Count);
            Assert.AreEqual(activeUser?.Metadata.Count, kinveyClient2?.ActiveUser?.Metadata.Count);
            Assert.IsTrue(activeUser?.UserName == kinveyClient2?.ActiveUser?.UserName);

            // Teardown
            kinveyClient1.ActiveUser.Logout();
        }
    }
}
