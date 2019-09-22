using Mixer.Base.Model.Chat;
using MixItUp.Base.Model.Chat;

namespace MixItUp.Base.ViewModel.Chat.Mixer
{
    public class MixerSkillChatMessageViewModel : MixerChatMessageViewModel
    {
        public MixerSkillModel Skill { get; private set; }

        public MixerSkillChatMessageViewModel(ChatMessageEventModel chatMessageEvent)
            : base(chatMessageEvent)
        {
            this.Skill = new MixerSkillModel(chatMessageEvent.message.Skill);
        }

        public MixerSkillChatMessageViewModel(ChatSkillAttributionEventModel chatMessageEvent)
            : base(chatMessageEvent)
        {
            this.Skill = new MixerSkillModel(chatMessageEvent.skill);
        }
    }
}
