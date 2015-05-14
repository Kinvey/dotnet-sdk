using System;
using NUnit.Framework;
using KinveyXamarin;
using System.Reflection;
using RestSharp;
using System.Linq;

namespace AndroidLibtester
{
	[TestFixture]
	public class LinqBuilderTest
	{

		string appkey = "123";
		string appsecret = "123";
		Client testClient;
		AppData<MyEntity> testData;
		string myCollection = "myCollection";

		public LinqBuilderTest ()
		{
		}

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


//		[Test()]
//		public void TestEquals()
//		{
//
//			var query = from cust in testData
//					where cust.lowercasetest == "James Dean"
//				select cust;
//
//			//need to enumerate to execute
//			query.Count ();
//			Assert.Equals (testData.writer.GetFullString(), "?query={\"lowercasetest\":\"James Dean\"}");
//
//		}
//
//		[Test()]
//		public void TestLogicalAnd(){
//			var query = from cust in testData
//					where cust.lowercasetest == "James Dean"
//				&& cust.Name == "someName"
//				select cust;
//
//			//need to enumerate to execute
//			query.Count ();
//			Assert.Equals (testData.writer.GetFullString(), "?query={\"lowercasetest\":\"James Dean\",\"Name\":\"someName\"}");
//		}
//
//		[Test()]
//		public void TestLogicalOrWithLeftExpression(){
//			var query = from cust in testData
//					where (cust.lowercasetest == "James Dean" && cust.Name == "Charlie")
//				|| cust.Name == "Max"
//				select cust;
//
//			//need to enumerate to execute
//			query.Count ();
//			Assert.Equals (testData.writer.GetFullString(), "?query={$or:[{\"lowercasetest\":\"James Dean\",\"Name\":\"Charlie\"},{\"Name\":\"Max\"}]}");
//		}
//
//		[Test()]
//		public void TestLogicalOrWithRightExpression(){
//			var query = from cust in testData
//					where cust.Name == "Max" 
//				|| (cust.lowercasetest == "James Dean" && cust.Name == "Charlie")
//				select cust;
//
//			//need to enumerate to execute
//			query.Count ();
//			Assert.Equals (testData.writer.GetFullString(), "?query={$or:[{\"Name\":\"Max\"},{(\"lowercasetest\":\"James Dean\",\"Name\":\"Charlie\")}]}");
//		}
//
//		[Test()]
//		public void TestLogicalOrWithBothExpressions(){
//			var query = from cust in testData
//					where (cust.ID == "10" && cust.Name == "Billy") || (cust.ID == "1" && cust.Name == "Charlie")
//				select cust;
//
//			//need to enumerate to execute
//			query.Count ();
//			Assert.Equals (testData.writer.GetFullString(), "?query={$or:[{(\"ID\":\"10\",\"Name\":\"Billy\")},{(\"ID\":\"1\",\"Name\":\"Charlie\")}]}");
//		}
//
//		[Test()]
//		public void TestOrderbyAscending(){
//			var query = from cust in testData
//				orderby cust.ID
//				select cust;
//
//			//need to enumerate to execute
//			query.Count ();
//			Assert.Equals (testData.writer.GetFullString(), "?query={}&sort={\"ID\":1}");
//		}
//
//		[Test()]
//		public void TestOrderByDescending(){
//			var query = from cust in testData
//				orderby cust.ID descending
//				select cust;
//
//			//need to enumerate to execute
//			query.Count ();
//			Assert.Equals (testData.writer.GetFullString(), "?query={}&sort={\"ID\":-1}");
//		}
//
//		[Test()]
//		public void TestLambda(){
//			var query = testData.Where(x => x.ID == "1").Where(x => x.Name == "Scott");
//
//			//need to enumerate to execute
//			query.Count ();
//			Assert.Equals (testData.writer.GetFullString(), "?query={\"ID\":\"1\",\"Name\":\"Scott\"}");
//		}
//
//		[Test()]
//		public void TestChainedLambda(){
//			var query = testData
//				.Where(x => x.ID == "111")
//				.Where(x => x.ID== "1" || x.Name == "Scott" || x.IsAvailable);
//
//			//need to enumerate to execute
//			query.Count ();
//			Assert.Equals (testData.writer.GetFullString(), "?query={\"ID\":\"111\",$or:[{$or:[{\"ID\":\"1\"},{\"Name\":\"Scott\"}]},{\"IsAvailable\":True}]}");
//		}
	}
}

