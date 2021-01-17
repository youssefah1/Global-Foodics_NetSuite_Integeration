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
using System.Net;

namespace FoodicsIntegeration.Tasks
{
    public class FoodicsdDiscounts_Task : Foodics_BaseIntegration
    {
        public override void Get(string Subsidiary)
        {

            var client = new RestClient(ConfigurationManager.AppSettings[Subsidiary + "Foodics.ResetURL"] + "discounts");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Bearer " + ConfigurationManager.AppSettings[Subsidiary + "Foodics.Token"]);
            var response = client.Execute<Dictionary<string, List<FoodicsDiscount>>>(request);
            if (response.StatusCode == HttpStatusCode.OK && response.Data != null)
            {
                foreach (var item in response.Data)
                {
                    if (item.Key == "data")
                    {
                        if (item.Value.Count > 0)
                            Generate_Save_NetSuiteLst(item.Value, Subsidiary);
                    }
                }
            }

        }

        private void Generate_Save_NetSuiteLst(List<FoodicsDiscount> lstitems, string Subsidiary)
        {
            try
            {
                List<Discount> NetSuitelst = new List<Discount>();

                foreach (var Foodicsitem in lstitems)
                {

                    Discount Netsuiteitem = new Discount();
                    //barcode
                    Netsuiteitem.Foodics_Id = Foodicsitem.id;
                    Netsuiteitem.Name_Ar = Foodicsitem.name_localized;
                    Netsuiteitem.Name_En = Foodicsitem.name;
                    Netsuiteitem.Qualification = Foodicsitem.qualification;
                    Netsuiteitem.Amount = Foodicsitem.amount;
                    Netsuiteitem.IsPercentage = Foodicsitem.is_percentage;
                    Netsuiteitem.IsTaxable = Foodicsitem.is_taxable;
                    Netsuiteitem.InActive = Foodicsitem.deleted_at.Year >1 ? true : false;
                    Netsuiteitem.Subsidiary_Id = Utility.ConvertToInt(ConfigurationManager.AppSettings[Subsidiary + "Netsuite.Subsidiary_Id"]);
                    NetSuitelst.Add(Netsuiteitem);
                }
                new GenericeDAO<Discount>().FoodicsIntegration(NetSuitelst);
            }
            catch (Exception ex)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
            }
            //return NetSuitelst;
        }
        public override Int64 Set(string parametersArr)
        {
            return 0;
        }



    }


}
