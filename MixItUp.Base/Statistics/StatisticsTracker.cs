using MixItUp.Base.MixerAPI;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
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

        private void Interactive_OnInteractiveControlUsed(object sender, Tuple<UserViewModel, InteractiveConnectedControlCommand> e)
        {
            this.interactiveTracker.OnStatisticEventOccurred(e.Item2.Name, e.Item1.UserName);
        }

        private void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel e)
        {
            this.donationsTracker.OnStatisticEventOccurred(e.ID, e.Amount);
        }
    }
}
