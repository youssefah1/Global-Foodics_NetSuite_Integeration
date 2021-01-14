﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Model
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




    }
}