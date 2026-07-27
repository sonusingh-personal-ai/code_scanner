using Entity;
using System.Collections.Generic;
using DAL = DataAccessLayer.dlModel;

namespace BusinessLogicLayer
{
    public class blModel
    {
        private enModel _enModel = null;
        private DAL _objDAL = null;

        public blModel(enModel enModel_)
        {
            this._enModel = enModel_;
        }

        public int Create()
        {
            return GetDALReference().Create();
        }

        public void Read()
        {
            GetDALReference().Read();
        }

        public List<enModel> ReadAll()
        {
            return GetDALReference().ReadAll();
        }


        public int Update()
        {
            return GetDALReference().Update();
        }

        private DAL GetDALReference()
        {
            if (_objDAL == null)
            {
                _objDAL = new DAL(this._enModel);
            }
            return _objDAL;
        }

    }
}
