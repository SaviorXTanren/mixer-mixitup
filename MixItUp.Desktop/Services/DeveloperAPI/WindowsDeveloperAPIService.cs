using Microsoft.Owin.Hosting;
using MixItUp.Base.Services;
using Owin;
using System;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace MixItUp.Desktop.Services.DeveloperAPI
{
    public class WindowsDeveloperAPIService : IDeveloperAPIService
    {
        private IDisposable webApp;
        public readonly string[] DeveloperAPIHttpListenerServerAddresses = new string[] { "http://localhost:8911/", "http://127.0.0.1:8911/" };

        public bool Start()
        {
            // Ensure it is cleaned up first
            End();

            StartOptions opts = new StartOptions();
            foreach(var url in DeveloperAPIHttpListenerServerAddresses)
            {
                opts.Urls.Add(url);
            }

            this.webApp = WebApp.Start<WindowsDeveloperAPIServiceStartup>(opts);
            return true;
        }

        public void End()
        {
            if (this.webApp != null)
            {
                this.webApp.Dispose();
                this.webApp = null;
            }
        }
    }

    public class WindowsDeveloperAPIServiceStartup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();

            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());

            config.MapHttpAttributeRoutes();
            config.MessageHandlers.Add(new NoCacheHeader());

            appBuilder.UseWebApi(config);
        }
    }
}
