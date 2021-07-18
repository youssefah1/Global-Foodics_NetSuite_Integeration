using Dapper;
using Foodics.NetSuite.Shared.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foodics.NetSuite.Shared.DAO
{
    public class CustomDAO : BaseDAO
    {
        public List<Invoice> SelectInvoice(int Order_Status)
        {
            StringBuilder query = new StringBuilder();
            //ProductStatus =3 closed product
            query.Append(@" SELECT     distinct  Invoice.*
                         FROM            Invoice
						 where Invoice.Id in (select invoice_id from InvoiceItem where isnull(InvoiceItem.Item_Id,0) >0 ");

            if (Order_Status == 4)
                query.Append(@" and  ProductStatus =3");
            if (Order_Status == 5)
                query.Append(" and Invoice.[Original_Foodics_Id] in (select invparnt.[Foodics_Id] from [dbo].[Invoice] invparnt where invparnt.[Foodics_Id]= Invoice.[Original_Foodics_Id]and isnull(invparnt.Netsuite_Id,0)>0  ) ");
            query.Append(" )and Order_Status=" + Order_Status + " and (Invoice.Netsuite_Id IS NULL or Invoice.Netsuite_Id =0) ");
            if (Order_Status == 4)
                query.Append(@"  and Invoice.Id  not in 
						 (select invoice_id from InvoiceItem where isnull(InvoiceItem.Item_Id,0) = 0 and  ProductStatus =3)");

           // query.Append(" and id = 356 ");
            //query.Append(" and Invoice.[Date] = '2021-07-08' ");
            //query.Append(" and Invoice.[Location_Id] = 207 ");

            using (db)
            {
                return db.Query<Invoice>(query.ToString()).ToList();
            }
        }
        public List<PaymentMethodEntity> SelectCustomerPayment(int Order_Status)
        {
            string query = @" SELECT   distinct  PaymentMethodEntity.*
                          FROM            Invoice INNER JOIN
                         PaymentMethodEntity ON Invoice.Id = PaymentMethodEntity.Entity_Id

						 where isnull(Invoice.Netsuite_Id,0)>0 and Invoice.Order_Status=" + Order_Status +
                     " and (PaymentMethodEntity.Netsuite_Id IS NULL or PaymentMethodEntity.Netsuite_Id =0) ";

            //query +=" and Invoice.[Date] < '2021-03-15' ";
            using (db)
            {
                return db.Query<PaymentMethodEntity>(query).ToList();
            }
        }
        public List<PaymentMethodEntity> SelectUpdateCustomerRefund()
        {
            string query = @" SELECT  * from BB_Customer_Refund";

            //query +=" and Invoice.[Date] < '2021-03-15' ";
            using (db)
            {
                return db.Query<PaymentMethodEntity>(query).ToList();
            }
        }
        public void GenerateAdjustmentBuild()
        {
            StringBuilder query = new StringBuilder();
            query.Append(@" SELECT InvoiceItem.Id, Invoice.Location_Id,Invoice.Subsidiary_Id,InvoiceItem.Item_Id, InvoiceItem.Quantity
                            INTO #Adjustment
                            FROM InvoiceItem INNER JOIN invoice ON InvoiceItem.Invoice_Id = Invoice.Id
                            AND ISNULL(InvoiceItem.AdjustementBuilt, 0) = 0
                            AND ISNULL(Invoice.Netsuite_Id, 0) > 0
                            AND Invoice.Order_Status=5
                            AND InvoiceItem.ProductStatus=6
                            AND InvoiceItem.Item_Type='AssemblyItem'
                           -- and Invoice.[Date]>='2021-03-15'

                            INSERT INTO [dbo].[AdjustmentBuild]
                                       (

                                        [Location_Id],
										[Subsidiary_Id]
                                       ,[Item_Id]
                                       ,[Quantity]
                                       ,[InActive]
                                       ,[UpdateDate]
                                       ,[CreateDate])
                            SELECT ass.Location_Id,ass.Subsidiary_Id, ass.Item_Id, SUM(ass.Quantity), 0, GETDATE(), GETDATE()
                            FROM #Adjustment ass
                            GROUP BY ass.Subsidiary_Id,ass.Location_Id, ass.Item_Id

                            UPDATE InvoiceItem
                                SET AdjustementBuilt = 1
                            FROM InvoiceItem
                            INNER JOIN #Adjustment ass
                            ON InvoiceItem.Id = ass.Id

                            DROP TABLE #Adjustment ");
            using (db)
            {
                db.ExecuteScalar(query.ToString());
            }

        }

        public List<AdjustmentBuild> SelectAdjustmentLocation()
        {
            string query = @" SELECT     distinct top(200)    *
                         FROM            AdjustmentBuild
						 where  (Netsuite_Id IS NULL or Netsuite_Id =0) ";

            using (db)
            {
                return db.Query<AdjustmentBuild>(query).ToList();
            }
        }
        public void UpdateProductCompnent()
        {
            StringBuilder query = new StringBuilder();
            query.Append(@" update comp
                        set comp.ComponentId = item.Netsuite_Id
                        FROM [dbo].[ItemCompnent] AS comp
                        INNER JOIN Item ON comp.ComponentFoodics_Id = item.Foodics_Id
                        WHERE ISNULL(comp.ComponentId, 0) = 0");

            query.Append(@" update comp
                        set comp.ItemId = item.Netsuite_Id
                        FROM [dbo].[ItemCompnent] AS comp
                        INNER JOIN Item ON comp.ItemFoodics_Id = item.Foodics_Id
                        WHERE ISNULL(comp.ItemId, 0) = 0");
            using (db)
            {
                db.ExecuteScalar(query.ToString());
            }

        }
        //public void UpdateInvoiceItem()
        //{
        //    //select* FROM[dbo].[InvoiceItem] AS invitem                        WHERE ISNULL(invitem.Item_Id, 0) = 0
        //    StringBuilder query = new StringBuilder();
        //    query.Append(@" update invitem
        //                set invitem.Item_Id = item.Netsuite_Id,
        //                    invitem.Item_Type = item.Item_Type_Name
        //                FROM [dbo].[InvoiceItem] AS invitem
        //                INNER JOIN Item ON invitem.FoodicsItem_Id = item.Foodics_Id
        //                WHERE ISNULL(invitem.Item_Id, 0) = 0 and ISNULL(item.Netsuite_Id, 0) > 0") ;


        //    using (db)
        //    {
        //        db.ExecuteScalar(query.ToString());
        //    }

        //}
        public void SetItemClass()
        {
            StringBuilder query = new StringBuilder();
            query.Append(@" update item
                        set[Category_Id] = [Categories].Netsuite_Id
                        from item
                        inner join [dbo].[Categories]
                        on [dbo].[Categories].[Foodics_Id] = item.[FoodicsCategory_Id]
                        where [FoodicsCategory_Id] is not NULL and isnull([Category_Id],0)= 0
                        and isnull([Categories].Netsuite_Id,0) > 0 ");
            using (db)
            {
                db.ExecuteScalar(query.ToString());
            }

        }

        public void InvoiceRelatedUpdate()
        {
            StringBuilder query = new StringBuilder();
            query.Append(" update [dbo].[InvoiceItem] set[Invoice_Id] = (select[Id] from[dbo].[Invoice] where [InvoiceItem].[Foodics_Id] = [Invoice].[Foodics_Id]) where[Invoice_Id] is null or  [Invoice_Id] = 0 ");
            query.Append(" update [dbo].[PaymentMethodEntity] set[Entity_Id] = (select[Id] from[dbo].[Invoice] where [PaymentMethodEntity].[Foodics_Id] = [Invoice].[Foodics_Id]) where[Entity_Id] is null or  [Entity_Id] = 0 ");
            query.Append(@" update invitem
                        set invitem.Item_Id = item.Netsuite_Id,
                            invitem.Item_Type = item.Item_Type_Name
                        FROM [dbo].[InvoiceItem] AS invitem
                        INNER JOIN Item ON invitem.FoodicsItem_Id = item.Foodics_Id
                        WHERE ISNULL(invitem.Item_Id, 0) = 0 and ISNULL(item.Netsuite_Id, 0) > 0" );
            using (db)
            {
                db.ExecuteScalar(query.ToString());
            }

        }

        public void GenerateAssemblyBuild()
        {
            StringBuilder query = new StringBuilder();
            query.Append(@" SELECT InvoiceItem.Id, Invoice.Location_Id,Invoice.Subsidiary_Id,InvoiceItem.Item_Id, InvoiceItem.Quantity
                            INTO #Assembly
                            FROM InvoiceItem INNER JOIN invoice ON InvoiceItem.Invoice_Id = Invoice.Id
                            AND ISNULL(InvoiceItem.AssemblyBuilt, 0) = 0
                            AND Invoice.Order_Status=4
                            AND InvoiceItem.ProductStatus=3
                            AND InvoiceItem.Item_Type='AssemblyItem'
                            and InvoiceItem.Quantity >0
                           -- And [Date]>='2021-03-15'

                            INSERT INTO[dbo].[AssemblyBuild]
                                       (

                                        [Location_Id],
										[Subsidiary_Id]
                                       ,[Item_Id]
                                       ,[Quantity]
                                       ,[InActive]
                                       ,[UpdateDate]
                                       ,[CreateDate])
                            SELECT ass.Location_Id,ass.Subsidiary_Id, ass.Item_Id, SUM(ass.Quantity), 0, GETDATE(), GETDATE()
                            FROM #Assembly ass
                            GROUP BY ass.Subsidiary_Id,ass.Location_Id, ass.Item_Id

                            UPDATE InvoiceItem
                                SET AssemblyBuilt = 1
                            FROM InvoiceItem
                            INNER JOIN #Assembly ass
                            ON InvoiceItem.Id = ass.Id

                            DROP TABLE #Assembly ");
            using (db)
            {
                db.ExecuteScalar(query.ToString());
            }

        }



        //public void UpdateCompnentitemID()
        //{
        //    StringBuilder query = new StringBuilder();
        //    query.Append(@"update [dbo].[ItemCompnent] 
        //                    set [ItemId]= (select [Netsuite_Id] from [dbo].[Item] where [Item].[Foodics_Id] = [ItemCompnent].[ItemFoodics_Id])
        //                    where ISNULL([ItemId],0) <=0 ; ");
        //    //delete duplocated after add component from foodics
        //    //query.Append(@" delete FROM ItemCompnent WHERE ID NOT IN(SELECT MAX(ID)         FROM ItemCompnent         GROUP BY[ItemId],[ComponentId])");
        //    using (db)
        //    {
        //        db.ExecuteScalar(query.ToString());
        //    }

        //}

        public void UpdateUnitsOfMeasureIngredient(string ids)
        {
            StringBuilder query = new StringBuilder();
            query.Append(@"update [dbo].[UnitsOfMeasureIngredient] 
                            set NetSuiteID =1 where UnitsOfMeasure_Id in (" + ids + ")");
            using (db)
            {
                db.ExecuteScalar(query.ToString());
            }

        }

        public void DeleteFoodicsItemsComponent(string IDs)
        {
            StringBuilder query = new StringBuilder();
            query.Append(" delete from [dbo].[ItemCompnent] where   [ItemFoodics_Id] in (" + IDs + ") ");
            using (db)
            {
                db.ExecuteScalar(query.ToString());
            }

        }
        public void Check_PaymentCash_Exist()
        {
            StringBuilder query = new StringBuilder();
            query.Append("IF NOT EXISTS(SELECT id FROM PaymentMethod WHERE Netsuite_Id = 1)");
            query.Append(" begin update PaymentMethod set Netsuite_Id = 1 where Name_En = 'cash' end ");
            using (db)
            {
                db.ExecuteScalar(query.ToString());
            }

        }

        public int Check_Create_unitName(string unitName)
        {
            StringBuilder query = new StringBuilder();
            query.Append("IF NOT EXISTS(SELECT id FROM UnitsOfMeasure WHERE conversionRate=1 and unitName = '" + unitName + "')");
            query.Append(" begin insert into UnitsOfMeasure (Name,abbreviation,pluralAbbreviation,unitName,pluralName,unit_id,baseUnit,conversionRate,InActive,UpdateDate,CreateDate)");
            query.Append(" values('" + unitName + "','" + unitName + "','" + unitName + "','" + unitName + "','" + unitName + "',1,'True',1,'False',GETDATE(),GETDATE()); select max(id) from  UnitsOfMeasure; end; ");
            query.Append(" else begin  SELECT id FROM UnitsOfMeasure WHERE conversionRate=1 and unitName = '" + unitName + "' end; ");
            using (db)
            {
                return Utility.ConvertToInt(db.ExecuteScalar(query.ToString()).ToString());
            }

        }

        public void Check_Create_unitName_ingredient(UnitsOfMeasureIngredient obj)
        {
            StringBuilder query = new StringBuilder();
            query.Append("IF NOT EXISTS(SELECT id FROM UnitsOfMeasureIngredient WHERE Storage_To_Ingredient_Value='" + obj.Storage_To_Ingredient_Value + "' and unitName = '" + obj.unitName + "' and UnitsOfMeasure_Id=" + obj.UnitsOfMeasure_Id + " )");
            query.Append(" begin insert into UnitsOfMeasureIngredient (UnitsOfMeasure_Id,unitName,Storage_To_Ingredient_Value,InActive,UpdateDate,CreateDate)");
            query.Append(" values('" + obj.UnitsOfMeasure_Id + "','" + obj.unitName + "','" + obj.Storage_To_Ingredient_Value + "','False',GETDATE(),GETDATE()) end ");
            using (db)
            {
                db.ExecuteScalar(query.ToString());
            }

        }




    }
}
