﻿using System;
using System.Configuration;
using System.Linq;

namespace JobWeb
{
    public class Global : System.Web.HttpApplication
    {
        static CSRedis.IRedisClient redisClient;
        public static CSRedis.IRedisClient getRedis() => redisClient;

        protected void Application_Start(object sender, EventArgs e)
        {
            //string conStr = "127.0.0.1:6379,asyncPipeline=true,preheat=100,poolsize=100";
            //127.0.0.1[:6379],password=123456,defaultDatabase=13,poolsize=50,ssl=false,writeBuffer=10240,prefix=key
            string redisConnectStr = ConfigurationManager.AppSettings["REDIS_CONNECT"];
            var arr = redisConnectStr.Split(new char[] { ':', ',' });
            int db = 0;
            int.TryParse(arr.Where(x => x.StartsWith("defaultDatabase")).Take(1).FirstOrDefault()
                .Substring("defaultDatabase".Length).Trim().Substring(1).Trim(), out db);
            redisClient = new CSRedis.RedisClient(arr[0], int.Parse(arr[1]));
            redisClient.Select(db);
            //redisClient.Set("1mb", new string('x', 1048576));
            //redisClient.Set("500kb", new string('x', 524288));
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
    }
}