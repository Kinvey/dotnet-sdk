using System;
using System.Collections.Generic;
using SQLite.Net.Attributes; 

namespace KinveyXamarin
{
	/// <summary>
	/// These are the structures stored in the database, one for each table
	/// </summary>
	public class SQLTemplates{

		/// <summary>
		/// This maintains the collection names
		/// </summary>
		public class TableItem{
			public string name { get; set;}
		}
			
		/// <summary>
		/// This maintains the entities themselves.
		/// </summary>
		public class OfflineEntity{
			[PrimaryKey]
			public string id { get; set;}
			public string json { get; set;}
			public string collection { get; set; }
		}

		/// <summary>
		/// This maintains a queue of pending requests.
		/// </summary>
		public class QueueItem{
			[PrimaryKey, AutoIncrement] 
			public int key { get; set; } 
			public string id { get; set; }
			public string collection { get; set; }
			public string action { get; set; }

		}

		/// <summary>
		/// This maintains a query and it's responses.
		/// </summary>
		public class QueryItem{
			[PrimaryKey, AutoIncrement] 
			public int key { get; set; } 
			public string query { get; set; }
			public string commaDelimitedIds { get; set; }
			public string collection { get; set; }
				
		}
	}
}

