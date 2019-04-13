using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyNoddyStore.HtmlHelpers;

namespace MyNoddyStore.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            int remainingMilliseconds = Session.GetRemainingTimeOrSetDefault(); //todo make getter only not setter. yes!!
            ViewBag.remainingTime = remainingMilliseconds;                        //use a view model object??
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