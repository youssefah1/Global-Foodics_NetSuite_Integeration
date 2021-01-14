using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foodics.NetSuite.Shared.Model
{
    public class Categories
    {
        public string id { get; set; }
        public string name { get; set; }
        public int Netsuite_Id { get; set; }
        public string Foodics_Id { get; set; }
        public bool InActive { get; set; }
        public int Subsidiary_Id { get; set; }


    }
}
