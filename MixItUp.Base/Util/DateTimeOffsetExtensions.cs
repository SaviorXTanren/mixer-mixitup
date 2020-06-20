using System;
using System.Collections.Generic;

namespace MixItUp.Base.Util
{
    public static class DateTimeOffsetExtensions
    {
        public static string ToFriendlyDateString(this DateTimeOffset dt) { return dt.ToString("d"); }

        public static string ToFriendlyDateTimeString(this DateTimeOffset dt) { return dt.ToString("g"); }

        public static string GetAge(this DateTimeOffset start, bool includeTime = false)
        {
            return start.GetAge(DateTimeOffset.UtcNow);
        }

        public static string GetAge(this DateTimeOffset start, DateTimeOffset end, bool includeTime = false)
        {
            DateTimeOffset valid = end;
            DateTimeOffset test = valid;

            int years = 0;
            int months = 0;
            int days = 0;
            int hours = 0;
            int minutes = 0;

            test = test.AddYears(-1);
            while (test > start)
            {
                years++;
                valid = valid.AddYears(-1);
                test = test.AddYears(-1);
            }
            test = valid;

            test = test.AddMonths(-1);
            while (test > start)
            {
                months++;
                valid = valid.AddMonths(-1);
                test = test.AddMonths(-1);
            }
            test = valid;

            test = test.AddDays(-1);
            while (test > start)
            {
                days++;
                valid = valid.AddDays(-1);
                test = test.AddDays(-1);
            }
            test = valid;

            List<string> dateSegments = new List<string>();
            if (years == 0 && months == 0 && days == 0)
            {
                if (includeTime)
                {
                    test = test.AddHours(-1);
                    while (test > start)
                    {
                        hours++;
                        valid = valid.AddHours(-1);
                        test = test.AddHours(-1);
                    }
                    test = valid;

                    test = test.AddMinutes(-1);
                    while (test > start)
                    {
                        minutes++;
                        valid = valid.AddMinutes(-1);
                        test = test.AddMinutes(-1);
                    }
                    test = valid;

                    if (hours > 0)
                    {
                        dateSegments.Add(hours + " Hours(s)");
                    }
                    if (minutes > 0)
                    {
                        dateSegments.Add(minutes + " Minute(s)");
                    }
                }
                else
                {
                    dateSegments.Add("<1 Day");
                }
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
    }
}
