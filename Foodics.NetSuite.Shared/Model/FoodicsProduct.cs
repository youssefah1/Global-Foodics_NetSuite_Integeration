using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foodics.NetSuite.Shared.Model
{
    public class FoodicsProduct
    {
        public string name { get; set; }
        public string name_localized { get; set; }
        public string sku { get; set; }
        public string barcode { get; set; }
        public string description { get; set; }
        public string description_localized { get; set; }
        public string image { get; set; }
        public bool is_active { get; set; }
        public bool is_stock_product { get; set; }
        public bool is_ready { get; set; }
        public int pricing_method { get; set; }
        public int selling_method { get; set; }
        public int costing_method { get; set; }
        public int preparation_time { get; set; }
        public decimal price { get; set; }
        public decimal cost { get; set; }
        public int calories { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime deleted_at { get; set; }
        public string id { get; set; }
        public category category { get; set; }
        public tax_group tax_group { get; set; }
        public modifier modifier { get; set; }
        public List<ingredients> ingredients { get; set; }


    }

    public class category
    {
        public string id { get; set; }
        public string name { get; set; }

    }

    public class modifier
    {
        public string id { get; set; }
        public string name { get; set; }

    }
    public class tax_group
    {
        public string id { get; set; }
        public string name { get; set; }

    }
    public class ingredients
    {
        public string id { get; set; }
        public string name { get; set; }
        public string sku { get; set; }

        public string ingredient_unit { get; set; }
        public pivot pivot { get; set; }

    }

    public class pivot
    {
        public float quantity { get; set; }
        public string inactive_in_order_types { get; set; }

    }



}
