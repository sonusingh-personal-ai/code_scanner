using Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataAccessLayer
{
    public class dlSettingInfo : DataAccessBridge
    {
        private enSettingInfo _enSettingInfo = null;
        public dlSettingInfo(enSettingInfo enSettingInfo_)
            : base("SettingInfo")
        {
            this._enSettingInfo = enSettingInfo_;
        }

        public int Create()
        {
            return base.Create(_enSettingInfo.SettingId, _enSettingInfo.Parameters, _enSettingInfo.Status, DateTime.Now);
        }

        public void Read()
        {
            using (IDataReader idr = base.Read(_enSettingInfo.Id, _enSettingInfo.SettingId))
            {
                if (idr.Read())
                {
                    ConstructObject(idr, _enSettingInfo);
                    //_enSetting.IsGetSuccess = true;
                }
            }
        }

        public List<enSettingInfo> ReadAll()
        {
            var listOfSettingInfo = new List<enSettingInfo>();
            using (IDataReader idr = base.Read(_enSettingInfo.Id, _enSettingInfo.SettingId))
            {
                while (idr.Read())
                {
                    var objENSettingInfo = new enSettingInfo();
                    ConstructObject(idr, objENSettingInfo);
                    listOfSettingInfo.Add(objENSettingInfo);
                    //_enSetting.IsGetSuccess = true;
                }
            }
            return listOfSettingInfo;
        }

        public int Delete()
        {
            return base.Delete(_enSettingInfo.Id, _enSettingInfo.SettingId);
        }

        private void ConstructObject(IDataReader dr_, enSettingInfo enSettingInfo_)
        {
            enSettingInfo_.Id = Convert.ToInt32(dr_["Id"]);
            enSettingInfo_.SettingId = Convert.ToInt32(dr_["SettingId"]);
            enSettingInfo_.Parameters = dr_["Parameters"].ToString();
            enSettingInfo_.Status = Convert.ToBoolean(dr_["Status"]);
            enSettingInfo_.CreatedOn = Convert.ToDateTime(dr_["CreatedOn"]);
        }
    }
}
