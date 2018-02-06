using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Statistics
{
    public abstract class StatisticDataTrackerBase
    {
        public string Name { get; private set; }

        public DateTimeOffset StartTime { get; private set; }

        private Func<StatisticDataTrackerBase, Task> updateFunction;

        public StatisticDataTrackerBase(string name, Func<StatisticDataTrackerBase, Task> updateFunction)
        {
            this.Name = name;
            this.updateFunction = updateFunction;

            this.StartTime = DateTimeOffset.Now;

            Task.Run(async () =>
            {
                while (true)
                {
                    await this.updateFunction(this);

                    await Task.Delay(60000);
                }
            });
        }

        public int TotalMinutes { get { return (int)Math.Max((DateTimeOffset.Now - this.StartTime).TotalMinutes, 1); } }
    }
}
