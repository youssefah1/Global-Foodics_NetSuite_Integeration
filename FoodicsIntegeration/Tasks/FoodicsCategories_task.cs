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
    public class FoodicsCategories_task : Foodics_BaseIntegration
    {
        public override void Get(string Subsidiary)
        {

            var client = new RestClient(ConfigurationManager.AppSettings[Subsidiary + "Foodics.ResetURL"] + "categories");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Bearer " + ConfigurationManager.AppSettings[Subsidiary + "Foodics.Token"]);
            var response = client.Execute<Dictionary<string, List<FoodicsCategories>>>(request);
            if (response.StatusCode == HttpStatusCode.OK && response.Data != null)
            {
                foreach (var item in response.Data)
                {
                    if (item.Key == "data")
                    {
                        //List<Branches> lst = item.Value;
                        if (item.Value.Count > 0)
                        {
                            //new GenericeDAO<FoodicsCategories>().FoodicsIntegration(item.Value);

                            // List<Customer> LstNetSuite = JsonConvert.DeserializeObject<List<Customer>>(JsonConvert.SerializeObject(item.Value));
                            //new GenericeDAO<Customer>().NetSuiteIntegration(LstNetSuite);

                            Generate_Save_NetSuiteLst(item.Value, Subsidiary);
                        }



                    }
                }
            }

        }

        private void Generate_Save_NetSuiteLst(List<FoodicsCategories> lstitems, string Subsidiary)
        {
            try
            {
                List<Categories> NetSuitelst = new List<Categories>();

                foreach (var Foodicsitem in lstitems)
                {
                    Categories obj = JsonConvert.DeserializeObject<Categories>(JsonConvert.SerializeObject(Foodicsitem));

                    obj.Foodics_Id = Foodicsitem.id;
                    obj.InActive = Foodicsitem.deleted_at != null ? false : true;
                    obj.Subsidiary_Id = Utility.ConvertToInt(ConfigurationManager.AppSettings[Subsidiary + "Netsuite.Subsidiary_Id"]);
                    NetSuitelst.Add(obj);

                }
                new GenericeDAO<Categories>().FoodicsIntegration(NetSuitelst);
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
