using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for StatisticsControl.xaml
    /// </summary>
    public partial class StatisticsControl : MainControlBase
    {
        private StatisticsViewModel viewModel;

        public StatisticsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new StatisticsViewModel((MainWindowViewModel)this.Window.ViewModel);
            await this.viewModel.OnOpen();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
        }
    }

    public class StatisticsViewModel : WindowControlViewModelBase
    {
        public ICommand RefreshCommand { get; private set; }

        public ObservableCollection<Axis> XAxes { get; private set; } = new ObservableCollection<Axis>();

        public ThreadSafeObservableCollection<ISeries> Series { get; private set; } = new ThreadSafeObservableCollection<ISeries>();

        private Dictionary<StatisticItemTypeEnum, StatisticSeriesBase> statisticSeries = new Dictionary<StatisticItemTypeEnum, StatisticSeriesBase>();

        private IEnumerable<StatisticModel> statistics;

        public StatisticsViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            this.XAxes.Add(new Axis
            {
                Labeler = value => new DateTime((long)value).ToString("t"),
                LabelsRotation = 30,

                UnitWidth = TimeSpan.FromMinutes(1).Ticks,
                MinStep = TimeSpan.FromMinutes(10).Ticks
            });

            this.Refresh();
            this.AddSeries(StatisticItemTypeEnum.Viewers);

            this.RefreshCommand = this.CreateCommand(() =>
            {
                this.Refresh();
            });
        }

        protected override Task OnOpenInternal()
        {
            this.Refresh();
            return base.OnVisibleInternal();
        }

        protected override Task OnVisibleInternal()
        {
            this.Refresh();
            return base.OnVisibleInternal();
        }

        private void Refresh()
        {
            int lastTotal = this.statistics?.Count() ?? 0;
            this.statistics = ServiceManager.Get<StatisticsService>().GetCurrentSessionStatistics();
            if (this.statistics.Count() > 0 && this.statisticSeries.Count > 0)
            {
                foreach (StatisticModel statistic in this.statistics.Skip(lastTotal))
                {
                    if (this.statisticSeries.ContainsKey(statistic.Type))
                    {
                        this.statisticSeries[statistic.Type].AddValue(statistic);
                    }
                }
            }
        }

        private void AddSeries(StatisticItemTypeEnum type)
        {
            IEnumerable<StatisticModel> values = this.statistics.Where(s => s.Type == type);
            StatisticSeriesBase series = null;

            switch (type)
            {
                case StatisticItemTypeEnum.Viewers:
                case StatisticItemTypeEnum.Chatters:
                    series = new StatisticLineSeries(type, StatisticValueTypeEnum.Integer);
                    break;

                case StatisticItemTypeEnum.Command:
                case StatisticItemTypeEnum.Action:

                case StatisticItemTypeEnum.StreamStart:
                case StatisticItemTypeEnum.StreamStop:
                case StatisticItemTypeEnum.StreamUpdated:

                case StatisticItemTypeEnum.UserJoined:
                case StatisticItemTypeEnum.UserLeft:
                case StatisticItemTypeEnum.ChatMessage:
                case StatisticItemTypeEnum.Follow:
                case StatisticItemTypeEnum.Raid:
                case StatisticItemTypeEnum.Subscription:
                case StatisticItemTypeEnum.Resubscription:
                case StatisticItemTypeEnum.SubscriptionGifted:

                case StatisticItemTypeEnum.TwitchBits:
                case StatisticItemTypeEnum.TwitchChannelPoints:
                case StatisticItemTypeEnum.TwitchHypeTrainStart:
                case StatisticItemTypeEnum.TwitchHypeTrainLevelUp:
                case StatisticItemTypeEnum.TwitchHypeTrainEnd:

                case StatisticItemTypeEnum.YouTubeSuperChat:

                case StatisticItemTypeEnum.TrovoSpell:
                case StatisticItemTypeEnum.TrovoMagicChat:

                case StatisticItemTypeEnum.Donation:
                case StatisticItemTypeEnum.StreamlootsCardRedeemed:
                case StatisticItemTypeEnum.StreamlootsPackPurchased:
                case StatisticItemTypeEnum.StreamlootsPackGifted:
                case StatisticItemTypeEnum.CrowdControlEffect:
                    break;
            }

            if (series != null)
            {
                this.statisticSeries[type] = series;
                this.Series.Add(series.Series);

                foreach (StatisticModel value in values)
                {
                    series.AddValue(value);
                }
            }
        }

        private void RemoveSeries(StatisticItemTypeEnum type)
        {
            if (this.statisticSeries.ContainsKey(type))
            {
                this.Series.Remove(this.statisticSeries[type].Series);
                this.statisticSeries.Remove(type);
            }
        }
    }

    public enum StatisticValueTypeEnum
    {
        Integer,
        Decimal,
        Text
    }

    public abstract class StatisticSeriesBase
    {
        public StatisticItemTypeEnum Type { get; private set; }

        public StatisticValueTypeEnum ValueType { get; private set; }

        public string Name { get; private set; }

        public ISeries Series { get; protected set; }

        public ThreadSafeObservableCollection<DateTimePoint> Values { get; private set; } = new ThreadSafeObservableCollection<DateTimePoint>();

        public StatisticSeriesBase(StatisticItemTypeEnum type, StatisticValueTypeEnum valueType)
        {
            this.Type = type;
            this.ValueType = valueType;
            this.Name = EnumLocalizationHelper.GetLocalizedName(this.Type);
        }

        public void AddValue(StatisticModel value)
        {
            if (this.ValueType == StatisticValueTypeEnum.Integer)
            {
                this.Values.Add(new DateTimePoint(value.DateTime, value.AmountInt));
            }
            else if (this.ValueType == StatisticValueTypeEnum.Decimal)
            {
                this.Values.Add(new DateTimePoint(value.DateTime, value.Amount));
            }
            else if (this.ValueType == StatisticValueTypeEnum.Text)
            {
                //this.Values.Add(new DateTimePoint(value.DateTime, value.AmountDouble));
            }
        }
    }

    public class StatisticLineSeries : StatisticSeriesBase
    {
        public StatisticLineSeries(StatisticItemTypeEnum type, StatisticValueTypeEnum valueType)
            : base(type, valueType)
        {
            this.Series = new LineSeries<DateTimePoint>
            {
                Name = this.Name,
                XToolTipLabelFormatter = (chartPoint) => $"{new DateTime((long)chartPoint.SecondaryValue):t}",
                Values = this.Values
            };
        }
    }

    public class StatisticScatterSeries : StatisticSeriesBase
    {
        public StatisticScatterSeries(StatisticItemTypeEnum type, StatisticValueTypeEnum valueType)
            : base(type, valueType)
        {
            this.Series = new ScatterSeries<DateTimePoint>
            {
                Name = this.Name,
                XToolTipLabelFormatter = (chartPoint) => $"{new DateTime((long)chartPoint.SecondaryValue):t}",
                Values = this.Values
            };
        }
    }

    public class CoinGeometry : SVGPathGeometry
    {
        public static SKPath svgPath = SKPath.ParseSvgPathData("M12,2C6.48,2,2,6.48,2,12s4.48,10,10,10s10-4.48,10-10S17.52,2,12,2z M12,20c-4.41,0-8-3.59-8-8c0-4.41,3.59-8,8-8 s8,3.59,8,8C20,16.41,16.41,20,12,20z M12.89,11.1c-1.78-0.59-2.64-0.96-2.64-1.9c0-1.02,1.11-1.39,1.81-1.39 c1.31,0,1.79,0.99,1.9,1.34l1.58-0.67c-0.15-0.44-0.82-1.91-2.66-2.23V5h-1.75v1.26c-2.6,0.56-2.62,2.85-2.62,2.96 c0,2.27,2.25,2.91,3.35,3.31c1.58,0.56,2.28,1.07,2.28,2.03c0,1.13-1.05,1.61-1.98,1.61c-1.82,0-2.34-1.87-2.4-2.09L8.1,14.75 c0.63,2.19,2.28,2.78,3.02,2.96V19h1.75v-1.24c0.52-0.09,3.02-0.59,3.02-3.22C15.9,13.15,15.29,11.93,12.89,11.1z");
        public CoinGeometry() : base(svgPath) { }
    }

    public class GiftGeometry : SVGPathGeometry
    {
        public static SKPath svgPath = SKPath.ParseSvgPathData("M20 6h-2.18c.11-.31.18-.65.18-1 0-1.66-1.34-3-3-3-1.05 0-1.96.54-2.5 1.35l-.5.67-.5-.68C10.96 2.54 10.05 2 9 2 7.34 2 6 3.34 6 5c0 .35.07.69.18 1H4c-1.11 0-1.99.89-1.99 2L2 19c0 1.11.89 2 2 2h16c1.11 0 2-.89 2-2V8c0-1.11-.89-2-2-2zm-5-2c.55 0 1 .45 1 1s-.45 1-1 1-1-.45-1-1 .45-1 1-1zM9 4c.55 0 1 .45 1 1s-.45 1-1 1-1-.45-1-1 .45-1 1-1zm11 15H4v-2h16v2zm0-5H4V8h5.08L7 10.83 8.62 12 11 8.76l1-1.36 1 1.36L15.38 12 17 10.83 14.92 8H20v6z");
        public GiftGeometry() : base(svgPath) { }
    }
}
