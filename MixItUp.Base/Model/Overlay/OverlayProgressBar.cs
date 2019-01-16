﻿using Mixer.Base.Model.Patronage;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum ProgressBarTypeEnum
    {
        Followers,
        Subscribers,
        Donations,
        Sparks,
        Milestones,
        Custom,
        Embers,
    }

    [DataContract]
    public class OverlayProgressBar : OverlayCustomHTMLItem
    {
        public const string HTMLTemplate =
            @"<div style=""position: absolute; background-color: {BACKGROUND_COLOR}; width: {BAR_WIDTH}px; height: {BAR_HEIGHT}px; transform: translate(-50%, -50%);"">
    <div style=""position: absolute; background-color: {PROGRESS_COLOR}; width: {PROGRESS_WIDTH}px; height: {BAR_HEIGHT}px;""></div>
</div>
<p style=""position: absolute; font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(-50%, -50%);"">{AMOUNT} ({PERCENTAGE}%)</p>";

        public const string GoalReachedCommandName = "On Goal Reached";

        [DataMember]
        public ProgressBarTypeEnum ProgressBarType { get; set; }

        [DataMember]
        public double CurrentAmountNumber { get; set; }
        [DataMember]
        public double GoalAmountNumber { get; set; }

        [DataMember]
        public string CurrentAmountCustom { get; set; }
        [DataMember]
        public string GoalAmountCustom { get; set; }

        [DataMember]
        public int ResetAfterDays { get; set; }

        [DataMember]
        public string ProgressColor { get; set; }
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
        public CustomCommand GoalReachedCommand { get; set; }

        [DataMember]
        private DateTimeOffset LastReset { get; set; }
        [DataMember]
        private bool GoalReached { get; set; }

        private int totalFollowers = 0;

        private bool refreshMilestone;

        public OverlayProgressBar() : base(CustomItemType, HTMLTemplate) { }

        public OverlayProgressBar(string htmlText, ProgressBarTypeEnum progressBarType, double currentAmount, double goalAmount, int resetAfterDays, string progressColor,
            string backgroundColor, string textColor, string textFont, int width, int height, CustomCommand goalReachedCommand)
            : this(htmlText, progressBarType, resetAfterDays, progressColor, backgroundColor, textColor, textFont, width, height, goalReachedCommand)
        {
            this.CurrentAmountNumber = currentAmount;
            this.GoalAmountNumber = goalAmount;
        }

        public OverlayProgressBar(string htmlText, ProgressBarTypeEnum progressBarType, string currentAmount, string goalAmount, int resetAfterDays, string progressColor,
            string backgroundColor, string textColor, string textFont, int width, int height, CustomCommand goalReachedCommand)
            : this(htmlText, progressBarType, resetAfterDays, progressColor, backgroundColor, textColor, textFont, width, height, goalReachedCommand)
        {
            this.CurrentAmountCustom = currentAmount;
            this.GoalAmountCustom = goalAmount;
        }

        private OverlayProgressBar(string htmlText, ProgressBarTypeEnum progressBarType, int resetAfterDays, string progressColor, string backgroundColor, string textColor,
            string textFont, int width, int height, CustomCommand goalReachedCommand)
            : base(CustomItemType, htmlText)
        {
            this.ProgressBarType = progressBarType;
            this.ResetAfterDays = resetAfterDays;
            this.ProgressColor = progressColor;
            this.BackgroundColor = backgroundColor;
            this.TextColor = textColor;
            this.TextFont = textFont;
            this.Width = width;
            this.Height = height;
            this.GoalReachedCommand = goalReachedCommand;
            this.LastReset = DateTimeOffset.Now;
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnFollowOccurred -= GlobalEvents_OnFollowOccurred;
            GlobalEvents.OnUnfollowOccurred -= GlobalEvents_OnUnfollowOccurred;
            GlobalEvents.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            GlobalEvents.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            GlobalEvents.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;
            GlobalEvents.OnSparkUseOccurred -= GlobalEvents_OnSparkUseOccurred;
            GlobalEvents.OnEmberUseOccurred -= GlobalEvents_OnEmberUseOccurred;
            GlobalEvents.OnPatronageUpdateOccurred -= GlobalEvents_OnPatronageUpdateOccurred;
            GlobalEvents.OnPatronageMilestoneReachedOccurred -= GlobalEvents_OnPatronageMilestoneReachedOccurred;

            if (this.ProgressBarType == ProgressBarTypeEnum.Followers)
            {
                totalFollowers = (int)ChannelSession.Channel.numFollowers;

                GlobalEvents.OnFollowOccurred += GlobalEvents_OnFollowOccurred;
                GlobalEvents.OnUnfollowOccurred += GlobalEvents_OnUnfollowOccurred;
            }
            else if (this.ProgressBarType == ProgressBarTypeEnum.Subscribers)
            {
                GlobalEvents.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
                GlobalEvents.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
            }
            else if (this.ProgressBarType == ProgressBarTypeEnum.Donations)
            {
                GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }
            else if (this.ProgressBarType == ProgressBarTypeEnum.Sparks)
            {
                GlobalEvents.OnSparkUseOccurred += GlobalEvents_OnSparkUseOccurred;
            }
            else if (this.ProgressBarType == ProgressBarTypeEnum.Embers)
            {
                GlobalEvents.OnEmberUseOccurred += GlobalEvents_OnEmberUseOccurred;
            }
            else if (this.ProgressBarType == ProgressBarTypeEnum.Milestones)
            {
                PatronageStatusModel patronageStatus = await ChannelSession.Connection.GetPatronageStatus(ChannelSession.Channel);
                if (patronageStatus != null)
                {
                    this.CurrentAmountNumber = patronageStatus.patronageEarned;
                }

                PatronageMilestoneModel currentMilestone = await ChannelSession.Connection.GetCurrentPatronageMilestone();
                if (currentMilestone != null)
                {
                    this.GoalAmountNumber = currentMilestone.target;
                }

                GlobalEvents.OnPatronageUpdateOccurred += GlobalEvents_OnPatronageUpdateOccurred;
                GlobalEvents.OnPatronageMilestoneReachedOccurred += GlobalEvents_OnPatronageMilestoneReachedOccurred;
            }

            await base.Initialize();
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            if (this.ResetAfterDays > 0 && this.LastReset.TotalDaysFromNow() > this.ResetAfterDays)
            {
                this.CurrentAmountNumber = 0;
                this.LastReset = DateTimeOffset.Now;
            }
            return await base.GetProcessedItem(user, arguments, extraSpecialIdentifiers);
        }

        public override OverlayCustomHTMLItem GetCopy()
        {
            OverlayProgressBar copy = this.Copy<OverlayProgressBar>();
            copy.GoalReachedCommand = null;
            return copy;
        }

        protected override async Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["BACKGROUND_COLOR"] = this.BackgroundColor;
            replacementSets["PROGRESS_COLOR"] = this.ProgressColor;
            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["BAR_WIDTH"] = this.Width.ToString();
            replacementSets["BAR_HEIGHT"] = this.Height.ToString();
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["TEXT_SIZE"] = ((3 * this.Height) / 4).ToString();

            double amount = this.CurrentAmountNumber;
            double goal = this.GoalAmountNumber;

            if (this.ProgressBarType == ProgressBarTypeEnum.Followers)
            {
                amount = this.totalFollowers;
                if (this.CurrentAmountNumber >= 0)
                {
                    amount = this.totalFollowers - this.CurrentAmountNumber;
                }
            }
            else if (this.ProgressBarType == ProgressBarTypeEnum.Milestones)
            {
                if (this.refreshMilestone)
                {
                    this.refreshMilestone = false;
                    PatronageMilestoneModel currentMilestone = await ChannelSession.Connection.GetCurrentPatronageMilestone();
                    if (currentMilestone != null)
                    {
                        goal = this.GoalAmountNumber = currentMilestone.target;
                    }
                }
            }
            else if (this.ProgressBarType == ProgressBarTypeEnum.Custom)
            {
                if (!string.IsNullOrEmpty(this.CurrentAmountCustom))
                {
                    string customAmount = await this.ReplaceStringWithSpecialModifiers(this.CurrentAmountCustom, user, arguments, extraSpecialIdentifiers);
                    double.TryParse(customAmount, out amount);
                }
                if (!string.IsNullOrEmpty(this.GoalAmountCustom))
                {
                    string customGoal = await this.ReplaceStringWithSpecialModifiers(this.GoalAmountCustom, user, arguments, extraSpecialIdentifiers);
                    double.TryParse(customGoal, out goal);
                }
            }

            double percentage = (amount / goal);

            if (!this.GoalReached && percentage >= 1.0)
            {
                this.GoalReached = true;
                if (this.GoalReachedCommand != null)
                {
                    await this.GoalReachedCommand.Perform();
                }
            }

            replacementSets["AMOUNT"] = amount.ToString();
            replacementSets["GOAL"] = goal.ToString();
            replacementSets["PERCENTAGE"] = ((int)(percentage * 100)).ToString();
            if (goal > 0)
            {
                int progressWidth = (int)(((double)this.Width) * percentage);
                progressWidth = MathHelper.Clamp(progressWidth, 0, this.Width);
                replacementSets["PROGRESS_WIDTH"] = progressWidth.ToString();
            }

            return replacementSets;
        }

        private void GlobalEvents_OnFollowOccurred(object sender, UserViewModel user) { this.totalFollowers++; }

        private void GlobalEvents_OnUnfollowOccurred(object sender, UserViewModel user) { this.totalFollowers--; }

        private void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user) { this.CurrentAmountNumber++; }

        private void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user) { this.CurrentAmountNumber++; }

        private void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation) { this.CurrentAmountNumber += donation.Amount; }

        private void GlobalEvents_OnSparkUseOccurred(object sender, Tuple<UserViewModel, int> user) { this.CurrentAmountNumber += user.Item2; }

        private void GlobalEvents_OnEmberUseOccurred(object sender, UserEmberUsageModel emberUsage) { this.CurrentAmountNumber += emberUsage.Amount; }

        private void GlobalEvents_OnPatronageUpdateOccurred(object sender, PatronageStatusModel patronageStatus) { this.CurrentAmountNumber = patronageStatus.patronageEarned; }

        private void GlobalEvents_OnPatronageMilestoneReachedOccurred(object sender, PatronageMilestoneModel patronageMilestone) { this.refreshMilestone = true; }
    }
}
