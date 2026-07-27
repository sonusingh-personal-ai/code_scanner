using System.Web.Mvc;
using Newtonsoft.Json;
using Entity;

namespace CodeScanner.Controllers
{
    public class BaseController : Controller
    {
        // GET: Base
        public enCookieDetail CookieDetail
        {
            get
            {
                if (Request.Cookies["LoginCookie"] == null)
                    return null;

                return (enCookieDetail)JsonConvert.DeserializeObject(Request.Cookies["LoginCookie"].Value, typeof(enCookieDetail));
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Request.Cookies["LoginCookie"] == null
                && (filterContext.Controller.ControllerContext.RouteData.Values["action"].ToString().ToLower() != "login"
                || filterContext.Controller.ControllerContext.RouteData.Values["controller"].ToString().ToLower() != "account"))
            {
                filterContext.Result = RedirectToAction("index", "home");
                return;
            }
        }

    }
}