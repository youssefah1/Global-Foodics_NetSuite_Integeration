using System;

namespace Shared.Model
{
    public class Inventory
    {
        public int Item_Id { get; set; }
        public int Location_Id { get; set; }
        public double QuantityAvailable { get; set; }
        public double QuantityCommitted { get; set; }
        public double QuantityOnOrder { get; set; }
        public double QuantityOnHand { get; set; }
        public bool InActive { get; set; }

        public string SerialNumbers { get; set; }
        public string Foodics_Id { get; set; }
    }
}
