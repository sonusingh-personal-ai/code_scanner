using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services;

namespace Jobs
{
    public class ExportJob
    {
        private readonly ExportService _exportService;

        public ExportJob()
        {
            _exportService = new ExportService();
        }

        // Hangfire will call this method on its schedule
        public void ProcessExports()
        {
            try
            {
                _exportService.ExportAll();
            }
            catch (Exception ex)
            {
                Utility.Log.Error("Error in scheduled export: " + ex);
            }
        }
    }
}
