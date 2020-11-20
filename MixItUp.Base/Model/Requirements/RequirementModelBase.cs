using MixItUp.Base.Model.Commands;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Requirements
{
    [DataContract]
    public abstract class RequirementModelBase
    {
        public virtual Task<bool> Validate(CommandParametersModel parameters)
        {
            return Task.FromResult(true);
        }

        public virtual Task Perform(CommandParametersModel parameters)
        {
            return Task.FromResult(0);
        }

        public virtual Task Refund(CommandParametersModel parameters)
        {
            return Task.FromResult(0);
        }

        public virtual void Reset() { }

        protected async Task SendChatMessage(string message)
        {
            if (ChannelSession.Services.Chat != null)
            {
                await ChannelSession.Services.Chat.SendMessage(message);
            }
        }
    }
}
