using System;
using CoreGraphics;
using UIKit;

namespace DemoKLS
{
	public partial class LoginViewController : UIViewController
	{
		public UIButton buttonLogin;
		public UIButton buttonLoginAuto;
		public UIButton buttonLoginAlice;
		public UIButton buttonLoginBob;
		public UIButton buttonLoginCharlie;
		public UIButton buttonLoginDan;
		UITextField usernameField;
		UITextField passwordField;
		//UIColor colorBackgroundButtonLogin = UIColor.FromRGB(5, 58, 114);
		UIColor colorBackgroundButtonLogin = UIColor.FromRGB(92, 127, 159);

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

			View.BackgroundColor = UIColor.FromRGB(7, 69, 126);
			Title = "Demo Kinvey Live Service - Login";

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
			buttonLogin.Frame = new CGRect(10, 162, w - 20, 44);
			buttonLogin.SetTitle("Login", UIControlState.Normal);
			buttonLogin.SetTitleColor(UIColor.White, UIControlState.Normal);
			buttonLogin.BackgroundColor = colorBackgroundButtonLogin;

			buttonLogin.TouchUpInside += async (sender, e) => {
				AppDelegate myAppDel = (UIApplication.SharedApplication.Delegate as DemoKLS.AppDelegate);
				await myAppDel.Login(usernameField.Text, passwordField.Text);
			};

			View.AddSubview(buttonLogin);

			//buttonLoginAuto = UIButton.FromType(UIButtonType.System);
			//buttonLoginAuto.Frame = new CGRect(w - buttonWidth - 10, 200, buttonWidth, 44);
			//buttonLoginAuto.SetTitle("Login Test", UIControlState.Normal);
			//buttonLoginAuto.SetTitleColor(UIColor.White, UIControlState.Normal);
			//buttonLoginAuto.BackgroundColor = colorBackgroundButtonLogin;

			//buttonLoginAuto.TouchUpInside += async (sender, e) => {
			//	AppDelegate myAppDel = (UIApplication.SharedApplication.Delegate as DemoKLS.AppDelegate);
			//	await myAppDel.Login("Test", "test");
			//};

			buttonLoginAlice = UIButton.FromType(UIButtonType.System);
			buttonLoginAlice.Frame = new CGRect(10, 250, w - 20, 44);
			buttonLoginAlice.SetTitle("Login Dr. Alice", UIControlState.Normal);
			buttonLoginAlice.SetTitleColor(UIColor.White, UIControlState.Normal);
			buttonLoginAlice.BackgroundColor = colorBackgroundButtonLogin;

			buttonLoginAlice.TouchUpInside += async (sender, e) => {
				AppDelegate myAppDel = (UIApplication.SharedApplication.Delegate as DemoKLS.AppDelegate);
				await myAppDel.LoginAlice();
			};

			View.AddSubview(buttonLoginAlice);

			buttonLoginBob = UIButton.FromType(UIButtonType.System);
			buttonLoginBob.Frame = new CGRect(10, 300, w - 20, 44);
			buttonLoginBob.SetTitle("Login Bob", UIControlState.Normal);
			buttonLoginBob.SetTitleColor(UIColor.White, UIControlState.Normal);
			buttonLoginBob.BackgroundColor = colorBackgroundButtonLogin;

			buttonLoginBob.TouchUpInside += async (sender, e) => {
				AppDelegate myAppDel = (UIApplication.SharedApplication.Delegate as DemoKLS.AppDelegate);
				await myAppDel.LoginBob();
			};

			View.AddSubview(buttonLoginBob);

			//buttonLoginCharlie = UIButton.FromType(UIButtonType.System);
			//buttonLoginCharlie.Frame = new CGRect(10, 350, w - 20, 44);
			//buttonLoginCharlie.SetTitle("Login Charlie", UIControlState.Normal);
			//buttonLoginCharlie.SetTitleColor(UIColor.White, UIControlState.Normal);
			//buttonLoginCharlie.BackgroundColor = colorBackgroundButtonLogin;

