using MixItUp.Base.Model.Settings;
using MixItUp.Base.Services.Trovo;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.Chat.Trovo;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System;
using System.Linq;

namespace MixItUp.Base.Model.Overlay.Widgets
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
    public class OverlayLabelWidgetV3Model : OverlayWidgetV3ModelBase
    {
        [DataMember]
        public OverlayLabelV3Type LabelType { get; set; }

        [DataMember]
        public string CounterName { get; set; }

        public override bool IsTestable { get { return false; } }

        private CancellationTokenSource refreshCancellationTokenSource;

        private OverlayLabelV3Model model;

        public OverlayLabelWidgetV3Model(OverlayLabelV3Type labelType, OverlayLabelV3Model model)
            : base(model)
        {
            this.LabelType = labelType;
        }

        [Obsolete]
        public OverlayLabelWidgetV3Model() { }

        protected override async Task EnableInternal()
        {
            await base.EnableInternal();

            this.model = base.Item as OverlayLabelV3Model;

            this.model.Username = Resources.Pending;
            this.model.Amount = 0;

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
                        double old = this.model.Amount;

                        if (this.LabelType == OverlayLabelV3Type.ViewerCount)
                        {
                            this.model.Amount = ServiceManager.Get<ChatService>().GetViewerCount();
                        }
                        else if (this.LabelType == OverlayLabelV3Type.ChatterCount)
                        {
                            this.model.Amount = ServiceManager.Get<UserService>().ActiveUserCount;
                        }

                        if (old != this.model.Amount)
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
                        this.model.Amount = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetFollowerCount(ServiceManager.Get<TwitchSessionService>().User);
                    }
                    else if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSessionService>().IsConnected)
                    {
                        this.model.Amount = (await ServiceManager.Get<TrovoSessionService>().UserConnection.GetFollowers(ServiceManager.Get<TwitchSessionService>().ChannelID, int.MaxValue)).Count();
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
                        this.model.Amount = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetSubscriberCount(ServiceManager.Get<TwitchSessionService>().User);
                    }
                    else if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSessionService>().IsConnected)
                    {
                        this.model.Amount = (await ServiceManager.Get<TrovoSessionService>().UserConnection.GetSubscribers(ServiceManager.Get<TwitchSessionService>().ChannelID, int.MaxValue)).Count();
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
                    this.model.Amount = counter.Amount;
                }
            }
        }

        protected override async Task DisableInternal()
        {
            await base.DisableInternal();

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
        }

        protected override Task TestInternal(CommandParametersModel parameters) { return Task.CompletedTask; }

        private async void EventService_OnFollowOccurred(object sender, UserV2ViewModel user)
        {
            if (this.LabelType == OverlayLabelV3Type.LastestFollower)
            {
                this.model.Username = user.DisplayName;
            }
            else if (this.LabelType == OverlayLabelV3Type.TotalFollowers)
            {
                this.model.Amount++;
            }
            await this.Update();
        }

        private async void EventService_OnRaidOccurred(object sender, Tuple<UserV2ViewModel, int> raid)
        {
            this.model.Username = raid.Item1.DisplayName;
            this.model.Amount = raid.Item2;
            await this.Update();
        }

        private async void EventService_OnSubscribeOccurred(object sender, UserV2ViewModel user)
        {
            if (this.LabelType == OverlayLabelV3Type.LatestSubscriber)
            {
                this.model.Username = user.DisplayName;
                this.model.Amount = 1;
            }
            else if (this.LabelType == OverlayLabelV3Type.TotalSubscribers)
            {
                this.model.Amount++;
            }
            await this.Update();
        }

        private async void EventService_OnResubscribeOccurred(object sender, Tuple<UserV2ViewModel, int> resubscribe)
        {
            if (this.LabelType == OverlayLabelV3Type.LatestSubscriber)
            {
                this.model.Username = resubscribe.Item1.DisplayName;
                this.model.Amount = resubscribe.Item2;
            }
            else if (this.LabelType == OverlayLabelV3Type.TotalSubscribers)
            {
                this.model.Amount++;
            }
            await this.Update();
        }

        private async void EventService_OnSubscriptionGiftedOccurred(object sender, Tuple<UserV2ViewModel, UserV2ViewModel> subscriptionGifted)
        {
            if (this.LabelType == OverlayLabelV3Type.LatestSubscriber)
            {
                this.model.Username = subscriptionGifted.Item2.DisplayName;
                this.model.Amount = 1;
            }
            else if (this.LabelType == OverlayLabelV3Type.TotalSubscribers)
            {
                this.model.Amount++;
            }
            await this.Update();
        }

        private async void EventService_OnMassSubscriptionsGiftedOccurred(object sender, Tuple<UserV2ViewModel, int> massSubscriptionsGifted)
        {
            this.model.Amount += massSubscriptionsGifted.Item2;
            await this.Update();
        }

        private async void EventService_OnDonationOccurred(object sender, UserDonationModel donation)
        {
            this.model.Username = donation.User.DisplayName;
            this.model.Amount = donation.Amount;
            await this.Update();
        }

        private async void EventService_OnTwitchBitsCheeredOccurred(object sender, TwitchUserBitsCheeredModel bitsCheered)
        {
            this.model.Username = bitsCheered.User.DisplayName;
            this.model.Amount = bitsCheered.Amount;
            await this.Update();
        }

        private async void EventService_OnTrovoSpellCastOccurred(object sender, TrovoChatSpellViewModel spell)
        {
            if (spell.IsElixir)
            {
                this.model.Username = spell.User.DisplayName;
                this.model.Amount = spell.ValueTotal;
                await this.Update();
            }
        }

        private async void CounterModel_OnCounterUpdated(object sender, CounterModel counter)
        {
            if (string.Equals(counter.Name, this.CounterName, StringComparison.OrdinalIgnoreCase))
            {
                this.model.Amount = counter.Amount;
                await this.Update();
            }
        }

        private async Task Update()
        {
            await this.CallFunction("update",
                new Dictionary<string, string>()
                {
                    { nameof(this.model.Username), this.model.Username },
                    { nameof(this.model.Amount), this.model.Amount.ToString() },
                },
                new CommandParametersModel());
        }
    }
}
