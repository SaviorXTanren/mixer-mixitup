using Mixer.Base.Util;
using Mixer.Base.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum ActionTypeEnum
    {
        Chat,
        Cooldown,
        Currency,
        [Name("External Program")]
        ExternalProgram,
        Giveaway,
        Input,
        Overlay,
        Sound,
        Whisper,
        Wait,
    }

    public abstract class ActionBase
    {
        public ActionTypeEnum Type { get; private set; }

        public ActionBase(ActionTypeEnum type)
        {
            this.Type = type;
        }

        public abstract Task Perform(UserViewModel user, IEnumerable<string> arguments);

        public virtual SerializableAction Serialize()
        {
            return new SerializableAction()
            {
                Type = this.Type
            };
        }

        protected async Task Wait500()
        {
            await Task.Delay(500);
        }
    }
}
