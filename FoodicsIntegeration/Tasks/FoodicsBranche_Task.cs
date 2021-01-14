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
    public class FoodicsBranche_Task : Foodics_BaseIntegration
    {
        public override void Get(string Subsidiary)
        {

            var client = new RestClient(ConfigurationManager.AppSettings[Subsidiary + "Foodics.ResetURL"] + "branches");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Bearer " + ConfigurationManager.AppSettings[Subsidiary + "Foodics.Token"]);
            var response = client.Execute<Dictionary<string, List<FoodicsBranche>>>(request);
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

                            // Generate_Save_NetSuiteLst(lstitems.Where(x => x.updated_at >= fromDate).ToList());


                            //  new GenericeDAO<FoodicsBranche>().FoodicsIntegration(item.Value.Where(x => x.updated_at >= fromDate).ToList());
                            Generate_Save_NetSuiteLst(item.Value, Subsidiary);
                        }

                    }
                }
            }

        }

        private void Generate_Save_NetSuiteLst(List<FoodicsBranche> lstitems, string Subsidiary)
        {
            try
            {
                List<Location> NetSuitelst = new List<Location>();


                foreach (var Foodicsitem in lstitems)
                {

                    Location Netsuiteitem = new Location();
                    //barcode
                    Netsuiteitem.Foodics_Id = Foodicsitem.id;
                    Netsuiteitem.Name_Ar = Foodicsitem.name_localized + " - " + Foodicsitem.reference + " - " + Subsidiary;
                    Netsuiteitem.Name_En = Foodicsitem.name + " - " + Foodicsitem.reference + " - " + Subsidiary;
                    Netsuiteitem.InActive = Foodicsitem.deleted_at != null ? false : true;

                    Netsuiteitem.Latitude = Foodicsitem.latitude;
                    Netsuiteitem.Longitude = Foodicsitem.longitude;
                    Netsuiteitem.Subsidiary_Id = Utility.ConvertToInt(ConfigurationManager.AppSettings[Subsidiary + "Netsuite.Subsidiary_Id"]);




                    NetSuitelst.Add(Netsuiteitem);


                }

                new GenericeDAO<Location>().NetSuiteIntegration(NetSuitelst);
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
