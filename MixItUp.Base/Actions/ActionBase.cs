using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum ActionTypeEnum
    {
        Chat,
        Currency,
        [Name("External Program")]
        ExternalProgram,
        Input,
        Overlay,
        Sound,
        Wait,
        [Name("OBS Studio")]
        OBSStudio,
        XSplit,
        Counter,
        [Name("Game Queue")]
        GameQueue,
        Interactive,
        [Name("Text To Speech")]
        TextToSpeech,
        [Obsolete]
        Rank,
        [Name("Web Request")]
        WebRequest,

        Custom = 99,
    }

    [DataContract]
    public abstract class ActionBase
    {
        [DataMember]
        public ActionTypeEnum Type { get; set; }

        public ActionBase() { }

        public ActionBase(ActionTypeEnum type)
        {
            this.Type = type;
        }

        public async Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            await this.AsyncSemaphore.WaitAsync();

            try
            {
                await this.PerformInternal(user, arguments);
            }
            catch (Exception ex) { Logger.Log(ex); }
            finally { this.AsyncSemaphore.Release(); }
        }

        protected abstract Task PerformInternal(UserViewModel user, IEnumerable<string> arguments);

        protected async Task<string> ReplaceStringWithSpecialModifiers(string str, UserViewModel user, IEnumerable<string> arguments)
        {
            str = str.Replace("$date", DateTimeOffset.Now.ToString("d"));
            str = str.Replace("$time", DateTimeOffset.Now.ToString("t"));
            str = str.Replace("$datetime", DateTimeOffset.Now.ToString("g"));

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

                    str = str.Replace("$" + currency.SpecialIdentifierName, currency.Name);
                    UserRankViewModel rank = currencyData.GetRank();
                    if (rank != null)
                    {
                        str = str.Replace("$" + currency.SpecialIdentifierRank, rank.Name);
                    }
                    str = str.Replace("$" + currency.SpecialIdentifier, currencyData.Amount.ToString());
                }
                str = str.Replace("$usertime", user.Data.ViewingTimeString);

                str = str.Replace("$useravatar", user.AvatarLink);
                str = str.Replace("$userurl", "https://www.mixer.com/" + user.UserName);

                str = str.Replace("$username", user.UserName);
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

                                str = str.Replace("$arg" + (i + 1) + currency.SpecialIdentifierName, currency.Name);
                                UserRankViewModel rank = currencyData.GetRank();
                                if (rank != null)
                                {
                                    str = str.Replace("$arg" + (i + 1) + currency.SpecialIdentifierRank, rank.Name);
                                }
                                str = str.Replace("$arg" + (i + 1) + currency.SpecialIdentifier, currencyData.Amount.ToString());
                            }
                            str = str.Replace("$arg" + (i + 1) + "usertime", userData.ViewingTimeString);
                        }

                        if (string.IsNullOrEmpty(argUser.avatarUrl))
                        {
                            argUser.avatarUrl = UserViewModel.DefaultAvatarLink;
                        }

                        str = str.Replace("$arg" + (i + 1) + "useravatar", argUser.avatarUrl);
                        str = str.Replace("$arg" + (i + 1) + "userurl", "https://www.mixer.com/" + argUser.username);
                        str = str.Replace("$arg" + (i + 1) + "username", argUser.username);             
                    }

                    str = str.Replace("$arg" + (i + 1) + "string", arguments.ElementAt(i));
                }
            }

            str = str.Replace("$allargs", string.Join(" ", arguments));

            foreach (string counter in ChannelSession.Counters.Keys)
            {
                str = str.Replace("$" + counter, ChannelSession.Counters[counter].ToString());
            }

            return str;
        }

        protected abstract SemaphoreSlim AsyncSemaphore { get; }
    }
}
