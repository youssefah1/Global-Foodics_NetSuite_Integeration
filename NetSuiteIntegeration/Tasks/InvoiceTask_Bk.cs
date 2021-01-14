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
        //public InvoiceTask()
        //{
        //    taskType = Shared.TaskType.Invoice;
        //}

        public override void Get()
        {
            /// <summary> This method get all items (with types we need in POS) from netsuite and check item type, 
            /// after that get all item info and save in DB.</summary>	

        }
        private RecordType GetRecordType(string recType)
        {
            RecordType rType = new RecordType();
            switch (recType)
            {
                case "InventoryItem":
                    rType = RecordType.inventoryItem;
                    break;
                case "LotNumberedInventoryItem":
                    rType = RecordType.lotNumberedInventoryItem;
                    break;
                case "SerializedInventoryItem":
                    rType = RecordType.serializedInventoryItem;
                    break;
                case "KitItem":
                    rType = RecordType.kitItem;
                    break;
                case "ItemGroup":
                    rType = RecordType.itemGroup;
                    break;
                case "AssemblyItem":
                    rType = RecordType.assemblyItem;
                    break;
                case "LotNumberedAssemblyItem":
                    rType = RecordType.lotNumberedAssemblyItem;
                    break;
                case "SerializedAssemblyItem":
                    rType = RecordType.serializedAssemblyItem;
                    break;
                default:
                    break;
            }
            return rType;

        }

        public override Int64 Set(string parametersArr)
        {
            try
            {


                //Set Values of Netsuite Customer ID
                //  LogDAO.Integration_Exception(LogIntegrationType.Info, TaskRunType.POST, "InvoiceTask", "Start");

                //new GenericeDAO<Foodics.NetSuite.Shared.Model.Invoice.Integrate>().GetAndUpdateCustomerIDandCreditMemo();
                //get recentrly added invoices after creating the return
                List<Foodics.NetSuite.Shared.Model.Invoice> invoiceLst = new GenericeDAO<Foodics.NetSuite.Shared.Model.Invoice>().GetWhere(" Netsuite_Id IS NULL or Netsuite_Id =0");
                Setting objSetting = new GenericeDAO<Setting>().GetAll().FirstOrDefault();
                // = new NetsuiteDAO().SelectInvoicesForIntegration();
                //  LogDAO.Integration_Exception(LogIntegrationType.Info, TaskRunType.POST, "InvoiceTask", "Count: " + invoiceLst.Count.ToString());
                bool result = true;
                if (invoiceLst.Count > 0)
                {
                    #region variables
                    Invoice[] InvoiceArr = new Invoice[invoiceLst.Count];

                    List<Foodics.NetSuite.Shared.Model.InvoiceItem> itemLst;
                    List<Foodics.NetSuite.Shared.Model.GiftCertificate.Integrate> giftCertificateLst;
                    //List<Foodics.NetSuite.Shared.Model.ItemLotSerial.Sales> lotSerialLst;

                    Foodics.NetSuite.Shared.Model.Invoice invoice_info;
                    Foodics.NetSuite.Shared.Model.InvoiceItem itemDetails;

                    GiftCertRedemption[] giftRedeem;
                    InvoiceItem[] invoiceItems;
                    
                    DateTime invoice_date;
                    Invoice invoiceObject;
                    InvoiceItem invoiceItemObject;
                    InvoiceItemList items;
                    InventoryAssignmentList InventoryAssignmentlst;
                    InventoryAssignment[] assignList;
                    InventoryAssignment assign;
                    InventoryDetail invDetails;

                    GiftCertRedemptionList redemptionLst;

                    DateCustomFieldRef trans_time;
                    ListOrRecordRef emirate_region, custSelectValue, custSelectCashier;
                    SelectCustomFieldRef emirate_ref = null, terminal, cashier;
                    RecordRef authCodeRef, taxCode, item, unit, price, itmSerial, discItem,
                              subsid, currency, entity, location, classification, department;
                    DoubleCustomFieldRef line_total, line_discount_amount, balance, paid, lineDiscount;
                    StringCustomFieldRef trans_no, pos_id, orderDiscount;

                    CustomFieldRef[] customFieldRefArr, customFieldRefArray;

                    int item_custom_cols = 2;
                    #endregion

                    for (int i = 0; i < invoiceLst.Count; i++)
                    {
                        try
                        {
                            invoice_info = invoiceLst[i];
                            //Netsuite invoice type
                            invoiceObject = new Invoice();

                            //get invoice items
                            itemLst = new GenericeDAO<Foodics.NetSuite.Shared.Model.InvoiceItem>().GetWhere("Invoice_Id ="+ invoice_info.Id);//new NetsuiteDAO().SelectInvoicItems(invoice_info.Id);

                            //get invoice gift certificates
                            #region gift certificate redemption
                            //try
                            //{
                            //    giftCertificateLst = new NetsuiteDAO().SelectEntityGiftCertificates(invoice_info.Id, 1);
                            //    if (giftCertificateLst.Count > 0)
                            //    {
                            //        redemptionLst = new GiftCertRedemptionList();
                            //        giftRedeem = new GiftCertRedemption[giftCertificateLst.Count];
                            //        for (int r = 0; r < giftCertificateLst.Count; r++)
                            //        {
                            //            GiftCertRedemption gift = new GiftCertRedemption();
                            //            authCodeRef = new RecordRef();
                            //            authCodeRef.internalId = giftCertificateLst[r].Netsuite_Id.ToString();
                            //            authCodeRef.type = RecordType.giftCertificate;
                            //            gift.authCode = authCodeRef;
                            //            gift.authCodeAppliedSpecified = true;
                            //            gift.authCodeApplied = Convert.ToDouble(giftCertificateLst[r].Amount);
                            //            giftRedeem[r] = gift;
                            //        }

                            //        redemptionLst.giftCertRedemption = giftRedeem;
                            //        invoiceObject.giftCertRedemptionList = redemptionLst;
                            //    }
                            //}
                            //catch (Exception ex)
                            //{
                            //    //LogDAO.Integration_Exception(LogIntegrationType.Error, TaskRunType.POST, "InvoiceTask", "Error adding GiftCertificates: " + ex.Message + " Invoice id = " + invoiceLst[i].Id.ToString());
                            //}
                            #endregion

                            #region invoice items

                            //Define Invoice Items List
                            invoiceItems = new InvoiceItem[itemLst.Count];
                            try
                            {
                                item_custom_cols = 2;
                                for (int k = 0; k < itemLst.Count; k++)
                                {
                                    itemDetails = itemLst[k];
                                    invoiceItemObject = new InvoiceItem();

                                    // TAX
                                    //invoiceItemObject.taxRate1Specified = true;
                                    //invoiceItemObject.taxRate1 = itemDetails.Tax_Rate;
                                    //invoiceItemObject.taxAmount = itemDetails.Tax_Amt;

                                    // tax code
                                    taxCode = new RecordRef();
                                    taxCode.internalId = objSetting.TaxCode_Netsuite_Id.ToString(); //"15";//itemDetails.Tax_Code.ToString();
                                    taxCode.type = RecordType.taxAcct;
                                    invoiceItemObject.taxCode = taxCode;

                                    // item
                                    item = new RecordRef();
                                    item.internalId = itemDetails.Item_Id.ToString();
                                    item.type = (RecordType)Enum.Parse(typeof(RecordType), itemDetails.Item_Type, true);
                                    //item.type = (RecordType)Enum.Parse(typeof(RecordType), "InventoryItem", true);
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

                                    if (itemDetails.Amount > 0)
                                    {
                                        //if (itemDetails.Customer_Price_Level > 0)
                                        //{
                                        //    // customer price level
                                        //    price.internalId = itemDetails.Customer_Price_Level.ToString();
                                        //    invoiceItemObject.price = price;
                                        //}
                                        //else if (itemDetails.Price_Level_Id > 0)
                                        //{
                                        //    // default price level
                                        //    price.internalId = itemDetails.Price_Level_Id.ToString();
                                        //    invoiceItemObject.price = price;
                                        //}
                                        //else
                                        //{
                                            // amount
                                            invoiceItemObject.amountSpecified = true;
                                            invoiceItemObject.amount = itemDetails.Amount;
                                        //}
                                    }
                                    
                                    #endregion

                                   // invoiceItemObject.costEstimateType = (ItemCostEstimateType)itemDetails.Cost_Estimate_Type;

                                    
                                    //if (itemDetails.Item_Type == "GiftCertificateItem")
                                    //{
                                    //    #region sell gift certificate
                                    //    try
                                    //    {
                                    //        invoiceItemObject.giftCertNumber = itemDetails.Gift_Code;
                                    //        invoiceItemObject.giftCertFrom = itemDetails.Sender;
                                    //        invoiceItemObject.giftCertMessage = itemDetails.Gift_Message;
                                    //        invoiceItemObject.giftCertRecipientName = itemDetails.Recipient_Name;
                                    //        invoiceItemObject.giftCertRecipientEmail = itemDetails.Recipient_Email;
                                    //    }
                                    //    catch { }
                                    //    #endregion
                                    //}
                                    //else
                                    //{
                                        // quantity
                                        invoiceItemObject.quantitySpecified = true;
                                        invoiceItemObject.quantity = itemDetails.Quantity;

                                        #region serials/Lot
                                        //if (Utility.ItemTypeSerialized.Contains(itemDetails.Item_Type))
                                        //{
                                        //    lotSerialLst = new NetsuiteDAO().SelectInvoiceLotSerials(itemDetails.Id);
                                        //    InventoryAssignmentlst = new InventoryAssignmentList();
                                        //    assignList = new InventoryAssignment[lotSerialLst.Count];

                                        //    for (int z = 0; z < lotSerialLst.Count; z++)
                                        //    {
                                        //        assign = new InventoryAssignment();

                                        //        itmSerial = new RecordRef();
                                        //        itmSerial.internalId = lotSerialLst[z].Netsuite_Id.ToString();
                                        //        itmSerial.type = RecordType.lotNumberedInventoryItem;
                                        //        itmSerial.typeSpecified = true;

                                        //        if (Utility.ItemTypeLot.Contains(itemDetails.Item_Type))
                                        //        {
                                        //            assign.quantitySpecified = true;
                                        //            assign.quantity = lotSerialLst[z].Quantity;
                                        //        }
                                        //        assign.issueInventoryNumber = itmSerial;

                                        //        assignList[z] = assign;
                                        //    }

                                        //    InventoryAssignmentlst.inventoryAssignment = assignList;
                                        //    invDetails = new InventoryDetail();
                                        //    invDetails.inventoryAssignmentList = InventoryAssignmentlst;

                                        //    invoiceItemObject.inventoryDetail = invDetails;
                                        //}

                                        #endregion
                                  //  }

                                    #region custom fields

                                    try
                                    {
                                        #region line-item discount

                                        //line_total = new DoubleCustomFieldRef();
                                        //line_total.scriptId = "custcol_da_pos_line_item_total";
                                        //line_total.value = Math.Round(itemDetails.Total_Line_Amount, 3);

                                        //line_discount_amount = new DoubleCustomFieldRef();
                                        //line_discount_amount.scriptId = "custcol_da_pos_line_item_discount";
                                        //line_discount_amount.value = Math.Round(itemDetails.Line_Discount_Amount, 3);

                                        // line-item amount
                                        invoiceItemObject.amount = (itemDetails.Quantity * itemDetails.Amount);

                                        #endregion

                                        #region Transaction Region (Emirate)
                                        //item_custom_cols = 2;
                                        //if (invoice_info.Transaction_Region > 0)
                                        //{
                                        //    item_custom_cols = 3;
                                        //    emirate_region = new ListOrRecordRef();
                                        //    emirate_region.internalId = invoice_info.Transaction_Region.ToString();

                                        //    emirate_ref = new SelectCustomFieldRef();
                                        //    emirate_ref.scriptId = "custcol_emirate";
                                        //    emirate_ref.value = emirate_region;
                                        //}
                                        #endregion

                                        //customFieldRefArr = new CustomFieldRef[item_custom_cols];
                                        //customFieldRefArr[0] = line_total;
                                        //customFieldRefArr[1] = line_discount_amount;

                                        //if (item_custom_cols == 3)
                                        //    customFieldRefArr[2] = emirate_ref;

                                        //invoiceItemObject.customFieldList = customFieldRefArr;
                                    }
                                    catch (Exception ex)
                                    {
                                      //  LogDAO.Integration_Exception(LogIntegrationType.Error, TaskRunType.POST, "InvoiceTask", "Error adding customField: " + ex.Message + " Invoice id = " + invoiceLst[i].Id.ToString());
                                    }
                                    #endregion

                                    invoiceItems[k] = invoiceItemObject;
                                }
                            }
                            catch (Exception ex)
                            {
                              //  LogDAO.Integration_Exception(LogIntegrationType.Error, TaskRunType.POST, "InvoiceTask", "Error adding invoice: " + ex.Message + " Invoice id = " + invoiceLst[i].Id.ToString());
                            }
                            //Assign invoive items
                            items = new InvoiceItemList();
                            items.item = invoiceItems;
                            invoiceObject.itemList = items;

                            //GiftCertRedemption
                            #endregion

                            #region Invoice Values

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

                            //RecordRef customForm = new RecordRef();
                            //customForm.internalId = "92";
                            //invoiceObject.customForm = customForm;

                            currency = new RecordRef();
                            currency.internalId = objSetting.Currency_Netsuite_Id.ToString();
                            currency.type = RecordType.currency;
                            invoiceObject.currency = currency;

                            entity = new RecordRef();

                            entity.internalId = invoice_info.Customer_Netsuite_Id > 0? invoice_info.Customer_Netsuite_Id.ToString(): objSetting.Customer_Netsuite_Id.ToString();
                            entity.type = RecordType.customer;
                            invoiceObject.entity = entity;

                            location = new RecordRef();
                            location.internalId = invoice_info.Location_Id.ToString(); //objSetting.Location_Netsuite_Id.ToString();
                            location.type = RecordType.location;
                            invoiceObject.location = location;

                            // department
                            //if (invoice_info.Department_Id > 0)
                            //{
                            //    department = new RecordRef();
                            //    department.internalId = invoice_info.Department_Id.ToString();
                            //    department.type = RecordType.department;
                            //    invoiceObject.department = department;
                            //}

                            //// class
                            //if (invoice_info.Class_Id > 0)
                            //{
                            //    classification = new RecordRef();
                            //    classification.internalId = invoice_info.Class_Id.ToString();
                            //    classification.type = RecordType.classification;
                            //    invoiceObject.@class = classification;
                            //}
                            // sales rep
                            if (invoice_info.Sales_Rep_Id > 0)
                            {
                                RecordRef sales_rep = new RecordRef();
                                sales_rep.internalId = invoice_info.Sales_Rep_Id.ToString();
                                sales_rep.type = RecordType.employee;
                                invoiceObject.salesRep = sales_rep;
                            }
                            #endregion

                            #region Invoice Custom Attributes

                            trans_no = new StringCustomFieldRef();
                            trans_no.scriptId = "custbody_da_pos_trans_no";
                            trans_no.value = invoice_info.BarCode.ToString();

                            pos_id = new StringCustomFieldRef();
                            pos_id.scriptId = "custbody_da_pos_id";
                            pos_id.value = invoice_info.Id.ToString();

                            custSelectValue = new ListOrRecordRef();

                            terminal = new SelectCustomFieldRef();
                            custSelectValue.internalId = invoice_info.Terminal_Id.ToString();
                            terminal.scriptId = "custbody_da_terminal";
                            terminal.value = custSelectValue;

                            custSelectCashier = new ListOrRecordRef();

                            cashier = new SelectCustomFieldRef();
                            custSelectCashier.internalId = invoice_info.Cashier.ToString();
                            cashier.scriptId = "custbody_da_cashier";
                            cashier.value = custSelectCashier;

                            balance = new DoubleCustomFieldRef();
                            balance.scriptId = "custbody_da_balance";
                            balance.value = Math.Round(invoice_info.Balance, 3);


                            paid = new DoubleCustomFieldRef();
                            paid.scriptId = "custbody_da_paid";
                            paid.value = Math.Round(invoice_info.Paid, 3);

                            trans_time = new DateCustomFieldRef();
                            trans_time.scriptId = "custbody_da_pos_trans_time";
                            trans_time.value = invoice_date;

                            #region Discount

                            invoiceObject.discountRate = (Math.Round(invoice_info.Total_Discount, 3) * -1).ToString();

                            lineDiscount = new DoubleCustomFieldRef();
                            lineDiscount.scriptId = "custbody_da_pos_line_item_discount";
                            lineDiscount.value = Math.Round(invoice_info.Line_Discount_Amount, 3) * -1;


                            orderDiscount = new StringCustomFieldRef();
                            orderDiscount.scriptId = "custbody_da_pos_order_discount";
                            orderDiscount.value = (Math.Round(invoice_info.Invoice_Discount_Rate, 3) * -1).ToString();
                            if (invoice_info.Invoice_Discount_Type == 1)
                                orderDiscount.value = (Math.Round(invoice_info.Invoice_Discount_Rate, 3) * -1).ToString() + "%";

                            if (invoice_info.Accounting_Discount_Item != 0)
                            {
                                discItem = new RecordRef();
                                discItem.internalId = invoice_info.Accounting_Discount_Item.ToString();
                                discItem.type = RecordType.discountItem;
                                invoiceObject.discountItem = discItem;
                            }
                            #endregion

                            int length = 7;
                            if (invoice_info.Terminal_Id > 0)
                                length = 8;

                            customFieldRefArray = new CustomFieldRef[length];
                            customFieldRefArray[0] = cashier;
                            customFieldRefArray[1] = balance;
                            customFieldRefArray[2] = paid;
                            customFieldRefArray[3] = trans_time;
                            customFieldRefArray[4] = trans_no;
                            customFieldRefArray[5] = lineDiscount;
                            customFieldRefArray[6] = orderDiscount;

                            if (invoice_info.Terminal_Id > 0)
                                customFieldRefArray[7] = terminal;

                            invoiceObject.customFieldList = customFieldRefArray;
                            #endregion

                            InvoiceArr[i] = invoiceObject;
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            invoiceLst.RemoveAt(i);
                          //  LogDAO.Integration_Exception(LogIntegrationType.Error, TaskRunType.POST, "InvoiceTask", "Error adding invoice: " + ex.Message + " Invoice id = " + invoiceLst[i].Id.ToString());
                        }
                    }
                    // Send invoice list to netsuite
                    WriteResponseList wr = Service(true).addList(InvoiceArr);
                    result = wr.status.isSuccess;

                    // LogDAO.Integration_Exception(LogIntegrationType.Info, TaskRunType.POST, "InvoiceTask", "Status: " + result);
                    if (result)
                        //Update database with returned Netsuite ids
                        UpdatedInvoice(invoiceLst, wr);
                }

                // post customerPayment to netsuite
              //  bool postPayments = PostCustomerPayment();
            }
            catch (Exception ex)
            {
              //  LogDAO.Integration_Exception(LogIntegrationType.Error, TaskRunType.POST, "InvoiceTask Error", "Error " + ex.Message);
            }

            // release the the object
            //new ScheduleTaskDAO().Processing(taskType, TaskRunType.POST, false);

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
                         //   LogDAO.Integration_Exception(LogIntegrationType.Error, TaskRunType.POST, "InvoiceTask UpdateDB Counter Error", "Error " + ex.Message + " Count = " + counter.ToString());
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
            catch (Exception ex){
               // LogDAO.Integration_Exception(LogIntegrationType.Error, TaskRunType.POST, "InvoiceTask UpdateDB Error", "Error " + ex.Message);
            }
        }

        #region payment methods
        //public bool PostCustomerPayment()
        //{
        //    List<Foodics.NetSuite.Shared.Model.PaymentMethodEntity> invoiceMethodLst = new GenericeDAO<Foodics.NetSuite.Shared.Model.PaymentMethodEntity>().GetWhere(" Netsuite_Id IS NULL or Netsuite_Id =0");
            
        //    Setting objSetting = new GenericeDAO<Setting>().GetAll().FirstOrDefault();
        //    if (invoiceMethodLst.Count > 0)
        //    {
                
        //        List<Record> cps = new List<Record>();
        //        bool is_valid = false;
        //        for (int f = 0; f < invoiceMethodLst.Count; f++)
        //        {
        //            Foodics.NetSuite.Shared.Model.Invoice invoiceobj = new GenericeDAO<Foodics.NetSuite.Shared.Model.Invoice>().GetWhere(" Foodics_Id =" + invoiceMethodLst[f].Foodics_Id).FirstOrDefault();
        //            #region Accept Payment
        //            CustomerPaymentApplyList AplyList = new CustomerPaymentApplyList();
        //            CustomerPaymentCreditList CreditList = new CustomerPaymentCreditList();
        //            CustomerPaymentDepositList DepositList = new CustomerPaymentDepositList();
        //            CustomerPaymentApply[] payApply = new CustomerPaymentApply[1];
        //            CustomerPaymentCredit[] paycredit;
        //            CustomerPaymentDeposit[] payDeposit;

        //            CustomerPayment cp = new CustomerPayment();
        //            cp.autoApply = false;
        //            is_valid = false;

        //            #region Payment Properties

        //            //customer
        //            RecordRef customer = new RecordRef();
        //            customer.internalId = invoiceobj.Customer_Netsuite_Id.ToString();
        //            customer.type = RecordType.customer;
        //            cp.customer = customer;

        //            //currency
        //            RecordRef currency = new RecordRef();
        //            currency.internalId = objSetting.Currency_Netsuite_Id.ToString();//invoiceMethodLst[f].Currency_Id.ToString();
        //            currency.type = RecordType.currency;
        //            cp.currency = currency;

        //            //exchangeRate
        //           // cp.exchangeRate = invoiceMethodLst[f].Exchange_Rate;

        //            // memo
        //            cp.memo = invoiceMethodLst[f].Notes;

        //            //tranDate 
        //            cp.tranDate = TimeZoneInfo.ConvertTimeToUtc(invoiceMethodLst[f].CreateDate, TimeZoneInfo.Local);
        //            cp.tranDateSpecified = true;

        //            //cp.subsidiary
        //            RecordRef subsidiary = new RecordRef();
        //            subsidiary.internalId = objSetting.Subsidiary_Netsuite_Id.ToString(); //invoiceMethodLst[f].Subsidiary_Id.ToString();
        //            subsidiary.type = RecordType.subsidiary;
        //            cp.subsidiary = subsidiary;

        //            //cp.location
        //            RecordRef location = new RecordRef();
        //            location.internalId = objSetting.Location_Netsuite_Id.ToString();//invoiceMethodLst[f].Location_Id.ToString();
        //            location.type = RecordType.location;
        //            cp.location = location;

        //            // payment amount
        //            cp.payment = invoiceMethodLst[f].Amount;
        //            cp.paymentSpecified = true;

        //            if (invoiceMethodLst[f].Department_Id > 0)
        //            {
        //                // department
        //                RecordRef department = new RecordRef();
        //                department.internalId = invoiceMethodLst[f].Department_Id.ToString();
        //                department.type = RecordType.department;
        //                cp.department = department;
        //            }

        //            if (invoiceMethodLst[f].Class_Id > 0)
        //            {
        //                // class
        //                RecordRef classification = new RecordRef();
        //                classification.internalId = invoiceMethodLst[f].Class_Id.ToString();
        //                classification.type = RecordType.classification;
        //                cp.@class = classification;
        //            }

        //            #region Payment Custom Attributes

        //            StringCustomFieldRef trans_no = new StringCustomFieldRef();
        //            trans_no.scriptId = "custbody_da_pos_trans_no";
        //            trans_no.value = invoiceMethodLst[f].Entity_BarCode.ToString();

        //            StringCustomFieldRef pos_id = new StringCustomFieldRef();
        //            pos_id.scriptId = "custbody_da_pos_id";
        //            pos_id.value = invoiceMethodLst[f].Entity_BarCode.ToString();

        //            LongCustomFieldRef trans_id = new LongCustomFieldRef();
        //            trans_id.scriptId = "custbody_da_transaction_id";
        //            trans_id.value = invoiceMethodLst[f].Entity_Id;

        //            CustomFieldRef[] customFieldRefArray = new CustomFieldRef[3];
        //            customFieldRefArray[0] = trans_no;
        //            customFieldRefArray[1] = pos_id;
        //            customFieldRefArray[2] = trans_id;

        //            cp.customFieldList = customFieldRefArray;
        //            #endregion

        //            #region Apply Invoice
        //            // Invoice
        //            payApply[0] = new CustomerPaymentApply();
        //            payApply[0].apply = true;
        //            payApply[0].docSpecified = true;
        //            payApply[0].amountSpecified = true;
        //            payApply[0].currency = currency.internalId;
        //            payApply[0].type = "Invoice";
        //            payApply[0].doc = invoiceMethodLst[f].Entity_Netsuite_Id;
        //            payApply[0].total = invoiceMethodLst[f].Amount;
        //            payApply[0].amount = invoiceMethodLst[f].Amount;
        //            payApply[0].applyDate = invoiceMethodLst[f].CreateDate;
        //            #endregion

        //            // payment method
        //            if (invoiceMethodLst[f].Payment_Method_Id > 0)
        //            {
        //                is_valid = true;

        //                // payment method
        //                RecordRef payment_method = new RecordRef();
        //                payment_method.internalId = invoiceMethodLst[f].Payment_Method_Id.ToString();
        //                payment_method.type = RecordType.customerPayment;
        //                cp.paymentMethod = payment_method;
        //                cp.authCode = invoiceMethodLst[f].Ref;

        //                // amount
        //                cp.payment = invoiceMethodLst[f].Amount;
        //                cp.paymentSpecified = true;
        //            }
        //            else if (invoiceMethodLst[f].Payment_Method_Id == -4) // Loyalty
        //            {
        //                is_valid = true;
        //                cp.memo = "Loyalty Points";

        //                // redeem account
        //                RecordRef redeem_account = new RecordRef();
        //                redeem_account.internalId = invoiceMethodLst[f].Account_Id.ToString();
        //                redeem_account.type = RecordType.account;
        //                cp.account = redeem_account;
        //            }
        //            else if (invoiceMethodLst[f].Payment_Method_Type_Netsuite_Id > 0)
        //            {
        //                is_valid = true;
        //                if (invoiceMethodLst[f].Payment_Method_Type == 3)
        //                {
        //                    // customer deposit
        //                    payDeposit = new CustomerPaymentDeposit[1];
        //                    payDeposit[0] = new CustomerPaymentDeposit();
        //                    payDeposit[0].apply = true;
        //                    payDeposit[0].docSpecified = true;
        //                    payDeposit[0].amountSpecified = true;
        //                    payDeposit[0].currency = currency.internalId;

        //                    payDeposit[0].doc = invoiceMethodLst[f].Payment_Method_Type_Netsuite_Id;
        //                    payDeposit[0].total = invoiceMethodLst[f].Amount;
        //                    payDeposit[0].amount = invoiceMethodLst[f].Amount;

        //                    DepositList.deposit = payDeposit;
        //                    cp.depositList = DepositList;
        //                }
        //                else
        //                {
        //                    // credit memo
        //                    paycredit = new CustomerPaymentCredit[1];
        //                    paycredit[0] = new CustomerPaymentCredit();
        //                    paycredit[0].apply = true;
        //                    paycredit[0].docSpecified = true;
        //                    paycredit[0].amountSpecified = true;
        //                    paycredit[0].currency = currency.internalId;
        //                    paycredit[0].type = "Credit Memo";
        //                    paycredit[0].doc = invoiceMethodLst[f].Payment_Method_Type_Netsuite_Id;
        //                    paycredit[0].total = invoiceMethodLst[f].Amount;
        //                    paycredit[0].amount = invoiceMethodLst[f].Amount;

        //                    CreditList.credit = paycredit;
        //                    cp.creditList = CreditList;
        //                }
        //            }

        //            AplyList.apply = payApply;
        //            cp.applyList = AplyList;

        //            #endregion

        //            if (is_valid)
        //                cps.Add(cp);
        //            #endregion
        //        }

        //        if (cps.Count > 0)
        //        {
        //            WriteResponseList wr = Service(true).addList(cps.ToArray());
        //            bool result = wr.status.isSuccess;

        //            // UpdatedInvoiceMethods(invoiceMethodLst, wr);

        //            return result;
        //        }
        //    }

        //    return true;
        //}
        //private void UpdatedInvoiceMethods(List<Model.PaymentMethodEntity.Integrate> invoiceMethodLst, WriteResponseList responseLst)
        //{
        //    //Tuple to hold local order ids and its corresponding Netsuite ids
        //    List<Tuple<int, int>> iDs = new List<Tuple<int, int>>();
        //    //loop to fill tuple values
        //    for (int counter = 0; counter < invoiceMethodLst.Count; counter++)
        //    {
        //        //ensure that order is added to netsuite
        //        if (responseLst.writeResponse[counter].status.isSuccess)
        //        {
        //            RecordRef rf = (RecordRef)responseLst.writeResponse[counter].baseRef;

        //            //add item to the tuple
        //            iDs.Add(new Tuple<int, int>(Convert.ToInt32(rf.internalId.ToString()), invoiceMethodLst[counter].Id));
        //        }

        //    }
        //    NetsuiteDAO objDAO = new NetsuiteDAO();
        //    //updates local db
        //    if (iDs.Count() > 0)
        //        objDAO.UpdateNetsuiteIDs(iDs, "PaymentMethodEntity");
        //}
        #endregion

        //#region gift certificate
        //public void GetGiftCertificate()
        //{
        //    GiftCertificateSearch custSearch = new GiftCertificateSearch();
        //    GiftCertificateSearchBasic custSearchBasic = new GiftCertificateSearchBasic();

        //    #region filter
        //    custSearchBasic.createdDate = new SearchDateField();
        //    custSearchBasic.createdDate.@operator = SearchDateFieldOperator.notBefore;
        //    custSearchBasic.createdDate.operatorSpecified = true;
        //    custSearchBasic.createdDate.searchValue = DateTime.Now.AddDays(-7);
        //    custSearchBasic.createdDate.searchValueSpecified = true;
        //    custSearch.basic = custSearchBasic;
        //    #endregion

        //    SearchResult response = null;

        //    int pageIndex = 0;
        //    do
        //    {
        //        pageIndex++;
        //        Preferences prefs = new Preferences();
        //        Service(true).preferences = prefs;
        //        prefs.warningAsErrorSpecified = true;
        //        prefs.warningAsError = false;
        //        SearchPreferences _srch_pref = new SearchPreferences();
        //        Service().searchPreferences = _srch_pref;
        //        Service().searchPreferences.bodyFieldsOnly = false;

        //        custSearch.basic = custSearchBasic;
        //        if (response == null)
        //        {
        //            response = Service().search(custSearch);
        //        }
        //        else // .searchMore returns the next page(s) of data.
        //        {
        //            response = Service().searchMoreWithId(response.searchId, pageIndex);
        //            //response = Service().searchMoreWithId(response.searchId, response.pageIndex);
        //        }
        //        List<Model.GiftCertificate.Integrate> list = new List<Model.GiftCertificate.Integrate>();
        //        Model.GiftCertificate.Integrate gift;

        //        foreach (Record record in response.recordList)
        //        {
        //            Type typeName = record.GetType();
        //            GiftCertificate gift_certificate = (GiftCertificate)record;
        //            gift = new Model.GiftCertificate.Integrate();
        //            gift.Netsuite_Id = Shared.Utility.ConvertToInt(gift_certificate.internalId);
        //            gift.Gift_Code = gift_certificate.giftCertCode;
        //            gift.Remaining = Convert.ToSingle(gift_certificate.amountRemaining);
        //            gift.Is_Sold = true;
        //            list.Add(gift);
        //        }
        //        if (list.Count > 0)
        //        {
        //            try
        //            {
        //                new GenericeDAO<Model.GiftCertificate.Integrate>().MultiFieldIntegration(list, "Gift_Code");
        //                list.Clear();
        //            }
        //            catch (Exception ex)
        //            {
        //                // throw ex;
        //            }
        //        }
        //    }
        //    while (response.pageIndex < response.totalPages);
        //}
        //#endregion
    }
}
