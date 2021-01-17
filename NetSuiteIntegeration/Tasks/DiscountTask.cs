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
    public class DiscountTask : NetSuiteBaseIntegration
    {
        public override Int64 Set(string parametersArr)
        {
            try
            {
                new CustomDAO().Check_PaymentCash_Exist();
                List<Foodics.NetSuite.Shared.Model.Discount> Lst_Items = new GenericeDAO<Foodics.NetSuite.Shared.Model.Discount>().GetWhere(" InActive = 0 and (Netsuite_Id IS NULL or Netsuite_Id =0)");
                if (Lst_Items.Count <= 0)
                    return 0;
                com.netsuite.webservices.DiscountItem[] ItemArr = new com.netsuite.webservices.DiscountItem[Lst_Items.Count];
                for (int i = 0; i < Lst_Items.Count; i++)
                {
                    RecordRef[] subsidiarylst = new RecordRef[1];
                    Foodics.NetSuite.Shared.Model.Discount Obj = Lst_Items[i];
                    com.netsuite.webservices.DiscountItem NewItemObject = new com.netsuite.webservices.DiscountItem();
                    Setting objSetting = new GenericeDAO<Setting>().GetWhere("Subsidiary_Netsuite_Id=" + Obj.Subsidiary_Id).FirstOrDefault();
                    if (objSetting != null && objSetting.Netsuite_Id <= 0)
                        return 0;

                    NewItemObject.displayName = Obj.Name_En +" - "+ objSetting.Name;
                    NewItemObject.itemId = Obj.Name_En + " - " + objSetting.Name;

                    NewItemObject.rate = Obj.Amount.ToString();
                    if (Obj.IsPercentage)
                        NewItemObject.rate += "%";

                    RecordRef subsidiary = new RecordRef();
                    subsidiary.internalId = Obj.Subsidiary_Id.ToString();
                    subsidiary.type = RecordType.subsidiary;
                    subsidiarylst[0] = subsidiary;
                    NewItemObject.subsidiaryList = subsidiarylst;

                    RecordRef Accountref = new RecordRef();
                    Accountref.internalId = objSetting.DiscountAccount_Netsuite_Id.ToString();
                    Accountref.type = RecordType.account;
                    NewItemObject.account = Accountref;
                    if (Obj.InActive)
                    {
                        NewItemObject.isInactive = true;
                        NewItemObject.isInactiveSpecified = true;
                    }
                    ItemArr[i] = NewItemObject;
                }
                WriteResponseList wr = Service(true).addList(ItemArr);
                bool result = wr.status.isSuccess;
                if (result)
                {
                    UpdatedLst(Lst_Items, wr);
                }
            }
            catch (Exception ex)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
            }
            return 0;
        }

        private void UpdatedLst(List<Foodics.NetSuite.Shared.Model.Discount> Lst_Items, WriteResponseList responseLst)
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

                GenericeDAO<Foodics.NetSuite.Shared.Model.Discount> objDAO = new GenericeDAO<Foodics.NetSuite.Shared.Model.Discount>();
                //updates local db
                objDAO.UpdateNetsuiteIDs(iDs, "Discount");
            }
            catch (Exception ex)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
            }
        }
        public override void Get()
        {
          


        }
    }
}
