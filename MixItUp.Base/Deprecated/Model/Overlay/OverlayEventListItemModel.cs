using MixItUp.Base.Model.Twitch.Bits;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [Obsolete]
    public enum OverlayEventListItemTypeEnum
    {
        Followers,
        Hosts,
        Subscribers,
        Donations,
        [Obsolete]
        Milestones,
        [Obsolete]
        Sparks,
        [Obsolete]
        Embers,
        Bits,
        Raids,
    }

    [Obsolete]
    [DataContract]
    public class OverlayEventListItemModel : OverlayListItemModelBase
    {
        public const string HTMLTemplate =
        @"<div style=""position: relative; border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px; height: {HEIGHT}px"">
          <p style=""position: absolute; top: 35%; left: 5%; width: 50%; float: left; text-align: left; font-family: '{TEXT_FONT}'; font-size: {TOP_TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">{NAME}</p>
          <p style=""position: absolute; top: 80%; right: 5%; width: 50%; text-align: right; font-family: '{TEXT_FONT}'; font-size: {BOTTOM_TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">{DETAILS}</p>
        </div>";

        [DataMember]
        public List<OverlayEventListItemTypeEnum> ItemTypes { get; set; }

        [JsonIgnore]
        private HashSet<Guid> follows = new HashSet<Guid>();
        [JsonIgnore]
        private HashSet<Guid> hosts = new HashSet<Guid>();
        [JsonIgnore]
        private HashSet<Guid> raids = new HashSet<Guid>();
        [JsonIgnore]
        private HashSet<Guid> subs = new HashSet<Guid>();

        public OverlayEventListItemModel() : base() { }

        public OverlayEventListItemModel(string htmlText, IEnumerable<OverlayEventListItemTypeEnum> itemTypes, int totalToShow, int fadeOut, string textFont, int width, int height,
            string borderColor, string backgroundColor, string textColor, OverlayListItemAlignmentTypeEnum alignment, OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation, OverlayItemEffectExitAnimationTypeEnum removeEventAnimation)
            : base(OverlayItemModelTypeEnum.EventList, htmlText, totalToShow, fadeOut, textFont, width, height, borderColor, backgroundColor, textColor, alignment, addEventAnimation, removeEventAnimation)
        {
            this.ItemTypes = new List<OverlayEventListItemTypeEnum>(itemTypes);
        }

        public override async Task LoadTestData()
        {
            for (int i = 0; i < 5; i++)
            {
                await this.AddEvent("Joe Smoe", "Followed");
            }
        }

        public override async Task Enable()
        {
            if (this.ItemTypes.Contains(OverlayEventListItemTypeEnum.Followers))
            {
                EventService.OnFollowOccurred += GlobalEvents_OnFollowOccurred;
            }
            if (this.ItemTypes.Contains(OverlayEventListItemTypeEnum.Hosts))
            {

            }
            if (this.ItemTypes.Contains(OverlayEventListItemTypeEnum.Raids))
            {
                EventService.OnRaidOccurred += GlobalEvents_OnRaidOccurred;
            }
            if (this.ItemTypes.Contains(OverlayEventListItemTypeEnum.Subscribers))
            {
                //EventService.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
                //EventService.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
                //EventService.OnSubscriptionGiftedOccurred += GlobalEvents_OnSubscriptionGiftedOccurred;
            }
            if (this.ItemTypes.Contains(OverlayEventListItemTypeEnum.Donations))
            {
                EventService.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
                StreamlootsService.OnStreamlootsPurchaseOccurred += GlobalEvents_OnStreamlootsPurchaseOccurred;
            }
            if (this.ItemTypes.Contains(OverlayEventListItemTypeEnum.Bits))
            {
                EventService.OnTwitchBitsCheeredOccurred += GlobalEvents_OnBitsOccurred;
            }

            await base.Enable();
        }

        public override async Task Disable()
        {
            EventService.OnFollowOccurred -= GlobalEvents_OnFollowOccurred;
            EventService.OnRaidOccurred -= GlobalEvents_OnRaidOccurred;
            //EventService.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            //EventService.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            //EventService.OnSubscriptionGiftedOccurred -= GlobalEvents_OnSubscriptionGiftedOccurred;
            EventService.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;
            StreamlootsService.OnStreamlootsPurchaseOccurred -= GlobalEvents_OnStreamlootsPurchaseOccurred;
            EventService.OnTwitchBitsCheeredOccurred -= GlobalEvents_OnBitsOccurred;

            await base.Disable();
        }

        private async void GlobalEvents_OnFollowOccurred(object sender, UserV2ViewModel user)
        {
            if (!this.follows.Contains(user.ID))
            {
                this.follows.Add(user.ID);
                await this.AddEvent(user.DisplayName, MixItUp.Base.Resources.Followed);
            }
        }

        private async void GlobalEvents_OnHostOccurred(object sender, UserV2ViewModel user)
        {
            if (!this.hosts.Contains(user.ID))
            {
                this.hosts.Add(user.ID);
                await this.AddEvent(user.DisplayName, MixItUp.Base.Resources.Hosted);
            }
        }

        private async void GlobalEvents_OnRaidOccurred(object sender, Tuple<UserV2ViewModel, int> raid)
        {
            if (!this.raids.Contains(raid.Item1.ID))
            {
                this.raids.Add(raid.Item1.ID);
                await this.AddEvent(raid.Item1.DisplayName, string.Format(MixItUp.Base.Resources.RaidedAmount, raid.Item2));
            }
        }

        private async void GlobalEvents_OnSubscribeOccurred(object sender, UserV2ViewModel user)
        {
            if (!this.subs.Contains(user.ID))
            {
                this.subs.Add(user.ID);
                await this.AddEvent(user.DisplayName, MixItUp.Base.Resources.Subscribed);
            }
        }

        private async void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserV2ViewModel, int> user)
        {
            if (!this.subs.Contains(user.Item1.ID))
            {
                this.subs.Add(user.Item1.ID);
                await this.AddEvent(user.Item1.DisplayName, string.Format(MixItUp.Base.Resources.ResubscribedAmount, user.Item2));
            }
        }

        private async void GlobalEvents_OnSubscriptionGiftedOccurred(object sender, Tuple<UserV2ViewModel, UserV2ViewModel> e)
        {
            if (!this.subs.Contains(e.Item2.ID))
            {
                this.subs.Add(e.Item2.ID);
                await this.AddEvent(e.Item2.DisplayName, MixItUp.Base.Resources.GiftedSub);
            }
        }

        private async void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation) { await this.AddEvent(donation.Username, string.Format(MixItUp.Base.Resources.DonatedAmount, donation.AmountText)); }

        private async void GlobalEvents_OnStreamlootsPurchaseOccurred(object sender, Tuple<UserV2ViewModel, int> purchase) { await this.AddEvent(purchase.Item1.DisplayName, string.Format(MixItUp.Base.Resources.StreamlootsPacksPurchasedAmount, purchase.Item2)); }

        private async void GlobalEvents_OnBitsOccurred(object sender, TwitchBitsCheeredEventModel e) { await this.AddEvent(e.User.DisplayName, string.Format(MixItUp.Base.Resources.TwitchBitsCheeredAmount, e.Amount)); }

        private async Task AddEvent(string name, string details)
        {
            OverlayListIndividualItemModel item = OverlayListIndividualItemModel.CreateAddItem(name + details, null, -1, this.HTML);
            item.TemplateReplacements.Add("NAME", name);
            item.TemplateReplacements.Add("DETAILS", details);
            item.TemplateReplacements.Add("TOP_TEXT_HEIGHT", ((int)(0.4 * ((double)this.Height))).ToString());
            item.TemplateReplacements.Add("BOTTOM_TEXT_HEIGHT", ((int)(0.2 * ((double)this.Height))).ToString());

            await this.listSemaphore.WaitAsync();

            this.Items.Add(item);
            this.SendUpdateRequired();

            this.listSemaphore.Release();
        }
    }
}
