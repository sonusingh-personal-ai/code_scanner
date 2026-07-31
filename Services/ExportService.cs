using BusinessLogicLayer;
using Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace Services
{
    public class ExportService
    {
        public void ExportAll()
        {
            var objENResponse = new enResponse();
            var objBLResponse = new blResponse(objENResponse);

            List<enResponse> listOfResponses = new List<enResponse>();
            try
            {
                // fetch all data (consider paging or limits if dataset is huge)
                listOfResponses = objBLResponse.ReadAllAndAggregate(typeof(enResponseSummary));
            }
            catch (Exception ex)
            {
                Utility.Log.Error("ExportService: Failed to read responses: " + ex);
                return;
            }

            if (listOfResponses == null || listOfResponses.Count == 0)
                return;
        }

        private string EscapeCsv(string s)
        {
            if (s == null) return string.Empty;
            if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
            {
                return '"' + s.Replace("\"", "\"\"") + '"';
            }
            return s;
        }
    }
}
