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
            DateTimeOffset now = DateTimeOffset.UtcNow;

            int years = 0;
            now = now.AddYears(-1);
            while (now > dt)
            {
                years++;
                now = now.AddYears(-1);
            }
            now = now.AddYears(1);

            int months = 0;
            now = now.AddMonths(-1);
            while (now > dt)
            {
                months++;
                now = now.AddMonths(-1);
            }
            now = now.AddMonths(1);

            int days = 0;
            now = now.AddDays(-1);
            while (now > dt)
            {
                days++;
                now = now.AddDays(-1);
            }

            List<string> dateSegments = new List<string>();
            if (years == 0 && months == 0 && days == 0)
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

        public static double TotalSecondsFromNow(this DateTimeOffset dt)
        {
            return (DateTimeOffset.Now - dt).TotalSeconds;
        }

        public static DateTimeOffset SubtractMonths(this DateTimeOffset dt, int months)
        {
            return dt.Subtract(TimeSpan.FromDays(months * 30));
        }
    }
}
