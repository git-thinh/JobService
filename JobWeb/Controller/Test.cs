using Autofac;
using CSRedis;
using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;

namespace JobWeb
{
    public class TestController : BaseController
    {
        [HttpGet]
        public IHttpActionResult Get() => Ok<string>("Ok");
    }
}