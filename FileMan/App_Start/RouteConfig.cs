using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Raf.FileMan
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Login",
                url: "Account/{action}",
                defaults: new { controller = "Account" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Redirect", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Share",
                url: "Share/{id}",
                defaults: new { controller = "MasterFiles", action = "Details" }
            );


        }
    }
}
