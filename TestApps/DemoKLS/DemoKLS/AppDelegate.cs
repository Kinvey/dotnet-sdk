using System;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using SQLite.Net.Platform.XamarinIOS;
using Kinvey;

namespace DemoKLS
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

		//string appKey = "kid_S1384V55e", appSecret = "b0a209eaabc54539bc96195b40fb4be7"; // [local] DemoKLS
		string appKey = "kid_HJZHrHi5x", appSecret = "cc9fdb8a9ddc428f889bd96ea59ef2fc"; // DemoKLS

		LoginViewController vc;

		//User alice;
		//User bob;

		System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			// Override point for customization after application launch.
			// If not required for your application you can safely delete this method

			BuildClient();

			return true;
		}

		public void BuildClient()
		{
			Client.Builder cb = new Client.Builder(appKey, appSecret)
				.setFilePath(NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].ToString())
				.setOfflinePlatform(new SQLitePlatformIOS())
				.setLogger(delegate (string msg)
				{
					Console.WriteLine(msg);
				});

			cb.Build();

			// create a new window instance based on the screen size
			Window = new UIWindow(UIScreen.MainScreen.Bounds);

			if (Client.SharedClient.IsUserLoggedIn())
			{
				var alreadyLoggedInController = new PatientViewController();
				var navController = new UINavigationController(alreadyLoggedInController);
				Window.RootViewController = navController;
			}
			else
			{
				vc = new LoginViewController();
				var navController = new UINavigationController(vc);
				Window.RootViewController = navController;
			}

			// make the window visible
			Window.MakeKeyAndVisible();
		}

		public async Task<User> LoginAlice()
		{
			try
			{
				await User.LoginAsync("Alice", "alice");

				// Register for realtime
				await Client.SharedClient.ActiveUser.RegisterRealtimeAsync();

				var doctorVC = new DoctorViewController();
				var navController = new UINavigationController(doctorVC);
				Window.RootViewController = navController;
			}
			catch (KinveyException e)
			{
				if (e.ErrorCategory == EnumErrorCategory.ERROR_REALTIME)
				{
					Console.WriteLine("VRG (exception caught) Exception from Realtime operation");
				}
				Console.WriteLine("VRG (exception caught) Exception Error -> " + e.Error);
			}

			return Client.SharedClient.ActiveUser;
		}

		public async Task<User> LoginBob()
		{
			try
			{
				await User.LoginAsync("Bob", "bob");

				// Register for realtime
				await Client.SharedClient.ActiveUser.RegisterRealtimeAsync();

				var patientVC = new PatientViewController();
				var navController = new UINavigationController(patientVC);
				Window.RootViewController = navController;
			}
			catch (KinveyException e)
			{
				if (e.ErrorCategory == EnumErrorCategory.ERROR_REALTIME)
				{
					Console.WriteLine("VRG (exception caught) Exception from Realtime operation");
				}
				Console.WriteLine("VRG (exception caught) Exception Error -> " + e.Error);
			}

			return Client.SharedClient.ActiveUser;
		}

		//public async Task<User> LoginCharlie()
		//{
		//	try
		//	{
		//		await User.LoginAsync("Charlie", "charlie");

		//		var alreadyLoggedInController = new PatientViewController();
		//		var navController = new UINavigationController(alreadyLoggedInController);
		//		Window.RootViewController = navController;

		//		// REALTIME REGISTRATION

		//		// Register for realtime
		//		await Client.SharedClient.ActiveUser.RegisterRealtimeAsync();

		//		// Create stream object corresponding to "meddevcmds" stream created on the backend
		//		streamCommand = new Stream<MedicalDeviceCommand>("device_command");
		//		streamStatus = new Stream<MedicalDeviceStatus>("device_status");
		//	}
		//	catch (KinveyException e)
		//	{
		//		if (e.ErrorCategory == EnumErrorCategory.ERROR_REALTIME)
		//		{
		//			Console.WriteLine("VRG (exception caught) Exception from Realtime operation");
		//		}
		//		Console.WriteLine("VRG (exception caught) Exception Error -> " + e.Error);
		//		Console.WriteLine("VRG (exception caught) Exception Description -> " + e.Description);
		//		Console.WriteLine("VRG (exception caught) Exception Debug -> " + e.Debug);
		//		Console.WriteLine("VRG (exception caught) Exception Request ID -> " + e.RequestID);
		//	}

		//	return Client.SharedClient.ActiveUser;
		//}

		public async Task<User> LoginDan()
		{
			try
			{
				await User.LoginAsync("Dan", "dan");

				var alreadyLoggedInController = new PatientViewController();
				var navController = new UINavigationController(alreadyLoggedInController);
				Window.RootViewController = navController;

				// REALTIME REGISTRATION

				// Register for realtime
				await Client.SharedClient.ActiveUser.RegisterRealtimeAsync();

				// Create stream object corresponding to "meddevcmds" stream created on the backend
				var streamCommand = new Stream<MedicalDeviceCommand>("device_command");
				var streamStatus = new Stream<MedicalDeviceStatus>("device_status");

				// Get user IDs for granting stream access
				var criteria = new UserDiscovery();

				// Alice
				criteria.FirstName = "Alice";
				var lookup = await Client.SharedClient.ActiveUser.LookupAsync(criteria);
				User alice = lookup[0];

				// Bob
				criteria.FirstName = "Bob";
				lookup = await Client.SharedClient.ActiveUser.LookupAsync(criteria);
				User bob = lookup[0];

				//// Charlie
				//criteria.FirstName = "Charlie";
				//lookup = await Client.SharedClient.ActiveUser.LookupAsync(criteria);
				//User charlie = lookup[0];

				// Grant stream access for the device stream
				var streamACLDeviceAlice = new StreamAccessControlList();
				streamACLDeviceAlice.Publishers.Add(alice.Id);
				streamACLDeviceAlice.Subscribers.Add(bob.Id);
				bool resultGrantDevice = await streamCommand.GrantStreamAccess(bob.Id, streamACLDeviceAlice);

				//var streamACLDeviceBob = new StreamAccessControlList();
				//streamACLDeviceBob.Subscribers.Add(bob.Id);
				//bool resultGrantDeviceBob = await streamCommand.GrantStreamAccess(alice.Id, streamACLDeviceBob);

				// Grant stream access for the status stream
				var streamACLStatusBob = new StreamAccessControlList();
				streamACLStatusBob.Publishers.Add(bob.Id);
				streamACLStatusBob.Publishers.Add(alice.Id);
				streamACLStatusBob.Subscribers.Add(alice.Id);
				streamACLStatusBob.Subscribers.Add(bob.Id);
				bool resultGrantStatusBob = await streamStatus.GrantStreamAccess(bob.Id, streamACLStatusBob);

				var streamACLStatusAlice = new StreamAccessControlList();
				streamACLStatusAlice.Subscribers.Add(alice.Id);
				streamACLStatusAlice.Subscribers.Add(bob.Id);
				streamACLStatusAlice.Publishers.Add(bob.Id);
				streamACLStatusAlice.Publishers.Add(alice.Id);
				bool resultGrantStatusAlice = await streamStatus.GrantStreamAccess(alice.Id, streamACLStatusAlice);
			}
			catch (KinveyException e)
			{
				if (e.ErrorCategory == EnumErrorCategory.ERROR_REALTIME)
				{
					Console.WriteLine("VRG (exception caught) Exception from Realtime operation");
				}
				Console.WriteLine("VRG (exception caught) Exception Error -> " + e.Error);
				Console.WriteLine("VRG (exception caught) Exception Description -> " + e.Description);
				Console.WriteLine("VRG (exception caught) Exception Debug -> " + e.Debug);
				Console.WriteLine("VRG (exception caught) Exception Request ID -> " + e.RequestID);
			}

			return Client.SharedClient.ActiveUser;
		}

		public async Task Logout()
		{
			//await stream.Unsubscribe(Client.SharedClient.ActiveUser.Id);
			Client.SharedClient?.ActiveUser?.UnregisterRealtimeAsync();
			Client.SharedClient?.ActiveUser?.Logout();
			var logInController = new LoginViewController();
			var navController = new UINavigationController(logInController);
			Window.RootViewController = navController;
		}



	}
}

