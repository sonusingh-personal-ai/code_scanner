using Entity;
using System.Collections.Generic;
using DAL = DataAccessLayer.dlDailyReport;

namespace BusinessLogicLayer
{
    public class blDailyReport
    {
        private enDailyReport _enDailyReport = null;
        private DAL _objDAL = null;

        public blDailyReport(enDailyReport enDailyReport_)
        {
            this._enDailyReport = enDailyReport_;
        }

        public int Create()
        {
            return GetDALReference().Create();
        }

        public void Read()
        {
            GetDALReference().Read();
        }

        public List<enDailyReport> ReadAll()
        {
            return GetDALReference().ReadAll();
        }

        private DAL GetDALReference()
        {
            if (_objDAL == null)
            {
                _objDAL = new DAL(this._enDailyReport);
            }
            return _objDAL;
        }
    }
}
