using Mixer.Base.Util;
using Mixer.Base.ViewModel;
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
        Giveaway,
        Input,
        Overlay,
        Sound,
        Wait,
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
            str = str.Replace("$user", "@" + user.UserName);
            for (int i = 0; i < arguments.Count(); i++)
            {
                str = str.Replace("$arg" + (i + 1), arguments.ElementAt(i));
            }
            return str;
        }
    }
}
