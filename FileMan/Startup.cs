using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FileMan.Startup))]
namespace FileMan
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
