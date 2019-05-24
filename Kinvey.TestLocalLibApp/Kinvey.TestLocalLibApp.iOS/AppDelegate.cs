using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;

namespace Kinvey.TestLocalLibApp.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            
            if (Client.SharedClient.IsUserLoggedIn())
            {
                var myDeviceToken = deviceToken.ToString();
                Client.SharedClient.Push().Initialize(myDeviceToken);
            }
        }
        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            var alert = UIAlertController.Create("Message", "Failed", UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
            Window.MakeKeyAndVisible();
            this.Window.RootViewController.PresentViewController(alert, true, null);
        }
        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            var alert = UIAlertController.Create("Message", "Received", UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
            Window.MakeKeyAndVisible();
            this.Window.RootViewController.PresentViewController(alert, true, null);
        }
    }
}
