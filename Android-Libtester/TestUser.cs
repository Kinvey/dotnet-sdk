using System;
using NUnit.Framework;
using KinveyXamarin;
using Kinvey.DotNet.Framework.Core;
using Kinvey.DotNet.Framework;
using System.Reflection;
using Kinvey.DotNet.Framework.Auth;

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

			Kinvey.DotNet.Framework.Auth.KinveyAuthRequest.LoginType lgType = (Kinvey.DotNet.Framework.Auth.KinveyAuthRequest.LoginType) ReflectionHelper.getFieldValue (req, "type");
			User memberUser = (User) ReflectionHelper.getFieldValue(req, "memberUser");
		
//			Assert.Equals (lgType, Kinvey.DotNet.Framework.Auth.KinveyAuthRequest.LoginType.IMPLICIT);
			Assert.True(lgType == Kinvey.DotNet.Framework.Auth.KinveyAuthRequest.LoginType.IMPLICIT);
			Assert.That (ReferenceEquals (memberUser, testUser));

		}

		[Test()]
		public void TestUserLogin(){

			string username = "username";
			string password = "password";

			User.LoginRequest req = testUser.LoginBlocking (username, password);

			Kinvey.DotNet.Framework.Auth.KinveyAuthRequest.LoginType lgType = (Kinvey.DotNet.Framework.Auth.KinveyAuthRequest.LoginType) ReflectionHelper.getFieldValue (req, "type");
			User memberUser = (User) ReflectionHelper.getFieldValue(req, "memberUser");
			KinveyAuthRequest.Builder builder = (KinveyAuthRequest.Builder) ReflectionHelper.GetInheritedFieldValue(memberUser, "builder");
			string user = (string)ReflectionHelper.getFieldValue (builder, "username");
			string pass = (string)ReflectionHelper.getFieldValue (builder, "password");

			Assert.True (lgType == Kinvey.DotNet.Framework.Auth.KinveyAuthRequest.LoginType.KINVEY);
			Assert.That (ReferenceEquals (memberUser, testUser));
			Assert.True (username.Equals(user));
			Assert.True (password.Equals(pass));
		}
			
	}
}

