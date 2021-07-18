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
    public class FoodicsModifierOptionProduct_Task : Foodics_BaseIntegration
    {
        public override void Get(string Subsidiary)
        {
            string MainURL = ConfigurationManager.AppSettings[Subsidiary + "Foodics.ResetURL"] + "modifier_options?include=ingredients,modifier,tax_group";
            //string MainURL = ConfigurationManager.AppSettings[Subsidiary + "Foodics.ResetURL"] + "modifier_options?include=ingredients,modifier,tax_group&filter[id]=93334e52-d562-49df-bcf3-5c40c7e164ef";
            string NextPage = MainURL;
            do
            {
                var client = new RestClient(NextPage);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                request.AddHeader("Authorization", "Bearer " + ConfigurationManager.AppSettings[Subsidiary + "Foodics.Token"]);
                var response = client.Execute<Dictionary<string, List<FoodicsProduct>>>(request);
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
                                Generate_Save_NetSuiteLst(item.Value, Subsidiary);
                        }
                    }
                }
            } while (!string.IsNullOrEmpty(NextPage));
        }

        private void Generate_Save_NetSuiteLst(List<FoodicsProduct> lstitems, string Subsidiary)
        {
            try
            {
                List<Item> NetSuitelst = new List<Item>();
                List<ItemCompnent> ItemCompnentList = new List<ItemCompnent>();
                foreach (var Foodicsitem in lstitems)
                {
                    Item Netsuiteitem = new Item();
                    Item itemobj = new GenericeDAO<Item>().GetByFoodicsId(Foodicsitem.id);
                    //barcode
                    Netsuiteitem.Foodics_Id = Foodicsitem.id;
                    Netsuiteitem.Item_Type = (int)Item_Type.AssemblyItem;
                    Netsuiteitem.Item_Type_Name = nameof(Item_Type.AssemblyItem);
                    Netsuiteitem.Foodics_Item_Type_Name = nameof(FoodicsItem_Type.ModifierOption);
                    Netsuiteitem.Name_Ar = Foodicsitem.name_localized;
                    Netsuiteitem.Name_En = Foodicsitem.name;
                    Netsuiteitem.Display_Name_Ar = Foodicsitem.name_localized;
                    Netsuiteitem.Display_Name_En = Foodicsitem.name;
                    Netsuiteitem.InActive = Foodicsitem.deleted_at.Year == 1 ? false : true;
                    Netsuiteitem.UPC_Code = Foodicsitem.sku;
                    Netsuiteitem.Img = Foodicsitem.image;
                    Netsuiteitem.Price = (double)Foodicsitem.price;
                    Netsuiteitem.Subsidiary_Id = Utility.ConvertToInt(ConfigurationManager.AppSettings[Subsidiary + "Netsuite.Subsidiary_Id"]);
                    Netsuiteitem.FoodicsUpdateDate = Foodicsitem.updated_at;
                    if (Foodicsitem.tax_group != null && !string.IsNullOrEmpty(Foodicsitem.tax_group.id))
                    {
                        Netsuiteitem.FoodicsTaxGroup_Id = Foodicsitem.tax_group.id;
                    }
                    if (Foodicsitem.modifier != null && !string.IsNullOrEmpty(Foodicsitem.modifier.id))
                    {
                        Netsuiteitem.FoodicsCategory_Id = Foodicsitem.modifier.id;
                        Categories.FoodicsCategories obj = new GenericeDAO<Categories.FoodicsCategories>().GetByFoodicsId(Foodicsitem.modifier.id);
                        if (obj != null && obj.Netsuite_Id > 0)
                            Netsuiteitem.Category_Id = obj.Netsuite_Id;
                    }
                    foreach (var ingredientsobj in Foodicsitem.ingredients)
                    {
                        Item itemcomponent = new GenericeDAO<Item>().GetByFoodicsId(ingredientsobj.id);
                        
                            ItemCompnent objitmcom = new ItemCompnent();
                            objitmcom.UnitId = 0;
                            objitmcom.UnitName = ingredientsobj.ingredient_unit;
                            objitmcom.Quantity = ingredientsobj.pivot.quantity;
                            if (itemcomponent != null && itemcomponent.Netsuite_Id > 0)
                                objitmcom.ComponentId = itemcomponent.Netsuite_Id;
                            else
                                objitmcom.ComponentId = 0;
                            objitmcom.ItemFoodics_Id = Foodicsitem.id;
                            objitmcom.ComponentFoodics_Id = ingredientsobj.id;

                            if (itemobj != null && itemobj.Netsuite_Id > 0)
                                objitmcom.ItemId = itemobj.Netsuite_Id;
                            else
                                objitmcom.ItemId = 0;

                            ItemCompnentList.Add(objitmcom);
                    }

                    NetSuitelst.Add(Netsuiteitem);

                }

                new GenericeDAO<Item>().FoodicsIntegration(NetSuitelst,false);
                new GenericeDAO<ItemCompnent>().ItemcompnentFoodicsIntegration(ItemCompnentList);
                //string Itemids = "'";
                //Itemids += string.Join("','", NetSuitelst.Select(x => x.Foodics_Id).Distinct().ToArray());
                //Itemids += "'";
                //new CustomDAO().DeleteFoodicsItemsComponent(Itemids);
                //new GenericeDAO<ItemCompnent>().ListInsertOnly(ItemCompnentList);
                new CustomDAO().UpdateProductCompnent();

                //return NetSuitelst;
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
