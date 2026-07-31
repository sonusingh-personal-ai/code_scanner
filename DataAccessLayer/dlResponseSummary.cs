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

        // Batch fetch latest summary per ResponseId for given list of response IDs
        public Dictionary<int, enResponseSummary> ReadLatestForResponseIds(List<int> responseIds_)
        {
            var result = new Dictionary<int, enResponseSummary>();
            if (responseIds_ == null || responseIds_.Count == 0)
                return result;

            // Build comma-separated ids for IN clause
            var ids = string.Join(",", responseIds_);

            // Query to get latest (by Id) summary per ResponseId
            var sql = $@"
                SELECT rs.Id, rs.ResponseId, rs.Parameters, rs.Dispaly, rs.Actual, rs.Status, rs.IsFinal
                FROM (
                    SELECT *, ROW_NUMBER() OVER(PARTITION BY ResponseId ORDER BY Id DESC) rn
                    FROM ResponseSummary
                    WHERE ResponseId IN ({ids})
                ) rs
                WHERE rs.rn = 1";

            using (var conn = new SqlConnection(ApplicationSettings.DefaultConnectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
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
