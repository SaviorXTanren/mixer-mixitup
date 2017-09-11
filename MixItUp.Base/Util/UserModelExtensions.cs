using Mixer.Base.Model.User;
using System;
using System.Text;

namespace MixItUp.Base.Util
{
    public static class UserModelExtensions
    {
        public static string GetMixerAge(this UserModel user)
        {
            return user.username + "'s Mixer Age: " + user.GetAge(user.createdAt.GetValueOrDefault());
        }

        public static string GetFollowAge(this UserModel user, DateTimeOffset followDate)
        {
            return user.username + "'s Follow Age: " + user.GetAge(user.createdAt.GetValueOrDefault());
        }

        public static string GetAge(this UserModel user, DateTimeOffset startDate)
        {
            TimeSpan upTime = DateTimeOffset.Now - startDate.ToOffset(DateTimeOffset.Now.Offset);
            int days = (int)upTime.TotalDays;
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
