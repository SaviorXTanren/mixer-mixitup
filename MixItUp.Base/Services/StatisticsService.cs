using Mixer.Base.Model.Patronage;
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

            GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;

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

                staticStats.AddValue(Resources.Subs, ChannelSession.Services.Statistics?.SubscriberTracker?.Total.ToString() ?? "0");
                staticStats.AddValue(Resources.Resubs, ChannelSession.Services.Statistics?.ResubscriberTracker?.Total.ToString() ?? "0");
                staticStats.AddValue(Resources.Gifted, ChannelSession.Services.Statistics?.GiftedSubscriptionsTracker?.Total.ToString() ?? "0");

                return Task.FromResult(0);
            });

            this.DonationsTracker = new EventStatisticDataTrackerModel("Donations", "CashMultiple", true, new List<string>() { "Username", "Amount", "Date & Time" }, (EventStatisticDataTrackerModel dataTracker) =>
            {
                return $"{Resources.Donators}: {dataTracker.UniqueIdentifiers},    {Resources.Total}: {dataTracker.TotalValueDecimal:C},    {Resources.Average}: {dataTracker.AverageValueString:C}";
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

        private void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel e)
        {
            this.DonationsTracker.OnStatisticEventOccurred(e.ID, e.Amount);
        }
    }
}