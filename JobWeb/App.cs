using Autofac;
using Hangfire;
using Hangfire.Server;
using Owin;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace JobWeb
{
    public class AppBuilderProvider : IApp, IDisposable
    {
        public ISubscriber RedisSubscriber { set; get; }
        public IContainer AutofacContainer { set; get; }

        #region [ Ctor ]

        private IAppBuilder _app;
        public AppBuilderProvider(IAppBuilder app) => _app = initRouter(app);

        public IAppBuilder Get() => _app;
        public IApp getApp() => this;
        public void Dispose() { }

        #endregion

        private IAppBuilder initRouter(IAppBuilder app)
        {
            Redis.initInstance(this);
            Redis.initPubSub(this);

            var builder = new ContainerBuilder();
            builder.RegisterInstance<IApp>(this).SingleInstance();
            builder.RegisterType<IJob>();
            builder.RegisterType<IBackgroundJobPerformer>();
            var container = builder.Build();
            this.AutofacContainer = container;
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
                            case CONFIG.ADMIN_PATH:
                                if (context.requestAuthorizedOrRedirectLogin())
                                    return next();
                                else return Task.FromResult(0);
                            case "/logout":
                                return context.responseLogout();
                            case "/login":
                                return context.responseLogin();
                            case CONFIG.ADMIN_PATH + "/css17220":
                                return context.css17220();
                            case CONFIG.ADMIN_PATH + "/js17220":
                                return context.js17220();
                            default:
                                return next();
                        }
                }
            });

            app.initHangfire();

            JobTest();

            return app;
        }

        public bool ApiCheckLogin(HttpRequestMessage request) => request.ApiCheckLogin();
        public void EventSend(string text) => EventSocket.Send(text);
        public void EventRegister(object client) => EventSocket.Register(client);



        public void RedisPublish(string message) => Redis.Publish(message);
        public void RedisUpdate(string storeKey, string itemKey, Dictionary<string, object> data) => Redis.Update(storeKey, itemKey, data);
        public void RedisDelete(string storeKey, string itemKey) => Redis.Delete(storeKey, itemKey);
        public void RedisDeleteAll(string storeKey) => Redis.DeleteAll(storeKey);

        public void JobTest()
        {
            string pathDll = HostingEnvironment.MapPath("~/Jobs/JUrl.dll");
            if (File.Exists(pathDll))
            {
                var asm = Assembly.LoadFrom(pathDll);
                var type = asm.GetTypes().Where(x => x.Name != "<>c").Take(1).SingleOrDefault();
                if (type != null)
                {
                    var updater = new ContainerBuilder();
                    updater.RegisterType(type);
                    updater.Update(this.AutofacContainer);

                    var job = (IJob)Activator.CreateInstance(type, new object[] { (IApp)(this) });
                    var jobId = BackgroundJob.Enqueue(() => job.test(null));
                }
            }
        }
    }
}