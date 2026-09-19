using Hangfire;
using Microsoft.Owin;
using Owin;
using Jobs;
using System;
using System.IO;
using System.Web.Hosting;
using NLog;
using NLog.Config;
using NLog.Common;

[assembly: OwinStartup(typeof(Startup))]
public class Startup
{
    public void Configuration(IAppBuilder app)
    {
        // log startup so we can confirm OWIN startup executed
        try { Log.Info("OWIN Startup running"); } catch { }
        // configure Hangfire to use the same DB as application (ensure package Hangfire.SqlServer installed)
        var conn = Utility.ApplicationSettings.DefaultConnectionString;
        try
        {
            Log.Info("Startup: Configuring Hangfire storage using connection string.");
            GlobalConfiguration.Configuration.UseSqlServerStorage(conn);
            Log.Info("Startup: GlobalConfiguration.Configuration.UseSqlServerStorage() completed.");
        }
        catch (Exception ex)
        {
            try { Log.Error("Startup: UseSqlServerStorage threw: " + ex.ToString()); } catch { }
        }

        try
        {
            if (JobStorage.Current == null)
            {
                Log.Warn("Startup: JobStorage.Current is null after UseSqlServerStorage.");
            }
            else
            {
                Log.Info("Startup: JobStorage.Current is set: " + JobStorage.Current.GetType().FullName);
            }
        }
        catch (Exception ex)
        {
            try { Log.Error("Startup: Failed to inspect JobStorage.Current: " + ex.ToString()); } catch { }
        }

        // Ensure project Log folder exists and is writable
        try
        {
            string logFolder = HostingEnvironment.MapPath("~/Log");
            if (string.IsNullOrEmpty(logFolder))
            {
                logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
            }
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
                Log.Info("Created log directory: " + logFolder);
            }
            // Test write permission
            string testFile = Path.Combine(logFolder, "._write_test");
            File.WriteAllText(testFile, DateTime.Now.ToString("o"));
            File.Delete(testFile);
            Log.Info("Log directory writable: " + logFolder);
        }
        catch (Exception ex)
        {
            try { Log.Error("Log directory check failed: " + ex.Message); } catch { }
        }

        // Force load NLog configuration and write a test entry to help diagnose logging
        try
        {
            try
            {
                InternalLogger.LogLevel = LogLevel.Info;
                InternalLogger.LogFile = "c:\\temp\\nlog-internal.log";
                InternalLogger.IncludeTimestamp = true;
            }
            catch { }

            string nlogPath = HostingEnvironment.MapPath("~/bin/NLog.config");
            if (string.IsNullOrEmpty(nlogPath) || !File.Exists(nlogPath))
            {
                // try the appdomain bin folder
                nlogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "NLog.config");
            }

            // as a fallback also check the app base directory (project root) for ease of local dev
            if (!File.Exists(nlogPath))
            {
                var alt = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NLog.config");
                if (File.Exists(alt)) nlogPath = alt;
            }

            if (File.Exists(nlogPath))
            {
                var cfg = new XmlLoggingConfiguration(nlogPath);
                LogManager.Configuration = cfg;
                LogManager.ReconfigExistingLoggers();
                Log.Info("NLog configuration loaded from: " + nlogPath);
                Log.Info("NLog test message at " + DateTime.Now.ToString("o"));
            }
            else
            {
                InternalLogger.Warn("NLog config not found at: " + nlogPath);
            }
        }
        catch (Exception ex)
        {
            try { InternalLogger.Error(ex.ToString()); } catch { }
        }

        try
        {
            app.UseHangfireServer();
            Log.Info("Startup: Hangfire server middleware started.");
        }
        catch (Exception ex)
        {
            try { Log.Error("Startup: app.UseHangfireServer failed: " + ex.ToString()); } catch { }
        }

        try
        {
            app.UseHangfireDashboard();
            Log.Info("Startup: Hangfire dashboard middleware started.");
        }
        catch (Exception ex)
        {
            try { Log.Error("Startup: app.UseHangfireDashboard failed: " + ex.ToString()); } catch { }
        }

        // register recurring job for exports. Run daily at 11:00 PM.
        //RecurringJob.AddOrUpdate<ExportJob>("export-job", job => job.ProcessExports(), Cron.Daily(23, 0));
        RecurringJob.AddOrUpdate<ExportJob>("export-job", job => job.ProcessExports(), Cron.Daily(15, 0));

        // Start a short-interval test scheduler (first run after 5 seconds)
        try
        {
            BackgroundJob.Schedule<Jobs.TestScheduler>(x => x.Tick(), TimeSpan.FromSeconds(5));
            Log.Info("Scheduled TestScheduler initial tick in 5 seconds");
        }
        catch (Exception ex)
        {
            try { Log.Error("Failed to schedule TestScheduler: " + ex); } catch { }
        }
    }
}
