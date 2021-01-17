using NetSuiteIntegeration.com.netsuite.webservices;
using System;
using System.Collections.Generic;
using System.Linq;
using Foodics.NetSuite.Shared.DAO;


using Foodics.NetSuite.Shared.Model;
using System.Data;
using Foodics.NetSuite.Shared;

namespace NetSuiteIntegeration.Tasks
{
    public class CustomerPaymentTask : NetSuiteBaseIntegration
    {
        //protected List<Model.ItemSubsidiary> subsidiaryListLocal = new List<Model.ItemSubsidiary>();
        //protected List<Model.Item> itemWithImages = new List<Model.Item>();
        //protected List<Model.ItemOptionListValue> ItemOptionList = new List<Model.ItemOptionListValue>();
        public override Int64 Set(string parametersArr)
        {
            List<Foodics.NetSuite.Shared.Model.PaymentMethodEntity> invoiceMethodLst = new CustomDAO().SelectCustomerPayment(4);
            //new GenericeDAO<Foodics.NetSuite.Shared.Model.PaymentMethodEntity>().GetWhere(" Netsuite_Id IS NULL or Netsuite_Id =0");
            try
            {
                //Setting objSetting = new GenericeDAO<Setting>().GetAll().FirstOrDefault();
                if (invoiceMethodLst.Count > 0)
                {

                    List<Record> cps = new List<Record>();
                    bool is_valid = false;
                    for (int f = 0; f < invoiceMethodLst.Count; f++)
                    //for (int f = 0; f < 1; f++)
                    {
                        PaymentMethodEntity payobj = invoiceMethodLst[f];
                        Foodics.NetSuite.Shared.Model.Invoice invoiceobj = new GenericeDAO<Foodics.NetSuite.Shared.Model.Invoice>().GetWhere(" Foodics_Id = '" + payobj.Foodics_Id + "'").FirstOrDefault();
                        Setting objSetting = new GenericeDAO<Setting>().GetWhere("Subsidiary_Netsuite_Id=" + invoiceobj.Subsidiary_Id).FirstOrDefault();
                        #region Accept Payment
                        CustomerPaymentApplyList AplyList = new CustomerPaymentApplyList();
                        CustomerPaymentCreditList CreditList = new CustomerPaymentCreditList();
                        CustomerPaymentDepositList DepositList = new CustomerPaymentDepositList();
                        CustomerPaymentApply[] payApply = new CustomerPaymentApply[1];
                        CustomerPayment cp = new CustomerPayment();
                        cp.autoApply = false;
                        is_valid = false;
                        #region Payment Properties
                        //customer
                        RecordRef customer = new RecordRef();
                        customer.internalId = invoiceobj.Customer_Netsuite_Id > 0 ? invoiceobj.Customer_Netsuite_Id.ToString() : objSetting.Customer_Netsuite_Id.ToString();
                        customer.type = RecordType.customer;
                        cp.customer = customer;

                        //currency
                        RecordRef currency = new RecordRef();
                        currency.internalId = objSetting.Currency_Netsuite_Id.ToString();//payobj.Currency_Id.ToString();
                        currency.type = RecordType.currency;
                        cp.currency = currency;
                        StringCustomFieldRef FoodicsRef, FoodicsNumb;

                        //exchangeRate
                        // cp.exchangeRate = payobj.Exchange_Rate;

                        // memo
                        cp.memo = payobj.Notes;

                        //tranDate 
                        cp.tranDate = TimeZoneInfo.ConvertTimeToUtc(payobj.Business_Date, TimeZoneInfo.Local);
                        cp.tranDateSpecified = true;

                        //cp.subsidiary
                        RecordRef subsidiary = new RecordRef();
                        subsidiary.internalId = objSetting.Subsidiary_Netsuite_Id.ToString(); //payobj.Subsidiary_Id.ToString();
                        subsidiary.type = RecordType.subsidiary;
                        cp.subsidiary = subsidiary;

                        //cp.location
                        RecordRef location = new RecordRef();
                        location.internalId = invoiceobj.Location_Id.ToString();//objSetting.Location_Netsuite_Id.ToString();//payobj.Location_Id.ToString();
                        location.type = RecordType.location;
                        cp.location = location;

                        // payment amount
                        cp.payment = payobj.Amount;
                        cp.paymentSpecified = true;

                        //if (payobj.Department_Id > 0)
                        //{
                        //    // department
                        //    RecordRef department = new RecordRef();
                        //    department.internalId = payobj.Department_Id.ToString();
                        //    department.type = RecordType.department;
                        //    cp.department = department;
                        //}

                        //if (payobj.Class_Id > 0)
                        //{
                        //    // class
                        //    RecordRef classification = new RecordRef();
                        //    classification.internalId = payobj.Class_Id.ToString();
                        //    classification.type = RecordType.classification;
                        //    cp.@class = classification;
                        //}

                        #region Payment Custom Attributes

                        DoubleCustomFieldRef payPercent = new DoubleCustomFieldRef();
                        payPercent.scriptId = "custbody_da_payment_method_percentage";
                        payPercent.value = payobj.Payment_Method_Percentage;

                        DoubleCustomFieldRef PayPercentAmount = new DoubleCustomFieldRef();
                        PayPercentAmount.scriptId = "custbody_da_payment_method_amount";
                        PayPercentAmount.value = (payobj.Payment_Method_Percentage * payobj.Amount) /100;

                        LongCustomFieldRef trans_id = new LongCustomFieldRef();
                        trans_id.scriptId = "custbody_da_transaction_id";
                        trans_id.value = invoiceobj.Netsuite_Id;

                        FoodicsRef = new StringCustomFieldRef();
                        FoodicsRef.scriptId = "custbody_da_foodics_reference";
                        FoodicsRef.value = invoiceobj.BarCode.ToString();

                        FoodicsNumb = new StringCustomFieldRef();
                        FoodicsNumb.scriptId = "custbody_da_foodics_number";
                        FoodicsNumb.value = invoiceobj.Number.ToString();

                        CustomFieldRef[] customFieldRefArray = new CustomFieldRef[5];
                        customFieldRefArray[0] = trans_id;
                        customFieldRefArray[1] = payPercent;
                        customFieldRefArray[2] = PayPercentAmount;
                        customFieldRefArray[3] = FoodicsRef;
                        customFieldRefArray[4] = FoodicsNumb;

                        cp.customFieldList = customFieldRefArray;
                        #endregion

                        #region Apply Invoice
                        // Invoice
                        payApply[0] = new CustomerPaymentApply();
                        payApply[0].apply = true;
                        payApply[0].docSpecified = true;
                        payApply[0].amountSpecified = true;
                        payApply[0].currency = currency.internalId;
                        payApply[0].type = "Invoice";
                        payApply[0].doc = invoiceobj.Netsuite_Id;
                        payApply[0].total = payobj.Amount;
                        payApply[0].amount = payobj.Amount;
                        payApply[0].applyDate = payobj.Business_Date;
                        #endregion

                        #region Invoice Custom Attributes
                      

                       

                        #endregion

                        // payment method
                        if (payobj.Payment_Method_Id > 0)
                        {
                            is_valid = true;

                            // payment method
                            RecordRef payment_method = new RecordRef();
                            payment_method.internalId = payobj.Payment_Method_Id.ToString();
                            payment_method.type = RecordType.customerPayment;
                            cp.paymentMethod = payment_method;
                            cp.authCode = payobj.Ref;

                            // amount
                            cp.payment = payobj.Amount;
                            cp.paymentSpecified = true;
                        }

                        AplyList.apply = payApply;
                        cp.applyList = AplyList;

                        #endregion

                        if (is_valid)
                            cps.Add(cp);
                        #endregion
                    }

                    if (cps.Count > 0)
                    {
                        WriteResponseList wr = Service(true).addList(cps.ToArray());
                        bool result = wr.status.isSuccess;

                        UpdatedLst(invoiceMethodLst, wr);

                        // return result;
                    }
                }
                // Send order list to netsuite
            }
            catch (Exception ex)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
            }
            return 0;
        }

