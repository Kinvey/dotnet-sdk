// Copyright (c) 2014, Kinvey, Inc. All rights reserved.
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

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Kinvey.DotNet.Framework;
using RestSharp;
using Kinvey.DotNet.Framework.Core;
using System.Threading;
using KinveyXamarin;
using System.Threading.Tasks;
using System.IO;
using SQLite.Net.Platform.XamarinAndroid;
using System.Linq;
using LinqExtender;
using System.Text;

namespace AndroidTestDrive
{
	[Activity (Label = "Android-TestDrive", MainLauncher = true)]
	public class MainActivity : Activity
	{
		int count = 1;

		private string appKey = "kid_eV220fVYa9";
		private string appSecret = "98b40ad7a65d4655859f2e7b1432e0a1";

		private static string COLLECTION = "myCollection";
		private static string STABLE_ID = "testdriver";

		Client kinveyClient;
		InMemoryCache<MyEntity> myCache = new InMemoryCache<MyEntity>();

		protected override void OnCreate (Bundle bundle)
		{

			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			kinveyClient = new Client.Builder(appKey, appSecret)
//				.setFilePath(Android.OS.Environment.ExternalStorageDirectory.ToString ())
				.setFilePath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal))
				.setOfflinePlatform(new SQLitePlatformAndroid()).build();

			kinveyClient.User ().Login (new KinveyDelegate<User>{ 
				onSuccess =  (user) => { 
					RunOnUiThread (() => {
						Toast.MakeText(this, "logged in as: " + user.Id, ToastLength.Short).Show();
					});
				},
				onError = (error) => {
					RunOnUiThread (() => {
						Toast.MakeText(this, "something went wrong: " + error.Message, ToastLength.Short).Show();
					});
				}
			});


			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.myButton);
			button.Click += delegate {
				button.Text = string.Format ("{0} clicks!", count++);
			};

			Button save = FindViewById<Button> (Resource.Id.saveButton);
			save.Click += delegate {
				new Thread (() =>
					saveAndToast ()
				).Start ();
			};

//			save.Click += async (sender, e) => {};


			Button load = FindViewById<Button> (Resource.Id.loadButton);
			load.Click += delegate {
				new Thread(() =>
					loadAndToast()	
				).Start();
			};

			Button loadCache = FindViewById<Button> (Resource.Id.loadWithCacheButton);
			loadCache.Click += delegate {
				new Thread(() =>
					loadFromCacheAndToast()	
				).Start();
			};

			Button loadQuery = FindViewById<Button> (Resource.Id.loadWithQuery);
			loadQuery.Click += delegate {
				new Thread(() => 
					loadFromQuery()
				).Start();
			};





		}

