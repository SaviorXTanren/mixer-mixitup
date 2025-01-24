using Microsoft.ApplicationInsights;
using MixItUp.Base;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MixItUp.WPF.Services
{
    public class WindowsTelemetryService : ITelemetryService
    {
        private const int MaxTelemetryEventsPerSession = 2000;

        private TelemetryClient telemetryClient = new TelemetryClient();
        private int totalEventsSent = 0;

        public WindowsTelemetryService()
        {
            this.telemetryClient.Context.Cloud.RoleInstance = "MixItUpApp";
            this.telemetryClient.Context.Cloud.RoleName = "MixItUpApp";
            this.telemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
            this.telemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            this.telemetryClient.Context.Component.Version = Assembly.GetEntryAssembly().GetName().Version.ToString();
        }

        public string Name { get { return "Telemetry"; } }

        public bool IsConnected { get; private set; }

        public Task<Result> Connect()
        {
            string key = ServiceManager.Get<SecretsService>().GetSecret("ApplicationInsightsKey");
            if (!string.IsNullOrEmpty(key))
            {
                this.telemetryClient.InstrumentationKey = key;
            }

            this.IsConnected = true;
            return Task.FromResult(new Result());
        }

        public async Task Disconnect()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() => { this.telemetryClient.Flush(); });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            await Task.Delay(2000); // Allow time to flush

            this.IsConnected = false;
        }

        public void TrackException(Exception ex)
        {
            this.TrySendEvent(() => this.telemetryClient.TrackException(ex));
        }

        public void TrackLogin(string userID, IEnumerable<StreamingPlatformTypeEnum> platforms)
        {
            this.TrySendEvent(() => this.telemetryClient.TrackEvent("Login", new Dictionary<string, string> { { "Platforms", string.Join(", ", platforms.Select(p => p.ToString())) } }));
        }

        public void TrackCommand(CommandTypeEnum type, string details = null)
        {
            if (string.IsNullOrEmpty(details))
            {
                details = "None";
            }
            this.TrySendEvent(() => this.telemetryClient.TrackEvent("Command", new Dictionary<string, string> { { "Type", EnumHelper.GetEnumName(type) }, { "Details", details } }));
        }

        public void TrackAction(ActionTypeEnum type)
        {
            this.TrySendEvent(() => this.telemetryClient.TrackEvent("Action", new Dictionary<string, string> { { "Type", EnumHelper.GetEnumName(type) } }));
        }

        public void TrackService(string type)
        {
            this.TrySendEvent(() => this.telemetryClient.TrackEvent("Service", new Dictionary<string, string> { { "Type", type } }));
        }

        public void SetUserID(string id)
        {
            this.telemetryClient.Context.User.Id = id.ToString();
        }

        private void TrySendEvent(Action eventAction)
        {
            if (ChannelSession.Settings != null && ChannelSession.Settings.OptOutTracking)
            {
                return;
            }

            if (this.totalEventsSent < WindowsTelemetryService.MaxTelemetryEventsPerSession)
            {
                eventAction();
                this.totalEventsSent++;
            }
        }
    }
}