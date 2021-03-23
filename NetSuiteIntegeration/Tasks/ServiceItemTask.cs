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
    public class ServiceItemTask : NetSuiteBaseIntegration
    {
        public override Int64 Set(string parametersArr)
        {



            List<Item> Lst_ItemsAll = new GenericeDAO<Item>().GetWhere("(Netsuite_Id IS NULL or Netsuite_Id =0) and inactive=0 and  Item_Type=" + (int)Item_Type.ServiceSaleItem).Take(200).ToList();
            //List<Item> Lst_ItemsAll = new GenericeDAO<Item>().GetWhere(" id>=1200 and inactive=0 and Item_Type=" + (int)Item_Type.Service).Take(100).ToList();

            List<Item> Lst_ItemsUpdate = Lst_ItemsAll.Where(x => x.Netsuite_Id > 0).ToList();
            List<Item> Lst_ItemsNew = Lst_ItemsAll.Where(x => x.Netsuite_Id == 0 || x.Netsuite_Id < 0).ToList();

            if (Lst_ItemsAll.Count <= 0)
                return 0;

            // Send order list to netsuite
            if (Lst_ItemsNew.Count > 0)
            {
                com.netsuite.webservices.ServiceSaleItem[] ItemArrNew = GenerateNetSuitelst(Lst_ItemsNew);
                WriteResponseList wrNew = Service(true).addList(ItemArrNew);
                bool result = wrNew.status.isSuccess;
                if (result)
                {
                    //Update database with returned Netsuite ids
                    UpdatedLst(Lst_ItemsNew, wrNew);
                }
            }

            if (Lst_ItemsUpdate.Count > 0)
            {
                com.netsuite.webservices.ServiceSaleItem[] ItemArrAdd = GenerateNetSuitelst(Lst_ItemsUpdate);
                // Send order list to netsuite
                WriteResponseList wr = Service(true).updateList(ItemArrAdd);
            }

            return 0;
        }
        public com.netsuite.webservices.ServiceSaleItem[] GenerateNetSuitelst(List<Item> Lst_ItemsAll)
        {
            com.netsuite.webservices.ServiceSaleItem[] ItemArr = new com.netsuite.webservices.ServiceSaleItem[Lst_ItemsAll.Count];
            try
            {
                for (int i = 0; i < Lst_ItemsAll.Count; i++)
                {
                    RecordRef[] subsidiarylst = new RecordRef[1];
                    Item Obj = Lst_ItemsAll[i];
                    Setting objSetting = new GenericeDAO<Setting>().GetWhere("Subsidiary_Netsuite_Id=" + Obj.Subsidiary_Id).FirstOrDefault();
                    Categories.CategoriesAccounts objCatAccount = new Categories.CategoriesAccounts();
                    com.netsuite.webservices.ServiceSaleItem NewItemObject = new com.netsuite.webservices.ServiceSaleItem();
                    if (Obj.Netsuite_Id <= 0)
                    {
                        NewItemObject.displayName = Obj.Display_Name_En + " 3 ";
                        //NewItemObject.itemId = Obj.UPC_Code;
                        NewItemObject.itemId = Obj.Display_Name_En + "3 ";
                        NewItemObject.salesDescription = Obj.Display_Name_En + " 3 ";
                    }
                    //check if new or can be updated
                    if (Obj.Netsuite_Id > 0)
                        NewItemObject.internalId = Obj.Netsuite_Id.ToString();

                    NewItemObject.pricingMatrix = Helper.GeneratePricingMatrix(objSetting, Obj.Price);
                   

                    CustomFieldRef[] custFieldList = new CustomFieldRef[] {

                        new StringCustomFieldRef {
                            value = Obj.Name_Ar!= null? Obj.Name_Ar:"",
                            scriptId = "custitem_da_item_name_ar"
                        }
                       
                        };
                    NewItemObject.customFieldList = custFieldList;


                    #region Items Account
                    RecordRef IncomAccountref = new RecordRef();
                    IncomAccountref.type = RecordType.account;
                    NewItemObject.incomeAccount = IncomAccountref;

                    IncomAccountref.internalId = objSetting.IncomeAccount_Netsuite_Id.ToString();







                    #endregion
                    RecordRef Tax_Schedule = new RecordRef();
                    Tax_Schedule.internalId = objSetting.TaxSchedule_Netsuite_Id.ToString();
                    Tax_Schedule.type = RecordType.salesTaxItem;
                    NewItemObject.taxSchedule = Tax_Schedule;

                    RecordRef subsidiary = new RecordRef();
                    subsidiary.internalId = objSetting.Subsidiary_Netsuite_Id.ToString();
                    subsidiary.type = RecordType.subsidiary;
                    subsidiarylst[0] = subsidiary;
                    NewItemObject.subsidiaryList = subsidiarylst;


                    NewItemObject.isInactive = Obj.InActive;
                    NewItemObject.isInactiveSpecified = true;

                    ItemArr[i] = NewItemObject;
                }
            }
            catch (Exception ex)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
            }
            return ItemArr;
        }
        private void UpdatedLst(List<Item> Lst_Items, WriteResponseList responseLst)
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
            GenericeDAO<Item> objDAO = new GenericeDAO<Item>();
            //updates local db
            objDAO.UpdateNetsuiteIDs(iDs, "Item");
        }


        public override void Get()
        {

        }
    }
}
