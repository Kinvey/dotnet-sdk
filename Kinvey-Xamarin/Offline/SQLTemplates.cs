using System;
using System.Collections.Generic;
using SQLite.Net.Attributes; 

namespace KinveyXamarin
{
	/// <summary>
	/// These are the structures stored in the database, one for each table
	/// </summary>
	public class SQLTemplates{

		public class TableItem{
			public string name { get; set;}
		}

		public class OfflineEntity{
//			[PrimaryKey, AutoIncrement]
//			public int key { get; set; }
			[PrimaryKey]
			public string id { get; set;}
			public string json { get; set;}
			public string collection { get; set; }
		}

		public class QueueItem{
			[PrimaryKey, AutoIncrement] 
			public int key { get; set; } 
			public string id { get; set; }
			public string collection { get; set; }
			public string action { get; set; }

		}

		public class QueryItem{
			[PrimaryKey, AutoIncrement] 
			public int key { get; set; } 
			public string query { get; set; }
			public string commaDelimitedIds { get; set; }
			public string collection { get; set; }
				
		}
	}
}

