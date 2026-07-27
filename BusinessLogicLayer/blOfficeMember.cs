using Entity;
using System.Collections.Generic;
using DAL = DataAccessLayer.dlOfficeMember;

namespace BusinessLogicLayer
{
    public class blOfficeMember
    {
        private enOfficeMember _enOfficeMember = null;
        private DAL _objDAL = null;

        public blOfficeMember(enOfficeMember enOfficeMember_)
        {
            this._enOfficeMember = enOfficeMember_;
        }

        public int Create()
        {
            return GetDALReference().Create();
        }

        public void Read()
        {
            GetDALReference().Read();
        }

        public List<enOfficeMember> ReadAll()
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
                _objDAL = new DAL(this._enOfficeMember);
            }
            return _objDAL;
        }
    }
}
