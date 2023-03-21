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
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayGoalWidgetV3Type
    {
        Followers,
        Subscribers,
        Donations,
        TwitchBits,
        TrovoElixir,

        Counter = 100,
    }

    [DataContract]
    public class OverlayGoalV3Model : OverlayVisualTextV3ModelBase
    {
        public static readonly string DefaultHTML = Resources.OverlayGoalDefaultHTML;
        public static readonly string DefaultCSS = Resources.OverlayGoalDefaultCSS;
        public static readonly string DefaultJavascript = Resources.OverlayGoalDefaultJavascript;

        [DataMember]
        public OverlayGoalWidgetV3Type GoalType { get; set; }

        [DataMember]
        public string CounterName { get; set; }

        [DataMember]
        public string StartingAmount { get; set; }
        [DataMember]
        public string GoalAmount { get; set; }
        [DataMember]
        public string CurrentAmount { get; set; }

        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string ProgressColor { get; set; }

        [DataMember]
        private double trackingAmount;

        public OverlayGoalV3Model(OverlayGoalWidgetV3Type goalType)
            : base(OverlayItemV3Type.Goal)
        {
            this.GoalType = goalType;
        }

        [Obsolete]
        public OverlayGoalV3Model() : base(OverlayItemV3Type.Goal) { }

        public override async Task Enable()
        {
            if (this.GoalType == OverlayGoalWidgetV3Type.Followers)
            {
                EventService.OnFollowOccurred += EventService_OnFollowOccurred;
                if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSessionService>().IsConnected)
                {
                    this.CurrentAmount = (await ServiceManager.Get<TwitchSessionService>().UserConnection.GetFollowerCount(ServiceManager.Get<TwitchSessionService>().User)).ToString();
                }
                else if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSessionService>().IsConnected)
                {
                    this.CurrentAmount = (await ServiceManager.Get<TrovoSessionService>().UserConnection.GetFollowers(ServiceManager.Get<TwitchSessionService>().ChannelID, int.MaxValue)).Count().ToString();
                }
                else if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Glimesh && ServiceManager.Get<GlimeshSessionService>().IsConnected)
                {
                    this.CurrentAmount = (await ServiceManager.Get<GlimeshSessionService>().UserConnection.GetFollowingUsers(ServiceManager.Get<GlimeshSessionService>().User, int.MaxValue)).Count().ToString();
                }
            }
            else if (this.GoalType == OverlayGoalWidgetV3Type.Subscribers)
            {
                EventService.OnSubscribeOccurred += EventService_OnSubscribeOccurred;
                EventService.OnResubscribeOccurred += EventService_OnResubscribeOccurred;
                EventService.OnSubscriptionGiftedOccurred += EventService_OnSubscriptionGiftedOccurred;
                EventService.OnMassSubscriptionsGiftedOccurred += EventService_OnMassSubscriptionsGiftedOccurred;
                if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSessionService>().IsConnected)
                {
                    this.CurrentAmount = (await ServiceManager.Get<TwitchSessionService>().UserConnection.GetSubscriberCount(ServiceManager.Get<TwitchSessionService>().User)).ToString();
                }
                else if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSessionService>().IsConnected)
                {
                    this.CurrentAmount = (await ServiceManager.Get<TrovoSessionService>().UserConnection.GetSubscribers(ServiceManager.Get<TwitchSessionService>().ChannelID, int.MaxValue)).Count().ToString();
                }
                else if (ChannelSession.Settings.DefaultStreamingPlatform == StreamingPlatformTypeEnum.Glimesh && ServiceManager.Get<GlimeshSessionService>().IsConnected)
                {
                    this.CurrentAmount = "0";
                }
            }
            else if (this.GoalType == OverlayGoalWidgetV3Type.Donations)
            {
                EventService.OnDonationOccurred += EventService_OnDonationOccurred;
            }
            else if (this.GoalType == OverlayGoalWidgetV3Type.TwitchBits)
            {
                EventService.OnTwitchBitsCheeredOccurred += EventService_OnTwitchBitsCheeredOccurred;
            }
            else if (this.GoalType == OverlayGoalWidgetV3Type.TrovoElixir)
            {
                EventService.OnTrovoSpellCastOccurred += EventService_OnTrovoSpellCastOccurred;
            }
            else if (this.GoalType == OverlayGoalWidgetV3Type.Counter)
            {
                CounterModel.OnCounterUpdated += CounterModel_OnCounterUpdated;
            }

            await base.Enable();
        }

        public override async Task Disable()
        {
            if (this.GoalType == OverlayGoalWidgetV3Type.Followers)
            {
                EventService.OnFollowOccurred -= EventService_OnFollowOccurred;
            }
            else if (this.GoalType == OverlayGoalWidgetV3Type.Subscribers)
            {
                EventService.OnSubscribeOccurred -= EventService_OnSubscribeOccurred;
                EventService.OnResubscribeOccurred -= EventService_OnResubscribeOccurred;
                EventService.OnSubscriptionGiftedOccurred -= EventService_OnSubscriptionGiftedOccurred;
                EventService.OnMassSubscriptionsGiftedOccurred -= EventService_OnMassSubscriptionsGiftedOccurred;
            }
            else if (this.GoalType == OverlayGoalWidgetV3Type.Donations)
            {
                EventService.OnDonationOccurred -= EventService_OnDonationOccurred;
            }
            else if (this.GoalType == OverlayGoalWidgetV3Type.TwitchBits)
            {
                EventService.OnTwitchBitsCheeredOccurred -= EventService_OnTwitchBitsCheeredOccurred;
            }
            else if (this.GoalType == OverlayGoalWidgetV3Type.TrovoElixir)
            {
                EventService.OnTrovoSpellCastOccurred -= EventService_OnTrovoSpellCastOccurred;
            }
            else if (this.GoalType == OverlayGoalWidgetV3Type.Counter)
            {
                CounterModel.OnCounterUpdated -= CounterModel_OnCounterUpdated;
            }

            await base.Disable();
        }

        protected override async Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, CommandParametersModel parameters)
        {
            item = await base.GetProcessedItem(item, parameters);

            item.HTML = ReplaceProperty(item.HTML, nameof(this.CurrentAmount), this.CurrentAmount);
            item.CSS = ReplaceProperty(item.CSS, nameof(this.CurrentAmount), this.CurrentAmount);
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.CurrentAmount), this.CurrentAmount);

            item.HTML = ReplaceProperty(item.HTML, nameof(this.GoalAmount), this.GoalAmount);
            item.CSS = ReplaceProperty(item.CSS, nameof(this.GoalAmount), this.GoalAmount);
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.GoalAmount), this.GoalAmount);

            item.HTML = ReplaceProperty(item.HTML, nameof(this.BackgroundColor), this.BackgroundColor);
            item.CSS = ReplaceProperty(item.CSS, nameof(this.BackgroundColor), this.BackgroundColor);
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.BackgroundColor), this.BackgroundColor);

            item.HTML = ReplaceProperty(item.HTML, nameof(this.ProgressColor), this.ProgressColor);
            item.CSS = ReplaceProperty(item.CSS, nameof(this.ProgressColor), this.ProgressColor);
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.ProgressColor), this.ProgressColor);

            return item;
        }

        private async void EventService_OnFollowOccurred(object sender, UserV2ViewModel user)
        {
            this.trackingAmount++;
            this.CurrentAmount = this.trackingAmount.ToString();
            await this.Update();
        }

        private async void EventService_OnSubscribeOccurred(object sender, UserV2ViewModel user)
        {
            this.trackingAmount++;
            this.CurrentAmount = this.trackingAmount.ToString();
            await this.Update();
        }

        private async void EventService_OnResubscribeOccurred(object sender, Tuple<UserV2ViewModel, int> resubscribe)
        {
            this.trackingAmount++;
            this.CurrentAmount = this.trackingAmount.ToString();
            await this.Update();
        }

        private async void EventService_OnSubscriptionGiftedOccurred(object sender, Tuple<UserV2ViewModel, UserV2ViewModel> subscriptionGifted)
        {
            this.trackingAmount++;
            this.CurrentAmount = this.trackingAmount.ToString();
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
            this.trackingAmount += donation.Amount;
            this.CurrentAmount = this.trackingAmount.ToCurrencyString();
            await this.Update();
        }

        private async void EventService_OnTwitchBitsCheeredOccurred(object sender, TwitchUserBitsCheeredModel bitsCheered)
        {
            this.trackingAmount += bitsCheered.Amount;
            this.CurrentAmount = this.trackingAmount.ToString();
            await this.Update();
        }

        private async void EventService_OnTrovoSpellCastOccurred(object sender, TrovoChatSpellViewModel spell)
        {
            if (spell.IsElixir)
            {
                this.trackingAmount += spell.ValueTotal;
                this.CurrentAmount = this.trackingAmount.ToString();
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
                "GoalUpdate",
                new Dictionary<string, string>()
                {
                    { nameof(this.CurrentAmount), this.CurrentAmount },
                    { nameof(this.GoalAmount), this.GoalAmount },
                    { nameof(this.Width), this.Width.ToString() },
                },
                new CommandParametersModel());
        }
    }
}
