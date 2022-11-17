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
        public OverlayTextItemV3Model TextItem { get; set; }

        public OverlayEventListWidgetV3Model(string id, string name, Guid overlayEndpointID, OverlayTextItemV3Model item)
            : base(id, name, overlayEndpointID, item)
        {
            this.TextItem = item;
        }

        [Obsolete]
        public OverlayEventListWidgetV3Model() { }

        public override async Task<OverlayOutputV3Model> GetProcessedItem(OverlayEndpointService overlayEndpointService, CommandParametersModel parameters)
        {
            return await this.Item.GetProcessedItem(overlayEndpointService, parameters, this.CurrentReplacements);
        }

        protected override async Task EnableInternal()
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

            await base.EnableInternal();
        }

        protected override async Task DisableInternal()
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

            await base.DisableInternal();
        }

        private async void EventService_OnFollowOccurred(object sender, UserV2ViewModel user)
        {
            this.CurrentReplacements[TypeReplacementKey] = MixItUp.Base.Resources.Follow;
            this.CurrentReplacements[DetailsReplacementKey] = user.DisplayName;
            await this.Update();
        }

        private async void EventService_OnRaidOccurred(object sender, Tuple<UserV2ViewModel, int> raid)
        {
            this.CurrentReplacements[TypeReplacementKey] = MixItUp.Base.Resources.Raid;
            this.CurrentReplacements[DetailsReplacementKey] = raid.Item1.DisplayName;
            this.CurrentReplacements[SubDetailsReplacementKey] = raid.Item2.ToString();
            await this.Update();
        }

        private async void EventService_OnSubscribeOccurred(object sender, UserV2ViewModel user)
        {
            this.CurrentReplacements[TypeReplacementKey] = MixItUp.Base.Resources.Subscriber;
            this.CurrentReplacements[DetailsReplacementKey] = user.DisplayName;
            await this.Update();
        }

        private async void EventService_OnResubscribeOccurred(object sender, Tuple<UserV2ViewModel, int> resubscribe)
        {
            this.CurrentReplacements[TypeReplacementKey] = MixItUp.Base.Resources.Resubscriber;
            this.CurrentReplacements[DetailsReplacementKey] = resubscribe.Item1.DisplayName;
            this.CurrentReplacements[SubDetailsReplacementKey] = resubscribe.Item2.ToString();
            await this.Update();
        }

        private async void EventService_OnSubscriptionGiftedOccurred(object sender, Tuple<UserV2ViewModel, UserV2ViewModel> subscriptionGifted)
        {
            this.CurrentReplacements[TypeReplacementKey] = MixItUp.Base.Resources.GiftedSub;
            this.CurrentReplacements[DetailsReplacementKey] = subscriptionGifted.Item1.DisplayName;
            this.CurrentReplacements[SubDetailsReplacementKey] = subscriptionGifted.Item2.DisplayName;
            await this.Update();
        }

        private async void EventService_OnMassSubscriptionsGiftedOccurred(object sender, Tuple<UserV2ViewModel, int> massSubscriptionsGifted)
        {
            this.CurrentReplacements[TypeReplacementKey] = MixItUp.Base.Resources.GiftedSubs;
            this.CurrentReplacements[DetailsReplacementKey] = massSubscriptionsGifted.Item1.DisplayName;
            this.CurrentReplacements[SubDetailsReplacementKey] = massSubscriptionsGifted.Item2.ToString();
            await this.Update();
        }

        private async void EventService_OnDonationOccurred(object sender, UserDonationModel donation)
        {
            this.CurrentReplacements[TypeReplacementKey] = MixItUp.Base.Resources.Donation;
            this.CurrentReplacements[DetailsReplacementKey] = donation.User.DisplayName;
            this.CurrentReplacements[SubDetailsReplacementKey] = donation.AmountText;
            await this.Update();
        }

        private async void EventService_OnTwitchBitsCheeredOccurred(object sender, TwitchUserBitsCheeredModel bitsCheered)
        {
            this.CurrentReplacements[TypeReplacementKey] = MixItUp.Base.Resources.BitsCheered;
            this.CurrentReplacements[DetailsReplacementKey] = bitsCheered.User.DisplayName;
            this.CurrentReplacements[SubDetailsReplacementKey] = bitsCheered.Amount.ToString();
            await this.Update();
        }

        private async void EventService_OnTrovoSpellCastOccurred(object sender, TrovoChatSpellViewModel spell)
        {
            if (spell.IsElixir)
            {
                this.CurrentReplacements[TypeReplacementKey] = MixItUp.Base.Resources.TrovoSpell;
                this.CurrentReplacements[DetailsReplacementKey] = spell.User.DisplayName;
                this.CurrentReplacements[SubDetailsReplacementKey] = spell.ValueTotal.ToString();
                await this.Update();
            }
        }

        private async Task Update()
        {
            JObject jobj = new JObject();
            jobj[TypeReplacementKey] = this.CurrentReplacements[TypeReplacementKey];
            jobj[DetailsReplacementKey] = this.CurrentReplacements[DetailsReplacementKey];
            jobj[SubDetailsReplacementKey] = this.CurrentReplacements[SubDetailsReplacementKey];
            await this.Update("EventListAdd", jobj);
        }
    }
}
