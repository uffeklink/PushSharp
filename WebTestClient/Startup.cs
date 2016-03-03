using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebTestClient.Startup))]
namespace WebTestClient
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
