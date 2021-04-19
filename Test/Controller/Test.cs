using System.Web.Http;

namespace Test
{
    public class TestController : BaseController
    {
        [HttpGet]
        public IHttpActionResult Get() => Ok<string>("Ok");
    }
}