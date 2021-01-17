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

            string[] Subsidary = new string[2];
            Subsidary[0] = "Overdose";
            //Subsidary[0] = "Monroe";
            //Subsidary[1] = "Laviviane";
            //foreach (string sub in Subsidary)
            //{
            //    LogDAO.Integration_Exception(LogIntegrationType.Info, sub + " Start Running ", DateTime.Now.ToString());

            //    GenerateRunningTask(sub);

            //    LogDAO.Integration_Exception(LogIntegrationType.Info, sub + " End Running ", DateTime.Now.ToString());
            //}

            #region cutom task call
            //CustomFoodicsTaskCall("Overdose");
            //for (int i = 0; i < 5; i++)
            //{
                CustomTaskCall();
            //}





            #endregion

        }
        private static void CustomTaskCall()
        {

            NetSuiteBaseIntegration[] NetSuitetaskInitial = new NetSuiteBaseIntegration[1];
            //NetSuitetaskInitial[0] = new ProductClassTask();
            //NetSuitetaskInitial[0] = new LocationTask();
            //NetSuitetaskInitial[0] = new UnitsTypeTask();
            //NetSuitetaskInitial[0] = new CustomerTask();
            //NetSuitetaskInitial[0] = new ItemTask();
            //NetSuitetaskInitial[0] = new PaymentMethodTask();
            //NetSuitetaskInitial[1] = new ProductTask();
            //NetSuitetaskInitial[0] = new GiftTask();
            //NetSuitetaskInitial[0] = new DiscountTask();
           // NetSuitetaskInitial[0] = new InvoiceTask();
           // NetSuitetaskInitial[1] = new CustomerPaymentTask();
           //NetSuitetaskInitial[2] = new InvoiceReturnTask();
           // NetSuitetaskInitial[3] = new CustomerRefundTask();
            NetSuitetaskInitial[0] = new AssemblyBuildTask();
            NetSuitetaskInitial[1] = new AdjustmentBuildTask();


            foreach (NetSuiteBaseIntegration ts in NetSuitetaskInitial)
            {
                try
                {

                    ts.Set("");
                    ts.Get();
                }
                catch (Exception ex)
                {
                    LogDAO.Integration_Exception(LogIntegrationType.Error, "NetSuitetaskInitial Main Program Error", "Error " + ex.Message);
                }
            }
        }
        private static void CustomFoodicsTaskCall(string Subsidary)
        {
            Foodics_BaseIntegration[] FoodicstaskInital = new Foodics_BaseIntegration[1];

            //FoodicstaskInital[0] = new FoodicsBranche_Task();
            //FoodicstaskInital[1] = new FoodicsPaymentMethod_Task();
            //FoodicstaskInital[2] = new FoodicsCategories_task();
            //FoodicstaskInital[0] = new FoodicsCustomer_Task();
            //FoodicstaskInital[0] = new FoodicsInventoryItem_Task();
            FoodicstaskInital[0] = new FoodicsdDiscounts_Task();

            foreach (Foodics_BaseIntegration ts in FoodicstaskInital)
            {
                try
                {
                    ts.Get(Subsidary);
                }
                catch (Exception ex)
                {
                    LogDAO.Integration_Exception(LogIntegrationType.Error, "FoodicstaskInital Main Program Error", "Error " + ex.Message);
                }
            }

            //Foodics_BaseIntegration[] FoodicstaskFinal = new Foodics_BaseIntegration[1];
            ////FoodicstaskFinal[0] = new Foodicsproducts_Task();
            //FoodicstaskFinal[0] = new DiscountTask();

            //foreach (Foodics_BaseIntegration ts in FoodicstaskFinal)
            //{
            //    try
            //    {
            //        ts.Get(Subsidary);
            //    }
            //    catch (Exception ex)
            //    {
            //        LogDAO.Integration_Exception(LogIntegrationType.Error, "NetSuitetaskInitial Main Program Error", "Error " + ex.Message);
            //    }
            //}

        }
        private static void GenerateRunningTask(string Subsidary)
        {

            int TaskRunType = Utility.ConvertToInt(ConfigurationManager.AppSettings["TaskRunType"]);


            if (TaskRunType == 1 || TaskRunType == 3)
            {
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


                //foodics integeration
                Foodics_BaseIntegration[] FoodicstaskInital = new Foodics_BaseIntegration[6];

                FoodicstaskInital[0] = new FoodicsBranche_Task();
                FoodicstaskInital[1] = new FoodicsPaymentMethod_Task();
                FoodicstaskInital[2] = new FoodicsCategories_task();
                FoodicstaskInital[3] = new FoodicsCustomer_Task();
                FoodicstaskInital[4] = new FoodicsInventoryItem_Task();
                FoodicstaskInital[5] = new FoodicsdDiscounts_Task();

                foreach (Foodics_BaseIntegration ts in FoodicstaskInital)
                {
                    try
                    {
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
                NetSuitetaskInitial[3] = new CustomerTask();
                NetSuitetaskInitial[4] = new ItemTask();
                NetSuitetaskInitial[5] = new DiscountTask();


                foreach (NetSuiteBaseIntegration ts in NetSuitetaskInitial)
                {
                    try
                    {

                        ts.Set("");
                        ts.Get();
                    }
                    catch (Exception ex)
                    {
                        LogDAO.Integration_Exception(LogIntegrationType.Error, "NetSuitetaskInitial Main Program Error", "Error " + ex.Message);
                    }
                }


            }
            if (TaskRunType == 2 || TaskRunType == 3)
            {
                //should run after posting items
                Foodics_BaseIntegration[] FoodicstaskFinal = new Foodics_BaseIntegration[4];
                FoodicstaskFinal[0] = new Foodicsproducts_Task();
                FoodicstaskFinal[1] = new FoodicsGiftCardProduct_Task();
                FoodicstaskFinal[2] = new FoodicsCombo_Task();
                FoodicstaskFinal[3] = new FoodicsOrder_Task();
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


                NetSuiteBaseIntegration[] NetSuitetaskFinal = new NetSuiteBaseIntegration[8];
                NetSuitetaskFinal[0] = new PaymentMethodTask();
                NetSuitetaskFinal[1] = new ProductTask();
                NetSuitetaskFinal[2] = new InvoiceTask();
                NetSuitetaskFinal[3] = new CustomerPaymentTask();
                NetSuitetaskFinal[4] = new AssemblyBuildTask();
                NetSuitetaskFinal[5] = new InvoiceReturnTask();
                NetSuitetaskFinal[6] = new CustomerRefundTask();
                NetSuitetaskFinal[7] = new AdjustmentBuildTask();

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
