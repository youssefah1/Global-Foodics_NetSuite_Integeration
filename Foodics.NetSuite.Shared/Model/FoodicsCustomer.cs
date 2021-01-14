using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foodics.NetSuite.Shared.Model
{
	public class FoodicsCustomer
	{
		public string id { get; set; }
		public string name { get; set; }
		public string dial_code { get; set; }
		public string phone { get; set; }
		public string email { get; set; }
		public int gender { get; set; }
		public string birth_date { get; set; }
		public bool is_blacklisted { get; set; }
		public bool is_house_account_enabled { get; set; }
		public int house_account_limit { get; set; }
		public bool is_loyalty_enabled { get; set; }
		public DateTime last_order_at { get; set; }
		public DateTime created_at { get; set; }
		public DateTime updated_at { get; set; }
		public DateTime deleted_at
		{
			get; set;

		}
	}
}