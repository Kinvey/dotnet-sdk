using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KinveyXamarin
{
	/// <summary>
	/// Kinvey user metadata.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class KinveyUserMetaData : JObject
	{
		public KinveyUserMetaData()
		{
			this.EmailVerification = new KMDEmailVerification();
			this.PasswordReset = new KMDPasswordReset();
		}

		[Preserve]
		[JsonProperty("authtoken")]
		public string AuthToken { get; set; }

		[Preserve]
		[JsonProperty("lmt")]
		public string LastModifiedTime { get; set; }

		/// <summary>
		/// Gets or sets the entity creation time.
		/// </summary>
		[Preserve]
		[JsonProperty("ect")]
		public String EntityCreationTime { get; set; }

		/// <summary>
		/// Gets or sets the email verification information for a user.
		/// </summary>
		[Preserve]
		[JsonProperty("emailVerification")]
		public KMDEmailVerification EmailVerification { get; set; }

		/// <summary>
		/// Gets or sets the password reset information for a user.
		/// </summary>
		[Preserve]
		[JsonProperty("passwordReset")]
		public KMDPasswordReset PasswordReset { get; set; }
	}
}
