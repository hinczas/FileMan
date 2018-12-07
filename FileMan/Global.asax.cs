using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using GdPicture14.WEB;

namespace FileMan
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DocuViewareManager.SetupConfiguration();
            DocuViewareLicensing.RegisterKEY("02c8c42521ba43bf8c7fe816602be4af88e8e6cd581ada6aGx4ZcGz7zE8pzwYyLtCWhhqcW/TElpE8vdNooePcDI5/eFimqlbgw4626xZ5akP8"); //Unlocking DocuVieware
        }
    }
}
