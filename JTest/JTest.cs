using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

//[LogEverything]
public class JTest : IJob
{
    readonly IApp m_app;
    public JTest(IApp app) => m_app = app;

    CancellationTokenSource cancellationToken;
    public void Cancel() => cancellationToken.Cancel();

    //[ProcessQueue("queueName")]
    //[LogEverything]
    public void test(PerformContext context)
    {
        cancellationToken = new CancellationTokenSource();

        //using (var conn = JobStorage.Current.GetConnection())
        //{
        //    var storage = (JobStorageConnection)conn;
        //    if (storage != null)
        //        var itemsInSet = storage.GetAllItemsFromSet("queueName");
        //}

        //bool isRemove = false;
        //var connection = JobStorage.Current.GetConnection();
        //var jo = connection.GetRecurringJobs().Where(x => x.Id == context.JobId).Take(1).SingleOrDefault();
        //if (jo == null) 
        //    isRemove = true;
        //else 
        //    isRemove = jo.Removed;
        //if (isRemove)
        //    return;

        context.WriteLine(DateTime.Now.ToString("-> Started: yyyy-MM-dd HH:mm:ss ..."));
        var progressBar = context.WriteProgressBar();
        foreach (var i in Enumerable.Range(1, 100).ToList().WithProgress(progressBar))
        //foreach (var i in Enumerable.Range(1, 100).ToList())
        {
            //m_app.NotifySend(context.JobId);
            context.WriteLine(string.Format("-> {0}/100 ...", i));
            if (i == 25) Cancel();
            Thread.Sleep(1000);
        }
        context.WriteLine(DateTime.Now.ToString("-> Complete: yyyy-MM-dd HH:mm:ss ..."));


        ////context.SetTextColor(ConsoleTextColor.Red);
        ////context.WriteLine("Error!");
        ////context.ResetTextColor();
        ////// create progress bar
        ////var progress = context.WriteProgressBar();
        ////// update value for previously created progress bar
        ////progress.SetValue(50);

    }

    public void call(PerformContext context, Dictionary<string, object> data) => test(context);
}