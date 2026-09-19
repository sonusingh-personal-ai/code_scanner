using Entity;
using System.Collections.Generic;
using DAL = DataAccessLayer.dlPrinterModelValue;

namespace BusinessLogicLayer
{
    public class blPrinterModelValue
    {
        private enPrinterModelValue _enPrinterModelValue = null;
        private DAL _objDAL = null;

        public blPrinterModelValue(enPrinterModelValue enPrinterModelValue_)
        {
            this._enPrinterModelValue = enPrinterModelValue_;
        }

        public int Create()
        {
            return GetDALReference().Create();
        }

        public void Read()
        {
            GetDALReference().Read();
        }

        public List<enPrinterModelValue> ReadAll()
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
                _objDAL = new DAL(this._enPrinterModelValue);
            }
            return _objDAL;
        }
    }
}