using System.Web;
using System.Web.Optimization;
using SenchaMinify;

namespace SenchaMinify.Demo
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(
                new SenchaBundle("~/bundles/extjs4-mvc-complex-dashboard")
                .IncludeDirectory("~/Scripts/extjs4-mvc-complex-dashboard", "*.js", true)
            );

            bundles.Add(
                new SenchaBundle("~/bundles/arrayGrid")
                .IncludeDirectory("~/Scripts/arrayGrid", "*.js", true)
            );

            bundles.Add(
                new SenchaBundle("~/bundles/mvc-portal")
                .IncludeDirectory("~/Scripts/mvc-portal", "*.js", true)
            );
        }
    }
}