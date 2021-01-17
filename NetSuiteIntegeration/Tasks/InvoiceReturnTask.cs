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
    public class InvoiceReturnTask : NetSuiteBaseIntegration
    {
        public override void Get()
        {
        }

        public override Int64 Set(string parametersArr)
        {
            {
                try
                {
                    List<Foodics.NetSuite.Shared.Model.Invoice> invoiceLst = new CustomDAO().SelectInvoice(5);
                    if (invoiceLst.Count > 0)
                        CreateCreditMemo(invoiceLst);
                }
                catch
                {
                    return 0;
                }

                return 1;
            }
        }
        private WriteResponseList CreateCreditMemo(List<Foodics.NetSuite.Shared.Model.Invoice> returnList)
        {

            //Define Array of credit Memo Then Loop
            CreditMemo[] memoList = new CreditMemo[returnList.Count];

            //add return id + return netsuite id to credit memo table, credit memo netsuite id
            #region Add Credit Memoes to CreditMemo Array

            for (int i = 0; i < returnList.Count; i++)
            {
                Foodics.NetSuite.Shared.Model.Invoice invoiceReturn = returnList[i];
                Foodics.NetSuite.Shared.Model.Invoice invoiceoriginal = new GenericeDAO<Foodics.NetSuite.Shared.Model.Invoice>().GetWhere(" Foodics_Id = '" + invoiceReturn.Original_Foodics_Id + "'").FirstOrDefault();
                if (invoiceoriginal.Netsuite_Id > 0)
                {
                    Setting objSetting = new GenericeDAO<Setting>().GetWhere("Subsidiary_Netsuite_Id=" + invoiceReturn.Subsidiary_Id).FirstOrDefault();

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
                    List<Foodics.NetSuite.Shared.Model.InvoiceItem> itemLst = new GenericeDAO<Foodics.NetSuite.Shared.Model.InvoiceItem>().GetWhere("Invoice_Id =" + invoiceReturn.Id + " and isnull(Item_Id,0) >0 ");
                    if (itemLst.Count > 0)
                    {
                        CreditMemoItemList crdtmemoitmlst = new CreditMemoItemList();
                        CreditMemoItem[] memoitemarr = new CreditMemoItem[itemLst.Count];

                        try
                        {
                            for (int k = 0; k < itemLst.Count; k++)
                            {
                                Foodics.NetSuite.Shared.Model.InvoiceItem itemDetails = itemLst[k];
                                CreditMemoItem invoiceItemObject = new CreditMemoItem();

                                // tax code
                                RecordRef taxCode = new RecordRef();
                                taxCode.internalId = objSetting.TaxCode_Netsuite_Id.ToString();
                                taxCode.type = RecordType.taxAcct;
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
                                invoiceItemObject.rate = Convert.ToString(itemDetails.Amount / 1.15);
                                #endregion
                                invoiceItemObject.quantitySpecified = true;
                                invoiceItemObject.quantity = itemDetails.Quantity;


                                memoitemarr[k] = invoiceItemObject;
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
                            //update netsuiteId property
                            InvoiceLst[counter].Netsuite_Id = Convert.ToInt32(rf.internalId.ToString());
                            //add item to the tuple
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