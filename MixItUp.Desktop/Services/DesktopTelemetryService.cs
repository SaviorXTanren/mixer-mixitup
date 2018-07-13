using Microsoft.ApplicationInsights;
using MixItUp.Base;
using MixItUp.Base.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class DesktopTelemetryService : ITelemetryService
    {
        private TelemetryClient telemetryClient = new TelemetryClient();

        public DesktopTelemetryService()
        {
            this.telemetryClient.Context.Cloud.RoleInstance = "MixItUpApp";
            this.telemetryClient.Context.Cloud.RoleName = "MixItUpApp";
            this.telemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
            this.telemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
        }

        public void TrackException(Exception ex)
        {
            this.telemetryClient.TrackException(ex);
        }

        public void TrackPageView(string pageName)
        {
            this.telemetryClient.TrackPageView(pageName);
        }

        public void Start()
        {
            this.telemetryClient.InstrumentationKey = ChannelSession.SecretManager.GetSecret("ApplicationInsightsKey");
        }

        public void End()
        {
            this.telemetryClient.Flush();
            Task.Delay(1000); // Allow time to flush
        }
    }
}
