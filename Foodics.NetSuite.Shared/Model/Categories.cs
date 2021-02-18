namespace Foodics.NetSuite.Shared.Model
{
    public class Categories
    {
        public class FoodicsCategories
        {
            public string id { get; set; }
            public string name { get; set; }
            public int Netsuite_Id { get; set; }
            public int CategoryType { get; set; }//1 for Product,2 for Inventory
            public string Foodics_Id { get; set; }
            public bool InActive { get; set; }
            public int Subsidiary_Id { get; set; }
        }
        public class CategoriesAccounts
        {
            public int Netsuite_Id { get; set; }
            public int cogs_account { get; set; }
            public int inter_cogs_account { get; set; }
            public int income_account { get; set; }
            public int gainloss_account { get; set; }
            public int asset_account { get; set; }
            public int income_intercompany_account { get; set; }
            public int price_variance_account { get; set; }
            public int cust_qty_variance_account { get; set; }

            public int cust_ex_rate_account { get; set; }
            public int customer_vari_account { get; set; }
            public int cust_vendor_account { get; set; }

        }

    }
}
