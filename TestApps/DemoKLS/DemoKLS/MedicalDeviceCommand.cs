using Newtonsoft.Json;
using Kinvey;

namespace DemoKLS
{
	[JsonObject(MemberSerialization.OptIn)]
	public class MedicalDeviceCommand
	{
		[JsonProperty("command")]
		public string Command { get; set; }
	}
}
