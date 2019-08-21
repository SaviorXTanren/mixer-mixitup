using Mixer.Base.Model.Chat;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Chat
{
    public enum MixerSkillCostTypeEnum
    {
        Sparks = 0,
        Embers = 1,
    }

    public enum MixerSkillTypeEnum
    {
        Sticker,
        Gif,
        Other
    }

    [DataContract]
    public class MixerSkillPayloadModel
    {
        private const string GiphyImageURLKey = "giphyUrl";

        [DataMember]
        public uint channelId { get; set; }
        [DataMember]
        public Guid skillId { get; set; }
        [DataMember]
        public Guid executionId { get; set; }
        [DataMember]
        public JObject manifest { get; set; }
        [DataMember]
        public JObject parameters { get; set; }
        public uint triggeringUserId { get; set; }
        public string currencyType { get; set; }
        public uint price { get; set; }

        public string GiphyImageURL { get { return (this.parameters != null && this.parameters.ContainsKey(GiphyImageURLKey)) ? this.parameters[GiphyImageURLKey].ToString() : null; } }
    }

    [DataContract]
    public class MixerSkillModel
    {
        public const string SparksCurrencyName = "Sparks";
        public const string EmbersCurrencyName = "Embers";

        public static readonly Guid SendAGifSkillID = Guid.Parse("ba35d561-411a-4b96-ab3c-6e9532a33027");

        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public MixerSkillTypeEnum Type { get; set; }
        [DataMember]
        public MixerSkillCostTypeEnum CostType { get; set; }
        [DataMember]
        public uint Cost { get; set; }
        [DataMember]
        public string Image { get; set; }

        public MixerSkillModel(ChatSkillModel chatSkill)
        {
            this.ID = chatSkill.skill_id;
            this.Name = chatSkill.skill_name;
            this.Type = MixerSkillTypeEnum.Sticker;
            this.CostType = (MixerSkillModel.EmbersCurrencyName.Equals(chatSkill.currency)) ? MixerSkillCostTypeEnum.Embers : MixerSkillCostTypeEnum.Sparks;
            this.Cost = chatSkill.cost;
            this.Image = chatSkill.icon_url;
        }

        public void SetPayload(MixerSkillPayloadModel payload)
        {
            if (this.ID.Equals(SendAGifSkillID))
            {
                this.Type = MixerSkillTypeEnum.Gif;
                this.Image = payload.GiphyImageURL;
            }
            else
            {
                this.Type = MixerSkillTypeEnum.Other;
            }
        }

        public bool IsSparksSkill { get { return this.CostType.Equals(MixerSkillCostTypeEnum.Sparks); } }

        public bool IsEmbersSkill { get { return this.CostType.Equals(MixerSkillCostTypeEnum.Embers); } }

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
                //{ "skillmessage", (!string.IsNullOrEmpty(this.Message)) ? this.Message : string.Empty },
            };
        }
    }
}
