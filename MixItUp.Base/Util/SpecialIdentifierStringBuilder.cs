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

                    this.ReplaceSpecialIdentifier(currency.SpecialIdentifierName, currency.Name);
                    UserRankViewModel rank = currencyData.GetRank();
                    if (rank != null)
                    {
                        this.ReplaceSpecialIdentifier(currency.SpecialIdentifierRank, rank.Name);
                    }
                    this.ReplaceSpecialIdentifier(currency.SpecialIdentifier, currencyData.Amount.ToString());
                }
                this.ReplaceSpecialIdentifier("usertime", user.Data.ViewingTimeString);
                this.ReplaceSpecialIdentifier("userhours", user.Data.ViewingHoursString);
                this.ReplaceSpecialIdentifier("usermins", user.Data.ViewingMinutesString);

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

                    UserModel argUser = await ChannelSession.Connection.GetUser(username);
                    if (argUser != null)
                    {
                        if (ChannelSession.Settings.UserData.ContainsKey(argUser.id))
                        {
                            UserDataViewModel userData = ChannelSession.Settings.UserData[argUser.id];

                            for (int c = 0; c < ChannelSession.Settings.Currencies.Count; c++)
                            {
                                UserCurrencyViewModel currency = ChannelSession.Settings.Currencies.Values.ElementAt(i);
                                UserCurrencyDataViewModel currencyData = userData.GetCurrency(currency);

                                this.ReplaceSpecialIdentifier("arg" + (i + 1) + currency.SpecialIdentifierName, currency.Name);
                                UserRankViewModel rank = currencyData.GetRank();
                                if (rank != null)
                                {
                                    this.ReplaceSpecialIdentifier("arg" + (i + 1) + currency.SpecialIdentifierRank, rank.Name);
                                }
                                this.ReplaceSpecialIdentifier("arg" + (i + 1) + currency.SpecialIdentifier, currencyData.Amount.ToString());
                            }
                            this.ReplaceSpecialIdentifier("arg" + (i + 1) + "usertime", userData.ViewingTimeString);
                        }

                        if (string.IsNullOrEmpty(argUser.avatarUrl))
                        {
                            argUser.avatarUrl = UserViewModel.DefaultAvatarLink;
                        }

                        this.ReplaceSpecialIdentifier("arg" + (i + 1) + "useravatar", argUser.avatarUrl);
                        this.ReplaceSpecialIdentifier("arg" + (i + 1) + "userurl", "https://www.mixer.com/" + argUser.username);
                        this.ReplaceSpecialIdentifier("arg" + (i + 1) + "username", argUser.username);
                    }

                    this.ReplaceSpecialIdentifier("arg" + (i + 1) + "string", arguments.ElementAt(i));
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
