// Copyright (c) 2015, Kinvey, Inc. All rights reserved.
//
// This software is licensed to you under the Kinvey terms of service located at
// http://www.kinvey.com/terms-of-use. By downloading, accessing and/or using this
// software, you hereby accept such terms of service  (and any agreement referenced
// therein) and agree that you have read, understand and agree to be bound by such
// terms of service and are of legal age to agree to such terms with Kinvey.
//
// This software contains valuable confidential and proprietary information of
// KINVEY, INC and is subject to applicable licensing agreements.
// Unauthorized reproduction, transmission or distribution of this file and its
// contents is a violation of applicable laws.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KinveyUtils;
using Newtonsoft.Json.Linq;

namespace KinveyXamarin
{
	/// <summary>
	/// Async user.  This class allows access to Kinvey's User API asynchronously.  
	/// </summary>
	public class AsyncUser: User
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.AsyncUser"/> class.
		/// </summary>
		/// <param name="client">A configured instance of a Kinvey client.</param>
		/// <param name="client">A configured AuthRequest Builder, containing information about the upcoming login request.</param>
		public AsyncUser (AbstractClient client, KinveyAuthRequest.Builder builder) : base(client, builder)
		{
		}

//		/// <summary>
//		/// Login (and create) an new kinvey user without any specified details.
//		/// </summary>
//		/// <param name="delegates">Delegates for success or failure.</param>
//		public void Login(KinveyDelegate<User> delegates)
//		{
//			Task.Run (() => {
//				try{
//					User user = base.LoginBlocking().Execute();
//					delegates.onSuccess(user);
//				}catch(Exception e){
//					delegates.onError(e);
//				}
//			});
//		}

			
//		/// <summary>
//		/// Login with a specified username and password.
//		/// </summary>
//		/// <param name="username">The username.</param>
//		/// <param name="password">The password.</param>
//		/// <param name="delegates">Delegates for success or failure.</param>
//		public void Login(string username, string password, KinveyDelegate<User> delegates)
//		{
//			Task.Run (() => {
//				try{
//					User user = base.LoginBlocking(username, password).Execute();
//					delegates.onSuccess(user);
//				}catch(Exception e){
//					delegates.onError(e);
//				}
//			});
//		}



//		/// <summary>
//		/// Login with a Kinvey Auth Token directly.
//		/// </summary>
//		/// <param name="userId">The _id of the current user.</param>
//		/// <param name="authToken">The user's Kinvey Auth Token..</param>
//		/// <param name="delegates">Delegates for success or failure.</param>
//		public void LoginKinveyAuthToken(string userId, string authToken, KinveyDelegate<User> delegates)
//		{
//			Task.Run (() => {
//				try{
//					User user = base.LoginBlocking(userId, authToken).Execute();
//					delegates.onSuccess(user);
//				}catch(Exception e){
//					delegates.onError(e);
//				}
//			});
//		}




//		/// <summary>
//		/// Login with a third party identity
//		/// </summary>
//		/// <param name="identity">The Third party identity.</param>
//		/// <param name="delegates">Delegates for success or failure.</param>
//		public void Login(ThirdPartyIdentity identity, KinveyDelegate<User> delegates)
//		{
//			Task.Run (() => {
//				try{
//					User user = base.LoginBlocking(identity).Execute();
//					delegates.onSuccess(user);
//				}catch(Exception e){
//					delegates.onError(e);
//				}
//			});
//		}



//		/// <summary>
//		/// Create a new Kinvey user, with the specified username and password.
//		/// </summary>
//		/// <param name="userid">the username.</param>
//		/// <param name="password">the password.</param>
//		/// <param name="delegates">Delegates for success or failure.</param>
//		/// <param name="customFieldsAndValues">[optional] Custom key/value pairs to be added to user at creation.</param>
//		public void Create(string username, string password, KinveyDelegate<User> delegates, Dictionary<string, JToken> customFieldsAndValues = null)
//		{
//			Task.Run (() => {
//				try{
//					User user = base.CreateBlocking(username, password, customFieldsAndValues).Execute();
//					delegates.onSuccess(user);
//				}catch(Exception e){
//					delegates.onError(e);
//				}
//			});
//		}


//		/// <summary>
//		/// Retrieve the specified User
//		/// </summary>
//		/// <param name="userid">Userid.</param>
//		/// <param name="delegates">Delegates for success or failure.</param>
//		public void Retrieve(string userid, KinveyDelegate<User> delegates)
//		{
//			Task.Run (() => {
//				try{
//					User user = base.RetrieveBlocking(userid).Execute();
//					delegates.onSuccess(user);
//				}catch(Exception e){
//					delegates.onError(e);
//				}
//			});
//		}


//		/// <summary>
//		/// Resolve the specified query, resolves, resolve_depth, retain to get a set of users
//		/// </summary>
//		/// <param name="query">Query.</param>
//		/// <param name="resolves">Resolves.</param>
//		/// <param name="resolve_depth">Resolve depth.</param>
//		/// <param name="retain">If set to <c>true</c> retain references.</param>
//		/// <param name="delegates">Delegates for success or failure.</param>
//		public void Retrieve(string query, string[] resolves, int resolve_depth, bool retain, KinveyDelegate<User[]> delegates)
//		{
//			Task.Run (() => {
//				try{
//					User[] user = base.RetrieveBlocking(query, resolves, resolve_depth, retain).Execute();
//					delegates.onSuccess(user);
//				}catch(Exception e){
//					delegates.onError(e);
//				}
//			});
//		}


//		/// <summary>
//		/// User Discovery Lookup of users, based on supplied criteria.
//		/// </summary>
//		/// <returns>The async task which will return an array of User objects.</returns>
//		/// <param name="criteria">UserDiscovery object which contains the lookup criteria.</param>
//		/// <param name="delegates">Delegates for success or failure.</param>
//		public void Lookup(UserDiscovery criteria, KinveyDelegate<User[]> delegates)
//		{
//			Task.Run ( () => {
//				try
//				{
//					User[] users = default(User[]);
//					if ((criteria != null) &&
//						(criteria.getCriteria() != null) &&
//						(criteria.getCriteria().Count > 0))
//					{
//						users = base.LookupBlocking(criteria).Execute();
//					}
//					delegates.onSuccess(users);
//
//				}
//				catch(Exception e)
//				{
//					delegates.onError(e);
//				}
//			});
//		}

//		/// <summary>
//		/// Update the current user
//		/// </summary>
//		/// <param name="delegates">Delegates for success or failure.</param>
//		public void Update(KinveyDelegate<User> delegates)
//		{
//			Task.Run (() => {
//				try{
//					User user = base.UpdateBlocking(this).Execute();
//					delegates.onSuccess(user);
//				}catch(Exception e){
//					delegates.onError(e);
//				}
//			});
//		}

//		/// <summary>
//		/// Update the specified user
//		/// </summary>
//		/// <param name="user">User.</param>
//		/// <param name="delegates">Delegates for success or failure.</param>
//		public void Update(User user, KinveyDelegate<User> delegates)
//		{
//			Task.Run (() => {
//				try{
//					User us = base.UpdateBlocking(user).Execute();
//					delegates.onSuccess(us);
//				}catch(Exception e){
//					delegates.onError(e);
//				}
//			});
//		}


//		/// <summary>
//		/// Resets the password for the specified user id
//		/// </summary>
//		/// <param name="userid">Userid.</param>
//		/// <param name="delegates">Delegates for success or failure.</param>
//		public void ResetPassword(string userid, KinveyDelegate<User> delegates)
//		{
//			Task.Run (() => {
//				try{
//					User user = base.ResetPasswordBlocking(userid).Execute();
//					delegates.onSuccess(user);
//				}catch(Exception e){
//					delegates.onError(e);
//				}
//			});
//		}

//		/// <summary>
//		/// Delete the specified userid, with a flag for hard delete
//		/// </summary>
//		/// <param name="userid">Userid.</param>
//		/// <param name="hard">If set to <c>true</c> the user will be permanently deleted.</param>
//		/// <param name="delegates">Delegates for success or failure.</param>
//		public void Delete(string userid, bool hard, KinveyDelegate<KinveyDeleteResponse> delegates)
//		{
//			Task.Run (() => {
//				try{
//					KinveyDeleteResponse del = base.DeleteBlocking(userid, hard).Execute();
//					delegates.onSuccess(del);
//				}catch(Exception e){
//					delegates.onError(e);
//				}
//			});
//		}

//		/// <summary>
//		/// Sends a verification email
//		/// </summary>
//		/// <param name="userid">Userid.</param>
//		/// <param name="delegates">Delegates for success or failure.</param>
//		public void EmailVerification(string userid, KinveyDelegate<User> delegates)
//		{
//			Task.Run (() => {
//				try{
//					User user = base.EmailVerificationBlocking(userid).Execute();
//					delegates.onSuccess(user);
//				}catch(Exception e){
//					delegates.onError(e);
//				}
//			});
//		}

	}
}

