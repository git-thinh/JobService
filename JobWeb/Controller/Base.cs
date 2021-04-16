﻿using Microsoft.AspNet.Identity.Owin;
using System.Web;
using System.Web.Http;

namespace JobWeb
{
    public class BaseController : ApiController
    {
        public readonly IAppJob m_app;
        public BaseController() => m_app = HttpContext.Current.GetOwinContext().Get<AppBuilderProvider>().getApp();
    }
}