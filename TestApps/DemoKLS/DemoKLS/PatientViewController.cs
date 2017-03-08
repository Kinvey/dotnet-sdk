using System;
using System.Threading.Tasks;
using Kinvey;
using CoreGraphics;
using UIKit;


namespace DemoKLS
{
	public partial class PatientViewController : UIViewController
	{
		internal UITextField SenderIDView;
		internal UILabel MessageView;
		nfloat h = 31.0f;
		UIColor colorBackgroundButtonLogin = UIColor.FromRGB(5, 58, 114);
		//UIColor colorBackgroundButtonLogin = UIColor.FromRGB(92, 127, 159);
		UIColor colorDarkBlue = UIColor.FromRGB(7, 69, 126);
		UIColor colorLightBlue = UIColor.FromRGB(92, 127, 159);


		// REALTIME REGISTRATION
		int settingValue = 70;

		Stream<MedicalDeviceStatus> streamStatus;
		Stream<MedicalDeviceCommand> streamCommand;

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			SetupStreams();

			RenderView();
		}

		public async Task SetupStreams()
		{
			streamCommand = new Stream<MedicalDeviceCommand>("device_command");
			streamStatus = new Stream<MedicalDeviceStatus>("device_status");
			await Subscribe();
		}

		public async Task Subscribe() 
		{
			var sender = Client.SharedClient.ActiveUser.Id;
			// Set up command subscribe delegate
			var streamDelegate = new KinveyStreamDelegate<MedicalDeviceCommand>
			{
				OnError = (err) => Console.WriteLine("STREAM Error: " + err.Message),
				OnNext = async (senderID, message) => {
					//Console.WriteLine("STREAM SenderID: " + senderID + " -- Command: " + message.Command);
					if (message.Command == MedicalDeviceCommand.EnumCommand.INCREMENT)
					{
						settingValue++;
					}
					else
					{
						settingValue--;
					}
					InvokeOnMainThread(() => this.ChangeText(senderID, settingValue.ToString()));
					await this.PublishStatus(settingValue.ToString());
				},
				OnStatus = (status) => {
					Console.WriteLine("Status: " + status.Status);
				}
			};

			await streamCommand.Subscribe(sender, streamDelegate);

		}

		public async Task PublishStatus(string setting)
		{
			var receiver = Client.SharedClient.ActiveUser.Id;
			var mds = new MedicalDeviceStatus();
			mds.Setting = setting;
			await streamStatus.Publish(receiver, mds);
		}


		public void ChangeText(string sender, string msg)
		{
			MessageView.Text = msg;
		}

		private void RenderView() { 
			Title = "Kinvey Live Service - Patient";
			View.BackgroundColor = UIColor.FromRGB(7, 69, 126);
			nfloat w = View.Bounds.Width;

			AppDelegate myAppDel = (UIApplication.SharedApplication.Delegate as DemoKLS.AppDelegate);

			var titleLabel = new UILabel
			{
				Text = "My Device Reading",
				TextColor = UIColor.White,
				TextAlignment = UITextAlignment.Center,
				Frame = new CGRect(10, 80, w - 20, h)
			};

			View.AddSubview(titleLabel);

			MessageView = new UILabel
			{
				Frame = new CGRect(10, 120, w - 20, 3 * h),
				Text = this.settingValue.ToString(),
				TextColor = UIColor.White,
				Font = UIFont.FromName("Helvetica-Bold", 60f),
				TextAlignment = UITextAlignment.Center
			};

			View.AddSubview(MessageView);

			UIButton buttonLogout;
			buttonLogout = UIButton.FromType(UIButtonType.System);
			buttonLogout.Frame = new CGRect(10, 322, w - 20, 44);
			buttonLogout.SetTitle("Logout", UIControlState.Normal);
			buttonLogout.SetTitleColor(UIColor.Black, UIControlState.Normal);
			buttonLogout.BackgroundColor = UIColor.Gray;

			var user = new UIViewController();
			user.View.BackgroundColor = colorDarkBlue;

			buttonLogout.TouchUpInside += async (sender, e) => {
				await myAppDel.Logout();
			};

			View.AddSubview(buttonLogout);
		}
	}
}
