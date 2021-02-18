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
    public class CategoriesTask : NetSuiteBaseIntegration
    {
        //protected List<Model.ItemSubsidiary> subsidiaryListLocal = new List<Model.ItemSubsidiary>();
        //protected List<Model.Item> itemWithImages = new List<Model.Item>();
        //protected List<Model.ItemOptionListValue> ItemOptionList = new List<Model.ItemOptionListValue>();
        public override Int64 Set(string parametersArr)
        {
            //List<Foodics.NetSuite.Shared.Model.Categories.FoodicsCategories> Lst_Items = new GenericeDAO<Foodics.NetSuite.Shared.Model.Categories.FoodicsCategories>().GetWhere("Netsuite_Id IS NULL or Netsuite_Id =0");
            //if (Lst_Items.Count <= 0)
            //    return 0;



            //string intrnlID = GetCustomizationId("customrecord_da_foodics_category");
            //com.netsuite.webservices.CustomRecord[] ItemArr = new com.netsuite.webservices.CustomRecord[Lst_Items.Count];
            //for (int i = 0; i < Lst_Items.Count; i++)
            //{
            //    Foodics.NetSuite.Shared.Model.Categories.FoodicsCategories Obj = Lst_Items[i];
            //    com.netsuite.webservices.CustomRecord NewItemObject = new com.netsuite.webservices.CustomRecord();
            //    NewItemObject.name = Obj.name;
            //    RecordRef recType = new RecordRef();
            //    recType.internalId = intrnlID;
            //    recType.type = RecordType.customRecord;
            //    recType.typeSpecified = true;
            //    NewItemObject.recType = recType;
            //    if (Obj.InActive)
            //    {
            //        NewItemObject.isInactive = true;
            //        NewItemObject.isInactiveSpecified = true;
            //    }
            //    ItemArr[i] = NewItemObject;
            //}
            //WriteResponseList wr = Service(true).addList(ItemArr);
            //bool result = wr.status.isSuccess;
            //if (result)
            //{
            //    //Update database with returned Netsuite ids
            //     UpdatedLst(Lst_Items, wr);
            //}
            return 0;
        }

        private void UpdatedLst(List<Foodics.NetSuite.Shared.Model.Categories.FoodicsCategories> Lst_Items, WriteResponseList responseLst)
        {
            //List<Tuple<int, string>> iDs = new List<Tuple<int, string>>();
            //try
            //{
            //    for (int counter = 0; counter < Lst_Items.Count; counter++)
            //    {
            //        if (responseLst.writeResponse[counter].status.isSuccess)
            //        {

            //            CustomRecordRef rf = (CustomRecordRef)responseLst.writeResponse[counter].baseRef;
            //            Lst_Items[counter].Netsuite_Id = Convert.ToInt32(rf.internalId.ToString());
            //            iDs.Add(new Tuple<int, string>(Convert.ToInt32(rf.internalId.ToString()), Lst_Items[counter].Foodics_Id));
            //        }

            //    }
            //}
            //catch (Exception ex)
            //{
            //    LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
            //}
            //GenericeDAO<Categories> objDAO = new GenericeDAO<Categories>();
            //objDAO.UpdateNetsuiteIDs(iDs, "Categories");
        }

        public override void Get()
        {
            /// <summary> This method get all items (with types we need in POS) from netsuite and check item type, 
            /// after that get all item info and save in DB.</summary>	

        }
    }
}
