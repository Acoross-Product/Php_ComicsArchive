using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Achive.Startup))]
namespace Achive
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
