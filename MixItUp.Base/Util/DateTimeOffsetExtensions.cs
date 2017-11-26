using System;
using System.Text;

namespace MixItUp.Base.Util
{
    public static class DateTimeOffsetExtensions
    {
        public static string ToFriendlyDateString(this DateTimeOffset dt) { return dt.ToString("d"); }

        public static string ToFriendlyDateTimeString(this DateTimeOffset dt) { return dt.ToString("g"); }

        public static string GetAge(this DateTimeOffset dt)
        {
            TimeSpan difference = DateTimeOffset.Now - dt.ToOffset(DateTimeOffset.Now.Offset);
            int days = (int)difference.TotalDays;
            int years = days / 365;
            days = days - (years * 365);
            int months = days / 30;
            days = days - (months * 30);

            StringBuilder mixerAgeString = new StringBuilder();
            if (years > 0)
            {
                mixerAgeString.Append(years + " Year");
                if (years > 1)
                {
                    mixerAgeString.Append("s");
                }
                mixerAgeString.Append(", ");
            }
            if (months > 0)
            {
                mixerAgeString.Append(months + " Month");
                if (months > 1)
                {
                    mixerAgeString.Append("s");
                }
                mixerAgeString.Append(", ");
            }
            mixerAgeString.Append(days + " Day");
            if (days > 1)
            {
                mixerAgeString.Append("s");
            }

            return mixerAgeString.ToString();
        }
    }
}
