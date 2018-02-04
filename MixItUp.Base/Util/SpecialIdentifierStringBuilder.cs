using Mixer.Base.Model.Game;
using Mixer.Base.Model.User;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public class SpecialIdentifierStringBuilder
    {
        private static Dictionary<string, string> CustomSpecialIdentifiers = new Dictionary<string, string>();

        public static void AddCustomSpecialIdentifier(string specialIdentifier, string replacement)
        {
            SpecialIdentifierStringBuilder.CustomSpecialIdentifiers[specialIdentifier] = replacement;
        }

        public static string ConvertScorpBotText(string text)
        {
            text = text.Replace("$user ", "$username ");
            text = text.Replace("$target", "$arg1username");
            for (int i = 1; i < 10; i++)
            {
                text = text.Replace("$target" + i, "$arg" + i + "username");
            }
            text = text.Replace("$msg", "$allargs");
            text = text.Replace("$hours", "$userhours");
            text = text.Replace("$raids", "");
            return text;
        }

        public static string ConvertToSpecialIdentifier(string text)
        {
            StringBuilder specialIdentifier = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsLetterOrDigit(text[i]))
                {
                    specialIdentifier.Append(text[i]);
                }
            }
            return specialIdentifier.ToString().ToLower();
        }

        private string text;

        public SpecialIdentifierStringBuilder(string text) { this.text = text; }

        public async Task ReplaceCommonSpecialModifiers(UserViewModel user, IEnumerable<string> arguments = null)
        {
            foreach (string counter in ChannelSession.Counters.Keys)
            {
                this.ReplaceSpecialIdentifier(counter, ChannelSession.Counters[counter].ToString());
            }

            foreach (var kvp in SpecialIdentifierStringBuilder.CustomSpecialIdentifiers)
            {
                this.ReplaceSpecialIdentifier(kvp.Key, kvp.Value);
            }

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
                    if (!string.IsNullOrEmpty(user.AvatarLink))
                    {
                        user.AvatarLink = avatarUser.avatarUrl;
                    }
                }

                for (int i = 0; i < ChannelSession.Settings.Currencies.Count; i++)
                {
                    UserCurrencyViewModel currency = ChannelSession.Settings.Currencies.Values.ElementAt(i);
                    UserCurrencyDataViewModel currencyData = user.Data.GetCurrency(currency);

                    UserRankViewModel rank = currencyData.GetRank();
                    this.ReplaceSpecialIdentifier(currency.UserRankNameSpecialIdentifier, rank.Name);
                    this.ReplaceSpecialIdentifier(currency.UserAmountSpecialIdentifier, currencyData.Amount.ToString());
                }
                this.ReplaceSpecialIdentifier("usertime", user.Data.ViewingTimeString);
                this.ReplaceSpecialIdentifier("userhours", user.Data.ViewingHoursString);
                this.ReplaceSpecialIdentifier("usermins", user.Data.ViewingMinutesString);

                this.ReplaceSpecialIdentifier("userfollowage", user.FollowAgeString);
                this.ReplaceSpecialIdentifier("usersubage", user.SubscribeAgeString);
                this.ReplaceSpecialIdentifier("usersubmonths", user.SubscribeMonths.ToString());

                if (this.ContainsSpecialIdentifier("usergame"))
                {
                    GameTypeModel game = await ChannelSession.Connection.GetGameType(user.GameTypeID);
                    this.ReplaceSpecialIdentifier("usergame", game.name.ToString());
                }

                this.ReplaceSpecialIdentifier("useravatar", user.AvatarLink);
                this.ReplaceSpecialIdentifier("userurl", "https://www.mixer.com/" + user.UserName);
                this.ReplaceSpecialIdentifier("username", user.UserName);
                this.ReplaceSpecialIdentifier("userid", user.ID.ToString());
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
                                this.ReplaceSpecialIdentifier("arg" + (i + 1) + currency.UserRankNameSpecialIdentifier, rank.Name);
                                this.ReplaceSpecialIdentifier("arg" + (i + 1) + currency.UserAmountSpecialIdentifier, currencyData.Amount.ToString());
                            }
                            this.ReplaceSpecialIdentifier("arg" + (i + 1) + "usertime", userData.ViewingTimeString);
                            this.ReplaceSpecialIdentifier("arg" + (i + 1) + "userhours", userData.ViewingHoursString);
                            this.ReplaceSpecialIdentifier("arg" + (i + 1) + "usermins", userData.ViewingMinutesString);
                        }

                        if (this.ContainsSpecialIdentifier("arg" + (i + 1) + "usergame"))
                        {
                            GameTypeModel game = await ChannelSession.Connection.GetGameType(argUser.GameTypeID);
                            this.ReplaceSpecialIdentifier("arg" + (i + 1) + "usergame", game.name.ToString());
                        }

                        this.ReplaceSpecialIdentifier("arg" + (i + 1) + "userfollowage", argUser.FollowAgeString);
                        this.ReplaceSpecialIdentifier("arg" + (i + 1) + "usersubage", argUser.SubscribeAgeString);
                        this.ReplaceSpecialIdentifier("arg" + (i + 1) + "usersubmonths", argUser.SubscribeMonths.ToString());

                        this.ReplaceSpecialIdentifier("arg" + (i + 1) + "useravatar", argUser.AvatarLink);
                        this.ReplaceSpecialIdentifier("arg" + (i + 1) + "userurl", "https://www.mixer.com/" + argUser.UserName);
                        this.ReplaceSpecialIdentifier("arg" + (i + 1) + "username", argUser.UserName);
                        this.ReplaceSpecialIdentifier("arg" + (i + 1) + "userid", argUser.ID.ToString());
                    }

                    this.ReplaceSpecialIdentifier("arg" + (i + 1) + "text", arguments.ElementAt(i));
                }

                this.ReplaceSpecialIdentifier("allargs", string.Join(" ", arguments));
            }
        }

        public void ReplaceSpecialIdentifier(string identifier, string replacement)
        {
            this.text = this.text.Replace("$" + identifier, replacement);
        }

        public bool ContainsSpecialIdentifier(string identifier)
        {
            return this.text.Contains("$" + identifier);
        }

        public override string ToString() { return this.text; }
    }
}
