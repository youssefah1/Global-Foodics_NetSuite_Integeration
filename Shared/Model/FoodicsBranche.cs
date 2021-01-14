using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Model
{
    public class FoodicsBranche
    {
        public string id { get; set; }
        public string name { get; set; }
        public string name_localized { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string phone { get; set; }
        public string opening_from { get; set; }
        public string opening_to { get; set; }

        public string inventory_end_of_day_time { get; set; }
        public string reference { get; set; }
        
        public Boolean receives_online_orders { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime deleted_at { get; set; }

    }
}