//		private void loginUserAndToast(){
//			User user;
//			if (kinveyClient.User ().isUserLoggedIn ()) {
//				user = kinveyClient.User ();
//			} else {
//				try{
//					user = kinveyClient.User ().LoginBlocking ().Execute();
//				}catch(Exception e){
//					Console.WriteLine ("Uh oh! " + e);
//					RunOnUiThread (() => {
//						Toast.MakeText(this, "something went wrong: " + e.Message, ToastLength.Short).Show();
//					});
//					return;
//				}
//			}
//
//			RunOnUiThread ( () => {
//				Toast.MakeText(this, "logged in as: " + user.Id, ToastLength.Short).Show();
//			});
//		}
//
		private void saveAndToast(){
		
			AsyncAppData<MyEntity> entityCollection = kinveyClient.AppData<MyEntity>(COLLECTION, typeof(MyEntity));
//			entityCollection.set

			MyEntity ent = new MyEntity();
//			ent.ID = STABLE_ID;
			ent.Email = "test@tester.com";
			ent.Name = "James Dean";
//			entityCollection.setCache (myCache, CachePolicy.CACHE_FIRST);
			entityCollection.setOffline(new SQLiteOfflineStore<MyEntity>(), OfflinePolicy.LOCAL_FIRST);
			entityCollection.Save (ent, new KinveyDelegate<MyEntity> { 
				onSuccess = (entity) => { 
					RunOnUiThread (() => {
						Toast.MakeText (this, "saved: " + entity.Name, ToastLength.Short).Show ();
					});
				},
				onError = (error) => {
					RunOnUiThread (() => {
						Toast.MakeText (this, "something went wrong: " + error.Message, ToastLength.Short).Show ();
					});
				}
			});
		
		
		}

		private void loadAndToast(){
			AppData<MyEntity> entityCollection = kinveyClient.AppData<MyEntity>(COLLECTION, typeof(MyEntity));
//			entityCollection.setCache (myCache, CachePolicy.NO_CACHE);
			entityCollection.setOffline(new SQLiteOfflineStore<MyEntity>(), OfflinePolicy.LOCAL_FIRST);
			MyEntity res = null;
			try{
				res = entityCollection.GetEntityBlocking (STABLE_ID).Execute ();
			}catch(Exception e){
				Console.WriteLine ("Uh oh! " + e);
				RunOnUiThread (() => {
					Toast.MakeText(this, "something went wrong: " + e.Message, ToastLength.Short).Show();
				});
				return;
			}


			RunOnUiThread ( () => {
				Toast.MakeText(this, "got: " + res.Name, ToastLength.Short).Show();
			});
		
		}

		private void loadFromCacheAndToast(){
			AsyncAppData<MyEntity> entityCollection = kinveyClient.AppData<MyEntity>(COLLECTION, typeof(MyEntity));
//			entityCollection.setCache (myCache, CachePolicy.CACHE_FIRST);
//			entityCollection.setOffline(new SQLiteOfflineStore<MyEntity>(), OfflinePolicy.LOCAL_FIRST);


			entityCollection.GetEntity (STABLE_ID, new KinveyDelegate<MyEntity> { 
				onSuccess = (entity) => { 
					RunOnUiThread (() => {
						Toast.MakeText (this, "got: " + entity.Name, ToastLength.Short).Show ();
					});
				},
				onError = (error) => {
					RunOnUiThread (() => {
						Toast.MakeText (this, "something went wrong: " + error.Message, ToastLength.Short).Show ();
					});
				}
			});


//			try{
//				MyEntity res = entityCollection.GetEntityBlocking (STABLE_ID).Execute ();
//				RunOnUiThread ( () => {
//					Toast.MakeText(this, "got " + res.Name, ToastLength.Short).Show();
//				});
//			}catch(Exception e){
//				Console.WriteLine ("Uh oh! " + e);
//				RunOnUiThread (() => {
//					Toast.MakeText(this, "something went wrong: " + e.Message, ToastLength.Short).Show();
//				});
//				return;
//			}			


		}


		private void loadFromQuery(){

			AsyncAppData<MyEntity> query = kinveyClient.AppData<MyEntity>(COLLECTION, typeof(MyEntity));
//
			var query1 = from cust in query
			             where cust.Name == "James Dean"
			             select cust;

			Task.Run (() => {
				foreach (MyEntity e in query1){
					Console.WriteLine("got -> " + e.Name);
				}
				Console.WriteLine("total at: " + query1.Count());
			});

//			Console.WriteLine ("1-------");
//			Console.WriteLine(query1.Count());
//			Console.WriteLine (query.writer.GetFullString());
//			Console.WriteLine ("1-------");
//
//			query.writer.Reset ();
//
//			var query2 = from cust in query
//			             where cust.lowercasetest == "James Dean"
//			                 && cust.Name == "someName"
//			             select cust;
//						
//			Console.WriteLine ("2-------");
//			Console.WriteLine(query2.Count());
//			Console.WriteLine (query.writer.GetFullString());
//			Console.WriteLine ("2-------");
//
//			query.writer.Reset ();
//
//			var query3 = from cust in query
//					where (cust.lowercasetest == "James Dean" && cust.Name == "Charlie")
//				|| cust.Name == "Max"
//				select cust;
//
//
//			Console.WriteLine ("3-------");
//			Console.WriteLine(query3.Count());
//			Console.WriteLine (query.writer.GetFullString());
//			Console.WriteLine ("3-------");
//
//			query.writer.Reset ();
//
//			var query4 = from cust in query
//					where cust.Name == "Max" 
//				|| (cust.lowercasetest == "James Dean" && cust.Name == "Charlie")
//				select cust;
//
//
//			Console.WriteLine ("4-------");
//			Console.WriteLine(query4.Count());
//			Console.WriteLine (query.writer.GetFullString());
//			Console.WriteLine ("4-------");
//
//			query.writer.Reset ();
//
//			var query5 = from cust in query
//					where (cust.ID == "10" && cust.Name == "Billy") || (cust.ID == "1" && cust.Name == "Charlie")
//				select cust;
//
//			Console.WriteLine ("5-------");
//			Console.WriteLine(query5.Count());
//			Console.WriteLine (query.writer.GetFullString());
//			Console.WriteLine ("5-------");
//
//			query.writer.Reset ();
//
//			var query6 = from cust in query
//				orderby cust.ID
//				select cust;
//
//			Console.WriteLine ("6-------");
//			Console.WriteLine(query6.Count());
//			Console.WriteLine (query.writer.GetFullString());
//			Console.WriteLine ("6-------");
//
//			query.writer.Reset ();
//
//			var query7 = from cust in query
//				orderby cust.ID descending
//				select cust;
//
//			Console.WriteLine ("7-------");
//			Console.WriteLine(query7.Count());
//			Console.WriteLine (query.writer.GetFullString());
//			Console.WriteLine ("7-------");
//
//			query.writer.Reset ();
//
//			var query8 = query.Where(x => x.ID == "1").Where(x => x.Name == "Scott");
//
//			Console.WriteLine ("8-------");
//			Console.WriteLine(query8.Count());
//			Console.WriteLine (query.writer.GetFullString());
//			Console.WriteLine ("8-------");
//
//			query.writer.Reset ();
//
//			var query9 = query
//				.Where(x => x.ID == "111")
//				.Where(x => x.ID== "1" || x.Name == "Scott" || x.IsAvailable);
//
//			Console.WriteLine ("9-------");
//			Console.WriteLine(query9.Count());
//			Console.WriteLine (query.writer.GetFullString());
//			Console.WriteLine ("9-------");
//
//



		}


	}
}


