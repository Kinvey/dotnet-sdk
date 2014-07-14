using System;
using System.Threading.Tasks;
using Kinvey.DotNet.Framework;
using Kinvey.DotNet.Framework.Auth;

namespace KinveyXamarin
{
	public class AsyncUser: User
	{
		public AsyncUser (AbstractClient client, KinveyAuthRequest.Builder builder) : base(client, builder)
		{
		}

		public void Login(KinveyDelegate<User> delegates)
		{
			Task.Run (() => {
				try{
					User user = base.LoginBlocking().Execute();
					delegates.onSuccess(user);
				}catch(Exception e){
					delegates.onError(e);
				}
			});
		}

		public void Login(string username, string password, KinveyDelegate<User> delegates)
		{
			Task.Run (() => {
				try{
					User user = base.LoginBlocking(username, password).Execute();
					delegates.onSuccess(user);
				}catch(Exception e){
					delegates.onError(e);
				}
			});
		}

		public void Login(Credential cred, KinveyDelegate<User> delegates)
		{
			Task.Run (() => {
				try{
					User user = base.LoginBlocking(cred).Execute();
					delegates.onSuccess(user);
				}catch(Exception e){
					delegates.onError(e);
				}
			});
		}

		public void LoginKinveyAuthToken(string userId, string authToken, KinveyDelegate<User> delegates)
		{
			Task.Run (() => {
				try{
					User user = base.LoginBlocking(userId, authToken).Execute();
					delegates.onSuccess(user);
				}catch(Exception e){
					delegates.onError(e);
				}
			});
		}

		public void logout()
		{
			Task.Run (() => {
				try{
					base.logoutBlocking().Execute();
//					delegates.onSuccess(default(User)); //TODO find a better way, logout has no return value and void is not nullable in c#
				}catch(Exception e){
//					delegates.onError(e);
					ClientLogger.Log(e);
				}
			});
		}
			
		public void Create(string userid, string password, KinveyDelegate<User> delegates)
		{
			Task.Run (() => {
				try{
					User user = base.CreateBlocking(userid, password).Execute();
					delegates.onSuccess(user);
				}catch(Exception e){
					delegates.onError(e);
				}
			});
		}

	}
}

