using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foodics.NetSuite.Shared.Model
{
    public class FoodicsDiscount
	{
        public string id { get; set; }
		public string name { get; set; }
		public string name_localized { get; set; }
		public int qualification { get; set; }
		public float amount { get; set; }
		public bool is_percentage { get; set; }
		public bool is_taxable { get; set; }
		public DateTime created_at { get; set; }
		public DateTime updated_at { get; set; }
		public DateTime deleted_at { get; set; }

	}
}
