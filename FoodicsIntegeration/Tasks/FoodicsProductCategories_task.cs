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
    public class FoodicsProductCategories_task : Foodics_BaseIntegration
    {
        public override void Get(string Subsidiary)
        {
            string NextPage = ConfigurationManager.AppSettings[Subsidiary + "Foodics.ResetURL"] + "categories";
            var client = new RestClient(NextPage);
            //var client = new RestClient(ConfigurationManager.AppSettings[Subsidiary + "Foodics.ResetURL"] + "categories");
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
                        if (item.Value.Count > 0)
                        {
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
                List<Categories.FoodicsCategories> NetSuitelst = new List<Categories.FoodicsCategories>();

                foreach (var Foodicsitem in lstitems)
                {
                    Categories.FoodicsCategories obj = JsonConvert.DeserializeObject<Categories.FoodicsCategories>(JsonConvert.SerializeObject(Foodicsitem));

                    obj.Foodics_Id = Foodicsitem.id;
                    obj.InActive = Foodicsitem.deleted_at.Year == 1 ? false : true;
                    obj.Subsidiary_Id = Utility.ConvertToInt(ConfigurationManager.AppSettings[Subsidiary + "Netsuite.Subsidiary_Id"]);
                    obj.CategoryType =1;
                    NetSuitelst.Add(obj);

                }
                new GenericeDAO<Categories.FoodicsCategories>().FoodicsIntegration(NetSuitelst);
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
