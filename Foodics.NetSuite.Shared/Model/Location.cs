using System;

namespace Foodics.NetSuite.Shared.Model
{
    public class Location
	{

		public int Id { get; set; }
		public int Netsuite_Id { get; set; }
		public int Parent_Netsuite_Id { get; set; }
		public string Name_En { get; set; }
		public string Name_Ar { get; set; }
		public string Address_En { get; set; }
		public string Address_Ar { get; set; }
		public string Phone { get; set; }
		public string Latitude { get; set; }
		public string Longitude { get; set; }
		public int Subsidiary_Id { get; set; }
		public string Type { get; set; }
		public string Receipt_Header { get; set; }
		public string Receipt_Footer { get; set; }
		public bool InActive { get; set; }
		public string Foodics_Id { get; set; }


	}
}
