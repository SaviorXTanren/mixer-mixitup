using MixItUp.Base.Model;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Twitch;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using MixItUp.SignalR.Client;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Services;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IWebhookService
    {
        Task<Result> Connect();
        Task Disconnect();

        Task Authenticate(string twitchAccessToken);
    }

    public class WebhookService : OAuthRestServiceBase, IWebhookService
    {
        public const string AuthenticateMethodName = "Authenticate";

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
                var _ = this.TwitchFollowEvent(followerId, followerUsername, followerDisplayName);
            });

            this.signalRConnection.Listen("TwitchStreamStartedEvent", () =>
            {
                var _ = this.TwitchStreamStartedEvent();
            });

            this.signalRConnection.Listen("TwitchStreamStoppedEvent", () =>
            {
                var _ = this.TwitchStreamStoppedEvent();
            });

            this.signalRConnection.Listen("TwitchChannelHypeTrainBegin", (int totalPoints, int levelPoints, int levelGoal) =>
            {
                var _ = this.TwitchChannelHypeTrainBegin(totalPoints, levelPoints, levelGoal);
            });

            //this.signalRConnection.Listen("TwitchChannelHypeTrainProgress", (int level, int totalPoints, int levelPoints, int levelGoal) =>
            //{
            //    var _ = this.TwitchChannelHypeTrainProgress(level, totalPoints, levelPoints, levelGoal);
            //});

            this.signalRConnection.Listen("TwitchChannelHypeTrainEnd", (int level, int totalPoints) =>
            {
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

        public async Task<Result> Connect()
        {
            if (!this.IsConnected)
            {
                this.signalRConnection.Connected += SignalRConnection_Connected;
                this.signalRConnection.Disconnected += SignalRConnection_Disconnected;

                await this.signalRConnection.Connect();

                return new Result(this.IsConnected);
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
            ChannelSession.ReconnectionOccurred("Webhook Events");

            var twitchUserOAuthToken = ServiceManager.Get<TwitchSessionService>().UserConnection.GetOAuthTokenCopy();
            await this.Authenticate(twitchUserOAuthToken?.accessToken);
        }

        private void SignalRConnection_Disconnected(object sender, Exception e)
        {
            ChannelSession.DisconnectionOccurred("Webhook Events");
        }

        public async Task Authenticate(string twitchAccessToken)
        {
            await this.AsyncWrapper(this.signalRConnection.Send(AuthenticateMethodName, twitchAccessToken));
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

            EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelFollowed, user);
            if (ServiceManager.Get<EventService>().CanPerformEvent(trigger))
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

                await ServiceManager.Get<EventService>().PerformEvent(trigger);

                GlobalEvents.FollowOccurred(user);

                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Twitch, user, string.Format("{0} Followed", user.FullDisplayName), ChannelSession.Settings.AlertFollowColor));
            }
        }

        private async Task TwitchStreamStartedEvent()
        {
            EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelStreamStart, ChannelSession.GetCurrentUser());
            if (ServiceManager.Get<EventService>().CanPerformEvent(trigger))
            {
                await ServiceManager.Get<EventService>().PerformEvent(trigger);
            }
        }

        private async Task TwitchStreamStoppedEvent()
        {
            EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelStreamStop, ChannelSession.GetCurrentUser());
            if (ServiceManager.Get<EventService>().CanPerformEvent(trigger))
            {
                await ServiceManager.Get<EventService>().PerformEvent(trigger);
            }
        }

        private async Task TwitchChannelHypeTrainBegin(int totalPoints, int levelPoints, int levelGoal)
        {
            Dictionary<string, string> eventCommandSpecialIdentifiers = new Dictionary<string, string>();
            eventCommandSpecialIdentifiers["hypetraintotalpoints"] = totalPoints.ToString();
            eventCommandSpecialIdentifiers["hypetrainlevelpoints"] = levelPoints.ToString();
            eventCommandSpecialIdentifiers["hypetrainlevelgoal"] = levelGoal.ToString();

            EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelHypeTrainBegin, ChannelSession.GetCurrentUser(), eventCommandSpecialIdentifiers);
            if (ServiceManager.Get<EventService>().CanPerformEvent(trigger))
            {
                await ServiceManager.Get<EventService>().PerformEvent(trigger);
            }
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

            EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelHypeTrainEnd, ChannelSession.GetCurrentUser(), eventCommandSpecialIdentifiers);
            if (ServiceManager.Get<EventService>().CanPerformEvent(trigger))
            {
                await ServiceManager.Get<EventService>().PerformEvent(trigger);
            }
        }
    }
}
