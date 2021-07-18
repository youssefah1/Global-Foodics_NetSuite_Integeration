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
            List<Foodics.NetSuite.Shared.Model.Categories.FoodicsCategories> Lst_Items = new GenericeDAO<Foodics.NetSuite.Shared.Model.Categories.FoodicsCategories>().GetWhere("Netsuite_Id IS NULL or Netsuite_Id =0");
            
            if (Lst_Items.Count <= 0)
                return 0;
            RecordRef[] subsidiarylst = new RecordRef[1];
            com.netsuite.webservices.Classification[] ItemArr = new com.netsuite.webservices.Classification[Lst_Items.Count];
            for (int i = 0; i < Lst_Items.Count; i++)
            {
                Foodics.NetSuite.Shared.Model.Categories.FoodicsCategories Obj = Lst_Items[i];
                com.netsuite.webservices.Classification NewItemObject = new com.netsuite.webservices.Classification();

                NewItemObject.name = Obj.name.Length > 30 ? Obj.name.Substring(0, 30) : Obj.name;
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
            new CustomDAO().InvoiceRelatedUpdate();
            new CustomDAO().SetItemClass();

            return 0;
        }

        private void UpdatedLst(List<Foodics.NetSuite.Shared.Model.Categories.FoodicsCategories> Lst_Items, WriteResponseList responseLst)
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
            /// 

            string IntrnelID = GetCustomizationId("customrecord_da_foodics_class_account");





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
                    List<Categories.CategoriesAccounts> lstCat = new List<Categories.CategoriesAccounts>();
                    for (int i = 0; i < response.recordList.Length; i++)
                    {
                        item_Custom = (CustomRecord)response.recordList[i];
                        flds = item_Custom.customFieldList;


                        Categories.CategoriesAccounts CatObj = new Categories.CategoriesAccounts();
                        for (int c = 0; c < flds.Length; c++)
                        {
                            

                            if (flds[c].scriptId == "custrecord_da_item_class")
                                CatObj.Netsuite_Id = Utility.ConvertToInt(((com.netsuite.webservices.SelectCustomFieldRef)flds[c]).value.internalId.ToString());

                            if (flds[c].scriptId == "custrecord_cogs_account")
                                CatObj.cogs_account = Utility.ConvertToInt(((com.netsuite.webservices.SelectCustomFieldRef)flds[c]).value.internalId.ToString());
                            if (flds[c].scriptId == "custrecord_inter_cogs_account")
                                CatObj.inter_cogs_account = Utility.ConvertToInt(((com.netsuite.webservices.SelectCustomFieldRef)flds[c]).value.internalId.ToString());

                            if (flds[c].scriptId == "custrecord_da_income_account")
                                CatObj.income_account = Utility.ConvertToInt(((com.netsuite.webservices.SelectCustomFieldRef)flds[c]).value.internalId.ToString());
                            if (flds[c].scriptId == "custrecord_da_gainloss_account")
                                CatObj.gainloss_account = Utility.ConvertToInt(((com.netsuite.webservices.SelectCustomFieldRef)flds[c]).value.internalId.ToString());

                            if (flds[c].scriptId == "custrecord_da_assetaccount")
                                CatObj.asset_account = Utility.ConvertToInt(((com.netsuite.webservices.SelectCustomFieldRef)flds[c]).value.internalId.ToString());
                            if (flds[c].scriptId == "custrecord_cust_income_intercompany")
                                CatObj.income_intercompany_account = Utility.ConvertToInt(((com.netsuite.webservices.SelectCustomFieldRef)flds[c]).value.internalId.ToString());

                            if (flds[c].scriptId == "custrecord_cust_da_price_variance")
                                CatObj.price_variance_account = Utility.ConvertToInt(((com.netsuite.webservices.SelectCustomFieldRef)flds[c]).value.internalId.ToString());
                            if (flds[c].scriptId == "custrecord_da_cust_qty_variance")
                                CatObj.cust_qty_variance_account = Utility.ConvertToInt(((com.netsuite.webservices.SelectCustomFieldRef)flds[c]).value.internalId.ToString());

                            if (flds[c].scriptId == "custrecord_da_cust_ex_rate")
                                CatObj.cust_ex_rate_account = Utility.ConvertToInt(((com.netsuite.webservices.SelectCustomFieldRef)flds[c]).value.internalId.ToString());
                            if (flds[c].scriptId == "custrecord_da_cust_customer_account_vari")
                                CatObj.customer_vari_account = Utility.ConvertToInt(((com.netsuite.webservices.SelectCustomFieldRef)flds[c]).value.internalId.ToString());

                            if (flds[c].scriptId == "custrecord_da_cust_vendor")
                                CatObj.cust_vendor_account = Utility.ConvertToInt(((com.netsuite.webservices.SelectCustomFieldRef)flds[c]).value.internalId.ToString());

                        }
                        lstCat.Add(CatObj);
                    }


                    new GenericeDAO<Categories.CategoriesAccounts>().BaseNetSuiteIntegration(lstCat, "Categories");




                }

            }

        }
    }
}
