using Mixer.Base.Model.Patronage;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
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
    }

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

        private HashSet<Guid> follows = new HashSet<Guid>();
        private HashSet<Guid> hosts = new HashSet<Guid>();
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
                GlobalEvents.OnFollowOccurred += GlobalEvents_OnFollowOccurred;
            }
            if (this.ItemTypes.Contains(OverlayEventListItemTypeEnum.Hosts))
            {
                GlobalEvents.OnHostOccurred += GlobalEvents_OnHostOccurred;
            }
            if (this.ItemTypes.Contains(OverlayEventListItemTypeEnum.Subscribers))
            {
                GlobalEvents.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
                GlobalEvents.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
                GlobalEvents.OnSubscriptionGiftedOccurred += GlobalEvents_OnSubscriptionGiftedOccurred;
            }
            if (this.ItemTypes.Contains(OverlayEventListItemTypeEnum.Donations))
            {
                GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
                GlobalEvents.OnStreamlootsPurchaseOccurred += GlobalEvents_OnStreamlootsPurchaseOccurred;
            }

            await base.Enable();
        }

        public override async Task Disable()
        {
            GlobalEvents.OnFollowOccurred -= GlobalEvents_OnFollowOccurred;
            GlobalEvents.OnHostOccurred -= GlobalEvents_OnHostOccurred;
            GlobalEvents.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            GlobalEvents.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            GlobalEvents.OnSubscriptionGiftedOccurred -= GlobalEvents_OnSubscriptionGiftedOccurred;
            GlobalEvents.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;
            GlobalEvents.OnStreamlootsPurchaseOccurred -= GlobalEvents_OnStreamlootsPurchaseOccurred;

            await base.Disable();
        }

        private async void GlobalEvents_OnFollowOccurred(object sender, UserViewModel user)
        {
            if (!this.follows.Contains(user.ID))
            {
                this.follows.Add(user.ID);
                await this.AddEvent(user.Username, "Followed");
            }
        }

        private async void GlobalEvents_OnHostOccurred(object sender, Tuple<UserViewModel, int> host)
        {
            if (!this.hosts.Contains(host.Item1.ID))
            {
                this.hosts.Add(host.Item1.ID);
                await this.AddEvent(host.Item1.Username, string.Format("Hosted ({0})", host.Item2));
            }
        }

        private async void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user)
        {
            if (!this.subs.Contains(user.ID))
            {
                this.subs.Add(user.ID);
                await this.AddEvent(user.Username, "Subscribed");
            }
        }

        private async void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user)
        {
            if (!this.subs.Contains(user.Item1.ID))
            {
                this.subs.Add(user.Item1.ID);
                await this.AddEvent(user.Item1.Username, string.Format("Resubscribed ({0} months)", user.Item2));
            }
        }

        private async void GlobalEvents_OnSubscriptionGiftedOccurred(object sender, Tuple<UserViewModel, UserViewModel> e)
        {
            if (!this.subs.Contains(e.Item2.ID))
            {
                this.subs.Add(e.Item2.ID);
                await this.AddEvent(e.Item2.Username, "Gifted Sub");
            }
        }

        private async void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation) { await this.AddEvent(donation.Username, string.Format("Donated {0}", donation.AmountText)); }

        private async void GlobalEvents_OnStreamlootsPurchaseOccurred(object sender, Tuple<UserViewModel, int> purchase) { await this.AddEvent(purchase.Item1.Username, string.Format("Purchased {0} Packs", purchase.Item2)); }

        private async Task AddEvent(string name, string details)
        {
            OverlayListIndividualItemModel item = OverlayListIndividualItemModel.CreateAddItem(name + details, null, -1, this.HTML);
            item.TemplateReplacements.Add("NAME", name);
            item.TemplateReplacements.Add("DETAILS", details);
            item.TemplateReplacements.Add("TOP_TEXT_HEIGHT", ((int)(0.4 * ((double)this.Height))).ToString());
            item.TemplateReplacements.Add("BOTTOM_TEXT_HEIGHT", ((int)(0.2 * ((double)this.Height))).ToString());

            await this.listSemaphore.WaitAndRelease(() =>
            {
                this.Items.Add(item);
                this.SendUpdateRequired();
                return Task.FromResult(0);
            });
        }
    }
}
