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
    [DataContract]
    public class OverlayTimerTrain : OverlayCustomHTMLItem
    {
        public const string HTMLTemplate =
            @"<p style=""position: absolute; font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(-50%, -50%);"">{TIME}</p>";

        public const string TimerTrainItemType = "timertrain";

        [DataMember]
        public int MinimumSecondsToShow { get; set; }
        [DataMember]
        public string TextColor { get; set; }
        [DataMember]
        public string TextFont { get; set; }
        [DataMember]
        public int TextSize { get; set; }

        [DataMember]
        public double FollowBonus { get; set; }
        [DataMember]
        public double HostBonus { get; set; }
        [DataMember]
        public double SubscriberBonus { get; set; }
        [DataMember]
        public double DonationBonus { get; set; }
        [DataMember]
        public double SparkBonus { get; set; }

        [DataMember]
        public double SecondsToAdd { get; set; }

        private HashSet<uint> follows = new HashSet<uint>();
        private HashSet<uint> hosts = new HashSet<uint>();
        private HashSet<uint> subs = new HashSet<uint>();

        public OverlayTimerTrain() : base(TimerTrainItemType, HTMLTemplate) { }

        public OverlayTimerTrain(string htmlText, int minimumSecondsToShow, string textColor, string textFont, int textSize, double followBonus,
            double hostBonus, double subscriberBonus, double donationBonus, double sparkBonus)
            : base(TimerTrainItemType, htmlText)
        {
            this.MinimumSecondsToShow = minimumSecondsToShow;
            this.TextColor = textColor;
            this.TextFont = textFont;
            this.TextSize = textSize;
            this.FollowBonus = followBonus;
            this.HostBonus = hostBonus;
            this.SubscriberBonus = subscriberBonus;
            this.DonationBonus = donationBonus;
            this.SparkBonus = sparkBonus;
        }

        [JsonIgnore]
        public override bool SupportsTestButton { get { return true; } }

        public override async Task LoadTestData()
        {
            this.SecondsToAdd = (double)this.MinimumSecondsToShow * 1.5;
            await Task.Delay((int)this.SecondsToAdd * 1000);
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnFollowOccurred -= GlobalEvents_OnFollowOccurred;
            GlobalEvents.OnHostOccurred -= GlobalEvents_OnHostOccurred;
            GlobalEvents.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            GlobalEvents.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            GlobalEvents.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;
            GlobalEvents.OnSparkUseOccurred -= GlobalEvents_OnSparkUseOccurred;

            if (this.FollowBonus > 0.0)
            {
                GlobalEvents.OnFollowOccurred += GlobalEvents_OnFollowOccurred;
            }
            if (this.HostBonus > 0.0)
            {
                GlobalEvents.OnHostOccurred += GlobalEvents_OnHostOccurred;
            }
            if (this.SubscriberBonus > 0.0)
            {
                GlobalEvents.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
                GlobalEvents.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
            }
            if (this.DonationBonus > 0.0)
            {
                GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }
            if (this.SparkBonus > 0.0)
            {
                GlobalEvents.OnSparkUseOccurred += GlobalEvents_OnSparkUseOccurred;
            }

            await base.Initialize();
        }

        public override OverlayCustomHTMLItem GetCopy() { return this.Copy<OverlayTimerTrain>(); }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            if (this.SecondsToAdd >= this.MinimumSecondsToShow)
            {
                OverlayTimerTrain copy = (OverlayTimerTrain)await base.GetProcessedItem(user, arguments, extraSpecialIdentifiers);
                this.SecondsToAdd = 0;
                return copy;
            }
            return null;
        }

        protected override Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["TEXT_SIZE"] = this.TextSize.ToString();

            return Task.FromResult(replacementSets);
        }

        private void GlobalEvents_OnFollowOccurred(object sender, UserViewModel user)
        {
            if (!this.follows.Contains(user.ID))
            {
                this.follows.Add(user.ID);
                this.SecondsToAdd += this.FollowBonus;
            }
        }

        private void GlobalEvents_OnHostOccurred(object sender, Tuple<UserViewModel, int> host)
        {
            if (!this.hosts.Contains(host.Item1.ID))
            {
                this.hosts.Add(host.Item1.ID);
                this.SecondsToAdd += (Math.Max(host.Item2, 1) * this.HostBonus);
            }
        }

        private void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user)
        {
            if (!this.subs.Contains(user.ID))
            {
                this.subs.Add(user.ID);
                this.SecondsToAdd += this.SubscriberBonus;
            }
        }

        private void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user)
        {
            if (!this.subs.Contains(user.Item1.ID))
            {
                this.subs.Add(user.Item1.ID);
                this.SecondsToAdd += this.SubscriberBonus;
            }
        }

        private void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation) { this.SecondsToAdd += (donation.Amount * this.DonationBonus); }

        private void GlobalEvents_OnSparkUseOccurred(object sender, Tuple<UserViewModel, int> sparkUsage) { this.SecondsToAdd += (sparkUsage.Item2 * this.SparkBonus); }
    }
}
