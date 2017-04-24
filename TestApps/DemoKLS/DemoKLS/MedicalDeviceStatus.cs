using Newtonsoft.Json;
using Kinvey;

namespace DemoKLS
{
	[JsonObject(MemberSerialization.OptIn)]
	public class MedicalDeviceStatus
	{
		[JsonProperty("setting")]
		public string Setting { get; set; }
	}
}
