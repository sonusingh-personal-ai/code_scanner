using Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using DAL = DataAccessLayer.dlResponse;

namespace BusinessLogicLayer
{
    public class blResponse
    {
        private enResponse _enResponse = null;
        private DAL _objDAL = null;

        public blResponse(enResponse enResponse_)
        {
            this._enResponse = enResponse_;
        }

        public int Create()
        {
            return GetDALReference().Create();
        }

        public int Delete()
        {
            return GetDALReference().Delelte();
        }

        public void Read()
        {
            GetDALReference().Read();
        }

        public List<enResponse> ReadAll(int? startRowNumber = null, int? endRowNumber = null, DateTime? startDate = null, DateTime? endDate = null,string searchStr = null)
        {
            return GetDALReference().ReadAll(startRowNumber, endRowNumber, startDate, endDate, searchStr);
        }

        public void ReadAndAggregate(params Type[] entityToAggregate_)
        {
            Read();
            Aggregate(entityToAggregate_);
        }

        public List<enResponse> ReadAllAndAggregate(int? startRowNumber = null, int? endRowNumber = null, DateTime? startDate = null, DateTime? endDate = null, string searchStr = null, params Type[] entityToAggregate_)
        {
            List<enResponse> listOfSettings = ReadAll(startRowNumber, endRowNumber, startDate, endDate, searchStr);
            foreach (var item in listOfSettings)
            {
                var objBLDocument = new blResponse(item);
                objBLDocument.Aggregate(entityToAggregate_);
            }
            return listOfSettings;
        }

        public void Aggregate(params Type[] entityToAggregate_)
        {
            if (entityToAggregate_.FirstOrDefault(item => item == typeof(enResponseSummary)) != null)
            {
                if (_enResponse.Id > 0)
                {
                    var objENResponseSummary = new enResponseSummary() { ResponseId = _enResponse.Id };
                    var objBLResponseSummary = new blResponseSummary(objENResponseSummary);
                    //_enResponse.listOfResponseSummary = objBLResponseSummary.ReadAll().FindAll(x => x.IsFinal == true);
                    _enResponse.ResponseSummary = objBLResponseSummary.ReadAll().LastOrDefault();
                }
            }
        }

        private DAL GetDALReference()
        {
            if (_objDAL == null)
            {
                _objDAL = new DAL(this._enResponse);
            }
            return _objDAL;
        }

    }
}
