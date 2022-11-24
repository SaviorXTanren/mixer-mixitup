using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Glimesh;
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
    public enum OverlayLabelWidgetV3Type
    {
        Viewers,
        Chatters,
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
        public const string NameReplacementKey = "Name";
        public const string AmountReplacementKey = "Amount";

        public static readonly string DefaultAmountHTML = Resources.OverlayLabelAmountDefaultHTML;
        public static readonly string DefaultNameHTML = Resources.OverlayLabelNameDefaultHTML;
        public static readonly string DefaultNameAmountHTML = Resources.OverlayLabelNameAmountDefaultHTML;
        public static readonly string DefaultCSS = Resources.OverlayTextDefaultCSS;
        public static readonly string DefaultJavascript = Resources.OverlayLabelDefaultJavascript;

        [DataMember]
        public OverlayLabelWidgetV3Type LabelType { get; set; }

        [DataMember]
        public string CounterName { get; set; }

        [DataMember]
        public string CurrentName { get; set; }
        [DataMember]
        public string CurrentAmount { get; set; }

        private CancellationTokenSource refreshCancellationTokenSource;

        private long trackingAmount = 0;

        public OverlayLabelV3Model(OverlayLabelWidgetV3Type labelType)
            : base(OverlayItemV3Type.Label)
        {
            this.LabelType = labelType;
        }

        [Obsolete]
        public OverlayLabelV3Model() : base(OverlayItemV3Type.Label) { }

        public override async Task Enable()
        {
            if (this.LabelType == OverlayLabelWidgetV3Type.Viewers || this.LabelType == OverlayLabelWidgetV3Type.Chatters)
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
                        string old = this.Text;

                        if (this.LabelType == OverlayLabelWidgetV3Type.Viewers)
                        {
                            this.CurrentAmount = ServiceManager.Get<ChatService>().GetViewerCount().ToString();
                        }
                        else if (this.LabelType == OverlayLabelWidgetV3Type.Chatters)
                        {
                            this.CurrentAmount = ServiceManager.Get<UserService>().ActiveUserCount.ToString();
                        }

                        if (!string.Equals(old, this.Text))
                        {
                            await this.Update();
                        }

                        await Task.Delay(1000);

                    } while (!cancellationToken.IsCancellationRequested);

                }, this.refreshCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.LastestFollower || this.LabelType == OverlayLabelWidgetV3Type.TotalFollowers)
            {
                EventService.OnFollowOccurred += EventService_OnFollowOccurred;
                if (this.LabelType == OverlayLabelWidgetV3Type.TotalFollowers)
                {
                    if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSessionService>().IsConnected)
                    {
                        this.trackingAmount = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetFollowerCount(ServiceManager.Get<TwitchSessionService>().User);
                    }
                    else if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSessionService>().IsConnected)
                    {
                        this.trackingAmount = (await ServiceManager.Get<TrovoSessionService>().UserConnection.GetFollowers(ServiceManager.Get<TwitchSessionService>().ChannelID, int.MaxValue)).Count();
                    }
                    else if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Glimesh && ServiceManager.Get<GlimeshSessionService>().IsConnected)
                    {
                        this.trackingAmount = (await ServiceManager.Get<GlimeshSessionService>().UserConnection.GetFollowingUsers(ServiceManager.Get<GlimeshSessionService>().User, int.MaxValue)).Count();
                    }

                    this.CurrentAmount = this.trackingAmount.ToString();
                }
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.LatestRaid)
            {
                EventService.OnRaidOccurred += EventService_OnRaidOccurred;
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.LatestSubscriber || this.LabelType == OverlayLabelWidgetV3Type.TotalSubscribers)
            {
                EventService.OnSubscribeOccurred += EventService_OnSubscribeOccurred;
                EventService.OnResubscribeOccurred += EventService_OnResubscribeOccurred;
                EventService.OnSubscriptionGiftedOccurred += EventService_OnSubscriptionGiftedOccurred;
                if (this.LabelType == OverlayLabelWidgetV3Type.TotalSubscribers)
                {
                    EventService.OnMassSubscriptionsGiftedOccurred += EventService_OnMassSubscriptionsGiftedOccurred;
                    if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSessionService>().IsConnected)
                    {
                        this.trackingAmount = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetSubscriberCount(ServiceManager.Get<TwitchSessionService>().User);
                    }
                    else if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSessionService>().IsConnected)
                    {
                        this.trackingAmount = (await ServiceManager.Get<TrovoSessionService>().UserConnection.GetSubscribers(ServiceManager.Get<TwitchSessionService>().ChannelID, int.MaxValue)).Count();
                    }
                    else if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Glimesh && ServiceManager.Get<GlimeshSessionService>().IsConnected)
                    {
                        this.trackingAmount = 0;
                    }

                    this.CurrentAmount = this.trackingAmount.ToString();
                }
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.LatestDonation)
            {
                EventService.OnDonationOccurred += EventService_OnDonationOccurred;
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.LatestTwitchBits)
            {
                EventService.OnTwitchBitsCheeredOccurred += EventService_OnTwitchBitsCheeredOccurred;
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.LatestTrovoElixir)
            {
                EventService.OnTrovoSpellCastOccurred += EventService_OnTrovoSpellCastOccurred;
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.Counter)
            {
                CounterModel.OnCounterUpdated += CounterModel_OnCounterUpdated;
            }

            await base.Enable();
        }

        public override async Task Disable()
        {
            if (this.LabelType == OverlayLabelWidgetV3Type.Viewers || this.LabelType == OverlayLabelWidgetV3Type.Chatters)
            {
                if (this.refreshCancellationTokenSource != null)
                {
                    this.refreshCancellationTokenSource.Cancel();
                }
                this.refreshCancellationTokenSource = null;
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.LastestFollower || this.LabelType == OverlayLabelWidgetV3Type.TotalFollowers)
            {
                EventService.OnFollowOccurred -= EventService_OnFollowOccurred;
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.LatestRaid)
            {
                EventService.OnRaidOccurred -= EventService_OnRaidOccurred;
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.LatestSubscriber || this.LabelType == OverlayLabelWidgetV3Type.TotalSubscribers)
            {
                EventService.OnSubscribeOccurred -= EventService_OnSubscribeOccurred;
                EventService.OnResubscribeOccurred -= EventService_OnResubscribeOccurred;
                EventService.OnSubscriptionGiftedOccurred -= EventService_OnSubscriptionGiftedOccurred;
                if (this.LabelType == OverlayLabelWidgetV3Type.TotalSubscribers)
                {
                    EventService.OnMassSubscriptionsGiftedOccurred -= EventService_OnMassSubscriptionsGiftedOccurred;
                }
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.LatestDonation)
            {
                EventService.OnDonationOccurred -= EventService_OnDonationOccurred;
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.LatestTwitchBits)
            {
                EventService.OnTwitchBitsCheeredOccurred -= EventService_OnTwitchBitsCheeredOccurred;
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.LatestTrovoElixir)
            {
                EventService.OnTrovoSpellCastOccurred -= EventService_OnTrovoSpellCastOccurred;
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.Counter)
            {
                CounterModel.OnCounterUpdated -= CounterModel_OnCounterUpdated;
            }

            await base.Disable();
        }

        protected override async Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, OverlayEndpointService overlayEndpointService, CommandParametersModel parameters)
        {
            item = await base.GetProcessedItem(item, overlayEndpointService, parameters);

            item.HTML = ReplaceProperty(item.HTML, "Name", this.CurrentName);
            item.CSS = ReplaceProperty(item.CSS, "Name", this.CurrentName);
            item.Javascript = ReplaceProperty(item.Javascript, "Name", this.CurrentName);

            item.HTML = ReplaceProperty(item.HTML, "Amount", this.CurrentAmount);
            item.CSS = ReplaceProperty(item.CSS, "Amount", this.CurrentAmount);
            item.Javascript = ReplaceProperty(item.Javascript, "Amount", this.CurrentAmount);

            return item;
        }

        private async void EventService_OnFollowOccurred(object sender, UserV2ViewModel user)
        {
            if (this.LabelType == OverlayLabelWidgetV3Type.LastestFollower)
            {
                this.CurrentName = user.DisplayName;
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.TotalFollowers)
            {
                this.trackingAmount++;
                this.CurrentAmount = this.trackingAmount.ToString();
            }
            await this.Update();
        }

        private async void EventService_OnRaidOccurred(object sender, Tuple<UserV2ViewModel, int> raid)
        {
            this.CurrentName = raid.Item1.DisplayName;
            this.CurrentAmount = raid.Item2.ToString();
            await this.Update();
        }

        private async void EventService_OnSubscribeOccurred(object sender, UserV2ViewModel user)
        {
            if (this.LabelType == OverlayLabelWidgetV3Type.LatestSubscriber)
            {
                this.CurrentName = user.DisplayName;
                this.CurrentAmount = 1.ToString();
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.TotalSubscribers)
            {
                this.trackingAmount++;
                this.CurrentAmount = this.trackingAmount.ToString();
            }
            await this.Update();
        }

        private async void EventService_OnResubscribeOccurred(object sender, Tuple<UserV2ViewModel, int> resubscribe)
        {
            if (this.LabelType == OverlayLabelWidgetV3Type.LatestSubscriber)
            {
                this.CurrentName = resubscribe.Item1.DisplayName;
                this.CurrentAmount = resubscribe.Item2.ToString();
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.TotalSubscribers)
            {
                this.trackingAmount++;
                this.CurrentAmount = this.trackingAmount.ToString();
            }
            await this.Update();
        }

        private async void EventService_OnSubscriptionGiftedOccurred(object sender, Tuple<UserV2ViewModel, UserV2ViewModel> subscriptionGifted)
        {
            if (this.LabelType == OverlayLabelWidgetV3Type.LatestSubscriber)
            {
                this.CurrentName = subscriptionGifted.Item2.DisplayName;
                this.CurrentAmount = 1.ToString();
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.TotalSubscribers)
            {
                this.trackingAmount++;
                this.CurrentAmount = this.trackingAmount.ToString();
            }
            await this.Update();
        }

        private async void EventService_OnMassSubscriptionsGiftedOccurred(object sender, Tuple<UserV2ViewModel, int> massSubscriptionsGifted)
        {
            this.trackingAmount += massSubscriptionsGifted.Item2;
            this.CurrentAmount = this.trackingAmount.ToString();
            await this.Update();
        }

        private async void EventService_OnDonationOccurred(object sender, UserDonationModel donation)
        {
            this.CurrentName = donation.User.DisplayName;
            this.CurrentAmount = donation.AmountText;
            await this.Update();
        }

        private async void EventService_OnTwitchBitsCheeredOccurred(object sender, TwitchUserBitsCheeredModel bitsCheered)
        {
            this.CurrentName = bitsCheered.User.DisplayName;
            this.CurrentAmount = bitsCheered.Amount.ToString();
            await this.Update();
        }

        private async void EventService_OnTrovoSpellCastOccurred(object sender, TrovoChatSpellViewModel spell)
        {
            if (spell.IsElixir)
            {
                this.CurrentName = spell.User.DisplayName;
                this.CurrentAmount = spell.ValueTotal.ToString();
                await this.Update();
            }
        }

        private async void CounterModel_OnCounterUpdated(object sender, CounterModel counter)
        {
            if (string.Equals(counter.Name, this.CounterName, StringComparison.OrdinalIgnoreCase))
            {
                this.CurrentAmount = counter.Amount.ToString();
                await this.Update();
            }
        }

        private async Task Update()
        {
            await this.Update(
                "LabelUpdate",
                new Dictionary<string, string>()
                {
                    { NameReplacementKey, this.CurrentName },
                    { AmountReplacementKey, this.CurrentAmount }
                },
                new CommandParametersModel());
        }
    }
}
