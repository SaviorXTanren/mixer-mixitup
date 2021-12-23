using MixItUp.Base.Model;
using MixItUp.Base.ViewModel.User;

namespace MixItUp.Base.ViewModel.Chat
{
    public class UserChatMessageViewModel : ChatMessageViewModel
    {
        public UserChatMessageViewModel(string id, StreamingPlatformTypeEnum platform, UserV2ViewModel user)
            : base(id, platform, user)
        { }
    }
}
