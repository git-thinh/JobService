using System.Collections.Generic;
using System.Web.Http;
using System.Linq;
using System.Configuration;

namespace Test
{
    public class ImageController : BaseController
    {
        //[LogInputOutput]
        [HttpGet]
        public IHttpActionResult all()
        {
            //return Ok<ICollection<string>>(m_app.CacheGetAllKey());
            return Ok<string>("Ok");
        }

        [HttpGet]
        public IHttpActionResult search(string key)
        {
            //string SITE_URI_LOCAL = ConfigurationManager.AppSettings["SITE_URI_LOCAL"];
            //var a = m_app.CacheGetAllKey().ToArray();
            //if (!string.IsNullOrWhiteSpace(key))
            //{
            //    key = key.ToLower();
            //    a = a.Where(x => x.ToLower().Contains(key)).ToArray();
            //}
            //return Ok<string[]>(a.Select(x => SITE_URI_LOCAL + x).ToArray());
            return Ok<string>("Ok");
        }

        //[HttpGet]
        //[Route("{instanceId:int}")]
        //public IHttpActionResult GetByInstanceId(int instanceId)
        //{
        //    return Ok<ICollection<string>>(m_app.CacheGetAllKey());
        //}
    }
}