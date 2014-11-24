using System;
using NUnit.Framework;
using KinveyXamarin;
using System.Reflection;
using RestSharp;
using LinqExtender;

namespace AndroidLibtester
{
	[TestFixture]
	public class TestAppData
	{

		string appkey = "123";
		string appsecret = "123";
		Client testClient;
		AppData<MyEntity> testData;
		string myCollection = "myCollection";

		[SetUp]
		public void Setup ()
		{
			testClient = new Client.Builder (appkey, appsecret).build ();
			testData = testClient.AppData<MyEntity> (myCollection, typeof(MyEntity));
		}


		[TearDown]
		public void Tear ()
		{
		}


		[Test()]
		public void TestGetEntity()
		{

			string someID = "some id";
			AppData<MyEntity>.GetEntityRequest req = testData.GetEntityBlocking (someID);

			RestRequest restReq = req.BuildRestRequest ();
			Assert.True (restReq.Method == Method.GET);
			Assert.True (restReq.Parameters.FindAll (delegate(Parameter p){
				return p.Type == ParameterType.UrlSegment;
			}).Count >= 2);

			Assert.True (restReq.Parameters.FindAll (delegate(Parameter p){
				return p.Type == ParameterType.HttpHeader;
			}).Count >= 3);

		}

		[Test()]
		public void TestGetAll()
		{
			AppData<MyEntity>.GetRequest req = testData.GetBlocking ();

			RestRequest restReq = req.BuildRestRequest ();
			Assert.True (restReq.Method == Method.GET);
			Assert.True (restReq.Parameters.FindAll (delegate(Parameter p){
				return p.Type == ParameterType.UrlSegment;
			}).Count >= 1);

			Assert.True (restReq.Parameters.FindAll (delegate(Parameter p){
				return p.Type == ParameterType.HttpHeader;
			}).Count >= 3);

		}

		[Test()]
		public void TestSaveEntity()
		{

			AppData<MyEntity>.SaveRequest req = testData.SaveBlocking (new MyEntity());

			RestRequest restReq = req.BuildRestRequest ();
			Assert.True (restReq.Method == Method.POST);
			Assert.True (restReq.Parameters.FindAll (delegate(Parameter p){
				return p.Type == ParameterType.UrlSegment;
			}).Count >= 1);

			Assert.True (restReq.Parameters.FindAll (delegate(Parameter p){
				return p.Type == ParameterType.HttpHeader;
			}).Count >= 3);

		}

	}
}

