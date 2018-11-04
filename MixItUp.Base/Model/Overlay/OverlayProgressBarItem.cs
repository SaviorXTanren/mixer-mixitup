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
    public class OverlayProgressBarItem : OverlayItemBase
    {
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

        [DataMember]
        public double CurrentAmount
        {
            get
            {
                double amount = this.CurrentAmountNumber;
                if (!string.IsNullOrEmpty(this.CurrentAmountCustom))
                {
                    double.TryParse(this.CurrentAmountCustom, out amount);
                }
                return amount;
            }
            set { }
        }

        [DataMember]
        public double GoalAmount
        {
            get
            {
                double amount = this.GoalAmountNumber;
                if (!string.IsNullOrEmpty(this.GoalAmountCustom))
                {
                    double.TryParse(this.GoalAmountCustom, out amount);
                }
                return amount;
            }
            set { }
        }

        [DataMember]
        public int ProgressWidth { get { return (int)(((double)this.Width) * (this.CurrentAmount / this.GoalAmount)); } set { } }

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

            OverlayProgressBarItem item = this.Copy<OverlayProgressBarItem>();
            if (this.ProgressBarType == ProgressBarTypeEnum.Followers)
            {
                item.CurrentAmountNumber = this.totalFollowers;
                if (this.CurrentAmountNumber >= 0)
                {
                    item.CurrentAmountNumber = this.totalFollowers - this.CurrentAmountNumber;
                }
            }
            else if (this.ProgressBarType == ProgressBarTypeEnum.Custom)
            {
                if (!string.IsNullOrEmpty(item.CurrentAmountCustom))
                {
                    item.CurrentAmountCustom = await this.ReplaceStringWithSpecialModifiers(item.CurrentAmountCustom, user, arguments, extraSpecialIdentifiers);
                }
                if (!string.IsNullOrEmpty(item.GoalAmountCustom))
                {
                    item.GoalAmountCustom = await this.ReplaceStringWithSpecialModifiers(item.GoalAmountCustom, user, arguments, extraSpecialIdentifiers);
                }
            }
            return item;
        }

        private void GlobalEvents_OnFollowOccurred(object sender, UserViewModel user) { this.totalFollowers++; }

        private void GlobalEvents_OnUnfollowOccurred(object sender, UserViewModel user) { this.totalFollowers--; }

        private void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user) { this.CurrentAmountNumber++; }

        private void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user) { this.CurrentAmountNumber++; }

        private void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation) { this.CurrentAmountNumber += donation.Amount; }

        private void GlobalEvents_OnSparksReceived(object sender, Tuple<UserViewModel, int> user) { this.CurrentAmountNumber += user.Item2; }
    }
}
