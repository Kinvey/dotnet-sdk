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

//			AbstractClient kinveyClient = (AbstractClient)new AbstractClient.Builder (new RestClient (), new Kinvey.DotNet.Framework.Core.KinveyClientRequestInitializer (appKey, appSecret, new KinveyHeaders ())).build ();
			kinveyClient = new Client.Builder(appKey, appSecret).build();

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



//			new Thread(() => 
//				loginUserAndToast ()
//			).Start();

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





		}

		private void loginUserAndToast(){
			User user;
			if (kinveyClient.User ().isUserLoggedIn ()) {
				user = kinveyClient.User ();
			} else {
				try{
					user = kinveyClient.User ().LoginBlocking ().Execute();
				}catch(Exception e){
					Console.WriteLine ("Uh oh! " + e);
					RunOnUiThread (() => {
						Toast.MakeText(this, "something went wrong: " + e.Message, ToastLength.Short).Show();
					});
					return;
				}
			}

			RunOnUiThread ( () => {
				Toast.MakeText(this, "logged in as: " + user.Id, ToastLength.Short).Show();
			});
		}

		private void saveAndToast(){
		
			AppData<MyEntity> entityCollection = kinveyClient.AppData<MyEntity>(COLLECTION, typeof(MyEntity));
			MyEntity ent = new MyEntity();
			ent.ID = STABLE_ID;
			ent.Email = "test@tester.com";
			ent.Name = "James Dean";
			try{
				entityCollection.SaveBlocking(ent).Execute();
			}catch(Exception e){
				Console.WriteLine ("Uh oh! " + e);
				RunOnUiThread (() => {
					Toast.MakeText(this, "something went wrong: " + e.Message, ToastLength.Short).Show();
				});
				return;
			}
			RunOnUiThread (() => {
				Toast.MakeText(this, "saved: " + ent.Name, ToastLength.Short).Show();
			});
		
		}

		private void loadAndToast(){
			AppData<MyEntity> entityCollection = kinveyClient.AppData<MyEntity>(COLLECTION, typeof(MyEntity));
			entityCollection.setCache (myCache, CachePolicy.NO_CACHE);
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
			AppData<MyEntity> entityCollection = kinveyClient.AppData<MyEntity>(COLLECTION, typeof(MyEntity));
			entityCollection.setCache (myCache, CachePolicy.CACHE_FIRST);

			try{
				MyEntity res = entityCollection.GetEntityBlocking (STABLE_ID).Execute ();
				RunOnUiThread ( () => {
					Toast.MakeText(this, "got " + res.Name + "from cache, size: " + myCache.getSize(), ToastLength.Short).Show();
				});
			}catch(Exception e){
				Console.WriteLine ("Uh oh! " + e);
				RunOnUiThread (() => {
					Toast.MakeText(this, "something went wrong: " + e.Message, ToastLength.Short).Show();
				});
				return;
			}			


		}

	}
}


