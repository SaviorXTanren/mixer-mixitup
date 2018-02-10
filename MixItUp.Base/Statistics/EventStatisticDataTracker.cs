using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Statistics
{
    public class EventStatisticDataTracker : UniqueNumberStatisticDataTracker
    {
        private event EventHandler<string> StatisticEventOccurred;
        private event EventHandler<Tuple<string, int>> StatisticEventWithValueOccurred;

        private Func<EventStatisticDataTracker, string> customToStringFunction;

        public EventStatisticDataTracker(string name, string iconName, Func<EventStatisticDataTracker, string> customToStringFunction = null)
            : base(name, iconName, (StatisticDataTrackerBase stats) => { return Task.FromResult(0); })
        {
            this.customToStringFunction = customToStringFunction;

            this.StatisticEventOccurred += EventStatisticsDataTracker_StatisticEventOccurred;
            this.StatisticEventWithValueOccurred += EventStatisticsDataTracker_StatisticEventWithValueOccurred;
        }

        public void OnStatisticEventOccurred(string key)
        {
            if (this.StatisticEventOccurred != null)
            {
                this.StatisticEventOccurred(this, key);
            }
        }

        public void OnStatisticEventOccurred(string key, int value)
        {
            if (this.StatisticEventOccurred != null)
            {
                this.StatisticEventWithValueOccurred(this, new Tuple<string, int>(key, value));
            }
        }

        public override string ToString()
        {
            if (this.customToStringFunction != null)
            {
                return this.customToStringFunction(this);
            }
            return base.ToString();
        }

        private void EventStatisticsDataTracker_StatisticEventOccurred(object sender, string e)
        {
            this.AddValue(e);
        }

        private void EventStatisticsDataTracker_StatisticEventWithValueOccurred(object sender, Tuple<string, int> e)
        {
            this.AddValue(e.Item1, e.Item2);
        }
    }
}
