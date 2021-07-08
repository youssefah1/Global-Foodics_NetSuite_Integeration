using NetSuiteIntegeration.com.netsuite.webservices;
using System;
using System.Collections.Generic;
using System.Linq;
using Foodics.NetSuite.Shared.DAO;


using Foodics.NetSuite.Shared.Model;
using System.Data;
using Foodics.NetSuite.Shared;
using Invoice = NetSuiteIntegeration.com.netsuite.webservices.Invoice;
using InvoiceItem = NetSuiteIntegeration.com.netsuite.webservices.InvoiceItem;
using CreditMemo = NetSuiteIntegeration.com.netsuite.webservices.CreditMemo;

namespace NetSuiteIntegeration.Tasks
{

    /// <summary>
    /// /Post invoice return to Netsuite
    /// including receipt items & credit memo
    /// </summary>
    public class CreditMemoTask : NetSuiteBaseIntegration
    {
        public override void Get()
        {
        }

        public override Int64 Set(string parametersArr)
        {
            {
                try
                {
                    List<Foodics.NetSuite.Shared.Model.Invoice> invoiceLst = new CustomDAO().SelectInvoice(5).Take(200).ToList();
                    if (invoiceLst.Count > 0)
                        CreateCreditMemo(invoiceLst);
                }
                catch (Exception ex)
                {
                    LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
                    return 0;
                }

                return 1;
            }
        }
        private WriteResponseList CreateCreditMemo(List<Foodics.NetSuite.Shared.Model.Invoice> returnList)
        {

            //Define Array of credit Memo Then Loop
            CreditMemo[] memoList = new CreditMemo[returnList.Count];
            CreditMemoItem[] memoitemarr;
            CreditMemoItem invoiceItemObject;
            //add return id + return netsuite id to credit memo table, credit memo netsuite id
            #region Add Credit Memoes to CreditMemo Array

            for (int i = 0; i < returnList.Count; i++)
            {
                Foodics.NetSuite.Shared.Model.Invoice invoiceReturn = returnList[i];
                Foodics.NetSuite.Shared.Model.Invoice invoiceoriginal = new GenericeDAO<Foodics.NetSuite.Shared.Model.Invoice>().GetWhere(" Foodics_Id = '" + invoiceReturn.Original_Foodics_Id + "'").FirstOrDefault();
                if (invoiceoriginal != null && invoiceoriginal.Netsuite_Id > 0)
                {
                    Setting objSetting = new GenericeDAO<Setting>().GetWhere("Subsidiary_Netsuite_Id=" + invoiceReturn.Subsidiary_Id).FirstOrDefault();
                    StringCustomFieldRef FoodicsRef, FoodicsNumb, orderDiscount;
                    CustomFieldRef[] customFieldRefArray;
                    CreditMemo memo = new CreditMemo();
                    // Return
                    RecordRef returnRef = new RecordRef();
                    returnRef.internalId = invoiceoriginal.Netsuite_Id.ToString();
                    returnRef.type = RecordType.invoice;
                    memo.createdFrom = returnRef;


                    //Customer

                    RecordRef entity = new RecordRef();
                    entity.internalId = invoiceReturn.Customer_Netsuite_Id > 0 ? invoiceReturn.Customer_Netsuite_Id.ToString() : objSetting.Customer_Netsuite_Id.ToString();
                    entity.type = RecordType.customer;
                    memo.entity = entity;

                    //currency
                    RecordRef currency = new RecordRef();
                    currency.internalId = objSetting.Currency_Netsuite_Id.ToString();
                    currency.type = RecordType.currency;
                    memo.currency = currency;

                    //date
                    memo.tranDateSpecified = true;
                    memo.tranDate = TimeZoneInfo.ConvertTimeToUtc(invoiceReturn.Date, TimeZoneInfo.Local);

                    //exchange rate
                    memo.exchangeRate = invoiceReturn.Exchange_Rate;

                    //subsidary
                    RecordRef subsid = new RecordRef();
                    subsid.internalId = invoiceReturn.Subsidiary_Id.ToString();
                    subsid.type = RecordType.subsidiary;
                    memo.subsidiary = subsid;


                    #region Item List
                    List<Foodics.NetSuite.Shared.Model.InvoiceItem> itemLst = new GenericeDAO<Foodics.NetSuite.Shared.Model.InvoiceItem>().GetWhere(" ProductStatus =6 and Invoice_Id =" + invoiceReturn.Id + " and isnull(Item_Id,0) >0 ");
                    int DiscountItems = itemLst.Where(x => x.Line_Discount_Amount > 0).Count();
                    //Define Invoice Items List
                    int totalItems = itemLst.Count + DiscountItems;
                    memoitemarr = new CreditMemoItem[totalItems];
                    if (itemLst.Count > 0)
                    {
                        CreditMemoItemList crdtmemoitmlst = new CreditMemoItemList();

                        try
                        {
                            int arr = 0;
                            for (int k = 0; k < totalItems; k++)
                            {
                                Foodics.NetSuite.Shared.Model.InvoiceItem itemDetails = itemLst[arr];
                                invoiceItemObject = CreateCreditItem(objSetting, itemDetails);
                                memoitemarr[k] = invoiceItemObject;
                                if (itemDetails.Line_Discount_Amount > 0)
                                {
                                    float Discount = itemDetails.Line_Discount_Amount;
                                    k++;
                                    Foodics.NetSuite.Shared.Model.InvoiceItem OtherCharge = new Foodics.NetSuite.Shared.Model.InvoiceItem();
                                    OtherCharge.Item_Id = objSetting.OtherChargeItem_Netsuite_Id;
                                    OtherCharge.Amount = Discount * -1;
                                    OtherCharge.Quantity = 1;
                                    OtherCharge.Item_Type = "OtherChargeResaleItem";
                                    invoiceItemObject = CreateCreditItem(objSetting, OtherCharge);
                                    memoitemarr[k] = invoiceItemObject;
                                }
                                arr++;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
                        }


                        crdtmemoitmlst.item = memoitemarr;
                        memo.itemList = crdtmemoitmlst;
                    }

                    #endregion

                    #region Discount
                    if (invoiceReturn.Total_Discount > 0)
                    {
                        RecordRef discountitem = new RecordRef();
                        discountitem.type = RecordType.discountItem;
                        memo.discountItem = discountitem;

                        memo.discountRate = (Math.Round((invoiceReturn.Total_Discount / 1.15), 3) * -1).ToString();
                        if (invoiceReturn.Discount_Id > 0)
                            discountitem.internalId = invoiceReturn.Discount_Id.ToString();
                        else
                            discountitem.internalId = objSetting.DiscountItem_Netsuite_Id.ToString();
                    }
                    else
                        memo.discountRate = "0";
                    if (invoiceReturn.Accounting_Discount_Item != 0)
                    {
                        RecordRef discItem = new RecordRef();
                        discItem.internalId = invoiceReturn.Accounting_Discount_Item.ToString();
                        discItem.type = RecordType.discountItem;
                        memo.discountItem = discItem;
                    }
                    #region Custom Attributes
                    orderDiscount = new StringCustomFieldRef();
                    orderDiscount.scriptId = "custbody_da_invoice_discount";
                    orderDiscount.value = invoiceReturn.Total_Discount.ToString();


                    FoodicsRef = new StringCustomFieldRef();
                    FoodicsRef.scriptId = "custbody_da_foodics_reference";
                    FoodicsRef.value = invoiceReturn.BarCode.ToString();

                    FoodicsNumb = new StringCustomFieldRef();
                    FoodicsNumb.scriptId = "custbody_da_foodics_number";
                    FoodicsNumb.value = invoiceReturn.Number.ToString();

                    customFieldRefArray = new CustomFieldRef[3];
                    customFieldRefArray[0] = orderDiscount;
                    customFieldRefArray[1] = FoodicsRef;
                    customFieldRefArray[2] = FoodicsNumb;

                    memo.customFieldList = customFieldRefArray;
                    #endregion

                    #endregion


                    memoList[i] = memo;
                }

            }

            //Post Memos to Netsuite
            WriteResponseList memoWR = Service(true).addList(memoList);
            bool receiptresult = memoWR.status.isSuccess;

            if (receiptresult)
            {

                UpdatedInvoice(returnList, memoWR);
            }
            #endregion
            return memoWR;
        }

        private CreditMemoItem CreateCreditItem(Setting objSetting, Foodics.NetSuite.Shared.Model.InvoiceItem itemDetails)
        {
            CreditMemoItem invoiceItemObject = new CreditMemoItem();

            // tax code
            RecordRef taxCode = new RecordRef();
            if (itemDetails.FoodicsTax > 0 && itemDetails.Item_Type != nameof(Item_Type.OtherChargeSaleItem))
                taxCode.internalId = objSetting.TaxCode_Netsuite_Id.ToString();
            else
                taxCode.internalId = objSetting.TaxCode_Free_Netsuite_Id.ToString();

            //taxCode.internalId = itemDetails.FoodicsTax > 0 ? objSetting.TaxCode_Netsuite_Id.ToString() : objSetting.TaxCode_Free_Netsuite_Id.ToString();
            taxCode.type = RecordType.taxAcct;
            if (int.Parse(taxCode.internalId) > 0)
                invoiceItemObject.taxCode = taxCode;

            // item
            RecordRef item = new RecordRef();
            item.internalId = itemDetails.Item_Id.ToString();
            item.type = (RecordType)Enum.Parse(typeof(RecordType), itemDetails.Item_Type, true);
            invoiceItemObject.item = item;

            if (Utility.ConvertToInt(itemDetails.Units) > 0)
            {
                RecordRef unit = new RecordRef();
                unit.internalId = itemDetails.Units.ToString();
                unit.type = RecordType.unitsType;
                invoiceItemObject.units = unit;
            }

            // price level
            #region price level
            RecordRef price = new RecordRef();
            price.type = RecordType.priceLevel;
            price.internalId = "-1";
            invoiceItemObject.price = price;
            float taxRate = 1 + (objSetting.TaxRate / 100);
            if (itemDetails.FoodicsTax > 0 && itemDetails.Item_Type != nameof(Item_Type.OtherChargeSaleItem))
            {
                if (objSetting.TaxApplied)//= tax inclusive in item price
                    invoiceItemObject.rate = Convert.ToString(itemDetails.Amount / taxRate);
                else
                    invoiceItemObject.rate = Convert.ToString(itemDetails.Amount);
            }
            else
            {
                invoiceItemObject.rate = Convert.ToString(itemDetails.Amount);
            }
            #endregion
            invoiceItemObject.quantitySpecified = true;
            invoiceItemObject.quantity = itemDetails.Quantity;
            return invoiceItemObject;
        }

        private void UpdatedInvoice(List<Foodics.NetSuite.Shared.Model.Invoice> InvoiceLst, WriteResponseList responseLst)
        {
            try
            {
                //Tuple to hold local invoice ids and its corresponding Netsuite ids
                List<Tuple<int, string>> iDs = new List<Tuple<int, string>>();
                //loop to fill tuple values
                for (int counter = 0; counter < InvoiceLst.Count; counter++)
                {
                    //ensure that invoice is added to netsuite
                    if (responseLst.writeResponse[counter].status.isSuccess)
                    {
                        try
                        {
                            RecordRef rf = (RecordRef)responseLst.writeResponse[counter].baseRef;
                            iDs.Add(new Tuple<int, string>(Convert.ToInt32(rf.internalId.ToString()), InvoiceLst[counter].Foodics_Id));
                        }
                        catch (Exception ex)
                        {
                            LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
                        }
                    }
                }
                GenericeDAO<Foodics.NetSuite.Shared.Model.Invoice> objDAO = new GenericeDAO<Foodics.NetSuite.Shared.Model.Invoice>();
                objDAO.UpdateNetsuiteIDs(iDs, "Invoice");
            }
            catch (Exception ex)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
            }
        }



    }
}