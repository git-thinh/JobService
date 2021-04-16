using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace JobWeb
{
    public static class Page
    {
        public static Task responseHome(this IOwinContext context)
        {
            string s = string.Empty;
            string file = HostingEnvironment.MapPath("~/site/home.html");
            if (File.Exists(file)) s = File.ReadAllText(file);
            context.Response.ContentType = "text/html";
            return context.Response.WriteAsync(s);
        }

        public static Task responseHtml(this IOwinContext context)
        {
            string s = string.Empty;
            context.Response.ContentType = "text/html";
            return context.Response.WriteAsync(s);
        }


        //static Task pageScriptResponse(IOwinContext context)
        //{
        //    string s = string.Empty;
        //    context.Response.ContentType = "text/html";
        //    return context.Response.WriteAsync(s);
        //}

        //static Task pageStyleResponse(IOwinContext context)
        //{
        //    string s = string.Empty;
        //    context.Response.ContentType = "text/html";
        //    return context.Response.WriteAsync(s);
        //}

        //static Task pageFontResponse(IOwinContext context)
        //{
        //    string s = string.Empty;
        //    context.Response.ContentType = "text/html";
        //    return context.Response.WriteAsync(s);
        //}

        //#endregion

        //#region [ PAGE - LOGIN, AUTHORIZE, USER ]

        //static Func<DashboardContext, bool> dashboardAuthorize = (dbContext) =>
        //{
        //    var context = new OwinContext(dbContext.GetOwinEnvironment());
        //    string path = context.Request.Uri.AbsolutePath,
        //        token = loginCheckGetToken(context);
        //    if (token.Length > 0) return true;
        //    loginRedirect(context);
        //    return false;
        //};



        //static string[] LoadUserFromRedis_()
        //{
        //    if (Connected)
        //    {
        //        m_userpass.Clear();
        //        var ds = _db.HashGetAll("USER");
        //        foreach (var kv in ds) m_userpass.TryAdd(kv.Name, (string)kv.Value);
        //    }

        //    return m_userpass.Keys.ToArray();
        //}
        //public string[] LoadUserFromRedis() => LoadUserFromRedis_();

        //#endregion


    }
}