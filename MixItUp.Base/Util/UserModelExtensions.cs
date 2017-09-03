using Mixer.Base.Model.User;
using System;
using System.Text;

namespace MixItUp.Base.Util
{
    public static class UserModelExtensions
    {
        public static string GetMixerAge(this UserModel user)
        {
            return user.GetFollowAge(user.createdAt.GetValueOrDefault());
        }

        public static string GetFollowAge(this UserModel user, DateTimeOffset followDate)
        {
            TimeSpan upTime = DateTimeOffset.Now - followDate.ToOffset(DateTimeOffset.Now.Offset);
            int days = (int)upTime.TotalDays;
            int years = days / 365;
            days = days - (years * 365);
            int months = days / 30;
            days = days - (months * 30);

            StringBuilder mixerAgeString = new StringBuilder();
            mixerAgeString.Append(user.username + "'s Mixer Age: ");
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
