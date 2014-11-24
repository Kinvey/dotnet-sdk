using System;
using NUnit.Framework;
using KinveyXamarin;

namespace AndroidLibtester
{
	[TestFixture]
	public class TestClient
	{

		string appkey = "123";
		string appsecret = "123";
		
		[SetUp]
		public void Setup ()
		{
		}

		
		[TearDown]
		public void Tear ()
		{
		}

		[Test ()]
		public void TestAppKeyAndSecret ()
		{

			Client testClient = new Client.Builder (appkey, appsecret).build ();

			Assert.AreEqual (appkey, ((KinveyClientRequestInitializer)testClient.RequestInitializer).AppKey);
			Assert.AreEqual (appsecret, ((KinveyClientRequestInitializer)testClient.RequestInitializer).AppSecret);
		}


		[Test ()]
		public void TestClientBuilder ()
		{
			string baseurl = "http://www.kinvey.com";
			string servpath = "/someService";

			Client testClient = new Client.Builder (appkey, appsecret)
				.setBaseURL (baseurl)
				.setServicePath (servpath)
				.build ();

			Assert.AreEqual (baseurl + "/someService/", testClient.BaseUrl);
			Assert.AreEqual ("someService/", testClient.ServicePath);
		}

		[Test ()]
		public void TestUserFactoryIsCached()
		{
			Client testClient = new Client.Builder (appkey, appsecret).build ();

			AsyncUser user = testClient.User ();
			AsyncUser secondUser = testClient.User ();

			Assert.That (ReferenceEquals (user, secondUser));

		}

	}
}

