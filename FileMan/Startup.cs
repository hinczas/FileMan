using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Raf.FileMan.Startup))]
namespace Raf.FileMan
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
