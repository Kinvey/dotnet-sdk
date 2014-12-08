using System;
using Newtonsoft.Json;

namespace KinveyXamarin
{
	/// <summary>
	/// This class represents the response sent from Kinvey after a delete has been executed.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class KinveyDeleteResponse
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KinveyDeleteResponse"/> class.
		/// </summary>
		public KinveyDeleteResponse ()
		{}
		/// <summary>
		/// Gets or sets the count of entities deleted.
		/// </summary>
		/// <value>The count.</value>
		[JsonProperty]
		public int count{get; set;}
	}
}

