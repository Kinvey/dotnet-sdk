using System;
using NUnit.Framework;
using KinveyXamarin;
using System.Reflection;
using RestSharp;

namespace AndroidLibtester
{
	[TestFixture]
	public class TestAppData
	{

		string appkey = "123";
		string appsecret = "123";
		Client testClient;
		DataStore<MyEntity> testData;
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
			DataStore<MyEntity>.GetEntityRequest req = testData.GetEntityBlocking (someID);

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
			DataStore<MyEntity>.GetRequest req = testData.GetBlocking ();

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

			DataStore<MyEntity>.SaveRequest req = testData.SaveBlocking (new MyEntity());

			RestRequest restReq = req.BuildRestRequest ();
			Assert.True (restReq.Method == Method.POST);
			Assert.True (restReq.Parameters.FindAll (delegate(Parameter p){
				return p.Type == ParameterType.UrlSegment;
			}).Count >= 1);

			Assert.True (restReq.Parameters.FindAll (delegate(Parameter p){
				return p.Type == ParameterType.HttpHeader;
			}).Count >= 3);

		}

		[Test()]
		public void TestDeleteEntity()
		{
			string someID = "some id";
			DataStore<MyEntity>.DeleteRequest req = testData.DeleteBlocking (someID);

			RestRequest restReq = req.BuildRestRequest ();
			Assert.True (restReq.Method == Method.DELETE);
			Assert.True (restReq.Parameters.FindAll (delegate(Parameter p){
				return p.Type == ParameterType.UrlSegment;
			}).Count >= 1);

			Assert.True (restReq.Parameters.FindAll (delegate(Parameter p){
				return p.Type == ParameterType.HttpHeader;
			}).Count >= 3);

		}


	}
}

