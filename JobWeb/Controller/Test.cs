using Autofac;
using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;

namespace JobWeb
{
    public class TestController : BaseController
    {
        const string PATH_JOBS = "~/Dll/Jobs.dll";
        [HttpGet]
        public IHttpActionResult Get()
        {
            return Ok<string>("Ok");
        }
    }
}