			//buttonLoginCharlie.TouchUpInside += async (sender, e) => {
			//	AppDelegate myAppDel = (UIApplication.SharedApplication.Delegate as DemoKLS.AppDelegate);
			//	await myAppDel.LoginCharlie();
			//};

			//View.AddSubview(buttonLoginCharlie);

			buttonLoginDan = UIButton.FromType(UIButtonType.System);
			buttonLoginDan.Frame = new CGRect(10, 400, w - 20, 44);
			buttonLoginDan.SetTitle("Login Dan", UIControlState.Normal);
			buttonLoginDan.SetTitleColor(UIColor.White, UIControlState.Normal);
			buttonLoginDan.BackgroundColor = colorBackgroundButtonLogin;

			buttonLoginDan.TouchUpInside += async (sender, e) => {
				AppDelegate myAppDel = (UIApplication.SharedApplication.Delegate as DemoKLS.AppDelegate);
				await myAppDel.LoginDan();
			};

			View.AddSubview(buttonLoginDan);
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}
	}

	public partial class PatientViewController : UIViewController
	{
		internal UITextField SenderIDView;
		internal UITextField MessageView;
		nfloat h = 31.0f;
		UIColor colorBackgroundButtonLogin = UIColor.FromRGB(5, 58, 114);
		//UIColor colorBackgroundButtonLogin = UIColor.FromRGB(92, 127, 159);

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Title = "Demo Kinvey Live Service - Patient";
			View.BackgroundColor = UIColor.FromRGB(7, 69, 126);
			nfloat w = View.Bounds.Width;

			AppDelegate myAppDel = (UIApplication.SharedApplication.Delegate as DemoKLS.AppDelegate);

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

			//UITextField PublishMessageView = new UITextField
			//{
			//	Placeholder = "Message to Publish",
			//	BorderStyle = UITextBorderStyle.RoundedRect,
			//	Frame = new CGRect(10, 202, w - 20, h),
			//};

			//View.AddSubview(PublishMessageView);

			//UIButton buttonPublish;
			//buttonPublish = UIButton.FromType(UIButtonType.System);
			//buttonPublish.Frame = new CGRect(10, 242, w - 20, 44);
			//buttonPublish.SetTitle("Publish", UIControlState.Normal);
			//buttonPublish.SetTitleColor(UIColor.Black, UIControlState.Normal);
			//buttonPublish.BackgroundColor = UIColor.Gray;
			//buttonPublish.TouchUpInside += async (sender, e) => {
			//	await myAppDel.PublishStatus(PublishMessageView.Text);
			//	PublishMessageView.Text = String.Empty;
			//};

			//View.AddSubview(buttonPublish);

			UIButton buttonLogout;
			buttonLogout = UIButton.FromType(UIButtonType.System);
			buttonLogout.Frame = new CGRect(10, 322, w - 20, 44);
			buttonLogout.SetTitle("Logout", UIControlState.Normal);
			buttonLogout.SetTitleColor(UIColor.Red, UIControlState.Normal);
			buttonLogout.BackgroundColor = colorBackgroundButtonLogin;

			var user = new UIViewController();
			user.View.BackgroundColor = UIColor.FromRGB(7, 69,126);

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

	public partial class DoctorViewController : UIViewController
	{
		internal UITextField SenderIDView;
		internal UITextField MessageView;
		nfloat h = 31.0f;
		//UIColor colorBackgroundButtonLogin = UIColor.FromRGB(5, 58, 114);
		UIColor colorBackgroundButtonLogin = UIColor.FromRGB(92, 127, 159);

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Title = "Demo Kinvey Live Service - Doctor";
			View.BackgroundColor = UIColor.FromRGB(7, 69, 126);
			nfloat w = View.Bounds.Width;
			var buttonWidth = (w / 2) - 20;

			AppDelegate myAppDel = (UIApplication.SharedApplication.Delegate as DemoKLS.AppDelegate);

			SenderIDView = new UITextField
			{
				Placeholder = "Sender ID",
				Frame = new CGRect(10, 82, w - 20, h),
				BorderStyle = UITextBorderStyle.RoundedRect,
				BackgroundColor = colorBackgroundButtonLogin,
				TextColor = UIColor.White
			};

			View.AddSubview(SenderIDView);

			MessageView = new UITextField
			{
				Placeholder = "Message",
				Frame = new CGRect(10, 122, w - 20, h),
				BorderStyle = UITextBorderStyle.RoundedRect,
				BackgroundColor = colorBackgroundButtonLogin,
				TextColor = UIColor.White
			};

			View.AddSubview(MessageView);

			//UITextField PublishMessageView = new UITextField
			//{
			//	Placeholder = "Message to Publish",
			//	BorderStyle = UITextBorderStyle.RoundedRect,
			//	Frame = new CGRect(10, 202, w - 20, h),
			//};

			//View.AddSubview(PublishMessageView);

			UIButton buttonPublishDecrement;
			buttonPublishDecrement = UIButton.FromType(UIButtonType.System);
			buttonPublishDecrement.Frame = new CGRect(10, 242, buttonWidth, 44);
			buttonPublishDecrement.SetTitle("Decrement", UIControlState.Normal);
			buttonPublishDecrement.SetTitleColor(UIColor.Black, UIControlState.Normal);
			buttonPublishDecrement.BackgroundColor = UIColor.Gray;
			buttonPublishDecrement.TouchUpInside += async (sender, e) => {
				await myAppDel.PublishCommand("Dec");
				//PublishMessageView.Text = String.Empty;
			};

			View.AddSubview(buttonPublishDecrement);

			UIButton buttonPublishIncrement;
			buttonPublishIncrement = UIButton.FromType(UIButtonType.System);
			buttonPublishIncrement.Frame = new CGRect(w - buttonWidth - 10, 242, buttonWidth, 44);
			buttonPublishIncrement.SetTitle("Increment", UIControlState.Normal);
			buttonPublishIncrement.SetTitleColor(UIColor.Black, UIControlState.Normal);
			buttonPublishIncrement.BackgroundColor = UIColor.Gray;
			buttonPublishIncrement.TouchUpInside += async (sender, e) => {
				await myAppDel.PublishCommand("Inc");
				//PublishMessageView.Text = String.Empty;
			};

			View.AddSubview(buttonPublishIncrement);

			//buttonLoginAuto = UIButton.FromType(UIButtonType.System);
			//buttonLoginAuto.Frame = new CGRect(w - buttonWidth - 10, 200, buttonWidth, 44);
			//buttonLoginAuto.SetTitle("Login Test", UIControlState.Normal);
			//buttonLoginAuto.SetTitleColor(UIColor.White, UIControlState.Normal);
			//buttonLoginAuto.BackgroundColor = colorBackgroundButtonLogin;

			//buttonLoginAuto.TouchUpInside += async (sender, e) => {
			//	AppDelegate myAppDel = (UIApplication.SharedApplication.Delegate as DemoKLS.AppDelegate);
			//	await myAppDel.Login("Test", "test");
			//};

			UIButton buttonLogout;
			buttonLogout = UIButton.FromType(UIButtonType.System);
			buttonLogout.Frame = new CGRect(10, 322, w - 20, 44);
			buttonLogout.SetTitle("Logout", UIControlState.Normal);
			buttonLogout.SetTitleColor(UIColor.Red, UIControlState.Normal);
			buttonLogout.BackgroundColor = colorBackgroundButtonLogin;

			var user = new UIViewController();
			user.View.BackgroundColor = UIColor.FromRGB(7, 69, 126);

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
