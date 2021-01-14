using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Model
{
    public class FoodicsPaymentMethod
    {
        public string id { get; set; }
		public string name { get; set; }
		public string name_localized { get; set; }
		public int type { get; set; }
		public string code { get; set; }
		public bool auto_open_drawer { get; set; }
		public bool is_active { get; set; }
		public DateTime created_at { get; set; }
		public DateTime updated_at { get; set; }
		public DateTime deleted_at { get; set; }

	}
}
