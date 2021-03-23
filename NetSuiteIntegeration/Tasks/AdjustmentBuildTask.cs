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
using System.Reflection;
using System.Configuration;

namespace NetSuiteIntegeration.Tasks
{


    public class AdjustmentBuildTask : NetSuiteBaseIntegration
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


        public override Int64 Set(string parametersArr)
        {
            try
            {
                new CustomDAO().GenerateAdjustmentBuild();
                List<Foodics.NetSuite.Shared.Model.AdjustmentBuild> ColLstAll = new CustomDAO().SelectAdjustmentLocation();
                var ColLst =
                ColLstAll.DistinctBy(x=> new { x.Location_Id, x.Netsuite_Id }).Select(x => new AdjustmentBuild
                {
                    Location_Id = x.Location_Id,
                    Subsidiary_Id = x.Subsidiary_Id,
                }).Distinct().ToList();
                if (ColLst.Count <= 0)
                    return 0;

                com.netsuite.webservices.InventoryAdjustment[] AdjustArr = new com.netsuite.webservices.InventoryAdjustment[ColLst.Count];
                for (int i = 0; i < ColLst.Count; i++)
                {
                    com.netsuite.webservices.InventoryAdjustment AdjustBuildObject;
                    Foodics.NetSuite.Shared.Model.AdjustmentBuild Obj_info;
                    try
                    {
                        Obj_info = ColLst[i];
                        //Netsuite invoice type
                        AdjustBuildObject = new com.netsuite.webservices.InventoryAdjustment();
                        Setting objSetting = new GenericeDAO<Setting>().GetWhere("Subsidiary_Netsuite_Id=" + Obj_info.Subsidiary_Id).FirstOrDefault();

                        AdjustBuildObject.tranDateSpecified = true;
                        AdjustBuildObject.tranDate = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now, TimeZoneInfo.Local);
                        //AdjustBuildObject.tranDate = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2021, 01, 01), TimeZoneInfo.Local);
                        //AdjustBuildObject.tranDate = TimeZoneInfo.ConvertTimeToUtc(Utility.ConvertToDateTime(ConfigurationManager.AppSettings["InvoiceDate"]), TimeZoneInfo.Local);

                        // adjustment account
                        RecordRef adjustment_account = new RecordRef();
                        adjustment_account.type = RecordType.account;
                        adjustment_account.typeSpecified = true;
                        adjustment_account.internalId = objSetting.AdjustmentAccount_Netsuite_Id.ToString();//"122";
                        AdjustBuildObject.account = adjustment_account;

                        // adjustment location
                        RecordRef adjustment_location = new RecordRef();
                        adjustment_location.type = RecordType.location;
                        adjustment_location.typeSpecified = true;
                        adjustment_location.internalId = Obj_info.Location_Id.ToString();
                        AdjustBuildObject.location = adjustment_location;
                        AdjustBuildObject.adjLocation = adjustment_location;

                        // subsidiary
                        RecordRef adjustment_subsidiary = new RecordRef();
                        adjustment_subsidiary.type = RecordType.subsidiary;
                        adjustment_subsidiary.typeSpecified = true;
                        adjustment_subsidiary.internalId = Obj_info.Subsidiary_Id.ToString();
                        AdjustBuildObject.subsidiary = adjustment_subsidiary;

                        List<Foodics.NetSuite.Shared.Model.AdjustmentBuild> adjustment_items = ColLstAll.Where(x => x.Subsidiary_Id == Obj_info.Subsidiary_Id && x.Location_Id == Obj_info.Location_Id).ToList();
                        InventoryAdjustmentInventory[] invadjustmentItemArray = new InventoryAdjustmentInventory[adjustment_items.Count()];
                        for (int x = 0; x < adjustment_items.Count(); x++)
                        {
                            RecordRef item = new RecordRef();
                            item.internalId = adjustment_items[x].Item_Id.ToString();

                            invadjustmentItemArray[x] = new InventoryAdjustmentInventory();
                            invadjustmentItemArray[x].item = item;
                            invadjustmentItemArray[x].location = adjustment_location;
                            invadjustmentItemArray[x].adjustQtyBy = (adjustment_items[x].Quantity * -1);
                            invadjustmentItemArray[x].adjustQtyBySpecified = true;


                        }

                        InventoryAdjustmentInventoryList invList = new InventoryAdjustmentInventoryList();
                        invList.inventory = invadjustmentItemArray;
                        AdjustBuildObject.inventoryList = invList;



                        AdjustArr[i] = AdjustBuildObject;
                    }
                    catch (Exception ex)
                    {
                        ColLst.RemoveAt(i);
                        LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
                    }
                }

                WriteResponseList wr = Service(true).addList(AdjustArr);
                bool result = wr.status.isSuccess;
                if (result)
                {
                    //Update database with returned Netsuite ids
                    Updatedlst(ColLst, wr);

                }
            }
            catch (Exception ex)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
            }



            return 0;
        }

        private void Updatedlst(List<Foodics.NetSuite.Shared.Model.AdjustmentBuild> InvoiceLst, WriteResponseList responseLst)
        {
            try
            {
                //Tuple to hold local invoice ids and its corresponding Netsuite ids
                List<Tuple<int, int, int>> iDs = new List<Tuple<int, int, int>>();
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
                            iDs.Add(new Tuple<int, int, int>(Convert.ToInt32(rf.internalId.ToString()), InvoiceLst[counter].Location_Id, InvoiceLst[counter].Subsidiary_Id));
                        }
                        catch (Exception ex)
                        {
                            //   LogDAO.Integration_Exception(LogIntegrationType.Error, TaskRunType.POST, "InvoiceTask UpdateDB Counter Error", "Error " + ex.Message + " Count = " + counter.ToString());
                        }
                    }
                }


                GenericeDAO<Foodics.NetSuite.Shared.Model.AssemblyBuild> objDAO = new GenericeDAO<Foodics.NetSuite.Shared.Model.AssemblyBuild>();
                objDAO.UpdateNetsuiteID_ADjustement(iDs, "AdjustmentBuild");
            }
            catch (Exception ex)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);

            }
        }


    }
}
