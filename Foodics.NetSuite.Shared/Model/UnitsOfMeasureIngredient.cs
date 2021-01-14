using System;

namespace Foodics.NetSuite.Shared.Model
{
    public class UnitsOfMeasureIngredient
    {
        public int Id { get;  }
        public int UnitsOfMeasure_Id { get; set; }
        public string Storage_To_Ingredient_Value { get; set; }
        public string unitName { get; set; }
        public bool InActive { get; set; }
    }
}
