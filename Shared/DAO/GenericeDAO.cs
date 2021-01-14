using Dapper;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;


namespace Shared
{
    public class GenericeDAO<T>
    {
        protected IDbConnection db;
        public GenericeDAO()
        {
            db = new SqlConnection(ConfigurationManager.ConnectionStrings["ConString"].ConnectionString);
        }
        public List<T> GetAll()
        {
            using (db)
            {
                string query = "Select " + Utility.GetColumns<T>() + " From [" + typeof(T).Name + "]";
                return db.Query<T>(query).ToList();
            }
        }
        public T GetById(object id)
        {
            using (db)
            {
                string query = "Select * From " + typeof(T).Name + " WHERE Id=" + id;
                return db.Query<T>(query).FirstOrDefault();
            }
        }
        public T GetByNetsuiteId(string id)
        {
            using (db)
            {
                string query = "Select * From " + typeof(T).Name + " WHERE Id=" + id;
                return db.Query<T>(query).FirstOrDefault();
            }
        }
        public List<T> GetWhereColumnEqual(string columnName, int value)
        {
            var column = typeof(T).GetProperties().Where(x => x.Name.ToLower().Equals(columnName.ToLower())).FirstOrDefault();
            if (column == null)
            {
                throw new System.Exception("ColumnNotExist");
            }

            using (db)
            {
                string query = "Select * From [" + typeof(T).Name + "] WHERE [" + columnName + "]=" + value;
                return db.Query<T>(query).ToList();
            }
        }
        public List<T> GetWhereColumnEqual(string columnName, string value)
        {
            bool exist = typeof(T).GetProperties().Select(x => x.Name.ToLower().Equals(columnName.ToLower())).FirstOrDefault();
            if (!exist)
            {
                //string not_exist = 
                throw new System.Exception("ColumnNotExist");
            }


            using (db)
            {
                string query = "Select * From [" + typeof(T).Name + "] WHERE [" + columnName + "]='" + value + "'";
                return db.Query<T>(query).ToList();
            }
        }
        public List<T> GetWhereColumnOperator(string columnName, string operatorSymbol, object value)
        {
            using (db)
            {
                string query = "Select * From [" + typeof(T).Name + "]";

                if (typeof(T).GetProperty(columnName).PropertyType == typeof(Int32))
                    query += " WHERE [" + columnName + "]" + operatorSymbol + value;
                else if (typeof(T).GetProperty(columnName).PropertyType == typeof(String))
                {
                    if (value.ToString().Equals(DB.Constant.NULL))
                        query += " WHERE [" + columnName + "]" + operatorSymbol + value;
                    else
                        query += " WHERE [" + columnName + "]" + operatorSymbol + "N'" + value + "'";
                }
                return db.Query<T>(query).ToList();
            }
        }
        public List<T> GetWhereColumnIn(string columnName, string values)
        {
            var column = typeof(T).GetProperties().Where(x => x.Name.ToLower().Equals(columnName.ToLower())).FirstOrDefault();
            if (column == null)
                throw new System.Exception("ColumnNotExist");

            using (db)
            {
                string query = "Select * From [" + typeof(T).Name + "] WHERE [" + columnName + "] IN (" + values + ")";
                return db.Query<T>(query).ToList();
            }
        }
        public List<T> GetWhere(string condition)
        {
            using (db)
            {
                string query = "Select * From [" + typeof(T).Name + "] WHERE " + condition;
                return db.Query<T>(query).ToList();
            }
        }
        public void FoodicsIntegration(List<T> list, string extraFields = "")
        {

            string tableName = typeof(T).Name;
            StringBuilder query = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {

                try
                {
                    T obj = list[i];

                    string Foodics_ID = (string)obj.GetType().GetProperty("id").GetValue(obj);
                    var proList = obj.GetType().GetProperties().Where(p => p.CanWrite);

                    query.Append("IF EXISTS(SELECT id FROM [" + tableName + "] WHERE id = '" + Foodics_ID + "') ");
                    query.Append("  BEGIN UPDATE [" + tableName + "] SET ");
                    foreach (var pro in proList)
                        query.Append(" " + pro.Name + "=" + Utility.GetColumnValue(obj, pro.Name) + ", ");
                    query.Append("       [UpdateDate] = GETDATE()   WHERE [ID] = '" + Foodics_ID + "' ");
                    query.Append("  END ELSE BEGIN INSERT INTO [" + tableName + "] ( ");
                    foreach (var pro in proList)
                        query.Append(" " + pro.Name + ", ");
                    query.Append("  [UpdateDate],[CreateDate],[Source_Type]) VALUES ( ");
                    foreach (var pro in proList)
                        query.Append(" " + Utility.GetColumnValue(obj, pro.Name) + ", ");
                    query.Append(" GETDATE(),GETDATE()," + (int)Source_Type.Foodics + ") END ");
                }
                catch (Exception ex)
                {
                }
            }
            using (db)
            {
                db.Execute(query.ToString());
            }
        }

