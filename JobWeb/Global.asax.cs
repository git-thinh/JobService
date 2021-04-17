using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace JobWeb
{
    public class Global : System.Web.HttpApplication
    {
        static CSRedis.IRedisClient redisClient;
        public static CSRedis.IRedisClient getRedis() => redisClient;

        protected void Application_Start(object sender, EventArgs e)
        {
            redisClient = new CSRedis.RedisClient("localhost", 1000);
            redisClient.Set("1mb", new string('x', 1048576));
            redisClient.Set("500kb", new string('x', 524288));
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            //if (Request.Url.AbsolutePath == "/test/stream")
            //{
            //    //Response.ContentType = "application/octet-stream";
            //    Response.ContentType = "text/plain";
            //    //Read in small 64 byte blocks
            //    redisClient.StreamTo(Response.OutputStream, 64, r => r.Get("1mb"));
            //    base.CompleteRequest();
            //}
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}