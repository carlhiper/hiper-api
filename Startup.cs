using System;
using System.Web.Http;
using Hiper.Api;
using Hiper.Api.Providers;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.OAuth;
using Owin;

[assembly: OwinStartup(typeof (Startup))]

namespace Hiper.Api
{
    public class Startup
    {
        internal static IDataProtectionProvider DataProtectionProvider { get; private set; }

        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            var config = new HttpConfiguration();

            WebApiConfig.Register(config);
            ConfigureOAuth(app);
            AutoMapperConfig.RegisterMappings();
            app.UseWebApi(config);

            // Enqueue a job
        }


        public void ConfigureOAuth(IAppBuilder app)
        {
            DataProtectionProvider = app.GetDataProtectionProvider();
            var oAuthServerOptions = new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(31),
                Provider = new AuthorizationServerProvider()
            };


            // Token Generation
            app.UseOAuthAuthorizationServer(oAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
        }
    }
}