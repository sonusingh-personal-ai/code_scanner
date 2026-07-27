using Entity;
using System.Collections.Generic;
using DAL = DataAccessLayer.dlResponseSummary;

namespace BusinessLogicLayer
{
    public class blResponseSummary
    {
        private enResponseSummary _enResponseSummary = null;
        private DAL _objDAL = null;

        public blResponseSummary(enResponseSummary enResponseSummary_)
        {
            this._enResponseSummary = enResponseSummary_;
        }

        public int Create()
        {
            return GetDALReference().Create();
        }

        public int Delete()
        {
            return GetDALReference().Delete();
        }

        public void Read()
        {
            GetDALReference().Read();
        }

        public List<enResponseSummary> ReadAll()
        {
            return GetDALReference().ReadAll();
        }

        private DAL GetDALReference()
        {
            if (_objDAL == null)
            {
                _objDAL = new DAL(this._enResponseSummary);
            }
            return _objDAL;
        }
    }
}
