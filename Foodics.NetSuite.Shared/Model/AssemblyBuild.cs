using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foodics.NetSuite.Shared.Model
{
    public class AssemblyBuild
    {
        public string id { get; }
        public int Netsuite_Id { get; set; }
        public int Location_Id { get; set; }
        public int Subsidiary_Id { get; set; }
        public int Item_Id { get; set; }
        public double Quantity { get; set; }
        public bool InActive { get; set; }

    }
}
