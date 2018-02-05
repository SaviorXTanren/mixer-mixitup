using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Statistics
{
    public class EventStatisticsDataTracker : UniqueNumberStatisticDataTracker
    {
        private event EventHandler<string> StatisticEventOccurred;

        public EventStatisticsDataTracker(string name) : base(name, (StatisticDataTrackerBase stats) => { return Task.FromResult(0); })
        {
            this.StatisticEventOccurred += EventStatisticsDataTracker_StatisticEventOccurred;
        }

        public void OnStatisticEventOccurred(string data)
        {
            if (this.StatisticEventOccurred != null)
            {
                this.StatisticEventOccurred(this, data);
            }
        }

        private void EventStatisticsDataTracker_StatisticEventOccurred(object sender, string e)
        {
            this.UniqueData.Add(e);
        }
    }
}
