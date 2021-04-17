﻿using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace JobWeb
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var app = new AppBuilderProvider(appBuilder);
            //Microsoft.AspNet.Identity.Owin
            appBuilder.CreatePerOwinContext(() => app);

            HttpConfiguration httpConfiguration = new HttpConfiguration();
            WebApiConfig.Register(httpConfiguration, app);
            appBuilder.UseWebApi(httpConfiguration);
        }
    }

    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config, IApp app)
        {
            config.Routes.MapHttpRoute(
                    name: "EventSocket",
                    routeTemplate: CONFIG.EVENT_WS_NAME,
                    defaults: new { controller = "Event", action = "Get" }
                );

            //config.Routes.MapHttpRoute(
            //        name: "TestApi",
            //        routeTemplate: "test",
            //        defaults: new { controller = "Test", action = "Get" }
            //    );

            config.Routes.MapHttpRoute(
                name: "ActionApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { action = "Get", id = RouteParameter.Optional }
            );

            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

            config.Filters.Add(new AuthorizeFilter(app));
        }
    }

    public class AuthorizeFilter : IActionFilter
    {
        readonly IApp m_app;
        public AuthorizeFilter(IApp app) : base() => m_app = app;
        public bool AllowMultiple { get { return false; } }
        public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext,
            CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            if (actionContext.Request.RequestUri.AbsolutePath == CONFIG.EVENT_WS_PATH
                || actionContext.Request.RequestUri.AbsolutePath == "/test"
                || m_app.ApiCheckLogin(actionContext.Request))
                return await continuation();
            else throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized));
        }
    }
}