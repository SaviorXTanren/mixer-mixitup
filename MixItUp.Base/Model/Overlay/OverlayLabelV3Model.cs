using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Trovo;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat.Trovo;
using MixItUp.Base.ViewModel.Chat.YouTube;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayLabelDisplayV3TypeEnum
    {
        ViewerCount,
        ChatterCount,
        LatestFollower,
        TotalFollowers,
        LatestSubscriber,
        TotalSubscribers,
        LatestRaid,
        LatestDonation,
        LatestTwitchBits,
        LatestTrovoElixir,
        LatestYouTubeSuperChat,

        Counter = 100,
    }

    public enum OverlayLabelDisplayV3SettingTypeEnum
    {
        RotatingDisplays,
        NewestOnly,
    }

    [DataContract]
    public class OverlayLabelDisplayV3Model
    {
        [DataMember]
        public OverlayLabelDisplayV3TypeEnum Type { get; set; }

        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember]
        public string Format { get; set; }

        [DataMember]
        public Guid UserID { get; set; }
        [DataMember]
        public UserV2Model UserFallback { get; set; }

        [DataMember]
        public double Amount { get; set; }
        [DataMember]
        public string AmountText { get; set; }

        [DataMember]
        public string CounterName { get; set; }
    }

    [DataContract]
    public class OverlayLabelV3Model : OverlayVisualTextV3ModelBase
    {
        public const string LabelAdds = "LabelAdds";

        public static readonly string DefaultHTML = OverlayResources.OverlayLabelDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayTextDefaultCSS + "\n\n" + OverlayResources.OverlayLabelDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayLabelDefaultJavascript;

        [DataMember]
        public OverlayLabelDisplayV3SettingTypeEnum DisplaySetting { get; set; }

        [DataMember]
        public int DisplayRotationSeconds { get; set; }

        [DataMember]
        public Dictionary<OverlayLabelDisplayV3TypeEnum, OverlayLabelDisplayV3Model> Displays { get; set; } = new Dictionary<OverlayLabelDisplayV3TypeEnum, OverlayLabelDisplayV3Model>();

        private CancellationTokenSource refreshCancellationTokenSource;

        public OverlayLabelV3Model() : base(OverlayItemV3Type.Label) { }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();
            properties[nameof(this.DisplaySetting)] = this.DisplaySetting.ToString();
            properties[nameof(this.DisplayRotationSeconds)] = this.DisplayRotationSeconds;
            return properties;
        }

        public override async Task ProcessGenerationProperties(Dictionary<string, object> properties, CommandParametersModel parameters)
        {
            await base.ProcessGenerationProperties(properties, parameters);

            List<string> labelAdds = new List<string>();
            foreach (var display in this.Displays)
            {
                if (display.Value.IsEnabled)
                {
                    string labelAdd = OverlayResources.OverlayLabelAddJavascript;
                    foreach (var kvp in await this.GetLabelDisplayProperties(display.Value))
                    {
                        labelAdd = OverlayV3Service.ReplaceProperty(labelAdd, kvp.Key, kvp.Value);
                    }
                    labelAdds.Add(labelAdd);
                }
            }

            properties[LabelAdds] = string.Join("\n", labelAdds);
        }

        protected override async Task WidgetEnableInternal()
        {
            await base.WidgetEnableInternal();

            if (this.Displays[OverlayLabelDisplayV3TypeEnum.ViewerCount].IsEnabled || this.Displays[OverlayLabelDisplayV3TypeEnum.ChatterCount].IsEnabled)
            {
                if (this.refreshCancellationTokenSource != null)
                {
                    this.refreshCancellationTokenSource.Cancel();
                }
                this.refreshCancellationTokenSource = new CancellationTokenSource();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                {
                    do
                    {
                        if (this.Displays[OverlayLabelDisplayV3TypeEnum.ViewerCount].IsEnabled)
                        {
                            this.Displays[OverlayLabelDisplayV3TypeEnum.ViewerCount].Amount = ServiceManager.Get<ChatService>().GetViewerCount();
                            await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.ViewerCount);
                        }

                        if (this.Displays[OverlayLabelDisplayV3TypeEnum.ChatterCount].IsEnabled)
                        {
                            this.Displays[OverlayLabelDisplayV3TypeEnum.ChatterCount].Amount = ServiceManager.Get<UserService>().ActiveUserCount;
                            await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.ChatterCount);
                        }

                        await Task.Delay(60000);

                    } while (!cancellationToken.IsCancellationRequested);

                }, this.refreshCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            
            if (this.Displays[OverlayLabelDisplayV3TypeEnum.LatestFollower].IsEnabled || this.Displays[OverlayLabelDisplayV3TypeEnum.TotalFollowers].IsEnabled)
            {
                EventService.OnFollowOccurred += EventService_OnFollowOccurred;

                if (this.Displays[OverlayLabelDisplayV3TypeEnum.LatestFollower].IsEnabled)
                {
                    UserV2ViewModel user = null;
                    if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSessionService>().IsConnected)
                    {
                        var followers = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIFollowers(ServiceManager.Get<TwitchSessionService>().User, maxResult: 1);
                        if (followers != null)
                        {
                            user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: followers.First().user_id, performPlatformSearch: true);
                        }
                    }
                    else if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSessionService>().IsConnected)
                    {
                        var followers = await ServiceManager.Get<TrovoSessionService>().UserConnection.GetFollowers(ServiceManager.Get<TrovoSessionService>().ChannelID, maxResults: 1);
                        if (followers != null)
                        {
                            user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Trovo, platformID: followers.First().user_id, platformUsername: followers.First().nickname, performPlatformSearch: true);
                        }
                    }

                    if (user != null)
                    {
                        this.Displays[OverlayLabelDisplayV3TypeEnum.LatestFollower].UserID = user.ID;
                    }
                }

                if (this.Displays[OverlayLabelDisplayV3TypeEnum.TotalFollowers].IsEnabled)
                {
                    long amount = 0;
                    if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSessionService>().IsConnected)
                    {
                        amount = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetFollowerCount(ServiceManager.Get<TwitchSessionService>().User);
                    }
                    else if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSessionService>().IsConnected)
                    {
                        amount = (await ServiceManager.Get<TrovoSessionService>().UserConnection.GetFollowers(ServiceManager.Get<TrovoSessionService>().ChannelID, int.MaxValue)).Count();
                    }
                    this.Displays[OverlayLabelDisplayV3TypeEnum.TotalFollowers].Amount = amount;
                }
            }
            
            if (this.Displays[OverlayLabelDisplayV3TypeEnum.LatestRaid].IsEnabled)
            {
                EventService.OnRaidOccurred += EventService_OnRaidOccurred;
            }
            
            if (this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].IsEnabled || this.Displays[OverlayLabelDisplayV3TypeEnum.TotalSubscribers].IsEnabled)
            {
                EventService.OnSubscribeOccurred += EventService_OnSubscribeOccurred;
                EventService.OnResubscribeOccurred += EventService_OnResubscribeOccurred;
                EventService.OnSubscriptionGiftedOccurred += EventService_OnSubscriptionGiftedOccurred;

                if (this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].IsEnabled)
                {
                    UserV2ViewModel user = null;
                    if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSessionService>().IsConnected)
                    {
                        var subscribers = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetSubscribers(ServiceManager.Get<TwitchSessionService>().User, maxResult: 1);
                        if (subscribers != null)
                        {
                            user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: subscribers.First().user_id, performPlatformSearch: true);
                        }
                    }

                    if (user != null)
                    {
                        this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].UserID = user.ID;
                        this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].AmountText = " ";
                    }
                }

                if (this.Displays[OverlayLabelDisplayV3TypeEnum.TotalSubscribers].IsEnabled)
                {
                    EventService.OnMassSubscriptionsGiftedOccurred += EventService_OnMassSubscriptionsGiftedOccurred;

                    long amount = 0;
                    if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSessionService>().IsConnected)
                    {
                        amount = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetSubscriberCount(ServiceManager.Get<TwitchSessionService>().User);
                    }
                    else if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSessionService>().IsConnected)
                    {
                        amount = (await ServiceManager.Get<TrovoSessionService>().UserConnection.GetSubscribers(ServiceManager.Get<TrovoSessionService>().ChannelID, int.MaxValue)).Count();
                    }
                    this.Displays[OverlayLabelDisplayV3TypeEnum.TotalSubscribers].Amount = amount;
                }
            }
            
            if (this.Displays[OverlayLabelDisplayV3TypeEnum.LatestDonation].IsEnabled)
            {
                EventService.OnDonationOccurred += EventService_OnDonationOccurred;
            }
            
            if (this.Displays[OverlayLabelDisplayV3TypeEnum.LatestTwitchBits].IsEnabled)
            {
                EventService.OnTwitchBitsCheeredOccurred += EventService_OnTwitchBitsCheeredOccurred;
            }
            
            if (this.Displays[OverlayLabelDisplayV3TypeEnum.LatestTrovoElixir].IsEnabled)
            {
                EventService.OnTrovoSpellCastOccurred += EventService_OnTrovoSpellCastOccurred;
            }

            if (this.Displays[OverlayLabelDisplayV3TypeEnum.LatestYouTubeSuperChat].IsEnabled)
            {
                EventService.OnYouTubeSuperChatOccurred += EventService_OnYouTubeSuperChatOccurred;
            }
            
            if (this.Displays[OverlayLabelDisplayV3TypeEnum.Counter].IsEnabled)
            {
                CounterModel.OnCounterUpdated += CounterModel_OnCounterUpdated;
                if (!string.IsNullOrEmpty(this.Displays[OverlayLabelDisplayV3TypeEnum.Counter].CounterName) &&
                    ChannelSession.Settings.Counters.TryGetValue(this.Displays[OverlayLabelDisplayV3TypeEnum.Counter].CounterName, out CounterModel counter))
                {
                    this.Displays[OverlayLabelDisplayV3TypeEnum.Counter].Amount = counter.Amount;
                }
            }
        }

        protected override async Task WidgetDisableInternal()
        {
            await base.WidgetDisableInternal();

            EventService.OnFollowOccurred -= EventService_OnFollowOccurred;
            EventService.OnRaidOccurred -= EventService_OnRaidOccurred;
            EventService.OnSubscribeOccurred -= EventService_OnSubscribeOccurred;
            EventService.OnResubscribeOccurred -= EventService_OnResubscribeOccurred;
            EventService.OnSubscriptionGiftedOccurred -= EventService_OnSubscriptionGiftedOccurred;
            EventService.OnMassSubscriptionsGiftedOccurred -= EventService_OnMassSubscriptionsGiftedOccurred;
            EventService.OnDonationOccurred -= EventService_OnDonationOccurred;
            EventService.OnTwitchBitsCheeredOccurred -= EventService_OnTwitchBitsCheeredOccurred;
            EventService.OnTrovoSpellCastOccurred -= EventService_OnTrovoSpellCastOccurred;
            EventService.OnYouTubeSuperChatOccurred -= EventService_OnYouTubeSuperChatOccurred;
            CounterModel.OnCounterUpdated -= CounterModel_OnCounterUpdated;
        }

        private async void EventService_OnFollowOccurred(object sender, UserV2ViewModel user)
        {
            if (this.Displays[OverlayLabelDisplayV3TypeEnum.LatestFollower].IsEnabled)
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestFollower].UserID = user.ID;
                await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.LatestFollower);
            }

            if (this.Displays[OverlayLabelDisplayV3TypeEnum.TotalFollowers].IsEnabled)
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.TotalFollowers].Amount++;
                await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.TotalFollowers);
            }
        }

        private async void EventService_OnRaidOccurred(object sender, Tuple<UserV2ViewModel, int> raid)
        {
            this.Displays[OverlayLabelDisplayV3TypeEnum.LatestRaid].UserID = raid.Item1.ID;
            this.Displays[OverlayLabelDisplayV3TypeEnum.LatestRaid].Amount = raid.Item2;
            await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.LatestRaid);
        }

        private async void EventService_OnSubscribeOccurred(object sender, SubscriptionDetailsModel subscription)
        {
            if (this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].IsEnabled)
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].UserID = subscription.User.ID;
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].Amount = 1;
                await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.LatestSubscriber);
            }

            if (this.Displays[OverlayLabelDisplayV3TypeEnum.TotalSubscribers].IsEnabled)
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.TotalSubscribers].Amount++;
                await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.TotalSubscribers);
            }
        }

        private async void EventService_OnResubscribeOccurred(object sender, SubscriptionDetailsModel subscription)
        {
            if (this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].IsEnabled)
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].UserID = subscription.User.ID;
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].Amount = subscription.Months;
                await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.LatestSubscriber);
            }

            if (this.Displays[OverlayLabelDisplayV3TypeEnum.TotalSubscribers].IsEnabled)
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.TotalSubscribers].Amount++;
                await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.TotalSubscribers);
            }
        }

        private async void EventService_OnSubscriptionGiftedOccurred(object sender, SubscriptionDetailsModel subscription)
        {
            if (this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].IsEnabled)
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].UserID = subscription.User.ID;
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestSubscriber].Amount = 1;
                await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.LatestSubscriber);
            }

            if (this.Displays[OverlayLabelDisplayV3TypeEnum.TotalSubscribers].IsEnabled)
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.TotalSubscribers].Amount++;
                await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.TotalSubscribers);
            }
        }

        private async void EventService_OnMassSubscriptionsGiftedOccurred(object sender, IEnumerable<SubscriptionDetailsModel> subscriptions)
        {
            this.Displays[OverlayLabelDisplayV3TypeEnum.TotalSubscribers].Amount += subscriptions.Count();
            await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.TotalSubscribers);
        }

        private async void EventService_OnDonationOccurred(object sender, UserDonationModel donation)
        {
            this.Displays[OverlayLabelDisplayV3TypeEnum.LatestDonation].UserID = donation.User.ID;
            if (donation.IsAnonymous || donation.User.IsUnassociated)
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestDonation].UserFallback = donation.User.Model;
            }
            else
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestDonation].UserFallback = null;
            }
            this.Displays[OverlayLabelDisplayV3TypeEnum.LatestDonation].Amount = donation.Amount;
            await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.LatestDonation);
        }

        private async void EventService_OnTwitchBitsCheeredOccurred(object sender, TwitchUserBitsCheeredModel bitsCheered)
        {
            this.Displays[OverlayLabelDisplayV3TypeEnum.LatestTwitchBits].UserID = bitsCheered.User.ID;
            this.Displays[OverlayLabelDisplayV3TypeEnum.LatestTwitchBits].Amount = bitsCheered.Amount;
            await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.LatestTwitchBits);
        }

        private async void EventService_OnTrovoSpellCastOccurred(object sender, TrovoChatSpellViewModel spell)
        {
            if (spell.IsElixir)
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestTrovoElixir].UserID = spell.User.ID;
                this.Displays[OverlayLabelDisplayV3TypeEnum.LatestTrovoElixir].Amount = spell.ValueTotal;
                await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.LatestTrovoElixir);
            }
        }

        private async void EventService_OnYouTubeSuperChatOccurred(object sender, YouTubeSuperChatViewModel superChat)
        {
            this.Displays[OverlayLabelDisplayV3TypeEnum.LatestYouTubeSuperChat].UserID = superChat.User.ID;
            this.Displays[OverlayLabelDisplayV3TypeEnum.LatestYouTubeSuperChat].AmountText = superChat.AmountDisplay;
            await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.LatestYouTubeSuperChat);
        }

        private async void CounterModel_OnCounterUpdated(object sender, CounterModel counter)
        {
            if (string.Equals(counter.Name, this.Displays[OverlayLabelDisplayV3TypeEnum.Counter].CounterName, StringComparison.OrdinalIgnoreCase))
            {
                this.Displays[OverlayLabelDisplayV3TypeEnum.Counter].Amount = counter.Amount;
                await this.SendUpdate(OverlayLabelDisplayV3TypeEnum.Counter);
            }
        }

        private async Task SendUpdate(OverlayLabelDisplayV3TypeEnum type)
        {
            OverlayLabelDisplayV3Model display = this.Displays[type];
            if (display.IsEnabled)
            {
                Dictionary<string, object> data = await this.GetLabelDisplayProperties(display);
                await this.CallFunction("update", data);
            }
        }

        private async Task<Dictionary<string, object>> GetLabelDisplayProperties(OverlayLabelDisplayV3Model display)
        {
            UserV2ViewModel user = null;
            if (display.UserFallback != null)
            {
                user = new UserV2ViewModel(display.UserFallback);
            }
            else if (display.UserID != Guid.Empty)
            {
                user = await ServiceManager.Get<UserService>().GetUserByID(display.UserID);
            }

            string amount = display.Amount.ToString();
            if (!string.IsNullOrEmpty(display.AmountText))
            {
                amount = display.AmountText;
            }

            CommandParametersModel parameters = new CommandParametersModel(user: user);
            parameters.SpecialIdentifiers["labelamount"] = amount;

            string result = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(display.Format, parameters);

            Dictionary<string, object> data = new Dictionary<string, object>();
            data[nameof(display.Type)] = display.Type.ToString();
            data[nameof(display.Format)] = result;

            return data;
        }
    }
}
