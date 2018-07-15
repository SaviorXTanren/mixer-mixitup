using Microsoft.ApplicationInsights;
using MixItUp.Base;
using MixItUp.Base.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            this.telemetryClient.Context.Component.Version = Assembly.GetEntryAssembly().GetName().Version.ToString();
        }

        public void TrackException(Exception ex)
        {
            if (!ChannelSession.Settings.OptOutTracking)
            {
                RefreshUserDetails();
                this.telemetryClient.TrackException(ex);
            }
        }

        public void TrackPageView(string pageName)
        {
            if (!ChannelSession.Settings.OptOutTracking)
            {
                RefreshUserDetails();
                this.telemetryClient.TrackPageView(pageName);
            }
        }

        public void TrackLogin()
        {
            if (!ChannelSession.Settings.OptOutTracking)
            {
                RefreshUserDetails();
                this.telemetryClient.TrackEvent("Login");
            }
        }

        public void Start()
        {
            string key = ChannelSession.SecretManager.GetSecret("ApplicationInsightsKey");
            if (!string.IsNullOrEmpty(key))
            {
                this.telemetryClient.InstrumentationKey = key;
            }
        }

        public void End()
        {
            this.telemetryClient.Flush();
            Task.Delay(1000); // Allow time to flush
        }

        private void RefreshUserDetails()
        {
            if (string.IsNullOrEmpty(this.telemetryClient.Context.User.Id))
            {
                if (string.IsNullOrEmpty(ChannelSession.Settings.TelemetryUserId))
                {
                    if (MixItUp.Base.Util.Logger.IsDebug)
                    {
                        ChannelSession.Settings.TelemetryUserId = "MixItUpDebuggingUser";
                    }
                    else
                    {
                        ChannelSession.Settings.TelemetryUserId = Guid.NewGuid().ToString();
                    }
                }

                this.telemetryClient.Context.User.Id = ChannelSession.Settings.TelemetryUserId;
            }
        }
    }
}
