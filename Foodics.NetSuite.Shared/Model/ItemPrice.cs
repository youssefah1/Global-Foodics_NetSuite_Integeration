namespace Foodics.NetSuite.Shared.Model
{
    public class ItemPrice
    {
        public int Item_Id { get; set; }
        public int Currency_Id { get; set; }
        public int Price_Level_Id { get; set; }
        public float Quantity { get; set; }
        public float Price { get; set; }
        public bool InActive { get; set; }
        public string Foodics_Id { get; set; }
    }
}
