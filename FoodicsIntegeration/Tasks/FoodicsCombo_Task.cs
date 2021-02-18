
using Microsoft.Build.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using Foodics.NetSuite.Shared.DAO;
using Foodics.NetSuite.Shared.Model;
using Foodics.NetSuite.Shared;

namespace FoodicsIntegeration.Tasks
{
    public class FoodicsCombo_Task : Foodics_BaseIntegration
    {
        public override void Get(string Subsidiary)
        {
           
            var client = new RestClient(ConfigurationManager.AppSettings["Foodics.ResetURL"] + "combos");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Bearer " + ConfigurationManager.AppSettings["Foodics.Token"]);
            var response = client.Execute<Dictionary<string, List<FoodicsCombo>>>(request);
            if (response.StatusCode == HttpStatusCode.OK && response.Data != null)
            {
                foreach (var item in response.Data)
                {
                    if (item.Key == "data")
                    {
                        //List<Branches> lst = item.Value;
                        if (item.Value.Count > 0)
                        {
                            //object fromDateObj = new GenericeDAO<FoodicsBranche>().GetLatestModifiedDate();
                            //DateTime fromDate = new DateTime();
                            //if (fromDateObj == null)
                            //{
                            //    fromDate = DateTime.Now;
                            //}
                            //else
                            //{
                            //    fromDate = (DateTime)fromDateObj;
                            //    //fromDate = fromDate.AddDays(6);
                            //}

                            //// Generate_Save_NetSuiteLst(lstitems.Where(x => x.updated_at >= fromDate).ToList());


                            //Generate_Save_NetSuiteLst(item.Value);
                        }
                            //new GenericeDAO<FoodicsCombo>().FoodicsIntegration(item.Value);
                          
                        //foreach (var item2 in lst)
                        //{
                        //    string name = item2.name;
                        //}
                    }
                }
            }

        }

        private void Generate_Save_NetSuiteLst(List<FoodicsCombo> lstitems)
        {
            try
            {
                List<Item> NetSuitelst = new List<Item>();
            List<Inventory> NetSuiteInventorylst = new List<Inventory>();
            List<ItemPrice> NetSuiteItemPricelst = new List<ItemPrice>();

            foreach (var Foodicsitem in lstitems)
            {

                Item Netsuiteitem = new Item();
                //barcode
                Netsuiteitem.Foodics_Id = Foodicsitem.id;
                Netsuiteitem.Item_Type = (int)Item_Type.Combo;
                Netsuiteitem.Name_Ar = Foodicsitem.name_localized;
                Netsuiteitem.Name_En = Foodicsitem.name;
                Netsuiteitem.Display_Name_Ar = Foodicsitem.description_localized;
                Netsuiteitem.Display_Name_En = Foodicsitem.description;
                Netsuiteitem.InActive = Foodicsitem.deleted_at != null ? false : true;
                Netsuiteitem.UPC_Code = Foodicsitem.sku;
                Netsuiteitem.Img = Foodicsitem.image;
                // Netsuiteitem.Unit_Type = Foodicsitem.storage_unit;
                //Netsuiteitem.Stock_Unit = Foodicsitem.;
                //Netsuiteitem.Purchase_Unit = Foodicsitem.storage_unit;
                //Netsuiteitem.Sales_Unit = Foodicsitem.storage_unit;
                //Netsuiteitem.Base_Unit = Foodicsitem.storage_unit;



                Inventory NetsuiteInventroy = new Inventory();
                NetsuiteInventroy.Foodics_Id = Foodicsitem.id;
                NetsuiteInventroy.SerialNumbers = Foodicsitem.barcode;
                //NetsuiteInventroy.QuantityOnHand =Convert.ToDouble( Foodicsitem.storage_unit);



                ItemPrice NetsuiteItemPrice = new ItemPrice();
                NetsuiteItemPrice.Foodics_Id = Foodicsitem.id;
                NetsuiteItemPrice.Price = 0;
                NetsuiteItemPrice.Price_Level_Id = 0;

                NetSuitelst.Add(Netsuiteitem);

                NetSuiteInventorylst.Add(NetsuiteInventroy);
                NetSuiteItemPricelst.Add(NetsuiteItemPrice);
            }




            new GenericeDAO<Item>().NetSuiteIntegration(NetSuitelst);
            new GenericeDAO<Inventory>().NetSuiteIntegration(NetSuiteInventorylst);
            new GenericeDAO<ItemPrice>().NetSuiteIntegration(NetSuiteItemPricelst);
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
