using System;

namespace Foodics.NetSuite.Shared
{
    public enum Source_Type
    {
        Foodics = 1,
        NetSuite = 2,
        
    }

    public enum Item_Type
    {
        InventoryItem = 1, //Item
        AssemblyItem = 2, //Product
        GiftCertificate= 3,//Gift 
        Combo = 4,//Combo
        ServiceSaleItem = 5,
        OtherChargeSaleItem = 6,

    }
    public enum FoodicsItem_Type
    {
        InventoryItem = 1, 
        Product = 2, 
        ModifierOption = 3,


    }
    public enum LogIntegrationType
    {
        Error = 1,
        Info = 2
    }
    public enum InvoiceSource
    {
        Cashier = 1,
        API = 2,
        CallCenter = 3
    }
}
