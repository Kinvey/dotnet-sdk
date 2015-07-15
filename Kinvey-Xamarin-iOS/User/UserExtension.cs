using System;
using KinveyXamarin;
using Foundation;

namespace KinveyXamariniOS
{
	public static class UserExtension
	{
		public static bool OnOAuthCallbackRecieved(this AsyncUser user, NSUrl url){
			Console.WriteLine (url.Query);
			Console.WriteLine (url.Query.Substring(url.Query.IndexOf ("code=") + 5) );
			string accesstoken = url.Query.Substring(url.Query.IndexOf ("code=") + 5) ;
			user.GetMICAccessToken(accesstoken);
			return true;
		}
	}
}

