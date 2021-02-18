using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foodics.NetSuite.Shared.Model
{
    public class FoodicsInventoryItem
    {

        public string name { get; set; }
        public string name_localized { get; set; }
        public string sku { get; set; }
        public string barcode { get; set; }
        public string cost { get; set; }
        public string minimum_level { get; set; }
        public string maximum_level { get; set; }
        public string par_level { get; set; }
        public string storage_unit { get; set; }
        public string ingredient_unit { get; set; }
        public string storage_to_ingredient_factor { get; set; }
        public string costing_method { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string deleted_at { get; set; }
        public bool is_product { get; set; }
        public string Integeration_Error { get; set; }

        public string id { get; set; }
        public category category { get; set; }



    }
    //public class category
    //{

    //    public string name { get; set; }
    //    public string id { get; set; }
    //}
    }
