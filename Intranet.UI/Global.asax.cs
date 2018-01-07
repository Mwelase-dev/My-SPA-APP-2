using System.Web.Routing;
using Intranet.UI.App_Start;
using System;
using System.Web;
using System.Web.Http;
using System.Web.Optimization;

namespace Intranet.UI
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            //AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            RouteTable.Routes.MapHubs();
        }
    }
}