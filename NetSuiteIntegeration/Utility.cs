using System;

namespace NetSuiteIntegeration
{
    public class Utility
    {
        public static string GetMessageError(Exception ex)
        {
            string error = "";
            if (ex.InnerException == null)
                error = ex.Message;
            else
                return GetMessageError(ex.InnerException);
            return error;
        }
        public static string GetColumns<T>()
        {
            string columns = string.Empty;
            foreach (var pro in typeof(T).GetProperties())
            {
                if (string.IsNullOrEmpty(columns))
                    columns = pro.Name;
                else
                    columns += "," + pro.Name;
            }
            return columns;
        }
        public static object GetColumnValue(object obj, string propertyName)
        {
            object result = obj.GetType().GetProperty(propertyName).GetValue(obj);
            if (result != null)
            {
                if (obj.GetType().GetProperty(propertyName).PropertyType.Name.Equals("String"))
                  result = "N'" + Wrap(result.ToString()) + "'";
                else if (obj.GetType().GetProperty(propertyName).PropertyType.Name.Equals("DateTime"))
                {
                    DateTime temp_date = Convert.ToDateTime(result);

                    if (temp_date <= DateTime.MinValue || temp_date >= DateTime.MaxValue)
                        result = "NULL";
                    else
                        result = "'" + temp_date.ToString("yyyy/MM/dd hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture) + "'";
                }
                else if (obj.GetType().GetProperty(propertyName).PropertyType.Name.Equals("Boolean"))
                    result = Convert.ToByte(result);
                return result;
            }

            return "NULL";
            
        }
        public static DateTime ConvertToDateTime(string date)
        {
            try
            {
                return Convert.ToDateTime(date);
            }
            catch { return DateTime.MinValue; }
        }
        public static int ConvertToInt(string str)
        {
            try
            {
                return Convert.ToInt32(str);
            }
            catch { return 0; }
        }
        public static double ConvertToDouble(string str)
        {
            try
            {
                return Convert.ToDouble(str);
            }
            catch { return 0; }
        }

        #region Wrap / UnWrap
        public static string Wrap(string Str)
        {
            if (Str == "")
                return Str;
            else
            {
                try
                {
                    if (Str.IndexOf("'") != -1)
                        Str = Str.Replace("'", "`");
                }
                catch { }
                return Str;
            }
        }
        public static string UnWrap(string Str)
        {
            if (Str == "")
                return Str;
            else
            {
                try
                {
                    if (Str.IndexOf("`") != -1)
                        Str = Str.Replace("`", "'");
                }
                catch { }
                return Str;
            }
        }
        #endregion
    }
}