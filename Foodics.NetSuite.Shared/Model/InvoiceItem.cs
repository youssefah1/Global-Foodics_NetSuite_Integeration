using System;

namespace Foodics.NetSuite.Shared.Model
{
    public class InvoiceItem
    {

        public int Id { get; }
        public int Netsuite_Id { get; set; }
        public int Invoice_Id { get; set; }
        public int Invoice_Netsuite_Id { get; set; }
        public int Item_Id { get; set; }
        public int ProductStatus { get; set; }
        
        public string FoodicsItem_Id { get; set; }
        public string Item_Code { get; set; }
        public string Item_Name { get; set; }
        public string Item_Type { get; set; }
        public float Quantity { get; set; }
        public float FoodicsTax { get; set; }
        public string Units { get; set; }
        public int Price_Level_Id { get; set; }
        public float Amount { get; set; }
        public int Cost_Estimate_Type { get; set; }
        public int Tax_Code { get; set; }
        public float Tax_Rate { get; set; }
        public float Tax_Amt { get; set; }
        public int Line_Discount_Type { get; set; }
        public float Line_Discount_Rate { get; set; }
        public float Line_Discount_Amount { get; set; }
        public float Total_Line_Amount { get; set; }
        public int Sales_Rep_Id { get; set; }
        public string Serials { get; set; }
        public string Notes { get; set; }
        public bool InActive { get; set; }
        public bool Is_Ingredients_Wasted { get; set; }


        public string Foodics_Id { get; set; }
        public string Sender { get; set; }
        public string Recipient_Name { get; set; }
        public string Recipient_Email { get; set; }
        public string Gift_Message { get; set; }
        public DateTime Expiration_Date { get; set; }
        public string Combo_Name { get; set; }
        public string ComboSize_Name { get; set; }


    }
}
