using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foodics.NetSuite.Shared.Model
{
    public class FoodicsCategories
    {
        public string id { get; set; }
        public string name { get; set; }
        public string name_localized { get; set; }
        
        public string image { get; set; }
        public string reference { get; set; }
        
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime deleted_at { get; set; }

    }
}
