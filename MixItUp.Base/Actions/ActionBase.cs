using Mixer.Base.Util;
using MixItUp.Base.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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

        public abstract Task Perform(UserViewModel user, IEnumerable<string> arguments);

        protected async Task Wait500()
        {
            await Task.Delay(500);
        }

        protected string ReplaceStringWithSpecialModifiers(string str, UserViewModel user, IEnumerable<string> arguments)
        {
            if (user != null)
            {
                if (ChannelSession.ChatUsers.ContainsKey(user.ID))
                {
                    str = str.Replace("$userAvatar", ChannelSession.ChatUsers[user.ID].AvatarLink);
                }

                str = str.Replace("$user", "@" + user.UserName);

                if (ChannelSession.Settings.UserData.ContainsKey(user.ID))
                {
                    str = str.Replace("$currency", ChannelSession.Settings.UserData[user.ID].CurrencyAmount.ToString());
                }
            }

            if (!string.IsNullOrEmpty(ChannelSession.Settings.CurrencyName))
            {
                str = str.Replace("$currencyName", ChannelSession.Settings.CurrencyName);
            }

            str = str.Replace("$date", DateTimeOffset.Now.ToString("g"));

            if (arguments != null)
            {
                for (int i = 0; i < arguments.Count(); i++)
                {
                    str = str.Replace("$arg" + (i + 1), arguments.ElementAt(i));
                }
            }

            foreach (string counter in ChannelSession.Counters.Keys)
            {
                str = str.Replace("$" + counter, ChannelSession.Counters[counter].ToString());
            }

            return str;
        }
    }
}
