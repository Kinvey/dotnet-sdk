using Newtonsoft.Json;

namespace Kinvey.Tests
{
    public class NotFoundErrorEntity : Entity
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
