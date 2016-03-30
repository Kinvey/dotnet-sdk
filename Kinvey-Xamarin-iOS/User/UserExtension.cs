using System;
using KinveyXamarin;
using Foundation;

namespace KinveyXamariniOS
{
	public static class UserExtension
	{
		public static bool OnOAuthCallbackRecieved(this User user, NSUrl url){
			Console.WriteLine (url.Query);
			Console.WriteLine (url.Query.Substring(url.Query.IndexOf ("code=") + 5) );
			string accesstoken = url.Query.Substring(url.Query.IndexOf ("code=") + 5) ;
			user.GetMICAccessTokenAsync(accesstoken);
			return true;
		}
	}
}

