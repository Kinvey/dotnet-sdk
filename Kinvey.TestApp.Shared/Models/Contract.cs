using Newtonsoft.Json;

namespace Kinvey.Kinvey.TestApp.Shared.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Contract : Entity
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }
    }
}
