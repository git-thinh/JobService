using System.Collections.Generic;
using System.Web.Http;
using System.Linq;
using System.Configuration;

namespace JobWeb
{
    public class UserController : BaseController
    {
        //[LogInputOutput]
        [HttpGet]
        public IHttpActionResult refresh()
        {
            //var a = m_app.LoadUserFromRedis();
            //return Ok<ICollection<string>>(a);
            return Ok<string>("Ok");
        }

    }
}