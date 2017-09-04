using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RumoPatios.Startup))]
namespace RumoPatios
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
