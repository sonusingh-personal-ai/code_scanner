using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Hangfire;
using Hangfire.SqlServer;

namespace CodeScanner
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public const string ScheduledExportJobId = "scheduled-export";
        public const int ScheduledExportHour = 22;   // 24-hour, local time (see ScheduledExportTimeZone)
        public const int ScheduledExportMinute = 00;

        public static TimeZoneInfo ScheduledExportTimeZone
        {
            get
            {
                try
                {
                    // Windows server id. If this app ever runs on Linux/.NET Core, swap for "Asia/Kolkata".
                    return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                }
                catch
                {
                    // Fall back to UTC rather than crash scheduling if the id isn't found on this OS.
                    Log.Error("Could not resolve 'India Standard Time' zone; falling back to UTC for scheduled export.");
                    return TimeZoneInfo.Utc;
                }
            }
        }

        public static void RegisterScheduledExportJob()
        {
            RecurringJob.AddOrUpdate(
                ScheduledExportJobId,
                () => MvcApplication.RunExportService(),
                Cron.Daily(ScheduledExportHour, ScheduledExportMinute),
                new RecurringJobOptions { TimeZone = ScheduledExportTimeZone });
        }

        protected void Application_Start()
        {
            //RouteTable.Routes.MapHubs();
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;
            ViewEngines.Engines.Remove(new WebFormViewEngine());
            HttpContext.Current.Cache.Add("CachedSQLParameters", new Dictionary<string, SqlParameterCollection>(), null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(24, 0, 0), System.Web.Caching.CacheItemPriority.Normal, null);

            // Ensure Hangfire storage and server are initialized as a fallback
            try
            {
                var conn = Utility.ApplicationSettings.DefaultConnectionString;
                GlobalConfiguration.Configuration.UseSqlServerStorage(conn);
                // Start a Hangfire server if none is running via OWIN
                try
                {
                    var options = new Hangfire.BackgroundJobServerOptions();
                    var server = new Hangfire.BackgroundJobServer(options);
                    Log.Info("Started Hangfire BackgroundJobServer from Application_Start");
                }
                catch (Exception exServer)
                {
                    Log.Error("Failed to start Hangfire BackgroundJobServer in Application_Start: " + exServer);
                }

                // schedule recurring export once per day, local (IST) time - see RegisterScheduledExportJob
                try
                {
                    RegisterScheduledExportJob();
                    Log.Info($"Scheduled recurring export job (daily at {ScheduledExportHour:D2}:{ScheduledExportMinute:D2} {ScheduledExportTimeZone.Id})");
                }
                catch (Exception exSched)
                {
                    Log.Error("Failed to schedule recurring export job: " + exSched);
                }
            }
            catch (Exception ex)
            {
                try { Log.Error("Failed to initialize Hangfire in Application_Start: " + ex); } catch { }
            }
        }

        public static void RunExportService()
        {
            try
            {
                Log.Info("Global.asax: ExportService: Starting Hangfire export job...");
                var service = new ExportService();
                service.ExportAll();
                Log.Info("Global.asax: Hangfire export job completed successfully.");
            }
            catch (Exception ex)
            {
                try { Log.Error("Global.asax: RunExportService failed: " + ex); } catch { }
            }
        }
    }
}