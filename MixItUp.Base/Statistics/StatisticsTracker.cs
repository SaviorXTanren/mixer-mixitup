using Mixer.Base.Model.Patronage;
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

        public TrackedNumberStatisticDataTracker ViewerTracker = new TrackedNumberStatisticDataTracker("Viewers", "EyeOutline", true, (StatisticDataTrackerBase stats) =>
        {
            TrackedNumberStatisticDataTracker numberStats = (TrackedNumberStatisticDataTracker)stats;

            int viewersCurrent = 0;
            if (ChannelSession.MixerChannel != null)
            {
                viewersCurrent = (int)ChannelSession.MixerChannel.viewersCurrent;
            }

            numberStats.AddValue(viewersCurrent);
            return Task.FromResult(0);
        });
        public TrackedNumberStatisticDataTracker ChatterTracker = new TrackedNumberStatisticDataTracker("Chatters", "MessageTextOutline", true, async (StatisticDataTrackerBase stats) =>
        {
            TrackedNumberStatisticDataTracker numberStats = (TrackedNumberStatisticDataTracker)stats;
            numberStats.AddValue(ChannelSession.Services.User.Count());
        });

        public EventStatisticDataTracker FollowTracker = new EventStatisticDataTracker("Follows", "AccountMultiplePlus", true, new List<string>() { "Username", "Date & Time" });
        public EventStatisticDataTracker UnfollowTracker = new EventStatisticDataTracker("Unfollows", "AccountMultipleMinus", true, new List<string>() { "Username", "Date & Time" });
        public EventStatisticDataTracker HostsTracker = new EventStatisticDataTracker("Hosts", "AccountSupervisor", true, new List<string>() { "Username", "Viewers", "Date & Time" }, (EventStatisticDataTracker dataTracker) =>
        {
            return string.Format("Hosts: {0},    Total Viewers: {1},    Average Viewers: {2}", dataTracker.UniqueIdentifiers, dataTracker.TotalValue, dataTracker.AverageValueString);
        });
        public EventStatisticDataTracker SubscriberTracker = new EventStatisticDataTracker("Subscribes", "AccountStar", true, new List<string>() { "Username", "Date & Time" });
        public EventStatisticDataTracker ResubscriberTracker = new EventStatisticDataTracker("Resubscribes", "AccountSettings", true, new List<string>() { "Username", "Date & Time" });
        public EventStatisticDataTracker GiftedSubscriptionsTracker = new EventStatisticDataTracker("Gifted Subs", "AccountHeart", true, new List<string>() { "Gifter", "Receiver", });
        public StaticTextStatisticDataTracker AllSubsTracker = new StaticTextStatisticDataTracker("Subscribers", "AccountStar", true, (StatisticDataTrackerBase stats) =>
        {
            StaticTextStatisticDataTracker staticStats = (StaticTextStatisticDataTracker)stats;
            staticStats.ClearValues();

            if (ChannelSession.Statistics != null)
            {
                staticStats.AddValue("Subs", ChannelSession.Statistics.SubscriberTracker.Total.ToString());
                staticStats.AddValue("Resubs", ChannelSession.Statistics.ResubscriberTracker.Total.ToString());
                staticStats.AddValue("Gifted", ChannelSession.Statistics.GiftedSubscriptionsTracker.Total.ToString());
            }
            else
            {
                staticStats.AddValue("Subs", "0");
                staticStats.AddValue("Resubs", "0");
                staticStats.AddValue("Gifted", "0");
            }

            return Task.FromResult(0);
        });

        public EventStatisticDataTracker InteractiveTracker = new EventStatisticDataTracker("Interactive", "GamepadVariant", true, new List<string>() { "Control Name", "Username", "Date & Time" }, (EventStatisticDataTracker dataTracker) =>
        {
            return string.Format("Total Uses: {0},    Average Uses: {1}", dataTracker.Total, dataTracker.AverageString);
        });

        public EventStatisticDataTracker SparksTracker = new EventStatisticDataTracker("Sparks", "/Assets/Images/Sparks.png", false, new List<string>() { "Username", "Amount" }, (EventStatisticDataTracker dataTracker) =>
        {
            return string.Format("Users: {0},    Total: {1},    Average: {2}", dataTracker.UniqueIdentifiers, dataTracker.TotalValue, dataTracker.AverageValueString);
        });
        public EventStatisticDataTracker EmbersTracker = new EventStatisticDataTracker("Embers", "/Assets/Images/Embers.png", false, new List<string>() { "Username", "Amount" }, (EventStatisticDataTracker dataTracker) =>
        {
            return string.Format("Users: {0},    Total: {1},    Average: {2}", dataTracker.UniqueIdentifiers, dataTracker.TotalValue, dataTracker.AverageValueString);
        });
        public StaticTextStatisticDataTracker MilestoneTracker = new StaticTextStatisticDataTracker("Milestones", "DiamondStone", true, async (StatisticDataTrackerBase stats) =>
        {
            StaticTextStatisticDataTracker staticStats = (StaticTextStatisticDataTracker)stats;
            staticStats.ClearValues();

            if (ChannelSession.MixerStreamerConnection != null && ChannelSession.MixerChannel != null)
            {
                PatronageStatusModel patronageStatus = await ChannelSession.MixerStreamerConnection.GetPatronageStatus(ChannelSession.MixerChannel);
                if (patronageStatus != null)
                {
                    PatronagePeriodModel patronagePeriod = await ChannelSession.MixerStreamerConnection.GetPatronagePeriod(patronageStatus);
                    if (patronagePeriod != null)
                    {
                        IEnumerable<PatronageMilestoneModel> patronageMilestones = patronagePeriod.milestoneGroups.SelectMany(mg => mg.milestones);
                        IEnumerable<PatronageMilestoneModel> patronageMilestonesEarned = patronageMilestones.Where(m => m.target <= patronageStatus.patronageEarned);
                        if (patronageMilestonesEarned.Count() > 0)
                        {
                            PatronageMilestoneModel patronageMilestoneHighestEarned = patronageMilestonesEarned.OrderByDescending(m => m.bonus).FirstOrDefault();
                            if (patronageMilestoneHighestEarned != null)
                            {
                                staticStats.AddValue("Milestone #", patronageStatus.currentMilestoneId.ToString());
                                staticStats.AddValue("Total Sparks", patronageStatus.patronageEarned.ToString());
                                staticStats.AddValue("Total Boost", patronageMilestoneHighestEarned.PercentageAmountText());
                                return;
                            }
                        }
                    }
                }
            }

            staticStats.AddValue("Milestone #", "0");
            staticStats.AddValue("Total Sparks", "0");
            staticStats.AddValue("Total Boost", "0%");
        });
        public StaticTextStatisticDataTracker SparksEmbersTracker = new StaticTextStatisticDataTracker("Sparks/Embers", "DiamondStone", true, (StatisticDataTrackerBase stats) =>
        {
            StaticTextStatisticDataTracker staticStats = (StaticTextStatisticDataTracker)stats;
            staticStats.ClearValues();

            if (ChannelSession.Statistics != null)
            {
                staticStats.AddValue("Sparks", ChannelSession.Statistics.SparksTracker.Total.ToString());
                staticStats.AddValue("Embers", ChannelSession.Statistics.EmbersTracker.Total.ToString());
            }
            else
            {
                staticStats.AddValue("Sparks", "0");
                staticStats.AddValue("Embers", "0");
            }

            return Task.FromResult(0);
        });

        public EventStatisticDataTracker DonationsTracker = new EventStatisticDataTracker("Donations", "CashMultiple", true, new List<string>() { "Username", "Amount", "Date & Time" }, (EventStatisticDataTracker dataTracker) =>
        {
            return string.Format("Donators: {0},    Total: {1:C},    Average: {2:C}", dataTracker.UniqueIdentifiers, dataTracker.TotalValueDecimal, dataTracker.AverageValueString);
        });

        public StatisticsTracker()
        {
            this.StartTime = DateTimeOffset.Now;

            GlobalEvents.OnFollowOccurred += Constellation_OnFollowOccurred;
            GlobalEvents.OnUnfollowOccurred += Constellation_OnUnfollowOccurred;
            GlobalEvents.OnHostOccurred += Constellation_OnHostedOccurred;
            GlobalEvents.OnSubscribeOccurred += Constellation_OnSubscribedOccurred;
            GlobalEvents.OnResubscribeOccurred += Constellation_OnResubscribedOccurred;
            GlobalEvents.OnSubscriptionGiftedOccurred += GlobalEvents_OnSubscriptionGiftedOccurred;

            ChannelSession.Interactive.OnInteractiveControlUsed += Interactive_OnInteractiveControlUsed;

            GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;

            GlobalEvents.OnSparkUseOccurred += GlobalEvents_OnSparkUseOccurred;
            GlobalEvents.OnEmberUseOccurred += GlobalEvents_OnEmberUseOccurred;

            this.Statistics = new List<StatisticDataTrackerBase>();

            this.Statistics.Add(this.ViewerTracker);
            this.Statistics.Add(this.ChatterTracker);
            this.Statistics.Add(this.FollowTracker);
            this.Statistics.Add(this.UnfollowTracker);
            this.Statistics.Add(this.HostsTracker);
            this.Statistics.Add(this.SubscriberTracker);
            this.Statistics.Add(this.ResubscriberTracker);
            this.Statistics.Add(this.GiftedSubscriptionsTracker);
            this.Statistics.Add(this.InteractiveTracker);
            this.Statistics.Add(this.SparksTracker);
            this.Statistics.Add(this.EmbersTracker);
            this.Statistics.Add(this.MilestoneTracker);
            this.Statistics.Add(this.DonationsTracker);
        }

        private void Constellation_OnFollowOccurred(object sender, UserViewModel e)
        {
            this.FollowTracker.OnStatisticEventOccurred(e.UserName);
        }

        private void Constellation_OnUnfollowOccurred(object sender, UserViewModel e)
        {
            this.UnfollowTracker.OnStatisticEventOccurred(e.UserName);
        }

        private void Constellation_OnHostedOccurred(object sender, Tuple<UserViewModel, int> e)
        {
            this.HostsTracker.OnStatisticEventOccurred(e.Item1.UserName, e.Item2);
        }

        private void Constellation_OnSubscribedOccurred(object sender, UserViewModel e)
        {
            this.SubscriberTracker.OnStatisticEventOccurred(e.UserName);
        }

        private void Constellation_OnResubscribedOccurred(object sender, Tuple<UserViewModel, int> e)
        {
            this.ResubscriberTracker.OnStatisticEventOccurred(e.Item1.UserName);
        }

        private void GlobalEvents_OnSubscriptionGiftedOccurred(object sender, Tuple<UserViewModel, UserViewModel> e)
        {
            this.GiftedSubscriptionsTracker.OnStatisticEventOccurred(e.Item1.UserName, e.Item2.UserName);
        }

        private void Interactive_OnInteractiveControlUsed(object sender, InteractiveInputEvent e)
        {
            if (e.Command != null && e.User != null)
            {
                this.InteractiveTracker.OnStatisticEventOccurred(e.Command.Name, e.User.UserName);
            }
        }

        private void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel e)
        {
            this.DonationsTracker.OnStatisticEventOccurred(e.ID, e.Amount);
        }

        private void GlobalEvents_OnSparkUseOccurred(object sender, Tuple<UserViewModel, uint> e)
        {
            this.SparksTracker.OnStatisticEventOccurred(e.Item1.UserName, e.Item2);
        }

        private void GlobalEvents_OnEmberUseOccurred(object sender, UserEmberUsageModel e)
        {
            this.EmbersTracker.OnStatisticEventOccurred(e.User.UserName, e.Amount);
        }
    }
}
