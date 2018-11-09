using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
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
        Custom
    }

    [DataContract]
    public class OverlayProgressBarItem : OverlayCustomHTMLItem
    {
        public const string HTMLTemplate =
            @"<div style=""position: absolute; background-color: {BACKGROUND_COLOR}; width: {BAR_WIDTH}px; height: {BAR_HEIGHT}px; transform: translate(-50%, -50%);"">
    <div style=""position: absolute; background-color: {PROGRESS_COLOR}; width: {PROGRESS_WIDTH}px; height: {BAR_HEIGHT}px;""></div>
</div>
<p style=""position: absolute; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(-50%, -50%);"">{AMOUNT}</p>";

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
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        private DateTimeOffset LastReset { get; set; }

        private int totalFollowers = 0;

        public OverlayProgressBarItem() { }

        public OverlayProgressBarItem(ProgressBarTypeEnum progressBarType, double currentAmount, double goalAmount, int resetAfterDays, string progressColor,
            string backgroundColor, string textColor, int width, int height)
            : this(progressBarType, resetAfterDays, progressColor, backgroundColor, textColor, width, height)
        {
            this.CurrentAmountNumber = currentAmount;
            this.GoalAmountNumber = goalAmount;
        }

        public OverlayProgressBarItem(ProgressBarTypeEnum progressBarType, string currentAmount, string goalAmount, int resetAfterDays, string progressColor,
            string backgroundColor, string textColor, int width, int height)
            : this(progressBarType, resetAfterDays, progressColor, backgroundColor, textColor, width, height)
        {
            this.CurrentAmountCustom = currentAmount;
            this.GoalAmountCustom = goalAmount;
        }

        private OverlayProgressBarItem(ProgressBarTypeEnum progressBarType, int resetAfterDays, string progressColor, string backgroundColor, string textColor, int width, int height)
            : base(OverlayProgressBarItem.HTMLTemplate)
        {
            this.ProgressBarType = progressBarType;
            this.ResetAfterDays = resetAfterDays;
            this.ProgressColor = progressColor;
            this.BackgroundColor = backgroundColor;
            this.TextColor = textColor;
            this.Width = width;
            this.Height = height;
            this.LastReset = DateTimeOffset.Now;
        }

        public override async Task Initialize()
        {
            totalFollowers = (int)ChannelSession.Channel.numFollowers;

            GlobalEvents.OnFollowOccurred -= GlobalEvents_OnFollowOccurred;
            GlobalEvents.OnUnfollowOccurred -= GlobalEvents_OnUnfollowOccurred;
            GlobalEvents.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            GlobalEvents.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            GlobalEvents.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;
            GlobalEvents.OnSparksReceived -= GlobalEvents_OnSparksReceived;

            if (this.ProgressBarType == ProgressBarTypeEnum.Followers)
            {
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
                GlobalEvents.OnSparksReceived += GlobalEvents_OnSparksReceived;
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

        protected override async Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["BACKGROUND_COLOR"] = this.BackgroundColor;
            replacementSets["PROGRESS_COLOR"] = this.ProgressColor;
            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["BAR_WIDTH"] = this.Width.ToString();
            replacementSets["BAR_HEIGHT"] = this.Height.ToString();
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

            replacementSets["AMOUNT"] = amount.ToString();
            replacementSets["GOAL"] = goal.ToString();
            replacementSets["PROGRESS_WIDTH"] = ((int)(((double)this.Width) * (amount / goal))).ToString();

            return replacementSets;
        }

        private void GlobalEvents_OnFollowOccurred(object sender, UserViewModel user) { this.totalFollowers++; }

        private void GlobalEvents_OnUnfollowOccurred(object sender, UserViewModel user) { this.totalFollowers--; }

        private void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user) { this.CurrentAmountNumber++; }

        private void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user) { this.CurrentAmountNumber++; }

        private void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation) { this.CurrentAmountNumber += donation.Amount; }

        private void GlobalEvents_OnSparksReceived(object sender, Tuple<UserViewModel, int> user) { this.CurrentAmountNumber += user.Item2; }
    }
}
