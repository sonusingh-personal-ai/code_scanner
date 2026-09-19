using Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataAccessLayer
{
    public class dlPrinterModel : DataAccessBridge
    {
        private enPrinterModel _enPrinterModel = null;
        public dlPrinterModel(enPrinterModel enPrinterModel_)
            : base("PrinterModel")
        {
            this._enPrinterModel = enPrinterModel_;
        }

        public int Create()
        {
            return base.Create(_enPrinterModel.Name, DateTime.Now);
        }

        public void Read()
        {
            using (IDataReader idr = base.Read(_enPrinterModel.Id))
            {
                if (idr.Read())
                {
                    ConstructObject(idr, _enPrinterModel);
                }
            }
        }

        public List<enPrinterModel> ReadAll()
        {
            var listOfPrinterModels = new List<enPrinterModel>();
            using (IDataReader idr = base.Read(_enPrinterModel?.Id))
            {
                while (idr.Read())
                {
                    var objENPrinterModel = new enPrinterModel();
                    ConstructObject(idr, objENPrinterModel);
                    listOfPrinterModels.Add(objENPrinterModel);
                }
            }
            return listOfPrinterModels;
        }

        public int Update()
        {
            return base.Update(_enPrinterModel.Id, _enPrinterModel.Name, _enPrinterModel.CreatedOn, DateTime.Now);
        }

        public int Delete()
        {
            return base.Delete(_enPrinterModel.Id);
        }

        private void ConstructObject(IDataReader dr_, enPrinterModel enPrinterModel_)
        {
            enPrinterModel_.Id = Convert.ToInt32(dr_["Id"]);
            enPrinterModel_.Name = dr_["Name"].ToString();
            enPrinterModel_.CreatedOn = Convert.ToDateTime(dr_["CreatedOn"]);
            enPrinterModel_.ModifiedOn = DBNull.Value == dr_["ModifiedOn"] ? (DateTime?)null : Convert.ToDateTime(dr_["ModifiedOn"]);
        }
    }
}