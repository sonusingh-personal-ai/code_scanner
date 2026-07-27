using Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataAccessLayer
{
    public class dlModel : DataAccessBridge
    {
        private enModel _enModel = null;
        public dlModel(enModel enModel_)
            : base("Model")
        {
            this._enModel = enModel_;
        }

        public int Create()
        {
            return base.Create(_enModel.Name, _enModel.Value, _enModel.Position, DateTime.Now);
        }

        public void Read()
        {
            using (IDataReader idr = base.Read(_enModel.Id, _enModel.Value))
            {
                if (idr.Read())
                {
                    ConstructObject(idr, _enModel);
                }
            }
        }

        public List<enModel> ReadAll()
        {
            var listOfModels = new List<enModel>();
            using (IDataReader idr = base.Read(_enModel.Id, _enModel.Value))
            {
                while (idr.Read())
                {
                    var objENModel = new enModel();
                    ConstructObject(idr, objENModel);
                    listOfModels.Add(objENModel);
                }
            }
            return listOfModels;
        }

        public int Update()
        {
            return base.Update(_enModel.Id, _enModel.Name, _enModel.Value, _enModel.Position, _enModel.CreatedOn, DateTime.Now);
        }

        private void ConstructObject(IDataReader dr_, enModel enModel_)
        {
            enModel_.Id = Convert.ToInt32(dr_["Id"]);
            enModel_.Name = dr_["Name"].ToString();
            enModel_.Value = dr_["Value"].ToString();
            enModel_.Position = Convert.ToInt32(dr_["Position"]);
            enModel_.CreatedOn = Convert.ToDateTime(dr_["CreatedOn"]);
            enModel_.ModifiedOn = DBNull.Value == dr_["ModifiedOn"] ? (DateTime?)null : Convert.ToDateTime(dr_["ModifiedOn"]);
        }
    }
}
