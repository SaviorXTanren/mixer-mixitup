using MixItUp.Base.ViewModel.User;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Requirements
{
    [DataContract]
    public abstract class RequirementModelBase
    {
        public virtual Task<bool> Validate(UserViewModel user)
        {
            return Task.FromResult(true);
        }

        public virtual Task Perform(UserViewModel user)
        {
            return Task.FromResult(0);
        }

        public virtual Task Refund(UserViewModel user)
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

        protected async Task SendChatWhisper(UserViewModel user, string message)
        {
            if (ChannelSession.Services.Chat != null)
            {
                await ChannelSession.Services.Chat.Whisper(user, message);
            }
        }
    }
}
