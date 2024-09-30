using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Util
{
    public enum DurationSpanTypeEnum
    {
        Seconds,
        Minutes,
        Hours,
        Days,
        Months,
        Years,
    }

    [DataContract]
    public class DurationSpan
    {
        [DataMember]
        public DurationSpanTypeEnum Type { get; set; }

        [DataMember]
        public int Amount { get; set; }

        public DurationSpan() { }

        public DurationSpan(DurationSpanTypeEnum type, int amount)
        {
            this.Type = type;
            this.Amount = amount;
        }

        public DateTimeOffset GetDateTimeOffsetFromNow()
        {
            switch (this.Type)
            {
                case DurationSpanTypeEnum.Years: return DateTimeOffset.Now.AddMonths(this.Amount * 12);
                case DurationSpanTypeEnum.Months: return DateTimeOffset.Now.AddMonths(this.Amount);
                case DurationSpanTypeEnum.Days: return DateTimeOffset.Now.AddDays(this.Amount);
                case DurationSpanTypeEnum.Hours: return DateTimeOffset.Now.AddHours(this.Amount);
                case DurationSpanTypeEnum.Minutes: return DateTimeOffset.Now.AddMinutes(this.Amount);
                case DurationSpanTypeEnum.Seconds: return DateTimeOffset.Now.AddSeconds(this.Amount);
            }
            return DateTimeOffset.Now;
        }
    }
}
