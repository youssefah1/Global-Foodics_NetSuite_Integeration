using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foodics.NetSuite.Shared.Model
{
    public class ItemCompnent
    {
       
        public int ItemId { get; set; }
        public int ComponentId { get; set; }
        public double Quantity { get; set; }
        public int UnitId { get; set; }

        public string UnitName { get; set; }

        public string ComponentFoodics_Id { get; set; }
        public string ItemFoodics_Id { get; set; }




    }
}
