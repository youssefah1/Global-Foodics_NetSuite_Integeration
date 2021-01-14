using Dapper;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;


namespace Foodics.NetSuite.Shared.DAO
{
    public class LogDAO :BaseDAO
    {
        //protected IDbConnection db;
        //public LogDAO()
        //{
        //    db = new SqlConnection(ConfigurationManager.ConnectionStrings["ConString"].ConnectionString);
        //}
      
        public static void Integration_Exception(LogIntegrationType log_Type,  string task_Name, string message)
        {


             IDbConnection db = new SqlConnection(Connection);
            int run_Type = 0;// (task_Run_Type.HasValue) ? (int)task_Run_Type : 0;
                
            string query = @"INSERT INTO LogIntegration (Date, Type, Task_Name, Task_Run_Type,  Message)
                                    VALUES (GETDATE(), " + (int)log_Type + ", N'" + task_Name + @"'
                                            , " + run_Type + ", N'" + message.Replace("'", "''") + @"');";
            db.Execute(query);
        }
    }
}
