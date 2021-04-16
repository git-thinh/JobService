using Hangfire.Dashboard;
using System;

public class DashboardAuthorizeFilter : IDashboardAuthorizationFilter
{
    readonly Func<DashboardContext, bool> fn_authorize = null;
    public DashboardAuthorizeFilter(Func<DashboardContext, bool> authorize) : base() { fn_authorize = authorize; }
    public bool Authorize(DashboardContext dbContext) => fn_authorize(dbContext);
}
