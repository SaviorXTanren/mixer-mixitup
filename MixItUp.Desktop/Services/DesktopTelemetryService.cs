using Microsoft.ApplicationInsights;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
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
        }

        public void Start()
        {
            string key = ChannelSession.SecretManager.GetSecret("ApplicationInsightsKey");
            if (!string.IsNullOrEmpty(key))
            {
                this.telemetryClient.InstrumentationKey = key;
            }

            PlayFabSettings.staticSettings.TitleId = ChannelSession.SecretManager.GetSecret("PlayFabTitleID");
        }

        public void End()
        {
            Task.Run(() => { this.telemetryClient.Flush(); });
            Task.Delay(2000); // Allow time to flush
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

        public void TrackLogin(string userID, bool isStreamer, bool isPartner)
        {
            this.TrySendEvent(() => this.telemetryClient.TrackEvent("Login", new Dictionary<string, string> { { "Is Streamer", isStreamer.ToString() }, { "Is Partner", isPartner.ToString() } }));
            this.SendPlayFabEvent("Login", new Dictionary<string, object>() { { "IsStreamer", isStreamer.ToString() }, { "IsPartner", isPartner.ToString() } });
            this.TrySendPlayFabTelemetry(PlayFabClientAPI.UpdateUserDataAsync(new UpdateUserDataRequest() { Data = new Dictionary<string, string>() { { "UserID", userID }, { "Platform", "Windows" }, { "IsStreamer", isStreamer.ToString() }, { "IsPartner", isPartner.ToString() } } }));
        }

        public void TrackCommand(CommandTypeEnum type, bool IsBasic)
        {
            this.TrySendEvent(() => this.telemetryClient.TrackEvent("Command", new Dictionary<string, string> { { "Type", EnumHelper.GetEnumName(type) }, { "Is Basic", IsBasic.ToString() } }));
            this.SendPlayFabEvent("Command", new Dictionary<string, object>() { { "Type", EnumHelper.GetEnumName(type) }, { "IsBasic", IsBasic.ToString() } });
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
            this.SendPlayFabEvent("SongRequest", "SongRequestService", EnumHelper.GetEnumName(songService));
        }

        public void TrackRemoteAuthentication(Guid clientID)
        {
            this.telemetryClient.TrackEvent("RemoteAuthentication", new Dictionary<string, string> { { "ClientID", clientID.ToString() } });
            this.SendPlayFabEvent("RemoteAuthentication", "ClientID", clientID.ToString());
            this.TrySendPlayFabTelemetry(PlayFabClientAPI.UpdateUserDataAsync(new UpdateUserDataRequest() { Data = new Dictionary<string, string>() { { "IsRemoteHost", true.ToString() } } }));
        }

        public void TrackRemoteSendProfiles(Guid clientID)
        {
            this.telemetryClient.TrackEvent("RemoteSendProfiles", new Dictionary<string, string> { { "ClientID", clientID.ToString() } });
            this.SendPlayFabEvent("RemoteSendProfiles", "ClientID", clientID.ToString());
        }

        public void TrackRemoteSendBoard(Guid clientID, Guid profileID, Guid boardID)
        {
            this.telemetryClient.TrackEvent("RemoteSendBoard", new Dictionary<string, string> { { "ClientID", clientID.ToString() }, { "ProfileID", profileID.ToString() },
                { "BoardID", boardID.ToString() } });
            this.SendPlayFabEvent("RemoteSendBoard", new Dictionary<string, object>() { { "ClientID", clientID.ToString() }, { "ProfileID", profileID.ToString() },
                { "BoardID", boardID.ToString() } });
        }

        public void SetUserId(string userId)
        {
            this.telemetryClient.Context.User.Id = userId;

            string playFabUserID = HashHelper.ComputeMD5Hash("Mixer-" + (ChannelSession.IsStreamer ? "Streamer" : "Moderator") + ChannelSession.User.id);
            this.TrySendPlayFabTelemetry(PlayFabClientAPI.LoginWithCustomIDAsync(new LoginWithCustomIDRequest { CustomId = playFabUserID, CreateAccount = true }));
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
                    Mixer.Base.Util.Logger.Log(ex);
                }
            });
        }
    }
}
