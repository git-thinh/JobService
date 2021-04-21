using Microsoft.Owin;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Test
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ThreadPool.SetMinThreads(10001, 10001);

            var iapp = new AppBuilderProvider(app);
            //RedisStatic.initInstance(iapp);
            //RedisStatic.initPubSub(iapp);

            iapp.initRouter();
            app.CreatePerOwinContext(() => iapp);

            WebApiConfig.Register(app, iapp);

            //Global.UpdateRedis(new Dictionary<string, object>() { { "key", 12345 } });
        }
    }

    public class WebApiConfig
    {
        public static void Register(IAppBuilder app, IApp iapp)
        {
            HttpConfiguration config = new HttpConfiguration();

            config.Routes.MapHttpRoute(
                    name: "EventSocket",
                    routeTemplate: CONFIG.EVENT_WS_NAME,
                    defaults: new { controller = "Event", action = "Get" }
                );

            config.Routes.MapHttpRoute(
                    name: "TestApi",
                    routeTemplate: "test/{action}",
                    defaults: new { controller = "Test", action = "Get" }
                );

            config.Routes.MapHttpRoute(
                name: "ActionApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { action = "Get", id = RouteParameter.Optional }
            );

            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

            config.Filters.Add(new AuthorizeFilter(iapp));

            SettingOAuthAuthorization(app);

            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            app.UseWebApi(config);
        }

        static void SettingOAuthAuthorization(IAppBuilder app)
        {
            // OAuth 2.0 Bearer Access Token Generation
            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions()
            {
                //For Dev enviroment only (on production should be AllowInsecureHttp = false)
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/oauth/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30),

                Provider = new CustomOAuthProvider2(),

                //Provider = new CustomOAuthProvider(),
                //AccessTokenFormat = new CustomJwtFormat("http://jwtauthzsrv.azurewebsites.net")
            });
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
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