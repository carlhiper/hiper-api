
using System.Security.Claims;
using System.Threading.Tasks;
using Hiper.Api.Repositories;
using Microsoft.Owin.Security.OAuth;

namespace Hiper.Api.Providers
{
    public class AuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            const string corsHeader = "Access-Control-Allow-Origin";
            if (!context.Response.Headers.ContainsKey(corsHeader))
            {
                context.Response.Headers.Add(corsHeader, new[] {"*"});
            }
          
                var repo = new UserRepository();
                var user = await repo.FindUser(context.UserName, context.Password);

                if (user == null)
                {
                    context.SetError("invalid_grant", "The user name or password is incorrect.");
                    return;
                }

            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim("sub", context.UserName));
            identity.AddClaim(new Claim("role", "user"));

            context.Validated(identity);
        }
    }
}