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
    public class ProductTask : NetSuiteBaseIntegration
    {
        public override Int64 Set(string parametersArr)
        {
            try
            {
                new CustomDAO().UpdateProductCompnent();
                //object fromDateObj = new GenericeDAO<Item>().GetLatestModifiedDate();
                //DateTime fromDate = new DateTime();
                //if (fromDateObj == null)
                //{
                //    fromDate = DateTime.Now.AddYears(-1);
                //}
                //else
                //{
                //    fromDate = (DateTime)fromDateObj;
                //}
                //fromDate = fromDate.AddDays(-2);

                List<Item> Lst_ItemsAll = new GenericeDAO<Item>().GetWhere("  (Netsuite_Id IS NULL or Netsuite_Id =0) and (Foodics_Id in (select ItemFoodics_Id from ItemCompnent))   and Item_Type=" + (int)Item_Type.AssemblyItem);

                List<Item> Lst_ItemsUpdate = Lst_ItemsAll.Where(x => x.Netsuite_Id > 0).ToList();
                List<Item> Lst_ItemsNew = Lst_ItemsAll.Where(x => x.Netsuite_Id == 0 || x.Netsuite_Id < 0).ToList();
                if (Lst_ItemsAll.Count <= 0)
                    return 0;
                // Send order list to netsuite
                if (Lst_ItemsNew.Count > 0)
                {
                    com.netsuite.webservices.AssemblyItem[] ItemArrNew = GenerateNetSuitelst(Lst_ItemsNew);

                    WriteResponseList wrNew = Service(true).addList(ItemArrNew);
                    bool result = wrNew.status.isSuccess;
                    if (result)
                    {
                        //Update database with returned Netsuite ids
                        UpdatedLst(Lst_ItemsNew, wrNew);
                    }
                }

                // Send order list to netsuite
                if (Lst_ItemsUpdate.Count > 0)
                {
                  //  com.netsuite.webservices.AssemblyItem[] ItemArrAdd = GenerateNetSuitelst(Lst_ItemsUpdate);

                  //  WriteResponseList wr = Service(true).updateList(ItemArrAdd);
                //    bool result = wr.status.isSuccess;

                }
            }
            catch (Exception ex)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
            }

            return 0;
        }

        public com.netsuite.webservices.AssemblyItem[] GenerateNetSuitelst(List<Item> Lst_Items)
        {
            RecordRef[] subsidiarylst = new RecordRef[1];
            Price[] pricelst = new Price[1];
            Pricing[] Pricinglst = new Pricing[1];
            com.netsuite.webservices.AssemblyItem[] ItemArr = new com.netsuite.webservices.AssemblyItem[Lst_Items.Count];
            try
            {
                for (int i = 0; i < Lst_Items.Count; i++)
                {
                    Item Obj = Lst_Items[i];
                    Setting objSetting = new GenericeDAO<Setting>().GetWhere("Subsidiary_Netsuite_Id=" + Obj.Subsidiary_Id).FirstOrDefault();
                    com.netsuite.webservices.AssemblyItem NewItemObject = new com.netsuite.webservices.AssemblyItem();
                    
                    NewItemObject.displayName = Obj.Display_Name_En;
                    //NewItemObject.itemId = Obj.UPC_Code;
                    NewItemObject.itemId = Obj.Display_Name_En;
                    
                    RecordRef classref = new RecordRef();
                    classref.internalId = Obj.Category_Id.ToString();
                    classref.type = RecordType.classification;
                    NewItemObject.@class = classref;

                    #region Custom fields

                    Obj.Storage_Unit = string.IsNullOrEmpty(Obj.Storage_Unit) ? "" : Obj.Storage_Unit;

                    CustomFieldRef[] custFieldList = new CustomFieldRef[] {

                        new StringCustomFieldRef {
                            value = !string.IsNullOrEmpty(Obj.Name_Ar)? Obj.Name_Ar.ToString():"",
                            scriptId = "custitem_da_item_name_ar"
                        }
                        ,
                         new StringCustomFieldRef {
                            value = Obj.UPC_Code.ToString(),
                            scriptId = "custitem1"
                        }
                        };
                    NewItemObject.customFieldList = custFieldList;
                    #endregion
                    if (Obj.Netsuite_Id > 0)
                        NewItemObject.internalId = Obj.Netsuite_Id.ToString();

                    #region pricing 

                    NewItemObject.pricingMatrix = Helper.GeneratePricingMatrix(objSetting, Obj.Price);
                    #endregion
                    #region AssemblyComponent

                    List<ItemCompnent> itemCompnentslst = new GenericeDAO<ItemCompnent>().GetWhere("ItemFoodics_Id ='" + Obj.Foodics_Id + "'");
                    if (itemCompnentslst.Count > 0)
                    {
                        ItemMember[] ItemMemberlst = new ItemMember[itemCompnentslst.Count];
                        ItemMemberList memberlst = new ItemMemberList();
                        for (int x = 0; x < itemCompnentslst.Count; x++)
                        {
                            ItemMember obj = new ItemMember();
                            ItemCompnent itmcompobj = itemCompnentslst[x];
                            RecordRef Itemref = new RecordRef();
                            Itemref.internalId = itmcompobj.ComponentId.ToString();
                            Itemref.type = RecordType.inventoryItem;

                            obj.item = Itemref;
                            obj.quantity = itmcompobj.Quantity;
                            obj.quantitySpecified = true;
                            ItemMemberlst[x] = obj;
                        }
                        memberlst.itemMember = ItemMemberlst;
                        NewItemObject.memberList = memberlst;
                        NewItemObject.memberList.replaceAll = false;
                    }
                    else
                    {
                        //ItemMember[] ItemMemberlst = new ItemMember[1];
                        //ItemMemberList memberlst = new ItemMemberList();

                        //ItemMember obj = new ItemMember();

                        //RecordRef Itemref = new RecordRef();
                        //Itemref.internalId = objSetting.Item_Tmp_Netsuite_Id.ToString();
                        //Itemref.type = RecordType.inventoryItem;

                        //obj.item = Itemref;
                        //obj.quantity = 10;

                        //ItemMemberlst[0] = obj;


                        //memberlst.itemMember = ItemMemberlst;
                        //NewItemObject.memberList = memberlst;
                    }


                    #endregion
                    RecordRef Tax_Schedule = new RecordRef();
                    Tax_Schedule.internalId = objSetting.TaxSchedule_Netsuite_Id.ToString();
                    Tax_Schedule.type = RecordType.salesTaxItem;
                    NewItemObject.taxSchedule = Tax_Schedule;

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
            }
            GenericeDAO<Item> objDAO = new GenericeDAO<Item>();
            //updates local db
            objDAO.UpdateNetsuiteIDs(iDs, "Item");
        }
        public override void Get()
        {
            /// <summary> This method get all items (with types we need in POS) from netsuite and check item type, 
            /// after that get all item info and save in DB.</summary>	

        }
    }
}
