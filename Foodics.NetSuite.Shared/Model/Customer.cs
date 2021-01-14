using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foodics.NetSuite.Shared.Model
{
    public class Customer
    {
        public String Id { get; set; }
        public string name { get; set; }
        public string dial_code { get; set; }
        public string phone { get; set; }
        public string email { get; set; }

        public string Foodics_Id { get; set; }
        public int Subsidiary_Id { get; set; }

        public int gender { get; set; }
        public string birth_date { get; set; }
        public bool is_blacklisted { get; set; }
        public bool InActive { get; set; }

        public DateTime Foodics_UpdateDate { get; set; }
        public int Netsuite_Id { get; set; }

    }
}