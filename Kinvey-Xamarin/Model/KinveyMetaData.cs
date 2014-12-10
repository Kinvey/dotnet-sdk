using System;
using Newtonsoft.Json;

namespace KinveyXamarin
{
	/// <summary>
	/// JSON representation of the _kmd field present on every entity stored in Kinvey
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class KinveyMetaData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KinveyMetaData"/> class.
		/// </summary>
		public KinveyMetaData ()
		{
		}

		/// <summary>
		/// The field name within every JSON object.
		/// </summary>
		public const string JSON_FIELD_NAME = "_kmd";

		/// <summary>
		/// Gets or sets the last modified time.
		/// </summary>
		/// <value>The last modified time.</value>
		[JsonProperty("lmt")]
		public String lastModifiedTime{get; set;}

		/// <summary>
		/// Gets or sets the entity creation time.
		/// </summary>
		/// <value>The entity creation time.</value>
		[JsonProperty("ect")]
		public String entityCreationTime{get; set;}
	}
}

