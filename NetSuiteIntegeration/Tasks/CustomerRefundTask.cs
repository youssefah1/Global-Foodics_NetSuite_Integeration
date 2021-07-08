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
    public class CustomerRefundTask : NetSuiteBaseIntegration
    {
        public override Int64 Set(string parametersArr)
        {
            List<Foodics.NetSuite.Shared.Model.PaymentMethodEntity> lstitemsAll = new CustomDAO().SelectCustomerPayment(5).Take(2000).ToList();
            int Exe_length = 200;
            int lstend = Exe_length;
            if (lstitemsAll.Count > 0)
            {
                for (int Index = 0; Index < lstitemsAll.Count; Index += Exe_length)
                {
                    if (Index + Exe_length >= lstitemsAll.Count)
                        lstend = lstitemsAll.Count - Index;
                    List<Foodics.NetSuite.Shared.Model.PaymentMethodEntity> returnList = lstitemsAll.GetRange(Index, lstend);
                    try
                    {
                        if (returnList.Count > 0)
                        {

                            CustomerRefund[] memoList = new CustomerRefund[returnList.Count];
                            for (int i = 0; i < returnList.Count; i++)
                            {
                                PaymentMethodEntity invoiceReturn = returnList[i];
                                CustomerRefund memo = new CustomerRefund();
                                CustomerRefundApply[] payApply = new CustomerRefundApply[1];
                                CustomerRefundApplyList AplyList = new CustomerRefundApplyList();
                                Foodics.NetSuite.Shared.Model.Invoice invoiceobj = new GenericeDAO<Foodics.NetSuite.Shared.Model.Invoice>().GetWhere(" Foodics_Id = '" + invoiceReturn.Foodics_Id + "'").FirstOrDefault();
                                Setting objSetting = new GenericeDAO<Setting>().GetWhere("Subsidiary_Netsuite_Id=" + invoiceobj.Subsidiary_Id).FirstOrDefault();
                                //Customer

                                RecordRef entity = new RecordRef();
                                entity.internalId = invoiceobj.Customer_Netsuite_Id > 0 ? invoiceobj.Customer_Netsuite_Id.ToString() : objSetting.Customer_Netsuite_Id.ToString();
                                entity.type = RecordType.customer;
                                memo.customer = entity;

                                //currency
                                RecordRef currency = new RecordRef();
                                currency.internalId = objSetting.Currency_Netsuite_Id.ToString();
                                currency.type = RecordType.currency;
                                memo.currency = currency;

                                //date
                                memo.tranDateSpecified = true;
                                memo.tranDate = TimeZoneInfo.ConvertTimeToUtc(invoiceobj.Date, TimeZoneInfo.Local);

                                //exchange rate
                                memo.exchangeRate = invoiceobj.Exchange_Rate;

                                //subsidary
                                RecordRef subsid = new RecordRef();
                                subsid.internalId = invoiceobj.Subsidiary_Id.ToString();
                                subsid.type = RecordType.subsidiary;
                                memo.subsidiary = subsid;

                                //cp.location
                                RecordRef location = new RecordRef();
                                location.internalId = invoiceobj.Location_Id.ToString();
                                location.type = RecordType.location;
                                memo.location = location;

                                #region Apply Invoice
                                // Invoice
                                payApply[0] = new CustomerRefundApply();
                                payApply[0].apply = true;
                                payApply[0].docSpecified = true;
                                payApply[0].amountSpecified = true;
                                payApply[0].currency = currency.internalId;
                                payApply[0].type = "CreditMemo";
                                payApply[0].doc = invoiceobj.Netsuite_Id;
                                payApply[0].total = invoiceobj.Paid;
                                payApply[0].amount = invoiceobj.Paid;
                                payApply[0].applyDate = invoiceobj.Date;

                                AplyList.apply = payApply;
                                memo.applyList = AplyList;
                                #endregion

                                #region payment Method
                                RecordRef payment_method = new RecordRef();
                                payment_method.internalId = invoiceReturn.Payment_Method_Id.ToString();
                                payment_method.type = RecordType.customerPayment;
                                memo.paymentMethod = payment_method;
                                #endregion

                                memoList[i] = memo;
                            }


                            if (memoList.Length > 0)
                            {
                                WriteResponseList wr = Service(true).addList(memoList.ToArray());
                                bool result = wr.status.isSuccess;
                                UpdatedLst(returnList, wr);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
                    }
                }
            }
            return 0;
        }
        /*
         * Update transaction date
        //public override Int64 Set(string parametersArr)
        //{

        //    List<Foodics.NetSuite.Shared.Model.PaymentMethodEntity> lstitemsAll = new CustomDAO().SelectUpdateCustomerRefund();
        //    int Exe_length = 100;
        //    int lstend = Exe_length;
        //    if (lstitemsAll.Count > 0)
        //    {
        //        for (int Index = 0; Index < lstitemsAll.Count; Index += Exe_length)
        //        {
        //            if (Index + Exe_length >= lstitemsAll.Count)
        //                lstend = lstitemsAll.Count - Index;
        //            List<Foodics.NetSuite.Shared.Model.PaymentMethodEntity> returnList = lstitemsAll.GetRange(Index, lstend);

        //            if (returnList.Count > 0)
        //            {

        //                CustomerRefund[] memoList = new CustomerRefund[returnList.Count];
        //                for (int i = 0; i < returnList.Count; i++)
        //                {
        //                    PaymentMethodEntity invoiceReturn = returnList[i];


        //                    CustomerRefund memo = new CustomerRefund();
        //                    memo.internalId = invoiceReturn.Netsuite_Id.ToString();
        //                    memo.tranDate = TimeZoneInfo.ConvertTimeToUtc(invoiceReturn.Business_Date, TimeZoneInfo.Local);
        //                    memo.tranDateSpecified = true;
        //                    memoList[i] = memo;
        //                }
        //                WriteResponseList wr = Service(true).updateList(memoList.ToArray());
        //            }

        //        }
        //    }*/




        //    return 0;
        //}

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
