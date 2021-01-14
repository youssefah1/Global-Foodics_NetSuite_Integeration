using NetSuiteIntegeration.com.netsuite.webservices;
using System;
using System.Collections.Generic;
using System.Linq;
using Foodics.NetSuite.Shared.DAO;


using Foodics.NetSuite.Shared.Model;
using System.Data;
using Foodics.NetSuite.Shared;
using Location = Foodics.NetSuite.Shared.Model.Location;

namespace NetSuiteIntegeration.Tasks
{
    public class PaymentMethodTask : NetSuiteBaseIntegration
    {
        public override Int64 Set(string parametersArr)
        {
            try
            {
                new CustomDAO().Check_PaymentCash_Exist();
                List<Foodics.NetSuite.Shared.Model.PaymentMethod> Lst_Items = new GenericeDAO<Foodics.NetSuite.Shared.Model.PaymentMethod>().GetWhere(" (Netsuite_Id IS NULL or Netsuite_Id =0)");
                if (Lst_Items.Count <= 0)
                    return 0;

                RecordRef[] subsidiarylst = new RecordRef[1];
                com.netsuite.webservices.PaymentMethod[] ItemArr = new com.netsuite.webservices.PaymentMethod[Lst_Items.Count];
                for (int i = 0; i < Lst_Items.Count; i++)
                {
                    Foodics.NetSuite.Shared.Model.PaymentMethod Obj = Lst_Items[i];
                    com.netsuite.webservices.PaymentMethod NewItemObject = new com.netsuite.webservices.PaymentMethod();
                    NewItemObject.name = Obj.Name_En;
                    
                    NewItemObject.undepFunds = true;
                    NewItemObject.undepFundsSpecified = true;

                    NewItemObject.isOnline = false;
                    NewItemObject.isOnlineSpecified = true;
                    if (Obj.InActive)
                    {
                        NewItemObject.isInactive = true;
                        NewItemObject.isInactiveSpecified = true;
                    }

                    ItemArr[i] = NewItemObject;
                }
                // Send order list to netsuite
                WriteResponseList wr = Service(true).addList(ItemArr);
                bool result = wr.status.isSuccess;
                if (result)
                {
                    //Update database with returned Netsuite ids
                    UpdatedLst(Lst_Items, wr);
                }
            }
            catch (Exception ex)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
            }
            return 0;
        }

        private void UpdatedLst(List<Foodics.NetSuite.Shared.Model.PaymentMethod> Lst_Items, WriteResponseList responseLst)
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

                GenericeDAO<com.netsuite.webservices.PaymentMethod> objDAO = new GenericeDAO<com.netsuite.webservices.PaymentMethod>();
                //updates local db
                objDAO.UpdateNetsuiteIDs(iDs, "PaymentMethod");
            }
            catch (Exception ex)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
            }
        }
        public override void Get()
        {
            /// <summary> This method get all items (with types we need in POS) from netsuite and check item type, 
            /// after that get all item info and save in DB.</summary>	
            /// 

            string IntrnelID = GetCustomizationId("customrecord_da_payment_method_percentag");





            CustomRecordSearch customRecordSearch = new CustomRecordSearch();
            CustomRecordSearchBasic customRecordSearchBasic = new CustomRecordSearchBasic();
            RecordRef recordRef = new RecordRef();
            recordRef.internalId = IntrnelID;
            recordRef.type = RecordType.customTransaction;
            customRecordSearchBasic.recType = recordRef;
            customRecordSearch.basic = customRecordSearchBasic;
            SearchResult response = Service(true).search(customRecordSearch);
            if (response.status.isSuccess)
            {
                if (response.totalRecords > 0)
                {
                    CustomRecord item_Custom;
                    CustomFieldRef[] flds;
                    List<Foodics.NetSuite.Shared.Model.PaymentMethod> lstsetting = new List<Foodics.NetSuite.Shared.Model.PaymentMethod>();
                    for (int i = 0; i < response.recordList.Length; i++)
                    {
                        item_Custom = (CustomRecord)response.recordList[i];
                        flds = item_Custom.customFieldList;

                        Foodics.NetSuite.Shared.Model.PaymentMethod payObj = new Foodics.NetSuite.Shared.Model.PaymentMethod();
                        //payObj.Netsuite_Id = Utility.ConvertToInt(item_Custom.internalId);

                        for (int c = 0; c < flds.Length; c++)
                        {

                            if (flds[c].scriptId == "custrecord_da_percentage")
                                payObj.Percentage = Utility.ConvertToInt(((com.netsuite.webservices.DoubleCustomFieldRef)flds[c]).value.ToString());
                            if (flds[c].scriptId == "custrecord_da_payment_method_ref")
                                payObj.Netsuite_Id = Utility.ConvertToInt(((com.netsuite.webservices.SelectCustomFieldRef)flds[c]).value.internalId.ToString());
                            
                        }
                        lstsetting.Add(payObj);
                    }


                    new GenericeDAO<Foodics.NetSuite.Shared.Model.PaymentMethod>().UpdatePaymentMethod(lstsetting);




                }

            }
        }
    }
}
