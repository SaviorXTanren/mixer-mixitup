using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Model.User;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Statistics
{
    public class StatisticsTracker
    {
        public List<StatisticDataTrackerBase> Statistics { get; private set; }

        private DateTimeOffset startTime = DateTimeOffset.Now;

        private EventStatisticDataTracker followTracker = new EventStatisticDataTracker("Follows", "AccountPlus");
        private EventStatisticDataTracker unfollowTracker = new EventStatisticDataTracker("Unfollows", "AccountMinus");
        private EventStatisticDataTracker hostsTracker = new EventStatisticDataTracker("Hosts", "AccountMultiple", (EventStatisticDataTracker dataTracker) =>
        {
            return string.Format("Total Hosts: {0},    Total Viewers From Hosts: {1},    Average Viewers From Hosts: {2}", dataTracker.MaxKeys, dataTracker.MaxValues, dataTracker.AverageValues);
        });
        private EventStatisticDataTracker subscriberTracker = new EventStatisticDataTracker("Subscribes", "AccountStar");
        private EventStatisticDataTracker resubscriberTracker = new EventStatisticDataTracker("Resubscribes", "AccountConvert");

        private EventStatisticDataTracker interactiveTracker = new EventStatisticDataTracker("Interactive", "GamepadVariant", (EventStatisticDataTracker dataTracker) =>
        {
            return string.Format("Buttons Pressed: {0},    Average Presses: {1}", dataTracker.MaxValues, dataTracker.AverageValues);
        });

        public StatisticsTracker()
        {
            ChannelSession.Constellation.OnFollowOccurred += Constellation_OnFollowOccurred;
            ChannelSession.Constellation.OnUnfollowOccurred += Constellation_OnUnfollowOccurred;
            ChannelSession.Constellation.OnHostedOccurred += Constellation_OnHostedOccurred;
            ChannelSession.Constellation.OnSubscribedOccurred += Constellation_OnSubscribedOccurred;
            ChannelSession.Constellation.OnResubscribedOccurred += Constellation_OnResubscribedOccurred;
            ChannelSession.Interactive.OnGiveInput += Interactive_OnGiveInput;

            this.Statistics = new List<StatisticDataTrackerBase>();

            this.Statistics.Add(new TrackedNumberStatisticDataTracker("Viewers", "Eye", (StatisticDataTrackerBase stats) =>
            {
                TrackedNumberStatisticDataTracker numberStats = (TrackedNumberStatisticDataTracker)stats;
                numberStats.AddValue((int)ChannelSession.Channel.viewersCurrent);
                return Task.FromResult(0);
            }));

            this.Statistics.Add(new TrackedNumberStatisticDataTracker("Chatters", "MessageTextOutline", (StatisticDataTrackerBase stats) =>
            {
                TrackedNumberStatisticDataTracker numberStats = (TrackedNumberStatisticDataTracker)stats;
                numberStats.AddValue(ChannelSession.Chat.ChatUsers.Count);
                return Task.FromResult(0);
            }));

            this.Statistics.Add(this.followTracker);
            this.Statistics.Add(this.unfollowTracker);
            this.Statistics.Add(this.hostsTracker);
            this.Statistics.Add(this.subscriberTracker);
            this.Statistics.Add(this.resubscriberTracker);

            this.Statistics.Add(this.interactiveTracker);
        }

        public string GetDefaultFileName() { return string.Format("Stream Statistics - {0}.csv", this.startTime.ToString("MM-dd-yy HH-mm")); }

        public Task Export(string filename = null)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                filename = this.GetDefaultFileName();
            }

            return Task.FromResult(0);
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
            this.unfollowTracker.OnStatisticEventOccurred(e.UserName);
        }

        private void Constellation_OnResubscribedOccurred(object sender, Tuple<UserViewModel, int> e)
        {
            this.unfollowTracker.OnStatisticEventOccurred(e.Item1.UserName, e.Item2);
        }

        private void Interactive_OnGiveInput(object sender, InteractiveGiveInputModel e)
        {
            if (e.input != null)
            {
                this.interactiveTracker.OnStatisticEventOccurred(e.input.controlID, 1);
            }
        }
    }
}
