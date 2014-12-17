
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using KinveyXamarin;

namespace AndroidStatusShare
{
	public class LoginFragment : KinveyFragment
	{

		private EditText username;
		private EditText password;
		private Button loginButton;
		private Button registerButton;



		public override int getViewId ()
		{
			return Resource.Layout.fragment_login;
		}

		public override void bindViews (View v)
		{
			username = v.FindViewById<EditText> (Resource.Id.et_login);
			password = v.FindViewById<EditText> (Resource.Id.et_password);

			loginButton = v.FindViewById<Button> (Resource.Id.login);
			loginButton.Click += (object sender, EventArgs e) => {

				KinveyService.login(username.Text, password.Text, new KinveyDelegate<User>{ 
						onSuccess =  (user) => { 
							Activity.RunOnUiThread (() => {
								Toast.MakeText(this.Activity, "logged in as: " + user.Id, ToastLength.Short).Show();
								loggedIn();
							});
						},
						onError = (error) => {
							Activity.RunOnUiThread (() => {
								Toast.MakeText(this.Activity, "something went wrong: " + error.Message, ToastLength.Short).Show();
							});
						}
					});
			};

			registerButton = v.FindViewById<Button> (Resource.Id.login_register);
			registerButton.Click += (sender, e) => {
				KinveyService.register(username.Text, password.Text, new KinveyDelegate<User>{ 
					onSuccess =  (user) => { 
						Activity.RunOnUiThread (() => {
							Toast.MakeText(this.Activity, "created: " + user.Id, ToastLength.Short).Show();
							loggedIn();
						});
					},
					onError = (error) => {
						Activity.RunOnUiThread (() => {
							Toast.MakeText(this.Activity, "something went wrong: " + error.Message, ToastLength.Short).Show();
						});
					}
				});


			};


		}

		public override void populateViews (){}

		private void loggedIn(){
			if (this.Activity != null) {
				((StatusShare)this.Activity).ReplaceFragment (new ShareListFragment (), false);
			}

		}
	}
}

