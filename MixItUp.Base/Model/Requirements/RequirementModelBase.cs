using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
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

        protected async Task<int> GetAmount(string amount, CommandParametersModel parameters)
        {
            if (!string.IsNullOrEmpty(amount))
            {
                if (amount.StartsWith(SpecialIdentifierStringBuilder.SpecialIdentifierHeader))
                {
                    amount = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(amount, parameters);
                }

                if (int.TryParse(amount, out int iAmount) && iAmount >= 0)
                {
                    return iAmount;
                }
            }
            return 0;
        }
    }
}
