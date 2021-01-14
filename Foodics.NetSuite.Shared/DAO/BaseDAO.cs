using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading;



namespace Foodics.NetSuite.Shared.DAO
{
    public class BaseDAO
    {
        protected IDbConnection db;
        public static string Connection = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        public BaseDAO()
        {
            db = new SqlConnection(Connection);
        }
        
        public string Lang { get { return Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName; } }
    }
}
