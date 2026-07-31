using System.Reflection;
using System.Data.SqlClient;

namespace DataAccessLayer
{
    public static class ExtensionMethods
    {
        public static SqlParameter Clone(this SqlParameter sqlParam_)
        {
            SqlParameter sqlParam = new SqlParameter();

            PropertyInfo[] propertyInfo = typeof(SqlParameter).GetProperties();

            foreach (PropertyInfo property in propertyInfo)
            {
                if (property.CanWrite)
                    property.SetValue(sqlParam, property.GetValue(sqlParam_, null), null);
            }

            return sqlParam;
        }
    }
}
