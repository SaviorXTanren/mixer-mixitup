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
            str = str.Replace("$date", DateTimeOffset.Now.ToString("g"));

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
                    str = str.Replace("$usercurrency" + (i + 1), user.Data.GetCurrencyAmount(ChannelSession.Settings.Currencies.Values.ElementAt(i)).ToString());
                }
                str = str.Replace("$userrankname", user.Data.RankName);
                str = str.Replace("$userrankpoints", user.Data.RankPoints.ToString());
                str = str.Replace("$userrank", user.Data.RankNameAndPoints);
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
                                str = str.Replace("$arg" + (i + 1) + "usercurrency" + (c + 1), userData.GetCurrencyAmount(ChannelSession.Settings.Currencies.Values.ElementAt(c)).ToString());
                            }
                            str = str.Replace("$arg" + (i + 1) + "userrankname", userData.RankName);
                            str = str.Replace("$arg" + (i + 1) + "userrankpoints", userData.RankPoints.ToString());
                            str = str.Replace("$arg" + (i + 1) + "userrank", userData.RankNameAndPoints);
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

            str = str.Replace("$allArgs", string.Join(" ", arguments));

            foreach (string counter in ChannelSession.Counters.Keys)
            {
                str = str.Replace("$" + counter, ChannelSession.Counters[counter].ToString());
            }

            return str;
        }

        protected abstract SemaphoreSlim AsyncSemaphore { get; }
    }
}
