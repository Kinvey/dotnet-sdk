using Newtonsoft.Json;

namespace Kinvey.Tests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ForbiddenErrorEntity : Entity
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
