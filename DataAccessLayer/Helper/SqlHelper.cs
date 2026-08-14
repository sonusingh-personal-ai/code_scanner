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
            _sqlCmd.CommandTimeout = 300; // 5 minutes timeout for long-running queries
            return _sqlCmd;
        }

        public SqlCommand GetCommand(string storedProcedure_)
        {
            _sqlCmd = new SqlCommand();
            _sqlCmd.Connection = GetConnection();
            _sqlCmd.CommandType = CommandType.StoredProcedure;
            _sqlCmd.CommandText = storedProcedure_;
            _sqlCmd.CommandTimeout = 300; // 5 minutes timeout for long-running queries
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
            _sqlCmd.CommandTimeout = 300; // 5 minutes timeout for long-running queries
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
            // use HttpRuntime.Cache as a fallback when HttpContext.Current is null (e.g. background threads)
            var cache = HttpContext.Current != null ? HttpContext.Current.Cache : System.Web.HttpRuntime.Cache;
            Dictionary<string, SqlParameterCollection> cachedSQLParameters = cache["CachedSQLParameters"] as Dictionary<string, SqlParameterCollection>;

            // ensure dictionary exists in cache
            if (cachedSQLParameters == null)
            {
                cachedSQLParameters = new Dictionary<string, SqlParameterCollection>();
                try
                {
                    cache.Insert("CachedSQLParameters", cachedSQLParameters);
                }
                catch
                {
                    // ignore cache insert failures; keep local dictionary
                }
            }

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

                // store a copy of the parameter collection for reuse
                try
                {
                    cachedSQLParameters.Add(storedProcedure_, objSqlCommand_.Parameters);
                }
                catch
                {
                    // ignore duplicate add failures in race conditions
                }
            }
            else
            {
                foreach (SqlParameter cachedParam in cachedSQLParameters[storedProcedure_])
                {
                    objSqlCommand_.Parameters.Add(cachedParam.Clone());
                }
            }

            // assign values provided by caller. Be defensive about lengths and nulls.
            int paramCount = objSqlCommand_.Parameters.Count;
            for (int i = 0; i < paramCount; i++)
            {
                if (parameterValues_ == null || i >= parameterValues_.Length || parameterValues_[i] == null)
                    objSqlCommand_.Parameters[i].Value = DBNull.Value;
                else
                    objSqlCommand_.Parameters[i].Value = parameterValues_[i];
            }
        }
    }
}
