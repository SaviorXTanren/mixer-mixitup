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
using Newtonsoft.Json.Linq;
using System;
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
    public class OverlayLabelWidgetV3Model : OverlayWidgetV3ModelBase
    {
        public const string UsernameReplacementKey = "Username";
        public const string AmountReplacementKey = "Amount";

        public static readonly string DefaultAmountHTML = Resources.OverlayLabelAmountDefaultHTML;
        public static readonly string DefaultUsernameAmountHTML = Resources.OverlayLabelUsernameAmountDefaultHTML;
        public static readonly string DefaultCSS = Resources.OverlayTextDefaultCSS;
        public static readonly string DefaultJavascript = string.Empty;

        [DataMember]
        public OverlayLabelWidgetV3Type LabelType { get; set; }

        [DataMember]
        public OverlayTextItemV3Model TextItem { get; set; }

        private CancellationTokenSource refreshCancellationTokenSource;

        private long trackingAmount = 0;

        public OverlayLabelWidgetV3Model(string name, Guid overlayEndpointID, OverlayTextItemV3Model item)
            : base(name, overlayEndpointID, item)
        {
            this.TextItem = item;
        }

        public override async Task<OverlayOutputV3Model> GetProcessedItem(OverlayEndpointService overlayEndpointService, CommandParametersModel parameters)
        {
            return await this.Item.GetProcessedItem(overlayEndpointService, parameters, this.CurrentReplacements);
        }

        protected override async Task EnableInternal()
        {
            if (!this.CurrentReplacements.ContainsKey(UsernameReplacementKey))
            {
                this.CurrentReplacements[UsernameReplacementKey] = String.Empty;
            }

            if (!this.CurrentReplacements.ContainsKey(AmountReplacementKey))
            {
                this.CurrentReplacements[AmountReplacementKey] = String.Empty;
            }

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
                        string old = this.TextItem.Text;

                        if (this.LabelType == OverlayLabelWidgetV3Type.Viewers)
                        {
                            this.CurrentReplacements[AmountReplacementKey] = ServiceManager.Get<ChatService>().GetViewerCount().ToString();
                        }
                        else if (this.LabelType == OverlayLabelWidgetV3Type.Chatters)
                        {
                            this.CurrentReplacements[AmountReplacementKey] = ServiceManager.Get<UserService>().ActiveUserCount.ToString();
                        }

                        if (!string.Equals(old, this.TextItem.Text))
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

        protected override async Task DisableInternal()
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

        private async void EventService_OnFollowOccurred(object sender, UserV2ViewModel user)
        {
            if (this.LabelType == OverlayLabelWidgetV3Type.LastestFollower)
            {
                this.CurrentReplacements[UsernameReplacementKey] = user.DisplayName;
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.TotalFollowers)
            {
                this.trackingAmount++;
                this.CurrentReplacements[AmountReplacementKey] = this.trackingAmount.ToString();
            }
            await this.Update();
        }

        private async void EventService_OnRaidOccurred(object sender, Tuple<UserV2ViewModel, int> raid)
        {
            this.CurrentReplacements[UsernameReplacementKey] = raid.Item1.DisplayName;
            this.CurrentReplacements[AmountReplacementKey] = raid.Item2.ToString();
            await this.Update();
        }

        private async void EventService_OnSubscribeOccurred(object sender, UserV2ViewModel user)
        {
            if (this.LabelType == OverlayLabelWidgetV3Type.LatestSubscriber)
            {
                this.CurrentReplacements[UsernameReplacementKey] = user.DisplayName;
                this.CurrentReplacements[AmountReplacementKey] = 1.ToString();
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.TotalSubscribers)
            {
                this.trackingAmount++;
                this.CurrentReplacements[AmountReplacementKey] = this.trackingAmount.ToString();
            }
            await this.Update();
        }

        private async void EventService_OnResubscribeOccurred(object sender, Tuple<UserV2ViewModel, int> resubscribe)
        {
            if (this.LabelType == OverlayLabelWidgetV3Type.LatestSubscriber)
            {
                this.CurrentReplacements[UsernameReplacementKey] = resubscribe.Item1.DisplayName;
                this.CurrentReplacements[AmountReplacementKey] = resubscribe.Item2.ToString();
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.TotalSubscribers)
            {
                this.trackingAmount++;
                this.CurrentReplacements[AmountReplacementKey] = this.trackingAmount.ToString();
            }
            await this.Update();
        }

        private async void EventService_OnSubscriptionGiftedOccurred(object sender, Tuple<UserV2ViewModel, UserV2ViewModel> subscriptionGifted)
        {
            if (this.LabelType == OverlayLabelWidgetV3Type.LatestSubscriber)
            {
                this.CurrentReplacements[AmountReplacementKey] = subscriptionGifted.Item2.DisplayName;
                this.CurrentReplacements[AmountReplacementKey] = 1.ToString();
            }
            else if (this.LabelType == OverlayLabelWidgetV3Type.TotalSubscribers)
            {
                this.trackingAmount++;
                this.CurrentReplacements[AmountReplacementKey] = this.trackingAmount.ToString();
            }
            await this.Update();
        }

        private async void EventService_OnMassSubscriptionsGiftedOccurred(object sender, Tuple<UserV2ViewModel, int> massSubscriptionsGifted)
        {
            this.trackingAmount += massSubscriptionsGifted.Item2;
            this.CurrentReplacements[AmountReplacementKey] = this.trackingAmount.ToString();
            await this.Update();
        }

        private async void EventService_OnDonationOccurred(object sender, UserDonationModel donation)
        {
            this.CurrentReplacements[UsernameReplacementKey] = donation.User.DisplayName;
            this.CurrentReplacements[AmountReplacementKey] = donation.AmountText;
            await this.Update();
        }

        private async void EventService_OnTwitchBitsCheeredOccurred(object sender, TwitchUserBitsCheeredModel bitsCheered)
        {
            this.CurrentReplacements[UsernameReplacementKey] = bitsCheered.User.DisplayName;
            this.CurrentReplacements[AmountReplacementKey] = bitsCheered.Amount.ToString();
            await this.Update();
        }

        private async void EventService_OnTrovoSpellCastOccurred(object sender, TrovoChatSpellViewModel spell)
        {
            if (spell.IsElixir)
            {
                this.CurrentReplacements[UsernameReplacementKey] = spell.User.DisplayName;
                this.CurrentReplacements[AmountReplacementKey] = spell.ValueTotal.ToString();
                await this.Update();
            }
        }

        private async void CounterModel_OnCounterUpdated(object sender, CounterModel counter)
        {
            this.CurrentReplacements[AmountReplacementKey] = counter.Amount.ToString();
            await this.Update();
        }

        private async Task Update()
        {
            JObject jobj = new JObject();
            jobj[UsernameReplacementKey] = this.CurrentReplacements[UsernameReplacementKey];
            jobj[AmountReplacementKey] = this.CurrentReplacements[AmountReplacementKey];
            await this.Update("UpdateLabel", jobj);
        }
    }
}
