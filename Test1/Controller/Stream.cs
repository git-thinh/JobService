using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Test
{
    public class StreamController : BaseController
    {
        [HttpGet]
        public HttpResponseMessage Get()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            //response.Headers.TransferEncodingChunked = true;
            //var content = new PushStreamContent(new Action<Stream, HttpContent, TransportContext>(WriteContent2), "application/json");
            
            var content = new PushStreamContent(new Action<Stream, HttpContent, TransportContext>(WriteContent), "text/plain");

            response.Content = content;
            return response;
        }

        public static void WriteContent(Stream stream, HttpContent content, TransportContext context)
        {
            ////Read in small 64 byte blocks
            ////Global.getRedis().StreamTo(stream, 64, r => r.Get("1mb"));
            //Global.getRedis().StreamTo(stream, 1024*1024, r => r.Get("1mb"));
            ////Global.getRedis().StreamTo(stream, 1024, r => r.Get("500kb"));
            //stream.Flush();
            //stream.Close();
        }

        public static void WriteContent2(Stream stream, HttpContent content, TransportContext context)
        {
            var serializer = JsonSerializer.CreateDefault();
            using (var sw = new StreamWriter(stream))
            using (var jw = new JsonTextWriter(sw))
            {
                jw.WriteStartArray();
                foreach (var id in Enumerable.Range(1, 100000))
                {
                    serializer.Serialize(jw, new TestModel()
                    {
                        Alias = "rvhuang",
                        BirthDate = new DateTime(1985, 02, 13),
                        FirstName = "Robert",
                        LastName = "Huang",
                        ID = id,
                        MiddleName = "Vandenberg",
                    });
                }
                jw.WriteEndArray();
            }
        }
    }
    public class TestModel
    {
        public string FirstName
        {
            get; set;
        }

        public string MiddleName
        {
            get; set;
        }

        public DateTime BirthDate
        {
            get; set;
        }

        public string LastName
        {
            get; set;
        }

        public string Alias
        {
            get; set;
        }

        public int ID
        {
            get; set;
        }
    }
}