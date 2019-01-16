using Mixer.Base.Model.Chat;

namespace MixItUp.Base.MixerAPI.Models
{
    public class ChatSkillModelWrapper
    {
        public const string SparksCurrencyName = "Sparks";
        public const string EmbersCurrencyName = "Embers";

        public static bool IsSparksChatSkill(ChatSkillModel skill) { return skill.currency.Equals(ChatSkillModelWrapper.SparksCurrencyName); }

        public static bool IsEmbersChatSkill(ChatSkillModel skill) { return skill.currency.Equals(ChatSkillModelWrapper.EmbersCurrencyName); }

        public ChatSkillModel Skill { get; private set; }

        public ChatSkillModelWrapper(ChatSkillModel skill)
        {
            this.Skill = skill;
        }

        public bool IsSparksSkill { get { return ChatSkillModelWrapper.IsSparksChatSkill(this.Skill); } }

        public bool IsEmbersSkill { get { return ChatSkillModelWrapper.IsEmbersChatSkill(this.Skill); } }
    }
}
