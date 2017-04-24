using Newtonsoft.Json;
using Kinvey;

namespace DemoKLS
{
	[JsonObject(MemberSerialization.OptIn)]
	public class MedicalDeviceCommand
	{
		[JsonProperty("command")]
		public EnumCommand Command { get; set; }

		public enum EnumCommand
		{
			INCREMENT,
			DECREMENT
		}
	}
}
