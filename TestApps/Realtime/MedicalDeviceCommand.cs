using Newtonsoft.Json;
using Kinvey;

namespace Realtime
{
	[JsonObject(MemberSerialization.OptIn)]
	public class MedicalDeviceCommand
	{
		[JsonProperty("command")]
		public string Command { get; set; }
	}
}
