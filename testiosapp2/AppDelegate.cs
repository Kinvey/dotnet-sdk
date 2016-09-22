using Foundation;
using UIKit;

using KinveyXamarin;
using KinveyXamariniOS;
using SQLite.Net.Platform.XamarinIOS;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace testiosapp2
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations

		public override UIWindow Window
		{
			get;
			set;
		}

		Client myClient;
		testiosapp2.LoginViewController vc;
		public string UserID { get { return myClient.ActiveUser.Id; } }
		public string AccessToken { get { return myClient.ActiveUser.AccessToken; } }

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			// Override point for customization after application launch.
			// If not required for your application you can safely delete this method

			BuildClient();

			return true;
		}

		public async Task BuildClient()
		{
			//myClient = new Client.Builder("kid_b1d6IY_x7l", "079412ee99f4485d85e6e362fb987de8")
			//myClient = new Client.Builder ("kid_ZkPDb_34T", "c3752d5079f34353ab89d07229efaf63") // MIC-SAML-TEST
			Client.Builder cb = new Client.Builder("kid_BkAIHRRh", "7772b17762e44c87a9b5783c35ea5930") // SSO-TEST
				.setFilePath(NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].ToString())
				.setOfflinePlatform(new SQLitePlatformIOS())
				.setBaseURL("https://alm-kcs.ngrok.io")
				.setLogger(delegate (string msg) { Console.WriteLine(msg); });

			myClient = await cb.Build();

			myClient.MICHostName = "https://alm-auth.ngrok.io"; // SSO-TEST
			myClient.MICApiVersion = "v3"; // SSO-TEST

			// create a new window instance based on the screen size
			Window = new UIWindow(UIScreen.MainScreen.Bounds);

			//if (true)
			if (myClient.IsUserLoggedIn())
			{
				var alreadyLoggedInController = new testiosapp2.DataViewController();
				var navController = new UINavigationController(alreadyLoggedInController);
				Window.RootViewController = navController;
			}
			else
			{
				vc = new testiosapp2.LoginViewController();
				var navController = new UINavigationController(vc);
				Window.RootViewController = navController;
			}

			// make the window visible
			Window.MakeKeyAndVisible();
		}

		public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
		{
			return myClient.ActiveUser.OnOAuthCallbackRecieved(url);
		}

		public async Task<User> Login(string user, string pass)
		{
			//			Dictionary<string, JToken> attr = new Dictionary<string, JToken> ();
			//			email.Add ("email", "gob@bluth.com");
			//			attr.Add ("my_field", "blah blah");
			//			var newuser = await myClient. ().CreateAsync ("George Michael Bluth", "cousin", attr);
			//			var newuser2 = await myClient.CurrentUser.CreateAsync ("Tobias Funke", "actor");
			//			Dictionary<string, JToken> last_name = new Dictionary<string, JToken>();
			//			last_name.Add("last_name", "Bluth");
			//			await myClient.CurrentUser.CreateAsync ("Lindsay Bluth", "me", last_name);
			//			await myClient.CurrentUser.CreateAsync ("Maeby Bluth", "Surely", last_name);

			try
			{
				//user = await User.LoginAsync("test", "test", myClient);
				//string username = "test";
				//string password = "test";
				string redirectURI = "kinveyAuthDemo://";

				await User.LoginWithAuthorizationCodeAPIAsync(user, pass, redirectURI, myClient);

				//					myClient.CurrentUser.LoginWithAuthorizationCodeLoginPage("kinveyAuthDemo://", new KinveyMICDelegate<User>{
				//						onSuccess = (loggedInUser) => { user = loggedInUser; },
				//						onError = (e) => { Console.WriteLine("Error with MIC Login"); },
				//						onReadyToRender = (url) => { UIApplication.SharedApplication.OpenUrl(new NSUrl(url)); }
				//					});


				//string str = "Finished Launching.";
				//Console.WriteLine("VRG : " + str);
				//Console.WriteLine("VRG: Logged in as: " + myClient.ActiveUser.Id);

				//var alert = UIAlertController.Create("UserID: " + myClient.ActiveUser.Id, "AccessToken: " + myClient.ActiveUser.AccessToken, UIAlertControllerStyle.Alert);
				//if (alert.PopoverPresentationController != null)
				//	alert.PopoverPresentationController.but.BarButtonItem = cvc.button;
				//alert.PresentViewController(alert, animated: true, completionHandler: null);
				//alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
				//vc.PresentViewController(alert, true, null);
				var alreadyLoggedInController = new testiosapp2.DataViewController();
				var navController = new UINavigationController(alreadyLoggedInController);
				Window.RootViewController = navController;

				await ManipulateData();

			}
			catch (KinveyException e)
			{
				//Console.WriteLine("VRG (exception caught) Exception Request ID -> " + e.RequestID);
				Console.WriteLine("VRG (exception caught) Exception Error -> " + e.Error);
				Console.WriteLine("VRG (exception caught) Exception Description -> " + e.Description);
				Console.WriteLine("VRG (exception caught) Exception Debug -> " + e.Debug);
			}

			return myClient.ActiveUser;
		}

		private async Task<DataStore<Book>> ManipulateData()
		{
			DataStore<Book> store = DataStore<Book>.Collection("Book", DataStoreType.NETWORK);
			try
			{

				List<Book> listBooks = new List<Book>();
				listBooks = await store.FindAsync();

			}
			catch (Exception e)
			{
				Console.Write(e);
			}
			return store;
		}

		public override void OnResignActivation(UIApplication application)
		{
			// Invoked when the application is about to move from active to inactive state.
			// This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
			// or when the user quits the application and it begins the transition to the background state.
			// Games should use this method to pause the game.
		}

		public override void DidEnterBackground(UIApplication application)
		{
			// Use this method to release shared resources, save user data, invalidate timers and store the application state.
			// If your application supports background exection this method is called instead of WillTerminate when the user quits.
		}

		public override void WillEnterForeground(UIApplication application)
		{
			// Called as part of the transiton from background to active state.
			// Here you can undo many of the changes made on entering the background.
		}

		public override void OnActivated(UIApplication application)
		{
			// Restart any tasks that were paused (or not yet started) while the application was inactive. 
			// If the application was previously in the background, optionally refresh the user interface.
		}

		public override void WillTerminate(UIApplication application)
		{
			// Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
		}
	}
}


