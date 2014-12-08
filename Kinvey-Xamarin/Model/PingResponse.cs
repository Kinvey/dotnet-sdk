using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace KinveyXamarin
{
	/// <summary>
	/// This class represents the response of a ping request.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class PingResponse
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.PingResponse"/> class.
		/// </summary>
		public PingResponse ()
		{}

		[JsonProperty]
		public string version;

		[JsonProperty]
		public string kinvey;

	}
}

