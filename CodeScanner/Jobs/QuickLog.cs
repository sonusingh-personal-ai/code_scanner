using System;

namespace CodeScanner.Jobs
{
    public static class QuickLog
    {
        public static void Write(string message)
        {
            try
            {
                Log.Info(message);
            }
            catch { }
        }
    }
}
