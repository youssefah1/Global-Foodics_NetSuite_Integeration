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
    public class FoodicsCustomer_Task : Foodics_BaseIntegration
    {
        public override void Get(string Subsidiary)
        {
            int Subsidiary_Id = Utility.ConvertToInt(ConfigurationManager.AppSettings[Subsidiary + "Netsuite.Subsidiary_Id"]);
            object fromDateObj = new GenericeDAO<Customer>().GetLatestModifiedDate(Subsidiary_Id,"Foodics_UpdateDate");
            DateTime fromDate = new DateTime();
            if (fromDateObj == null)
            {
                fromDate = new DateTime(2021, 02, 15);
            }
            else
            {
                fromDate = (DateTime)fromDateObj;
            }
            string MainURL = ConfigurationManager.AppSettings[Subsidiary+"Foodics.ResetURL"] + "customers?filter[updated_after]=" + fromDate.ToString("yyyy-MM-dd"); 
            string NextPage = MainURL;
            do
            {
                var client = new RestClient(NextPage);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer " + ConfigurationManager.AppSettings[Subsidiary+"Foodics.Token"]);
                var response = client.Execute<Dictionary<string, List<FoodicsCustomer>>>(request);
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
                              Generate_Save_NetSuiteLst(item.Value, Subsidiary_Id);
                            }
                        }
                    }
                }
            } while (!string.IsNullOrEmpty(NextPage));

        }

        private void Generate_Save_NetSuiteLst(List<FoodicsCustomer> lstitems, int Subsidiary_Id)
        {
            try
            {
                List<Customer> NetSuitelst = new List<Customer>();

                foreach (var Foodicsitem in lstitems)
                {
                    Customer obj = JsonConvert.DeserializeObject<Customer>(JsonConvert.SerializeObject(Foodicsitem));

                    obj.Foodics_Id = Foodicsitem.id;
                    obj.InActive = Foodicsitem.deleted_at != null ? false : true;

                    obj.Foodics_UpdateDate = Foodicsitem.updated_at;
                    obj.Subsidiary_Id = Subsidiary_Id;
                    NetSuitelst.Add(obj);

                }
                new GenericeDAO<Customer>().FoodicsIntegration(NetSuitelst);
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
