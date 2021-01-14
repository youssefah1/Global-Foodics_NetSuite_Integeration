namespace Foodics.NetSuite.Shared.Model
{ 
    public class PaymentMethod
    {
		public int Netsuite_Id { get; set; }
		public string Name_En { get; set; }
		public string Name_Ar { get; set; }
		public int Sort_No { get; set; }
		public bool Is_Depit_Card { get; set; }
		public string Method_Type_Name { get; set; }
		public int Method_Type_Id { get; set; }
		public bool Require_Line_Level_Data { get; set; }
		public int Account_Id { get; set; }
		public bool Is_UnDepFunds { get; set; }
		public bool InActive { get; set; }
		public string Foodics_Id { get; set; }

		public float Percentage { get; set; }
	}
}
