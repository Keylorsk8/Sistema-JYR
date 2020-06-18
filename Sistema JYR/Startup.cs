using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Sistema_JYR.Startup))]
namespace Sistema_JYR
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
