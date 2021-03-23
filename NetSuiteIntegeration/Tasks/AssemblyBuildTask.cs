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
        public override Int64 Set(string parametersArr)
        {

            new CustomDAO().InvoiceRelatedUpdate();
            new CustomDAO().GenerateAssemblyBuild();

            List<Foodics.NetSuite.Shared.Model.AssemblyBuild> lstitemsAll = new GenericeDAO<Foodics.NetSuite.Shared.Model.AssemblyBuild>().GetWhere(" item_id >0 and (Netsuite_Id IS NULL or Netsuite_Id =0)").Take(1000).ToList();
            int Exe_length = 200;
            int lstend = Exe_length;
            if (lstitemsAll.Count > 0)
            {
                for (int Index = 0; Index < lstitemsAll.Count; Index += Exe_length)
                {
                    if (Index + Exe_length >= lstitemsAll.Count)
                        lstend = lstitemsAll.Count - Index;

                    List<Foodics.NetSuite.Shared.Model.AssemblyBuild> ColLst = lstitemsAll.GetRange(Index, lstend);


                    try
                    {

                        //get recentrly added invoices after creating the return

                        bool result = true;
                        if (ColLst.Count > 0)
                        {
                            #region variables
                            com.netsuite.webservices.AssemblyBuild[] AssemblyArr = new com.netsuite.webservices.AssemblyBuild[ColLst.Count];
                            Foodics.NetSuite.Shared.Model.AssemblyBuild Obj_info;
                            com.netsuite.webservices.AssemblyBuild AssemblyBuildObject;
                            RecordRef Mainitem,
                                     subsid, location;
                            #endregion
                            for (int i = 0; i < ColLst.Count; i++)
                            {
                                try
                                {
                                    Obj_info = ColLst[i];
                                    //Netsuite invoice type
                                    AssemblyBuildObject = new com.netsuite.webservices.AssemblyBuild();
                                    AssemblyBuildObject.quantity = Obj_info.Quantity;
                                    AssemblyBuildObject.quantitySpecified = true;

                                    Mainitem = new RecordRef();
                                    Mainitem.internalId = Obj_info.Item_Id.ToString(); //objSetting.Location_Netsuite_Id.ToString();
                                    Mainitem.type = RecordType.assemblyItem;
                                    AssemblyBuildObject.item = Mainitem;

                                    AssemblyBuildObject.tranDateSpecified = true;
                                    AssemblyBuildObject.tranDate = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now, TimeZoneInfo.Local);
                                    //AssemblyBuildObject.tranDate = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2021, 03, 15), TimeZoneInfo.Local);
                                    //AssemblyBuildObject.tranDate = TimeZoneInfo.ConvertTimeToUtc(Utility.ConvertToDateTime(ConfigurationManager.AppSettings["InvoiceDate"]), TimeZoneInfo.Local);

                                    location = new RecordRef();
                                    location.internalId = Obj_info.Location_Id.ToString(); //objSetting.Location_Netsuite_Id.ToString();
                                    location.type = RecordType.location;
                                    AssemblyBuildObject.location = location;

                                    subsid = new RecordRef();
                                    subsid.internalId = Obj_info.Subsidiary_Id.ToString();
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

                }

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
            catch (Exception ex)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);

            }
        }


    }
}
