using System;

namespace Foodics.NetSuite.Shared.Model
{
    public class PaymentMethodEntity
    {

        public int Id { get; }
        public int Netsuite_Id { get; set; }
        public string Payment_Method { get; set; }
        public int Payment_Method_Type { get; set; }
        public int Payment_Method_Type_Netsuite_Id { get; set; }
        public int Payment_Method_Id { get; set; }
        public float Payment_Method_Percentage { get; set; }
        
        public float Amount { get; set; }
        public string Ref { get; set; }
        public string Notes { get; set; }

        public DateTime Business_Date { get; set; }

        public string Foodics_Id { get; set; }

        public int Entity_Id { get; set; }
        public int Entity_Type { get; set; } // Invoice_Id Or SalesOrder_Id
        public int Entity_Netsuite_Id { get; set; } // Invoice_Netsuite_Id Or SalesOrder_Netsuite_Id



        public bool InActive { get; set; }

    }
}
