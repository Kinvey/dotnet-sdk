using Newtonsoft.Json;
using Kinvey;
using System.Runtime.Serialization;

namespace TestFramework
{
	[JsonObject(MemberSerialization.OptIn)]
    [DataContract]
	public class AddressEntity : Entity
	{
		[JsonProperty]
        [DataMember]
		public bool IsApartment { get; set; }

		[JsonProperty]
        [DataMember]
        public string Street { get; set; }
	}
}
