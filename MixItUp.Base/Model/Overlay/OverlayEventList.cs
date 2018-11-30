using Mixer.Base.Model.Patronage;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum EventListItemTypeEnum
    {
        Followers,
        Hosts,
        Subscribers,
        Donations,
        Milestones,
    }

    [DataContract]
    public class OverlayEventListItem
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Details { get; set; }

        public OverlayEventListItem() { }

        public OverlayEventListItem(string name, string details)
        {
            this.Name = name;
            this.Details = details;
        }
    }

    [DataContract]
    public class OverlayEventList : OverlayCustomHTMLItem
    {
        public const string HTMLTemplate =
    @"<div style=""position: relative; border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px; height: {HEIGHT}px"">
  <p style=""position: absolute; top: 35%; left: 5%; width: 50%; float: left; text-align: left; font-family: '{TEXT_FONT}'; font-size: {TOP_TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">{NAME}</p>
  <p style=""position: absolute; top: 80%; right: 5%; width: 50%; text-align: right; font-family: '{TEXT_FONT}'; font-size: {BOTTOM_TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">{DETAILS}</p>
</div>";

        public const string EventListItemType = "eventlist";

        [DataMember]
        public List<EventListItemTypeEnum> ItemTypes { get; set; }

        [DataMember]
        public int TotalToShow { get; set; }
        [DataMember]
        public bool ResetOnLoad { get; set; }

        [DataMember]
        public string BorderColor { get; set; }
        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string TextColor { get; set; }

        [DataMember]
        public string TextFont { get; set; }

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public OverlayEffectEntranceAnimationTypeEnum AddEventAnimation { get; set; }
        [DataMember]
        public string AddEventAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.AddEventAnimation); } set { } }
        [DataMember]
        public OverlayEffectExitAnimationTypeEnum RemoveEventAnimation { get; set; }
        [DataMember]
        public string RemoveEventAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.RemoveEventAnimation); } set { } }

        [DataMember]
        private List<OverlayEventListItem> events = new List<OverlayEventListItem>();

        private List<OverlayEventListItem> eventsToAdd = new List<OverlayEventListItem>();

        public OverlayEventList() : base(EventListItemType, HTMLTemplate) { }

        public OverlayEventList(string htmlText, IEnumerable<EventListItemTypeEnum> itemTypes, int totalToShow, bool resetOnLoad, string textFont, int width, int height,
            string borderColor, string backgroundColor, string textColor, OverlayEffectEntranceAnimationTypeEnum addEventAnimation, OverlayEffectExitAnimationTypeEnum removeEventAnimation)
            : base(EventListItemType, htmlText)
        {
            this.ItemTypes = new List<EventListItemTypeEnum>(itemTypes);
            this.TotalToShow = totalToShow;
            this.ResetOnLoad = resetOnLoad;
            this.TextFont = textFont;
            this.Width = width;
            this.Height = height;
            this.BorderColor = borderColor;
            this.BackgroundColor = backgroundColor;
            this.TextColor = textColor;
            this.AddEventAnimation = addEventAnimation;
            this.RemoveEventAnimation = removeEventAnimation;
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnFollowOccurred -= GlobalEvents_OnFollowOccurred;
            GlobalEvents.OnHostOccurred -= GlobalEvents_OnHostOccurred;
            GlobalEvents.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            GlobalEvents.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            GlobalEvents.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;
            GlobalEvents.OnPatronageMilestoneReachedOccurred -= GlobalEvents_OnPatronageMilestoneReachedOccurred;

            if (this.ResetOnLoad)
            {
                this.events.Clear();
            }

            if (this.ItemTypes.Contains(EventListItemTypeEnum.Followers))
            {
                GlobalEvents.OnFollowOccurred += GlobalEvents_OnFollowOccurred;
            }
            if (this.ItemTypes.Contains(EventListItemTypeEnum.Hosts))
            {
                GlobalEvents.OnHostOccurred += GlobalEvents_OnHostOccurred;
            }
            if (this.ItemTypes.Contains(EventListItemTypeEnum.Subscribers))
            {
                GlobalEvents.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
                GlobalEvents.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
            }
            if (this.ItemTypes.Contains(EventListItemTypeEnum.Donations))
            {
                GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }
            if (this.ItemTypes.Contains(EventListItemTypeEnum.Milestones))
            {
                GlobalEvents.OnPatronageMilestoneReachedOccurred += GlobalEvents_OnPatronageMilestoneReachedOccurred;
            }

            await base.Initialize();
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            if (this.eventsToAdd.Count > 0)
            {
                return await base.GetProcessedItem(user, arguments, extraSpecialIdentifiers);
            }
            return null;
        }

        public override OverlayCustomHTMLItem GetCopy() { return this.Copy<OverlayEventList>(); }

        protected override Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayEventListItem eventToAdd = this.eventsToAdd.First();
            this.eventsToAdd.RemoveAt(0);

            if (this.events.Count >= this.TotalToShow)
            {
                this.events.RemoveAt(0);
            }
            this.events.Add(eventToAdd);

            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["BACKGROUND_COLOR"] = this.BackgroundColor;
            replacementSets["BORDER_COLOR"] = this.BorderColor;
            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["WIDTH"] = this.Width.ToString();
            replacementSets["HEIGHT"] = this.Height.ToString();
            replacementSets["TOP_TEXT_HEIGHT"] = ((int)(0.4 * ((double)this.Height))).ToString();
            replacementSets["BOTTOM_TEXT_HEIGHT"] = ((int)(0.2 * ((double)this.Height))).ToString();

            replacementSets["NAME"] = eventToAdd.Name;
            replacementSets["DETAILS"] = eventToAdd.Details;

            return Task.FromResult(replacementSets);
        }

        private void AddEvent(string name, string details)
        {
            this.eventsToAdd.Add(new OverlayEventListItem(name, details));
        }

        private void GlobalEvents_OnFollowOccurred(object sender, UserViewModel user) { this.AddEvent(user.UserName, "Followed"); }

        private void GlobalEvents_OnHostOccurred(object sender, Tuple<UserViewModel, int> host) { this.AddEvent(host.Item1.UserName, string.Format("Hosted ({0})", host.Item2)); }

        private void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user) { this.AddEvent(user.UserName, "Subscribed"); }

        private void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user) { this.AddEvent(user.Item1.UserName, string.Format("Resubscribed ({0} months)", user.Item2)); }

        private void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation) { this.AddEvent(donation.UserName, string.Format("Donated {0}", donation.AmountText)); }

        private void GlobalEvents_OnPatronageMilestoneReachedOccurred(object sender, PatronageMilestoneModel patronageMilestone) { this.AddEvent(string.Format("{0} Milestone", patronageMilestone.DollarAmountText()), string.Format("{0} Sparks", patronageMilestone.target)); }
    }
}
