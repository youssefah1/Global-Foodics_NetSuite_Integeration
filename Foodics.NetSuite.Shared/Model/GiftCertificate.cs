using System;

namespace Foodics.NetSuite.Shared.Model
{
    public class GiftCertificate
    {
        public class UI
        {
            public int Object_Id { get; set; }
            public float Amount { get; set; }
            public DateTime Expiration_Date { get; set; }
        }
        public class Integrate
        {
            public int Netsuite_Id { get; set; }
            public float Amount { get; set; }
            public float Remaining { get; set; }
            public string Gift_Code { get; set; }
            public bool Is_Sold { get; set; }
            
        }
    }
}
