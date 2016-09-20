using System;

using CoreGraphics;
using UIKit;

namespace testiosapp2
{
	public partial class MyViewController : UIViewController
	{
		public UIButton button;

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

			button = UIButton.FromType(UIButtonType.System);
			button.Frame = new CGRect(20, 200, 280, 44);
			button.SetTitle("Click Me", UIControlState.Normal);

			var user = new UIViewController();
			user.View.BackgroundColor = UIColor.Magenta;

			button.TouchUpInside += (sender, e) => {
				this.NavigationController.PushViewController(user, true);
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

