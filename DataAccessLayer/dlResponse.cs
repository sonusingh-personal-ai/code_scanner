using Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utility;

namespace DataAccessLayer
{
    public class dlResponse : DataAccessBridge
    {
        private enResponse _enResponse = null;
        public dlResponse(enResponse enResponse_)
            : base("Response")
        {
            this._enResponse = enResponse_;
        }

        public int Create()
        {
            return base.Create(_enResponse.Barcode, _enResponse.QcStatus, _enResponse.VisualBy, _enResponse.TestedBy, _enResponse.ProductionLine, _enResponse.ProcessEngg, _enResponse.SerialCardNo, _enResponse.Model, _enResponse.ConProgNo, _enResponse.DisProgNo, _enResponse.SystemRating, _enResponse.CurrentDate, _enResponse.CurrentTime, _enResponse.ResponseTime, DateTime.Now, _enResponse.PrinterModelId, _enResponse.Line);
        }

        public void Read(int? startRowNumber = null, int? endRowNumber = null, DateTime? startDate = null, DateTime? endDate = null, string searchStr = null, int? line = null)
        {
            using (IDataReader idr = base.Read(startRowNumber, endRowNumber, startDate, endDate, _enResponse.CurrentDate, _enResponse.Id, _enResponse.Barcode, _enResponse.QcStatus, searchStr, line))
            {
                if (idr.Read())
                {
                    ConstructObject(idr, _enResponse);
                }
            }
        }

        public List<enResponse> ReadAll(int? startRowNumber = null, int? endRowNumber = null, DateTime? startDate = null, DateTime? endDate = null, string searchStr = null, int? line = null)
        {
            var listOfResponses = new List<enResponse>();
            using (IDataReader idr = base.Read(startRowNumber, endRowNumber, startDate, endDate, _enResponse.CurrentDate, _enResponse.Id, _enResponse.Barcode, _enResponse.QcStatus, searchStr, line))
            {
                while (idr.Read())
                {
                    var objENResponse = new enResponse();
                    ConstructObject(idr, objENResponse);
                    listOfResponses.Add(objENResponse);
                }
            }
            return listOfResponses;
        }

        public int Delelte()
        {
            return base.Delete(_enResponse.Id);
        }

        // Delete using an existing SqlTransaction (batch operations)
        public int Delete(System.Data.SqlClient.SqlTransaction objSqlTransaction_)
        {
            return base.Delete(objSqlTransaction_, _enResponse.Id);
        }

        public static int ResetLine(int? lineId = null)
        {
            using (var conn = new SqlConnection(ApplicationSettings.DefaultConnectionString))
            {
                string sql = lineId.HasValue && lineId.Value > 0
                    ? "UPDATE [dbo].[Response] SET [Line] = NULL WHERE [Line] = @LineId"
                    : "UPDATE [dbo].[Response] SET [Line] = NULL";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (lineId.HasValue && lineId.Value > 0)
                    {
                        cmd.Parameters.Add("@LineId", SqlDbType.Int).Value = lineId.Value;
                    }
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        private void ConstructObject(IDataReader dr_, enResponse enResponse_)
        {
            enResponse_.Id = Convert.ToInt32(dr_["Id"]);
            enResponse_.Barcode = dr_["Barcode"].ToString();
            enResponse_.QcStatus = Convert.ToInt32(dr_["QcStatus"]);
            enResponse_.VisualBy = Convert.ToInt32(dr_["VisualBy"]);
            enResponse_.TestedBy = Convert.ToInt32(dr_["TestedBy"]);
            enResponse_.ProductionLine = Convert.ToInt32(dr_["ProductionLine"]);
            enResponse_.ProcessEngg = Convert.ToInt32(dr_["ProcessEngg"]);
            enResponse_.PrinterModelId = dr_["PrinterModelId"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr_["PrinterModelId"]);
            enResponse_.Line = dr_["Line"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr_["Line"]);
            enResponse_.SerialCardNo = dr_["SerialCardNo"].ToString();
            enResponse_.Model = dr_["Model"].ToString();
            enResponse_.ConProgNo = dr_["ConProgNo"].ToString();
            enResponse_.DisProgNo = dr_["DisProgNo"].ToString();
            enResponse_.SystemRating = dr_["SystemRating"].ToString();
            enResponse_.CurrentDate = dr_["CurrentDate"].ToString();
            enResponse_.CurrentTime = dr_["CurrentTime"].ToString();
            enResponse_.ResponseTime = Convert.ToDateTime(dr_["ResponseTime"]);
            enResponse_.CreatedOn = Convert.ToDateTime(dr_["CreatedOn"]);
            enResponse_.RecordsCount = Convert.ToInt32(dr_["TotalRecords"]);
            enResponse_.RowNumber = Convert.ToInt32(dr_["RowNumber"]);
        }

    }
}
