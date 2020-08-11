using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Requirements
{
    [DataContract]
    public abstract class RequirementModelBase
    {
        public virtual Task<bool> Validate(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            return Task.FromResult(true);
        }

        public virtual Task Perform(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            return Task.FromResult(0);
        }

        public virtual Task Refund(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
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

        protected async Task<int> GetAmount(string amount, UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (!string.IsNullOrEmpty(amount))
            {
                if (amount.StartsWith(SpecialIdentifierStringBuilder.SpecialIdentifierHeader))
                {
                    amount = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(amount, user, platform, arguments, specialIdentifiers);
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
