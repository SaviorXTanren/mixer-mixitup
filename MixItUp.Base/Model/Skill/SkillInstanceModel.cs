using Mixer.Base.Model.Skills;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Skill
{
    public enum SkillTypeEnum
    {
        Sticker,
        Gif,
        Effect,
        Rally
    }

    [DataContract]
    public class SkillInstanceModel
    {
        private const string GifUrlKey = "gifUrl";

        [DataMember]
        public SkillModel Skill { get; set; }
        [DataMember]
        public JObject Manifest { get; set; }
        [DataMember]
        public JObject Parameters { get; set; }

        public SkillInstanceModel() { }

        public SkillInstanceModel(SkillModel skill, JObject manifest, JObject parameters)
        {
            this.Skill = skill;
            this.Manifest = manifest;
            this.Parameters = parameters;
        }

        [JsonIgnore]
        public SkillTypeEnum Type
        {
            get
            {
                if (this.IsGif)
                {
                    return SkillTypeEnum.Gif;
                }
                else
                {
                    switch (this.Skill.category)
                    {
                        case "Effects":
                            return SkillTypeEnum.Effect;
                        case "Stickers":
                            return SkillTypeEnum.Sticker;
                        case "Rallies":
                            return SkillTypeEnum.Rally;
                    }
                }
                return SkillTypeEnum.Sticker;
            }
        }

        [JsonIgnore]
        public string ImageUrl
        {
            get
            {
                if (this.IsGif)
                {
                    return this.GifUrl;
                }
                else
                {
                    return this.Skill.iconUrl;
                }
            }
        }

        [JsonIgnore]
        public bool IsGif { get { return this.Parameters.ContainsKey(SkillInstanceModel.GifUrlKey); } }

        [JsonIgnore]
        public string GifUrl
        {
            get
            {
                if (this.IsGif)
                {
                    return this.Parameters[SkillInstanceModel.GifUrlKey].ToString();
                }
                return null;
            }
        }
    }
}
