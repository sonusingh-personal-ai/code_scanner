using Entity;
using System.Collections.Generic;
using DAL = DataAccessLayer.dlPrinterModel;

namespace BusinessLogicLayer
{
    public class blPrinterModel
    {
        private enPrinterModel _enPrinterModel = null;
        private DAL _objDAL = null;

        public blPrinterModel(enPrinterModel enPrinterModel_)
        {
            this._enPrinterModel = enPrinterModel_;
        }

        public int Create()
        {
            return GetDALReference().Create();
        }

        public void Read()
        {
            GetDALReference().Read();
        }

        public List<enPrinterModel> ReadAll()
        {
            return GetDALReference().ReadAll();
        }

        public int Update()
        {
            return GetDALReference().Update();
        }

        public int Delete()
        {
            return GetDALReference().Delete();
        }

        private DAL GetDALReference()
        {
            if (_objDAL == null)
            {
                _objDAL = new DAL(this._enPrinterModel);
            }
            return _objDAL;
        }
    }
}