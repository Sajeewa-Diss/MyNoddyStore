using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyNoddyStore.AdHocHelpers;

namespace MyNoddyStore.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            int remainingMilliseconds = Session.GetRemainingTimeOrSetDefault(); //todo remove this viewbag setter??!!
            ViewBag.remainingTime = remainingMilliseconds;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}