using MixItUp.Base.ViewModel.Chat.Mixer;
using MixItUp.Base.ViewModel.User;

namespace MixItUp.Base.Model.User
{
    public class UserEmberUsageModel
    {
        public UserViewModel User { get; set; }

        public uint Amount { get; set; }

        public string Message { get; set; }

        public UserEmberUsageModel(MixerSkillChatMessageViewModel skillMessage)
            : this(skillMessage.User, skillMessage.Skill.Cost, skillMessage.PlainTextMessage)
        { }

        public UserEmberUsageModel(UserViewModel user, uint amount, string message)
        {
            this.User = user;
            this.Amount = amount;
            this.Message = message;
        }
    }
}
