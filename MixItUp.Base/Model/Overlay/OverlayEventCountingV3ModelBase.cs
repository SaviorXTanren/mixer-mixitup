using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
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
        public int FollowAmount { get; set; }

        [DataMember]
        public int RaidAmount { get; set; }
        [DataMember]
        public double RaidPerViewAmount { get; set; }

        [DataMember]
        public Dictionary<int, int> TwitchSubscriptionsAmount { get; set; } = new Dictionary<int, int>();
        [DataMember]
        public double TwitchBitsAmount { get; set; }

        [DataMember]
        public Dictionary<string, int> YouTubeMembershipsAmount { get; set; } = new Dictionary<string, int>();
        [DataMember]
        public double YouTubeSuperChatAmount { get; set; }

        [DataMember]
        public Dictionary<int, int> TrovoSubscriptionsAmount { get; set; } = new Dictionary<int, int>();
        [DataMember]
        public double TrovoElixirSpellAmount { get; set; }

        [DataMember]
        public double DonationAmount { get; set; }

        public OverlayEventCountingV3ModelBase(OverlayItemV3Type type) : base(type) { }

        public abstract Task ProcessEvent(UserV2ViewModel user, double amount);

        protected override async Task WidgetEnableInternal()
        {
            await base.WidgetEnableInternal();

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

        protected override async Task WidgetDisableInternal()
        {
            await base.WidgetDisableInternal();

            EventService.OnFollowOccurred -= EventService_OnFollowOccurred;
            EventService.OnRaidOccurred -= EventService_OnRaidOccurred;
            EventService.OnSubscribeOccurred -= EventService_OnSubscribeOccurred;
            EventService.OnResubscribeOccurred -= EventService_OnSubscribeOccurred;
            EventService.OnSubscriptionGiftedOccurred -= EventService_OnSubscribeOccurred;
            EventService.OnMassSubscriptionsGiftedOccurred -= EventService_OnMassSubscriptionsGiftedOccurred;
            EventService.OnDonationOccurred -= EventService_OnDonationOccurred;
            EventService.OnTwitchBitsCheeredOccurred -= EventService_OnTwitchBitsCheeredOccurred;
            EventService.OnTrovoSpellCastOccurred -= EventService_OnTrovoSpellCastOccurred;
            EventService.OnYouTubeSuperChatOccurred -= EventService_OnYouTubeSuperChatOccurred;
        }

        private async void EventService_OnFollowOccurred(object sender, UserV2ViewModel user)
        {
            await this.ProcessEvent(user, this.FollowAmount);
        }

        private async void EventService_OnRaidOccurred(object sender, Tuple<UserV2ViewModel, int> raid)
        {
            await this.ProcessEvent(raid.Item1, this.RaidAmount + (this.RaidPerViewAmount * raid.Item2));
        }

        private async void EventService_OnSubscribeOccurred(object sender, SubscriptionDetailsModel subscription)
        {
            if (subscription.Platform == StreamingPlatformTypeEnum.Twitch)
            {
                if (this.TwitchSubscriptionsAmount.TryGetValue(subscription.Tier, out int damage))
                {
                    await this.ProcessEvent(subscription.User, damage);
                }
            }
            else if (subscription.Platform == StreamingPlatformTypeEnum.YouTube)
            {
                if (this.YouTubeMembershipsAmount.TryGetValue(subscription.YouTubeMembershipTier, out int damage))
                {
                    await this.ProcessEvent(subscription.User, damage);
                }
            }
            else if (subscription.Platform == StreamingPlatformTypeEnum.Trovo)
            {
                if (this.TrovoSubscriptionsAmount.TryGetValue(subscription.Tier, out int damage))
                {
                    await this.ProcessEvent(subscription.User, damage);
                }
            }
        }

        private async void EventService_OnMassSubscriptionsGiftedOccurred(object sender, IEnumerable<SubscriptionDetailsModel> subscriptions)
        {
            if (subscriptions.Count() > 0)
            {
                double totalDamage = 0;
                foreach (SubscriptionDetailsModel subscription in subscriptions)
                {
                    if (subscription.Platform == StreamingPlatformTypeEnum.Twitch)
                    {
                        if (this.TwitchSubscriptionsAmount.TryGetValue(subscription.Tier, out int damage))
                        {
                            totalDamage += damage;
                        }
                    }
                    else if (subscription.Platform == StreamingPlatformTypeEnum.YouTube)
                    {
                        if (this.YouTubeMembershipsAmount.TryGetValue(subscription.YouTubeMembershipTier, out int damage))
                        {
                            totalDamage += damage;
                        }
                    }
                    else if (subscription.Platform == StreamingPlatformTypeEnum.Trovo)
                    {
                        if (this.TrovoSubscriptionsAmount.TryGetValue(subscription.Tier, out int damage))
                        {
                            totalDamage += damage;
                        }
                    }
                }

                await this.ProcessEvent(subscriptions.First().User, totalDamage);
            }
        }

        private async void EventService_OnDonationOccurred(object sender, UserDonationModel donation)
        {
            await this.ProcessEvent(donation.User, this.DonationAmount * donation.Amount);
        }

        private async void EventService_OnTwitchBitsCheeredOccurred(object sender, TwitchUserBitsCheeredModel bitsCheered)
        {
            await this.ProcessEvent(bitsCheered.User, this.TwitchBitsAmount * bitsCheered.Amount);
        }

        private async void EventService_OnYouTubeSuperChatOccurred(object sender, YouTubeSuperChatViewModel superChat)
        {
            await this.ProcessEvent(superChat.User, this.YouTubeSuperChatAmount * superChat.Amount);
        }

        private async void EventService_OnTrovoSpellCastOccurred(object sender, TrovoChatSpellViewModel spell)
        {
            if (spell.IsElixir)
            {
                await this.ProcessEvent(spell.User, this.TrovoElixirSpellAmount * spell.ValueTotal);
            }
        }
    }
}
