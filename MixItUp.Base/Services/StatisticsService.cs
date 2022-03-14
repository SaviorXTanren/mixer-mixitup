using MixItUp.Base.Model.Statistics;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services.Glimesh;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
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
        public EventStatisticDataTrackerModel RaidsTracker { get; private set; }

        public EventStatisticDataTrackerModel SubscriberTracker { get; private set; }
        public EventStatisticDataTrackerModel ResubscriberTracker { get; private set; }
        public EventStatisticDataTrackerModel GiftedSubscriptionsTracker { get; private set; }
        public StaticTextStatisticDataTrackerModel AllSubsTracker { get; private set; }

        public EventStatisticDataTrackerModel DonationsTracker { get; private set; }
        public EventStatisticDataTrackerModel BitsTracker { get; private set; }

        public StatisticsService() { }

        public void Initialize()
        {
            this.StartTime = DateTimeOffset.Now;

            GlobalEvents.OnFollowOccurred += Constellation_OnFollowOccurred;
            GlobalEvents.OnHostOccurred += Constellation_OnHostedOccurred;
            GlobalEvents.OnRaidOccurred += GlobalEvents_OnRaidOccurred;
            GlobalEvents.OnSubscribeOccurred += Constellation_OnSubscribedOccurred;
            GlobalEvents.OnResubscribeOccurred += Constellation_OnResubscribedOccurred;
            GlobalEvents.OnSubscriptionGiftedOccurred += GlobalEvents_OnSubscriptionGiftedOccurred;

            GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            GlobalEvents.OnBitsOccurred += GlobalEvents_OnBitsOccurred;

            this.ViewerTracker = new TrackedNumberStatisticDataTrackerModel(Resources.Viewers, "Eye", (StatisticDataTrackerModelBase stats) =>
            {
                TrackedNumberStatisticDataTrackerModel numberStats = (TrackedNumberStatisticDataTrackerModel)stats;

                int viewersCurrent = 0;
                if (ServiceManager.Get<TwitchSessionService>().IsLive)
                {
                    viewersCurrent = (int)ServiceManager.Get<TwitchSessionService>().Stream.viewer_count;
                }
                else if (ServiceManager.Get<GlimeshSessionService>().User?.channel?.stream != null)
                {
                    viewersCurrent = (int)ServiceManager.Get<GlimeshSessionService>().User?.channel?.stream?.countViewers;
                }

                numberStats.AddValue(viewersCurrent);
                return Task.CompletedTask;
            });

            this.ChatterTracker = new TrackedNumberStatisticDataTrackerModel(Resources.Chatters, "MessageTextOutline", (StatisticDataTrackerModelBase stats) =>
            {
                TrackedNumberStatisticDataTrackerModel numberStats = (TrackedNumberStatisticDataTrackerModel)stats;
                numberStats.AddValue(ServiceManager.Get<UserService>().GetActiveUserCount());
                return Task.CompletedTask;
            });

            this.FollowTracker = new EventStatisticDataTrackerModel(Resources.Follows, "AccountPlus", new List<string>() { "Username", "Date & Time" });
            this.HostsTracker = new EventStatisticDataTrackerModel(Resources.Hosts, "AccountNetwork", new List<string>() { "Username", "Date & Time" });

            this.RaidsTracker = new EventStatisticDataTrackerModel(Resources.Raids, "AccountMultipleMinus", new List<string>() { "Username", "Viewers", "Date & Time" }, (EventStatisticDataTrackerModel dataTracker) =>
            {
                return $"{MixItUp.Base.Resources.Raids}: {dataTracker.UniqueIdentifiers},    {MixItUp.Base.Resources.TotalViewers}: {dataTracker.TotalValue},    {MixItUp.Base.Resources.AverageViewers}: {dataTracker.AverageValueString}";
            });

            this.SubscriberTracker = new EventStatisticDataTrackerModel(Resources.Subscribes, "AccountStar", new List<string>() { "Username", "Date & Time" });
            this.ResubscriberTracker = new EventStatisticDataTrackerModel(Resources.Resubscribes, "AccountSettings", new List<string>() { "Username", "Date & Time" });
            this.GiftedSubscriptionsTracker = new EventStatisticDataTrackerModel(Resources.GiftedSubs, "AccountHeart", new List<string>() { "Gifter", "Receiver", });

            this.AllSubsTracker = new StaticTextStatisticDataTrackerModel(Resources.Subscribers, "AccountStar", (StatisticDataTrackerModelBase stats) =>
            {
                StaticTextStatisticDataTrackerModel staticStats = (StaticTextStatisticDataTrackerModel)stats;
                staticStats.ClearValues();

                staticStats.AddValue(Resources.Subs, ServiceManager.Get<StatisticsService>()?.SubscriberTracker?.Total.ToString() ?? "0");
                staticStats.AddValue(Resources.Resubs, ServiceManager.Get<StatisticsService>()?.ResubscriberTracker?.Total.ToString() ?? "0");
                staticStats.AddValue(Resources.Gifted, ServiceManager.Get<StatisticsService>()?.GiftedSubscriptionsTracker?.Total.ToString() ?? "0");

                return Task.CompletedTask;
            });

            this.DonationsTracker = new EventStatisticDataTrackerModel(Resources.Donations, "CashMultiple", new List<string>() { "Username", "Amount", "Date & Time" }, (EventStatisticDataTrackerModel dataTracker) =>
            {
                return $"{Resources.Donators}: {dataTracker.UniqueIdentifiers},    {Resources.Total}: {dataTracker.TotalValueDecimal:C},    {Resources.Average}: {dataTracker.AverageValueString:C}";
            });

            this.BitsTracker = new EventStatisticDataTrackerModel(Resources.Bits, "Decagram", new List<string>() { "Username", "Amount", "Date & Time" }, (EventStatisticDataTrackerModel dataTracker) =>
            {
                return $"{Resources.Users}: {dataTracker.UniqueIdentifiers},    {Resources.Total}: {dataTracker.TotalValue},    {Resources.Average}: {dataTracker.AverageValueString}";
            });

            this.Statistics = new List<StatisticDataTrackerModelBase>();
            this.Statistics.Add(this.ViewerTracker);
            this.Statistics.Add(this.ChatterTracker);
            this.Statistics.Add(this.FollowTracker);
            this.Statistics.Add(this.HostsTracker);
            this.Statistics.Add(this.RaidsTracker);
            this.Statistics.Add(this.SubscriberTracker);
            this.Statistics.Add(this.ResubscriberTracker);
            this.Statistics.Add(this.GiftedSubscriptionsTracker);
            this.Statistics.Add(this.DonationsTracker);
            this.Statistics.Add(this.BitsTracker);
        }

        private void Constellation_OnFollowOccurred(object sender, UserV2ViewModel e)
        {
            this.FollowTracker.OnStatisticEventOccurred(e.Username);
        }

        private void Constellation_OnUnfollowOccurred(object sender, UserV2ViewModel e)
        {
            this.UnfollowTracker.OnStatisticEventOccurred(e.Username);
        }

        private void Constellation_OnHostedOccurred(object sender, UserV2ViewModel e)
        {
            this.HostsTracker.OnStatisticEventOccurred(e.Username);
        }

        private void GlobalEvents_OnRaidOccurred(object sender, Tuple<UserV2ViewModel, int> e)
        {
            this.RaidsTracker.OnStatisticEventOccurred(e.Item1.Username, e.Item2);
        }

        private void Constellation_OnSubscribedOccurred(object sender, UserV2ViewModel e)
        {
            this.SubscriberTracker.OnStatisticEventOccurred(e.Username);
        }

        private void Constellation_OnResubscribedOccurred(object sender, Tuple<UserV2ViewModel, int> e)
        {
            this.ResubscriberTracker.OnStatisticEventOccurred(e.Item1.Username);
        }

        private void GlobalEvents_OnSubscriptionGiftedOccurred(object sender, Tuple<UserV2ViewModel, UserV2ViewModel> e)
        {
            this.GiftedSubscriptionsTracker.OnStatisticEventOccurred(e.Item1.Username, e.Item2.Username);
        }

        private void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel e)
        {
            this.DonationsTracker.OnStatisticEventOccurred(e.Username, e.Amount);
        }

        private void GlobalEvents_OnBitsOccurred(object sender, TwitchUserBitsCheeredModel e)
        {
            this.BitsTracker.OnStatisticEventOccurred(e.User.Username, e.Amount);
        }
    }
}