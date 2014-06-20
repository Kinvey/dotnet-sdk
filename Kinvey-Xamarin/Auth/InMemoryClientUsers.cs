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

namespace Kinvey.DotNet.Framework.Auth
{
    public class InMemoryClientUsers : IClientUsers
    {
        private Dictionary<string, string> userList;
        private string activeUser;
        private static InMemoryClientUsers _instance;

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

        public string CurrentUser
        {
            get { return activeUser; }
            set { this.activeUser = value; }
        }

        public static InMemoryClientUsers GetClientUsers()
        {
            if (_instance == null)
            {
                _instance = new InMemoryClientUsers();
            }
            return _instance;
        }

        public void AddUser(string userId, string type)
        {
            userList.Add(userId, type);
        }

        public void RemoveUser(string userId)
        {
            if (userId == CurrentUser)
            {
                CurrentUser = null;
            }
            userList.Remove(userId);
        }

        public void SwitchUser(string userId)
        {
            if (userList.ContainsKey(userId))
            {
                CurrentUser = userId;
            }
        }

        public void SetCurrentUser(string userId)
        {
            if (userList.ContainsKey(userId))
            {
                CurrentUser = userId;
            }
        }

        public string GetCurrentUserType()
        {
            string userType = userList[activeUser];
            return userType == null ? "" : userType;
        }
    }
}
