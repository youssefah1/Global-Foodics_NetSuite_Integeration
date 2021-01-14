using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Model
{
    public class FoodicsGiftCardProduct
    {
        public string id { get; set; }
		public string name { get; set; }
		public string name_localized { get; set; }
		public string sku { get; set; }
		public string barcode { get; set; }
		public string image { get;set;}
	public bool is_active { get; set; }
	public int pricing_method { get; set; }
	public decimal price { get; set; }
	public DateTime created_at { get; set; }
	public DateTime updated_at { get; set; }
	public DateTime deleted_at { get; set; }
	

}
}
