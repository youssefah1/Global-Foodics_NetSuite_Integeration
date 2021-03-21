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
    public class ItemTask : NetSuiteBaseIntegration
    {
        public override Int64 Set(string parametersArr)
        {
           

                
                List<Item> Lst_ItemsAll = new GenericeDAO<Item>().GetWhere(" (Netsuite_Id IS NULL or Netsuite_Id =0) and inactive=0 and  Item_Type=" + (int)Item_Type.InventoryItem).Take(200).ToList();
                //List<Item> Lst_ItemsAll = new GenericeDAO<Item>().GetWhere(" id>=1200 and inactive=0 and Item_Type=" + (int)Item_Type.InventoryItem).Take(100).ToList();

            List<Item> Lst_ItemsUpdate = Lst_ItemsAll.Where(x => x.Netsuite_Id > 0).ToList();
            List<Item> Lst_ItemsNew = Lst_ItemsAll.Where(x => x.Netsuite_Id == 0 || x.Netsuite_Id < 0).ToList();

            if (Lst_ItemsAll.Count <= 0)
                return 0;

            // Send order list to netsuite
            if (Lst_ItemsNew.Count > 0)
            {
                com.netsuite.webservices.InventoryItem[] ItemArrNew = GenerateNetSuitelst(Lst_ItemsNew);
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
                com.netsuite.webservices.InventoryItem[] ItemArrAdd = GenerateNetSuitelst(Lst_ItemsUpdate);
                // Send order list to netsuite
                WriteResponseList wr = Service(true).updateList(ItemArrAdd);
            }

            return 0;
        }
        public com.netsuite.webservices.InventoryItem[] GenerateNetSuitelst(List<Item> Lst_ItemsAll)
        {
            com.netsuite.webservices.InventoryItem[] ItemArr = new com.netsuite.webservices.InventoryItem[Lst_ItemsAll.Count];
            try
            {
                for (int i = 0; i < Lst_ItemsAll.Count; i++)
                {
                    RecordRef[] subsidiarylst = new RecordRef[1];
                    Item Obj = Lst_ItemsAll[i];
                    Setting objSetting = new GenericeDAO<Setting>().GetWhere("Subsidiary_Netsuite_Id=" + Obj.Subsidiary_Id).FirstOrDefault();
                    Categories.CategoriesAccounts objCatAccount = new Categories.CategoriesAccounts();
                    com.netsuite.webservices.InventoryItem NewItemObject = new com.netsuite.webservices.InventoryItem();
                    if (Obj.Netsuite_Id <= 0)
                    {
                        NewItemObject.displayName = Obj.Display_Name_En;
                        //NewItemObject.itemId = Obj.UPC_Code;
                        NewItemObject.itemId = Obj.Display_Name_En;
                        NewItemObject.salesDescription = Obj.Display_Name_En;
                    }
                    //check if new or can be updated
                    if (Obj.Netsuite_Id > 0)
                        NewItemObject.internalId = Obj.Netsuite_Id.ToString();

                    NewItemObject.trackLandedCost = true;
                    NewItemObject.trackLandedCostSpecified = true;

                    NewItemObject.pricingMatrix = Helper.GeneratePricingMatrix(objSetting, Obj.Price);
                    if (Obj.Category_Id > 0)
                    {
                        objCatAccount = new GenericeDAO<Categories.CategoriesAccounts>().GetWhere("Netsuite_Id=" + Obj.Category_Id).FirstOrDefault();
                        RecordRef classref = new RecordRef();
                        classref.internalId = Obj.Category_Id.ToString();
                        classref.type = RecordType.classification;
                        NewItemObject.@class = classref;
                    }
                    CustomFieldRef[] custFieldList = new CustomFieldRef[] {

                        new StringCustomFieldRef {
                            value =   Obj.Name_Ar!= null? Obj.Name_Ar:"",
                            scriptId = "custitem_da_item_name_ar"
                        }
                        ,
                         new StringCustomFieldRef {
                            value = Obj.UPC_Code.ToString(),
                            scriptId = "custitem1"
                        }
                         ,
                         new StringCustomFieldRef {
                              value = Obj.storage_to_ingredient_factor!= null? Obj.storage_to_ingredient_factor:"",
                            scriptId = "custitem_da_item_ingredient"
                        }
                        };
                    NewItemObject.customFieldList = custFieldList;
                    UnitsOfMeasure objunit = new UnitsOfMeasure();
                    if (!string.IsNullOrEmpty(Obj.Ingredient_Unit))
                    {
                        List<UnitsOfMeasure> unitlst = new GenericeDAO<UnitsOfMeasure>().GetWhere("Name ='" + Obj.Ingredient_Unit + "' and abbreviation ='" + Obj.Storage_Unit + "' and conversionRate ='" + Obj.storage_to_ingredient_factor + "'");
                        if (unitlst.Count > 0)
                            objunit = unitlst[0];
                        else
                        {
                            unitlst = new GenericeDAO<UnitsOfMeasure>().GetWhere("Name ='" + Obj.Ingredient_Unit + "' and abbreviation ='" + Obj.Storage_Unit + "'");
                            if (unitlst.Count > 0)
                                objunit = unitlst[0];
                        }
                    }
                    else if (!string.IsNullOrEmpty(Obj.Storage_Unit))
                    {
                        List<UnitsOfMeasure> unitlst = new GenericeDAO<UnitsOfMeasure>().GetWhere("unitName ='" + Obj.Storage_Unit + "' and abbreviation ='" + Obj.Storage_Unit + "' and conversionRate ='" + Obj.storage_to_ingredient_factor + "'");
                        if (unitlst.Count > 0)
                            objunit = unitlst[0];

                    }

                    if (objunit != null && objunit.Id > 0)
                    {
                        RecordRef unitsTyperef = new RecordRef();
                        unitsTyperef.internalId = objunit.unit_id.ToString();
                        unitsTyperef.type = RecordType.unitsType;
                        NewItemObject.unitsType = unitsTyperef;

                        RecordRef unitsTyperefother = new RecordRef();
                        unitsTyperefother.internalId = objunit.details_id.ToString();
                        NewItemObject.saleUnit = unitsTyperefother;
                        NewItemObject.stockUnit = unitsTyperefother;
                        NewItemObject.purchaseUnit = unitsTyperefother;

                    }


                    #region Items Account
                    RecordRef IncomAccountref = new RecordRef();
                    IncomAccountref.type = RecordType.account;
                    NewItemObject.incomeAccount = IncomAccountref;
                    if (objCatAccount.income_account > 0)
                        IncomAccountref.internalId = objCatAccount.income_account.ToString();
                    else
                        IncomAccountref.internalId = objSetting.IncomeAccount_Netsuite_Id.ToString();

                    RecordRef cogsAccountref = new RecordRef();
                    cogsAccountref.type = RecordType.account;
                    NewItemObject.cogsAccount = cogsAccountref;
                    if (objCatAccount.cogs_account > 0)
                        cogsAccountref.internalId = objCatAccount.cogs_account.ToString();
                    else
                        cogsAccountref.internalId = objSetting.CogsAccount_Netsuite_Id.ToString();

                    RecordRef assetAccountref = new RecordRef();
                    assetAccountref.type = RecordType.account;
                    NewItemObject.assetAccount = assetAccountref;
                    if (objCatAccount.asset_account > 0)
                        assetAccountref.internalId = objCatAccount.asset_account.ToString();
                    else
                        assetAccountref.internalId = objSetting.AssetAccount_Netsuite_Id.ToString();

                    RecordRef intercoIncomeref = new RecordRef();
                    intercoIncomeref.type = RecordType.account;
                    NewItemObject.intercoIncomeAccount = intercoIncomeref;
                    if (objCatAccount.income_intercompany_account > 0)
                        intercoIncomeref.internalId = objCatAccount.income_intercompany_account.ToString();
                    else
                        intercoIncomeref.internalId = objSetting.IntercoIncomeAccount_Netsuite_Id.ToString();

                    RecordRef intercoCogsAccount = new RecordRef();
                    intercoCogsAccount.type = RecordType.account;
                    NewItemObject.intercoCogsAccount = intercoCogsAccount;
                    if (objCatAccount.inter_cogs_account > 0)
                        intercoCogsAccount.internalId = objCatAccount.inter_cogs_account.ToString();
                    else
                        intercoCogsAccount.internalId = objSetting.IntercoCogsAccount_Netsuite_Id.ToString();

                    RecordRef gainLossAccount = new RecordRef();
                    gainLossAccount.type = RecordType.account;
                    NewItemObject.gainLossAccount = gainLossAccount;
                    if (objCatAccount.gainloss_account > 0)
                        gainLossAccount.internalId = objCatAccount.gainloss_account.ToString();
                    else
                        gainLossAccount.internalId = objSetting.GainLossAccount_Netsuite_Id.ToString();

                    //if (objCatAccount.price_variance_account > 0)//You do not have permissions to set a value for element purchasepricevarianceacct 
                    //{
                    //    RecordRef PriceAccount = new RecordRef();
                    //    PriceAccount.type = RecordType.account;
                    //    PriceAccount.internalId = objCatAccount.price_variance_account.ToString();
                    //    NewItemObject.purchasePriceVarianceAcct = PriceAccount;
                    //}

                    if (objCatAccount.cust_qty_variance_account > 0)
                    {
                        RecordRef refgAccount = new RecordRef();
                        refgAccount.type = RecordType.account;
                        refgAccount.internalId = objCatAccount.cust_qty_variance_account.ToString();
                        NewItemObject.billQtyVarianceAcct = refgAccount;
                    }

                    if (objCatAccount.cust_ex_rate_account > 0)
                    {
                        RecordRef refgAccount = new RecordRef();
                        refgAccount.type = RecordType.account;
                        refgAccount.internalId = objCatAccount.cust_ex_rate_account.ToString();
                        NewItemObject.billExchRateVarianceAcct = refgAccount;
                    }
                    if (objCatAccount.price_variance_account > 0)
                    {
                        RecordRef refgAccount = new RecordRef();
                        refgAccount.type = RecordType.account;
                        refgAccount.internalId = objCatAccount.price_variance_account.ToString();
                        NewItemObject.billPriceVarianceAcct = refgAccount;
                    }
                  
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
        //public override void Get()
        //{
        //    ItemSearch custSearch = new ItemSearch();
        //    ItemSearchBasic custSearchBasic = new ItemSearchBasic();

        //    object fromDateObj = new GenericeDAO<Item>().GetLatestModifiedDate();
        //    DateTime fromDate = new DateTime();
        //    custSearchBasic.lastModifiedDate = new SearchDateField();
        //    if (fromDateObj == null)
        //    {
        //        fromDate = DateTime.Now;
        //        custSearchBasic.lastModifiedDate.@operator = SearchDateFieldOperator.before;
        //    }
        //    else
        //    {
        //        custSearchBasic.lastModifiedDate.@operator = SearchDateFieldOperator.notBefore;
        //        fromDate = (DateTime)fromDateObj;
        //        fromDate = fromDate.AddDays(-2);
        //    }
        //    custSearchBasic.lastModifiedDate.operatorSpecified = true;
        //    custSearchBasic.lastModifiedDate.searchValue = fromDate;
        //    custSearchBasic.lastModifiedDate.searchValueSpecified = true;

        //    SearchMultiSelectField fileFilter = new SearchMultiSelectField();
        //    fileFilter.@operator = SearchMultiSelectFieldOperator.anyOf;
        //    fileFilter.operatorSpecified = true;
        //    RecordRef[] rec = new RecordRef[1];

        //    rec[0] = new RecordRef();
        //    rec[0].internalId = "2463";
        //    fileFilter.searchValue = rec;
        //    custSearchBasic.internalId = fileFilter;


        //    custSearch.basic = custSearchBasic;
        //    SearchResult response = null;
        //    List<ItemCompnent> ItemCompnentList = new List<ItemCompnent>();
        //    int pageIndex = 0;
        //    do
        //    {
        //        pageIndex++;
        //        Preferences prefs = new Preferences();
        //        Service(true).preferences = prefs;
        //        prefs.warningAsErrorSpecified = true;
        //        prefs.warningAsError = false;
        //        SearchPreferences _srch_pref = new SearchPreferences();
        //        Service().searchPreferences = _srch_pref;
        //        Service().searchPreferences.bodyFieldsOnly = false;

        //        custSearch.basic = custSearchBasic;
        //        if (response == null)
        //        {
        //            response = Service().search(custSearch);
        //        }
        //        else // .searchMore returns the next page(s) of data.
        //        {
        //            response = Service().searchMoreWithId(response.searchId, pageIndex);
        //            //response = Service().searchMoreWithId(response.searchId, response.pageIndex);
        //        }
        //        //List<Model.Item> list = new List<Model.Item>();
        //        //List<Model.ItemPrice> priceList = new List<Model.ItemPrice>();



        //        foreach (Record record in response.recordList)
        //        {
        //            Type typeName = record.GetType();
        //            string tpName = typeName.Name;
        //            try
        //            {
        //                Item entity = new Item();
        //                PricingMatrix priceMatrix = new PricingMatrix();

        //                if (tpName == "AssemblyItem")
        //                {
        //                    object assemblyRec = (AssemblyItem)record;
        //                    AssemblyItem Assitem = (AssemblyItem)record;


        //                    foreach (var item in Assitem.memberList.itemMember)
        //                    {
        //                        ItemCompnent objitmcom = new ItemCompnent();

        //                        RecordRef itemref = new RecordRef();
        //                        itemref.type = RecordType.inventoryItem;
        //                        itemref = item.item;


        //                        objitmcom.UnitId = Utility.ConvertToInt(item.memberUnit);
        //                        objitmcom.Quantity = item.quantity;
        //                        objitmcom.ComponentId = Utility.ConvertToInt(itemref.internalId);
        //                        objitmcom.ItemId = Utility.ConvertToInt(Assitem.internalId);

        //                        ItemCompnentList.Add(objitmcom);

        //                    }
        //                }

        //            }
        //            catch (Exception ex)
        //            {
        //                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
        //            }
        //        }

        //    }
        //    while (response.pageIndex < response.totalPages);

        //    //string Itemids = string.Join(",", ItemCompnentList.Select(x => x.ItemId).Distinct().ToArray());
        //    //CustomDAO objCustomDAO = new CustomDAO();
        //    //objCustomDAO.DeleteNetsuiteItemsComponent(Itemids);
        //    new GenericeDAO<ItemCompnent>().ItemcompnentNetSuiteIntegration(ItemCompnentList);
        //}


        private Item GetItemInfo(object entity)
        {
            return new Item();
        }

        public override void Get()
        {

        }
    }
}
