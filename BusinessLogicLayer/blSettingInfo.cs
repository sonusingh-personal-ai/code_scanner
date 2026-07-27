using Entity;
using System.Collections.Generic;
using DAL = DataAccessLayer.dlSettingInfo;

namespace BusinessLogicLayer
{
    public class blSettingInfo
    {
        private enSettingInfo _enSettingInfo = null;
        private DAL _objDAL = null;

        public blSettingInfo(enSettingInfo enSettingInfo_)
        {
            this._enSettingInfo = enSettingInfo_;
        }

        public int Create()
        {
            return GetDALReference().Create();
        }

        public void Read()
        {
            GetDALReference().Read();
        }

        public List<enSettingInfo> ReadAll()
        {
            return GetDALReference().ReadAll();
        }

        public int Delete()
        {
            return GetDALReference().Delete();
        }

        private DAL GetDALReference()
        {
            if (_objDAL == null)
            {
                _objDAL = new DAL(this._enSettingInfo);
            }
            return _objDAL;
        }
    }
}
