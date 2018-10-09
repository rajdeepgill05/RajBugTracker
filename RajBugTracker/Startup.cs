using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RajBugTracker.Startup))]
namespace RajBugTracker
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
