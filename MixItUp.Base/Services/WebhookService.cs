using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Twitch;
using MixItUp.Base.Services.Glimesh;
using MixItUp.Base.Services.Trovo;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.YouTube;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using MixItUp.SignalR.Client;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Services;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public class AuthenticationMultiRequest
    {
        public string TwitchAccessToken { get; set; }
        public string GlimeshAccessToken { get; set; }
        public string TrovoAccessToken { get; set; }
        public string YouTubeAccessToken { get; set; }
    }

    public interface IWebhookService
    {
        Task<Result> Connect();
        Task Disconnect();

        Task Authenticate(string twitchAccessToken, string glimeshAccessToken, string trovoAccessToken, string youTubeAccessToken);
    }

    public class WebhookService : OAuthRestServiceBase, IWebhookService
    {
        public const string AuthenticateMethodName = "Authenticate";
        // public const string AuthenticateMultiMethodName = "AuthenticateMulti";

        private readonly string apiAddress;
        private readonly SignalRConnection signalRConnection;

        public bool IsConnected { get { return this.signalRConnection.IsConnected(); } }
        public bool IsAllowed { get; private set; } = false;


        public WebhookService(string apiAddress, string webhookHubAddress)
        {
            this.apiAddress = apiAddress;
            this.signalRConnection = new SignalRConnection(webhookHubAddress);

            this.signalRConnection.Listen("TwitchFollowEvent", (string followerId, string followerUsername, string followerDisplayName) =>
            {
                Logger.Log($"Webhook Event - Follow - {followerId} - {followerUsername}");
                var _ = this.TwitchFollowEvent(followerId, followerUsername, followerDisplayName);
            });

            this.signalRConnection.Listen("TwitchStreamStartedEvent", () =>
            {
                Logger.Log($"Webhook Event - Stream Start");
                var _ = this.TwitchStreamStartedEvent();
            });

            this.signalRConnection.Listen("TwitchStreamStoppedEvent", () =>
            {
                Logger.Log($"Webhook Event - Stream Stop");
                var _ = this.TwitchStreamStoppedEvent();
            });

            this.signalRConnection.Listen("TwitchChannelHypeTrainBegin", (int totalPoints, int levelPoints, int levelGoal) =>
            {
                Logger.Log($"Webhook Event - Hype Train Begin");
                var _ = this.TwitchChannelHypeTrainBegin(totalPoints, levelPoints, levelGoal);
            });

            //this.signalRConnection.Listen("TwitchChannelHypeTrainProgress", (int level, int totalPoints, int levelPoints, int levelGoal) =>
            //{
            //    var _ = this.TwitchChannelHypeTrainProgress(level, totalPoints, levelPoints, levelGoal);
            //});

            this.signalRConnection.Listen("TwitchChannelHypeTrainEnd", (int level, int totalPoints) =>
            {
                Logger.Log($"Webhook Event - Hype Train End - {level} - {totalPoints}");
                var _ = this.TwitchChannelHypeTrainEnd(level, totalPoints);
            });

            this.signalRConnection.Listen("AuthenticationCompleteEvent", (bool approved) =>
            {
                IsAllowed = approved;
                if (!IsAllowed)
                {
                    // Force disconnect is it doesn't retry
                    var _ = this.Disconnect();
                }
            });
        }

        public void BackgroundConnect()
        {
            AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
            {
                Result result = await this.Connect();
                if (!result.Success)
                {
                    SignalRConnection_Disconnected(this, new Exception());
                }
            }, new CancellationToken());
        }

        public async Task<Result> Connect()
        {
            if (!this.IsConnected)
            {
                this.signalRConnection.Connected -= SignalRConnection_Connected;
                this.signalRConnection.Disconnected -= SignalRConnection_Disconnected;

                this.signalRConnection.Connected += SignalRConnection_Connected;
                this.signalRConnection.Disconnected += SignalRConnection_Disconnected;

                if (await this.signalRConnection.Connect())
                {
                    return new Result(this.IsConnected);
                }
                return new Result(MixItUp.Base.Resources.WebhooksServiceFailedConnection);
            }
            return new Result(MixItUp.Base.Resources.WebhookServiceAlreadyConnected);
        }

        public async Task Disconnect()
        {
            this.signalRConnection.Connected -= SignalRConnection_Connected;
            this.signalRConnection.Disconnected -= SignalRConnection_Disconnected;

            await this.signalRConnection.Disconnect();
        }

        private async void SignalRConnection_Connected(object sender, EventArgs e)
        {
            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.WebhookEvents);

            var twitchUserOAuthToken = ServiceManager.Get<TwitchSessionService>()?.UserConnection?.Connection?.GetOAuthTokenCopy();
            var glimeshUserOAuthToken = ServiceManager.Get<GlimeshSessionService>()?.UserConnection?.Connection?.GetOAuthTokenCopy();
            var trovoUserOAuthToken = ServiceManager.Get<TrovoSessionService>()?.UserConnection?.Connection?.GetOAuthTokenCopy();
            var youTubeUserOAuthToken = ServiceManager.Get<YouTubeSessionService>()?.UserConnection?.Connection?.GetOAuthTokenCopy();

            await this.Authenticate(twitchUserOAuthToken?.accessToken, glimeshUserOAuthToken?.accessToken, trovoUserOAuthToken?.accessToken, youTubeUserOAuthToken?.accessToken);
        }

        private async void SignalRConnection_Disconnected(object sender, Exception e)
        {
            ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.WebhookEvents);

            Result result = new Result();
            do
            {
                await this.Disconnect();

                await Task.Delay(5000 + RandomHelper.GenerateRandomNumber(5000));

                result = await this.Connect();
            }
            while (!result.Success);
        }

        public async Task Authenticate(string twitchAccessToken, string glimeshAccessToken, string trovoAccessToken, string youTubeAccessToken)
        {
            await this.AsyncWrapper(this.signalRConnection.Send(AuthenticateMethodName, twitchAccessToken));
            //await this.AsyncWrapper(this.signalRConnection.Send(
            //    AuthenticateMultiMethodName,
            //    new AuthenticationMultiRequest
            //    {
            //        TwitchAccessToken = twitchAccessToken,
            //        GlimeshAccessToken = glimeshAccessToken,
            //        TrovoAccessToken = trovoAccessToken,
            //        YouTubeAccessToken = youTubeAccessToken
            //    }));
        }

        protected override string GetBaseAddress() { return this.apiAddress; }

        protected override Task<OAuthTokenModel> GetOAuthToken(bool autoRefreshToken = true) { return Task.FromResult<OAuthTokenModel>(new OAuthTokenModel()); }

        private async Task AsyncWrapper(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private async Task TwitchFollowEvent(string followerId, string followerUsername, string followerDisplayName)
        {
            UserViewModel user = ServiceManager.Get<UserService>().GetActiveUserByPlatformID(StreamingPlatformTypeEnum.Twitch, followerId);
            if (user == null)
            {
                user = await UserViewModel.Create(new TwitchWebhookFollowModel()
                {
                    StreamerID = ServiceManager.Get<TwitchSessionService>().UserNewAPI.id,

                    UserID = followerId,
                    Username = followerUsername,
                    UserDisplayName = followerDisplayName
                });
            }

            ServiceManager.Get<TwitchEventService>().FollowCache.Add(user.TwitchID);

            if (user.UserRoles.Contains(UserRoleEnum.Banned))
            {
                return;
            }

            CommandParametersModel parameters = new CommandParametersModel(user);
            if (ServiceManager.Get<EventService>().CanPerformEvent(EventTypeEnum.TwitchChannelFollowed, parameters))
            {
                user.FollowDate = DateTimeOffset.Now;

                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestFollowerUserData] = user.ID;

                foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                {
                    currency.AddAmount(user.Data, currency.OnFollowBonus);
                }

                foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                {
                    if (user.HasPermissionsTo(streamPass.Permission))
                    {
                        streamPass.AddAmount(user.Data, streamPass.FollowBonus);
                    }
                }

                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Twitch, user, string.Format("{0} Followed", user.FullDisplayName), ChannelSession.Settings.AlertFollowColor));

                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelFollowed, parameters);

                GlobalEvents.FollowOccurred(user);
            }
        }

        private async Task TwitchStreamStartedEvent()
        {
            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelStreamStart, new CommandParametersModel());
        }

        private async Task TwitchStreamStoppedEvent()
        {
            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelStreamStop, new CommandParametersModel());
        }

        private async Task TwitchChannelHypeTrainBegin(int totalPoints, int levelPoints, int levelGoal)
        {
            Dictionary<string, string> eventCommandSpecialIdentifiers = new Dictionary<string, string>();
            eventCommandSpecialIdentifiers["hypetraintotalpoints"] = totalPoints.ToString();
            eventCommandSpecialIdentifiers["hypetrainlevelpoints"] = levelPoints.ToString();
            eventCommandSpecialIdentifiers["hypetrainlevelgoal"] = levelGoal.ToString();
            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelHypeTrainBegin, new CommandParametersModel(ChannelSession.GetCurrentUser(), eventCommandSpecialIdentifiers));

            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Twitch, MixItUp.Base.Resources.HypeTrainStarted, ChannelSession.Settings.AlertHypeTrainColor));
        }

        //private async Task TwitchChannelHypeTrainProgress(int level, int totalPoints, int levelPoints, int levelGoal)
        //{
        //    Dictionary<string, string> eventCommandSpecialIdentifiers = new Dictionary<string, string>();
        //    eventCommandSpecialIdentifiers["hypetraintotallevel"] = level.ToString();
        //    eventCommandSpecialIdentifiers["hypetraintotalpoints"] = totalPoints.ToString();
        //    eventCommandSpecialIdentifiers["hypetrainlevelpoints"] = levelPoints.ToString();
        //    eventCommandSpecialIdentifiers["hypetrainlevelgoal"] = levelGoal.ToString();

        //    EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelHypeTrainProgress, ChannelSession.GetCurrentUser(), eventCommandSpecialIdentifiers);
        //    if (ServiceManager.Get<EventService>().CanPerformEvent(trigger))
        //    {
        //        await ServiceManager.Get<EventService>().PerformEvent(trigger);
        //    }
        //}

        private async Task TwitchChannelHypeTrainEnd(int level, int totalPoints)
        {
            Dictionary<string, string> eventCommandSpecialIdentifiers = new Dictionary<string, string>();
            eventCommandSpecialIdentifiers["hypetraintotallevel"] = level.ToString();
            eventCommandSpecialIdentifiers["hypetraintotalpoints"] = totalPoints.ToString();
            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelHypeTrainEnd, new CommandParametersModel(ChannelSession.GetCurrentUser(), eventCommandSpecialIdentifiers));

            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Twitch, string.Format(MixItUp.Base.Resources.HypeTrainEndedReachedLevel, level.ToString()), ChannelSession.Settings.AlertHypeTrainColor));
        }
    }
}
