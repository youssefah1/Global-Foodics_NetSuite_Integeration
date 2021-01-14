namespace Foodics.NetSuite.Shared.Model
{
    public class CreditMemo
    {
        public class UI
        {
        }
        public class Integrate
        {
            public int Id { get; }
            public int Netsuite_Id { get; set; }
            public int Return_Netsuite_Id { get; set; }
            public int Return_Id { get; set; }
            public int Return_Type { get; set; }
            public float Amount { get; set; }
            public int Currency_Id { get; set; }
            public float ExchangeRate { get; set; }
            public bool isApplied { get; set; }
        }
    }
}
