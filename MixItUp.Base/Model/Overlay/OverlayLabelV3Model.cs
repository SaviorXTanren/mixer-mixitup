using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Trovo;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat.Trovo;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayLabelV3Type
    {
        ViewerCount,
        ChatterCount,
        LastestFollower,
        TotalFollowers,
        LatestRaid,
        LatestSubscriber,
        TotalSubscribers,
        LatestDonation,
        LatestTwitchBits,
        LatestTrovoElixir,

        Counter = 100,
    }

    [DataContract]
    public class OverlayLabelV3Model : OverlayVisualTextV3ModelBase
    {
        public static readonly string DefaultAmountHTML = Resources.OverlayLabelAmountDefaultHTML;
        public static readonly string DefaultUsernameHTML = Resources.OverlayLabelUsernameDefaultHTML;
        public static readonly string DefaultUsernameAmountHTML = Resources.OverlayLabelUsernameAmountDefaultHTML;
        public static readonly string DefaultCSS = Resources.OverlayTextDefaultCSS;
        public static readonly string DefaultJavascript = Resources.OverlayLabelDefaultJavascript;

        [DataMember]
        public OverlayLabelV3Type LabelType { get; set; }

        [DataMember]
        public string CounterName { get; set; }

        [DataMember]
        public string Username { get;set; }
        [DataMember]
        public double Amount { get; set; }

        private CancellationTokenSource refreshCancellationTokenSource;

        public OverlayLabelV3Model(OverlayLabelV3Type labelType)
            : base(OverlayItemV3Type.Label)
        {
            this.LabelType = labelType;
        }

        [Obsolete]
        private OverlayLabelV3Model() : base(OverlayItemV3Type.Label) { }

        public override async Task Enable()
        {
            this.Username = Resources.Pending;
            this.Amount = 0;

            if (this.LabelType == OverlayLabelV3Type.ViewerCount || this.LabelType == OverlayLabelV3Type.ChatterCount)
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
                        double old = this.Amount;

                        if (this.LabelType == OverlayLabelV3Type.ViewerCount)
                        {
                            this.Amount = ServiceManager.Get<ChatService>().GetViewerCount();
                        }
                        else if (this.LabelType == OverlayLabelV3Type.ChatterCount)
                        {
                            this.Amount = ServiceManager.Get<UserService>().ActiveUserCount;
                        }

                        if (old != this.Amount)
                        {
                            await this.Update();
                        }

                        await Task.Delay(1000);

                    } while (!cancellationToken.IsCancellationRequested);

                }, this.refreshCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            else if (this.LabelType == OverlayLabelV3Type.LastestFollower || this.LabelType == OverlayLabelV3Type.TotalFollowers)
            {
                EventService.OnFollowOccurred += EventService_OnFollowOccurred;
                if (this.LabelType == OverlayLabelV3Type.TotalFollowers)
                {
                    if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSessionService>().IsConnected)
                    {
                        this.Amount = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetFollowerCount(ServiceManager.Get<TwitchSessionService>().User);
                    }
                    else if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSessionService>().IsConnected)
                    {
                        this.Amount = (await ServiceManager.Get<TrovoSessionService>().UserConnection.GetFollowers(ServiceManager.Get<TwitchSessionService>().ChannelID, int.MaxValue)).Count();
                    }
                }
            }
            else if (this.LabelType == OverlayLabelV3Type.LatestRaid)
            {
                EventService.OnRaidOccurred += EventService_OnRaidOccurred;
            }
            else if (this.LabelType == OverlayLabelV3Type.LatestSubscriber || this.LabelType == OverlayLabelV3Type.TotalSubscribers)
            {
                EventService.OnSubscribeOccurred += EventService_OnSubscribeOccurred;
                EventService.OnResubscribeOccurred += EventService_OnResubscribeOccurred;
                EventService.OnSubscriptionGiftedOccurred += EventService_OnSubscriptionGiftedOccurred;
                if (this.LabelType == OverlayLabelV3Type.TotalSubscribers)
                {
                    EventService.OnMassSubscriptionsGiftedOccurred += EventService_OnMassSubscriptionsGiftedOccurred;
                    if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSessionService>().IsConnected)
                    {
                        this.Amount = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetSubscriberCount(ServiceManager.Get<TwitchSessionService>().User);
                    }
                    else if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSessionService>().IsConnected)
                    {
                        this.Amount = (await ServiceManager.Get<TrovoSessionService>().UserConnection.GetSubscribers(ServiceManager.Get<TwitchSessionService>().ChannelID, int.MaxValue)).Count();
                    }
                }
            }
            else if (this.LabelType == OverlayLabelV3Type.LatestDonation)
            {
                EventService.OnDonationOccurred += EventService_OnDonationOccurred;
            }
            else if (this.LabelType == OverlayLabelV3Type.LatestTwitchBits)
            {
                EventService.OnTwitchBitsCheeredOccurred += EventService_OnTwitchBitsCheeredOccurred;
            }
            else if (this.LabelType == OverlayLabelV3Type.LatestTrovoElixir)
            {
                EventService.OnTrovoSpellCastOccurred += EventService_OnTrovoSpellCastOccurred;
            }
            else if (this.LabelType == OverlayLabelV3Type.Counter)
            {
                CounterModel.OnCounterUpdated += CounterModel_OnCounterUpdated;
                if (ChannelSession.Settings.Counters.TryGetValue(this.CounterName, out CounterModel counter))
                {
                    this.Amount = counter.Amount;
                }
            }

            await base.Enable();
        }

        public override async Task Disable()
        {
            if (this.refreshCancellationTokenSource != null)
            {
                this.refreshCancellationTokenSource.Cancel();
            }
            this.refreshCancellationTokenSource = null;

            EventService.OnFollowOccurred -= EventService_OnFollowOccurred;
            EventService.OnRaidOccurred -= EventService_OnRaidOccurred;
            EventService.OnSubscribeOccurred -= EventService_OnSubscribeOccurred;
            EventService.OnResubscribeOccurred -= EventService_OnResubscribeOccurred;
            EventService.OnSubscriptionGiftedOccurred -= EventService_OnSubscriptionGiftedOccurred;
            EventService.OnMassSubscriptionsGiftedOccurred -= EventService_OnMassSubscriptionsGiftedOccurred;
            EventService.OnDonationOccurred -= EventService_OnDonationOccurred;
            EventService.OnTwitchBitsCheeredOccurred -= EventService_OnTwitchBitsCheeredOccurred;
            EventService.OnTrovoSpellCastOccurred -= EventService_OnTrovoSpellCastOccurred;
            CounterModel.OnCounterUpdated -= CounterModel_OnCounterUpdated;

            await base.Disable();
        }

        protected override async Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, CommandParametersModel parameters)
        {
            OverlayOutputV3Model output = await base.GetProcessedItem(item, parameters);

            item.HTML = ReplaceProperty(item.HTML, nameof(this.Username), this.Username);
            item.CSS = ReplaceProperty(item.CSS, nameof(this.Username), this.Username);
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.Username), this.Username);

            item.HTML = ReplaceProperty(item.HTML, nameof(this.Amount), this.Amount.ToString());
            item.CSS = ReplaceProperty(item.CSS, nameof(this.Amount), this.Amount.ToString());
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.Amount), this.Amount.ToString());

            return output;
        }

        private async void EventService_OnFollowOccurred(object sender, UserV2ViewModel user)
        {
            if (this.LabelType == OverlayLabelV3Type.LastestFollower)
            {
                this.Username = user.DisplayName;
            }
            else if (this.LabelType == OverlayLabelV3Type.TotalFollowers)
            {
                this.Amount++;
            }
            await this.Update();
        }

        private async void EventService_OnRaidOccurred(object sender, Tuple<UserV2ViewModel, int> raid)
        {
            this.Username = raid.Item1.DisplayName;
            this.Amount = raid.Item2;
            await this.Update();
        }

        private async void EventService_OnSubscribeOccurred(object sender, UserV2ViewModel user)
        {
            if (this.LabelType == OverlayLabelV3Type.LatestSubscriber)
            {
                this.Username = user.DisplayName;
                this.Amount = 1;
            }
            else if (this.LabelType == OverlayLabelV3Type.TotalSubscribers)
            {
                this.Amount++;
            }
            await this.Update();
        }

        private async void EventService_OnResubscribeOccurred(object sender, Tuple<UserV2ViewModel, int> resubscribe)
        {
            if (this.LabelType == OverlayLabelV3Type.LatestSubscriber)
            {
                this.Username = resubscribe.Item1.DisplayName;
                this.Amount = resubscribe.Item2;
            }
            else if (this.LabelType == OverlayLabelV3Type.TotalSubscribers)
            {
                this.Amount++;
            }
            await this.Update();
        }

        private async void EventService_OnSubscriptionGiftedOccurred(object sender, Tuple<UserV2ViewModel, UserV2ViewModel> subscriptionGifted)
        {
            if (this.LabelType == OverlayLabelV3Type.LatestSubscriber)
            {
                this.Username = subscriptionGifted.Item2.DisplayName;
                this.Amount = 1;
            }
            else if (this.LabelType == OverlayLabelV3Type.TotalSubscribers)
            {
                this.Amount++;
            }
            await this.Update();
        }

        private async void EventService_OnMassSubscriptionsGiftedOccurred(object sender, Tuple<UserV2ViewModel, int> massSubscriptionsGifted)
        {
            this.Amount += massSubscriptionsGifted.Item2;
            await this.Update();
        }

        private async void EventService_OnDonationOccurred(object sender, UserDonationModel donation)
        {
            this.Username = donation.User.DisplayName;
            this.Amount = donation.Amount;
            await this.Update();
        }

        private async void EventService_OnTwitchBitsCheeredOccurred(object sender, TwitchUserBitsCheeredModel bitsCheered)
        {
            this.Username = bitsCheered.User.DisplayName;
            this.Amount = bitsCheered.Amount;
            await this.Update();
        }

        private async void EventService_OnTrovoSpellCastOccurred(object sender, TrovoChatSpellViewModel spell)
        {
            if (spell.IsElixir)
            {
                this.Username = spell.User.DisplayName;
                this.Amount = spell.ValueTotal;
                await this.Update();
            }
        }

        private async void CounterModel_OnCounterUpdated(object sender, CounterModel counter)
        {
            if (string.Equals(counter.Name, this.CounterName, StringComparison.OrdinalIgnoreCase))
            {
                this.Amount = counter.Amount;
                await this.Update();
            }
        }

        private async Task Update()
        {
            await this.Update("update",
                new Dictionary<string, string>()
                {
                    { nameof(this.Username), this.Username },
                    { nameof(this.Amount), this.Amount.ToString() },
                },
                new CommandParametersModel());
        }
    }
}
