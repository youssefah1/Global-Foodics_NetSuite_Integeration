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
    public class UnitsTypeTask : NetSuiteBaseIntegration
    {
        public override void Get()
        {
            UnitsTypeSearch custSearch = new UnitsTypeSearch();
            UnitsTypeSearchBasic custSearchBasic = new UnitsTypeSearchBasic();
            custSearch.basic = custSearchBasic;

            Preferences prefs = new Preferences();
            Service(true).preferences = prefs;
            prefs.warningAsErrorSpecified = true;
            prefs.warningAsError = false;
            SearchPreferences _srch_pref = new SearchPreferences();
            Service().searchPreferences = _srch_pref;
            Service().searchPreferences.bodyFieldsOnly = false;

            bool bodyonly = Service().searchPreferences.bodyFieldsOnly;

            SearchResult response = Service().search(custSearch);
            if (response.totalRecords > 0)
            {
                List<UnitsOfMeasure> list = new List<UnitsOfMeasure>();
                for (int i = 0; i < response.totalRecords; i++)
                {
                    UnitsType cr = (UnitsType)response.recordList[i];

                    try
                    {
                        UnitsTypeUomList uomList = cr.uomList;
                        for (int j = 0; j < uomList.uom.Length; j++)
                        {
                            UnitsOfMeasure entity = new UnitsOfMeasure();

                            entity.unit_id = Convert.ToInt32(cr.internalId);
                            entity.details_id = Convert.ToInt32(uomList.uom[j].internalId);
                            entity.Netsuite_Id = Convert.ToInt32(cr.internalId + uomList.uom[j].internalId);
                            entity.InActive = cr.isInactive;
                            entity.Name = cr.name;

                            entity.baseUnit = uomList.uom[j].baseUnit;
                            entity.unitName = uomList.uom[j].unitName;
                            entity.pluralName = uomList.uom[j].pluralName;

                            entity.abbreviation = uomList.uom[j].abbreviation;
                            entity.pluralAbbreviation = uomList.uom[j].pluralAbbreviation;
                            entity.conversionRate = (float)uomList.uom[j].conversionRate;
                            list.Add(entity);
                        }

                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                if (list.Count > 0)
                {
                    new GenericeDAO<UnitsOfMeasure>().UnitmeasureNetSuiteIntegration(list);
                }
            }
        }
        public override Int64 Set(string parametersArr)
        {
            try
            {

                //get recentrly added invoices after creating the return
                List<Foodics.NetSuite.Shared.Model.UnitsOfMeasure> ColLst = new GenericeDAO<Foodics.NetSuite.Shared.Model.UnitsOfMeasure>().GetWhere(" Netsuite_Id IS NULL or Netsuite_Id =0");
                List<Foodics.NetSuite.Shared.Model.UnitsOfMeasure> Lst_Update = new GenericeDAO<Foodics.NetSuite.Shared.Model.UnitsOfMeasure>().GetWhere(" [Id] in(select [UnitsOfMeasure_Id] from [dbo].[UnitsOfMeasureIngredient] where isnull([NetSuiteID],0) =0) ");
                if (ColLst.Count > 0)
                {
                    com.netsuite.webservices.UnitsType[] ArrNew = GenerateNetSuiteNewlst(ColLst);
                    WriteResponseList wrNew = Service(true).addList(ArrNew);
                    bool result = wrNew.status.isSuccess;
                    if (result)
                    {
                        Updatedlst(ColLst, wrNew);
                    }
                }
                if (Lst_Update.Count > 0)
                {
                    com.netsuite.webservices.UnitsType[] ArrAdd = GenerateNetSuiteUpdatelst(Lst_Update);
                    WriteResponseList wr = Service(true).updateList(ArrAdd);
                    bool result = wr.status.isSuccess;
                    if (result)
                    {
                        Updatedlstingredients(Lst_Update, wr);
                    }
                }
            }
            catch (Exception ex)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
            }



            return 0;
        }
        private UnitsType[] GenerateNetSuiteNewlst(List<UnitsOfMeasure> ColLst)
        {
            #region variables
            com.netsuite.webservices.UnitsType[] UnitsTypeArr = new com.netsuite.webservices.UnitsType[ColLst.Count];
            com.netsuite.webservices.UnitsType unitsTypeobj;
            UnitsTypeUomList UnitsTypeUomLst;
            UnitsTypeUom UnitsTypeUomobj;
            Foodics.NetSuite.Shared.Model.UnitsOfMeasure Obj_info;
            Foodics.NetSuite.Shared.Model.UnitsOfMeasureIngredient Obj_ingredient;
            #endregion

            for (int i = 0; i < ColLst.Count; i++)
            {
                try
                {
                    Obj_info = ColLst[i];

                    UnitsTypeUomLst = new UnitsTypeUomList();
                    UnitsTypeUomobj = new UnitsTypeUom();
                    unitsTypeobj = new UnitsType();

                    UnitsTypeUomobj.baseUnit = Obj_info.baseUnit;
                    UnitsTypeUomobj.baseUnitSpecified = true;

                    UnitsTypeUomobj.conversionRate = 1;
                    UnitsTypeUomobj.conversionRateSpecified = true;

                    UnitsTypeUomobj.abbreviation = Obj_info.abbreviation;
                    UnitsTypeUomobj.pluralAbbreviation = Obj_info.pluralAbbreviation;
                    UnitsTypeUomobj.pluralName = Obj_info.pluralName;
                    UnitsTypeUomobj.unitName = Obj_info.unitName;

                    List<UnitsOfMeasureIngredient> Ingredientlst = new GenericeDAO<UnitsOfMeasureIngredient>().GetWhere(" UnitsOfMeasure_Id =" + Obj_info.Id);
                    UnitsTypeUom[] UnitsTypeUomarr = new UnitsTypeUom[Ingredientlst.Count + 1];
                    UnitsTypeUomarr[0] = UnitsTypeUomobj;
                    for (int x = 0; x < Ingredientlst.Count; x++)
                    {
                        Obj_ingredient = Ingredientlst[x];
                        UnitsTypeUom UnitsTypeUom_Item = new UnitsTypeUom();
                        UnitsTypeUom_Item.conversionRate = string.IsNullOrEmpty(Obj_ingredient.Storage_To_Ingredient_Value) ? 1 : Utility.ConvertToDouble(Obj_ingredient.Storage_To_Ingredient_Value);
                        UnitsTypeUom_Item.conversionRateSpecified = true;
                        UnitsTypeUom_Item.abbreviation = Obj_ingredient.unitName;
                        UnitsTypeUom_Item.pluralAbbreviation = Obj_ingredient.unitName;
                        UnitsTypeUom_Item.pluralName = Obj_ingredient.unitName;
                        UnitsTypeUom_Item.unitName = Obj_ingredient.unitName;
                        UnitsTypeUomarr[x + 1] = UnitsTypeUom_Item;
                    }
                    UnitsTypeUomLst.uom = UnitsTypeUomarr;
                    unitsTypeobj.uomList = UnitsTypeUomLst;
                    unitsTypeobj.name = Obj_info.Name;
                    UnitsTypeArr[i] = unitsTypeobj;
                }
                catch (Exception ex)
                {
                    ColLst.RemoveAt(i);
                    LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
                }
            }
            return UnitsTypeArr;
        }
        private void Updatedlst(List<Foodics.NetSuite.Shared.Model.UnitsOfMeasure> InvoiceLst, WriteResponseList responseLst)
        {
            try
            {
                string strids = "";
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
                            iDs.Add(new Tuple<int, string>(Convert.ToInt32(rf.internalId.ToString()), InvoiceLst[counter].Id.ToString()));

                            strids += InvoiceLst[counter].Id.ToString();
                            if (counter + 1 < InvoiceLst.Count)
                                strids += ",";
                        }
                        catch (Exception ex)
                        {
                            LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
                        }
                    }
                }
                // NetsuiteDAO objDAO = new NetsuiteDAO();
                //updates local db
                // LogDAO.Integration_Exception(LogIntegrationType.Info, TaskRunType.POST, "InvoiceTask UpdateDB", "Updating " + iDs.Count().ToString() + " from " + InvoiceLst.Count().ToString());

                //objDAO.UpdateNetsuiteIDs(iDs, "Invoice");

                GenericeDAO<Foodics.NetSuite.Shared.Model.UnitsOfMeasure> objDAO = new GenericeDAO<Foodics.NetSuite.Shared.Model.UnitsOfMeasure>();
                objDAO.MainUpdateNetsuiteIDs(iDs, "UnitsOfMeasure");
                new CustomDAO().UpdateUnitsOfMeasureIngredient(strids);
            }

            catch (Exception ex)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
            }
        }
        private void Updatedlstingredients(List<Foodics.NetSuite.Shared.Model.UnitsOfMeasure> InvoiceLst, WriteResponseList responseLst)
        {
            try
            {
                string ids = "";
                for (int counter = 0; counter < InvoiceLst.Count; counter++)
                {
                    if (responseLst.writeResponse[counter].status.isSuccess)
                    {
                        try
                        {
                            ids += InvoiceLst[counter].Id.ToString();
                            if (counter + 1 < InvoiceLst.Count)
                                ids += ",";
                        }
                        catch (Exception ex)
                        {
                            LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(ids))
                    new CustomDAO().UpdateUnitsOfMeasureIngredient(ids);

            }
            catch (Exception ex)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
            }
        }
        private UnitsType[] GenerateNetSuiteUpdatelst(List<UnitsOfMeasure> ColLst)
        {
            #region variables
            com.netsuite.webservices.UnitsType[] UnitsTypeArr = new com.netsuite.webservices.UnitsType[ColLst.Count];
            com.netsuite.webservices.UnitsType unitsTypeobj;
            UnitsTypeUomList UnitsTypeUomLst;
            Foodics.NetSuite.Shared.Model.UnitsOfMeasure Obj_info;
            Foodics.NetSuite.Shared.Model.UnitsOfMeasureIngredient Obj_ingredient;
            #endregion
            for (int i = 0; i < ColLst.Count; i++)
            {
                try
                {
                    Obj_info = ColLst[i];
                    List<UnitsOfMeasureIngredient> Ingredientlst = new GenericeDAO<UnitsOfMeasureIngredient>().GetWhere(" isnull(NetSuiteID,0)<=0 and UnitsOfMeasure_Id =" + Obj_info.Id);
                    UnitsTypeUom[] UnitsTypeUomarr = new UnitsTypeUom[Ingredientlst.Count];
                    UnitsTypeUomLst = new UnitsTypeUomList();
                    unitsTypeobj = new UnitsType();
                    for (int x = 0; x < Ingredientlst.Count; x++)
                    {
                        Obj_ingredient = Ingredientlst[x];
                        UnitsTypeUom UnitsTypeUom_Item = new UnitsTypeUom();
                        UnitsTypeUom_Item.conversionRate = string.IsNullOrEmpty(Obj_ingredient.Storage_To_Ingredient_Value) ? 1 : Utility.ConvertToDouble(Obj_ingredient.Storage_To_Ingredient_Value);
                        UnitsTypeUom_Item.conversionRateSpecified = true;

                        UnitsTypeUom_Item.abbreviation = Obj_ingredient.unitName;
                        UnitsTypeUom_Item.pluralAbbreviation = Obj_ingredient.unitName;
                        UnitsTypeUom_Item.pluralName = Obj_ingredient.unitName;
                        UnitsTypeUom_Item.unitName = Obj_ingredient.unitName;
                        UnitsTypeUomarr[x] = UnitsTypeUom_Item;
                    }
                    UnitsTypeUomLst.uom = UnitsTypeUomarr;
                    unitsTypeobj.uomList = UnitsTypeUomLst;
                    unitsTypeobj.name = Obj_info.Name;
                    unitsTypeobj.internalId = Obj_info.unit_id.ToString();
                    UnitsTypeUomLst.replaceAll = false;
                    UnitsTypeArr[i] = unitsTypeobj;
                }
                catch (Exception ex)
                {
                    ColLst.RemoveAt(i);
                    LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
                }

            }
            return UnitsTypeArr;
        }
    }
}
