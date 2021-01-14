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

namespace NetSuiteIntegeration.Tasks
{

    
    public class AssemblyBuildTask : NetSuiteBaseIntegration
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
                new CustomDAO().GenerateAssemblyBuild();
                //get recentrly added invoices after creating the return
                List<Foodics.NetSuite.Shared.Model.AssemblyBuild> ColLst = new GenericeDAO<Foodics.NetSuite.Shared.Model.AssemblyBuild>().GetWhere(" Netsuite_Id IS NULL or Netsuite_Id =0").Take(200).ToList();
                bool result = true;
                if (ColLst.Count > 0)
                {
                    #region variables
                    com.netsuite.webservices.AssemblyBuild[] AssemblyArr = new com.netsuite.webservices.AssemblyBuild[ColLst.Count];
                    Foodics.NetSuite.Shared.Model.AssemblyBuild Obj_info;
                    com.netsuite.webservices.AssemblyBuild AssemblyBuildObject;
                    RecordRef  Mainitem, 
                             subsid,  location;
                    #endregion
                    for (int i = 0; i < ColLst.Count; i++)
                    {
                        try
                        {
                            Obj_info = ColLst[i];
                            Setting objSetting = new GenericeDAO<Setting>().GetWhere("Subsidiary_Netsuite_Id=" + Obj_info.Subsidiary_Id).FirstOrDefault();

                            //Netsuite invoice type
                            AssemblyBuildObject = new com.netsuite.webservices.AssemblyBuild();
                            AssemblyBuildObject.quantity = Obj_info.Quantity;
                            AssemblyBuildObject.quantitySpecified = true;

                            Mainitem = new RecordRef();
                            Mainitem.internalId = Obj_info.Item_Id.ToString(); //objSetting.Location_Netsuite_Id.ToString();
                            Mainitem.type = RecordType.assemblyItem;
                            AssemblyBuildObject.item = Mainitem;


                            location = new RecordRef();
                            location.internalId = Obj_info.Location_Id.ToString(); //objSetting.Location_Netsuite_Id.ToString();
                            location.type = RecordType.location;
                            AssemblyBuildObject.location = location;

                            subsid = new RecordRef();
                            subsid.internalId = objSetting.Subsidiary_Netsuite_Id.ToString();
                            subsid.type = RecordType.subsidiary;
                            AssemblyBuildObject.subsidiary = subsid;
                          
                            AssemblyArr[i] = AssemblyBuildObject;
                        }
                        catch (Exception ex)
                        {
                            ColLst.RemoveAt(i);
                            LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
                        }
                    }
                    // Send invoice list to netsuite
                    WriteResponseList wr = Service(true).addList(AssemblyArr);
                    result = wr.status.isSuccess;
                    if (result)
                    {
                        //Update database with returned Netsuite ids
                        Updatedlst(ColLst, wr);
                      
                    }

                }

                // post customerPayment to netsuite
              //  bool postPayments = PostCustomerPayment();
            }
            catch (Exception ex)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
            }



            return 0;
        }

        private void Updatedlst(List<Foodics.NetSuite.Shared.Model.AssemblyBuild> InvoiceLst, WriteResponseList responseLst)
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
                            iDs.Add(new Tuple<int, string>(Convert.ToInt32(rf.internalId.ToString()), InvoiceLst[counter].id));
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

                GenericeDAO<Foodics.NetSuite.Shared.Model.AssemblyBuild> objDAO = new GenericeDAO<Foodics.NetSuite.Shared.Model.AssemblyBuild>();
                objDAO.MainUpdateNetsuiteIDs(iDs, "AssemblyBuild");
            }
            catch (Exception ex){
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
                
            }
        }

       
    }
}
