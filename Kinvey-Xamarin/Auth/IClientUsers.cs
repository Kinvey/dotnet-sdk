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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinveyXamarin
{
	/// <summary>
	/// This interface defines the behaiovor of a kinvey client user.
	/// </summary>
    public interface IClientUsers
    {
		/// <summary>
		/// Adds the user.
		/// </summary>
		/// <param name="userID">User._id.</param>
		/// <param name="type">Type.</param>
        void AddUser(string userID, string type);

		/// <summary>
		/// Removes the user.
		/// </summary>
		/// <param name="userID">User._id.</param>
        void RemoveUser(string userID);

		/// <summary>
		/// Unloads the current user and loads the new one
		/// </summary>
		/// <param name="userID">User._id</param>
        void SwitchUser(string userID);

		/// <summary>
		/// Gets or sets the current user's _id.
		/// </summary>
		/// <value>The current user's _id.</value>
        string CurrentUser { get; set; }

		/// <summary>
		/// Gets the login type of the current user.
		/// </summary>
		/// <returns>The current user's login type.</returns>
        string GetCurrentUserType();
    }
}
