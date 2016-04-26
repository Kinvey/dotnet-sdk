using System;
using SQLite.Net.Attributes;

namespace KinveyXamarin
{
	public class PendingWriteAction
	{

		[PrimaryKey, AutoIncrement]
		public int key { get; set; }

		public string OfflineMetaDataAsJson { get; set; }

		public string collection { get; set; }

		public string action { get; set; }

	}
}

