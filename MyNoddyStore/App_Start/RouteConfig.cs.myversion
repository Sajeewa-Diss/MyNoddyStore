using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MyNoddyStore
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(null, "", new
            //{                                    /* handles URL "/" and lists the first page of all products from all categories */
            //    controller = "Product",
            //    action = "List",
            //    category = (string)null,
            //    page = 1
            //});

            routes.MapRoute( 
                name: null,
                url: "Page{page}",
                defaults: new { Controller = "Product", action = "List" });

            routes.MapRoute(null, "{category}",
                new { controller = "Product", action = "List", page = 1 });          /* handles URL "/Football" and lists the first page of a specific products category */

            routes.MapRoute(null, "{category}/Page{page}",                           /* handles URL "/Football/Page2" and lists the specific page of a specific products category */
                new { controller = "Product", action = "List" },
                new { page = @"\d+" });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
