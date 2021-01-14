using System;

namespace Foodics.NetSuite.Shared.Model
{
    public class Setting
    {
        public int Id { get; }
        public int Netsuite_Id { get; set; }
        public string Name { get; set; }

        public int Price_Level_Netsuite_Id { get; set; }
        public int Currency_Netsuite_Id { get; set; }
        public int Customer_Netsuite_Id { get; set; }
        public int Subsidiary_Netsuite_Id { get; set; }

        public int TaxCode_Netsuite_Id { get; set; }

        public int Item_Tmp_Netsuite_Id { get; set; }

        public int LiabilityAccount_Netsuite_Id { get; set; }
        public int AdjustmentAccount_Netsuite_Id { get; set; }
        public int CogsAccount_Netsuite_Id { get; set; }
        public int IntercoIncomeAccount_Netsuite_Id { get; set; }
        public int GainLossAccount_Netsuite_Id { get; set; }
        public int IntercoCogsAccount_Netsuite_Id { get; set; }
        public int AssetAccount_Netsuite_Id { get; set; }
        public int Location_Netsuite_Id { get; set; }
        public int TaxSchedule_Netsuite_Id { get; set; }
        public bool InActive { get; set; }
        public int DiscountAccount_Netsuite_Id { get; set; }

        public float TaxRate { get; set; }
        public class integerate
        {
            public int Netsuite_Id { get; set; }
            public float TaxRate { get; set; }
            public int Currency_Netsuite_Id { get; set; }
            public int Subsidiary_Netsuite_Id { get; set; }
            public int TaxCode_Netsuite_Id { get; set; }
            public int Customer_Netsuite_Id { get; set; }
            public int AdjustmentAccount_Netsuite_Id { get; set; }
            public int CogsAccount_Netsuite_Id { get; set; }
            public int AssetAccount_Netsuite_Id { get; set; }
        }
    }
}