        public void NetSuiteIntegration(List<T> list, string extraFields = "")
        {

            string tableName = typeof(T).Name;
            StringBuilder query = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {

                try
                {
                    T obj = list[i];

                    string Foodics_ID = (string)obj.GetType().GetProperty("Foodics_Id").GetValue(obj);
                    var proList = obj.GetType().GetProperties().Where(p => p.CanWrite);

                    query.Append("IF EXISTS(SELECT Foodics_Id FROM [" + tableName + "] WHERE Foodics_Id = '" + Foodics_ID + "') ");
                    query.Append("  BEGIN UPDATE [" + tableName + "] SET ");
                    foreach (var pro in proList)
                    {
                       // if (!string.IsNullOrEmpty(Utility.GetColumnValue(obj, pro.Name).ToString()))
                            query.Append(" " + pro.Name + "=" + Utility.GetColumnValue(obj, pro.Name) + ", ");
                    }
                    query.Append("       [UpdateDate] = GETDATE()   WHERE [Foodics_Id] = '" + Foodics_ID + "' ");
                    query.Append("  END ELSE BEGIN INSERT INTO [" + tableName + "] ( ");
                    foreach (var pro in proList)
                        query.Append(" " + pro.Name + ", ");
                    query.Append("  [UpdateDate],[CreateDate]) VALUES ( ");
                    foreach (var pro in proList)
                        query.Append(" " + Utility.GetColumnValue(obj, pro.Name) + ", ");
                    query.Append(" GETDATE(),GETDATE()) END ");
                }
                catch (Exception ex)
                {
                }
            }
            using (db)
            {
                db.Execute(query.ToString());
            }
        }
        public void MultiFieldIntegration(List<T> newList, string extraFields = "")
        {
            List<T> list = new List<T>();
            int totalRecords = 1000;
            if (newList.Count < 1000)
            {
                totalRecords = newList.Count;
            }
            while (newList.Any())
            {

                list = newList.Take(totalRecords).ToList();
                newList = newList.Skip(totalRecords).ToList();

                string tableName = typeof(T).Name;
                StringBuilder query = new StringBuilder();
                int Insert_Code = 0;
                string salesOrder = "";
                for (int i = 0; i < list.Count; i++)
                {
                    T obj = list[i];
                    string addFields = "";
                    string[] fields = extraFields.Split(' ');
                    if (tableName == "SalesOrderItems")
                    {
                        string objSalesOrderId = obj.GetType().GetProperty("SalesOrder_Netsuite_Id").GetValue(obj).ToString();
                        if (objSalesOrderId != salesOrder)
                        {
                            Insert_Code = 0;
                            salesOrder = objSalesOrderId;
                        }
                        obj.GetType().GetProperty("Insert_Code").SetValue(obj, Insert_Code);
                    }

                    Insert_Code++;
                    foreach (var field in fields)
                    {
                        addFields += " AND " + field + " = " + Utility.GetColumnValue(obj, obj.GetType().GetProperty(field).Name);
                        //addFields += " AND " + field + " = " + obj.GetType().GetProperty(field).GetValue(obj).ToString();
                    }
                    var pro_list = obj.GetType().GetProperties().Where(x => x.CanWrite);
                    query.AppendLine("IF EXISTS(SELECT Id FROM [" + tableName + "] WHERE Id > 0 " + addFields + ") ");
                    query.AppendLine("  BEGIN UPDATE [" + tableName + "] SET ");
                    foreach (var pro in pro_list)
                        query.Append(" " + pro.Name + "=" + Utility.GetColumnValue(obj, pro.Name) + ", ");
                    query.Append("        [UpdateDate] = GETDATE() WHERE Id > 0 " + addFields + " ");
                    query.AppendLine("  END ELSE BEGIN INSERT INTO [" + tableName + "] ( ");
                    foreach (var pro in pro_list)
                        query.Append(" " + pro.Name + ", ");
                    query.Append(" [UpdateDate],[CreateDate]) VALUES ( ");
                    foreach (var pro in pro_list)
                        query.Append(" " + Utility.GetColumnValue(obj, pro.Name) + ", ");
                    query.AppendLine(" GETDATE(),GETDATE()) END ");
                }

                try
                {
                    string x = query.ToString();
                    int rslt = db.Execute(query.ToString());
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
        }
        public void UpdateList(List<T> list)
        {
            string tableName = typeof(T).Name;
            StringBuilder query = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                T obj = list[i];
                int netSuite_Id = (int)obj.GetType().GetProperty("Netsuite_Id").GetValue(obj);
                var pro_list = obj.GetType().GetProperties().Where(x => x.CanWrite);
                query.Append("UPDATE [" + tableName + "] SET ");
                foreach (var pro in pro_list)
                    query.Append(" " + pro.Name + "=" + Utility.GetColumnValue(obj, pro.Name) + ", ");
                query.AppendLine("[UpdateDate] = GETDATE() WHERE [Netsuite_Id] = " + netSuite_Id + " ");
            }
            using (db)
            {
                db.Execute(query.ToString());
            }
        }
        public void UpdateNetSuiteID(string tableName, string netsuite_id, string request_id)
        {
            StringBuilder query = new StringBuilder();
            query.Append("UPDATE [" + tableName + "] SET [Netsuite_Id] = " + netsuite_id);
            query.Append(", [UpdateDate] = GETDATE() WHERE [request_id] = " + request_id + " ");

            using (db)
            {
                db.Execute(query.ToString());
            }
        }
        public void TruncateInsert(List<T> list, string datafield = "")
        {
            if (list.Count > 0)
            {
                string tableName = typeof(T).Name;
                StringBuilder query = new StringBuilder();
                query.Append("truncate table  [" + tableName + "] ;");
                query.Append("  BEGIN INSERT INTO [" + tableName + "] ( ");
                T obj = list[0];
                var pro_list = obj.GetType().GetProperties().Where(x => x.CanWrite);
                foreach (var pro in pro_list)
                    query.Append(" " + pro.Name + ", ");
                query.Append(" [UpdateDate],[CreateDate]) VALUES ( ");
                for (int i = 0; i < list.Count; i++)
                {
                    obj = list[i];
                    pro_list = obj.GetType().GetProperties().Where(x => x.CanWrite);
                    foreach (var pro in pro_list)
                        query.Append(" " + Utility.GetColumnValue(obj, pro.Name) + ", ");
                    if (i == (list.Count - 1))
                        query.Append(" GETDATE(),GETDATE()) ");
                    else
                        query.Append(" GETDATE(),GETDATE()) , (");
                }
                query.AppendLine(" END ");
                using (db)
                {
                    db.Execute(query.ToString());
                }
            }
        }
        public void InsertRecord(List<T> list)
        {
            if (list.Count > 0)
            {
                string tableName = typeof(T).Name;
                StringBuilder query = new StringBuilder();
                query.Append("  BEGIN INSERT INTO [" + tableName + "] ( ");
                T obj = list[0];
                var pro_list = obj.GetType().GetProperties().Where(x => x.CanWrite);
                foreach (var pro in pro_list)
                    query.Append(" " + pro.Name + ", ");
                query.Append(" [UpdateDate],[CreateDate]) VALUES ( ");
                for (int i = 0; i < list.Count; i++)
                {
                    obj = list[i];
                    foreach (var pro in pro_list)
                        query.Append(" " + Utility.GetColumnValue(obj, pro.Name) + ", ");
                    if (i == (list.Count - 1))
                        query.Append(" GETDATE(),GETDATE()) ");
                    else
                        query.Append(" GETDATE(),GETDATE()) , (");
                }
                query.Append(" END ");
                using (db)
                {
                    db.Execute(query.ToString());
                }
            }
        }
        #region sales order
        public void UpdateNetsuiteIDs(List<Tuple<int, int>> iDs, string tableName, string PK = "", bool workOrder = false, bool itemFulfillment = false)
        {
            StringBuilder query = new StringBuilder();

            if (workOrder)
            {
                for (int i = 0; i < iDs.Count; i++)
                {
                    query.Append(@"UPDATE [" + tableName + @"] SET WorkOrder_Netsuite_Id=" + iDs[i].Item1 +
                        @"  Where WorkOrder_Netsuite_Id IS NULL AND Id=" + iDs[i].Item2 + " ");
                }
            }
            else if (itemFulfillment)
            {
                for (int i = 0; i < iDs.Count; i++)
                {
                    query.Append(@"UPDATE [" + tableName + @"] SET itemFulfillment_Netsuite_Id=" + iDs[i].Item1 +
                        @"  Where itemFulfillment_Netsuite_Id IS NULL AND Id=" + iDs[i].Item2 + " ");
                }
            }
            else if (PK != "")
            {
                for (int i = 0; i < iDs.Count; i++)
                {
                    query.Append(@"UPDATE [" + tableName + "] SET Netsuite_Id=" + iDs[i].Item1 + "  Where Netsuite_Id IS NULL AND " + PK + @" = " + iDs[i].Item2 + " ");
                }
            }
            else
            {
                for (int i = 0; i < iDs.Count; i++)
                {
                    query.Append(@"UPDATE [" + tableName + "] SET Netsuite_Id=" + iDs[i].Item1 + "  Where Netsuite_Id IS NULL AND Id=" + iDs[i].Item2 + " ");
                    query.Append(@"UPDATE [SalesOrderItems] SET SalesOrder_Netsuite_Id = " + iDs[i].Item1 + "  Where SalesOrder_Netsuite_Id IS NULL AND SalesOrder_Id = " + iDs[i].Item2 + " ");
                }
            }
            using (db)
            {
                db.Execute(query.ToString());
            }
        }


        #endregion
        public static IEnumerable<List<T>> splitList<T>(List<T> locations, int nSize = 1000)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }

