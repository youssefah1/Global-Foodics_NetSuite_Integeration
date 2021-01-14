using System;

namespace Foodics.NetSuite.Shared.Model
{
    public class UnitsOfMeasure
    {
        public int Id { get;  }
        public int Netsuite_Id { get; set; }
        public string Name { get; set; }
        public string abbreviation { get; set; }
        public string pluralAbbreviation { get; set; }
        public string unitName { get; set; }
        public string pluralName { get; set; }
        public int unit_id { get; set; }
        public int details_id { get; set; }
        public float conversionRate { get; set; }
        public bool baseUnit { get; set; }
        public bool InActive { get; set; }
    }
}
