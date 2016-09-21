using System;

using CoreGraphics;
using UIKit;

namespace testiosapp2
{
	public partial class MyViewController : UIViewController
	{
		UIApplicationDelegate appDel = UIApplication.SharedApplication.Delegate as UIApplicationDelegate;
		public UIButton button;
		UITextField usernameField;
		UITextField passwordField;

		public MyViewController()
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

			View.BackgroundColor = UIColor.Orange;
			Title = "Test App 2";

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

			var user = new UIViewController();
			user.View.BackgroundColor = UIColor.Magenta;

			button.TouchUpInside += async (sender, e) => {
				this.NavigationController.PushViewController(user, true);

				AppDelegate myAppDel = (appDel.Self as testiosapp2.AppDelegate);

				await myAppDel.Login(usernameField.Text, passwordField.Text);
			};

			View.AddSubview(button);
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}
	}
}

