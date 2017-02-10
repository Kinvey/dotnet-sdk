using System;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using SQLite.Net.Platform.XamarinIOS;
using Kinvey;

namespace Realtime
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

		//string appKey = "kid_byWWRXzJCe", appSecret = "4a58018febe945fea5ba76c08ce1e870"; // VINAY 1ST APP
		string appKey = "kid_BJYSU7Yug", appSecret = "9dc0806a28df425999f73767554d068d"; // [local] RealtimeTestApp

		LoginViewController vc;
		//public string UserID { get { return myClient.ActiveUser.Id; } }
		//public string AccessToken { get { return myClient.ActiveUser.AccessToken; } }

		Stream<MedicalDeviceCommand> stream;
		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			// Override point for customization after application launch.
			// If not required for your application you can safely delete this method

			BuildClient();

			return true;
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

		public void BuildClient()
		{
			Client.Builder cb = new Client.Builder(appKey, appSecret)
				.setFilePath(NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].ToString())
				.setOfflinePlatform(new SQLitePlatformIOS())
				//.setCredentdialStore(new IOSNativeCredentialStore())
				//.SetSSOGroupKey("KinveyOrg")
				.setBaseURL("http://127.0.0.1:7007/")
				.setLogger(delegate (string msg) { Console.WriteLine(msg); });

			cb.Build();

			// create a new window instance based on the screen size
			Window = new UIWindow(UIScreen.MainScreen.Bounds);

			if (Client.SharedClient.IsUserLoggedIn())
			{
				var alreadyLoggedInController = new Realtime.PubSubViewController();
				var navController = new UINavigationController(alreadyLoggedInController);
				Window.RootViewController = navController;
			}
			else
			{
				vc = new Realtime.LoginViewController();
				var navController = new UINavigationController(vc);
				Window.RootViewController = navController;
			}

			// make the window visible
			Window.MakeKeyAndVisible();
		}

		public async Task<User> Login(string user, string pass)
		{
			try
			{
				await User.LoginAsync(user, pass);

				var alreadyLoggedInController = new Realtime.PubSubViewController();
				var navController = new UINavigationController(alreadyLoggedInController);
				Window.RootViewController = navController;

				await Client.SharedClient.ActiveUser.RegisterRealtime();

				DataStore<ToDo> store = DataStore<ToDo>.Collection("ToDo", DataStoreType.NETWORK, Client.SharedClient);

				await store.Subscribe(new KinveyRealtimeDelegate<ToDo>
				{
					onError = (err) => Console.WriteLine("Error: " + err.Message),
					onSuccess = (result) => {
						Console.WriteLine("ToDo: Name: " + result.Name + " -- Details: " + result.Details);
						InvokeOnMainThread(() => alreadyLoggedInController.ChangeText(result.Name, result.Details));
					},
					OnConnectionStatusMessage = (connectstatus) => Console.WriteLine("Conn Status: " + connectstatus)
				});

				var todo = new ToDo();
				todo.Name = "Test Todo";
				todo.Details = "Test Todo Details";

				todo = await store.SaveAsync(todo);

				//stream = new Stream<MedicalDeviceCommand>("my");
				//stream.Subscribe(new KinveyRealtimeDelegate<MedicalDeviceCommand>
				//{
				//	onError = (err) => Console.WriteLine("Error: " + err.Message),
				//	onSuccess = (result) => {
				//		Console.WriteLine("SenderID: " + result.SenderID + " -- Command: " + result.Command);
				//		InvokeOnMainThread(() => alreadyLoggedInController.ChangeText(result.SenderID, result.Command));
				//	},
				//	OnConnectionStatusMessage = (connectstatus) => Console.WriteLine("Conn Status: " + connectstatus)
				//});
			}
			catch (KinveyException e)
			{
				//Console.WriteLine("VRG (exception caught) Exception Request ID -> " + e.RequestID);
				Console.WriteLine("VRG (exception caught) Exception Error -> " + e.Error);
				Console.WriteLine("VRG (exception caught) Exception Description -> " + e.Description);
				Console.WriteLine("VRG (exception caught) Exception Debug -> " + e.Debug);
			}

			return Client.SharedClient.ActiveUser;
		}

		public void Logout()
		{
			Client.SharedClient?.ActiveUser?.UnregisterRealtime();
			Client.SharedClient?.ActiveUser?.Logout();
			var logInController = new Realtime.LoginViewController();
			var navController = new UINavigationController(logInController);
			Window.RootViewController = navController;
		}

		public void Publish(string command)
		{
			var mdc = new MedicalDeviceCommand();
			mdc.Command = command;
			bool success = stream.Publish("1234abcd", mdc);
		}
	}
}

