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
    public class FoodicsOtherChargetem_Task : Foodics_BaseIntegration
    {
        public override void Get(string Subsidiary)
        {
            string MainURL = ConfigurationManager.AppSettings[Subsidiary+"Foodics.ResetURL"] + "charges";
            string NextPage = MainURL;
            do
            {
                var client = new RestClient(NextPage);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                request.AddHeader("Authorization", "Bearer " + ConfigurationManager.AppSettings[Subsidiary+"Foodics.Token"]);
                var response = client.Execute<Dictionary<string, List<FoodicsOtherChargeItem>>>(request);
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

        private void Generate_Save_NetSuiteLst(List<FoodicsOtherChargeItem> lstitems,string Subsidiary)
        {
            try
            {
                List<Item> NetSuitelst = new List<Item>();
                foreach (var Foodicsitem in lstitems)
                {
                    Item Netsuiteitem = new Item();
                    Netsuiteitem.Foodics_Id = Foodicsitem.id;
                    Netsuiteitem.Item_Type = (int)Item_Type.OtherChargeSaleItem;
                    Netsuiteitem.Item_Type_Name = nameof(Item_Type.OtherChargeSaleItem);
                    Netsuiteitem.Foodics_Item_Type_Name = nameof(Item_Type.OtherChargeSaleItem);
                    Netsuiteitem.Name_Ar = Foodicsitem.name_localized;
                    Netsuiteitem.Name_En = Foodicsitem.name;
                    Netsuiteitem.Display_Name_Ar = Foodicsitem.name_localized;
                    Netsuiteitem.Display_Name_En = Foodicsitem.name;
                    Netsuiteitem.InActive = Foodicsitem.deleted_at.Year == 1 ? false : true;
                    Netsuiteitem.FoodicsUpdateDate = Foodicsitem.updated_at;
                    Netsuiteitem.Subsidiary_Id = Utility.ConvertToInt(ConfigurationManager.AppSettings[Subsidiary + "Netsuite.Subsidiary_Id"]);
                    Netsuiteitem.Price = Utility.ConvertToDouble(Foodicsitem.value);

                    NetSuitelst.Add(Netsuiteitem);
                }
                new GenericeDAO<Item>().FoodicsIntegration(NetSuitelst,false);
                
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
