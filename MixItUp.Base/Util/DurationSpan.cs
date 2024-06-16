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
            if (this.Type == DurationSpanTypeEnum.Years)
            {
                return DateTimeOffset.Now.AddMonths(this.Amount * 12);
            }
            else if (this.Type == DurationSpanTypeEnum.Months)
            {
                return DateTimeOffset.Now.AddMonths(this.Amount);
            }
            else if (this.Type == DurationSpanTypeEnum.Days)
            {
                return DateTimeOffset.Now.AddDays(this.Amount);
            }
            else if (this.Type == DurationSpanTypeEnum.Hours)
            {
                return DateTimeOffset.Now.AddHours(this.Amount);
            }
            else if (this.Type == DurationSpanTypeEnum.Minutes)
            {
                return DateTimeOffset.Now.AddMinutes(this.Amount);
            }
            else if (this.Type == DurationSpanTypeEnum.Seconds)
            {
                return DateTimeOffset.Now.AddSeconds(this.Amount);
            }
            return DateTimeOffset.Now;
        }
    }
}
