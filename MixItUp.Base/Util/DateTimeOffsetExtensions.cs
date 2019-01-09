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
            if (difference.TotalDays < 1)
            {
                dateSegments.Add("<1 Day");
            }
            else
            {
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
            }

            return string.Join(", ", dateSegments);
        }

        public static int TotalMonthsFromNow(this DateTimeOffset dt)
        {
            DateTime currentDateTime = DateTimeOffset.Now.Date;
            DateTime tempDateTime = dt.Date;

            int subMonths = 0;
            while (tempDateTime <= currentDateTime)
            {
                tempDateTime = tempDateTime.AddMonths(1);
                subMonths++;
            }

            subMonths = Math.Max(subMonths - 1, 0);

            return subMonths;
        }

        public static int TotalDaysFromNow(this DateTimeOffset dt)
        {
            return (int)(DateTimeOffset.Now.Date - dt.Date).TotalDays;
        }

        public static int TotalMinutesFromNow(this DateTimeOffset dt)
        {
            return (int)(DateTimeOffset.Now - dt).TotalMinutes;
        }
    }
}
