using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ERPComisions.Startup))]
namespace ERPComisions
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
