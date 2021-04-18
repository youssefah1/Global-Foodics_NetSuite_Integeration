//using FoodicsIntegeration.Tasks;
using Foodics.NetSuite.Shared;
using Foodics.NetSuite.Shared.DAO;
using FoodicsIntegeration.Tasks;
using NetSuiteIntegeration.Tasks;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foodics_NetSuite_Integeration
{
    class Program
    {
        static void Main(string[] args)
        {
            int TaskRunType = Utility.ConvertToInt(ConfigurationManager.AppSettings["TaskRunType"]);

            //string[] Subsidary = new string[1];
            //Subsidary[0] = "Overdose";
            string[] Subsidary = new string[2];
            Subsidary[0] = "Laviviane";
            Subsidary[1] = "Monroe";
            foreach (string sub in Subsidary)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Info, sub + " Start Running ", DateTime.Now.ToString());
               //CustomFoodicsTaskCall(sub);
                GenerateRunningTask(sub);
                LogDAO.Integration_Exception(LogIntegrationType.Info, sub + " End Running ", DateTime.Now.ToString());
            }

            #region cutom task call
            //if (TaskRunType == 1)
            //CustomFoodicsTaskCall("Overdose");
            //else
            // CustomNetSuiteTaskCall();
            //for (int i = 0; i < 10; i++)
            //{


             // CustomNetSuiteTaskCall();


            //}


            #endregion

        }
        private static void CustomNetSuiteTaskCall()
        {

            NetSuiteBaseIntegration[] NetSuitetaskInitial = new NetSuiteBaseIntegration[2];
           // NetSuitetaskInitial[0] = new ProductClassTask();
            //NetSuitetaskInitial[0] = new ServiceItemTask();
            //NetSuitetaskInitial[0] = new LocationTask();
            //NetSuitetaskInitial[0] = new UnitsTypeTask();
            //NetSuitetaskInitial[0] = new CustomerTask();
            // NetSuitetaskInitial[0] = new ItemTask();
            //NetSuitetaskInitial[1] = new PaymentMethodTask();
           //  NetSuitetaskInitial[0] = new ProductTask();
            //            NetSuitetaskInitial[0] = new ServiceItemTask();
            //NetSuitetaskInitial[0] = new ProductClassTask();
            
            //NetSuitetaskInitial[0] = new GiftTask();
            //NetSuitetaskInitial[0] = new DiscountTask();

             //NetSuitetaskInitial[0] = new AssemblyBuildTask();
            NetSuitetaskInitial[0] = new InvoiceTask();
            //NetSuitetaskInitial[0] = new InvoiceReturnTask();
            //NetSuitetaskInitial[3] = new AdjustmentBuildTask();

            NetSuitetaskInitial[1] = new CustomerPaymentTask();
            // NetSuitetaskInitial[0] = new CustomerRefundTask();


            foreach (NetSuiteBaseIntegration ts in NetSuitetaskInitial)
            {
                try
                {
                    
                    ts.Set("");
                   // ts.Get();
                }
                catch (Exception ex)
                {
                    LogDAO.Integration_Exception(LogIntegrationType.Error, "NetSuitetaskInitial Main Program Error", "Error " + ex.Message);
                }
            }
        }
        private static void CustomFoodicsTaskCall(string Subsidary)
        {
            Foodics_BaseIntegration[] FoodicstaskFinal = new Foodics_BaseIntegration[1];
            // FoodicstaskFinal[0] = new FoodicsCustomer_Task();
             FoodicstaskFinal[0] = new FoodicsOrder_Task();
            //FoodicstaskFinal[0] = new FoodicsInventoryItem_Task();
            //FoodicstaskFinal[0] = new FoodicsInventoryItem_Task();
            //FoodicstaskFinal[0] = new Foodicsproducts_Task();
            //FoodicstaskFinal[0] = new FoodicsProductCategories_task();
            //FoodicstaskFinal[0] = new FoodicsModifiers_task();
            //FoodicstaskFinal[0] = new FoodicsModifierOption_Task();
            foreach (Foodics_BaseIntegration ts in FoodicstaskFinal)
            {
                try
                {
                     ts.Get(Subsidary);
                }
                catch (Exception ex)
                {
                    LogDAO.Integration_Exception(LogIntegrationType.Error, "NetSuitetaskInitial Main Program Error", "Error " + ex.Message);
                }
            }

        }
        private static void GenerateRunningTask(string Subsidary)
        {

            int TaskRunType = Utility.ConvertToInt(ConfigurationManager.AppSettings["TaskRunType"]);


            if (TaskRunType == 1 || TaskRunType == 3)
            {
                #region Setting

               
                //NetSuiteBaseIntegration[] NetSuitetaskBase = new NetSuiteBaseIntegration[1];
                //NetSuitetaskBase[0] = new SettingTask();

                //foreach (NetSuiteBaseIntegration ts in NetSuitetaskBase)
                //{
                //    try
                //    {

                //        ts.Set("");
                //        ts.Get();
                //    }
                //    catch (Exception ex)
                //    {
                //        LogDAO.Integration_Exception(LogIntegrationType.Error, "NetSuitetaskBase Main Program Error", "Error " + ex.Message);
                //    }
                //}

 #endregion
                //foodics integeration
                Foodics_BaseIntegration[] FoodicstaskInital = new Foodics_BaseIntegration[7];

                FoodicstaskInital[0] = new FoodicsBranche_Task();
                FoodicstaskInital[1] = new FoodicsPaymentMethod_Task();
                FoodicstaskInital[2] = new FoodicsProductCategories_task();
                FoodicstaskInital[3] = new FoodicsInventoryCategories_task();
                FoodicstaskInital[4] = new FoodicsModifiers_task();
                FoodicstaskInital[5] = new FoodicsInventoryItem_Task();
                FoodicstaskInital[6] = new FoodicsdDiscounts_Task();
               // FoodicstaskInital[7] = new FoodicsCustomer_Task();


                foreach (Foodics_BaseIntegration ts in FoodicstaskInital)
                {
                    try
                    {
                        LogDAO.Integration_Exception(LogIntegrationType.Error, "FoodicstaskInital Main Program Task", ts.GetType().Name);
                        ts.Get(Subsidary);
                    }
                    catch (Exception ex)
                    {
                        LogDAO.Integration_Exception(LogIntegrationType.Error, "FoodicstaskInital Main Program Error", "Error " + ex.Message);
                    }
                }



                NetSuiteBaseIntegration[] NetSuitetaskInitial = new NetSuiteBaseIntegration[6];
                NetSuitetaskInitial[0] = new ProductClassTask();
                NetSuitetaskInitial[1] = new LocationTask();
                NetSuitetaskInitial[2] = new UnitsTypeTask();
                NetSuitetaskInitial[3] = new ItemTask();
                NetSuitetaskInitial[4] = new DiscountTask();
                NetSuitetaskInitial[5] = new CustomerTask();
                foreach (NetSuiteBaseIntegration ts in NetSuitetaskInitial)
                {
                    try
                    {
                        LogDAO.Integration_Exception(LogIntegrationType.Error, "NetSuitetaskInitial Main Program Task",ts.GetType().Name);
                        ts.Set("");
                        ts.Get();
                    }
                    catch (Exception ex)
                    {
                        LogDAO.Integration_Exception(LogIntegrationType.Error, "NetSuitetaskInitial Main Program Error", "Error " + ex.Message);
                    }
                }
                Foodics_BaseIntegration[] Foodicstask2 = new Foodics_BaseIntegration[3];
                Foodicstask2[0] = new Foodicsproducts_Task();
                Foodicstask2[1] = new FoodicsModifierOption_Task();
                Foodicstask2[2] = new FoodicsPaymentMethod_Task();

                foreach (Foodics_BaseIntegration ts in Foodicstask2)
                {
                    try
                    {
                        LogDAO.Integration_Exception(LogIntegrationType.Error, "Foodicstask2 Main Program Task", ts.GetType().Name);

                        ts.Get(Subsidary);
                    }
                    catch (Exception ex)
                    {
                        LogDAO.Integration_Exception(LogIntegrationType.Error, "Foodicstask2 Main Program Error", "Error " + ex.Message);
                    }
                }

                NetSuiteBaseIntegration[] NetSuitetask02 = new NetSuiteBaseIntegration[2];
                NetSuitetask02[0] = new PaymentMethodTask();
                NetSuitetask02[1] = new ProductTask();

                foreach (NetSuiteBaseIntegration ts in NetSuitetask02)
                {
                    try
                    {
                        LogDAO.Integration_Exception(LogIntegrationType.Error, "NetSuitetask02 Main Program Task", ts.GetType().Name);
                        ts.Set("");
                        ts.Get();
                    }
                    catch (Exception ex)
                    {
                        LogDAO.Integration_Exception(LogIntegrationType.Error, "NetSuitetask02 Main Program Error", "Error " + ex.Message);
                    }
                }


            }
            if (TaskRunType == 2 || TaskRunType == 3)
            {
                //should run after posting items
                Foodics_BaseIntegration[] FoodicstaskFinal = new Foodics_BaseIntegration[1];
                FoodicstaskFinal[0] = new FoodicsOrder_Task();
                foreach (Foodics_BaseIntegration ts in FoodicstaskFinal)
                {
                    try
                    {
                      // ts.Get(Subsidary);
                    }
                    catch (Exception ex)
                    {
                        LogDAO.Integration_Exception(LogIntegrationType.Error, "NetSuitetaskInitial Main Program Error", "Error " + ex.Message);
                    }
                }
                NetSuiteBaseIntegration[] NetSuitetaskFinal = new NetSuiteBaseIntegration[6];
                NetSuitetaskFinal[0] = new AssemblyBuildTask();
                NetSuitetaskFinal[1] = new InvoiceTask();
                NetSuitetaskFinal[2] = new CustomerPaymentTask();
                NetSuitetaskFinal[3] = new InvoiceReturnTask();
                NetSuitetaskFinal[4] = new CustomerRefundTask();
                NetSuitetaskFinal[5] = new AdjustmentBuildTask();

                foreach (NetSuiteBaseIntegration ts in NetSuitetaskFinal)
                {
                    try
                    {

                        ts.Set("");
                        ts.Get();
                    }
                    catch (Exception ex)
                    {
                        LogDAO.Integration_Exception(LogIntegrationType.Error, "NetSuitetaskFinal Main Program Error", "Error " + ex.Message);
                    }
                }
            }
        }
    }
}
