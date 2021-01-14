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

        public static PricingMatrix GeneratePricingMatrix(Setting objSetting,Double price)
        {
            Price[] pricelst = new Price[1];
            Pricing[] Pricinglst = new Pricing[1];
            PricingMatrix PricingMatrixobj = new PricingMatrix();

            RecordRef Currency = new RecordRef();
            Currency.internalId = objSetting.Currency_Netsuite_Id.ToString();
            //Currency.typeSpecified = true;
            // Currency.type = RecordType.account;

            RecordRef PriceLevel = new RecordRef();
            PriceLevel.internalId = objSetting.Price_Level_Netsuite_Id.ToString();
            PriceLevel.typeSpecified = true;

            PriceLevel.type = RecordType.priceLevel;



            Price objprice = new Price();
            objprice.value = price;//Double.Parse(obj_ItemPrice.Price.ToString());
            objprice.quantity = 1;//Double.Parse("1");
                                  // objprice.value = Double.Parse(obj_ItemPrice.Price.ToString());
                                  // objprice.quantity = Double.Parse(obj_ItemPrice.Price.ToString());
            objprice.quantitySpecified = false;
            objprice.valueSpecified = true;

            pricelst[0] = objprice;



            Pricing objPricing = new Pricing();
            objPricing.currency = Currency;
            objPricing.priceLevel = PriceLevel;
            objPricing.priceList = pricelst;
            objPricing.discount = double.NaN;//0;
            objPricing.discountSpecified = false;


            Pricinglst[0] = objPricing;
            //NewItemObject.taxSchedule = Tax_Schedule;


            //PricingMatrixobj.pricing = new Pricing();

            PricingMatrixobj.pricing = Pricinglst;
            //PricingMatrixobj.replaceAll = false;

            return PricingMatrixobj;


        }
    }
}
