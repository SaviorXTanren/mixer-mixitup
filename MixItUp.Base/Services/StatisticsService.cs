using Mixer.Base.Model.Patronage;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.Model.Statistics;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public class StatisticsService
    {
        public List<StatisticDataTrackerModelBase> Statistics { get; private set; }

        public DateTimeOffset StartTime { get; private set; }

        public TrackedNumberStatisticDataTrackerModel ViewerTracker { get; private set; }
        public TrackedNumberStatisticDataTrackerModel ChatterTracker { get; private set; }

        public EventStatisticDataTrackerModel FollowTracker { get; private set; }
        public EventStatisticDataTrackerModel UnfollowTracker { get; private set; }
        public EventStatisticDataTrackerModel HostsTracker { get; private set; }

        public EventStatisticDataTrackerModel SubscriberTracker { get; private set; }
        public EventStatisticDataTrackerModel ResubscriberTracker { get; private set; }
        public EventStatisticDataTrackerModel GiftedSubscriptionsTracker { get; private set; }
        public StaticTextStatisticDataTrackerModel AllSubsTracker { get; private set; }

        public EventStatisticDataTrackerModel MixPlayTracker { get; private set; }
        public EventStatisticDataTrackerModel SparksTracker { get; private set; }
        public EventStatisticDataTrackerModel EmbersTracker { get; private set; }
        public StaticTextStatisticDataTrackerModel MilestoneTracker { get; private set; }
        public StaticTextStatisticDataTrackerModel SparksEmbersTracker { get; private set; }

        public EventStatisticDataTrackerModel DonationsTracker { get; private set; }

        public StatisticsService() { }

        public void Initialize()
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

            this.ViewerTracker = new TrackedNumberStatisticDataTrackerModel("Viewers", "EyeOutline", true, (StatisticDataTrackerModelBase stats) =>
            {
                TrackedNumberStatisticDataTrackerModel numberStats = (TrackedNumberStatisticDataTrackerModel)stats;

                int viewersCurrent = 0;
                if (ChannelSession.MixerChannel != null)
                {
                    viewersCurrent = (int)ChannelSession.MixerChannel.viewersCurrent;
                }

                numberStats.AddValue(viewersCurrent);
                return Task.FromResult(0);
            });

            this.ChatterTracker = new TrackedNumberStatisticDataTrackerModel("Chatters", "MessageTextOutline", true, (StatisticDataTrackerModelBase stats) =>
            {
                if (ChannelSession.Services.User != null)
                {
                    TrackedNumberStatisticDataTrackerModel numberStats = (TrackedNumberStatisticDataTrackerModel)stats;
                    numberStats.AddValue(ChannelSession.Services.User.Count());
                }
                return Task.FromResult(0);
            });

            this.FollowTracker = new EventStatisticDataTrackerModel("Follows", "AccountMultiplePlus", true, new List<string>() { "Username", "Date & Time" });
            this.UnfollowTracker = new EventStatisticDataTrackerModel("Unfollows", "AccountMultipleMinus", true, new List<string>() { "Username", "Date & Time" });

            this.HostsTracker = new EventStatisticDataTrackerModel("Hosts", "AccountSupervisor", true, new List<string>() { "Username", "Viewers", "Date & Time" }, (EventStatisticDataTrackerModel dataTracker) =>
            {
                return string.Format("Hosts: {0},    Total Viewers: {1},    Average Viewers: {2}", dataTracker.UniqueIdentifiers, dataTracker.TotalValue, dataTracker.AverageValueString);
            });

            this.SubscriberTracker = new EventStatisticDataTrackerModel("Subscribes", "AccountStar", true, new List<string>() { "Username", "Date & Time" });
            this.ResubscriberTracker = new EventStatisticDataTrackerModel("Resubscribes", "AccountSettings", true, new List<string>() { "Username", "Date & Time" });
            this.GiftedSubscriptionsTracker = new EventStatisticDataTrackerModel("Gifted Subs", "AccountHeart", true, new List<string>() { "Gifter", "Receiver", });

            this.AllSubsTracker = new StaticTextStatisticDataTrackerModel("Subscribers", "AccountStar", true, (StatisticDataTrackerModelBase stats) =>
            {
                StaticTextStatisticDataTrackerModel staticStats = (StaticTextStatisticDataTrackerModel)stats;
                staticStats.ClearValues();

                if (ChannelSession.Services.Statistics != null)
                {
                    staticStats.AddValue("Subs", ChannelSession.Services.Statistics.SubscriberTracker.Total.ToString());
                    staticStats.AddValue("Resubs", ChannelSession.Services.Statistics.ResubscriberTracker.Total.ToString());
                    staticStats.AddValue("Gifted", ChannelSession.Services.Statistics.GiftedSubscriptionsTracker.Total.ToString());
                }
                else
                {
                    staticStats.AddValue("Subs", "0");
                    staticStats.AddValue("Resubs", "0");
                    staticStats.AddValue("Gifted", "0");
                }

                return Task.FromResult(0);
            });

            this.MixPlayTracker = new EventStatisticDataTrackerModel("MixPlay", "GamepadVariant", true, new List<string>() { "Control Name", "Username", "Date & Time" }, (EventStatisticDataTrackerModel dataTracker) =>
            {
                return string.Format("Total Uses: {0},    Average Uses: {1}", dataTracker.Total, dataTracker.AverageString);
            });

            this.SparksTracker = new EventStatisticDataTrackerModel("Sparks", "/Assets/Images/Sparks.png", false, new List<string>() { "Username", "Amount" }, (EventStatisticDataTrackerModel dataTracker) =>
            {
                return string.Format("Users: {0},    Total: {1},    Average: {2}", dataTracker.UniqueIdentifiers, dataTracker.TotalValue, dataTracker.AverageValueString);
            });
            this.EmbersTracker = new EventStatisticDataTrackerModel("Embers", "/Assets/Images/Embers.png", false, new List<string>() { "Username", "Amount" }, (EventStatisticDataTrackerModel dataTracker) =>
            {
                return string.Format("Users: {0},    Total: {1},    Average: {2}", dataTracker.UniqueIdentifiers, dataTracker.TotalValue, dataTracker.AverageValueString);
            });

            this.MilestoneTracker = new StaticTextStatisticDataTrackerModel("Milestones", "DiamondStone", true, async (StatisticDataTrackerModelBase stats) =>
            {
                StaticTextStatisticDataTrackerModel staticStats = (StaticTextStatisticDataTrackerModel)stats;
                staticStats.ClearValues();

                if (ChannelSession.MixerUserConnection != null && ChannelSession.MixerChannel != null)
                {
                    PatronageStatusModel patronageStatus = await ChannelSession.MixerUserConnection.GetPatronageStatus(ChannelSession.MixerChannel);
                    if (patronageStatus != null)
                    {
                        PatronagePeriodModel patronagePeriod = await ChannelSession.MixerUserConnection.GetPatronagePeriod(patronageStatus);
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

            this.SparksEmbersTracker = new StaticTextStatisticDataTrackerModel("Sparks/Embers", "DiamondStone", true, (StatisticDataTrackerModelBase stats) =>
            {
                StaticTextStatisticDataTrackerModel staticStats = (StaticTextStatisticDataTrackerModel)stats;
                staticStats.ClearValues();

                if (ChannelSession.Services.Statistics != null)
                {
                    staticStats.AddValue("Sparks", ChannelSession.Services.Statistics.SparksTracker.TotalValue.ToString());
                    staticStats.AddValue("Embers", ChannelSession.Services.Statistics.EmbersTracker.TotalValue.ToString());
                }
                else
                {
                    staticStats.AddValue("Sparks", "0");
                    staticStats.AddValue("Embers", "0");
                }

                return Task.FromResult(0);
            });

            this.DonationsTracker = new EventStatisticDataTrackerModel("Donations", "CashMultiple", true, new List<string>() { "Username", "Amount", "Date & Time" }, (EventStatisticDataTrackerModel dataTracker) =>
            {
                return string.Format("Donators: {0},    Total: {1:C},    Average: {2:C}", dataTracker.UniqueIdentifiers, dataTracker.TotalValueDecimal, dataTracker.AverageValueString);
            });

            this.Statistics = new List<StatisticDataTrackerModelBase>();
            this.Statistics.Add(this.ViewerTracker);
            this.Statistics.Add(this.ChatterTracker);
            this.Statistics.Add(this.FollowTracker);
            this.Statistics.Add(this.UnfollowTracker);
            this.Statistics.Add(this.HostsTracker);
            this.Statistics.Add(this.SubscriberTracker);
            this.Statistics.Add(this.ResubscriberTracker);
            this.Statistics.Add(this.GiftedSubscriptionsTracker);
            this.Statistics.Add(this.MixPlayTracker);
            this.Statistics.Add(this.SparksTracker);
            this.Statistics.Add(this.EmbersTracker);
            this.Statistics.Add(this.MilestoneTracker);
            this.Statistics.Add(this.DonationsTracker);
        }

        private void Constellation_OnFollowOccurred(object sender, UserViewModel e)
        {
            this.FollowTracker.OnStatisticEventOccurred(e.Username);
        }

        private void Constellation_OnUnfollowOccurred(object sender, UserViewModel e)
        {
            this.UnfollowTracker.OnStatisticEventOccurred(e.Username);
        }

        private void Constellation_OnHostedOccurred(object sender, Tuple<UserViewModel, int> e)
        {
            this.HostsTracker.OnStatisticEventOccurred(e.Item1.Username, e.Item2);
        }

        private void Constellation_OnSubscribedOccurred(object sender, UserViewModel e)
        {
            this.SubscriberTracker.OnStatisticEventOccurred(e.Username);
        }

        private void Constellation_OnResubscribedOccurred(object sender, Tuple<UserViewModel, int> e)
        {
            this.ResubscriberTracker.OnStatisticEventOccurred(e.Item1.Username);
        }

        private void GlobalEvents_OnSubscriptionGiftedOccurred(object sender, Tuple<UserViewModel, UserViewModel> e)
        {
            this.GiftedSubscriptionsTracker.OnStatisticEventOccurred(e.Item1.Username, e.Item2.Username);
        }

        private void Interactive_OnInteractiveControlUsed(object sender, InteractiveInputEvent e)
        {
            if (e.Command != null && e.User != null)
            {
                this.MixPlayTracker.OnStatisticEventOccurred(e.Command.Name, e.User.Username);
            }
        }

        private void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel e)
        {
            this.DonationsTracker.OnStatisticEventOccurred(e.ID, e.Amount);
        }

        private void GlobalEvents_OnSparkUseOccurred(object sender, Tuple<UserViewModel, uint> e)
        {
            this.SparksTracker.OnStatisticEventOccurred(e.Item1.Username, e.Item2);
        }

        private void GlobalEvents_OnEmberUseOccurred(object sender, UserEmberUsageModel e)
        {
            this.EmbersTracker.OnStatisticEventOccurred(e.User.Username, e.Amount);
        }
    }
}