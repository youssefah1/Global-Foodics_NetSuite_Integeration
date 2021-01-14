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
    public class SettingTask : NetSuiteBaseIntegration
    {
        //protected List<Model.ItemSubsidiary> subsidiaryListLocal = new List<Model.ItemSubsidiary>();
        //protected List<Model.Item> itemWithImages = new List<Model.Item>();
        //protected List<Model.ItemOptionListValue> ItemOptionList = new List<Model.ItemOptionListValue>();
        public override Int64 Set(string parametersArr)
        {

            return 0;
        }


        public override void Get()
        {
            /// <summary> This method get all items (with types we need in POS) from netsuite and check item type, 
            /// after that get all item info and save in DB.</summary>	
            /// 

            string IntrnelID = GetCustomizationId("customrecord_da_foodics_settings");





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
                    List<Setting.integerate> lstsetting = new List<Setting.integerate>();
                    for (int i = 0; i < response.recordList.Length; i++)
                    {
                        item_Custom = (CustomRecord)response.recordList[i];
                        flds = item_Custom.customFieldList;

                        Setting.integerate settingObj = new Setting.integerate();
                        settingObj.Netsuite_Id = Utility.ConvertToInt(item_Custom.internalId);

                        for (int c = 0; c < flds.Length; c++)
                        {

                            if (flds[c].scriptId == "custrecord_da_foodics_tax_rate")
                                settingObj.TaxRate = Utility.ConvertToInt(((com.netsuite.webservices.DoubleCustomFieldRef)flds[c]).value.ToString());
                            if (flds[c].scriptId == "custrecord_da_foodics_currency")
                                settingObj.Currency_Netsuite_Id = Utility.ConvertToInt(((com.netsuite.webservices.SelectCustomFieldRef)flds[c]).value.internalId.ToString());
                            if (flds[c].scriptId == "custrecord_da_foodics_subsidiary")
                                settingObj.Subsidiary_Netsuite_Id = Utility.ConvertToInt(((com.netsuite.webservices.SelectCustomFieldRef)flds[c]).value.internalId.ToString());
                            if (flds[c].scriptId == "custrecord_da_tax_code")
                                settingObj.TaxCode_Netsuite_Id = Utility.ConvertToInt(((com.netsuite.webservices.SelectCustomFieldRef)flds[c]).value.internalId.ToString());
                            if (flds[c].scriptId == "custrecord_da_foodics_default_customer")
                                settingObj.Customer_Netsuite_Id = Utility.ConvertToInt(((com.netsuite.webservices.SelectCustomFieldRef)flds[c]).value.internalId.ToString());
                        }
                        lstsetting.Add(settingObj);
                    }


                    new GenericeDAO<Setting.integerate>().BaseNetSuiteIntegration(lstsetting, "Setting");




                }

            }
        }
    }
}