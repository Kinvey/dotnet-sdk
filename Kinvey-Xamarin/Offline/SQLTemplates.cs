using System;
using SQLite.Net.Attributes; 

namespace KinveyXamarin
{
	public class SQLTemp{

		public class TableItem{
			public string name { get; set;}
		}

		public class OfflineEntity{
			public string id { get; set;}
			public string json { get; set;}
			public string user { get; set;}
		}

		public class QueueItem{
			[PrimaryKey, AutoIncrement] 
			public string key { get; set; } 
			public string id { get; set; }
			public string action { get; set; }

		}

		public class QueryItem{
			public string query { get; set; }
			public string ids { get; set; }
				
		}
	}


}

