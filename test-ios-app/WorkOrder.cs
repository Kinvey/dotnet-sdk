using System;
using Kinvey;

namespace testiosapp
{
	public class WorkOrder
	{
		public WorkOrder ()
		{
		}

		public string Address { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Zip { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public string Ticket { get; set; }
		public DateTime DateCreated { get; set; }
		public DateTime DateDue { get; set; }
		public string Notes { get; set; }
		public string Stage { get; set; }
		public string Tech { get; set; }
		public string Type { get; set; }
		public string Vendor { get; set; }
	}
}
