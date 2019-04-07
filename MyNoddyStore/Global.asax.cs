using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MyNoddyStore.Entities;
using MyNoddyStore.Infrastructure.Binders;

namespace MyNoddyStore
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            ModelBinders.Binders.Add(typeof(Cart), new CartModelBinder());
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Session_Start(Object sender, EventArgs e)
        {
            //
            //            I am not sure whether there are any framework classes for handling this, but if you really need to loop through the sessions created on the IIS server for each request, why not store each session in your own collection object that you can access from your code? For example, in the global.asax file, you can add your own code in the Session_Start event, to save the specific session to your List. You will have to check whether the session is a new session, which can be done via Session.IsNewSession property.Everytime a new session is created, the Session_Start event in Global.asax is fired.

            //But there can be issues if you don't remove sessions from your List when they timeout or end, so how I might go about doing this is:

            //In Session_Start event, Check for this.Session.IsNewSession boolean value

            //If Session.IsNewSession is true, get current session (using this.Session, because Global.asax has the current new session in its context), and save it in a Dictionary object with the key as Session.SessionId.

            //This will create a unique key-pair collection for each Session that is created in the server.

            //In Session_End event, get the Session.SessionID property of the current Session (this.Session), which is the one which has ended.

            //Use the Session.SessionID of the finished Session value to remove the key value pair in the Dictionary containing the Sessions.

            //Once this infrastructure is in place, and the Dictionary object resides in a place which can be accessed by your application code, you can retrieve this dictionary and iterate through it to get the Sessions active in the Server at that point in time.

            string sessionID = HttpContext.Current.Session.SessionID;
            System.Diagnostics.Debug.WriteLine("sesion id is:" + sessionID);
        }

    }
}
