using System;
using System.Data;
using System.Data.SqlClient;

namespace DataAccessLayer
{
    public abstract class DataAccessBridge
    {
        private string _bridgeForTableName = String.Empty;
        private SqlHelper _sqlHelper = null;

        public DataAccessBridge(string bridgeForTableName_)
        {
            this._bridgeForTableName = bridgeForTableName_;
            _sqlHelper = new SqlHelper();
        }

        protected int Create(params object[] parameterValues_)
        {
            SqlCommand sqlCmd = _sqlHelper.GetCommandAndFillParameterValues("usp_" + _bridgeForTableName + "_CREATE", parameterValues_);
            using (sqlCmd)
            {
                using (sqlCmd.Connection)
                {
                    sqlCmd.Connection.Open();
                    return Convert.ToInt32(sqlCmd.ExecuteScalar());
                }
            }
        }

        protected int Create(SqlTransaction objSqlTransaction_, params object[] parameterValues_)
        {
            return Convert.ToInt32(_sqlHelper.GetCommandAndFillParameterValues(objSqlTransaction_, "usp_" + _bridgeForTableName + "_CREATE", parameterValues_)
                .ExecuteScalar());
        }

        protected IDataReader Read(params object[] parameterValues_)
        {
            SqlCommand sqlCmd = _sqlHelper.GetCommandAndFillParameterValues("usp_" + _bridgeForTableName + "_READ", parameterValues_);
            sqlCmd.Connection.Open();
            return sqlCmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        protected int Update(params object[] parameterValues_)
        {
            SqlCommand sqlCmd = _sqlHelper.GetCommandAndFillParameterValues("usp_" + _bridgeForTableName + "_UPDATE", parameterValues_);
            using (sqlCmd)
            {
                using (sqlCmd.Connection)
                {
                    sqlCmd.Connection.Open();
                    return sqlCmd.ExecuteNonQuery();
                }
            }
        }

        protected int Update(SqlTransaction objSqlTransaction_, params object[] parameterValues_)
        {
            return _sqlHelper.GetCommandAndFillParameterValues(objSqlTransaction_,"usp_" + _bridgeForTableName + "_UPDATE", parameterValues_).ExecuteNonQuery();
        }

        protected int Delete(params object[] parameterValues_)
        {
            SqlCommand sqlCmd = _sqlHelper.GetCommandAndFillParameterValues("usp_" + _bridgeForTableName + "_DELETE", parameterValues_);
            using (sqlCmd)
            {
                using (sqlCmd.Connection)
                {
                    sqlCmd.Connection.Open();
                    return sqlCmd.ExecuteNonQuery();
                }
            }
        }

        protected int Delete(SqlTransaction objSqlTransaction_, params object[] parameterValues_)
        {
            return _sqlHelper.GetCommandAndFillParameterValues(objSqlTransaction_, "usp_" + _bridgeForTableName + "_DELETE", parameterValues_).ExecuteNonQuery();
        }
    }
}
