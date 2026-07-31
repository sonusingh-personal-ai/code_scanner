using Hangfire;
using Microsoft.Owin;
using Owin;
using Jobs;
using System;

[assembly: OwinStartup(typeof(Startup))]
public class Startup
{
    public void Configuration(IAppBuilder app)
    {
        // configure Hangfire to use the same DB as application (ensure package Hangfire.SqlServer installed)
        var conn = Utility.ApplicationSettings.DefaultConnectionString;
        GlobalConfiguration.Configuration.UseSqlServerStorage(conn);

        app.UseHangfireServer();
        app.UseHangfireDashboard();

        // register recurring job: daily at 12:00 AM
        RecurringJob.AddOrUpdate<ExportJob>("export-job", job => job.ProcessExports(), Cron.Daily());
    }
}
