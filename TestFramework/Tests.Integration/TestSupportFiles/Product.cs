using Newtonsoft.Json;
using Kinvey;

namespace TestFramework
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Product : Entity
	{
		[JsonProperty("mn")]
		public string MN { get; set; }

		[JsonProperty("desc")]
		public string Desc { get; set; }

		[JsonProperty("MaterialNumber")]
		public string MaterialNumber { get; set; }

		[JsonProperty("Description")]
		public string Description { get; set; }

		[JsonProperty("Vertical")]
		public string Vertical { get; set; }

		[JsonProperty("SubVertical")]
		public string SubVertical { get; set; }

		[JsonProperty("Classification")]
		public string Classification { get; set; }

		[JsonProperty("MaterialGroup")]
		public string MaterialGroup { get; set; }

		[JsonProperty("MaterialFreightGroup")]
		public int MaterialFreightGroup { get; set; }

		[JsonProperty("MaterialType")]
		public string MaterialType { get; set; }

		[JsonProperty("BaseUOM")]
		public string BaseUOM { get; set; }

		[JsonProperty("PackageQuantity")]
		public int PackageQuantity { get; set; }

		[JsonProperty("GSAFlag")]
		public string GSAFlag { get; set; }

		[JsonProperty("DiscontinuedIndicator")]
		public string DiscontinuedIndicator { get; set; }

		[JsonProperty("DeactivateDate")]
		public string DeactivateDate { get; set; }

		[JsonProperty("TAAFlag")]
		public string TAAFlag { get; set; }

		[JsonProperty("MinOrderQuantity")]
		public int MinOrderQuantity { get; set; }

		[JsonProperty("Active")]
		public bool Active { get; set; }

		[JsonProperty("Created")]
		public string Created { get; set; }

		[JsonProperty("Modified")]
		public string Modified { get; set; }
	}
}

