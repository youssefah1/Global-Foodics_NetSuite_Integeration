using Foodics.NetSuite.Shared;
using Foodics.NetSuite.Shared.DAO;
using Foodics.NetSuite.Shared.Model;
using Microsoft.Build.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;

namespace FoodicsIntegeration.Tasks
{
    public class FoodicsInventoryItem_Task : Foodics_BaseIntegration
    {
        public override void Get(string Subsidiary)
        {
            string MainURL = ConfigurationManager.AppSettings[Subsidiary+"Foodics.ResetURL"] + "inventory_items?include=category&filter[updated_after]=2021-01-01";
            string NextPage = MainURL;
            do
            {
                var client = new RestClient(NextPage);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                request.AddHeader("Authorization", "Bearer " + ConfigurationManager.AppSettings[Subsidiary+"Foodics.Token"]);
                List<FoodicsInventoryItem> lstitems = new List<FoodicsInventoryItem>();
                var response = client.Execute<Dictionary<string, List<FoodicsInventoryItem>>>(request);
                if (response.StatusCode == HttpStatusCode.OK && response.Data != null)
                {
                    string content = response.Content;
                    var Jobj = JObject.Parse(content);
                    var nodes = Jobj.Descendants()
                   .OfType<JProperty>()
                   .Where(p => p.Name == "next")
                   .Select(p => p.Parent)
                   .ToList();
                    if (nodes.Count > 0)
                    {
                        FoodicsLinks objLinks = new FoodicsLinks();
                        try
                        {
                            objLinks = JsonConvert.DeserializeObject<FoodicsLinks>(JsonConvert.SerializeObject(nodes[0]));
                            NextPage = objLinks.next;
                            if (!string.IsNullOrEmpty(NextPage))
                            {
                                int startIndex = NextPage.LastIndexOf("?") + 1;
                                int endIndex = NextPage.Length - startIndex;
                                string page = NextPage.Substring(startIndex, endIndex);
                                NextPage = MainURL + "&" + page;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
                        }
                    }
                    foreach (var item in response.Data)
                    {
                        if (item.Key == "data")
                        {
                            if (item.Value.Count > 0)
                            {
                                Generate_Save_NetSuiteLst(item.Value, Subsidiary);
                            }
                        }
                    }
                }

            } while (!string.IsNullOrEmpty(NextPage));
        }

        private void Generate_Save_NetSuiteLst(List<FoodicsInventoryItem> lstitems,string Subsidiary)
        {
            try
            {
                List<Item> NetSuitelst = new List<Item>();
                //List<Inventory> NetSuiteInventorylst = new List<Inventory>();
                //List<ItemPrice> NetSuiteItemPricelst = new List<ItemPrice>();

                foreach (var Foodicsitem in lstitems)
                {

                    Item Netsuiteitem = new Item();
                    Netsuiteitem.Foodics_Id = Foodicsitem.id;
                    Netsuiteitem.Item_Type = (int)Item_Type.InventoryItem;
                    Netsuiteitem.Item_Type_Name = nameof(Item_Type.InventoryItem);
                    Netsuiteitem.Foodics_Item_Type_Name = nameof(FoodicsItem_Type.InventoryItem);
                    Netsuiteitem.Name_Ar = Foodicsitem.name_localized;
                    Netsuiteitem.Name_En = Foodicsitem.name;
                    Netsuiteitem.Display_Name_Ar = Foodicsitem.name_localized;
                    Netsuiteitem.Display_Name_En = Foodicsitem.name;
                    Netsuiteitem.InActive = Foodicsitem.deleted_at != null ? true : false;
                    Netsuiteitem.UPC_Code = Foodicsitem.sku;
                    Netsuiteitem.Storage_Unit = Foodicsitem.storage_unit;
                    Netsuiteitem.Ingredient_Unit = Foodicsitem.ingredient_unit;
                    Netsuiteitem.storage_to_ingredient_factor = Foodicsitem.storage_to_ingredient_factor;
                    Netsuiteitem.FoodicsUpdateDate = Foodicsitem.updated_at;
                    Netsuiteitem.Subsidiary_Id = Utility.ConvertToInt(ConfigurationManager.AppSettings[Subsidiary + "Netsuite.Subsidiary_Id"]);
                    if (Foodicsitem.category != null && !string.IsNullOrEmpty(Foodicsitem.category.id))
                    {
                        Netsuiteitem.FoodicsCategory_Id = Foodicsitem.category.id;
                        Categories.FoodicsCategories obj = new GenericeDAO<Categories.FoodicsCategories>().GetByFoodicsId(Foodicsitem.category.id);
                        Netsuiteitem.Category_Id = obj.Netsuite_Id;
                    }
                   
                    int UnitsOfMeasure_Id = 0;
                        if (!string.IsNullOrEmpty(Netsuiteitem.Ingredient_Unit))
                            UnitsOfMeasure_Id = new CustomDAO().Check_Create_unitName(Netsuiteitem.Ingredient_Unit);

                        if (UnitsOfMeasure_Id > 0 && Netsuiteitem.Storage_Unit.ToLower() != Netsuiteitem.Ingredient_Unit.ToLower())
                        {
                            UnitsOfMeasureIngredient obj = new UnitsOfMeasureIngredient();
                            obj.Storage_To_Ingredient_Value = Netsuiteitem.storage_to_ingredient_factor;
                            obj.unitName = Netsuiteitem.Storage_Unit;
                            obj.UnitsOfMeasure_Id = UnitsOfMeasure_Id;

                            new CustomDAO().Check_Create_unitName_ingredient(obj);
                        }
                        else if (!string.IsNullOrEmpty(Netsuiteitem.Storage_Unit) && Netsuiteitem.Storage_Unit.ToLower() != Netsuiteitem.Ingredient_Unit.ToLower())
                        {
                            new CustomDAO().Check_Create_unitName(Netsuiteitem.Storage_Unit);
                        }

                    //}

                    Netsuiteitem.Price = Utility.ConvertToDouble(Foodicsitem.cost);


                    //Inventory NetsuiteInventroy = new Inventory();
                    //NetsuiteInventroy.Foodics_Id = Foodicsitem.id;
                    //NetsuiteInventroy.SerialNumbers = Foodicsitem.barcode;
                    //ItemPrice NetsuiteItemPrice = new ItemPrice();
                    //NetsuiteItemPrice.Foodics_Id = Foodicsitem.id;
                    //NetsuiteItemPrice.Price = float.Parse(Foodicsitem.cost != null ? Foodicsitem.cost : "0");
                    //NetsuiteItemPrice.Price_Level_Id = 0;

                    NetSuitelst.Add(Netsuiteitem);
                    //NetSuiteInventorylst.Add(NetsuiteInventroy);
                    //NetSuiteItemPricelst.Add(NetsuiteItemPrice);
                }
                new GenericeDAO<Item>().FoodicsIntegration(NetSuitelst);
                //new GenericeDAO<Inventory>().NetSuiteIntegration(NetSuiteInventorylst);
                //new GenericeDAO<ItemPrice>().NetSuiteIntegration(NetSuiteItemPricelst);
            }
            catch (Exception ex)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
            }
        }

        public override Int64 Set(string parametersArr)
        {
            return 0;
        }



    }


}
