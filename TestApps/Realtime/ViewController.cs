using System;
using CoreGraphics;
using UIKit;

namespace Realtime
{
	public partial class LoginViewController : UIViewController
	{
		public UIButton buttonLogin;
		public UIButton buttonLoginAuto;
		UITextField usernameField;
		UITextField passwordField;
		UIColor colorBackgroundButtonLogin = UIColor.FromRGB(5, 58, 114);

		public LoginViewController()
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
			Title = "Realtime Test App 1 - Login";

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

			var buttonWidth = (w / 2) - 20;
			buttonLogin = UIButton.FromType(UIButtonType.System);
			buttonLogin.Frame = new CGRect(10, 200, buttonWidth, 44);
			buttonLogin.SetTitle("Login", UIControlState.Normal);
			buttonLogin.SetTitleColor(UIColor.White, UIControlState.Normal);
			buttonLogin.BackgroundColor = colorBackgroundButtonLogin;

			buttonLogin.TouchUpInside += async (sender, e) => {
				AppDelegate myAppDel = (UIApplication.SharedApplication.Delegate as Realtime.AppDelegate);
				await myAppDel.Login(usernameField.Text, passwordField.Text);
			};

			View.AddSubview(buttonLogin);

			buttonLoginAuto = UIButton.FromType(UIButtonType.System);
			buttonLoginAuto.Frame = new CGRect(w-buttonWidth-10, 200, buttonWidth, 44);
			buttonLoginAuto.SetTitle("Login Test", UIControlState.Normal);
			buttonLoginAuto.SetTitleColor(UIColor.White, UIControlState.Normal);
			buttonLoginAuto.BackgroundColor = colorBackgroundButtonLogin;

			buttonLoginAuto.TouchUpInside += async (sender, e) => {
				AppDelegate myAppDel = (UIApplication.SharedApplication.Delegate as Realtime.AppDelegate);
				await myAppDel.Login("Test", "test");
			};

			View.AddSubview(buttonLoginAuto);
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}
	}

	public partial class PubSubViewController : UIViewController
	{
		internal UITextField SenderIDView;
		internal UITextField MessageView;
		nfloat h = 31.0f;
		UIColor colorBackgroundButtonLogin = UIColor.FromRGB(5, 58, 114);

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Title = "Realtime Test App 1 - Pub/Sub";
			View.BackgroundColor = UIColor.Blue;
			nfloat w = View.Bounds.Width;

			AppDelegate myAppDel = (UIApplication.SharedApplication.Delegate as Realtime.AppDelegate);

			SenderIDView = new UITextField
			{
				Placeholder = "Sender ID",
				Frame = new CGRect(10, 82, w - 20, h),
				BorderStyle = UITextBorderStyle.RoundedRect,
				BackgroundColor = UIColor.FromRGB(50, 50, 255),
				TextColor = UIColor.White
			};

			View.AddSubview(SenderIDView);

			MessageView = new UITextField
			{
				Placeholder = "Message",
				Frame = new CGRect(10, 122, w - 20, h),
				BorderStyle = UITextBorderStyle.RoundedRect,
				BackgroundColor = UIColor.FromRGB(50, 50, 255),
				TextColor = UIColor.White
			};

			View.AddSubview(MessageView);

			UITextField PublishMessageView = new UITextField
			{
				Placeholder = "Message to Publish",
				BorderStyle = UITextBorderStyle.RoundedRect,
				Frame = new CGRect(10, 202, w - 20, h),
			};

			View.AddSubview(PublishMessageView);

			UIButton buttonPublish;
			buttonPublish = UIButton.FromType(UIButtonType.System);
			buttonPublish.Frame = new CGRect(10, 242, w - 20, 44);
			buttonPublish.SetTitle("Publish", UIControlState.Normal);
			buttonPublish.SetTitleColor(UIColor.Black, UIControlState.Normal);
			buttonPublish.BackgroundColor = UIColor.Gray;
			buttonPublish.TouchUpInside += async (sender, e) => {
				await myAppDel.Publish(PublishMessageView.Text);
				PublishMessageView.Text = String.Empty;
			};

			View.AddSubview(buttonPublish);

			UIButton buttonLogout;
			buttonLogout = UIButton.FromType(UIButtonType.System);
			buttonLogout.Frame = new CGRect(10, 322, w - 20, 44);
			buttonLogout.SetTitle("Logout", UIControlState.Normal);
			buttonLogout.SetTitleColor(UIColor.Red, UIControlState.Normal);
			buttonLogout.BackgroundColor = colorBackgroundButtonLogin;

			var user = new UIViewController();
			user.View.BackgroundColor = UIColor.Blue;

			buttonLogout.TouchUpInside += async (sender, e) => {
				await myAppDel.Logout();
			};

			View.AddSubview(buttonLogout);
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
		}

		public void ChangeText(string sender, string msg)
		{
			SenderIDView.Frame = new CGRect(10, 82, View.Bounds.Width - 20, h);
			SenderIDView.Text = "Sender ID: " + sender;

			MessageView.Frame = new CGRect(10, 122, View.Bounds.Width - 20, h);
			MessageView.Text = "Message: " + msg;
		}
	}
}
