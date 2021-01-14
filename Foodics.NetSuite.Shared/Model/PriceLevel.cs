namespace Foodics.NetSuite.Shared.Model
{ 
    public class PriceLevel
    {
        public int Netsuite_Id { get; set; }
        public string Name { get; set; }
        public float Discount_Percentage { get; set; }
        public bool InActive { get; set; }
    }
}
