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
                    case "tml":
                        return context.responseHtml();
                    case "/admin/stats": 
                        return next();
                    default:
                        switch (path)
                        {
                            case "/": return context.responseHome();
                            case "/favicon.ico":
                                return context.Response.WriteAsync(string.Empty);
                            case ConfigJob.PATH_ROOT:
                                if (context.requestAuthorizedOrRedirectLogin())
                                    return Task.FromResult(0);
                                else return next();
                            case "/logout":
                                return context.responseLogout();
                            case "/login":
                                return context.responseLogin();
                            case ConfigJob.PATH_ROOT + "/css17220":
                                return context.css17220();
                            case ConfigJob.PATH_ROOT + "/js17220":
                                return context.js17220();
                            default:
                                return next();
                        }
                }
            });

            app.initHangfire();

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