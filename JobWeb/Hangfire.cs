using Autofac;
using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard;
using Hangfire.InMemory;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace JobWeb
{
    public static class HangfireExtension
    {
        public static void initHangfire(this IAppBuilder app)
        {
            GlobalConfiguration.Configuration
                //.UseIgnoredAssemblyVersionTypeResolver()
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseInMemoryStorage(new InMemoryStorageOptions()
                {
                    //DisableJobSerialization = false
                })
                //.UseFilter(new LogFailureAttribute())
                .UseConsole()
            //.UseLogProvider(new CustomLogProvider())
            ;
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 5 });

            var dashboardOptions = new DashboardOptions
            {
                //IgnoreAntiforgeryToken = true,
                //IsReadOnlyFunc = (DashboardContext context) => true,
                DashboardTitle = "Admin",
                Authorization = new[] { new DashboardAuthorizeFilter(dashboardAuthorize) }
            };
            app.UseHangfireDashboard(CONFIG.ADMIN_PATH, dashboardOptions);
            app.UseHangfireServer();

        }

        static Func<DashboardContext, bool> dashboardAuthorize = (dbContext)
            => (new OwinContext(dbContext.GetOwinEnvironment())).requestAuthorizedOrRedirectLogin();

        public static Task css17220(this IOwinContext context)
        {
            StringBuilder bi = new StringBuilder(string.Empty);
            string path = HostingEnvironment.MapPath("~/Asset/Hangfire/css");
            if (Directory.Exists(path))
            {
                var fs = Directory.GetFiles(path, "*.css").OrderBy(x => x).ToArray();
                foreach (string file in fs) bi.Append(File.ReadAllText(file));
            }
            context.Response.ContentType = "text/css";
            return context.Response.WriteAsync(bi.ToString());
        }

        public static Task js17220(this IOwinContext context)
        {
            StringBuilder bi = new StringBuilder(string.Empty);
            string path = HostingEnvironment.MapPath("~/Asset/Hangfire/js");
            if (Directory.Exists(path))
            {
                var fs = Directory.GetFiles(path, "*.js").OrderBy(x => x).ToArray();
                foreach (string file in fs) bi.Append(File.ReadAllText(file));
            }
            context.Response.ContentType = "text/javascript";
            return context.Response.WriteAsync(bi.ToString());
        }

    }
}