        private void UpdatedLst(List<Foodics.NetSuite.Shared.Model.PaymentMethodEntity> Lst_Items, WriteResponseList responseLst)
        {
            //Tuple to hold local order ids and its corresponding Netsuite ids
            List<Tuple<int, string>> iDs = new List<Tuple<int, string>>();
            //loop to fill tuple values
            //for (int counter = 0; counter < Lst_Items.Count; counter++)
            try
            {
                for (int counter = 0; counter < Lst_Items.Count; counter++)
                {
                    //ensure that order is added to netsuite
                    if (responseLst.writeResponse[counter].status.isSuccess)
                    {

                        RecordRef rf = (RecordRef)responseLst.writeResponse[counter].baseRef;
                        //update netsuiteId property
                        Lst_Items[counter].Netsuite_Id = Convert.ToInt32(rf.internalId.ToString());
                        //add item to the tuple
                        iDs.Add(new Tuple<int, string>(Convert.ToInt32(rf.internalId.ToString()), Lst_Items[counter].Foodics_Id));
                    }

                }
            }
            catch (Exception ex)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
            }
            GenericeDAO<PaymentMethodEntity> objDAO = new GenericeDAO<PaymentMethodEntity>();
            //updates local db
            objDAO.UpdateNetsuiteIDs(iDs, "PaymentMethodEntity");
        }
        public override void Get()
        {
            /// <summary> This method get all items (with types we need in POS) from netsuite and check item type, 
            /// after that get all item info and save in DB.</summary>	

        }
    }
}
