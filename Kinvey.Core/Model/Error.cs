using Newtonsoft.Json;

namespace Kinvey
{
    /// <summary>
	/// Represents JSON object with information about an error. 
	/// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Error
    {
        /// <summary>
		/// An index of an entity in the collection <see cref="Kinvey.KinveyDataStoreResponse{T}.Entities"/>.
		/// </summary>
        [JsonProperty]
        public int Index { get; set; }

        /// <summary>
        /// Error code./>
        /// </summary>
        [JsonProperty]
        public int Code { get; set; }

        /// <summary>
        /// Error message./>
        /// </summary>
        [JsonProperty]
        public string Errmsg { get; set; }
    }
}
