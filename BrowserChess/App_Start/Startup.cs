using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(BrowserChess.App_Start.Startup))]

namespace BrowserChess.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}