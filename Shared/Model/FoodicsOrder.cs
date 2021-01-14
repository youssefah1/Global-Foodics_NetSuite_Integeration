using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Model
{
    public class FoodicsOrder
	{
        public string id { get; set; }
		public string promotion_id { get; set; }
		public int discount_type { get; set; }
		public string reference { get; set; }
		public int number { get; set; }
		public int type { get; set; }
		public int source { get; set; }
		public int status { get; set; }
		public int delivery_status { get; set; }
		public string guests { get; set; }
		public string kitchen_notes { get; set; }
		public string customer_notes { get; set; }
		public DateTime business_date { get; set; }
		public decimal subtotal_price { get; set; }
		public decimal discount_amount { get; set; }
		public decimal rounding_amount { get; set; }
		public decimal total_price { get; set; }
		public decimal tax_exclusive_discount_amount { get; set; }
		public int delay_in_seconds { get; set; }
		public DateTime opened_at { get; set; }
		public DateTime accepted_at { get; set; }
		public DateTime due_at { get; set; }
		public DateTime driver_assigned_at { get; set; }
		public DateTime dispatched_at { get; set; }
		public DateTime driver_collected_at { get; set; }
		public DateTime delivered_at { get; set; }
		public DateTime created_at { get; set; }
		public DateTime updated_at { get; set; }
		public DateTime deleted_at { get; set; }

	}
}
