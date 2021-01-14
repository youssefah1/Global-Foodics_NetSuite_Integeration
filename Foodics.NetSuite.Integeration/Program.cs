using FoodicsIntegeration.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foodics_NetSuite_Integeration
{
    class Program
    {
        static void Main(string[] args)
        {
            BaseIntegration[] task = new BaseIntegration[1];
            task[0] = new FoodicsBranche_Task();
           //task[0] = new FoodicsInventoryItem_Task();
            //task[0] = new Foodicsproducts_Task();
            //task[0] = new FoodicsGiftCardProduct_Task();
            //task[0] = new FoodicsCombo_Task();
            //task[0] = new FoodicsPaymentMethod_Task();
            //task[0] = new FoodicsOrder_Task();
            //task[0] = new FoodicsGiftcardtransaction_Task();
            //task[0] = new FoodicsCustomer_Task();
            foreach (BaseIntegration ts in task)
            {
                try
                {
                    ts.Get();
                    //ts.Set("");
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
