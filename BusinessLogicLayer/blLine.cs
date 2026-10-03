using Entity;
using System;
using System.Collections.Generic;
using DAL = DataAccessLayer.dlLine;

namespace BusinessLogicLayer
{
    public class blLine
    {
        private enLine _enLine = null;
        private DAL _objDAL = null;

        public blLine()
        {
        }

        public blLine(enLine enLine_)
        {
            this._enLine = enLine_;
        }

        public int Create()
        {
            return GetDALReference().Create();
        }

        public void Read()
        {
            GetDALReference().Read();
        }

        public List<enLine> ReadAll()
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

        public int UpdateSequence(int id, int sequence)
        {
            var en = new enLine { Id = id };
            var dal = new DAL(en);
            dal.Read();
            if (string.IsNullOrEmpty(en.Name)) return 0;

            en.Sequence = sequence;
            en.ModifiedOn = DateTime.Now;
            return dal.Update();
        }

        private DAL GetDALReference()
        {
            if (_objDAL == null)
            {
                _objDAL = new DAL(this._enLine);
            }
            return _objDAL;
        }
    }
}
