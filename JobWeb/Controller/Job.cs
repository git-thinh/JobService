using System.Web.Http;
using Hangfire;
using System;
using System.Reflection;
using System.IO;
using System.Web.Hosting;
using Autofac;
using Hangfire.Server;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using Hangfire.Common;

namespace JobWeb
{
    public class JobController : BaseController
    {
        const string PATH_JOBS = "~/Dll/Jobs.dll";

        [HttpGet]
        public IHttpActionResult call(string name)
        {
            var jo = new oResponse();
            try
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    string pathDll = HostingEnvironment.MapPath(PATH_JOBS);
                    if (File.Exists(pathDll))
                    {
                        Assembly assembly = Assembly.LoadFrom(pathDll);
                        AppDomain.CurrentDomain.Load(assembly.GetName());
                        Type type = assembly.GetTypes().Where(x => x.FullName.ToLower().EndsWith("." + name.ToLower()))
                            .Take(1).SingleOrDefault();
                        if (type != null)
                        {
                            var updater = new ContainerBuilder();
                            updater.RegisterType(type);
                            updater.Update(m_app.Container);

                            var job = (IBackgroundJobPerformer)Activator.CreateInstance(type, new object[] { m_app });
                            var jobId = BackgroundJob.Enqueue(() => job.Perform(null));
                            //RecurringJob.AddOrUpdate<JImage>("IMG_SYNC", x => x.SyncInSharePoint(null), "* 23 * * *");
                            jo.Data = jobId;
                            jo.Ok = true;
                        }
                        else jo.Message = "Can not find a job Jobs." + name;
                    }
                }
                else jo.Message = "Missing ...?name=[NameJob]";
            }
            catch (Exception ex)
            {
                jo.Message = ex.Message;
            }
            return Ok<oResponse>(jo);
        }

        [HttpGet]
        public IHttpActionResult recurring(string name, string time = "* 23 * * *")
        {
            var jo = new oResponse();
            try
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    time = HttpUtility.UrlDecode(time);
                    string pathDll = HostingEnvironment.MapPath(PATH_JOBS);
                    if (File.Exists(pathDll))
                    {
                        Assembly assembly = Assembly.LoadFrom(pathDll);
                        AppDomain.CurrentDomain.Load(assembly.GetName());
                        Type type = assembly.GetTypes().Where(x => x.FullName.ToLower().EndsWith("." + name.ToLower()))
                            .Take(1).SingleOrDefault();
                        if (type != null)
                        {
                            var updater = new ContainerBuilder();
                            updater.RegisterType(type);
                            updater.Update(m_app.Container);

                            var job = (IBackgroundJobPerformer)Activator.CreateInstance(type, new object[] { m_app });
                            //var jobId = BackgroundJob.Enqueue(() => job.Perform(null));

                            string jobId = type.FullName;
                            RecurringJob.AddOrUpdate(jobId,() => job.Perform(null), time);

                            jo.Data = jobId;
                            jo.Ok = true;
                        }
                        else jo.Message = "Can not find a job Jobs." + name;
                    }
                }
                else jo.Message = "Missing ...?name=[NameJob]";
            }
            catch (Exception ex)
            {
                jo.Message = ex.Message;
            }
            return Ok<oResponse>(jo);
        }

        [HttpGet]
        public IHttpActionResult refresh()
        {

            var jo = new oResponse();
            try
            {
                string pathDll = HostingEnvironment.MapPath(PATH_JOBS);
                if (File.Exists(pathDll))
                {
                    Assembly assembly = Assembly.LoadFrom(pathDll);
                    AppDomain.CurrentDomain.Load(assembly.GetName());
                    var arr = assembly.GetTypes().Where(x => x.Name != "<>c").ToArray();
                    var manager = new RecurringJobManager();
                    var err = new Dictionary<string, string>() { };
                    var ls = new List<string>();
                    foreach (var type in arr)
                    {
                        try
                        {
                            var updater = new ContainerBuilder();
                            updater.RegisterType(type);
                            updater.Update(m_app.Container);

                            var job = (IBackgroundJobPerformer)Activator.CreateInstance(type, new object[] { m_app });
                            BackgroundJob.Enqueue(() => job.Perform(null));
                            //RecurringJob.AddOrUpdate(jobId, () => job.Perform(null), "* 23 * * *");
                            //manager.AddOrUpdate(type.FullName, Job.FromExpression(() => job.Perform(null)), "* 23 * * *");
                            ls.Add(type.FullName);
                        }
                        catch (Exception ex)
                        {
                            err.Add(type.FullName, ex.Message);
                        }
                    }

                    jo.Data = ls;
                    jo.Error = err;
                    jo.Ok = true;
                }
            }
            catch (Exception ex)
            {
                jo.Message = ex.Message;
            }
            return Ok<oResponse>(jo);            
        }

        [HttpGet]
        public IHttpActionResult test()
        {
            var jo = new oResponse();
            try
            {
                string pathDll = HostingEnvironment.MapPath(PATH_JOBS);
                if (File.Exists(pathDll))
                {
                    Assembly assembly = Assembly.LoadFrom(pathDll);
                    AppDomain.CurrentDomain.Load(assembly.GetName());
                    Type type = assembly.GetType("Jobs.Test");
                    if (type != null)
                    {
                        var updater = new ContainerBuilder();
                        updater.RegisterType(type);
                        updater.Update(m_app.Container);

                        var job = (IBackgroundJobPerformer)Activator.CreateInstance(type, new object[] { m_app });
                        var jobId = BackgroundJob.Enqueue(() => job.Perform(null));
                        //RecurringJob.AddOrUpdate<JImage>("IMG_SYNC", x => x.SyncInSharePoint(null), "* 23 * * *");
                        jo.Data = jobId;
                        jo.Ok = true;
                    }
                    else jo.Message = "Can not find a job Jobs.Test";
                }
            }
            catch (Exception ex)
            {
                jo.Message = ex.Message;
            }
            return Ok<oResponse>(jo);
        }

        [HttpGet]
        public IHttpActionResult Get()
        {
            var dic = new Dictionary<string, string[]>() { };

            var a = this.Request.RequestUri.ToString().Split('/');
            string url = a[0] + "//" + a[2] + "/api/job/";

            string[] arr = new string[] { };
            string pathDll = HostingEnvironment.MapPath(PATH_JOBS);
            if (File.Exists(pathDll))
            {
                var asm = Assembly.LoadFrom(pathDll);
                arr = asm.GetTypes()
                    .Where(x => x.Name != "<>c")
                    .Select(x => x.Name).ToArray();

                dic.Add("_", new string[] {
                    url + "refresh",
                    a[0] + "//" + a[2] + "/api/image/all",
                    a[0] + "//" + a[2] + "/api/image/search?key=",
                    url + "test",
                    url + "call?name=test",
                });

                foreach (var key in arr)
                    dic.Add(key, new string[] {
                        url + "call?name=" + key,
                        url + "recurring?name=" + key + "&time=*%2023%20*%20*%20*",
                    });
            }
            return Ok<Dictionary<string, string[]>>(dic);
        }
    }
}