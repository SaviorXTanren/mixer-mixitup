using MixItUp.Base.ViewModel.User;

namespace MixItUp.Base.Model.Chat.Mixer
{
    public enum MixerChatUserModerationType
    {
        Purge,
        Timeout,
        GlobalTimeout,
        Ban
    }

    public class MixerChatUserModerationModel
    {
        public UserViewModel User { get; set; }

        public UserViewModel Moderator { get; set; }

        public MixerChatUserModerationType Type { get; set; }

        public string Length { get; set; }

        public MixerChatUserModerationModel(UserViewModel user, UserViewModel moderator, MixerChatUserModerationType type, string length = null)
        {
            this.User = user;
            this.Moderator = moderator;
            this.Type = type;
            this.Length = length;
        }
    }
}
