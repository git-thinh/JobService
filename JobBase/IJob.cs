using Hangfire.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IJob
{ 
    void test(PerformContext context);
    void call(PerformContext context, Dictionary<string, object> data);
}
