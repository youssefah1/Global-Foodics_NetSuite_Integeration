namespace Foodics.NetSuite.Shared.Model
{ 
    public class Discount
	{
		public int Netsuite_Id { get; set; }
		public string Name_En { get; set; }
		public string Name_Ar { get; set; }
		public int Qualification { get; set; }
		public float Amount { get; set; }
		public bool IsPercentage { get; set; }
		public bool IsTaxable { get; set; }
		public bool InActive { get; set; }
		public string Foodics_Id { get; set; }
		public int Subsidiary_Id { get; set; }
	}
}
