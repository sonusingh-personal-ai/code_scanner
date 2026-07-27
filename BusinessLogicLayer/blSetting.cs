using Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using DAL = DataAccessLayer.dlSetting;

namespace BusinessLogicLayer
{
    public class blSetting
    {
        private enSetting _enSetting = null;
        private DAL _objDAL = null;

        public blSetting(enSetting enSetting_)
        {
            this._enSetting = enSetting_;
        }

        public int Create()
        {
            return GetDALReference().Create();
        }

        public void Read()
        {
            GetDALReference().Read();
        }

        public List<enSetting> ReadAll()
        {
            return GetDALReference().ReadAll();
        }

        public void ReadAndAggregate(params Type[] entityToAggregate_)
        {
            Read();
            Aggregate(entityToAggregate_);
        }

        public List<enSetting> ReadAllAndAggregate(params Type[] entityToAggregate_)
        {
            List<enSetting> listOfSettings = ReadAll();
            foreach (var item in listOfSettings)
            {
                var objBLDocument = new blSetting(item);
                objBLDocument.Aggregate(entityToAggregate_);
            }
            return listOfSettings;
        }

        public void Aggregate(params Type[] entityToAggregate_)
        {
            if (entityToAggregate_.FirstOrDefault(item => item == typeof(enSettingInfo)) != null)
            {
                if (_enSetting.Id > 0)
                {
                    var objBLSettingInfo = new blSettingInfo(new enSettingInfo() { SettingId = _enSetting.Id });
                    _enSetting.SettingInfo = objBLSettingInfo.ReadAll();
                }
            }
            if (entityToAggregate_.FirstOrDefault(item => item == typeof(enModel)) != null)
            {
                if (_enSetting.Id > 0)
                {
                    _enSetting.Model = new enModel() { Value = _enSetting.FileId };
                    var objBLModel = new blModel(_enSetting.Model);
                    objBLModel.Read();
                }
            }
        }

        public int Update()
        {
            return GetDALReference().Update();
        }

        //public int Delete()
        //{
        //    return GetDALReference().Delete();
        //}

        private DAL GetDALReference()
        {
            if (_objDAL == null)
            {
                _objDAL = new DAL(this._enSetting);
            }
            return _objDAL;
        }
    }
}
