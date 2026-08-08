using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Storage;

namespace CodeScanner.Controllers
{
    public class HangfireTestController : Controller
    {
        // GET: /HangfireTest/Trigger
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult Trigger()
        {
            try
            {
                RecurringJob.TriggerJob(MvcApplication.ScheduledExportJobId);
                Log.Info("HangfireTestController.Trigger(): Triggered scheduled-export");
                return Content("Triggered scheduled-export");
            }
            catch (Exception ex)
            {
                try { Log.Error("HangfireTestController.Trigger failed: " + ex.ToString()); } catch { }
                try
                {
                    Log.Info("HangfireTestController.Trigger: Attempting to recreate scheduled-export recurring job.");
                    RecurringJob.RemoveIfExists(MvcApplication.ScheduledExportJobId);
                    MvcApplication.RegisterScheduledExportJob();
                    Log.Info("HangfireTestController.Trigger: Recreated scheduled-export recurring job.");
                    RecurringJob.TriggerJob(MvcApplication.ScheduledExportJobId);
                    Log.Info("HangfireTestController.Trigger: Triggered recreated scheduled-export");
                    return Content("Recreated and triggered scheduled-export");
                }
                catch (Exception ex2)
                {
                    try { Log.Error("HangfireTestController.Trigger: Failed to recreate/trigger scheduled-export: " + ex2.ToString()); } catch { }
                    return Content("Failed to trigger or recreate scheduled-export: " + ex.Message + " / " + ex2.Message);
                }
            }
        }

        // GET: /HangfireTest/EnqueueTest
        public ActionResult EnqueueTest()
        {
            try
            {
                Log.Info("EnqueueTest called. Inspecting JobStorage.Current...");
                try { Log.Info("EnqueueTest: JobStorage.Current = " + (JobStorage.Current == null ? "null" : JobStorage.Current.GetType().FullName)); } catch { }
                // Ensure Hangfire JobStorage is initialized (fallback if Startup didn't run)
                var storage = JobStorage.Current;
                string conn = null;
                if (storage == null)
                {
                    try
                    {
                        conn = Utility.ApplicationSettings.DefaultConnectionString;
                        Log.Info("EnqueueTest: Attempting GlobalConfiguration.Configuration.UseSqlServerStorage fallback.");
                        GlobalConfiguration.Configuration.UseSqlServerStorage(conn);
                        storage = JobStorage.Current;
                        if (storage == null)
                        {
                            Log.Info("EnqueueTest: JobStorage.Current still null, creating new SqlServerStorage instance.");
                            storage = new SqlServerStorage(conn);
                            JobStorage.Current = storage;
                        }
                        Log.Info("EnqueueTest: Hangfire storage initialized: " + (storage == null ? "null" : storage.GetType().FullName));
                    }
                    catch (Exception ex)
                    {
                        try { Log.Error("EnqueueTest: Failed to init Hangfire storage in controller: " + ex.ToString()); } catch { }
                        return Content("Failed to init Hangfire storage in controller: " + ex.ToString());
                    }
                }

                try
                {
                    try { Log.Info("EnqueueTest: Using storage instance: " + (storage == null ? "null" : storage.GetType().FullName)); } catch { }
                    var client = new BackgroundJobClient(storage);
                    client.Enqueue(() => CodeScanner.Jobs.QuickLog.Write("Hangfire Enqueued QuickLog at " + DateTime.Now.ToString("o")));
                    return Content("Enqueued QuickLog job");
                }
                catch (Exception ex)
                {
                    try { Log.Error("EnqueueTest: Failed to enqueue after init: " + ex.ToString()); } catch { }
                    return Content("Failed to enqueue after init: " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                try { Log.Error("EnqueueTest: Unexpected error: " + ex.ToString()); } catch { }
                return Content("Failed to enqueue: " + ex.Message);
            }
        }

        // GET: /HangfireTest/WriteLog
        public ActionResult WriteLog()
        {
            try
            {
                Log.Info("HangfireTestController.WriteLog called at " + DateTime.Now.ToString("o"));
                return Content("Wrote Info log entry");
            }
            catch (Exception ex)
            {
                try { Log.Error("WriteLog failed: " + ex); } catch { }
                return Content("Failed to write log: " + ex.Message);
            }
        }

        // GET: /HangfireTest/Status
        public ActionResult Status()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Recent export files:");
            try
            {
                var folder = Server.MapPath("~/App_Data/Exports");
                if (Directory.Exists(folder))
                {
                    var files = Directory.GetFiles(folder).OrderByDescending(f => new FileInfo(f).LastWriteTime).Take(10);
                    foreach (var f in files) sb.AppendLine(Path.GetFileName(f));
                }
                else sb.AppendLine("Exports folder does not exist.");
            }
            catch (Exception ex) { sb.AppendLine("Error listing exports: " + ex.Message); }

            sb.AppendLine();
            sb.AppendLine("Recent log tail:");
            try
            {
                // prefer project Log folder (~/Log), fall back to ~/bin/Log for older config
                var logFolder = Server.MapPath("~/Log");
                if (!Directory.Exists(logFolder))
                    logFolder = Server.MapPath("~/bin/Log");

                var today = DateTime.Now.ToString("yyyyMMdd");
                var logFile = Path.Combine(logFolder, today + " Custom_Log.txt");

                // Ensure log folder exists
                try
                {
                    if (!Directory.Exists(logFolder)) Directory.CreateDirectory(logFolder);
                }
                catch (Exception ex)
                {
                    sb.AppendLine("Failed to ensure log folder: " + ex.Message);
                }

                // If log file does not exist, create an empty file with a header so callers see logs exist
                try
                {
                    if (!System.IO.File.Exists(logFile))
                    {
                        System.IO.File.WriteAllText(logFile, "Log created at " + DateTime.Now.ToString("o") + System.Environment.NewLine);
                        sb.AppendLine("Log file created: " + logFile);
                    }

                    var lines = System.IO.File.ReadLines(logFile).Reverse().Take(50).Reverse();
                    foreach (var line in lines) sb.AppendLine(line);
                }
                catch (Exception ex) { sb.AppendLine("Error reading/creating log: " + ex.Message); }
            }
            catch (Exception ex) { sb.AppendLine("Error reading log: " + ex.Message); }

            return Content(sb.ToString(), "text/plain");
        }
    }
}