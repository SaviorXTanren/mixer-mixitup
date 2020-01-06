using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Statistics
{
    public class StatisticDataPoint
    {
        public string Identifier { get; private set; }

        public double ValueDecimal { get; private set; }
        public string ValueString { get; private set; }

        public DateTimeOffset DateTime { get; set; }

        public StatisticDataPoint(string identifier) : this(identifier, -1) { }

        public StatisticDataPoint(int value) : this(null, value) { }

        public StatisticDataPoint(string identifier, int value) : this(identifier, (double)value) { }

        public StatisticDataPoint(string identifier, string value)
            : this(identifier)
        {
            this.ValueString = value;
        }

        public StatisticDataPoint(string identifier, double value)
        {
            this.Identifier = identifier;
            this.ValueDecimal = value;
            this.DateTime = DateTimeOffset.Now;
        }
    }

    public abstract class StatisticDataTrackerBase
    {
        public string Name { get; private set; }
        public string IconName { get; private set; }
        public bool IsPackIcon { get; private set; }

        public DateTimeOffset StartTime { get; private set; }

        public List<StatisticDataPoint> DataPoints { get; protected set; }

        private Func<StatisticDataTrackerBase, Task> updateFunction;

        public StatisticDataTrackerBase(string name, string iconName, bool isPackIcon, Func<StatisticDataTrackerBase, Task> updateFunction)
        {
            this.Name = name;
            this.IconName = iconName;
            this.IsPackIcon = isPackIcon;
            this.DataPoints = new List<StatisticDataPoint>();
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

        public int UniqueIdentifiers { get { return this.DataPoints.Select(dp => dp.Identifier).Distinct().Count(); } }

        public double AverageUniqueIdentifiers { get { return ((double)this.UniqueIdentifiers) / ((double)this.TotalMinutes); } }

        public int Total { get { return this.DataPoints.Count; } }

        public double Average { get { return ((double)this.Total) / ((double)this.TotalMinutes); } }
        public string AverageString { get { return Math.Round(Average, 2).ToString(); } }

        public int MaxValue { get { return (int)this.MaxValueDecimal; } }
        public double MaxValueDecimal { get { return (this.DataPoints.Count > 0) ? this.DataPoints.Select(dp => dp.ValueDecimal).ToArray().Max() : 0.0; } }

        public int TotalValue { get { return (int)this.TotalValueDecimal; } }
        public double TotalValueDecimal { get { return this.DataPoints.Select(dp => dp.ValueDecimal).ToArray().Sum(); } }

        public double AverageValue { get { return this.TotalValueDecimal / ((double)this.TotalMinutes); } }
        public string AverageValueString { get { return Math.Round(AverageValue, 2).ToString(); } }

        public bool IsImageIcon { get { return !this.IsPackIcon; } }

        public abstract IEnumerable<string> GetExportHeaders();

        public virtual IEnumerable<List<string>> GetExportData()
        {
            List<List<string>> results = new List<List<string>>();
            foreach (StatisticDataPoint dataPoint in this.DataPoints)
            {
                List<string> resultRow = new List<string>();

                if (!string.IsNullOrEmpty(dataPoint.Identifier))
                {
                    resultRow.Add(dataPoint.Identifier);
                }

                if (dataPoint.ValueDecimal >= 0)
                {
                    resultRow.Add(dataPoint.ValueDecimal.ToString());
                }

                if (!string.IsNullOrEmpty(dataPoint.ValueString))
                {
                    resultRow.Add(dataPoint.ValueString);
                }

                resultRow.Add(dataPoint.DateTime.LocalDateTime.ToString("MM/dd/yy HH:mm"));

                results.Add(resultRow);
            }
            return results;
        }
    }

    public class StaticTextStatisticDataTracker : StatisticDataTrackerBase
    {
        public StaticTextStatisticDataTracker(string name, string iconName, bool isPackIcon, Func<StatisticDataTrackerBase, Task> updateFunction)
            : base(name, iconName, isPackIcon, updateFunction)
        { }

        public void AddValue(string identifier, string value) { this.DataPoints.Add(new StatisticDataPoint(identifier, value)); }

        public void ClearValues() { this.DataPoints.Clear(); }

        public override IEnumerable<string> GetExportHeaders() { return this.DataPoints.Select(dp => dp.Identifier); }

        public override IEnumerable<List<string>> GetExportData()
        {
            List<List<string>> results = new List<List<string>>();
            results.Add(new List<string>());
            foreach (StatisticDataPoint dataPoint in this.DataPoints)
            {
                results[0].Add(dataPoint.ValueString);
            }
            return results;
        }

        public override string ToString()
        {
            List<string> values = new List<string>();
            foreach (StatisticDataPoint dataPoint in this.DataPoints)
            {
                values.Add(string.Format("{0}: {1}", dataPoint.Identifier, dataPoint.ValueString));
            }
            return string.Join(",    ", values);
        }
    }

    public class TrackedNumberStatisticDataTracker : StatisticDataTrackerBase
    {
        public TrackedNumberStatisticDataTracker(string name, string iconName, bool isPackIcon, Func<StatisticDataTrackerBase, Task> updateFunction)
            : base(name, iconName, isPackIcon, updateFunction)
        { }

        public void AddValue(int value) { this.DataPoints.Add(new StatisticDataPoint(value)); }

        public override IEnumerable<string> GetExportHeaders() { return new List<string>() { "Max", "Average" }; }

        public override IEnumerable<List<string>> GetExportData()
        {
            List<List<string>> results = new List<List<string>>();
            results.Add(new List<string>() { this.MaxValue.ToString(), this.AverageValue.ToString() });
            return results;
        }

        public override string ToString() { return string.Format("Max: {0},    Average: {1}", this.MaxValue, this.AverageValueString); }
    }

    public class EventStatisticDataTracker : StatisticDataTrackerBase
    {
        private event EventHandler<string> StatisticEventOccurred;
        private event EventHandler<Tuple<string, double>> StatisticEventWithDecimalValueOccurred;
        private event EventHandler<Tuple<string, string>> StatisticEventWithStringValueOccurred;

        private IEnumerable<string> exportHeaders;

        private Func<EventStatisticDataTracker, string> customToStringFunction;

        public EventStatisticDataTracker(string name, string iconName, bool isPackIcon, IEnumerable<string> exportHeaders, Func<EventStatisticDataTracker, string> customToStringFunction = null)
            : base(name, iconName, isPackIcon, (StatisticDataTrackerBase stats) => { return Task.FromResult(0); })
        {
            this.exportHeaders = exportHeaders;
            this.customToStringFunction = customToStringFunction;

            this.StatisticEventOccurred += EventStatisticsDataTracker_StatisticEventOccurred;
            this.StatisticEventWithDecimalValueOccurred += EventStatisticsDataTracker_StatisticEventWithDecimalValueOccurred;
            this.StatisticEventWithStringValueOccurred += EventStatisticDataTracker_StatisticEventWithStringValueOccurred;
        }

        public override IEnumerable<string> GetExportHeaders() { return this.exportHeaders; }

        public void AddValue(string identifier) { this.DataPoints.Add(new StatisticDataPoint(identifier)); }

        public void AddValue(string identifier, double value) { this.DataPoints.Add(new StatisticDataPoint(identifier, value)); }

        public void AddValue(string identifier, string value) { this.DataPoints.Add(new StatisticDataPoint(identifier, value)); }

        public void OnStatisticEventOccurred(string key)
        {
            if (this.StatisticEventOccurred != null)
            {
                this.StatisticEventOccurred(this, key);
            }
        }

        public void OnStatisticEventOccurred(string key, double value)
        {
            if (this.StatisticEventWithDecimalValueOccurred != null)
            {
                this.StatisticEventWithDecimalValueOccurred(this, new Tuple<string, double>(key, value));
            }
        }

        public void OnStatisticEventOccurred(string key, string value)
        {
            if (this.StatisticEventWithStringValueOccurred != null)
            {
                this.StatisticEventWithStringValueOccurred(this, new Tuple<string, string>(key, value));
            }
        }

        public override string ToString()
        {
            if (this.customToStringFunction != null)
            {
                return this.customToStringFunction(this);
            }
            return string.Format("Total: {0},    Average: {1}", this.UniqueIdentifiers, this.AverageUniqueIdentifiers);
        }

        private void EventStatisticsDataTracker_StatisticEventOccurred(object sender, string e)
        {
            this.AddValue(e);
        }

        private void EventStatisticsDataTracker_StatisticEventWithDecimalValueOccurred(object sender, Tuple<string, double> e)
        {
            this.AddValue(e.Item1, e.Item2);
        }

        private void EventStatisticDataTracker_StatisticEventWithStringValueOccurred(object sender, Tuple<string, string> e)
        {
            this.AddValue(e.Item1, e.Item2);
        }
    }
}
