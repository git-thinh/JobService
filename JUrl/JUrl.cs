using Hangfire.Console;
using Hangfire.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using LZ4;

public class JUrl : IJob
{
    readonly IApp m_app;
    CancellationTokenSource cancellationToken;
    public JUrl(IApp app) => m_app = app;
    public void Cancel() => cancellationToken.Cancel();

    public void call(PerformContext context, Dictionary<string, object> data)
    {
        cancellationToken = new CancellationTokenSource();
        if (data == null) return;
        string url = data.Get<string>("url", string.Empty);
        if (url.Length > 0)
        {
            var buf = downloadUrl(url);
            string ok = buf.Buffer != null ? "OK" : "FAIL";

            var lz = LZ4Codec.Wrap(buf.Buffer);
            //var decompressed = LZ4Codec.Unwrap(compressed);
            long s1 = buf.Buffer.Length;
            long s2 = lz.Length;
            buf.Buffer = lz;

            data.Add("_job_ok", ok == "OK");
            data.Add("_job", "JUrl");
            data.Add("_data", buf.Buffer);

            m_app.RedisUpdate("URL", url, data);

            context.WriteLine("-> Started: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss" + " ..."));
            context.WriteLine("-> " + ok + ": " + url);
        }
    }

    public void test(PerformContext context)
    {
        var data = new Dictionary<string, object>() {
            { "url", "https://vnexpress.net/" },
            { "_rd_publish", true }
        };
        call(context, data);
    }

    static oBuffer downloadUrl(string url)
    {
        oBuffer rs = new oBuffer() { Url = url };
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = 3000;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                rs.MineType = response.ContentType;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (Stream responseStream = request.GetResponse().GetResponseStream())
                    {
                        byte[] buffer = new byte[0x1000];
                        int bytes;
                        while ((bytes = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                            ms.Write(buffer, 0, bytes);
                    }
                    rs.Buffer = ms.ToArray();
                }
            }
        }
        catch (Exception ex)
        {
            rs.Error = ex.StackTrace;
        }
        return rs;
    }
}