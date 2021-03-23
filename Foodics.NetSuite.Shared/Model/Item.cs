using System;

namespace Foodics.NetSuite.Shared.Model
{
    public class Item
    {
		public int Id { get; }
		public int Netsuite_Id { get; set; }
		public int Parent_Netsuite_Id { get; set; }
		public int Item_Type { get; set; }
		public string Item_Type_Name { get; set; }
		public string FoodicsCategory_Id { get; set; }
		public int Category_Id { get; set; }
		public string Name_En { get; set; }
		public string Name_Ar { get; set; }
		public string Display_Name_En { get; set; }
		public string Display_Name_Ar { get; set; }
		public int Img_Id { get; set; }
		public string Img { get; set; }
		public int Color_Id { get; set; }
		public string UPC_Code { get; set; }
		public string Unit_Type { get; set; }
		public string Storage_Unit { get; set; }
		public string Ingredient_Unit { get; set; }
		public string storage_to_ingredient_factor { get; set; }
		public int Sales_Unit { get; set; }
		public int Subsidiary_Id { get; set; }
		public string Base_Unit { get; set; }
		public int Department_Id { get; set; }
		public int Class_Id { get; set; }
		public int Location_Id { get; set; }
		public int Default_Location_Id { get; set; }
		public string Sales_Description { get; set; }
		public bool IsSpecial { get; set; }
		public bool Available_For_Pos { get; set; }
		public bool Show_Shortcut { get; set; }
		public string Matrix_Type { get; set; }
		public bool InActive { get; set; }
		//public DateTime UpdateDate { get; set; }
		//public DateTime CreateDate { get; set; }
		public int Days_Before_Expiration { get; set; }
		public string Foodics_Id { get; set; }
		public DateTime FoodicsUpdateDate { get; set; }
		
		public Double Price { get; set; }

	}
}
