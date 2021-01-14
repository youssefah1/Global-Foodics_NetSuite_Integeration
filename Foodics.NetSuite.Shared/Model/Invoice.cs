using System;
using System.Collections.Generic;

namespace Foodics.NetSuite.Shared.Model
{
    public class Invoice
    {


        public int Id { get; }
        public int Netsuite_Id { get; set; }
        public int Customer_Netsuite_Id { get; set; }
        public int Customer_Payment_Netsuite_Id { get; set; }
        public int Customer_Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime AsOfDate { get; set; }

        public string CreatedBy { get; set; }
        public string Number { get; set; }
        public string Source { get; set; }
        public string Interval_Note { get; set; }
        public int Currency_Id { get; set; }
        public float Exchange_Rate { get; set; }
        public int Posting_Period_Id { get; set; }
        public int Subsidiary_Id { get; set; }
        public int Location_Id { get; set; }
        public int Terminal_Id { get; set; }
        public int Cashier { get; set; }
        public int Discount_Id { get; set; }
        public int Order_Status { get; set; }//4 closed,5 Returned
        public float Tax { get; set; }
        public float Net_Payable { get; set; }
        public float Paid { get; set; }
        public float Balance { get; set; }
        public string BarCode { get; set; }

        public string Foodics_Id { get; set; }
        public string Original_Foodics_Id { get; set; }

        public int Invoice_Discount_Type { get; set; }
        public float Invoice_Discount_Rate { get; set; }
        public float Total_Discount { get; set; }
        public int Accounting_Discount_Item { get; set; }
        public int Sales_Rep_Id { get; set; }


        public string Notes { get; set; }



        public bool InActive { get; set; }

    }
}
