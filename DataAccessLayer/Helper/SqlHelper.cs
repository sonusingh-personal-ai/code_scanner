using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Web;
using Utility;

namespace DataAccessLayer
{
    public class SqlHelper
    {
        #region Variables

        private static string _connectionString = ApplicationSettings.DefaultConnectionString;
        private SqlCommand _sqlCmd = null;
        private SqlConnection _sqlCon = null;

        #endregion

        public SqlConnection GetConnection()
        {
            _sqlCon = new SqlConnection(_connectionString);
            return _sqlCon;
        }

        public SqlCommand GetCommand()
        {
            _sqlCmd = new SqlCommand();
            _sqlCmd.Connection = GetConnection();
            _sqlCmd.CommandType = CommandType.StoredProcedure;
            return _sqlCmd;
        }

        public SqlCommand GetCommand(string storedProcedure_)
        {
            _sqlCmd = new SqlCommand();
            _sqlCmd.Connection = GetConnection();
            _sqlCmd.CommandType = CommandType.StoredProcedure;
            _sqlCmd.CommandText = storedProcedure_;
            _sqlCmd.Parameters.Clear();
            //_sqlCmd.Connection.Open();
            return _sqlCmd;
        }

        public SqlCommand GetCommand(SqlTransaction objSqlTransaction_, string storedProcedure_)
        {
            _sqlCmd = new SqlCommand();
            _sqlCmd.Connection = objSqlTransaction_.Connection;
            _sqlCmd.Transaction = objSqlTransaction_;
            _sqlCmd.CommandType = CommandType.StoredProcedure;
            _sqlCmd.CommandText = storedProcedure_;
            _sqlCmd.Parameters.Clear();
            return _sqlCmd;
        }

        public SqlCommand GetCommandAndFillParameterValues(string storedProcedure_, params object[] parameterValues_)
        {
            SqlCommand sqlCmd = GetCommand(storedProcedure_);
            FillParameterValues(sqlCmd, storedProcedure_, parameterValues_);
            return sqlCmd;
        }

        public SqlCommand GetCommandAndFillParameterValues(SqlTransaction objSqlTransaction_, string storedProcedure_, params object[] parameterValues_)
        {
            SqlCommand sqlCmd = GetCommand(objSqlTransaction_, storedProcedure_);
            FillParameterValues(sqlCmd, storedProcedure_, parameterValues_);
            return sqlCmd;
        }

        public void FillParameterValues(SqlCommand objSqlCommand_, string storedProcedure_, params object[] parameterValues_)
        {
            Dictionary<string, SqlParameterCollection> cachedSQLParameters = HttpContext.Current.Cache["CachedSQLParameters"] as Dictionary<string, SqlParameterCollection>;

            if (!cachedSQLParameters.ContainsKey(storedProcedure_))
            {
                if (objSqlCommand_.Connection.State == ConnectionState.Open)
                    SqlCommandBuilder.DeriveParameters(objSqlCommand_);
                else
                {
                    objSqlCommand_.Connection.Open();
                    SqlCommandBuilder.DeriveParameters(objSqlCommand_);
                    objSqlCommand_.Connection.Close();
                }
                
                //Remove the "@Return_Value" parameter from the collection
                if (objSqlCommand_.Parameters != null && objSqlCommand_.Parameters.Count > 0)
                    objSqlCommand_.Parameters.RemoveAt(0);

                cachedSQLParameters.Add(storedProcedure_, objSqlCommand_.Parameters);
            }
            else
            {
                foreach (SqlParameter cachedParam in cachedSQLParameters[storedProcedure_])
                {
                    objSqlCommand_.Parameters.Add(cachedParam.Clone());
                }
            }

            for (int i = 0; i < objSqlCommand_.Parameters.Count; i++)
            {
                if (parameterValues_[i] == null)
                    objSqlCommand_.Parameters[i].Value = DBNull.Value;
                else
                    objSqlCommand_.Parameters[i].Value = parameterValues_[i];
            }
        }
    }
}
