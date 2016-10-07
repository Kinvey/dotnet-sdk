// Copyright (c) 2016, Kinvey, Inc. All rights reserved.
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
using System.Text;

namespace KinveyXamarin
{
	/// <summary>
	/// Native credential.
	/// </summary>
	public class NativeCredential
	{
		private const string USERID = "__userID__";

		/// <summary>
		/// Gets or sets the credential user ID, corresponding to the userID of the user object this credential represents.
		/// </summary>
		/// <value>The user ID associated with this credential.</value>
		public virtual string UserID { get; set; }

		/// <summary>
		/// Gets the properties.
		/// </summary>
		/// <value>The properties.</value>
		public virtual Dictionary<string, string> Properties { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NativeCredential"/> class.
		/// </summary>
		public NativeCredential()
			: this(string.Empty, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NativeCredential"/> class.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="properties">Properties.</param>
		public NativeCredential(string username, Dictionary<string, string> properties)
		{
			UserID = username;
			Properties = (properties == null) ? new Dictionary<string, string>() : new Dictionary<string, string>(properties);
		}

		/// <summary>
		/// Serialize this instance.
		/// </summary>
		public string Serialize()
		{
			var sb = new StringBuilder();

			sb.Append(NativeCredential.USERID);
			sb.Append(Constants.STR_EQUAL);
			sb.Append(Uri.EscapeDataString(UserID));

			foreach (var p in Properties)
			{
				sb.Append(Constants.STR_AMPERSAND);
				sb.Append(Uri.EscapeDataString(p.Key));
				sb.Append(Constants.STR_EQUAL);
				sb.Append(Uri.EscapeDataString(p.Value));
			}

			return sb.ToString();
		}

		/// <summary>
		/// Deserialize the specified serializedNativeCredential.
		/// </summary>
		/// <param name="serializedNativeCredential">Serialized native credential.</param>
		static public NativeCredential Deserialize(string serializedNativeCredential)
		{
			var nativeCredential = new NativeCredential();

			foreach (var p in serializedNativeCredential.Split(Constants.CHAR_AMPERSAND))
			{
				var keyval = p.Split(Constants.CHAR_EQUAL);

				var key = Uri.UnescapeDataString(keyval[0]);
				var val = keyval.Length > 1 ? Uri.UnescapeDataString(keyval[1]) : String.Empty;

				if (key == NativeCredential.USERID)
				{
					nativeCredential.UserID = val;
				}
				else
				{
					if (val.Equals(String.Empty))
					{
						val = null;
					}

					nativeCredential.Properties[key] = val;
				}
			}

			return nativeCredential;
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:NativeCredential"/>.
		/// </summary>
		/// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:NativeCredential"/>.</returns>
		public override string ToString()
		{
			return Serialize();
		}
	}
}
