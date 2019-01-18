using Microsoft.ApplicationInsights;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class DesktopTelemetryService : ITelemetryService
    {
        private const int MaxTelemetryEventsPerSession = 2000;

        private TelemetryClient telemetryClient = new TelemetryClient();
        private int totalEventsSent = 0;

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
            this.TrySendEvent(() => this.telemetryClient.TrackException(ex));
        }

        public void TrackPageView(string pageName)
        {
            this.TrySendEvent(() => this.telemetryClient.TrackPageView(pageName));
        }

        public void TrackLogin()
        {
            this.TrySendEvent(() => this.telemetryClient.TrackEvent("Login"));
        }

        public void TrackCommand(CommandTypeEnum type)
        {
            this.TrySendEvent(() => this.telemetryClient.TrackEvent("Command", new Dictionary<string, string> { { "Type", EnumHelper.GetEnumName(type) } }));
        }

        public void TrackAction(ActionTypeEnum type)
        {
            this.TrySendEvent(() => this.telemetryClient.TrackEvent("Action", new Dictionary<string, string> { { "Type", EnumHelper.GetEnumName(type) } }));
        }

        public void TrackInteractiveGame(InteractiveGameModel game)
        {
            this.TrySendEvent(() => this.telemetryClient.TrackEvent("InteractiveGame", new Dictionary<string, string> { { "Name", game.name } }));
        }

        public void TrackSongRequest(SongRequestServiceTypeEnum songService)
        {
            this.TrySendEvent(() => this.telemetryClient.TrackEvent("SongRequest", new Dictionary<string, string> { { "Song Request Service", EnumHelper.GetEnumName(songService) } }));
        }

        public void Start()
        {
            string key = ChannelSession.SecretManager.GetSecret("ApplicationInsightsKey");
            if (!string.IsNullOrEmpty(key))
            {
                this.telemetryClient.InstrumentationKey = key;
            }
        }

        public void SetUserId(string userId)
        {
            this.telemetryClient.Context.User.Id = userId;
        }

        public void End()
        {
            Task.Run(() => { this.telemetryClient.Flush(); });
            Task.Delay(2000); // Allow time to flush
        }

        private void TrySendEvent(Action eventAction)
        {
            if (!ChannelSession.Settings.OptOutTracking && this.totalEventsSent < DesktopTelemetryService.MaxTelemetryEventsPerSession)
            {
                eventAction();
                this.totalEventsSent++;
            }
        }
    }
}
