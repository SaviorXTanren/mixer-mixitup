using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.ViewModel.Chat.Trovo;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayEventListWidgetV3Type
    {
        Follow,
        Raid,
        Subscriber,
        Donation,
        TwitchBits,
        TrovoElixir,
    }

    [DataContract]
    public class OverlayEventListEventV3Model
    {
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string Details { get; set; }
        [DataMember]
        public string SubDetails { get; set; }
    }

    [DataContract]
    public class OverlayEventListWidgetV3Model : OverlayListItemV3ModelBase
    {
        public const string TypeReplacementKey = "Type";
        public const string DetailsReplacementKey = "Details";
        public const string SubDetailsReplacementKey = "SubDetails";

        public static readonly string DefaultHTML = Resources.OverlayEventListDefaultHTML;
        public static readonly string DefaultCSS = Resources.OverlayEventListDefaultCSS;
        public static readonly string DefaultJavascript = Resources.OverlayEventListDefaultJavascript;

        [DataMember]
        public HashSet<OverlayEventListWidgetV3Type> EventTypes { get; set; } = new HashSet<OverlayEventListWidgetV3Type>();

        [DataMember]
        public List<OverlayEventListEventV3Model> Events { get; set; } = new List<OverlayEventListEventV3Model>();

        public OverlayEventListWidgetV3Model(HashSet<OverlayEventListWidgetV3Type> eventTypes)
            : base(OverlayItemV3Type.EventList)
        {
            this.EventTypes = eventTypes;
        }

        [Obsolete]
        public OverlayEventListWidgetV3Model() : base(OverlayItemV3Type.EventList) { }

        public override async Task Enable()
        {
            foreach (OverlayEventListWidgetV3Type eventType in this.EventTypes)
            {
                if (eventType == OverlayEventListWidgetV3Type.Follow)
                {
                    EventService.OnFollowOccurred += EventService_OnFollowOccurred;
                }
                else if (eventType == OverlayEventListWidgetV3Type.Raid)
                {
                    EventService.OnRaidOccurred += EventService_OnRaidOccurred;
                }
                else if (eventType == OverlayEventListWidgetV3Type.Subscriber)
                {
                    EventService.OnSubscribeOccurred += EventService_OnSubscribeOccurred;
                    EventService.OnResubscribeOccurred += EventService_OnResubscribeOccurred;
                    EventService.OnSubscriptionGiftedOccurred += EventService_OnSubscriptionGiftedOccurred;
                }
                else if (eventType == OverlayEventListWidgetV3Type.Donation)
                {
                    EventService.OnDonationOccurred += EventService_OnDonationOccurred;
                }
                else if (eventType == OverlayEventListWidgetV3Type.TwitchBits)
                {
                    EventService.OnTwitchBitsCheeredOccurred += EventService_OnTwitchBitsCheeredOccurred;
                }
                else if (eventType == OverlayEventListWidgetV3Type.TrovoElixir)
                {
                    EventService.OnTrovoSpellCastOccurred += EventService_OnTrovoSpellCastOccurred;
                }
            }

            await base.Enable();
        }

        public override async Task Disable()
        {
            foreach (OverlayEventListWidgetV3Type eventType in this.EventTypes)
            {
                if (eventType == OverlayEventListWidgetV3Type.Follow)
                {
                    EventService.OnFollowOccurred -= EventService_OnFollowOccurred;
                }
                else if (eventType == OverlayEventListWidgetV3Type.Raid)
                {
                    EventService.OnRaidOccurred -= EventService_OnRaidOccurred;
                }
                else if (eventType == OverlayEventListWidgetV3Type.Subscriber)
                {
                    EventService.OnSubscribeOccurred -= EventService_OnSubscribeOccurred;
                    EventService.OnResubscribeOccurred -= EventService_OnResubscribeOccurred;
                    EventService.OnSubscriptionGiftedOccurred -= EventService_OnSubscriptionGiftedOccurred;
                }
                else if (eventType == OverlayEventListWidgetV3Type.Donation)
                {
                    EventService.OnDonationOccurred -= EventService_OnDonationOccurred;
                }
                else if (eventType == OverlayEventListWidgetV3Type.TwitchBits)
                {
                    EventService.OnTwitchBitsCheeredOccurred -= EventService_OnTwitchBitsCheeredOccurred;
                }
                else if (eventType == OverlayEventListWidgetV3Type.TrovoElixir)
                {
                    EventService.OnTrovoSpellCastOccurred -= EventService_OnTrovoSpellCastOccurred;
                }
            }

            await base.Disable();
        }

        private async void EventService_OnFollowOccurred(object sender, UserV2ViewModel user)
        {
            await this.AddEvent(MixItUp.Base.Resources.Follow, user.DisplayName, user: user);
        }

        private async void EventService_OnRaidOccurred(object sender, Tuple<UserV2ViewModel, int> raid)
        {
            await this.AddEvent(MixItUp.Base.Resources.Raid, raid.Item1.DisplayName, subDetails: raid.Item2.ToString(), user: raid.Item1);
        }

        private async void EventService_OnSubscribeOccurred(object sender, UserV2ViewModel user)
        {
            await this.AddEvent(MixItUp.Base.Resources.Subscriber, user.DisplayName, user: user);
        }

        private async void EventService_OnResubscribeOccurred(object sender, Tuple<UserV2ViewModel, int> resubscribe)
        {
            await this.AddEvent(MixItUp.Base.Resources.Resubscriber, resubscribe.Item1.DisplayName, subDetails: resubscribe.Item2.ToString(), user: resubscribe.Item1);
        }

        private async void EventService_OnSubscriptionGiftedOccurred(object sender, Tuple<UserV2ViewModel, UserV2ViewModel> subscriptionGifted)
        {
            await this.AddEvent(MixItUp.Base.Resources.GiftedSub, subscriptionGifted.Item1.DisplayName, subDetails: subscriptionGifted.Item2.DisplayName, user: subscriptionGifted.Item1);
        }

        private async void EventService_OnMassSubscriptionsGiftedOccurred(object sender, Tuple<UserV2ViewModel, int> massSubscriptionsGifted)
        {
            await this.AddEvent(MixItUp.Base.Resources.GiftedSubs, massSubscriptionsGifted.Item1.DisplayName, subDetails: massSubscriptionsGifted.Item2.ToString(), user: massSubscriptionsGifted.Item1);
        }

        private async void EventService_OnDonationOccurred(object sender, UserDonationModel donation)
        {
            await this.AddEvent(MixItUp.Base.Resources.Donation, donation.User.DisplayName, subDetails: donation.AmountText, user: donation.User);
        }

        private async void EventService_OnTwitchBitsCheeredOccurred(object sender, TwitchUserBitsCheeredModel bitsCheered)
        {
            await this.AddEvent(MixItUp.Base.Resources.BitsCheered, bitsCheered.User.DisplayName, subDetails: bitsCheered.Amount.ToString(), user: bitsCheered.User);
        }

        private async void EventService_OnTrovoSpellCastOccurred(object sender, TrovoChatSpellViewModel spell)
        {
            if (spell.IsElixir)
            {
                await this.AddEvent(MixItUp.Base.Resources.TrovoSpell, spell.User.DisplayName, subDetails: spell.ValueTotal.ToString(), user: spell.User);
            }
        }

        private async Task AddEvent(string type, string details, string subDetails = null, UserV2ViewModel user = null)
        {
            this.Events.Add(new OverlayEventListEventV3Model()
            {
                Type = type,
                Details = details,
                SubDetails = subDetails
            });

            await this.Update("EventListAdd", new Dictionary<string, string>()
            {
                { TypeReplacementKey, type },
                { DetailsReplacementKey, details },
                { SubDetailsReplacementKey, subDetails }
            },
            new CommandParametersModel(user));
        }
    }
}
