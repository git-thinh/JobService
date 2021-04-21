using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace Test
{
    public class CustomOAuthProvider2 : OAuthAuthorizationServerProvider
    {
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            //string clientId;
            //string clientSecret;

            //if (context.TryGetBasicCredentials(out clientId, out clientSecret))
            //{
            //    // validate the client Id and secret 
            //    context.Validated();
            //}
            //else
            //{
            //    context.SetError("invalid_client", "Client credentials could not be retrieved from the Authorization header");
            //    context.Rejected();
            //}
            return Task.FromResult<object>(null);
        }

        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            //Dummy check here, you need to do your DB checks against membership system http://bit.ly/SPAAuthCode
            if (context.UserName != context.Password)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect");
                //return;
                return Task.FromResult<object>(null);
            }

            var identity = new ClaimsIdentity("JWT");

            identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
            identity.AddClaim(new Claim("sub", context.UserName));
            identity.AddClaim(new Claim(ClaimTypes.Role, "Manager"));
            identity.AddClaim(new Claim(ClaimTypes.Role, "Supervisor"));

            var props = new AuthenticationProperties(new Dictionary<string, string>
                {
                    {
                         "audience", (context.ClientId == null) ? string.Empty : context.ClientId
                    }
                });

            var ticket = new AuthenticationTicket(identity, props);
            context.Validated(ticket);
            return Task.FromResult<object>(null);

            //////UserManager<IdentityUser> userManager = context.OwinContext.GetUserManager<UserManager<IdentityUser>>();
            //////IdentityUser user;
            //////try
            //////{
            //////    user = await userManager.FindAsync(context.UserName, context.Password);
            //////}
            //////catch
            //////{
            //////    // Could not retrieve the user due to error.
            //////    context.SetError("server_error");
            //////    context.Rejected();
            //////    return;
            //////}
            //////if (user != null)
            //////{
            //////    ClaimsIdentity identity = await userManager.CreateIdentityAsync(
            //////                                            user,
            //////                                            DefaultAuthenticationTypes.ExternalBearer);
            //////    context.Validated(identity);
            //////}
            //////else
            //////{
            //////    context.SetError("invalid_grant", "Invalid UserId or password'");
            //////    context.Rejected();
            //////}
        }
    }

    //private static UserManager<IdentityUser> CreateManager(IdentityFactoryOptions<UserManager<IdentityUser>> options, IOwinContext context)
    //{
    //    var userStore = new UserStore<IdentityUser>(context.Get<OwinAuthDbContext>());
    //    var owinManager = new UserManager<IdentityUser>(userStore);
    //    return owinManager;
    //}
}