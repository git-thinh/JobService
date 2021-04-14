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
            string file = HostingEnvironment.MapPath("~/home.html");
            if (File.Exists(file)) s = File.ReadAllText(file);
            context.Response.ContentType = "text/html";
            return context.Response.WriteAsync(s);
        }

        public static Task responseLogout(this IOwinContext context)
        {
            int time;
            //string token = loginCheckGetToken(context);
            //if (token.Length > 0) m_token.TryRemove(token, out time);
            return context.redirectToLogin();
        }

        public static Task responseLogin(this IOwinContext context)
        {
            if (context.Request.Method == "GET")
            {
                //context.Response.Cookies.Delete("___ID");
                context.Response.OnSendingHeaders(state =>
                {
                    context.Response.Cookies.Delete("___ID");
                }, null);

                string s = string.Empty;
                string file = HostingEnvironment.MapPath("~/login.html");
                if (File.Exists(file)) s = File.ReadAllText(file);
                context.Response.ContentType = "text/html";
                return context.Response.WriteAsync(s);
            }

            //bool ok = false;
            //try
            //{
            //    if (Connected)
            //    {
            //        string json = new StreamReader(context.Request.Body).ReadToEndAsync().Result;
            //        oUser user = JsonConvert.DeserializeObject<oUser>(json);
            //        if (m_userpass.ContainsKey(user.Username))
            //        {
            //            string pass;
            //            if (m_userpass.TryGetValue(user.Username, out pass) && user.Password.Equals(pass))
            //            {
            //                string token = Guid.NewGuid().ToString();
            //                m_token.TryAdd(token, 1);
            //                context.Response.Cookies.Append("___ID", token);
            //                context.Response.ContentType = "application/json; charset=utf-8";
            //                ok = true;
            //            }
            //        }
            //    }

            //    if (ok) return context.Response.WriteAsync(@"{""ok"":true,""link"":""" + PATH_ROOT_ADMIN + @"""}");
            //    else return context.Response.WriteAsync(@"{""ok"":false}");
            //}
            //catch (Exception ex)
            //{
            //    return context.Response.WriteAsync(@"{""ok"":false,""error"":""" + ex.Message + @"""}");
            //}

            return context.Response.WriteAsync(@"{""ok"":true}");
        }

        public static Task redirectToLogin(this IOwinContext context)
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";
            //return context.Response.WriteAsync(@"<meta http-equiv=""refresh"" content=""0; url=/login"" />");
            return context.Response.WriteAsync(@"<script> location.href = '/login'; </script>");
        }

        public static bool requestAuthorizedOrRedirectLogin(this IOwinContext context)
        {
            if (string.IsNullOrWhiteSpace(context.requestCheckGetToken()))
            {
                context.redirectToLogin();
                return false;
            }
            return true;
        }

        public static string requestCheckGetToken(this IOwinContext context)
        {
            string token = context.Request.Cookies["___ID"];
            if (token.tokenValid()) return token;
            return string.Empty;
        }

        public static bool tokenValid(this string token)
        {
            //if (!string.IsNullOrWhiteSpace(token) && m_token.ContainsKey(token)) return true;
            //return false;
            return true;
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