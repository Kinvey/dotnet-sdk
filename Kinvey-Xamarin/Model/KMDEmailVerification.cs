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
	public class KMDEmailVerification : ISerializable<string>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KMDEmailVerification"/> class.
		/// </summary>
		[Preserve]
		public KMDEmailVerification()
		{
		}

		/// <summary>
		/// The field name within every JSON object.
		/// </summary>
		public const string JSON_FIELD_NAME = "emailVerification";

		/// <summary>
		/// Gets or sets the status of email verification for the user.
		/// </summary>
		[Preserve]
		[JsonProperty("status")]
		public String Status { get; set; }

		/// <summary>
		/// Gets or sets the last time when the state of email verification changed.
		/// </summary>
		[Preserve]
		[JsonProperty("lastStateChangeAt")]
		public String LastStateChangeAt { get; set; }

		/// <summary>
		/// Gets or sets the last time when email verification was confirmed.
		/// </summary>
		[Preserve]
		[JsonProperty("lastConfirmedAt")]
		public String LastConfirmedAt { get; set; }

		/// <summary>
		/// Gets or sets the email address of the user used for email verification.
		/// </summary>
		[Preserve]
		[JsonProperty("emailAddress")]
		public String EmailAddress { get; set; }

		/// <summary>
		/// Serialize this instance of <see cref="KinveyXamarin.KMDEmailVerification"/> in the local cache.
		/// </summary>
		public string Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
