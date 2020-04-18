using Mixer.Base.Model.Chat;
using MixItUp.Base.Model.Chat;
using System.Collections.Generic;

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

        public Dictionary<string, string> GetSpecialIdentifiers()
        {
            Dictionary<string, string> results = this.Skill.GetSpecialIdentifiers();
            results["skillmessage"] = this.PlainTextMessage;
            return results;
        }
    }
}
