using MixItUp.Base.ViewModels;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Util
{
    public enum ResetTypeEnum
    {
        None,
        Days,
        Weeks,
        Months
    }

    [DataContract]
    public class ResetTracker
    {
        [DataMember]
        public ResetTypeEnum Type { get; set; } = ResetTypeEnum.None;
        [DataMember]
        public int Amount { get; set; }

        [DataMember]
        public DayOfWeek DayOfTheWeek { get; set; } = DayOfWeek.Monday;
        [DataMember]
        public int DayOfTheMonth { get; set; } = 1;

        [DataMember]
        public DateTimeOffset LastReset { get; set; } = DateTimeOffset.Now;

        [Obsolete]
        [DataMember]
        public DateTimeOffset StartDateTime { get; set; }

        public ResetTracker() { }

        public ResetTracker(ResetTypeEnum type, int amount, DayOfWeek dayOfTheWeek, int dayOftheMonth)
        {
            this.Type = type;
            this.Amount = amount;
            this.DayOfTheWeek = dayOfTheWeek;
            this.DayOfTheMonth = dayOftheMonth;
        }

        public DateTimeOffset GetEndDateTimeOffset()
        {
            if (this.Type == ResetTypeEnum.Days)
            {
                return this.LastReset.AddDays(this.Amount).Date;
            }
            else if (this.Type == ResetTypeEnum.Weeks)
            {
                DateTimeOffset result = this.LastReset.AddDays(this.Amount * 7).Date;
                while (result.DayOfWeek != this.DayOfTheWeek)
                {
                    result = result.Subtract(TimeSpan.FromDays(1));
                }
                return result;
            }
            else if (this.Type == ResetTypeEnum.Months)
            {
                DateTimeOffset result = DateTimeOffset.Now.Date.AddMonths(this.Amount);
                return new DateTimeOffset(result.Year, result.Month, Math.Min(this.DayOfTheMonth, DateTime.DaysInMonth(result.Year, result.Month)), 0, 0, 0, TimeSpan.Zero);
            }
            return DateTimeOffset.Now.Date;
        }

        public bool MustBeReset() { return this.GetEndDateTimeOffset() <= DateTimeOffset.Now.Date; }

        public void PerformReset()
        {
            this.UpgradeToNewerFormat();

            if (this.MustBeReset())
            {
                if (this.Type == ResetTypeEnum.Days && this.Amount == 1)
                {
                    this.LastReset = DateTimeOffset.Now.Date;
                }
                else
                {
                    while (this.MustBeReset())
                    {
                        this.LastReset = this.GetEndDateTimeOffset();
                    }
                }
            }
        }

        public void UpgradeToNewerFormat()
        {
#pragma warning disable CS0612 // Type or member is obsolete
            if (this.StartDateTime != DateTimeOffset.MinValue)
            {
                this.DayOfTheWeek = this.StartDateTime.DayOfWeek;
                this.DayOfTheMonth = this.StartDateTime.Day;
                this.StartDateTime = DateTimeOffset.MinValue;
            }
#pragma warning restore CS0612 // Type or member is obsolete
        }
    }

    public class ResetTrackerViewModel : UIViewModelBase
    {
        public ResetTracker Model { get; set; }

        public ResetTrackerViewModel()
        {
            this.Model = new ResetTracker();
            this.Model.DayOfTheWeek = DateTimeOffset.Now.DayOfWeek;
            this.Model.DayOfTheMonth = DateTimeOffset.Now.Day;
        }

        public ResetTrackerViewModel(ResetTracker model)
        {
            this.Model = model ?? new ResetTracker();
            this.Model.UpgradeToNewerFormat();
        }

        public int Amount
        {
            get { return this.Model.Amount; }
            set
            {
                this.Model.Amount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ResetDisplayText));
                this.NotifyPropertyChanged(nameof(this.NextResetDisplayText));
            }
        }

        public IEnumerable<ResetTypeEnum> Types { get; set; } = EnumHelper.GetEnumList<ResetTypeEnum>();
        public ResetTypeEnum SelectedType
        {
            get { return this.Model.Type; }
            set
            {
                this.Model.Type = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ResetDisplayText));
                this.NotifyPropertyChanged(nameof(this.NextResetDisplayText));
                this.NotifyPropertyChanged(nameof(this.ShowDayOfTheWeekSelector));
                this.NotifyPropertyChanged(nameof(this.ShowDayOfTheMonthSelector));
            }
        }

        public bool ShowDayOfTheWeekSelector { get { return this.SelectedType == ResetTypeEnum.Weeks; } }

        public IEnumerable<DayOfWeek> DaysOfTheWeek { get; set; } = EnumHelper.GetEnumList<DayOfWeek>();
        public DayOfWeek SelectedDayOfTheWeek
        {
            get { return this.Model.DayOfTheWeek; }
            set
            {
                this.Model.DayOfTheWeek = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ResetDisplayText));
                this.NotifyPropertyChanged(nameof(this.NextResetDisplayText));
            }
        }

        public bool ShowDayOfTheMonthSelector { get { return this.SelectedType == ResetTypeEnum.Months; } }

        public int DayOfTheMonth
        {
            get { return this.Model.DayOfTheMonth; }
            set
            {
                this.Model.DayOfTheMonth = Math.Min(Math.Max(value, 0), 31);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ResetDisplayText));
                this.NotifyPropertyChanged(nameof(this.NextResetDisplayText));
            }
        }

        public string ResetDisplayText
        {
            get
            {
                if (this.Model.Amount > 0)
                {
                    if (this.Model.Type == ResetTypeEnum.Days)
                    {
                        return $"{this.Model.Amount} {EnumLocalizationHelper.GetLocalizedName(this.Model.Type)}";
                    }
                    else if (this.Model.Type == ResetTypeEnum.Weeks)
                    {
                        return $"{this.Model.Amount} {EnumLocalizationHelper.GetLocalizedName(this.Model.Type)} ({EnumLocalizationHelper.GetLocalizedName(this.Model.DayOfTheWeek)})";
                    }
                    else if (this.Model.Type == ResetTypeEnum.Months)
                    {
                        return $"{this.Model.Amount} {EnumLocalizationHelper.GetLocalizedName(this.Model.Type)} ({Resources.Day} {this.Model.DayOfTheMonth})";
                    }
                }
                return Resources.Never;
            }
        }

        public string NextResetDisplayText
        {
            get
            {
                if (this.Model.Amount > 0)
                {
                    return this.Model.GetEndDateTimeOffset().Date.ToShortDateString();
                }
                return Resources.Never;
            }
        }
    }
}
