using Autofac;
using Hangfire;
using Hangfire.Server;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace JobWeb
{
    using WebSocketSendAsync = Func<ArraySegment<byte>, int,/*message type*/ bool, /*end of message*/ CancellationToken, Task>;
    public class AppBuilderProvider : IAppJob, IDisposable
    {
        const string PATH_ROOT_JOB = "/admin";
        const string PATH_ROOT_ADMIN = "/admin";

        public IContainer Container { set; get; }

        #region [ Ctor ]

        private IAppBuilder _app;
        public AppBuilderProvider(IAppBuilder app) => _app = initRouter(app);

        public IAppBuilder Get() => _app;
        public IAppJob getApp() => this;
        public void Dispose() { }

        #endregion

        private IAppBuilder initRouter(IAppBuilder app)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance<IAppJob>(this).SingleInstance();
            builder.RegisterType<IBackgroundJobPerformer>();
            var container = builder.Build();
            this.Container = container;
            GlobalConfiguration.Configuration.UseAutofacActivator(container, false);

            app.Use((context, next) =>
            {
                string path = context.Request.Uri.AbsolutePath, ext = string.Empty;
                if (path.Length > 3) ext = path.Substring(path.Length - 3, 3);
                switch (ext)
                {
                    ////case "jpg":
                    ////case "shx":
                    ////    return pageImage(context, next);
                    ////case "tml":
                    ////    return pageHtmlResponse(context);
                    ////case ".js":
                    ////    return pageScriptResponse(context);
                    ////case "css":
                    ////    return pageStyleResponse(context);
                    ////case "iff":
                    ////    return pageFontResponse(context);
                    //////case "/admin/stats": return next();
                    default:
                        switch (path)
                        {
                            case "/": return context.responseHome();
                            case PATH_ROOT_ADMIN:
                                if (context.requestAuthorizedOrRedirectLogin())
                                    return Task.FromResult(0);
                                else return next();
                            case "/favicon.ico":
                                return context.Response.WriteAsync(string.Empty);
                            case "/logout":
                                return context.responseLogout();
                            case "/login":
                                return context.responseLogin();
                            //case PATH_ROOT_JOB + "/css17220":
                            //    return css17200(context);
                            //case PATH_ROOT_JOB + "/js17220":
                            //    return js17200(context);
                            default:
                                return next();
                        }
                }
            });

            //app.MapWhen(context => context.Request.Uri.AbsolutePath.Equals("/login"),
            //    (appBuilder_) => appBuilder_.Run((ctx)=> ctx.responseLogin()));

            //setupJob(app);

            return app;
        }

        public void EventSend(string text)
        {
            throw new NotImplementedException();
        }

        public void EventRegister(object client)
        {
            throw new NotImplementedException();
        }

        public bool LoginCheckApi(HttpRequestMessage request)
        {
            throw new NotImplementedException();
        }

    }
}