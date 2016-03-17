using System;
using NUnit.Framework;
using KinveyXamarin;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace UnitTestFramework
{
	[TestFixture]
	public class TestUser
	{
		private Client kinveyClient;
		private const string user = "testuser";
		private const string pass = "testpass";
		private const string app_id = "abcdefg";
		private const string app_secret = "0123456789abcdef";

		[SetUp]
		public void Setup ()
		{
			kinveyClient = new Client.Builder(app_id, app_secret).build();
			KinveyDelegate<User> delegates = new KinveyDelegate<User>();
			kinveyClient.User().Login(user, pass, delegates);
			System.Threading.Thread.Sleep(3000);  // TODO find better way of waiting for setup to complete
		}


		[TearDown]
		public void Tear ()
		{
			kinveyClient.User().Logout();
		}


		// CREATE TESTS
		//

		[Test]
		public void TestCreateBlocking ()
		{
			// Arrange
			User.LoginRequest loginRequest = kinveyClient.User().CreateBlocking(user, pass);

			// Act
			loginRequest.buildAuthRequest();

			// Assert
			Assert.False(loginRequest == null);
		}


		// DELETE TESTS
		//

		[Test]
		public void TestDeleteUserSoft()
		{
			// Arrange
			string userID = "12345";

			// Act
			User.DeleteRequest deleteRequest = kinveyClient.User().DeleteBlocking(userID, false);

			// Assert
			Assert.True(deleteRequest.RequestMethod == "DELETE");
			Assert.True(deleteRequest.hard == false);
			Assert.AreEqual(deleteRequest.userID, userID);
			Assert.AreSame(deleteRequest.uriTemplate, "user/{appKey}/{userID}?hard={hard}");
		}

		[Test]
		public void TestDeleteUserHard()
		{
			// Arrange
			string userID = "4567";

			// Act
			User.DeleteRequest deleteRequest = kinveyClient.User().DeleteBlocking(userID, true);

			// Assert
			Assert.True(deleteRequest.RequestMethod == "DELETE");
			Assert.True(deleteRequest.hard == true);
			Assert.AreEqual(deleteRequest.userID, userID);
			Assert.AreSame(deleteRequest.uriTemplate, "user/{appKey}/{userID}?hard={hard}");
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async Task TestDeleteUserSoftAsync()
		{
//			// Arrange
//			string userID = "4567";
//
//			// Act
//			User.DeleteRequest deleteRequest = await kinveyClient.User().DeleteAsync(userID, false);
//
//			// Assert
//			Assert.True(deleteRequest.RequestMethod == "DELETE");
		}

		[Test]
		[Ignore("Placeholder - No unit test yet")]
		public async void TestDeleteUserHardAsync()
		{
//			string userID;
//
//			User.DeleteRequest deleteRequest = await kinveyClient.User().DeleteAsync(userID, true);
//
//			// Assert
//			Assert.True(deleteRequest.RequestMethod == "DELETE");
		}
	}
}
