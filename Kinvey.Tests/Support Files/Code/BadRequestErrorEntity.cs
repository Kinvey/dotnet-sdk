using Newtonsoft.Json;

namespace Kinvey.Tests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class BadRequestErrorEntity : Entity
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
