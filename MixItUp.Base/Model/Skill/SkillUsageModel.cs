using Mixer.Base.Model.Chat;
using Mixer.Base.Model.Skills;
using Mixer.Base.Util;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Skill
{
    public enum SkillCostTypeEnum
    {
        Sparks = 0,
        Embers = 1,
    }

    [DataContract]
    public class SkillUsageModel
    {
        public const string SparksCurrencyName = "Sparks";
        public const string EmbersCurrencyName = "Embers";

        public static SkillCostTypeEnum GetSkillCostType(SkillModel skill)
        {
            switch (skill.currency)
            {
                case 2:
                    return SkillCostTypeEnum.Embers;
                case 1:
                default:
                    return SkillCostTypeEnum.Sparks;
            }
        }

        public static SkillCostTypeEnum GetChatSkillCostType(ChatSkillModel skill)
        {
            switch (skill.currency)
            {
                case SkillUsageModel.EmbersCurrencyName:
                    return SkillCostTypeEnum.Embers;
                case SkillUsageModel.SparksCurrencyName:
                default:
                    return SkillCostTypeEnum.Sparks;
            }
        }

        public static bool IsSparksChatSkill(ChatSkillModel skill) { return SkillUsageModel.GetChatSkillCostType(skill) == SkillCostTypeEnum.Sparks; }

        public static bool IsEmbersChatSkill(ChatSkillModel skill) { return SkillUsageModel.GetChatSkillCostType(skill) == SkillCostTypeEnum.Embers; }

        [DataMember]
        public UserViewModel User { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public SkillTypeEnum Type { get; set; }
        [DataMember]
        public SkillCostTypeEnum CostType { get; set; }
        [DataMember]
        public uint Cost { get; set; }
        [DataMember]
        public string Image { get; set; }
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public ChatSkillModel ChatSkill { get; set; }
        [DataMember]
        public SkillInstanceModel SkillInstance { get; set; }

        public SkillUsageModel(UserViewModel user, SkillInstanceModel skill)
            : this(user, skill.Skill.name, skill.Type, SkillUsageModel.GetSkillCostType(skill.Skill), skill.Skill.price, skill.ImageUrl, string.Empty)
        {
            this.SkillInstance = skill;
        }

        public SkillUsageModel(UserViewModel user, ChatSkillModel skill, string message)
            : this(user, skill.skill_name, SkillTypeEnum.Sticker, SkillUsageModel.GetChatSkillCostType(skill), skill.cost, skill.icon_url, message)
        {
            this.ChatSkill = skill;
        }

        private SkillUsageModel(UserViewModel user, string name, SkillTypeEnum type, SkillCostTypeEnum costType, uint cost, string image, string message)
        {
            this.User = user;
            this.Name = name;
            this.Type = type;
            this.CostType = costType;
            this.Cost = cost;
            this.Image = image;
            this.Message = message;
        }

        public bool IsSparksSkill { get { return this.CostType.Equals(SkillCostTypeEnum.Sparks); } }

        public bool IsEmbersSkill { get { return this.CostType.Equals(SkillCostTypeEnum.Embers); } }

        public Dictionary<string, string> GetSpecialIdentifiers()
        {
            return new Dictionary<string, string>()
            {
                { "skillname", this.Name },
                { "skilltype", EnumHelper.GetEnumName(this.Type) },
                { "skillcosttype", EnumHelper.GetEnumName(this.CostType) },
                { "skillcost", this.Cost.ToString() },
                { "skillimage", this.Image },
                { "skillissparks", this.IsSparksSkill.ToString() },
                { "skillisembers", this.IsEmbersSkill.ToString() },
                { "skillmessage", (!string.IsNullOrEmpty(this.Message)) ? this.Message : string.Empty },
            };
        }
    }
}
