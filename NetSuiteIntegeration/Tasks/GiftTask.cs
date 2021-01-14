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
    public class GiftTask : NetSuiteBaseIntegration
    {
        //protected List<Model.ItemSubsidiary> subsidiaryListLocal = new List<Model.ItemSubsidiary>();
        //protected List<Model.Item> itemWithImages = new List<Model.Item>();
        //protected List<Model.ItemOptionListValue> ItemOptionList = new List<Model.ItemOptionListValue>();
        public override Int64 Set(string parametersArr)
        {
            List<Item> Lst_Items = new GenericeDAO<Item>().GetWhere(" (Netsuite_Id IS NULL or Netsuite_Id =0) and Item_Type="+(int)Item_Type.GiftCertificate);
            Setting objSetting = new GenericeDAO<Setting>().GetAll().FirstOrDefault();
            if (objSetting == null )
                return 0;
            if (Lst_Items.Count <= 0)
                return 0;




            RecordRef[] subsidiarylst = new RecordRef[1];
            Price[] pricelst = new Price[1];
            Pricing[] Pricinglst = new Pricing[1];
            // AssemblyComponentList assemplyList = new AssemblyComponentList();
            ItemMemberList memberlst = new ItemMemberList();
            ItemMember[] ItemMemberlst = new ItemMember[1];
            com.netsuite.webservices.GiftCertificateItem[] ItemArr = new com.netsuite.webservices.GiftCertificateItem[Lst_Items.Count];
            for (int i = 0; i < Lst_Items.Count; i++)
            //for (int i = 0; i < 1; i++)
            {
                Item Obj = Lst_Items[i];
                
                com.netsuite.webservices.GiftCertificateItem NewItemObject = new com.netsuite.webservices.GiftCertificateItem();
                NewItemObject.displayName = Obj.Display_Name_En;
                NewItemObject.itemId = Obj.UPC_Code;

                #region Custom fields

                if (!string.IsNullOrEmpty(Obj.Storage_Unit))
                {


                    CustomFieldRef[] custFieldList = new CustomFieldRef[] {

                        new StringCustomFieldRef {
                            value = Obj.Storage_Unit.ToString(),
                            scriptId = "custitem_da_it_uom"
                        },

                    };
                    NewItemObject.customFieldList = custFieldList;
                }

                #endregion

        
                //obj.


                #region AssemblyComponent
               // AssemblyComponent obj = new AssemblyComponent();
                //obj.


                
                //ItemMember obj = new ItemMember();

                //RecordRef Itemref = new RecordRef();
                //Itemref.internalId = "612";//objSetting.Subsidiary_Netsuite_Id.ToString();
                //Itemref.type = RecordType.inventoryItem;

                //obj.item = Itemref;
                //obj.quantity = 1;




                //ItemMemberlst[0] = obj;


                //memberlst.itemMember = ItemMemberlst;
                //NewItemObject.memberList = memberlst;

                //NewItemObject.item
                ////NewItemObject.component
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


                RecordRef Liability = new RecordRef();
                Liability.internalId = objSetting.LiabilityAccount_Netsuite_Id.ToString();
                Liability.type = RecordType.subsidiary;
                NewItemObject.liabilityAccount = Liability;


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
