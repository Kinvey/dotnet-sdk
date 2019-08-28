using Newtonsoft.Json;

namespace Kinvey.Tests
{
    public class ConflictErrorEntity : Entity
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
