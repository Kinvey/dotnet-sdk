using Newtonsoft.Json;
using Kinvey;

namespace Realtime
{
	[JsonObject(MemberSerialization.OptIn)]
	public class MedicalDeviceCommand : IStreamable
	{
		[JsonProperty("SenderID")]
		public string SenderID { get; set; }

		[JsonProperty("command")]
		public string Command { get; set; }
	}
}
