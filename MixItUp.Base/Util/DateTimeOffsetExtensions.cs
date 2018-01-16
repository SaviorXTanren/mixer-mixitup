using System;
using System.Collections.Generic;

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

            List<string> dateSegments = new List<string>();
            if (years > 0)
            {
                dateSegments.Add(years + " Year(s)");
            }
            if (months > 0)
            {
                dateSegments.Add(months + " Month(s)");
            }
            if (days > 0)
            {
                dateSegments.Add(days + " Day(s)");
            }

            return string.Join(", ", dateSegments);
        }
    }
}
