using Microsoft.AspNet.Identity.Owin;
using System.Web;
using System.Web.Http;

namespace Test
{
    public class BaseController : ApiController
    {
        public readonly IApp m_app;
        public BaseController() => m_app = HttpContext.Current.GetOwinContext().Get<AppBuilderProvider>().getApp();
    }
}