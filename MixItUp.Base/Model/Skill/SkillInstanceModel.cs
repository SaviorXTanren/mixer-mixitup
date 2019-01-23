using Mixer.Base.Model.Skills;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
        private const string ManifestNameKey = "name";

        private const string GiphyManifestName = "giphy";
        private const string GiphyImageURL = "giphyUrl";

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
                    try
                    {
                        return this.Parameters[SkillInstanceModel.GiphyImageURL].ToString();
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                }
                return this.Skill.iconUrl;
            }
        }

        [JsonIgnore]
        public bool IsGif
        {
            get
            {
                if (this.Manifest.ContainsKey(SkillInstanceModel.ManifestNameKey) && !string.IsNullOrEmpty(this.Manifest[SkillInstanceModel.ManifestNameKey].ToString()))
                {
                    return this.Manifest[SkillInstanceModel.ManifestNameKey].ToString().Equals(SkillInstanceModel.GiphyManifestName);
                }
                return false;
            }
        }
    }
}
