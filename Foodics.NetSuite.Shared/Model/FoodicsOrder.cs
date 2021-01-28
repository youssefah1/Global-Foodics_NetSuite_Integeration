using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foodics.NetSuite.Shared.Model
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
		public List<Products> Products { get; set; }
		public int status { get; set; }//4 closed,5 Returned
		public List<payments> payments { get; set; }
		public original_order original_order { get; set; }
		public Customer Customer { get; set; }
		public Branch Branch { get; set; }
		public discount discount { get; set; }
		public creator creator { get; set; }

	}


	public class Products
	{
		public Product Product { get; set; }
		public decimal unit_price { get; set; }
		public int quantity { get; set; }
		public int discount_type { get; set; }
		public int returned_quantity { get; set; }
		public decimal discount_amount { get; set; }
		public int total_price { get; set; }
		public int total_cost { get; set; }
		public decimal tax_exclusive_discount_amount { get; set; }
		public int tax_exclusive_unit_price { get; set; }
		public int tax_exclusive_total_price { get; set; }
		public int status { get; set; }
		public string kitchen_notes { get; set; }
		public bool is_ingredients_wasted { get; set; }
		public List<options> options { get; set; }

	}

	public class Product
	{

		public string id { get; set; }
		public string sku { get; set; }
	}

	public class payments
	{
		public payment_method payment_method { get; set; }
		public decimal amount { get; set; }
		public decimal tendered { get; set; }
		public decimal tips { get; set; }
		public DateTime Business_Date { get; set; }

	}

	public class payment_method
	{

		public string id { get; set; }
		public string name { get; set; }

	}

	public class Branch
	{

		public string id { get; set; }
		public string name { get; set; }
	}

	public class creator
	{

		public string id { get; set; }
		public string name { get; set; }
	}
	public class discount
	{

		public string id { get; set; }
		public string name { get; set; }
	}
	public class original_order
	{

		public string id { get; set; }
	}
	public class options
	{

		public int quantity { get; set; }
		public decimal unit_price { get; set; }
		public decimal total_price { get; set; }
		public modifier_option modifier_option { get; set; }
	}
	public class modifier_option
	{

		public string id { get; set; }
		public string name { get; set; }
		public string sku { get; set; }
		
	}

}
