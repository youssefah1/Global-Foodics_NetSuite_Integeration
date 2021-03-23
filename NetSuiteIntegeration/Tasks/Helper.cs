using Foodics.NetSuite.Shared.Model;
using NetSuiteIntegeration.com.netsuite.webservices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSuiteIntegeration.Tasks
{
    class Helper
    {

        public static PricingMatrix GeneratePricingMatrix(Setting objSetting, Double price)
        {
            Price[] pricelst = new Price[1];
            Pricing[] Pricinglst = new Pricing[1];
            PricingMatrix PricingMatrixobj = new PricingMatrix();

            RecordRef Currency = new RecordRef();
            Currency.internalId = objSetting.Currency_Netsuite_Id.ToString();
            //Currency.typeSpecified = true;
            // Currency.type = RecordType.account;
            //for (int i = 0; i < 5; i++)
            //{
            RecordRef PriceLevel = new RecordRef();
            PriceLevel.internalId = "1";//objSetting.Price_Level_Netsuite_Id.ToString();
            PriceLevel.typeSpecified = true;

            PriceLevel.type = RecordType.priceLevel;



            Price objprice = new Price();
            objprice.value = price;
            objprice.quantity = 0;//0 for item,1 for assembly
                                
            objprice.quantitySpecified = true;
            objprice.valueSpecified = true;

            pricelst[0] = objprice;



            Pricing objPricing = new Pricing();
            objPricing.currency = Currency;
            objPricing.priceLevel = PriceLevel;
            objPricing.priceList = pricelst;
            objPricing.discount = double.NaN;//0;
            objPricing.discountSpecified = false;


            Pricinglst[0] = objPricing;
            //}

            //NewItemObject.taxSchedule = Tax_Schedule;


            //PricingMatrixobj.pricing = new Pricing();

            PricingMatrixobj.pricing = Pricinglst;
            //PricingMatrixobj.replaceAll = false;

            return PricingMatrixobj;


        }
    }
}
