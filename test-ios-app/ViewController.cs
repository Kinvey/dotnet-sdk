using System;

using CoreGraphics;
using UIKit;

namespace testiosapp
{
	public partial class MyLoginViewController : UIViewController
	{
		public UIButton button;
		UITextField usernameField;
		UITextField passwordField;

		public MyLoginViewController()
		{
		}

		//protected MyViewController(IntPtr handle) : base(handle)
		//{
		//	// Note: this .ctor should not contain any initialization logic.
		//}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			// Perform any additional setup after loading the view, typically from a nib.

			View.BackgroundColor = UIColor.Blue;
			Title = "SSO Test App 1";

			nfloat h = 31.0f;
			nfloat w = View.Bounds.Width;

			usernameField = new UITextField
			{
				Placeholder = "Username",
				BorderStyle = UITextBorderStyle.RoundedRect,
				Frame = new CGRect(10, 82, w - 20, h)
			};
			View.AddSubview(usernameField);

			passwordField = new UITextField
			{
				SecureTextEntry = true,
				Placeholder = "Password",
				BorderStyle = UITextBorderStyle.RoundedRect,
				Frame = new CGRect(10, 122, w - 20, h)
			};
			View.AddSubview(passwordField);

			button = UIButton.FromType(UIButtonType.System);
			button.Frame = new CGRect(10, 200, w - 20, 44);
			button.SetTitle("Login", UIControlState.Normal);
			button.SetTitleColor(UIColor.Black, UIControlState.Normal);
			button.BackgroundColor = UIColor.LightGray;

			//var user = new UIViewController();
			//user.View.BackgroundColor = UIColor.Magenta;

			button.TouchUpInside += async (sender, e) => {

				AppDelegate myAppDel = (UIApplication.SharedApplication.Delegate as testiosapp.AppDelegate);

				await myAppDel.Login(usernameField.Text, passwordField.Text);

				//this.NavigationController.PushViewController(user, true);
			};

			View.AddSubview(button);
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}
	}

	public partial class DataViewController : UIViewController
	{
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Title = "Login Succeeded!";
			View.BackgroundColor = UIColor.Blue;
			nfloat h = 31.0f;
			nfloat w = View.Bounds.Width;

			AppDelegate myAppDel = (UIApplication.SharedApplication.Delegate as testiosapp.AppDelegate);

			UITextField IDView = new UITextField
			{
				Text = "User ID: " + myAppDel.UserID,
				TextAlignment = UITextAlignment.Center,
				Frame = new CGRect(10, 82, w - 20, h),
				BackgroundColor = UIColor.White
			};
			View.AddSubview(IDView);

			UITextField AccessTokenView = new UITextField
			{
				Text = "Access Token: " + myAppDel.AccessToken,
				TextAlignment = UITextAlignment.Center,
				Frame = new CGRect(10, 122, w - 20, h),
				BackgroundColor = UIColor.White
			};
			View.AddSubview(AccessTokenView);
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
		}
	}
}
