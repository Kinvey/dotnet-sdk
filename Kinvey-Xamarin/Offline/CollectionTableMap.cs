using System;
using SQLite.Net.Attributes;

namespace KinveyXamarin
{
	public class CollectionTableMap
	{
		[PrimaryKey]
		public string CollectionName { get; set; }

		public string TableName { get; set; }

		public CollectionTableMap () { }
	}
}
