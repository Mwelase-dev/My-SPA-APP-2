using System;
using System.Web;
using System.Web.Optimization;

namespace Intranet.UI.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.IgnoreList.Clear();
            AddDefaultIgnorePatterns(bundles.IgnoreList);

           bundles.Add(
               new ScriptBundle("~/scripts/es5")
               .Include("~/scripts/es5-shim.js")
               .Include("~/scripts/es5-sham.js"));

            bundles.Add(
                new ScriptBundle("~/scripts/modernizr")
                .Include("~/scripts/modernizr-{version}.js"));

            bundles.Add(
              new ScriptBundle("~/scripts/vendor")

                .Include(SetJqueryVersion())
                .Include("~/scripts/bootstrapjs")
                .Include("~/scripts/knockout-{version}.js")
                .Include("~/scripts/sammy-{version}.js")
                .Include("~/scripts/moment.js")
                .Include("~/scripts/Q.js")
                .Include("~/scripts/breeze.debug.js")
                .Include("~/scripts/toastr.js")
                .Include("~/scripts/jquery-ui-1.10.3.js")

                // HTML Editor
                .Include("~/scripts/tinymce/tiny_mce.js")
                .Include("~/scripts/tinymce/jquery.tinymce.js")
                .Include("~/scripts/tinymce/tiny_mce_jquery.js")

                //flot scripts(graph)
                .Include("~/scripts/flot/jquery.flot.js")
                .Include("~/scripts/flot/jquery.flot.stack.js")
                //.Include("~/scripts/flot/jquery.flot.stack.patched.js")

                // Gallery View
                .Include("~/scripts/galleryview/jquery.easing.1.3.js")
                .Include("~/scripts/galleryview/jquery.galleryview-3.0-dev.js")
                .Include("~/scripts/galleryview/jquery.timers-1.2.js")
                
                // JQuery time picker
                .Include("~/scripts/jquery-ui-timepicker-addon.js")

                // Custom extenders
                .Include("~/scripts/ko.extensions.js")
                );

            bundles.Add(
             new StyleBundle("~/Content/css")
                .Include("~/Content/ie10mobile.css") // Must be first. IE10 mobile viewport fix
                .Include("~/Content/durandal.css")
                .Include("~/Content/bootstrap.min.css")
                .Include("~/Content/bootstrap-responsive.min.css")
                .Include("~/Content/font-awesome.min.css")
                .Include("~/Content/toastr.css")
                .Include("~/Content/styles.css")
                .Include("~/Scripts/galleryview/css/jquery.galleryview-3.0-dev.css")
                .Include("~/Content/themes/base/jquery-ui.css")
             );
        }

        public static void AddDefaultIgnorePatterns(IgnoreList ignoreList)
        {
            if (ignoreList == null)
            {
                throw new ArgumentNullException("ignoreList");
            }

            ignoreList.Ignore("*.intellisense.js");
            ignoreList.Ignore("*-vsdoc.js");
            //ignoreList.Ignore("*.debug.js", OptimizationMode.WhenEnabled);
            //ignoreList.Ignore("*.min.js", OptimizationMode.WhenDisabled);
            //ignoreList.Ignore("*.min.css", OptimizationMode.WhenDisabled);
        }

        /// <summary>
        /// This is a way around to use the appropriate jquery.
        /// For I.E less than 9 use jqeury 1 else use jquery 2
        /// </summary>
        /// <returns></returns>
        private static string SetJqueryVersion()
        {
            //var browser = HttpContext.Current.Request.Browser;

            //if (browser.Browser.ToLower() == "ie" && double.Parse(browser.Version) < 9)
                return "~/scripts/jquery-1.11.0.js";
            //else
            //    return "~/scripts/jquery-{version}.js";
        }
        
    }
}
