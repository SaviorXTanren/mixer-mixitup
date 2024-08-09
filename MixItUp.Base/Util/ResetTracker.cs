using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Util
{
    public enum ResetTypeEnum
    {
        [Obsolete]
        Hours,
        Days,
        Weeks,
        Months
    }

    [DataContract]
    public class ResetTracker
    {
        [DataMember]
        public ResetTypeEnum Type { get; set; }
        [DataMember]
        public int Amount { get; set; }

        [DataMember]
        public DateTimeOffset StartDateTime { get; set; }

        public ResetTracker() { }

        public ResetTracker(ResetTypeEnum type, int amount, DateTimeOffset startDateTime)
        {
            this.Type = type;
            this.Amount = amount;
            this.StartDateTime = startDateTime;
        }

        public DateTimeOffset GetEndDateTimeOffset()
        {
            if (this.Type == ResetTypeEnum.Hours)
            {
                DateTimeOffset result = DateTimeOffset.Now;
                return new DateTimeOffset(result.Year, result.Month, result.Day, result.Hour, 0, 0, result.Offset);
            }
            else
            {
                switch (this.Type)
                {
                    case ResetTypeEnum.Days: return this.StartDateTime.AddDays(this.Amount).Date;
                    case ResetTypeEnum.Weeks: return this.StartDateTime.AddDays(this.Amount * 7).Date;
                    case ResetTypeEnum.Months: return this.StartDateTime.AddMonths(this.Amount).Date;
                }
            }
            return DateTimeOffset.Now;
        }

        public void UpdateStartDateTimeToLatest()
        {
            DateTimeOffset endDateTime = this.GetEndDateTimeOffset();
            if (endDateTime <= DateTimeOffset.Now)
            {
                if (this.Type == ResetTypeEnum.Hours)
                {
                    this.StartDateTime = DateTimeOffset.Now;
                    this.StartDateTime = new DateTimeOffset(this.StartDateTime.Year, this.StartDateTime.Month, this.StartDateTime.Day, this.StartDateTime.Hour, 0, 0, this.StartDateTime.Offset);
                }
                else if (this.Type == ResetTypeEnum.Days && this.Amount == 1)
                {
                    this.StartDateTime = DateTimeOffset.Now.Date;
                }
                else
                {
                    while (endDateTime <= DateTimeOffset.Now)
                    {
                        this.StartDateTime = endDateTime;
                        endDateTime = this.GetEndDateTimeOffset();
                    }
                }
            }
        }
    }

    public class ResetTrackerViewModel : UIViewModelBase
    {
        public ResetTracker Model { get; set; }

        public ResetTrackerViewModel() { this.Model = new ResetTracker(); }

        public ResetTrackerViewModel(ResetTracker model) { this.Model = model; }

        public int Amount
        {
            get { return this.Model.Amount; }
            set
            {
                this.Model.Amount = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ResetDisplayText));
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
            }
        }

        public DateTimeOffset StartDateTime
        {
            get { return this.Model.StartDateTime; }
            set
            {
                this.Model.StartDateTime = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ResetDisplayText));
            }
        }

        public string ResetDisplayText
        {
            get
            {
                if (this.Model.Amount > 0)
                {
                    DateTimeOffset startDate = this.Model.StartDateTime;
                    if (startDate == DateTimeOffset.MinValue)
                    {
                        startDate = DateTimeOffset.Now;
                    }

                    if (this.Model.Type == ResetTypeEnum.Weeks)
                    {
                        string dayOfTheWeek = startDate.ToString("dddd");
                        return $"{this.Model.Amount} {EnumLocalizationHelper.GetLocalizedName(this.Model.Type)} ({dayOfTheWeek})";
                    }
                    else if (this.Model.Type == ResetTypeEnum.Months)
                    {
                        return $"{this.Model.Amount} {EnumLocalizationHelper.GetLocalizedName(this.Model.Type)} ({Resources.Day} {startDate.Day})";
                    }
                    else
                    {
                        return $"{this.Model.Amount} {EnumLocalizationHelper.GetLocalizedName(this.Model.Type)}";
                    }
                }
                return Resources.Never;
            }
        }
    }
}
