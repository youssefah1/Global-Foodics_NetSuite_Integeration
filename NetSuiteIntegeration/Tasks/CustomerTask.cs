using NetSuiteIntegeration.com.netsuite.webservices;
using System;
using System.Collections.Generic;
using System.Linq;
using Foodics.NetSuite.Shared.DAO;


using Foodics.NetSuite.Shared.Model;
using System.Data;
using Foodics.NetSuite.Shared;
using System.Configuration;

namespace NetSuiteIntegeration.Tasks
{
    public class CustomerTask : NetSuiteBaseIntegration
    {
        public override Int64 Set(string parametersArr)
        {
            try
            {
                List<Foodics.NetSuite.Shared.Model.Customer> Lst_CustomAll = new GenericeDAO<Foodics.NetSuite.Shared.Model.Customer>().GetWhere("Netsuite_Id IS NULL or Netsuite_Id =0").Take(200).ToList();

                //List<Foodics.NetSuite.Shared.Model.Customer> Lst_CustomAll = new GenericeDAO<Foodics.NetSuite.Shared.Model.Customer>().GetWhere(" (Netsuite_Id IS NULL or Netsuite_Id =0) and  (Foodics_UpdateDate >= '"+ ConfigurationManager.AppSettings["InvoiceDate"] + "')").Take(2000).ToList();
                int Exe_length = 200;
                int lstend = Exe_length;
                if (Lst_CustomAll.Count > 0)
                {
                    for (int Index = 0; Index < Lst_CustomAll.Count; Index += Exe_length)
                    {
                        if (Index + Exe_length >= Lst_CustomAll.Count)
                            lstend = Lst_CustomAll.Count - Index;
                        List<Foodics.NetSuite.Shared.Model.Customer> Lst_Items = Lst_CustomAll.GetRange(Index, lstend);


                        com.netsuite.webservices.Customer[] ItemArr = new com.netsuite.webservices.Customer[Lst_Items.Count];
                        for (int i = 0; i < Lst_Items.Count; i++)
                        {
                            Foodics.NetSuite.Shared.Model.Customer Obj = Lst_Items[i];

                            com.netsuite.webservices.Customer NewItemObject = new com.netsuite.webservices.Customer();
                            string[] Fullname = Obj.name.Split(' ');
                            if (Fullname.Length > 0)
                                NewItemObject.firstName = Fullname[0];

                            if (Fullname.Length > 1)
                                NewItemObject.lastName = Obj.name.Remove(0, NewItemObject.firstName.Length);
                            else
                                NewItemObject.lastName = "--";


                            NewItemObject.isPerson = true;
                            NewItemObject.isPersonSpecified = true;
                            NewItemObject.email = Obj.email;
                            NewItemObject.phone = Obj.phone;

                            RecordRef subsidiary = new RecordRef();
                            subsidiary.internalId = Obj.Subsidiary_Id.ToString();
                            subsidiary.type = RecordType.subsidiary;
                            NewItemObject.subsidiary = subsidiary;

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
                            UpdatedLst(Lst_Items, wr);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
            }
            return 0;
        }

        private void UpdatedLst(List<Foodics.NetSuite.Shared.Model.Customer> Lst_Items, WriteResponseList responseLst)
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
            objDAO.UpdateNetsuiteIDs(iDs, "Customer");
        }
        public override void Get()
        {
            /// <summary> This method get all items (with types we need in POS) from netsuite and check item type, 
            /// after that get all item info and save in DB.</summary>	

        }
    }
}
