using Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utility;

namespace DataAccessLayer
{
    public class dlResponseSummary : DataAccessBridge
    {
        private enResponseSummary _enResponseSummary = null;
        public dlResponseSummary(enResponseSummary enResponseSummary_)
            : base("ResponseSummary")
        {
            this._enResponseSummary = enResponseSummary_;
        }

        // Batch fetch all summaries for given response IDs (useful for exports)
        public Dictionary<int, List<enResponseSummary>> ReadAllForResponseIds(List<int> responseIds_)
        {
            var result = new Dictionary<int, List<enResponseSummary>>();
            if (responseIds_ == null || responseIds_.Count == 0)
                return result;

            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            foreach (var id in responseIds_)
            {
                table.Rows.Add(id);
            }

            var sql = @"
                SELECT Id, ResponseId, Parameters, Dispaly, Actual, Status, IsFinal
                FROM ResponseSummary
                WHERE ResponseId IN (SELECT Id FROM @Ids)
                ORDER BY ResponseId, Id ASC";

            try
            {
                using (var conn = new SqlConnection(ApplicationSettings.DefaultConnectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandTimeout = 300; // 5 minutes timeout for large datasets
                    var p = cmd.Parameters.AddWithValue("@Ids", table);
                    p.SqlDbType = SqlDbType.Structured;
                    p.TypeName = "dbo.IntList";

                    conn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var en = new enResponseSummary();
                            ConstructObject(dr, en);
                            if (!result.ContainsKey(en.ResponseId))
                                result[en.ResponseId] = new List<enResponseSummary>();
                            result[en.ResponseId].Add(en);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Message != null && ex.Message.IndexOf("IntList", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var ids = string.Join(",", responseIds_);
                    var fallbackSql = $@"
                        SELECT Id, ResponseId, Parameters, Dispaly, Actual, Status, IsFinal
                        FROM ResponseSummary
                        WHERE ResponseId IN ({ids})
                        ORDER BY ResponseId, Id ASC";

                    using (var conn = new SqlConnection(ApplicationSettings.DefaultConnectionString))
                    using (var cmd = new SqlCommand(fallbackSql, conn))
                    {
                        cmd.CommandTimeout = 300; // 5 minutes timeout for large datasets
                        conn.Open();
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var en = new enResponseSummary();
                                ConstructObject(dr, en);
                                if (!result.ContainsKey(en.ResponseId))
                                    result[en.ResponseId] = new List<enResponseSummary>();
                                result[en.ResponseId].Add(en);
                            }
                        }
                    }
                }
                else
                {
                    throw;
                }
            }

            return result;
        }

        // Batch fetch latest summary per ResponseId for given list of response IDs
        public Dictionary<int, enResponseSummary> ReadLatestForResponseIds(List<int> responseIds_)
        {
            var result = new Dictionary<int, enResponseSummary>();
            if (responseIds_ == null || responseIds_.Count == 0)
                return result;
            // Use a table-valued parameter (TVP) to avoid huge IN(...) lists which can blow up the SQL optimizer
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            foreach (var id in responseIds_)
            {
                table.Rows.Add(id);
            }

            var sql = @"
                SELECT rs.Id, rs.ResponseId, rs.Parameters, rs.Dispaly, rs.Actual, rs.Status, rs.IsFinal
                FROM (
                    SELECT *, ROW_NUMBER() OVER(PARTITION BY ResponseId ORDER BY Id DESC) rn
                    FROM ResponseSummary rs
                    WHERE rs.ResponseId IN (SELECT Id FROM @Ids)
                ) rs
                WHERE rs.rn = 1";

            // Prefer TVP call but fall back to IN(...) if the DB type is missing on the host
            try
            {
                using (var conn = new SqlConnection(ApplicationSettings.DefaultConnectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandTimeout = 300; // 5 minutes timeout for large datasets
                    var p = cmd.Parameters.AddWithValue("@Ids", table);
                    p.SqlDbType = SqlDbType.Structured;
                    // TypeName must match the user-defined table type created in the database
                    p.TypeName = "dbo.IntList";

                    conn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var en = new enResponseSummary();
                            ConstructObject(dr, en);
                            result[en.ResponseId] = en;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // If TVP type dbo.IntList is not present on the server, fall back to an IN(...) query
                if (ex.Message != null && ex.Message.IndexOf("IntList", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var ids = string.Join(",", responseIds_);
                    var fallbackSql = $@"
                        SELECT rs.Id, rs.ResponseId, rs.Parameters, rs.Dispaly, rs.Actual, rs.Status, rs.IsFinal
                        FROM (
                            SELECT *, ROW_NUMBER() OVER(PARTITION BY ResponseId ORDER BY Id DESC) rn
                            FROM ResponseSummary
                            WHERE ResponseId IN ({ids})
                        ) rs
                        WHERE rs.rn = 1";

                    using (var conn = new SqlConnection(ApplicationSettings.DefaultConnectionString))
                    using (var cmd = new SqlCommand(fallbackSql, conn))
                    {
                        cmd.CommandTimeout = 300; // 5 minutes timeout for large datasets
                        conn.Open();
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var en = new enResponseSummary();
                                ConstructObject(dr, en);
                                result[en.ResponseId] = en;
                            }
                        }
                    }
                }
                else
                {
                    throw;
                }
            }

            return result;
        }

        public int Create()
        {
            return base.Create(_enResponseSummary.ResponseId, _enResponseSummary.Parameters, _enResponseSummary.Dispaly, _enResponseSummary.Actual, _enResponseSummary.Status, _enResponseSummary.IsFinal);
        }

        public void Read()
        {
            using (IDataReader idr = base.Read(_enResponseSummary.Id, _enResponseSummary.ResponseId))
            {
                if (idr.Read())
                {
                    ConstructObject(idr, _enResponseSummary);
                }
            }
        }

        public List<enResponseSummary> ReadAll()
        {
            var listOfResponseSummaries = new List<enResponseSummary>();
            using (IDataReader idr = base.Read(_enResponseSummary.Id, _enResponseSummary.ResponseId))
            {
                while (idr.Read())
                {
                    var objENResponseSummary = new enResponseSummary();
                    ConstructObject(idr, objENResponseSummary);
                    listOfResponseSummaries.Add(objENResponseSummary);
                }
            }
            return listOfResponseSummaries;
        }

        public int Delete()
        {
            return base.Delete(_enResponseSummary.ResponseId);
        }

        // Delete using an existing SqlTransaction (for batch operations)
        public int Delete(System.Data.SqlClient.SqlTransaction objSqlTransaction_)
        {
            return base.Delete(objSqlTransaction_, _enResponseSummary.ResponseId);
        }

        private void ConstructObject(IDataReader dr_, enResponseSummary enResponseSummary_)
        {
            enResponseSummary_.Id = Convert.ToInt32(dr_["Id"]);
            enResponseSummary_.ResponseId = Convert.ToInt32(dr_["ResponseId"]);
            enResponseSummary_.Parameters = dr_["Parameters"].ToString();
            enResponseSummary_.Dispaly = dr_["Dispaly"].ToString();
            enResponseSummary_.Actual = dr_["Actual"].ToString();
            enResponseSummary_.Status = dr_["Status"].ToString();
            enResponseSummary_.IsFinal = Convert.ToBoolean(dr_["IsFinal"]);
        }

    }
}
