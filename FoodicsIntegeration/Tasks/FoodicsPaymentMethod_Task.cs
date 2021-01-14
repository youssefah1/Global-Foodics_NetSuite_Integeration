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
    public class FoodicsPaymentMethod_Task : Foodics_BaseIntegration
    {
        public override void Get(string Subsidiary)
        {
           
            var client = new RestClient(ConfigurationManager.AppSettings[Subsidiary+"Foodics.ResetURL"] + "payment_methods");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Bearer " + ConfigurationManager.AppSettings[Subsidiary+"Foodics.Token"]);
            var response = client.Execute<Dictionary<string, List<FoodicsPaymentMethod>>>(request);
            if (response.StatusCode == HttpStatusCode.OK && response.Data != null)
            {
                foreach (var item in response.Data)
                {
                    if (item.Key == "data")
                    {
                        //List<Branches> lst = item.Value;
                        if (item.Value.Count > 0)
                            Generate_Save_NetSuiteLst(item.Value);
                        //new GenericeDAO<FoodicsPaymentMethod>().FoodicsIntegration(item.Value);
                        //foreach (var item2 in lst)
                        //{
                        //    string name = item2.name;
                        //}
                    }
                }
            }

        }

        private void Generate_Save_NetSuiteLst(List<FoodicsPaymentMethod> lstitems)
        {
            try
            {
                List<PaymentMethod> NetSuitelst = new List<PaymentMethod>();

            foreach (var Foodicsitem in lstitems)
            {

                PaymentMethod Netsuiteitem = new PaymentMethod();
                //barcode
                Netsuiteitem.Foodics_Id = Foodicsitem.id;
                Netsuiteitem.Method_Type_Id = Foodicsitem.type;
                Netsuiteitem.Name_Ar = Foodicsitem.name_localized;
                Netsuiteitem.Name_En = Foodicsitem.name;
                Netsuiteitem.InActive = Foodicsitem.deleted_at != null ? false : true;

                    NetSuitelst.Add(Netsuiteitem);

            }




            new GenericeDAO<PaymentMethod>().NetSuiteIntegration(NetSuitelst);
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
