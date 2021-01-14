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
    public class FoodicsGiftcardtransaction_Task : Foodics_BaseIntegration
    {
        public override void Get(string Subsidiary)
        {
           
            var client = new RestClient(ConfigurationManager.AppSettings["Foodics.ResetURL"] + "gift_card_transactions");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Bearer " + ConfigurationManager.AppSettings["Foodics.Token"]);
            var response = client.Execute<Dictionary<string, List<FoodicsGiftcardtransaction>>>(request);
            if (response.StatusCode == HttpStatusCode.OK && response.Data != null)
            {
                foreach (var item in response.Data)
                {
                    if (item.Key == "data")
                    {
                        //List<Branches> lst = item.Value;
                        //if (item.Value.Count > 0)
                        //    new GenericeDAO<FoodicsGiftcardtransaction>().FoodicsIntegration(item.Value);
                        //foreach (var item2 in lst)
                        //{
                        //    string name = item2.name;
                        //}
                    }
                }
            }

        }
        public override Int64 Set(string parametersArr)
        {
            return 0;
        }

        

    }
   

}
