using Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class dlOfficeMember : DataAccessBridge
    {
        private enOfficeMember _enOfficeMember = null;
        public dlOfficeMember(enOfficeMember enOfficeMember_)
            : base("OfficeMember")
        {
            this._enOfficeMember = enOfficeMember_;
        }

        public int Create()
        {
            return base.Create(_enOfficeMember.Type, _enOfficeMember.Name, DateTime.Now);
        }

        public void Read()
        {
            using (IDataReader idr = base.Read(_enOfficeMember.ID))
            {
                if (idr.Read())
                {
                    ConstructObject(idr, _enOfficeMember);
                }
            }
        }

        public List<enOfficeMember> ReadAll()
        {
            var listOfOfficeMembers = new List<enOfficeMember>();
            using (IDataReader idr = base.Read(_enOfficeMember.ID))
            {
                while (idr.Read())
                {
                    var objENOfficeMember = new enOfficeMember();
                    ConstructObject(idr, objENOfficeMember);
                    listOfOfficeMembers.Add(objENOfficeMember);
                }
            }
            return listOfOfficeMembers;
        }

        public int Update()
        {
            return base.Update(_enOfficeMember.ID, _enOfficeMember.Type, _enOfficeMember.Name, _enOfficeMember.InsertedOn, DateTime.Now);
        }

        public int Delete()
        {
            return base.Delete(_enOfficeMember.ID);
        }

        private void ConstructObject(IDataReader dr_, enOfficeMember enOfficeMember_)
        {
            enOfficeMember_.ID = Convert.ToInt32(dr_["ID"]);
            enOfficeMember_.Type = Convert.ToInt32(dr_["Type"]);
            enOfficeMember_.Name = dr_["Name"].ToString();
            enOfficeMember_.InsertedOn = Convert.ToDateTime(dr_["InsertedOn"]);
            enOfficeMember_.ModifiedOn = DBNull.Value == dr_["ModifiedOn"] ? (DateTime?)null : Convert.ToDateTime(dr_["ModifiedOn"]);
        }
    }
}
