using System;
using System.Collections.Generic;
using System.Globalization;

namespace MixItUp.Base.Util
{
    public static class DateTimeOffsetExtensions
    {
        /// <summary>
        /// Creates a DateTimeOffset from a UTC Unix time in milliseconds.
        /// </summary>
        /// <param name="milliseconds">The total milliseconds in UTC Unix time</param>
        /// <returns>The equivalent DateTimeOffset</returns>
        public static DateTimeOffset FromUTCUnixTimeMilliseconds(long milliseconds) { return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).ToOffset(DateTimeOffset.Now.Offset); }

        /// <summary>
        /// Creates a DateTimeOffset from a UTC Unix time in seconds.
        /// </summary>
        /// <param name="seconds">The total secpnds in UTC Unix time</param>
        /// <returns>The equivalent DateTimeOffset</returns>
        public static DateTimeOffset FromUTCUnixTimeSeconds(long seconds) { return DateTimeOffset.FromUnixTimeSeconds(seconds).ToOffset(DateTimeOffset.Now.Offset); }

        /// <summary>
        /// Creates a DateTimeOffset from an ISO 8601 string.
        /// </summary>
        /// <param name="dateTime">The total milliseconds in ISO 8601 time</param>
        /// <returns>The equivalent DateTimeOffset</returns>
        public static DateTimeOffset FromUTCISO8601String(string dateTime) { return DateTimeOffset.Parse(dateTime).ToOffset(DateTimeOffset.Now.Offset); }

        public static DateTimeOffset FromGeneralString(string dateTime)
        {
            try
            {
                if (!string.IsNullOrEmpty(dateTime))
                {
                    if (dateTime.Contains("Z", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (DateTimeOffset.TryParse(dateTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset valueUTC))
                        {
                            return valueUTC.ToCorrectLocalTime();
                        }
                    }
                    else if (DateTime.TryParse(dateTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime value))
                    {
                        return new DateTimeOffset(value).ToCorrectLocalTime();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return DateTimeOffset.MinValue;
        }

        /// <summary>
        /// Creates an ISO 8601 string.
        /// </summary>
        /// <param name="dateTime">The DateTimeOffset to convert</param>
        /// <returns>The equivalent ISO 8601 string</returns>
        public static string ToUTCISO8601String(this DateTimeOffset dateTime) { return dateTime.ToOffset(DateTimeOffset.UtcNow.Offset).ToString("s"); }

        /// <summary>
        /// Creates an RFC 3339 string
        /// </summary>
        /// <param name="dateTime">The DateTimeOffset to convert</param>
        /// <returns>The equivalent RFC 3339 string</returns>
        public static string ToRFC3339String(this DateTimeOffset dateTime) { return dateTime.ToUTCISO8601String() + "Z"; }

        public static string ToFriendlyDateString(this DateTimeOffset dt) { return dt.ToString("d"); }

        public static string ToFriendlyTimeString(this DateTimeOffset dt) { return dt.ToString("t"); }

        public static string ToFriendlyDateTimeString(this DateTimeOffset dt) { return dt.ToString("g"); }

        public static DateTimeOffset ToCorrectLocalTime(this DateTimeOffset dt) { return dt.ToOffset(DateTimeOffset.Now.Offset); }

        public static string GetAge(this DateTimeOffset start, bool includeTime = false)
        {
            return start.GetAge(DateTimeOffset.UtcNow, includeTime);
        }

        public static string GetAge(this DateTimeOffset start, DateTimeOffset end, bool includeTime = false)
        {
            if (start == DateTimeOffset.MinValue || start == DateTimeOffset.MaxValue || end == DateTimeOffset.MinValue || end == DateTimeOffset.MaxValue)
            {
                return MixItUp.Base.Resources.Unknown;
            }

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
                        dateSegments.Add(hours + " " + MixItUp.Base.Resources.TimeHours);
                    }
                    if (minutes > 0)
                    {
                        dateSegments.Add(minutes + " " + MixItUp.Base.Resources.TimeMinutes);
                    }
                }
                else
                {
                    dateSegments.Add("<1 " + MixItUp.Base.Resources.Day);
                }
            }
            else
            {
                if (years > 0)
                {
                    dateSegments.Add(years + " " + MixItUp.Base.Resources.TimeYears);
                }
                if (months > 0)
                {
                    dateSegments.Add(months + " " + MixItUp.Base.Resources.TimeMonths);
                }
                if (days > 0)
                {
                    dateSegments.Add(days + " " + MixItUp.Base.Resources.TimeDays);
                }
            }

            return string.Join(", ", dateSegments);
        }

        public static int TotalMonthsFromNow(this DateTimeOffset dt)
        {
            if (dt == DateTimeOffset.MinValue)
            {
                return 0;
            }

            int subMonths = 0;
            DateTime currentDateTime = DateTimeOffset.Now.Date;
            DateTime startDateTime = dt.Date;
            DateTime tempDateTime = dt.Date;

            do
            {
                subMonths++;
                tempDateTime = tempDateTime.AddMonths(1);
                if (tempDateTime.Day < startDateTime.Day)
                {
                    int correctDay = Math.Min(CultureInfo.InvariantCulture.Calendar.GetDaysInMonth(tempDateTime.Year, tempDateTime.Month), startDateTime.Day);
                    tempDateTime = tempDateTime.AddDays(correctDay - tempDateTime.Day);
                }
            } while (tempDateTime <= currentDateTime);

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
