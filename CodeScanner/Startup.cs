using Owin;
using Microsoft.Owin;
using System;
using Hangfire;
using Hangfire.SqlServer;
[assembly: OwinStartup(typeof(CodeScanner.Startup))]

namespace CodeScanner
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            //app.MapSignalR();
            // 1. Configure Hangfire Global Storage
            //GlobalConfiguration.Configuration
            //    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            //    .UseSimpleAssemblyNameTypeSerializer()
            //    .UseRecommendedSerializerSettings()
            //    .UseSqlServerStorage("HangfireConnection", new SqlServerStorageOptions
            //    {
            //        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            //        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            //        QueuePollInterval = TimeSpan.Zero,
            //        UseRecommendedIsolationLevel = true,
            //        DisableGlobalLocks = true
            //    });

            //// 2. Start the Hangfire Processing Server
            //app.UseHangfireServer();

            //// 3. Mount the Hangfire Dashboard (Default URL: /hangfire)
            //app.UseHangfireDashboard("/hangfire");
        }
    }
}