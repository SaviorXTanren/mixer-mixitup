using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Statistics
{
    public class TrackedNumberStatisticDataTracker : StatisticDataTrackerBase
    {
        public List<int> allNumbers = new List<int>();

        public TrackedNumberStatisticDataTracker(string name, string iconName, Func<StatisticDataTrackerBase, Task> updateFunction)
            : base(name, iconName, updateFunction)
        { }

        public int Max { get { return (this.allNumbers.Count > 0) ? this.allNumbers.Max() : 0; } }

        public double Average { get { return ((double)this.allNumbers.Sum()) / ((double)this.TotalMinutes); } }

        public void AddValue(int value) { this.allNumbers.Add(value); }

        public override string ToString() { return string.Format("Max: {0},    Average: {1}", this.Max, this.Average); }
    }
}
