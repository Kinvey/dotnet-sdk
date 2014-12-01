// Copyright (c) 2014, Kinvey, Inc. All rights reserved.
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinveyXamarin
{
	/// <summary>
	/// This Client User implementation is done in memory, and will not persist between application executions.
	/// </summary>
    public class InMemoryClientUsers : IClientUsers
    {
		/// <summary>
		/// The user list, backing this implementation.
		/// </summary>
        private Dictionary<string, string> userList;
		/// <summary>
		/// The active user.
		/// </summary>
        private string activeUser;
		/// <summary>
		/// This is a singleton so this is the instance.
		/// </summary>
        private static InMemoryClientUsers _instance;

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.InMemoryClientUsers"/> class.
		/// </summary>
        private InMemoryClientUsers()
        {
            if (userList == null)
            {
                userList = new Dictionary<string, string>();
            }
            if (activeUser == null)
            {
                activeUser = "";
            }
        }

		/// <summary>
		/// Gets or sets the current user's _id.
		/// </summary>
		/// <value>The current user's _id.</value>
        public string CurrentUser
        {
            get { return activeUser; }
            set { this.activeUser = value; }
        }

		/// <summary>
		/// Accessor for this singleton.
		/// </summary>
		/// <returns>The instance of the singleto</returns>
        public static InMemoryClientUsers GetClientUsers()
        {
            if (_instance == null)
            {
                _instance = new InMemoryClientUsers();
            }
            return _instance;
        }

		/// <summary>
		/// Add a new user to this in memory client user store.
		/// </summary>
		/// <param name="userID">User._id.</param>
		/// <param name="type">Type.</param>
		/// <param name="userId">User identifier.</param>
        public void AddUser(string userId, string type)
        {
            userList.Add(userId, type);
        }

		/// <summary>
		/// Removes the user from this user store.
		/// </summary>
		/// <param name="userID">User._id.</param>
		/// <param name="userId">User identifier.</param>
        public void RemoveUser(string userId)
        {
            if (userId == CurrentUser)
            {
                CurrentUser = null;
            }
            userList.Remove(userId);
        }

		/// <summary>
		/// Unloads the current user and loads the new one
		/// </summary>
		/// <param name="userID">User._id</param>
		/// <param name="userId">User identifier.</param>
        public void SwitchUser(string userId)
        {
            if (userList.ContainsKey(userId))
            {
                CurrentUser = userId;
            }
        }

		/// <summary>
		/// Sets the current user.
		/// </summary>
		/// <param name="userId">User._id.</param>
        public void SetCurrentUser(string userId)
        {
            if (userList.ContainsKey(userId))
            {
                CurrentUser = userId;
            }
        }
		/// <summary>
		/// Gets the login type of the current user.
		/// </summary>
		/// <returns>The current user's login type.</returns>
        public string GetCurrentUserType()
        {
            string userType = userList[activeUser];
            return userType == null ? "" : userType;
        }
    }
}
