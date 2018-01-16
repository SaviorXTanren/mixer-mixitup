using Mixer.Base.Model.User;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public class SpecialIdentifierStringBuilder
    {
        public static string ConvertScorpBotText(string text)
        {
            text = text.Replace("$user", "$username");
            text = text.Replace("$target", "$arg1username");
            for (int i = 1; i < 10; i++)
            {
                text = text.Replace("$target" + i, "$arg" + i + "username");
            }
            text = text.Replace("$msg", "$allargs");
            text = text.Replace("$hours", "$userhours");
            return text;
        }

        private string text;

        public SpecialIdentifierStringBuilder(string text) { this.text = text; }

        public async Task ReplaceCommonSpecialModifiers(UserViewModel user, IEnumerable<string> arguments = null)
        {
            this.ReplaceSpecialIdentifier("date", DateTimeOffset.Now.ToString("d"));
            this.ReplaceSpecialIdentifier("time", DateTimeOffset.Now.ToString("t"));
            this.ReplaceSpecialIdentifier("datetime", DateTimeOffset.Now.ToString("g"));

            if (user != null)
            {
                if (string.IsNullOrEmpty(user.AvatarLink))
                {
                    user.AvatarLink = UserViewModel.DefaultAvatarLink;
                }

                if (user.AvatarLink.Equals(UserViewModel.DefaultAvatarLink))
                {
                    UserModel avatarUser = await ChannelSession.Connection.GetUser(user.UserName);
                    user.AvatarLink = avatarUser.avatarUrl;
                }

                for (int i = 0; i < ChannelSession.Settings.Currencies.Count; i++)
                {
                    UserCurrencyViewModel currency = ChannelSession.Settings.Currencies.Values.ElementAt(i);
                    UserCurrencyDataViewModel currencyData = user.Data.GetCurrency(currency);

                    UserRankViewModel rank = currencyData.GetRank();
                    if (rank != null)
                    {
                        this.ReplaceSpecialIdentifier(currency.UserRankNameSpecialIdentifier, rank.Name);
                    }
                    this.ReplaceSpecialIdentifier(currency.UserAmountSpecialIdentifier, currencyData.Amount.ToString());
                }
                this.ReplaceSpecialIdentifier("usertime", user.Data.ViewingTimeString);
                this.ReplaceSpecialIdentifier("userhours", user.Data.ViewingHoursString);
                this.ReplaceSpecialIdentifier("usermins", user.Data.ViewingMinutesString);

                this.ReplaceSpecialIdentifier("userfollowage", user.FollowAgeString);
                this.ReplaceSpecialIdentifier("usersubage", user.SubscribeAgeString);
                this.ReplaceSpecialIdentifier("usersubmonths", user.SubscribeMonths.ToString());

                this.ReplaceSpecialIdentifier("useravatar", user.AvatarLink);
                this.ReplaceSpecialIdentifier("userurl", "https://www.mixer.com/" + user.UserName);

                this.ReplaceSpecialIdentifier("username", user.UserName);
            }

            if (arguments != null)
            {
                for (int i = 0; i < arguments.Count(); i++)
                {
                    string username = arguments.ElementAt(i);
                    username = username.Replace("@", "");

                    UserModel argUserModel = await ChannelSession.Connection.GetUser(username);
                    if (argUserModel != null)
                    {
                        UserViewModel argUser = new UserViewModel(argUserModel);
                        await argUser.SetDetails();

                        if (ChannelSession.Settings.UserData.ContainsKey(argUser.ID))
                        {
                            UserDataViewModel userData = ChannelSession.Settings.UserData[argUser.ID];

                            for (int c = 0; c < ChannelSession.Settings.Currencies.Count; c++)
                            {
                                UserCurrencyViewModel currency = ChannelSession.Settings.Currencies.Values.ElementAt(i);
                                UserCurrencyDataViewModel currencyData = userData.GetCurrency(currency);

                                UserRankViewModel rank = currencyData.GetRank();
                                if (rank != null)
                                {
                                    this.ReplaceSpecialIdentifier("arg" + (i + 1) + currency.UserRankNameSpecialIdentifier, rank.Name);
                                }
                                this.ReplaceSpecialIdentifier("arg" + (i + 1) + currency.UserAmountSpecialIdentifier, currencyData.Amount.ToString());
                            }
                            this.ReplaceSpecialIdentifier("arg" + (i + 1) + "usertime", userData.ViewingTimeString);
                        }

                        this.ReplaceSpecialIdentifier("arg" + (i + 1) + "userfollowage", argUser.FollowAgeString);
                        this.ReplaceSpecialIdentifier("arg" + (i + 1) + "usersubage", argUser.SubscribeAgeString);
                        this.ReplaceSpecialIdentifier("arg" + (i + 1) + "usersubmonths", argUser.SubscribeMonths.ToString());

                        this.ReplaceSpecialIdentifier("arg" + (i + 1) + "useravatar", argUser.AvatarLink);
                        this.ReplaceSpecialIdentifier("arg" + (i + 1) + "userurl", "https://www.mixer.com/" + argUser.UserName);
                        this.ReplaceSpecialIdentifier("arg" + (i + 1) + "username", argUser.UserName);
                    }

                    this.ReplaceSpecialIdentifier("arg" + (i + 1) + "text", arguments.ElementAt(i));
                }

                this.ReplaceSpecialIdentifier("allargs", string.Join(" ", arguments));
            }

            foreach (string counter in ChannelSession.Counters.Keys)
            {
                this.ReplaceSpecialIdentifier(counter, ChannelSession.Counters[counter].ToString());
            }
        }

        public void ReplaceSpecialIdentifier(string identifier, string replacement)
        {
            this.text = this.text.Replace("$" + identifier, replacement);
        }

        public override string ToString() { return this.text; }
    }
}
