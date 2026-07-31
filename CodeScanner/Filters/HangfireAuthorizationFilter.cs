using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeScanner.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // In OWIN ASP.NET environment, retrieve HttpContext from context
            var owinContext = new OwinContext(context.GetOwinEnvironment());

            // Example: Allow only authenticated users with Admin role
            return owinContext.Authentication.User != null &&
                   owinContext.Authentication.User.IsInRole("Admin");
        }
    }
}