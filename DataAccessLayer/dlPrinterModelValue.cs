using Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataAccessLayer
{
    public class dlPrinterModelValue : DataAccessBridge
    {
        private enPrinterModelValue _enPrinterModelValue = null;
        public dlPrinterModelValue(enPrinterModelValue enPrinterModelValue_)
            : base("PrinterModelValue")
        {
            this._enPrinterModelValue = enPrinterModelValue_;
        }

        public int Create()
        {
            return base.Create(_enPrinterModelValue.ModelId, _enPrinterModelValue.Name, _enPrinterModelValue.Value, DateTime.Now);
        }

        public void Read()
        {
            using (IDataReader idr = base.Read(_enPrinterModelValue.Id))
            {
                if (idr.Read())
                {
                    ConstructObject(idr, _enPrinterModelValue);
                }
            }
        }

        public List<enPrinterModelValue> ReadAll()
        {
            var listOfPrinterModelValues = new List<enPrinterModelValue>();
            using (IDataReader idr = base.Read(_enPrinterModelValue.Id))
            {
                while (idr.Read())
                {
                    var objENPrinterModelValue = new enPrinterModelValue();
                    ConstructObject(idr, objENPrinterModelValue);
                    listOfPrinterModelValues.Add(objENPrinterModelValue);
                }
            }
            return listOfPrinterModelValues;
        }

        public int Update()
        {
            return base.Update(_enPrinterModelValue.Id, _enPrinterModelValue.ModelId, _enPrinterModelValue.Name, _enPrinterModelValue.Value, _enPrinterModelValue.CreatedOn, DateTime.Now);
        }

        public int Delete()
        {
            return base.Delete(_enPrinterModelValue.Id);
        }

        private void ConstructObject(IDataReader dr_, enPrinterModelValue enPrinterModelValue_)
        {
            enPrinterModelValue_.Id = Convert.ToInt32(dr_["Id"]);
            enPrinterModelValue_.ModelId = Convert.ToInt32(dr_["ModelId"]);
            enPrinterModelValue_.Name = dr_["Name"].ToString();
            enPrinterModelValue_.Value = dr_["Value"].ToString();
            enPrinterModelValue_.CreatedOn = Convert.ToDateTime(dr_["CreatedOn"]);
            enPrinterModelValue_.ModifiedOn = DBNull.Value == dr_["ModifiedOn"] ? (DateTime?)null : Convert.ToDateTime(dr_["ModifiedOn"]);
        }
    }
}