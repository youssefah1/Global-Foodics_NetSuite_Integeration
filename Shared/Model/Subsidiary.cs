using System;

namespace Shared.Model
{
    public class Subsidiary
	{

		public int Netsuite_Id { get; set; }
		public int Parent_Netsuite_Id { get; set; }
		public string Name_En { get; set; }
		public string Name_Ar { get; set; }
		public string Country { get; set; }
		public string Currency { get; set; }
		public bool InActive { get; set; }
		public string Foodics_Id { get; set; }

	}
}
