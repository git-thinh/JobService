using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Test
{
   
    public class BasicAuthenController : ApiController
    {
        [BasicAuthentication]
        public string Get()
        {
            return "WebAPI Method Called";
        }
    }
}

/*
$.ajax({
    type: 'GET',
    url: "api/BasicAuthen",
    datatype: 'json',
    headers: { Authorization: 'Basic ' + btoa('admin:12345') },
    success: function (data) { console.log(data); },
    error: function (data) {}
});
 */
