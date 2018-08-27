using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OnlinePermissionSlips.Startup))]
namespace OnlinePermissionSlips
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
