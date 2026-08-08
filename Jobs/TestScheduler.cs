using System;
using Hangfire;

namespace Jobs
{
    public class TestScheduler
    {
        // Called by Hangfire. Logs a tick and re-schedules itself after 5 seconds.
        public void Tick()
        {
            try
            {
                Log.Info("Hangfire TestScheduler Tick at " + DateTime.Now.ToString("o"));
            }
            catch { }

            // NOTE: removed self-rescheduling to avoid an infinite short-interval loop.
            // If you need periodic test ticks, schedule them via RecurringJob or enable via config.
        }
    }
}
