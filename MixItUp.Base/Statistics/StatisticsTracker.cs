using Mixer.Base.Model.Chat;
using Mixer.Base.Model.Patronage;
using Mixer.Base.Model.Skills;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Statistics
{
    public class StatisticsTracker
    {
        public List<StatisticDataTrackerBase> Statistics { get; private set; }

        public DateTimeOffset StartTime { get; private set; }

        private EventStatisticDataTracker followTracker = new EventStatisticDataTracker("Follows", "AccountPlus", new List<string>() { "Username", "Date & Time" });
        private EventStatisticDataTracker unfollowTracker = new EventStatisticDataTracker("Unfollows", "AccountMinus", new List<string>() { "Username", "Date & Time" });
        private EventStatisticDataTracker hostsTracker = new EventStatisticDataTracker("Hosts", "AccountMultiple", new List<string>() { "Username", "Viewers", "Date & Time" }, (EventStatisticDataTracker dataTracker) =>
        {
            return string.Format("Hosts: {0},    Total Host Viewers: {1},    Average Host Viewers: {2}", dataTracker.UniqueIdentifiers, dataTracker.TotalValue, dataTracker.AverageValueString);
        });
        private EventStatisticDataTracker subscriberTracker = new EventStatisticDataTracker("Subscribes", "AccountStar", new List<string>() { "Username", "Date & Time" });
        private EventStatisticDataTracker resubscriberTracker = new EventStatisticDataTracker("Resubscribes", "AccountConvert", new List<string>() { "Username", "Date & Time" });

        private EventStatisticDataTracker interactiveTracker = new EventStatisticDataTracker("Interactive", "GamepadVariant", new List<string>() { "Control Name", "Username", "Date & Time" }, (EventStatisticDataTracker dataTracker) =>
        {
            return string.Format("Total Uses: {0},    Average Uses: {1}", dataTracker.Total, dataTracker.AverageString);
        });

        private EventStatisticDataTracker donationsTracker = new EventStatisticDataTracker("Donations", "CashMultiple", new List<string>() { "Username", "Amount", "Date & Time" }, (EventStatisticDataTracker dataTracker) =>
        {
            return string.Format("Donaters: {0},    Total: {1:C},    Average: {2:C}", dataTracker.UniqueIdentifiers, dataTracker.TotalValueDecimal, dataTracker.AverageValueString);
        });

        private EventStatisticDataTracker sparksTracker = new EventStatisticDataTracker("Sparks", "WeatherLightning", new List<string>() { "Username", "Amount" }, (EventStatisticDataTracker dataTracker) =>
        {
            return string.Format("Users: {0},    Total: {1},    Average: {2}", dataTracker.UniqueIdentifiers, dataTracker.TotalValue, dataTracker.AverageValueString);
        });

        public StatisticsTracker()
        {
            this.StartTime = DateTimeOffset.Now;

            ChannelSession.Constellation.OnFollowOccurred += Constellation_OnFollowOccurred;
            ChannelSession.Constellation.OnUnfollowOccurred += Constellation_OnUnfollowOccurred;
            ChannelSession.Constellation.OnHostedOccurred += Constellation_OnHostedOccurred;
            ChannelSession.Constellation.OnSubscribedOccurred += Constellation_OnSubscribedOccurred;
            ChannelSession.Constellation.OnResubscribedOccurred += Constellation_OnResubscribedOccurred;

            ChannelSession.Interactive.OnInteractiveControlUsed += Interactive_OnInteractiveControlUsed;

            GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;

            GlobalEvents.OnSparkUseOccurred += GlobalEvents_OnSparkUseOccurred;
            GlobalEvents.OnChatSkillOccurred += GlobalEvents_OnChatSkillOccurred;
            GlobalEvents.OnSkillOccurred += GlobalEvents_OnSkillOccurred;

            this.Statistics = new List<StatisticDataTrackerBase>();

            this.Statistics.Add(new TrackedNumberStatisticDataTracker("Viewers", "Eye", (StatisticDataTrackerBase stats) =>
            {
                TrackedNumberStatisticDataTracker numberStats = (TrackedNumberStatisticDataTracker)stats;

                int viewersCurrent = 0;
                if (ChannelSession.Channel != null)
                {
                    viewersCurrent = (int)ChannelSession.Channel.viewersCurrent;
                }

                numberStats.AddValue(viewersCurrent);
                return Task.FromResult(0);
            }));

            this.Statistics.Add(new TrackedNumberStatisticDataTracker("Chatters", "MessageTextOutline", async (StatisticDataTrackerBase stats) =>
            {
                TrackedNumberStatisticDataTracker numberStats = (TrackedNumberStatisticDataTracker)stats;
                numberStats.AddValue(await ChannelSession.ActiveUsers.Count());
            }));

            this.Statistics.Add(this.followTracker);
            this.Statistics.Add(this.unfollowTracker);
            this.Statistics.Add(this.hostsTracker);
            this.Statistics.Add(this.subscriberTracker);
            this.Statistics.Add(this.resubscriberTracker);

            this.Statistics.Add(this.interactiveTracker);
            this.Statistics.Add(this.sparksTracker);
            this.Statistics.Add(new StaticTextStatisticDataTracker("Milestones", "Diamond", async (StatisticDataTrackerBase stats) =>
            {
                StaticTextStatisticDataTracker staticStats = (StaticTextStatisticDataTracker)stats;
                staticStats.ClearValues();

                if (ChannelSession.Connection != null && ChannelSession.Channel != null)
                {
                    PatronageStatusModel patronageStatus = await ChannelSession.Connection.GetPatronageStatus(ChannelSession.Channel);
                    if (patronageStatus != null)
                    {
                        PatronagePeriodModel patronagePeriod = await ChannelSession.Connection.GetPatronagePeriod(patronageStatus);
                        if (patronagePeriod != null)
                        {
                            IEnumerable<PatronageMilestoneModel> patronageMilestones = patronagePeriod.milestoneGroups.SelectMany(mg => mg.milestones);
                            IEnumerable<PatronageMilestoneModel> patronageMilestonesEarned = patronageMilestones.Where(m => m.target <= patronageStatus.patronageEarned);
                            long patronageEarnedReward = patronageMilestonesEarned.Max(m => m.reward);
                            double patronageEarnedRewardDollars = Math.Round(((double)patronageEarnedReward) / 100.0, 2);

                            staticStats.AddValue("Milestone #", patronageStatus.currentMilestoneId.ToString());
                            staticStats.AddValue("Total Sparks", patronageStatus.patronageEarned.ToString());
                            staticStats.AddValue("Total Payout", string.Format("{0:C}", patronageEarnedRewardDollars));
                            return;
                        }
                    }
                }

                staticStats.AddValue("Milestone #", "0");
                staticStats.AddValue("Total Sparks", "0");
                staticStats.AddValue("Total Payout", "$0.00");
            }));

            this.Statistics.Add(this.donationsTracker);
        }

        private void Constellation_OnFollowOccurred(object sender, UserViewModel e)
        {
            this.followTracker.OnStatisticEventOccurred(e.UserName);
        }

        private void Constellation_OnUnfollowOccurred(object sender, UserViewModel e)
        {
            this.unfollowTracker.OnStatisticEventOccurred(e.UserName);
        }

        private void Constellation_OnHostedOccurred(object sender, Tuple<UserViewModel, int> e)
        {
            this.hostsTracker.OnStatisticEventOccurred(e.Item1.UserName, e.Item2);
        }

        private void Constellation_OnSubscribedOccurred(object sender, UserViewModel e)
        {
            this.subscriberTracker.OnStatisticEventOccurred(e.UserName);
        }

        private void Constellation_OnResubscribedOccurred(object sender, Tuple<UserViewModel, int> e)
        {
            this.resubscriberTracker.OnStatisticEventOccurred(e.Item1.UserName);
        }

        private void Interactive_OnInteractiveControlUsed(object sender, InteractiveInputEvent e)
        {
            if (e.Command != null && e.User != null)
            {
                this.interactiveTracker.OnStatisticEventOccurred(e.Command.Name, e.User.UserName);
            }
        }

        private void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel e)
        {
            this.donationsTracker.OnStatisticEventOccurred(e.ID, e.Amount);
        }

        private void GlobalEvents_OnSparkUseOccurred(object sender, Tuple<UserViewModel, int> e)
        {
            this.sparksTracker.OnStatisticEventOccurred(e.Item1.UserName, e.Item2);
        }

        private void GlobalEvents_OnChatSkillOccurred(object sender, Tuple<UserViewModel, ChatSkillModel> e)
        {
            this.sparksTracker.OnStatisticEventOccurred(e.Item1.UserName, e.Item2.cost);
        }

        private void GlobalEvents_OnSkillOccurred(object sender, Tuple<UserViewModel, SkillModel> e)
        {
            this.sparksTracker.OnStatisticEventOccurred(e.Item1.UserName, e.Item2.price);
        }
    }
}
