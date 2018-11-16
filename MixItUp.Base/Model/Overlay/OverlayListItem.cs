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
    public enum ListItemTypeEnum
    {
        Followers,
        Hosts,
        Subscribers,
        Donations,
        Sparks,
        Milestones,
    }

    [DataContract]
    public class OverlayListEvent
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Details { get; set; }

        public OverlayListEvent() { }

        public OverlayListEvent(string name, string details)
        {
            this.Name = name;
            this.Details = details;
        }
    }

    [DataContract]
    public class OverlayListItem : OverlayCustomHTMLItem
    {
        public const string HTMLTemplate =
    @"<div style=""position: relative; border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px; height: {HEIGHT}px"">
  <p style=""position: absolute; top: 35%; left: 5%; width: 50%; float: left; text-align: left; font-size: {TOP_TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">{USERNAME}</p>
  <p style=""position: absolute; top: 80%; right: 5%; width: 50%; text-align: right; font-size: {BOTTOM_TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">{EVENT}</p>
</div>";

        [DataMember]
        public List<ListItemTypeEnum> ItemTypes { get; set; }

        [DataMember]
        public int TotalToShow { get; set; }
        [DataMember]
        public bool ResetOnLoad { get; set; }

        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string BorderColor { get; set; }
        [DataMember]
        public string TextColor { get; set; }

        [DataMember]
        public string TextFont { get; set; }

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        private DateTimeOffset LastReset { get; set; }

        [DataMember]
        private List<OverlayListEvent> events = new List<OverlayListEvent>();

        public OverlayListItem() { }

        private OverlayListItem(string htmlText, IEnumerable<ListItemTypeEnum> itemTypes, int totalToShow, bool resetOnLoad, string backgroundColor, string borderColor, string textColor,
            string textFont, int width, int height)
            : base(htmlText)
        {
            this.ItemTypes = new List<ListItemTypeEnum>(itemTypes);
            this.TotalToShow = totalToShow;
            this.ResetOnLoad = resetOnLoad;
            this.BackgroundColor = backgroundColor;
            this.BorderColor = borderColor;
            this.TextColor = textColor;
            this.TextFont = textFont;
            this.Width = width;
            this.Height = height;
            this.LastReset = DateTimeOffset.Now;
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnFollowOccurred -= GlobalEvents_OnFollowOccurred;
            GlobalEvents.OnHostOccurred -= GlobalEvents_OnHostOccurred;
            GlobalEvents.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            GlobalEvents.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            GlobalEvents.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;
            GlobalEvents.OnSparkUseOccurred -= GlobalEvents_OnSparkUseOccurred;
            GlobalEvents.OnPatronageMilestoneReachedOccurred -= GlobalEvents_OnPatronageMilestoneReachedOccurred;

            if (this.ResetOnLoad)
            {
                this.events.Clear();
            }

            if (this.ItemTypes.Contains(ListItemTypeEnum.Followers))
            {
                GlobalEvents.OnFollowOccurred += GlobalEvents_OnFollowOccurred;
            }
            if (this.ItemTypes.Contains(ListItemTypeEnum.Hosts))
            {
                GlobalEvents.OnHostOccurred += GlobalEvents_OnHostOccurred;
            }
            if (this.ItemTypes.Contains(ListItemTypeEnum.Subscribers))
            {
                GlobalEvents.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
                GlobalEvents.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
            }
            if (this.ItemTypes.Contains(ListItemTypeEnum.Donations))
            {
                GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }
            if (this.ItemTypes.Contains(ListItemTypeEnum.Sparks))
            {
                GlobalEvents.OnSparkUseOccurred += GlobalEvents_OnSparkUseOccurred;
            }
            if (this.ItemTypes.Contains(ListItemTypeEnum.Milestones))
            {
                GlobalEvents.OnPatronageMilestoneReachedOccurred += GlobalEvents_OnPatronageMilestoneReachedOccurred;
            }

            await base.Initialize();
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            return await base.GetProcessedItem(user, arguments, extraSpecialIdentifiers);
        }

        protected override async Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            //replacementSets["BACKGROUND_COLOR"] = this.BackgroundColor;
            //replacementSets["PROGRESS_COLOR"] = this.ProgressColor;
            //replacementSets["TEXT_COLOR"] = this.TextColor;
            //replacementSets["BAR_WIDTH"] = this.Width.ToString();
            //replacementSets["BAR_HEIGHT"] = this.Height.ToString();
            //replacementSets["TEXT_SIZE"] = ((3 * this.Height) / 4).ToString();

            //double amount = this.CurrentAmountNumber;
            //double goal = this.GoalAmountNumber;

            //if (this.ProgressBarType == ProgressBarTypeEnum.Followers)
            //{
            //    amount = this.totalFollowers;
            //    if (this.CurrentAmountNumber >= 0)
            //    {
            //        amount = this.totalFollowers - this.CurrentAmountNumber;
            //    }
            //}
            //else if (this.ProgressBarType == ProgressBarTypeEnum.Milestones)
            //{
            //    if (this.refreshMilestone)
            //    {
            //        this.refreshMilestone = false;
            //        PatronageMilestoneModel currentMilestone = await ChannelSession.Connection.GetCurrentPatronageMilestone();
            //        if (currentMilestone != null)
            //        {
            //            goal = this.GoalAmountNumber = currentMilestone.target;
            //        }
            //    }
            //}
            //else if (this.ProgressBarType == ProgressBarTypeEnum.Custom)
            //{
            //    if (!string.IsNullOrEmpty(this.CurrentAmountCustom))
            //    {
            //        string customAmount = await this.ReplaceStringWithSpecialModifiers(this.CurrentAmountCustom, user, arguments, extraSpecialIdentifiers);
            //        double.TryParse(customAmount, out amount);
            //    }
            //    if (!string.IsNullOrEmpty(this.GoalAmountCustom))
            //    {
            //        string customGoal = await this.ReplaceStringWithSpecialModifiers(this.GoalAmountCustom, user, arguments, extraSpecialIdentifiers);
            //        double.TryParse(customGoal, out goal);
            //    }
            //}

            //replacementSets["AMOUNT"] = amount.ToString();
            //replacementSets["GOAL"] = goal.ToString();
            //if (goal > 0)
            //{
            //    replacementSets["PROGRESS_WIDTH"] = ((int)(((double)this.Width) * (amount / goal))).ToString();
            //}

            return replacementSets;
        }

        private void AddEvent(string name, string details)
        {
            this.events.Insert(0, new OverlayListEvent(name, details));
        }

        private void GlobalEvents_OnFollowOccurred(object sender, UserViewModel user) { }

        private void GlobalEvents_OnHostOccurred(object sender, Tuple<UserViewModel, int> e)
        {

        }

        private void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user)
        {

        }

        private void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user)
        {

        }

        private void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation)
        {

        }

        private void GlobalEvents_OnSparkUseOccurred(object sender, Tuple<UserViewModel, int> user)
        {

        }

        private void GlobalEvents_OnPatronageUpdateOccurred(object sender, PatronageStatusModel patronageStatus)
        {

        }

        private void GlobalEvents_OnPatronageMilestoneReachedOccurred(object sender, PatronageMilestoneModel patronageMilestone)
        {

        }
    }
}
