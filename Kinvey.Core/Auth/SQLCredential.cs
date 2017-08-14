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

using SQLite.Net.Attributes;

/// <summary>
/// SQL credential.
/// </summary>
public class SQLCredential
{
	/// <summary>
	/// Gets or sets the Kinvey social identity info for this credential.
	/// </summary>
	/// <value>The credential Kinvey metadata.</value>
	public string AuthSocialID { get; set; }

	/// <summary>
	/// Gets or sets the access token.
	/// </summary>
	/// <value>The access token.</value>
	public string AccessToken { get; set; }

	/// <summary>
	/// Gets or sets the auth token.
	/// </summary>
	/// <value>The auth token.</value>
	public string AuthToken { get; set; }

	/// <summary>
	/// Gets or sets the auth token.
	/// </summary>
	/// <value>The auth token.</value>
	public byte[] SecAuthToken { get; set; }

	/// <summary>
	/// Gets or sets the user ID.
	/// </summary>
	/// <value>The user Id.</value>
	[PrimaryKey]
	public string UserID { get; set; }

	/// <summary>
	/// Gets or sets the user name.
	/// </summary>
	/// <value>The redirect uri.</value>
	public string UserName { get; set; }

	/// <summary>
	/// Gets or sets the refresh token.
	/// </summary>
	/// <value>The refresh token.</value>
	public string RefreshToken { get; set; }

	/// <summary>
	/// Gets or sets the redirect uri.
	/// </summary>
	/// <value>The redirect uri.</value>
	public string RedirectUri { get; set; }

	/// <summary>
	/// Gets or sets the Kinvey metadata for this credential.
	/// </summary>
	/// <value>The credential Kinvey metadata.</value>
	public string UserKMD { get; set; }

	/// <summary>
	/// Gets or sets the custom attributes for the user.
	/// </summary>
	/// <value>The attributes dictionary</value>
	public string Attributes { get; set; }

	/// <summary>
	/// Gets or sets the device ID associated with this active user.
	/// </summary>
	/// <value>The attributes dictionary</value>
	public string DeviceID { get; set; }

	/// <summary>
	/// Gets or sets the MIC ID associated with the auth service used.
	/// </summary>
	/// <value>The attributes dictionary</value>
	public string MICClientID { get; set; }
}
