using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using MixItUp.Base.Services.External;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.DeveloperAPI
{
    public class WindowsDeveloperAPIService : IDeveloperAPIService
    {
        private IWebHost webApp;
        public readonly string[] DeveloperAPIHttpListenerServerAddresses = new string[] { "http://*:8911/" };

        public string Name { get { return "Developer API"; } }

        public bool IsConnected { get; private set; }

        public async Task<ExternalServiceResult> Connect()
        {
            // Ensure it is cleaned up first
            await this.Disconnect();

            this.webApp = WebHost
                .CreateDefaultBuilder()
                .UseUrls(DeveloperAPIHttpListenerServerAddresses)
                .UseStartup<WindowsDeveloperAPIServiceStartup>()
                .Build();

            var _ = this.webApp.RunAsync();

            this.IsConnected = true;
            return new ExternalServiceResult(true);
        }

        public async Task Disconnect()
        {
            if (this.webApp != null)
            {
                try
                {
                    await this.webApp.StopAsync();
                }
                catch { }

                this.webApp.Dispose();
                this.webApp = null;
            }
            this.IsConnected = false;
        }
    }

    public class WindowsDeveloperAPIServiceStartup
    {
        private const string MethodOverrideHeader = "X-HTTP-Method-Override";

        public IConfiguration Configuration { get; }

        public WindowsDeveloperAPIServiceStartup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Use(this.AddCustomHeaders);
            app.UseMvc();
        }

        private Task AddCustomHeaders(HttpContext context, Func<Task> next)
        {
            context.Response.Headers.Add("Cache-Control", "no-cache");
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Access-Control-Allow-Origin");
            context.Response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS, PUT, DELETE");

            if (context.Request.Headers.TryGetValue(MethodOverrideHeader, out StringValues values))
            {
                context.Request.Method = values.FirstOrDefault();
            }

            if (!context.Request.Method.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return next.Invoke();
            }
            else
            {
                context.Response.StatusCode = 200;
                return context.Response.WriteAsync("OK");
            }
        }
    }
}
