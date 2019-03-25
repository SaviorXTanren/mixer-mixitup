using Microsoft.ApplicationInsights;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Services;
using PlayFab;
using PlayFab.ClientModels;
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

            PlayFabSettings.staticSettings.TitleId = ChannelSession.SecretManager.GetSecret("PlayFabTitleID");
        }

        public void TrackException(Exception ex)
        {
            this.TrySendEvent(() => this.telemetryClient.TrackException(ex));
            this.SendPlayFabEvent("Exception", "Details", ex.ToString());
        }

        public void TrackPageView(string pageName)
        {
            this.TrySendEvent(() => this.telemetryClient.TrackPageView(pageName));
            this.SendPlayFabEvent("PageView", "Name", pageName);
        }

        public void TrackLogin(bool isStreamer, bool isPartner)
        {
            this.TrySendEvent(() => this.telemetryClient.TrackEvent("Login", new Dictionary<string, string> { { "Is Streamer", isStreamer.ToString() }, { "Is Partner", isPartner.ToString() } }));
            this.SendPlayFabEvent("Login", new Dictionary<string, object>() { { "Is Streamer", isStreamer.ToString() }, { "Is Partner", isPartner.ToString() } });
        }

        public void TrackCommand(CommandTypeEnum type, bool IsBasic)
        {
            this.TrySendEvent(() => this.telemetryClient.TrackEvent("Command", new Dictionary<string, string> { { "Type", EnumHelper.GetEnumName(type) }, { "Is Basic", IsBasic.ToString() } }));
            this.SendPlayFabEvent("Command", new Dictionary<string, object>() { { "Type", EnumHelper.GetEnumName(type) }, { "Is Basic", IsBasic.ToString() } });
        }

        public void TrackAction(ActionTypeEnum type)
        {
            this.TrySendEvent(() => this.telemetryClient.TrackEvent("Action", new Dictionary<string, string> { { "Type", EnumHelper.GetEnumName(type) } }));
            this.SendPlayFabEvent("Action", "Type", EnumHelper.GetEnumName(type));
        }

        public void TrackInteractiveGame(InteractiveGameModel game)
        {
            this.TrySendEvent(() => this.telemetryClient.TrackEvent("InteractiveGame", new Dictionary<string, string> { { "Name", game.name } }));
            this.SendPlayFabEvent("InteractiveGame", "Name", game.name);
        }

        public void TrackSongRequest(SongRequestServiceTypeEnum songService)
        {
            this.TrySendEvent(() => this.telemetryClient.TrackEvent("SongRequest", new Dictionary<string, string> { { "Song Request Service", EnumHelper.GetEnumName(songService) } }));
            this.SendPlayFabEvent("SongRequest", "Song Request Service", EnumHelper.GetEnumName(songService));
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
            this.TrySendPlayFabTelemetry(PlayFabClientAPI.LoginWithCustomIDAsync(new LoginWithCustomIDRequest { CustomId = userId, CreateAccount = true }));
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

        private void SendPlayFabEvent(string eventName, string key, object value) { this.SendPlayFabEvent(eventName, new Dictionary<string, object>() { { key, value } }); }

        private void SendPlayFabEvent(string eventName, Dictionary<string, object> body) { PlayFabClientAPI.WritePlayerEventAsync(new WriteClientPlayerEventRequest { EventName = eventName, Body = body }); }

        private void TrySendPlayFabTelemetry<T>(Task<T> eventTask)
        {
            Task.Run(async () =>
            {
                try
                {
                    T result = await eventTask;
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            });
        }
    }
}
