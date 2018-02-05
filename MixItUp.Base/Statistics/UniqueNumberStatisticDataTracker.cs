using MixItUp.Base.Util;
using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Statistics
{
    public class UniqueNumberStatisticDataTracker : StatisticDataTrackerBase
    {
        protected LockedHashSet<string> UniqueData { get; private set; }

        public UniqueNumberStatisticDataTracker(string name, Func<StatisticDataTrackerBase, Task> updateFunction)
            : base(name, updateFunction)
        {
            this.UniqueData = new LockedHashSet<string>();
        }

        public int Max { get { return this.UniqueData.Count; } }

        public double Average { get { return ((double)this.Max) / ((double)this.TotalMinutes); } }

        public void AddValue(string data) { this.UniqueData.Add(data); }

        public override string ToString() { return string.Format("Total: {0}, Average: {1}", this.Max, this.Average); }
    }
}
