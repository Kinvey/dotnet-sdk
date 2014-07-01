using System;
using NUnit.Framework;
using KinveyXamarin;
using Kinvey.DotNet.Framework.Core;

namespace AndroidLibtester
{
	[TestFixture]
	public class TestClient
	{
		
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
			string appkey = "123";
			string appsecret = "123";
			Client testClient = new Client.Builder (appkey, appsecret).build ();

			Assert.AreEqual (appkey, ((KinveyClientRequestInitializer)testClient.RequestInitializer).AppKey);
			Assert.AreEqual (appsecret, ((KinveyClientRequestInitializer)testClient.RequestInitializer).AppSecret);
		}


		[Test ()]
		public void TestClientBuilder ()
		{

			string appkey = "123";
			string appsecret = "123";
			string baseurl = "http://www.kinvey.com";
			string servpath = "/someService";

			Client testClient = new Client.Builder (appkey, appsecret)
				.setBaseURL (baseurl)
				.setServicePath (servpath)
				.build ();

			Console.WriteLine ("b " + testClient.BaseUrl);
			Console.WriteLine ("s " + testClient.ServicePath);

			Assert.AreEqual (baseurl + "/someService/", testClient.BaseUrl);
			Assert.AreEqual ("someService/", testClient.ServicePath);




		}


	}
}

