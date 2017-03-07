using System;
using System.Threading.Tasks;
using CoreGraphics;
using UIKit;
using Kinvey;
using System.Diagnostics;

namespace DemoKLS
{
	public partial class DoctorViewController : UIViewController
	{
		internal UITextField SenderIDView;
		internal UILabel MessageView;
		internal UITextField TimeView;

		nfloat h = 31.0f;
		//UIColor colorBackgroundButtonLogin = UIColor.FromRGB(5, 58, 114);
		UIColor colorBackgroundButtonLogin = UIColor.FromRGB(92, 127, 159);
		UIColor colorDarkBlue = UIColor.FromRGB(7, 69, 126);
		UIColor colorLightBlue = UIColor.FromRGB(92, 127, 159);

		Stream<MedicalDeviceCommand> streamCommand;
		Stream<MedicalDeviceStatus> streamStatus;
		User patient;

		Stopwatch stopwatch = new Stopwatch();

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			SetupStreams();

			RenderView();
		}

		public async Task SetupStreams() { 

			// Create stream object corresponding to "meddevcmds" stream created on the backend
			streamCommand = new Stream<MedicalDeviceCommand>("device_command");
			streamStatus = new Stream<MedicalDeviceStatus>("device_status");

			// Subscribe to status stream for Bob
			var criteria = new UserDiscovery();
			criteria.FirstName = "Bob";
			var lookup = await Client.SharedClient.ActiveUser.LookupAsync(criteria);

			patient = lookup[0];

			await Subscribe();
		}

		public async Task Subscribe() { 
			// Set up status subscribe delegate
			var streamDelegate = new KinveyStreamDelegate<MedicalDeviceStatus>
			{
				OnError = (err) => Console.WriteLine("STREAM Error: " + err.Message),
				OnNext = (senderID, message) => {
					stopwatch.Stop();
					TimeSpan timeForRoundtrip = stopwatch.Elapsed;
					stopwatch.Reset();
					string time = timeForRoundtrip.TotalMilliseconds + " ms";
					InvokeOnMainThread(() => this.ChangeText(message.Setting, time));
				},
				OnStatus = (status) => {
					Console.WriteLine("Status: " + status.Status);
				}
			};

			await streamStatus.Subscribe(patient.Id, streamDelegate);			
		}

		public async Task Publish(MedicalDeviceCommand.EnumCommand command)
		{
			var mdc = new MedicalDeviceCommand();
			mdc.Command = command;
			stopwatch.Start();
			await streamCommand.Publish(patient.Id, mdc);
		}

		private void RenderView() { 
			Title = "Kinvey Live Service - Doctor";
			View.BackgroundColor = UIColor.FromRGB(7, 69, 126);
			nfloat w = View.Bounds.Width;
			var buttonWidth = (w / 2) - 20;

			AppDelegate myAppDel = (UIApplication.SharedApplication.Delegate as DemoKLS.AppDelegate);
			var titleLabel = new UILabel
			{
				Text = "Bob's Device Reading",
				TextColor = UIColor.White,
				Frame = new CGRect(10, 80, w - 20, h),
				TextAlignment = UITextAlignment.Center	
			};

			View.AddSubview(titleLabel);

			MessageView = new UILabel
			{
				Text = "--",
				Frame = new CGRect(10, 120, w - 20, 3*h),
				TextColor = UIColor.White,
				Font = UIFont.FromName("Helvetica-Bold", 60f),
				TextAlignment = UITextAlignment.Center
			};

			View.AddSubview(MessageView);

			TimeView = new UITextField
			{
				Placeholder = "Roundtrip Time",
				Frame = new CGRect(10, 200, w - 20, h),
				BorderStyle = UITextBorderStyle.RoundedRect,
				BackgroundColor = UIColor.White,
				TextColor = UIColor.Black
			};

			View.AddSubview(TimeView);
			TimeView.Hidden = true;


			UIButton buttonPublishDecrement;
			buttonPublishDecrement = UIButton.FromType(UIButtonType.System);
			buttonPublishDecrement.Frame = new CGRect(10, 280, buttonWidth, 44);
			buttonPublishDecrement.SetTitle("Decrease", UIControlState.Normal);
			buttonPublishDecrement.SetTitleColor(UIColor.Red, UIControlState.Normal);
			buttonPublishDecrement.BackgroundColor = colorLightBlue;
			buttonPublishDecrement.TouchUpInside += async (sender, e) => {
				await this.Publish(MedicalDeviceCommand.EnumCommand.DECREMENT);
				//PublishMessageView.Text = String.Empty;
			};

			View.AddSubview(buttonPublishDecrement);

			UIButton buttonPublishIncrement;
			buttonPublishIncrement = UIButton.FromType(UIButtonType.System);
			buttonPublishIncrement.Frame = new CGRect(w - buttonWidth - 10, 280, buttonWidth, 44);
			buttonPublishIncrement.SetTitle("Increase", UIControlState.Normal);
			buttonPublishIncrement.SetTitleColor(UIColor.Green, UIControlState.Normal);
			buttonPublishIncrement.BackgroundColor = colorLightBlue;
			//buttonPublishIncrement.BackgroundColor = UIColor.Green;
			buttonPublishIncrement.TouchUpInside += async (sender, e) => {
				await this.Publish(MedicalDeviceCommand.EnumCommand.INCREMENT);
				//PublishMessageView.Text = String.Empty;
			};

			View.AddSubview(buttonPublishIncrement);

			UIButton buttonLogout;
			buttonLogout = UIButton.FromType(UIButtonType.System);
			buttonLogout.Frame = new CGRect(10, 360, w - 20, 44);
			buttonLogout.SetTitle("Logout", UIControlState.Normal);
			buttonLogout.SetTitleColor(UIColor.Black, UIControlState.Normal);
			buttonLogout.BackgroundColor = UIColor.Gray;

			var user = new UIViewController();
			user.View.BackgroundColor = UIColor.FromRGB(7, 69, 126);

			buttonLogout.TouchUpInside += async (sender, e) => {
				await myAppDel.Logout();
			};

			View.AddSubview(buttonLogout);
		}

		public void ChangeText(string msg, string time)
		{
			MessageView.Text = msg;
			TimeView.Text = "Roundtrip Time: " + time;
		}
	}
}
