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
    public class LocationTask : NetSuiteBaseIntegration
    {
        public override Int64 Set(string parametersArr)
        {
            List<Foodics.NetSuite.Shared.Model.Location> Lst_Items = new GenericeDAO<Foodics.NetSuite.Shared.Model.Location>().GetWhere("Netsuite_Id IS NULL or Netsuite_Id =0");
         
            if (Lst_Items.Count <= 0)
                return 0;

            try
            {

                
                com.netsuite.webservices.Location[] ItemArr = new com.netsuite.webservices.Location[Lst_Items.Count];
                for (int i = 0; i < Lst_Items.Count; i++)
                {
                    Foodics.NetSuite.Shared.Model.Location Obj = Lst_Items[i];
                    RecordRef[] subsidiarylst = new RecordRef[1];
                    com.netsuite.webservices.Location NewItemObject = new com.netsuite.webservices.Location();
                    NewItemObject.name = Obj.Name_En.Length <30? Obj.Name_En: Obj.Name_En.Substring(0,30);
                    NewItemObject.latitude = Utility.ConvertToDouble(Obj.Latitude);
                    NewItemObject.longitude = Utility.ConvertToDouble(Obj.Longitude);
                    
                    RecordRef subsidiary = new RecordRef();
                    subsidiary.internalId = Obj.Subsidiary_Id.ToString();
                    subsidiary.type = RecordType.subsidiary;
                    subsidiarylst[0] = subsidiary;
                    NewItemObject.subsidiaryList = subsidiarylst;

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

        private void UpdatedLst(List<Foodics.NetSuite.Shared.Model.Location> Lst_Items, WriteResponseList responseLst)
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

                GenericeDAO<Location> objDAO = new GenericeDAO<Location>();
                //updates local db
                objDAO.UpdateNetsuiteIDs(iDs, "Location");
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

    }
}
}
