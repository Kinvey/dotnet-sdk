using System;
using KinveyXamarin;
using Android.Content;

namespace KinveyXamarinAndroid
{
	public static class UserExtension
	{
		public static void OnOAuthCallbackRecieved(this User user, Intent intent){

			global::Android.Net.Uri uri = intent.Data;
			string accessToken = uri.GetQueryParameter("code");
			user.GetMICAccessTokenAsync(accessToken);
		}

	}
}
