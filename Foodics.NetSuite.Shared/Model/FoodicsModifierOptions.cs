using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foodics.NetSuite.Shared.Model
{
    public class FoodicsModifierOptions
    {
        public string id { get; set; }
        public string name { get; set; }
        public string name_localized { get; set; }
        public string sku { get; set; }
        public bool is_active { get; set; }
        public int costing_method { get; set; }
        public int preparation_time { get; set; }
        public decimal price { get; set; }
        public decimal cost { get; set; }
        public int calories { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime deleted_at { get; set; }
        public tax_group tax_group { get; set; }



    }
     



}
