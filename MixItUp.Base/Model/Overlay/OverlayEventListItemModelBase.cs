using Mixer.Base.Model.Patronage;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
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
        Milestones,
        Sparks,
        Embers,
    }

    [DataContract]
    public class OverlayEventListItemModelBase : OverlayListItemModelBase
    {
        [DataContract]
        public class OverlayEventItemModel
        {
            [DataMember]
            public string Name { get; set; }
            [DataMember]
            public string Details { get; set; }

            public OverlayEventItemModel() { }

            public OverlayEventItemModel(string name, string details)
            {
                this.Name = name;
                this.Details = details;
            }
        }

        public const string HTMLTemplate =
        @"<div style=""position: relative; border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px; height: {HEIGHT}px"">
          <p style=""position: absolute; top: 35%; left: 5%; width: 50%; float: left; text-align: left; font-family: '{TEXT_FONT}'; font-size: {TOP_TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">{NAME}</p>
          <p style=""position: absolute; top: 80%; right: 5%; width: 50%; text-align: right; font-family: '{TEXT_FONT}'; font-size: {BOTTOM_TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">{DETAILS}</p>
        </div>";

        [DataMember]
        public List<OverlayEventListItemTypeEnum> ItemTypes { get; set; }

        [DataMember]
        public OverlayEventItemModel NewEvent { get; set; }

        private HashSet<uint> follows = new HashSet<uint>();
        private HashSet<uint> hosts = new HashSet<uint>();
        private HashSet<uint> subs = new HashSet<uint>();

        public OverlayEventListItemModelBase() : base() { }

        public OverlayEventListItemModelBase(string htmlText, IEnumerable<OverlayEventListItemTypeEnum> itemTypes, int totalToShow, string textFont, int width, int height,
            string borderColor, string backgroundColor, string textColor, OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation, OverlayItemEffectExitAnimationTypeEnum removeEventAnimation)
            : base(OverlayItemModelTypeEnum.EventList, htmlText, totalToShow, textFont, width, height, borderColor, backgroundColor, textColor, addEventAnimation, removeEventAnimation)
        {
            this.ItemTypes = new List<OverlayEventListItemTypeEnum>(itemTypes);
        }

        public override async Task LoadTestData()
        {
            for (int i = 0; i < 5; i++)
            {
                this.AddEvent("Joe Smoe", "Followed");

                await Task.Delay(1000);
            }
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnFollowOccurred -= GlobalEvents_OnFollowOccurred;
            GlobalEvents.OnHostOccurred -= GlobalEvents_OnHostOccurred;
            GlobalEvents.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            GlobalEvents.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            GlobalEvents.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;
            GlobalEvents.OnSparkUseOccurred -= GlobalEvents_OnSparkUseOccurred;
            GlobalEvents.OnEmberUseOccurred -= GlobalEvents_OnEmberUseOccurred;
            GlobalEvents.OnPatronageMilestoneReachedOccurred -= GlobalEvents_OnPatronageMilestoneReachedOccurred;

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
            }
            if (this.ItemTypes.Contains(OverlayEventListItemTypeEnum.Donations))
            {
                GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }
            if (this.ItemTypes.Contains(OverlayEventListItemTypeEnum.Sparks))
            {
                GlobalEvents.OnSparkUseOccurred += GlobalEvents_OnSparkUseOccurred;
            }
            if (this.ItemTypes.Contains(OverlayEventListItemTypeEnum.Embers))
            {
                GlobalEvents.OnEmberUseOccurred += GlobalEvents_OnEmberUseOccurred;
            }
            if (this.ItemTypes.Contains(OverlayEventListItemTypeEnum.Milestones))
            {
                GlobalEvents.OnPatronageMilestoneReachedOccurred += GlobalEvents_OnPatronageMilestoneReachedOccurred;
            }

            await base.Initialize();
        }

        private void GlobalEvents_OnFollowOccurred(object sender, UserViewModel user)
        {
            if (!this.follows.Contains(user.ID))
            {
                this.follows.Add(user.ID);
                this.AddEvent(user.UserName, "Followed");
            }
        }

        private void GlobalEvents_OnHostOccurred(object sender, Tuple<UserViewModel, int> host)
        {
            if (!this.hosts.Contains(host.Item1.ID))
            {
                this.hosts.Add(host.Item1.ID);
                this.AddEvent(host.Item1.UserName, string.Format("Hosted ({0})", host.Item2));
            }
        }

        private void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user)
        {
            if (!this.subs.Contains(user.ID))
            {
                this.subs.Add(user.ID);
                this.AddEvent(user.UserName, "Subscribed");
            }
        }

        private void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user)
        {
            if (!this.subs.Contains(user.Item1.ID))
            {
                this.subs.Add(user.Item1.ID);
                this.AddEvent(user.Item1.UserName, string.Format("Resubscribed ({0} months)", user.Item2));
            }
        }

        private void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation) { this.AddEvent(donation.UserName, string.Format("Donated {0}", donation.AmountText)); }

        private void GlobalEvents_OnSparkUseOccurred(object sender, Tuple<UserViewModel, int> sparkUsage) { this.AddEvent(sparkUsage.Item1.UserName, string.Format("{0} Sparks", sparkUsage.Item2)); }

        private void GlobalEvents_OnEmberUseOccurred(object sender, UserEmberUsageModel emberUsage) { this.AddEvent(emberUsage.User.UserName, string.Format("{0} Embers", emberUsage.Amount)); }

        private void GlobalEvents_OnPatronageMilestoneReachedOccurred(object sender, PatronageMilestoneModel patronageMilestone) { this.AddEvent(string.Format("{0} Milestone", patronageMilestone.DollarAmountText()), string.Format("{0} Sparks", patronageMilestone.target)); }

        private void AddEvent(string name, string details)
        {
            this.NewEvent = new OverlayEventItemModel(name, details);
            this.SendUpdateRequired();
        }
    }
}