        public object GetLatestModifiedDate()
        {
            string tableName = typeof(T).Name;
            StringBuilder query = new StringBuilder();
            query.Append("SELECT MAX(UpdateDate) as TheDate  FROM " + tableName);
            object LastDate = new object();
            using (db)
            {
                LastDate = db.ExecuteScalar(query.ToString());
            }

            return LastDate;
        }
        public DataTable ExecAndReturnDT(string sql)
        {
            try
            {
                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConString"].ConnectionString))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = con;
                        command.CommandText = sql;

                        SqlDataReader dr = command.ExecuteReader();
                        dt.Load(dr);
                    }
                    con.Close();
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception(sql + " " + ex.Message);
            }
        }
        public DataTable RequestDetails(string request_id)
        {
            string sql = @" SELECT TOP(1) tbl_CallRequest.*, Admin_Employees.Netsuite_Id AS Technician, 
                                tbl_zones.fld_EnglishName AS ZoneName, tbl_Cities.fld_EnglishName AS CityName, 
	                            ProcessFieldTypes_Values.field_value_text_en ContractType
                            FROM tbl_CallRequest INNER JOIN  Admin_Employees ON 
	                            tbl_CallRequest.fld_Technician = Admin_Employees.person_id 
	                            LEFT JOIN tbl_zones ON tbl_CallRequest.fld_Zone = tbl_zones.fld_id 
	                            LEFT JOIN tbl_Cities ON tbl_CallRequest.fld_City = tbl_Cities.fld_id 
	                            LEFT JOIN ProcessFieldTypes_Values ON tbl_CallRequest.fld_ContractType = ProcessFieldTypes_Values.field_value_id 
                            WHERE tbl_CallRequest.request_id = " + request_id;
            return ExecAndReturnDT(sql);

        }
    }
}
