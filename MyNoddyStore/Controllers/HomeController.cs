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
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Index()
        {
            int remainingMilliseconds = Session.GetRemainingTime();
            ViewBag.remainingTime = remainingMilliseconds;

            if (remainingMilliseconds == int.MinValue){ //set inGame flag. a numeric flag is easiest to verify in Js.
                ViewBag.inGame = 0;
            } else {
                ViewBag.inGame = 1;
            }
            return View();
        }
    }
}