using MixItUp.Base.Model.Twitch.Bits;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat.Trovo;
using MixItUp.Base.ViewModel.Chat.YouTube;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public abstract class OverlayEventCountingV3ModelBase : OverlayVisualTextV3ModelBase
    {
        [DataMember]
        public double FollowAmount { get; set; }

        [DataMember]
        public double RaidAmount { get; set; }
        [DataMember]
        public double RaidPerViewAmount { get; set; }

        [DataMember]
        public Dictionary<int, double> TwitchSubscriptionsAmount { get; set; } = new Dictionary<int, double>();
        [DataMember]
        public double TwitchBitsAmount { get; set; }

        [DataMember]
        public Dictionary<string, double> YouTubeMembershipsAmount { get; set; } = new Dictionary<string, double>();
        [DataMember]
        public double YouTubeSuperChatAmount { get; set; }

        [DataMember]
        public Dictionary<int, double> TrovoSubscriptionsAmount { get; set; } = new Dictionary<int, double>();
        [DataMember]
        public double TrovoElixirSpellAmount { get; set; }

        [DataMember]
        public double DonationAmount { get; set; }

        public OverlayEventCountingV3ModelBase(OverlayItemV3Type type) : base(type) { }

        public abstract Task ProcessEvent(UserV2ViewModel user, double amount);

        public override async Task Initialize()
        {
            await base.Initialize();

            this.RemoveEventHandlers();

            if (this.FollowAmount > 0)
            {
                EventService.OnFollowOccurred += EventService_OnFollowOccurred;
            }

            if (this.RaidAmount > 0 || this.RaidPerViewAmount > 0)
            {
                EventService.OnRaidOccurred += EventService_OnRaidOccurred;
            }

            if (this.TwitchSubscriptionsAmount.Any(d => d.Value > 0) || this.YouTubeMembershipsAmount.Any(d => d.Value > 0) || this.TrovoSubscriptionsAmount.Any(d => d.Value > 0))
            {
                EventService.OnSubscribeOccurred += EventService_OnSubscribeOccurred;
                EventService.OnResubscribeOccurred += EventService_OnSubscribeOccurred;
                EventService.OnSubscriptionGiftedOccurred += EventService_OnSubscribeOccurred;
                EventService.OnMassSubscriptionsGiftedOccurred += EventService_OnMassSubscriptionsGiftedOccurred;
            }

            if (this.DonationAmount > 0)
            {
                EventService.OnDonationOccurred += EventService_OnDonationOccurred;
            }

            if (this.TwitchBitsAmount > 0)
            {
                EventService.OnTwitchBitsCheeredOccurred += EventService_OnTwitchBitsCheeredOccurred;
            }

            if (this.YouTubeSuperChatAmount > 0)
            {
                EventService.OnYouTubeSuperChatOccurred += EventService_OnYouTubeSuperChatOccurred;
            }

            if (this.TrovoElixirSpellAmount > 0)
            {
                EventService.OnTrovoSpellCastOccurred += EventService_OnTrovoSpellCastOccurred;
            }
        }

        public override async Task Uninitialize()
        {
            await base.Uninitialize();

            this.RemoveEventHandlers();
        }

        public void EnableFollows()
        {
            this.RemoveEventHandlers();
            EventService.OnFollowOccurred += EventService_OnFollowOccurred;
        }

        public void EnableSubscriptions()
        {
            this.RemoveEventHandlers();
            EventService.OnSubscribeOccurred += EventService_OnSubscribeOccurred;
            EventService.OnResubscribeOccurred += EventService_OnSubscribeOccurred;
            EventService.OnSubscriptionGiftedOccurred += EventService_OnSubscribeOccurred;
            EventService.OnMassSubscriptionsGiftedOccurred += EventService_OnMassSubscriptionsGiftedOccurred;
        }

        public void ClearAllAmountsToZero()
        {
            this.FollowAmount = 0;
            this.RaidAmount = 0;
            this.RaidPerViewAmount = 0;

            for (int i = 0; i < this.TwitchSubscriptionsAmount.Count; i++)
            {
                this.TwitchSubscriptionsAmount[i] = 0;
            }
            this.TwitchBitsAmount = 0;

            this.YouTubeMembershipsAmount.Clear();
            this.YouTubeSuperChatAmount = 0;

            for (int i = 0; i < this.TrovoSubscriptionsAmount.Count; i++)
            {
                this.TrovoSubscriptionsAmount[i] = 0;
            }
            this.TrovoElixirSpellAmount = 0;

            this.DonationAmount = 0;
        }

        private async void EventService_OnFollowOccurred(object sender, UserV2ViewModel user)
        {
            Logger.Log(LogLevel.Debug, $"Processing follow for {this.ID} Overlay Widget");
            await this.ProcessEvent(user, this.FollowAmount);
        }

        private async void EventService_OnRaidOccurred(object sender, Tuple<UserV2ViewModel, int> raid)
        {
            Logger.Log(LogLevel.Debug, $"Processing raid of {raid.Item2} for {this.ID} Overlay Widget");
            await this.ProcessEvent(raid.Item1, this.RaidAmount + (this.RaidPerViewAmount * raid.Item2));
        }

        private async void EventService_OnSubscribeOccurred(object sender, SubscriptionDetailsModel subscription)
        {
            Logger.Log(LogLevel.Debug, $"Processing sub of {subscription} for {this.ID} Overlay Widget");
            if (subscription.Platform == StreamingPlatformTypeEnum.Twitch)
            {
                if (this.TwitchSubscriptionsAmount.TryGetValue(subscription.Tier, out double amount))
                {
                    await this.ProcessEvent(subscription.User, amount);
                }
            }
            else if (subscription.Platform == StreamingPlatformTypeEnum.YouTube)
            {
                if (this.YouTubeMembershipsAmount.TryGetValue(subscription.YouTubeMembershipTier, out double amount))
                {
                    await this.ProcessEvent(subscription.User, amount);
                }
            }
            else if (subscription.Platform == StreamingPlatformTypeEnum.Trovo)
            {
                if (this.TrovoSubscriptionsAmount.TryGetValue(subscription.Tier, out double amount))
                {
                    await this.ProcessEvent(subscription.User, amount);
                }
            }
        }

        private async void EventService_OnMassSubscriptionsGiftedOccurred(object sender, IEnumerable<SubscriptionDetailsModel> subscriptions)
        {
            Logger.Log(LogLevel.Debug, $"Processing mass sub of {subscriptions.Count()} for {this.ID} Overlay Widget");
            if (subscriptions.Count() > 0)
            {
                double total = 0;
                foreach (SubscriptionDetailsModel subscription in subscriptions)
                {
                    if (subscription.Platform == StreamingPlatformTypeEnum.Twitch)
                    {
                        if (this.TwitchSubscriptionsAmount.TryGetValue(subscription.Tier, out double amount))
                        {
                            total += amount;
                        }
                    }
                    else if (subscription.Platform == StreamingPlatformTypeEnum.YouTube)
                    {
                        if (this.YouTubeMembershipsAmount.TryGetValue(subscription.YouTubeMembershipTier, out double amount))
                        {
                            total += amount;
                        }
                    }
                    else if (subscription.Platform == StreamingPlatformTypeEnum.Trovo)
                    {
                        if (this.TrovoSubscriptionsAmount.TryGetValue(subscription.Tier, out double amount))
                        {
                            total += amount;
                        }
                    }
                }

                await this.ProcessEvent(subscriptions.First().User, total);
            }
        }

        private async void EventService_OnDonationOccurred(object sender, UserDonationModel donation)
        {
            Logger.Log(LogLevel.Debug, $"Processing donation of {donation.Amount} for {this.ID} Overlay Widget");
            await this.ProcessEvent(donation.User, this.DonationAmount * donation.Amount);
        }

        private async void EventService_OnTwitchBitsCheeredOccurred(object sender, TwitchBitsCheeredEventModel bitsCheered)
        {
            Logger.Log(LogLevel.Debug, $"Processing Twitch bits of {bitsCheered.Amount} for {this.ID} Overlay Widget");
            await this.ProcessEvent(bitsCheered.User, this.TwitchBitsAmount * bitsCheered.Amount);
        }

        private async void EventService_OnYouTubeSuperChatOccurred(object sender, YouTubeSuperChatViewModel superChat)
        {
            Logger.Log(LogLevel.Debug, $"Processing YouTube Super Chat of {superChat.Amount} for {this.ID} Overlay Widget");
            await this.ProcessEvent(superChat.User, this.YouTubeSuperChatAmount * superChat.Amount);
        }

        private async void EventService_OnTrovoSpellCastOccurred(object sender, TrovoChatSpellViewModel spell)
        {
            if (spell.IsElixir)
            {
                Logger.Log(LogLevel.Debug, $"Processing Trovo Elixir of {spell.ValueTotal} for {this.ID} Overlay Widget");
                await this.ProcessEvent(spell.User, this.TrovoElixirSpellAmount * spell.ValueTotal);
            }
        }

        private void RemoveEventHandlers()
        {
            EventService.OnFollowOccurred -= EventService_OnFollowOccurred;
            EventService.OnRaidOccurred -= EventService_OnRaidOccurred;
            EventService.OnSubscribeOccurred -= EventService_OnSubscribeOccurred;
            EventService.OnResubscribeOccurred -= EventService_OnSubscribeOccurred;
            EventService.OnSubscriptionGiftedOccurred -= EventService_OnSubscribeOccurred;
            EventService.OnMassSubscriptionsGiftedOccurred -= EventService_OnMassSubscriptionsGiftedOccurred;
            EventService.OnDonationOccurred -= EventService_OnDonationOccurred;
            EventService.OnTwitchBitsCheeredOccurred -= EventService_OnTwitchBitsCheeredOccurred;
            EventService.OnYouTubeSuperChatOccurred -= EventService_OnYouTubeSuperChatOccurred;
            EventService.OnTrovoSpellCastOccurred -= EventService_OnTrovoSpellCastOccurred;
        }
    }
}
