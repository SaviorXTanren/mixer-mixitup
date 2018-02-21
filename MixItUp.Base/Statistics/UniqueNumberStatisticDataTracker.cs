using MixItUp.Base.Util;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Statistics
{
    public class UniqueNumberStatisticDataTracker : StatisticDataTrackerBase
    {
        public LockedDictionary<string, double> UniqueData { get; private set; }

        public UniqueNumberStatisticDataTracker(string name, string iconName, Func<StatisticDataTrackerBase, Task> updateFunction)
            : base(name, iconName, updateFunction)
        {
            this.UniqueData = new LockedDictionary<string, double>();
        }

        public int MaxKeys { get { return this.UniqueData.Count; } }

        public double AverageKeys { get { return ((double)this.MaxKeys) / ((double)this.TotalMinutes); } }

        public int MaxValues { get { return (int)this.UniqueData.Values.Sum(); } }

        public double MaxValuesDecimal { get { return this.UniqueData.Values.Sum(); } }

        public double AverageValues { get { return Math.Round(((double)this.MaxValuesDecimal) / ((double)this.TotalMinutes), 2); } }

        public void AddValue(string key) { this.AddValue(key, 0); }

        public void AddValue(string key, double value)
        {
            if (!this.UniqueData.ContainsKey(key))
            {
                this.UniqueData[key] = 0.0;
            }
            this.UniqueData[key] += value;
        }

        public override string ToString() { return string.Format("Total: {0},    Average: {1}", this.MaxKeys, this.AverageKeys); }
    }
}
