using Newtonsoft.Json;
using SQLite.Net.Attributes;
namespace KinveyXamarin
{
	/// <summary>
	/// Base class for model objects backed by Kinvey.  Implements the
	/// <see cref="IPersistable"/> interface
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class Entity : IPersistable
	{
		/// <summary>
		/// Gets or sets the Kinvey ID.
		/// </summary>
		/// <value>The identifier.</value>
		[JsonProperty ("_id")]
		[Preserve]
		[PrimaryKey, Column("_id")]
		public string ID { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="AccessControlList"/> for this Kinvey-backed object.
		/// </summary>
		/// <value>The acl.</value>
		[JsonProperty ("_acl")]
		[Preserve]
		[Column ("_acl")]
		public AccessControlList ACL { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="KinveyMetaData"/> for this Kinvey-backed object.
		/// </summary>
		/// <value>The kmd.</value>
		[JsonProperty ("_kmd")]
		[Preserve]
		[Column("_kmd")]
		public KinveyMetaData KMD { get; set; }
	}
}
