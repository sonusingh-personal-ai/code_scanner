using Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataAccessLayer
{
    public class dlLine : DataAccessBridge
    {
        private enLine _enLine = null;

        public dlLine()
            : base("Line")
        {
        }

        public dlLine(enLine enLine_)
            : base("Line")
        {
            this._enLine = enLine_;
        }

        public int Create()
        {
            return base.Create(_enLine.Name, DateTime.Now);
        }

        public void Read()
        {
            using (IDataReader idr = base.Read(_enLine?.Id))
            {
                if (idr.Read())
                {
                    ConstructObject(idr, _enLine);
                }
            }
        }

        public List<enLine> ReadAll()
        {
            var listOfLines = new List<enLine>();
            using (IDataReader idr = base.Read(_enLine?.Id))
            {
                while (idr.Read())
                {
                    var objENLine = new enLine();
                    ConstructObject(idr, objENLine);
                    listOfLines.Add(objENLine);
                }
            }
            return listOfLines;
        }

        public int Update()
        {
            return base.Update(_enLine.Id, _enLine.Name, DateTime.Now);
        }

        public int Delete()
        {
            return base.Delete(_enLine?.Id);
        }

        private void ConstructObject(IDataReader dr_, enLine enLine_)
        {
            enLine_.Id = Convert.ToInt32(dr_["Id"]);
            enLine_.Name = dr_["Name"].ToString();
            enLine_.CreatedOn = Convert.ToDateTime(dr_["CreatedOn"]);
            enLine_.ModifiedOn = DBNull.Value == dr_["ModifiedOn"] ? (DateTime?)null : Convert.ToDateTime(dr_["ModifiedOn"]);
        }
    }
}
