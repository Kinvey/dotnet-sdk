using System;
using NUnit.Framework;
using KinveyXamarin;
using System.Reflection;

namespace AndroidLibtester
{
	[TestFixture]
	public class TestUser
	{

		string appkey = "123";
		string appsecret = "123";
		Client testClient;
		User testUser;


		[SetUp]
		public void Setup ()
		{
			testClient = new Client.Builder (appkey, appsecret).build ();
			testUser = testClient.User ();
		}


		[TearDown]
		public void Tear ()
		{
		}

	
		[Test()]
		public void TestImplicitLogin()
		{
			User.LoginRequest req = testUser.LoginBlocking ();

			KinveyAuthRequest.LoginType lgType = (KinveyAuthRequest.LoginType) ReflectionHelper.getFieldValue (req, "type");
			User memberUser = (User) ReflectionHelper.getFieldValue(req, "memberUser");
		
			Assert.True(lgType == KinveyAuthRequest.LoginType.IMPLICIT);
			Assert.That (ReferenceEquals (memberUser, testUser));

		}

		[Test()]
		public void TestUserLogin(){

			string username = "username";
			string password = "password";

			User.LoginRequest req = testUser.LoginBlocking (username, password);

			KinveyAuthRequest.LoginType lgType = (KinveyAuthRequest.LoginType) ReflectionHelper.getFieldValue (req, "type");
			User memberUser = (User) ReflectionHelper.getFieldValue(req, "memberUser");
			KinveyAuthRequest.Builder builder = (KinveyAuthRequest.Builder) ReflectionHelper.GetInheritedFieldValue(memberUser, "builder");
			string user = (string)ReflectionHelper.getFieldValue (builder, "username");
			string pass = (string)ReflectionHelper.getFieldValue (builder, "password");

			Assert.True (lgType == KinveyAuthRequest.LoginType.KINVEY);
			Assert.That (ReferenceEquals (memberUser, testUser));
			Assert.True (username.Equals(user));
			Assert.True (password.Equals(pass));
		}
			
	}
}

