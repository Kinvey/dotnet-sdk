using System;
using Newtonsoft.Json;
using SQLite.Net;

namespace KinveyXamarin
{
	/// <summary>
	/// JSON representation of the emailVerification field present on user
	/// entities stored in Kinvey that have verified through email
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class KMDPasswordReset : ISerializable<string>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KMDPasswordReset"/> class.
		/// </summary>
		[Preserve]
		public KMDPasswordReset()
		{
		}

		/// <summary>
		/// Gets or sets the status of the password reset request for the user.  This field is set 
		/// to "InProgress" during the fulfillment of the request, and is empty when the request is complete.
		/// </summary>
		[Preserve]
		[JsonProperty("status")]
		public String Status { get; set; }

		/// <summary>
		/// Gets or sets the last time when the state of the password reset request changed.  If the status field 
		/// is set to "InProgress", this field reflects when the password reset request was issued.  If the status 
		/// field is empty, this field reflects when the password reset request was fulfilled.
		/// </summary>
		[Preserve]
		[JsonProperty("lastStateChangeAt")]
		public String LastStateChangeAt { get; set; }

		/// <summary>
		/// Serialize this instance of <see cref="KinveyXamarin.KMDPasswordReset"/> in the local cache.
		/// </summary>
		public string Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}

