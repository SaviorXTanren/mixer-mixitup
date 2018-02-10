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

        private EventStatisticDataTracker hostsTracker = new EventStatisticDataTracker("Hosts", "AccountMultiple", (EventStatisticDataTracker dataTracker) =>
        {
            return string.Format("Total Hosts: {0},    Total Viewers From Hosts: {1},    Average Viewers From Hosts: {2}", dataTracker.MaxKeys, dataTracker.MaxValues, dataTracker.AverageValues);
        });

        private EventStatisticDataTracker interactiveTracker = new EventStatisticDataTracker("Interactive", "GamepadVariant", (EventStatisticDataTracker dataTracker) =>
        {
            return string.Format("Buttons Pressed: {0},    Average Presses: {1}", dataTracker.MaxValues, dataTracker.AverageValues);
        });

        public StatisticsTracker()
        {
            ChannelSession.Constellation.OnEventOccurred += Constellation_OnEventOccurred;
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

            this.Statistics.Add(this.hostsTracker);

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

        private void Constellation_OnEventOccurred(object sender, Mixer.Base.Model.Constellation.ConstellationLiveEventModel e)
        {
            ChannelModel channel = null;
            UserViewModel user = null;

            JToken userToken;
            if (e.payload.TryGetValue("user", out userToken))
            {
                user = new UserViewModel(userToken.ToObject<UserModel>());

                JToken subscribeStartToken;
                if (e.payload.TryGetValue("since", out subscribeStartToken))
                {
                    user.SubscribeDate = subscribeStartToken.ToObject<DateTimeOffset>();
                }
            }
            else if (e.payload.TryGetValue("hoster", out userToken))
            {
                channel = userToken.ToObject<ChannelModel>();
                user = new UserViewModel(channel.id, channel.token);
            }

            if (e.channel.Equals(ConstellationClientWrapper.ChannelFollowEvent.ToString()) && user != null)
            {
                this.followTracker.OnStatisticEventOccurred(user.UserName);
            }
            else if (e.channel.Equals(ConstellationClientWrapper.ChannelHostedEvent.ToString()) && channel != null)
            {
                this.followTracker.OnStatisticEventOccurred(channel.token, (int)channel.viewersCurrent);
            }
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
