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
    public class ProductClassTask : NetSuiteBaseIntegration
    {
        //protected List<Model.ItemSubsidiary> subsidiaryListLocal = new List<Model.ItemSubsidiary>();
        //protected List<Model.Item> itemWithImages = new List<Model.Item>();
        //protected List<Model.ItemOptionListValue> ItemOptionList = new List<Model.ItemOptionListValue>();
        public override Int64 Set(string parametersArr)
        {
            List<Foodics.NetSuite.Shared.Model.Categories> Lst_Items = new GenericeDAO<Foodics.NetSuite.Shared.Model.Categories>().GetWhere("Netsuite_Id IS NULL or Netsuite_Id =0");
            
            if (Lst_Items.Count <= 0)
                return 0;
            RecordRef[] subsidiarylst = new RecordRef[1];
            com.netsuite.webservices.Classification[] ItemArr = new com.netsuite.webservices.Classification[Lst_Items.Count];
            for (int i = 0; i < Lst_Items.Count; i++)
           // for (int i = 0; i < 1; i++)
            {
                Foodics.NetSuite.Shared.Model.Categories Obj = Lst_Items[i];

                com.netsuite.webservices.Classification NewItemObject = new com.netsuite.webservices.Classification();


                //NewItemObject.internalId = intrnlID;
                NewItemObject.name = Obj.name;



                


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
            // Send order list to netsuite
            WriteResponseList wr = Service(true).addList(ItemArr);
            bool result = wr.status.isSuccess;
            if (result)
            {
                //Update database with returned Netsuite ids
                 UpdatedLst(Lst_Items, wr);
            }
            return 0;
        }

        private void UpdatedLst(List<Foodics.NetSuite.Shared.Model.Categories> Lst_Items, WriteResponseList responseLst)
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
            }
            GenericeDAO<Categories> objDAO = new GenericeDAO<Categories>();
            //updates local db
            objDAO.UpdateNetsuiteIDs(iDs, "Categories");
        }
        public override void Get()
        {
            /// <summary> This method get all items (with types we need in POS) from netsuite and check item type, 
            /// after that get all item info and save in DB.</summary>	

        }
    }
}
