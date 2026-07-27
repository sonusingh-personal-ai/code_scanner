using Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataAccessLayer
{
    public class dlSetting : DataAccessBridge
    {
        private enSetting _enSetting = null;
        public dlSetting(enSetting enSetting_)
            : base("Setting")
        {
            this._enSetting = enSetting_;
        }

        public int Create()
        {
            return base.Create(_enSetting.Header,_enSetting.Footer,_enSetting.Fields,_enSetting.FileId, DateTime.Now);
        }

        public void Read()
        {
            using (IDataReader idr = base.Read(_enSetting.Id,_enSetting.FileId))
            {
                if (idr.Read())
                {
                    ConstructObject(idr, _enSetting);
                    //_enSetting.IsGetSuccess = true;
                }
            }
        }

        public List<enSetting> ReadAll()
        {
            var listOfSettings = new List<enSetting>();
            using (IDataReader idr = base.Read(_enSetting.Id, _enSetting.FileId))
            {
                while (idr.Read())
                {
                    var objENSetting = new enSetting();
                    ConstructObject(idr, objENSetting);
                    listOfSettings.Add(objENSetting);
                    //_enSetting.IsGetSuccess = true;
                }
            }
            return listOfSettings;
        }

        public int Update()
        {
            return base.Update(_enSetting.Id, _enSetting.Header, _enSetting.Footer, _enSetting.Fields, _enSetting.FileId,_enSetting.CreatedOn, DateTime.Now);
        }

        private void ConstructObject(IDataReader dr_, enSetting enSetting_)
        {
            enSetting_.Id = Convert.ToInt32(dr_["Id"]);
            enSetting_.Header = dr_["Header"].ToString();
            enSetting_.Footer = dr_["Footer"].ToString();
            enSetting_.Fields = Convert.ToInt32(dr_["Fields"]);
            enSetting_.FileId = dr_["FileId"].ToString();
            enSetting_.CreatedOn = Convert.ToDateTime(dr_["CreatedOn"]);
            enSetting_.ModifiedOn = DBNull.Value == dr_["ModifiedOn"] ? (DateTime?)null : Convert.ToDateTime(dr_["ModifiedOn"]);
        }
    }
}
