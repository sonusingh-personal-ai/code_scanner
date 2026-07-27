using Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataAccessLayer
{
    public class dlDailyReport : DataAccessBridge
    {
        private enDailyReport _enDailyReport = null;
        public dlDailyReport(enDailyReport enDailyReport_)
            : base("DailyReport")
        {
            this._enDailyReport = enDailyReport_;
        }

        public int Create()
        {
            return base.Create(_enDailyReport.Date, _enDailyReport.Month, _enDailyReport.Year,_enDailyReport.FilePath, DateTime.Now);
        }

        public void Read()
        {
            using (IDataReader idr = base.Read(_enDailyReport.Id,_enDailyReport.Date,_enDailyReport.Month,_enDailyReport.Year))
            {
                if (idr.Read())
                {
                    ConstructObject(idr, _enDailyReport);
                }
            }
        }

        public List<enDailyReport> ReadAll()
        {
            var listOfDailyReports = new List<enDailyReport>();
            using (IDataReader idr = base.Read(_enDailyReport.Id, _enDailyReport.Date, _enDailyReport.Month, _enDailyReport.Year))
            {
                while (idr.Read())
                {
                    var objENDailyReport = new enDailyReport();
                    ConstructObject(idr, objENDailyReport);
                    listOfDailyReports.Add(objENDailyReport);
                }
            }
            return listOfDailyReports;
        }

        private void ConstructObject(IDataReader dr_, enDailyReport enDailyReport_)
        {
            enDailyReport_.Id = Convert.ToInt32(dr_["Id"]);
            enDailyReport_.Date = Convert.ToInt32(dr_["Date"]);
            enDailyReport_.Month = Convert.ToInt32(dr_["Month"]);
            enDailyReport_.Year = Convert.ToInt32(dr_["Year"]);
            enDailyReport_.FilePath = dr_["FilePath"].ToString();
            enDailyReport_.CreatedOn = Convert.ToDateTime(dr_["CreatedOn"]);
        }
    }
}
