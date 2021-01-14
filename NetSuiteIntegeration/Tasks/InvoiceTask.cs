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

namespace NetSuiteIntegeration.Tasks
{


    public class InvoiceTask : NetSuiteBaseIntegration
    {
        public override void Get()
        {
            /// <summary> This method get all items (with types we need in POS) from netsuite and check item type, 
            /// after that get all item info and save in DB.</summary>	

        }
        public override Int64 Set(string parametersArr)
        {
            try
            {

                new CustomDAO().UpdateInvoiceItem();

                List<Foodics.NetSuite.Shared.Model.Invoice> invoiceLst = new CustomDAO().SelectInvoice(4);
                bool result = true;
                if (invoiceLst.Count > 0)
                {
                    #region variables
                    Invoice[] InvoiceArr = new Invoice[invoiceLst.Count];

                    List<Foodics.NetSuite.Shared.Model.InvoiceItem> itemLst;

                    Foodics.NetSuite.Shared.Model.Invoice invoice_info;
                    Foodics.NetSuite.Shared.Model.InvoiceItem itemDetails;

                    InvoiceItem[] invoiceItems;
                    DateTime invoice_date;
                    Invoice invoiceObject;
                    InvoiceItem invoiceItemObject;
                    InvoiceItemList items;

                    RecordRef taxCode, item, unit, price, 
                              subsid, currency, entity, location;
                    StringCustomFieldRef  FoodicsRef, FoodicsNumb;
                    CustomFieldRef[] customFieldRefArray;
                    RecordRef myCustomForm = new RecordRef();
                    myCustomForm.type = RecordType.invoice;
                    myCustomForm.typeSpecified = true;
                    myCustomForm.internalId = "196";
                    #endregion
                    for (int i = 0; i < invoiceLst.Count; i++)
                    {
                        try
                        {
                            invoice_info = invoiceLst[i];
                            Setting objSetting = new GenericeDAO<Setting>().GetWhere("Subsidiary_Netsuite_Id=" + invoice_info.Subsidiary_Id).FirstOrDefault();
                            invoiceObject = new Invoice();
                            invoiceObject.customForm = myCustomForm;
                            //get invoice items
                            itemLst = new GenericeDAO<Foodics.NetSuite.Shared.Model.InvoiceItem>().GetWhere("Invoice_Id =" + invoice_info.Id+ " and isnull(Item_Id,0) >0 ");
                            #region invoice items
                            //Define Invoice Items List
                            invoiceItems = new InvoiceItem[itemLst.Count];
                            try
                            {
                                for (int k = 0; k < itemLst.Count; k++)
                                {
                                    itemDetails = itemLst[k];
                                    invoiceItemObject = new InvoiceItem();
                                    // tax code
                                    taxCode = new RecordRef();
                                    taxCode.internalId = objSetting.TaxCode_Netsuite_Id.ToString();
                                    taxCode.type = RecordType.taxAcct;
                                    invoiceItemObject.taxCode = taxCode;
                                    // item
                                    item = new RecordRef();
                                    item.internalId = itemDetails.Item_Id.ToString();
                                    item.type = (RecordType)Enum.Parse(typeof(RecordType), itemDetails.Item_Type, true);
                                    invoiceItemObject.item = item;
                                    if (Utility.ConvertToInt(itemDetails.Units) > 0)
                                    {
                                        unit = new RecordRef();
                                        unit.internalId = itemDetails.Units.ToString();
                                        unit.type = RecordType.unitsType;
                                        invoiceItemObject.units = unit;
                                    }
                                    // price level
                                    #region price level
                                    price = new RecordRef();
                                    price.type = RecordType.priceLevel;
                                    price.internalId = "-1";
                                    invoiceItemObject.price = price;
                                    invoiceItemObject.rate = Convert.ToString(itemDetails.Amount / 1.15);
                                    #endregion
                                    invoiceItemObject.quantitySpecified = true;
                                    invoiceItemObject.quantity = itemDetails.Quantity;
                                    invoiceItems[k] = invoiceItemObject;
                                }
                            }
                            catch (Exception ex)
                            {
                                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
                            }
                            //Assign invoive items
                            items = new InvoiceItemList();
                            items.item = invoiceItems;
                            invoiceObject.itemList = items;
                            #endregion
                            #region Standard Attributes
                            invoice_date = TimeZoneInfo.ConvertTimeToUtc(invoice_info.Date, TimeZoneInfo.Local);

                            invoiceObject.tranDateSpecified = true;
                            invoiceObject.dueDateSpecified = true;
                            invoiceObject.tranDate = invoice_date;
                            invoiceObject.dueDate = invoice_date;
                            invoiceObject.exchangeRate = invoice_info.Exchange_Rate;
                            invoiceObject.memo = invoice_info.Notes;
                            if (invoice_info.Subsidiary_Id > 0)
                            {
                                subsid = new RecordRef();
                                subsid.internalId = objSetting.Subsidiary_Netsuite_Id.ToString();
                                subsid.type = RecordType.subsidiary;
                                invoiceObject.subsidiary = subsid;
                            }
                            currency = new RecordRef();
                            currency.internalId = objSetting.Currency_Netsuite_Id.ToString();
                            currency.type = RecordType.currency;
                            invoiceObject.currency = currency;
                            entity = new RecordRef();
                            entity.internalId = invoice_info.Customer_Netsuite_Id > 0 ? invoice_info.Customer_Netsuite_Id.ToString() : objSetting.Customer_Netsuite_Id.ToString();
                            entity.type = RecordType.customer;
                            invoiceObject.entity = entity;

                            location = new RecordRef();
                            location.internalId = invoice_info.Location_Id.ToString(); //objSetting.Location_Netsuite_Id.ToString();
                            location.type = RecordType.location;
                            invoiceObject.location = location;
                            
                            if (invoice_info.Sales_Rep_Id > 0)
                            {
                                RecordRef sales_rep = new RecordRef();
                                sales_rep.internalId = invoice_info.Sales_Rep_Id.ToString();
                                sales_rep.type = RecordType.employee;
                                invoiceObject.salesRep = sales_rep;
                            }
                            #endregion

                            #region Discount
                            invoiceObject.discountRate = (Math.Round(invoice_info.Total_Discount, 3) * -1).ToString();
                            #endregion

                            #region Invoice Custom Attributes
                            FoodicsRef = new StringCustomFieldRef();
                            FoodicsRef.scriptId = "custbody_da_foodics_reference";
                            FoodicsRef.value = invoice_info.BarCode.ToString();

                            FoodicsNumb = new StringCustomFieldRef();
                            FoodicsNumb.scriptId = "custbody_da_foodics_number";
                            FoodicsNumb.value = invoice_info.Number.ToString();

                            customFieldRefArray = new CustomFieldRef[2];
                            customFieldRefArray[0] = FoodicsRef;
                            customFieldRefArray[1] = FoodicsNumb;
                            
                            invoiceObject.customFieldList = customFieldRefArray;
                            #endregion
                            InvoiceArr[i] = invoiceObject;
                        }
                        catch (Exception ex)
                        {
                            invoiceLst.RemoveAt(i);
                            LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
                        }
                    }
                    // Send invoice list to netsuite
                    WriteResponseList wr = Service(true).addList(InvoiceArr);
                    result = wr.status.isSuccess;
                    if (result)
                    {
                        //Update database with returned Netsuite ids
                        UpdatedInvoice(invoiceLst, wr);
                    }
                }
            }
            catch (Exception ex)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
            }



            return 0;
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
                // NetsuiteDAO objDAO = new NetsuiteDAO();
                //updates local db
                // LogDAO.Integration_Exception(LogIntegrationType.Info, TaskRunType.POST, "InvoiceTask UpdateDB", "Updating " + iDs.Count().ToString() + " from " + InvoiceLst.Count().ToString());

                //objDAO.UpdateNetsuiteIDs(iDs, "Invoice");

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
